/* Copyright Airship and Contributors */

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UrbanAirship.Editor {
    [InitializeOnLoad]
    public class UAUpdater {

        private static string[] obsoleteFiles = {
            "Assets/UrbanAirship/Editor/UADependencies.cs",
            "Assets/Plugins/Android",
            "Assets/PlayServicesResolver"
        };

        private static string[] obsoleteDirectories = {
            "Assets/Plugins/Android/urbanairship-plugin-lib",
            "Assets/Plugins/Android/urbanairship-sdk",
            "Assets/Plugins/Android/urbanairship-resources",
            "Assets/UrbanAirship/Editor/m2repository",
            "Assets/Plugins/iOS/Airship"
        };

        static UAUpdater () {
            MigrateResources ();
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
                Directory.Move (dir, Path.Combine ("Assets/Plugins/Android/urbanairship-resources.androidlib/res", name));
                refreshAssets = true;
            }

            if (refreshAssets) {
                AssetDatabase.Refresh ();
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
