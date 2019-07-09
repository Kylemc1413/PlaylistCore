using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPA;
using IPALogger = IPA.Logging.Logger;
using PlaylistCore.Data;
using PlaylistCore.Utilities;
namespace PlaylistCore
{
    public class Plugin : IBeatSaberPlugin
    {


        public void OnApplicationStart()
        {
            Sprites.ConvertToSprites();
            PlaylistsCollection.ReloadPlaylists();
            SongCore.Loader.SongsLoadedEvent += Loader_SongsLoadedEvent;
          //  SongCore.Loader.OnLevelPacksRefreshed += ;
        }

        private void Loader_SongsLoadedEvent(SongCore.Loader arg1, Dictionary<string, CustomPreviewBeatmapLevel> arg2)
        {
            PlaylistsCollection.MatchSongsForAllPlaylists();
         //   DebugLogPlaylists();

        }

        public void Init(object thisIsNull, IPALogger pluginLogger)
        {

            Utilities.Logging.Log = pluginLogger;
        }

        public void OnApplicationQuit()
        {

        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {

        }

        public void OnSceneUnloaded(Scene scene)
        {

        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {

        }

        public void OnUpdate()
        {

        }

        public void OnFixedUpdate()
        {

        }

        public static void DebugLogPlaylists()
        {
            foreach(var playlist in PlaylistsCollection.loadedPlaylists)
            {
                Logging.Log.Notice(playlist.playlistTitle + " - " + playlist.playlistAuthor);

                foreach(var c in playlist.songs)
                {
                    Logging.Log.Warn(c.songName + c.hash);
                    if(c.level != null)
                        Logging.Log.Notice(c.level.customLevelPath);
                }
            }
        }
    }
}
