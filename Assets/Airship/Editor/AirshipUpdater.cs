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

        private const string ResourcesLibRes = "Assets/Plugins/Android/airship-resources.androidlib/res";

        // Legacy resource roots holding customer notification icons, newest layout first.
        // 9.x kept them in urbanairship-resources.androidlib/res -- the 9.x updater moved
        // them there itself -- and older installs still had them under
        // urbanairship-plugin-lib/res. Both roots are in obsoleteDirectories, so anything
        // left behind here is deleted, which is why migration has to cover both.
        private static string[] legacyResourceRoots = {
            "Assets/Plugins/Android/urbanairship-resources.androidlib/res",
            "Assets/Plugins/Android/urbanairship-plugin-lib/res"
        };

        private static void MigrateResources () {
            bool refreshAssets = false;

            foreach (string root in legacyResourceRoots) {
                if (!Directory.Exists (root)) {
                    continue;
                }

                foreach (string drawable in Directory.GetDirectories (root, "drawable*")) {
                    if (MigrateDrawableDirectory (drawable)) {
                        refreshAssets = true;
                    }
                }
            }

            if (refreshAssets) {
                AssetDatabase.Refresh ();
            }
        }

        /// <summary>
        /// Moves one drawable-* directory into the new androidlib, merging into an existing
        /// destination instead of failing the way Directory.Move would. A file already
        /// present at the destination is left alone and its source copy is kept, so
        /// DeleteObsoleteFiles can refuse to delete the directory rather than discard it.
        /// </summary>
        /// <returns><c>true</c> if anything moved.</returns>
        private static bool MigrateDrawableDirectory (string source) {
            string destination = Path.Combine (ResourcesLibRes, Path.GetFileName (source));
            bool moved = false;

            try {
                Directory.CreateDirectory (destination);

                foreach (string file in Directory.GetFiles (source)) {
                    string target = Path.Combine (destination, Path.GetFileName (file));
                    if (File.Exists (target)) {
                        continue;
                    }
                    File.Move (file, target);
                    moved = true;
                }
            } catch (Exception e) {
                Debug.LogError ("Airship: failed to migrate notification resources from " +
                    source + ". Move them into " + ResourcesLibRes + " manually. " + e.Message);
                return moved;
            }

            return moved;
        }

        /// <summary>
        /// Checks whether an obsolete directory still holds drawable resources that
        /// MigrateResources could not move. Deleting one of these would destroy the
        /// customer's notification icons, so the delete is skipped instead.
        /// </summary>
        private static bool HasUnmigratedResources (string directory) {
            string res = Path.Combine (directory, "res");
            if (!Directory.Exists (res)) {
                return false;
            }

            foreach (string drawable in Directory.GetDirectories (res, "drawable*")) {
                if (Directory.GetFiles (drawable).Length > 0) {
                    return true;
                }
            }

            return false;
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
                if (!Directory.Exists (directory)) {
                    continue;
                }

                if (HasUnmigratedResources (directory)) {
                    Debug.LogWarning ("Airship: " + directory + " still contains drawable " +
                        "resources that could not be migrated, so it was left in place. " +
                        "Move them into " + ResourcesLibRes + " and delete the directory.");
                    continue;
                }

                Directory.Delete (directory, true);
                refreshAssets = true;
            }

            if (refreshAssets) {
                AssetDatabase.Refresh ();
            }
        }
    }
}
