/* Copyright Airship and Contributors */

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AirshipSDK.Editor {
    [InitializeOnLoad]
    public class AirshipUpdater {

        private static string[] obsoleteFiles = {
            "Assets/UrbanAirship/Editor/UADependencies.cs",
            "Assets/Scripts/UrbanAirshipBehaviour.cs",
            "Assets/Plugins/iOS/UAUnityPlugin.h",
            "Assets/Plugins/iOS/UAUnityPlugin.m",
            "Assets/Plugins/iOS/UAUnityMessageViewController.h",
            "Assets/Plugins/iOS/UAUnityMessageViewController.m"
        };

        private static string[] obsoleteDirectories = {
            "Assets/UrbanAirship",
            "Assets/PlayServicesResolver",
            "Assets/Plugins/Android/urbanairship-resources.androidlib",
            "Assets/Plugins/Android/urbanairship-plugin-lib",
            "Assets/Plugins/Android/urbanairship-sdk",
            "Assets/Plugins/Android/urbanairship-resources",
            "Assets/UrbanAirship/Editor/m2repository",
            "Assets/Plugins/iOS/Airship"
        };

        static AirshipUpdater () {
            MigrateResources ();
            MigrateSettings ();
            DeleteObsoleteFiles ();
        }

        private static void MigrateResources () {
            if (!Directory.Exists ("Assets/Plugins/Android/urbanairship-plugin-lib/res")) {
                return;
            }

            string[] drawables = Directory.GetDirectories ("Assets/Plugins/Android/urbanairship-plugin-lib/res", "drawable*");
            if (drawables.Length == 0) {
                return;
            }

            bool refreshAssets = false;

            foreach (string dir in drawables) {
                string name = Path.GetDirectoryName (dir);
                Directory.Move (dir, Path.Combine ("Assets/Plugins/Android/airship-resources.androidlib/res", name));
                refreshAssets = true;
            }

            if (refreshAssets) {
                AssetDatabase.Refresh ();
            }
        }

        private static void MigrateSettings () {
            string oldPath = "ProjectSettings/UrbanAirship.xml";
            string newPath = "ProjectSettings/Airship.xml";
            if (File.Exists (oldPath) && !File.Exists (newPath)) {
                File.Move (oldPath, newPath);
            }
        }

        private static void DeleteObsoleteFiles () {
            bool refreshAssets = false;

            foreach (string file in obsoleteFiles) {
                if (File.Exists (file)) {
                    File.Delete (file);
                    refreshAssets = true;
                }
            }

            foreach (string directory in obsoleteDirectories) {
                if (Directory.Exists (directory)) {
                    Directory.Delete (directory, true);
                    refreshAssets = true;
                }
            }

            if (refreshAssets) {
                AssetDatabase.Refresh ();
            }
        }
    }
}
