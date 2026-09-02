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

                // Process google-services.json independently of the app credentials.
                // Controlled solely by the "Process google-service" setting.
                // It works whether the app is configured via the editor or at runtime via TakeOff.
                if (target == BuildTarget.Android) {
                    config.ApplyFirebaseConfig ();
                }

                if (!config.IsConfigured) {
                    UnityEngine.Debug.Log ("Airship editor config is empty. " +
                        "Skipping app credential config generation. Make sure to call Airship.Shared.TakeOff() at runtime.");
                    return;
                }

                string validationError = config.ValidationError;
                if (validationError != null) {
                    // Credentials were entered but are incomplete, so this is a mistake rather
                    // than a deliberate runtime-TakeOff setup. Returning here shipped a player
                    // with no Airship config at all, and in batchmode the dialog is a no-op,
                    // so CI produced that player silently.
                    string message = validationError +
                        " Set the app credentials in Window -> Airship -> Settings, or clear them " +
                        "and call Airship.Shared.TakeOff() at runtime.";

                    EditorUtility.DisplayDialog ("Airship", message, "OK");
                    throw new BuildFailedException ("Airship: " + message);
                }

                config.Apply ();
                UnityEngine.Debug.Log ("Updated Airship Config");
            }
        }
    }

}
