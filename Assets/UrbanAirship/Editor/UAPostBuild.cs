/*
 Copyright 2018 Urban Airship and Contributors
*/
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

            string[] frameworks = {
                "CFNetwork.framework",
                "CoreGraphics.framework",
                "Foundation.framework",
                "MobileCoreServices.framework",
                "Security.framework",
                "SystemConfiguration.framework",
                "UIKit.framework",
                "CoreTelephony.framework",
                "CoreLocation.framework",
                "CoreData.framework",
                "UserNotifications.framework",
                "WebKit.framework",
                "StoreKit.framework"
            };

            string[] targets = {
                proj.TargetGuidByName (PBXProject.GetUnityTargetName ()),
                proj.TargetGuidByName (PBXProject.GetUnityTestTargetName ())
            };

            string airshipConfig = Path.Combine (buildPath, "AirshipConfig.plist");
            if (File.Exists (airshipConfig)) {
                File.Delete (airshipConfig);
            }

            File.Copy (Path.Combine (Application.dataPath, "Plugins/iOS/AirshipConfig.plist"), airshipConfig);
            string airshipGUID = proj.AddFile ("AirshipConfig.plist", "AirshipConfig.plist", PBXSourceTree.Source);

            foreach (string target in targets) {
                proj.AddBuildProperty (target, "OTHER_LDFLAGS", "$(inherited)");
                proj.AddBuildProperty (target, "OTHER_LDFLAGS", "-ObjC -lz -lsqlite3");
                proj.AddFileToBuild (target, airshipGUID);

                foreach (string framework in frameworks) {
                    proj.AddFrameworkToProject (target, framework, false);
                }
            }

            File.WriteAllText (projectPath, proj.WriteToString ());
        }

        private static void UpdateProjectPlist (string plistPath) {
            PlistDocument plist = new PlistDocument ();
            plist.ReadFromString (File.ReadAllText (plistPath));

            PlistElementDict rootDict = plist.root;
            rootDict.CreateArray ("UIBackgroundModes").AddString ("remote-notification");

            File.WriteAllText (plistPath, plist.WriteToString ());
        }
#endif
    }
}