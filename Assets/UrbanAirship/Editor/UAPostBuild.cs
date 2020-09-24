/* Copyright Airship and Contributors */

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace UrbanAirship.Editor {

    public class UAPostBuild {

        [PostProcessBuildAttribute (1)]
        public static void OnPostProcessBuild (BuildTarget target, string buildPath) {

#if UNITY_IOS
            if (target == BuildTarget.iOS) {
                UpdatePbxProject (buildPath + "/Unity-iPhone.xcodeproj/project.pbxproj", buildPath);
                UpdateProjectPlist (buildPath + "/Info.plist");
            }
#endif

            UnityEngine.Debug.Log ("Finished Urban Airship post build steps.");
        }

#if UNITY_IOS
        private static void UpdatePbxProject (string projectPath, string buildPath) {
            PBXProject proj = new PBXProject ();
            proj.ReadFromString (File.ReadAllText (projectPath));

#if UNITY_2019_3_OR_NEWER
            string[] targets = {
                proj.GetUnityMainTargetGuid ()
            };
#else
            string[] targets = {
                proj.TargetGuidByName (PBXProject.GetUnityTargetName ()),
                proj.TargetGuidByName (PBXProject.GetUnityTestTargetName ())
            };
#endif

            string airshipConfig = Path.Combine (buildPath, "AirshipConfig.plist");
            if (File.Exists (airshipConfig)) {
                File.Delete (airshipConfig);
            }

            File.Copy (Path.Combine (Application.dataPath, "Plugins/iOS/AirshipConfig.plist"), airshipConfig);
            string airshipGUID = proj.AddFile ("AirshipConfig.plist", "AirshipConfig.plist", PBXSourceTree.Source);

            foreach (string target in targets) {
                proj.AddFileToBuild (target, airshipGUID);
                proj.AddBuildProperty(target, "CLANG_ENABLE_MODULES", "YES");
            }

            File.WriteAllText (projectPath, proj.WriteToString ());
        }

        private static void UpdateProjectPlist (string plistPath) {
            PlistDocument plist = new PlistDocument ();
            plist.ReadFromString (File.ReadAllText (plistPath));

            PlistElementDict rootDict = plist.root;
            rootDict.CreateArray ("UIBackgroundModes").AddString ("remote-notification");
            rootDict.SetString ("UAUnityPluginVersion", UrbanAirship.PluginInfo.Version);
            File.WriteAllText (plistPath, plist.WriteToString ());
        }
#endif
    }
}
