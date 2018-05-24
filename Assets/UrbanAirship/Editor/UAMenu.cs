/*
 Copyright 2018 Urban Airship and Contributors
*/

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace UrbanAirship.Editor {
    public class UAMenu {
        [MenuItem ("Window/Urban Airship/Settings", false, 1)]
        public static void Settings () {
            UAConfigEditor window = (UAConfigEditor) EditorWindow.GetWindow (typeof (UAConfigEditor), true, "Urban Airship Config");
            window.minSize = new Vector2 (400, 400);
            window.Show ();
        }

        [MenuItem ("Window/Urban Airship/Docs/API Docs")]
        public static void APIDocs () {
            Application.OpenURL (PluginInfo.APIDocsURL);
        }

        [MenuItem ("Window/Urban Airship/Docs/Getting Started Guide")]
        public static void GettingStartedGuide () {
            Application.OpenURL (PluginInfo.GettingStartedGuideURL);
        }

        [MenuItem ("Window/Urban Airship/About")]
        public static void About () {
            EditorUtility.DisplayDialog (
                "Urban Airship",
                "Unity plugin version " + PluginInfo.Version,
                "Ok");
        }
    }
}
