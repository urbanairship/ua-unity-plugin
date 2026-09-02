/* Copyright Airship and Contributors */

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace AirshipSDK.Editor {
    public class AirshipMenu {
        [MenuItem ("Window/Airship/Settings", false, 1)]
        public static void Settings () {
            AirshipConfigEditor window = (AirshipConfigEditor) EditorWindow.GetWindow (typeof (AirshipConfigEditor), true, "Airship Config");
            window.minSize = new Vector2 (400, 400);
            window.Show ();
        }

        [MenuItem ("Window/Airship/Docs/API Docs")]
        public static void APIDocs () {
            Application.OpenURL (PluginInfo.APIDocsURL);
        }

        [MenuItem ("Window/Airship/Docs/Getting Started Guide")]
        public static void GettingStartedGuide () {
            Application.OpenURL (PluginInfo.GettingStartedGuideURL);
        }

        [MenuItem ("Window/Airship/About")]
        public static void About () {
            EditorUtility.DisplayDialog (
                "Airship",
                "Unity plugin version " + PluginInfo.Version,
                "Ok");
        }
    }
}
