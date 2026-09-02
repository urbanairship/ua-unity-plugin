/* Copyright Airship and Contributors */

#if UNITY_ANDROID
using System;
using System.Globalization;
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
            // Without a parseable requirement there is nothing to compare against, so leave
            // the project alone. This is also the state inside the plugin repo itself, where
            // PluginInfo.RequiredKotlinVersion is still the unsubstituted build placeholder.
            if (ParseVersion (RequiredKotlinVersion) == null) {
                Debug.Log ("Airship: no usable required Kotlin version (" +
                    RequiredKotlinVersion + "); leaving the Gradle project unchanged.");
                return;
            }

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
                RaiseKotlinVersion);

            // Legacy buildscript classpath: classpath 'org.jetbrains.kotlin:kotlin-gradle-plugin:1.7.22'
            updated = Regex.Replace (
                updated,
                @"(['""]org\.jetbrains\.kotlin:kotlin-gradle-plugin:)([^'""]+)(['""])",
                RaiseKotlinVersion);

            if (updated == original) {
                return;
            }

            File.WriteAllText (buildGradlePath, updated);
            Debug.Log ("Airship: set Kotlin Gradle plugin version to " +
                RequiredKotlinVersion + " in " + buildGradlePath);
        }

        /// <summary>
        /// Raises a matched Kotlin plugin version to the required one, leaving a version
        /// that is already new enough untouched.
        ///
        /// Downgrading is never safe: a customer on a newer Kotlin than the one the bundled
        /// Airship artifacts were built against has a working build, and rewriting it down
        /// breaks their other Kotlin dependencies. The requirement is a floor, not a pin.
        /// </summary>
        private static string RaiseKotlinVersion (Match match) {
            string existing = match.Groups[2].Value;

            if (CompareVersions (existing, RequiredKotlinVersion) >= 0) {
                return match.Value;
            }

            return match.Groups[1].Value + RequiredKotlinVersion + match.Groups[3].Value;
        }

        /// <summary>
        /// Compares two Gradle plugin versions component by component. A version that
        /// cannot be parsed -- a Gradle variable reference such as `$kotlin_version`, or an
        /// unsubstituted placeholder -- compares as the greater value, so an unrecognized
        /// version is left in place rather than overwritten on a guess.
        /// </summary>
        private static int CompareVersions (string left, string right) {
            int[] leftParts = ParseVersion (left);
            int[] rightParts = ParseVersion (right);

            if (leftParts == null || rightParts == null) {
                return 1;
            }

            int count = Math.Max (leftParts.Length, rightParts.Length);
            for (int i = 0; i < count; i++) {
                int l = i < leftParts.Length ? leftParts[i] : 0;
                int r = i < rightParts.Length ? rightParts[i] : 0;
                if (l != r) {
                    return l < r ? -1 : 1;
                }
            }

            return 0;
        }

        /// <summary>
        /// Splits a version into its numeric components, dropping any pre-release or build
        /// suffix ("2.3.0-Beta1" -> 2.3.0). Returns <c>null</c> if it is not numeric.
        /// </summary>
        private static int[] ParseVersion (string version) {
            if (string.IsNullOrEmpty (version)) {
                return null;
            }

            int suffix = version.IndexOfAny (new[] { '-', '+' });
            string numeric = suffix < 0 ? version : version.Substring (0, suffix);

            string[] parts = numeric.Split ('.');
            int[] components = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++) {
                if (!int.TryParse (parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out components[i])) {
                    return null;
                }
            }

            return components;
        }
    }
}
#endif
