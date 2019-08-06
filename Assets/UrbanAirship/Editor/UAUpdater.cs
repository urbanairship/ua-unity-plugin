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
            "Assets/UrbanAirship/Editor/m2repository"
        };

        static UAUpdater () {

            MigrateResources ();
            DeleteObsoleteFiles ();
            RemoveObsoleteIOSLibraries(PluginInfo.IOSAirshipVersion);
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
                Directory.Move (dir, Path.Combine ("Assets/Plugins/Android/urbanairship-resources/res", name));
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

        /// <summary>
        /// Removes obsolete Airship iOS libraries
        /// </summary>
        /// <param name="keepVersion">iOS library version to keep</param>
        private static void RemoveObsoleteIOSLibraries(string keepVersion)
        {
            Debug.Log("UAUpdater: Removing obsolete iOS libraries");

            string iOSLibraryFolder = "Assets/Plugins/iOS/Airship";

            // get a list of all the libraries in the plugin
            string[] libraries = Directory.GetFiles(iOSLibraryFolder, "libUA*");

            // filter the new library out of the list
            string[] librariesToRemove = Array.FindAll(libraries, library => !library.Contains(keepVersion));

            if (librariesToRemove.Length == 0) {
                Debug.Log("UAUpdater: No obsolete libraries to remove");
                return;
            }

            foreach (string library in librariesToRemove) {
                Debug.Log("UAUpdater: Deleting file: " + library);
                File.Delete(library);
            }

            Debug.Log("UAUpdater: Removed obsolete iOS libraries");
        }
    }
}
