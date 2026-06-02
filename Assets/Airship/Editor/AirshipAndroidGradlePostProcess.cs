/* Copyright Airship and Contributors */

#if UNITY_ANDROID
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.Android;
using UnityEngine;

namespace AirshipSDK.Editor {

    /// <summary>
    /// Patches the generated Android Gradle project so that it compiles with a
    /// Kotlin version compatible with the Airship SDK artifacts the plugin ships.
    /// </summary>
    public class AirshipAndroidGradlePostProcess : IPostGenerateGradleAndroidProject {

        /// <summary>
        /// The Kotlin version required to read the metadata of the bundled Airship
        /// SDK artifacts.
        /// </summary>
        private static readonly string RequiredKotlinVersion = PluginInfo.RequiredKotlinVersion;

        public int callbackOrder { get { return 999; } }

        public void OnPostGenerateGradleAndroidProject (string path) {
            // `path` points at the generated unityLibrary module. The Kotlin plugin
            // version is declared in the root project's build.gradle, one level up.
            DirectoryInfo unityLibraryDir = new DirectoryInfo (path);
            DirectoryInfo rootProjectDir = unityLibraryDir.Parent;
            if (rootProjectDir == null) {
                return;
            }

            // Patch any build.gradle that declares the Kotlin Android plugin. The
            // declaration usually lives in the root build.gradle, but we also check
            // the module to be robust against template differences.
            PatchKotlinVersion (Path.Combine (rootProjectDir.FullName, "build.gradle"));
            PatchKotlinVersion (Path.Combine (path, "build.gradle"));
        }

        private static void PatchKotlinVersion (string buildGradlePath) {
            if (!File.Exists (buildGradlePath)) {
                return;
            }

            string original = File.ReadAllText (buildGradlePath);
            string updated = original;

            // plugins DSL: id 'org.jetbrains.kotlin.android' version '1.7.22'
            updated = Regex.Replace (
                updated,
                @"(id\s+['""]org\.jetbrains\.kotlin\.android['""]\s+version\s+['""])([^'""]+)(['""])",
                "${1}" + RequiredKotlinVersion + "${3}");

            // Legacy buildscript classpath: classpath 'org.jetbrains.kotlin:kotlin-gradle-plugin:1.7.22'
            updated = Regex.Replace (
                updated,
                @"(['""]org\.jetbrains\.kotlin:kotlin-gradle-plugin:)([^'""]+)(['""])",
                "${1}" + RequiredKotlinVersion + "${3}");

            if (updated == original) {
                return;
            }

            File.WriteAllText (buildGradlePath, updated);
            Debug.Log ("Airship: set Kotlin Gradle plugin version to " +
                RequiredKotlinVersion + " in " + buildGradlePath);
        }
    }
}
#endif
