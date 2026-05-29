/* Copyright Airship and Contributors */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

namespace AirshipSDK.Editor {

#if UNITY_2018_1_OR_NEWER
    public class AirshipPreBuild : IPreprocessBuildWithReport {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild (BuildReport report) {
            GenerateConfig.Apply (report.summary.platform);
        }
    }
#else
    public class AirshipPreBuild : IPreprocessBuild {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild (BuildTarget target, string path) {
            GenerateConfig.Apply (target);
        }
    }
#endif

    class GenerateConfig {
        public static void Apply (BuildTarget target) {
            if (target == BuildTarget.iOS || target == BuildTarget.Android) {
                AirshipConfig config = AirshipConfig.LoadConfig ();

                if (!config.IsConfigured) {
                    UnityEngine.Debug.Log ("Airship editor config is empty. " +
                        "Skipping config file generation. Make sure to call Airship.Shared.TakeOff() at runtime.");
                    return;
                }

                if (!config.IsValid) {
                    EditorUtility.DisplayDialog ("Airship", "Airship not configured. Set the app credentials in Window -> Airship -> Settings", "OK");
                    return;
                }

                config.Apply ();
                UnityEngine.Debug.Log ("Updated Airship Config");
            }
        }
    }

}
