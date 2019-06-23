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

namespace PlaylistCore
{
    public class Plugin : IBeatSaberPlugin
    {


        public void OnApplicationStart()
        {


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

    }
}
