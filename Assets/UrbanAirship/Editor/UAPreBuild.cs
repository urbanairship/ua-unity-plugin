/* Copyright Urban Airship and Contributors */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

namespace UrbanAirship.Editor {

#if UNITY_2018_1_OR_NEWER
    public class UAPreBuild : IPreprocessBuildWithReport {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild (BuildReport report) {
            GenerateConfig.Apply (report.summary.platform);
        }
    }
#else
    public class UAPreBuild : IPreprocessBuild {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild (BuildTarget target, string path) {
            GenerateConfig.Apply (target);
        }
    }
#endif

    class GenerateConfig {
        public static void Apply (BuildTarget target) {
            if (target == BuildTarget.iOS || target == BuildTarget.Android) {
                UAConfig config = UAConfig.LoadConfig ();
                if (!config.IsValid) {
                    EditorUtility.DisplayDialog ("Urban Airship", "Urban Airship not configured. Set the app credentials in Window -> Urban Airship -> Settings", "OK");
                    return;
                }

                config.Apply ();
                UnityEngine.Debug.Log ("Updated Urban Airship Config");
            }
        }
    }

}
