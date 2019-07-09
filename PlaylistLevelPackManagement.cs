using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlaylistCore.Data;
using SongCore.OverrideClasses;
using PlaylistCore.Utilities;
namespace PlaylistCore
{
    public class PlaylistLevelPackManagement
    {

        public static void SetPlaylistPacks()
        {
            SongCoreBeatmapLevelPackCollectionSO newCollection = SongCore.Loader.CustomBeatmapLevelPackCollectionSO;
            List<CustomBeatmapLevelPack> _customBeatmapLevelPacks = newCollection.GetPrivateField<List<CustomBeatmapLevelPack>>("_customBeatmapLevelPacks");
            List<IBeatmapLevelPack> _allBeatmapLevelPacks = newCollection.GetPrivateField<IBeatmapLevelPack[]>("_allBeatmapLevelPacks").ToList();

            _customBeatmapLevelPacks.RemoveAll(x => x.packID.StartsWith($"{CustomLevelLoaderSO.kCustomLevelPackPrefixId}Playlist_"));
            _allBeatmapLevelPacks.RemoveAll(x => x.packID.StartsWith($"{CustomLevelLoaderSO.kCustomLevelPackPrefixId}Playlist_"));

            newCollection.SetPrivateField("_customBeatmapLevelPacks", _customBeatmapLevelPacks);
            newCollection.SetPrivateField("_allBeatmapLevelPacks", _allBeatmapLevelPacks.ToArray());

            foreach (var playlist in PlaylistsCollection.loadedPlaylists)
            {
                var pack = PlaylistsCollection.CreateLevelPackFromPlaylist(playlist);
                Logging.Log.Info($"{pack.packName} {pack.beatmapLevelCollection.beatmapLevels.Count()}");
                newCollection.AddLevelPack(pack);
            }

        }
    }
}
