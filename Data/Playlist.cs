using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PlaylistCore.Utilities;
namespace PlaylistCore.Data
{
    public static class PlaylistsCollection
    {
        public static List<Playlist> loadedPlaylists = new List<Playlist>();

        public static void ReloadPlaylists(bool fullRefresh = true)
        {
            try
            {
                List<string> playlistFiles = new List<string>();
                string[] localJSONPlaylists = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "Playlists"), "*.json");
                string[] localBPLISTPlaylists = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "Playlists"), "*.bplist");
                playlistFiles.AddRange(localJSONPlaylists);
                playlistFiles.AddRange(localBPLISTPlaylists);

                Logging.Log.Info($"Found {localJSONPlaylists.Length + localBPLISTPlaylists.Length} playlists in Playlists folder");

                if (fullRefresh)
                {
                    loadedPlaylists.Clear();

                    foreach (string path in playlistFiles)
                    {
                        try
                        {
                            Playlist playlist = Playlist.LoadPlaylist(path);
                            loadedPlaylists.Add(playlist);
                        }
                        catch (Exception e)
                        {
                            Logging.Log.Warn($"Unable to parse playlist @ {path}! Exception: {e}");
                        }
                    }
                }
                else
                {
                    foreach (string path in playlistFiles)
                    {
                        if (!loadedPlaylists.Any(x => x.fileLoc == path))
                        {
                            try
                            {
                                Playlist playlist = Playlist.LoadPlaylist(path);
                                if (Path.GetFileName(path) == "favorites.json" && playlist.playlistTitle == "Your favorite songs")
                                    continue;
                                loadedPlaylists.Add(playlist);
                                //bananbread songloader loaded playlist id
                                if (SongCore.Loader.AreSongsLoaded)
                                {
                                    MatchSongsForPlaylist(playlist);
                                }
                            }
                            catch (Exception e)
                            {
                                Logging.Log.Info($"Unable to parse playlist @ {path}! Exception: {e}");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Log.Critical("Unable to load playlists! Exception: " + e);
            }

        }

        public static void MatchSongsForPlaylist(Playlist playlist, bool matchAll = false)
        {
            if (!SongCore.Loader.AreSongsLoaded || SongCore.Loader.AreSongsLoading)
            {
                Logging.Log.Info("Songs not loaded. Not Matching songs for playlist.");
                return;
            }
            if (!playlist.songs.All(x => x.level != null) || matchAll)
            {
                playlist.songs.AsParallel().ForAll(x =>
                {
                    if (x.level == null || matchAll)
                    {
                        try
                        {
                      //      Logging.Log.Info("Trying to match " + x.hash + " with " + SongCore.Loader.CustomLevels.Values.Count + " songs");
                            if (x.level == null && !string.IsNullOrEmpty(x.hash)) 
                            {
                                x.level = SongCore.Loader.CustomLevels.Values.FirstOrDefault(y => string.Equals(y.levelID.Split('_')[2], x.hash, StringComparison.OrdinalIgnoreCase));
                            }

                        }
                        catch (Exception e)
                        {
                            Logging.Log.Warn($"Unable to match song with {(string.IsNullOrEmpty(x.hash) ? " unknown hash!" : ("hash " + x.hash + " !"))}");
                        }
                    }
                });
            }

        }

        public static void MatchSongsForAllPlaylists(bool matchAll = false)
        {
            Logging.Log.Info("Matching songs for all playlists!");
            Task.Run(() =>
            {
                for (int i = 0; i < loadedPlaylists.Count; i++)
                {
                    MatchSongsForPlaylist(loadedPlaylists[i], matchAll);
                }
                //Update LevelPacks
//                Plugin.DebugLogPlaylists();
            });
        }

        public static void RemoveLevelFromPlaylist(Playlist playlist, string hash)
        {
            if (playlist.songs.Any(x => x.hash == hash))
            {
                PlaylistSong song = playlist.songs.First(x => x.hash == hash);
                song.level = null;
                playlist.songs.Remove(song);
            }
                playlist.SavePlaylist();
            //Update Playlist Pack
  
        }
        public static void AddLevelToPlaylist(Playlist playlist, CustomPreviewBeatmapLevel level)
        {
            string hash = SongCore.Utilities.Hashing.GetCustomLevelHash(level);
            if (playlist.songs.Any(x => string.Equals(x.hash, hash, StringComparison.OrdinalIgnoreCase))) return;
            PlaylistSong song = new PlaylistSong();
            song.hash = hash;
            song.level = level;
            song.levelId = level.levelID;
            song.songName = level.songName + " - " + level.levelAuthorName;
            playlist.songs.Add(song);
            playlist.SavePlaylist();
        }

    }
    public class Playlist
    {
        public string playlistTitle { get; set; } = "";
        public string playlistAuthor { get; set; } = "";
        public string image { get; set; }
        public int playlistSongCount { get; set; }
        public List<PlaylistSong> songs { get; set; }
        public string fileLoc { get; set; }
        public string customDetailUrl { get; set; } = "";
        public string customArchiveUrl { get; set; } = "";

        [NonSerialized]
        public Sprite icon;
        public Playlist()
        {

        }

        public Playlist(JObject playlistNode)
        {


            if (playlistNode.ContainsKey("image"))
                image = (string)playlistNode["image"];

            if (!string.IsNullOrWhiteSpace(image))
            {
                try
                {
                    icon = Sprites.Base64ToSprite(image.Substring(image.IndexOf(",") + 1));
                }
                catch
                {
                    Logging.Log.Critical("Unable to convert playlist image to sprite!");
                    icon = Sprites.PlaylistIcon;
                }
            }
            else
            {
                icon = Sprites.PlaylistIcon;
            }
            if (playlistNode.ContainsKey("playlistTitle"))
                playlistTitle = (string)playlistNode["playlistTitle"];
            if (playlistNode.ContainsKey("playlistAuthor"))
                playlistAuthor = (string)playlistNode["playlistAuthor"];
            if (playlistNode.ContainsKey("imcustomDetailUrlage"))
                customDetailUrl = (string)playlistNode["customDetailUrl"];
            if (playlistNode.ContainsKey("customArchiveUrl"))
                customArchiveUrl = (string)playlistNode["customArchiveUrl"];

            this.songs = new List<PlaylistSong>();

            JArray songs = (JArray)playlistNode["songs"];
            foreach (JObject song in songs)
            {
                PlaylistSong newSong = new PlaylistSong();
                if (song.ContainsKey("key"))
                    newSong.key = (string)song["key"];
                if (song.ContainsKey("songName"))
                    newSong.songName = (string)song["songName"];
                if (song.ContainsKey("hash"))
                    newSong.hash = (string)song["hash"];
                if (song.ContainsKey("levelId"))
                    newSong.levelId = (string)song["levelId"];

                this.songs.Add(newSong);
            }

            playlistSongCount = this.songs.Count;

            if (playlistNode.ContainsKey("fileLoc"))
                fileLoc = (string)playlistNode["fileLoc"];

            if (playlistNode.ContainsKey("playlistURL"))
                fileLoc = (string)playlistNode["playlistURL"];
        }

        public static Playlist LoadPlaylist(string path)
        {
            Playlist playlist = new Playlist(JObject.Parse(File.ReadAllText(path)));
            playlist.fileLoc = path;
            return playlist;
        }

        public void SavePlaylist(string path = "")
        {
            SharedCoroutineStarter.instance.StartCoroutine(SavePlaylistCoroutine(path));
        }

        public IEnumerator SavePlaylistCoroutine(string path = "")
        {
            Logging.Log.Info($"Saving playlist \"{playlistTitle}\"...");
            try
            {
                image = Sprites.SpriteToBase64(icon);
                playlistSongCount = songs.Count;
            }
            catch (Exception e)
            {
                Logging.Log.Critical("Unable to save playlist! Exception: " + e);
                yield break;
            }
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    fileLoc = Path.GetFullPath(path);
                }

                File.WriteAllText(fileLoc, JsonConvert.SerializeObject(this, Formatting.Indented));

                Logging.Log.Info("Playlist saved!");
            }
            catch (Exception e)
            {
                Logging.Log.Critical("Unable to save playlist! Exception: " + e);
                yield break;
            }
        }

        public bool PlaylistEqual(object obj)
        {
            if (obj == null) return false;

            var playlist = obj as Playlist;

            if (playlist == null) return false;

            int songCountThis = (songs != null ? (songs.Count > 0 ? songs.Count : playlistSongCount) : playlistSongCount);
            int songCountObj = (playlist.songs != null ? (playlist.songs.Count > 0 ? playlist.songs.Count : playlist.playlistSongCount) : playlist.playlistSongCount);

            return playlistTitle == playlist.playlistTitle &&
                   playlistAuthor == playlist.playlistAuthor &&
                   songCountThis == songCountObj;
        }

    }

    public class PlaylistSong
    {
        public string key { get { if (_key == null) return ""; else return _key; } set { _key = value; } }
        private string _key;
        public string songName { get { if (_songName == null) return ""; else return _songName; } set { _songName = value; } }
        private string _songName;

        public string hash { get { if (_hash == null) return ""; else return _hash; } set { _hash = value; } }
        private string _hash;

        public string levelId { get { if (_levelId == null) return ""; else return _levelId; } set { _levelId = value; } }
        private string _levelId;

        [JsonIgnore]
        public CustomPreviewBeatmapLevel level { get { return _level; } set { if (_level != value) { _level = value; UpdateSongInfo(); } } }
        private CustomPreviewBeatmapLevel _level;

        private void UpdateSongInfo()
        {
            if (level != null)
            {
                songName = level.songName + " - " + level.levelAuthorName;
                levelId = level.levelID;
                //bananbread id customlevel 
                hash = SongCore.Utilities.Hashing.GetCustomLevelHash(level);
            }
        }

    }
}
