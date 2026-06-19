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

namespace AirshipSDK.Editor {

    public class AirshipPostBuild {

        [PostProcessBuildAttribute (1)]
        public static void OnPostProcessBuild (BuildTarget target, string buildPath) {

#if UNITY_IOS
            if (target == BuildTarget.iOS) {
                UpdatePbxProject (buildPath + "/Unity-iPhone.xcodeproj/project.pbxproj", buildPath);
                UpdateProjectPlist (buildPath + "/Info.plist");
            }
#endif

            UnityEngine.Debug.Log ("Finished Airship post build steps.");
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

            string airshipConfigSource = Path.Combine (Application.dataPath, "Plugins/iOS/AirshipConfig.plist");
            if (File.Exists (airshipConfigSource)) {
                string airshipConfig = Path.Combine (buildPath, "AirshipConfig.plist");
                if (File.Exists (airshipConfig)) {
                    File.Delete (airshipConfig);
                }

                File.Copy (airshipConfigSource, airshipConfig);
                string airshipGUID = proj.AddFile ("AirshipConfig.plist", "AirshipConfig.plist", PBXSourceTree.Source);

                foreach (string target in targets) {
                    proj.AddFileToBuild (target, airshipGUID);
                }
            }

            // Update the Header Search Paths
            // so the Swift compiler finds the module.modulemap file.
            UpdateHeaderSearchPaths(buildPath, proj);

            // Add a script phase to copy SPM resource bundles into the app bundle.
            // SPM static libraries produce resource bundles (e.g. Airship_AirshipCore.bundle)
            // that need to be inside the .app for Bundle.module to find them at runtime.
            AddCopySPMResourceBundlesPhase(proj);

            File.WriteAllText (projectPath, proj.WriteToString ());
        }

        private static void UpdateProjectPlist (string plistPath) {
            PlistDocument plist = new PlistDocument ();
            plist.ReadFromString (File.ReadAllText (plistPath));

            PlistElementDict rootDict = plist.root;

            PlistElementArray backgroundModes;
            if (rootDict.values.TryGetValue ("UIBackgroundModes", out PlistElement existingModes) && existingModes is PlistElementArray existingArray) {
                backgroundModes = existingArray;
            } else {
                backgroundModes = rootDict.CreateArray ("UIBackgroundModes");
            }

            bool hasRemoteNotification = backgroundModes.values.Any (element => (element as PlistElementString)?.value == "remote-notification");
            if (!hasRemoteNotification) {
                backgroundModes.AddString ("remote-notification");
            }

            rootDict.SetString ("AirshipUnityPluginVersion", PluginInfo.Version);
            File.WriteAllText (plistPath, plist.WriteToString ());
        }

        private static void UpdateHeaderSearchPaths (string buildPath, PBXProject proj) {
            // Copy the Airship modulemap into the iOS project
            CopyModuleMap (buildPath, proj);

#if UNITY_2019_3_OR_NEWER
            string[] targets = {
                proj.GetUnityFrameworkTargetGuid ()
            };
#else
            // Fallback for older Unity versions
            string[] targets = {
                proj.TargetGuidByName (PBXProject.GetUnityTargetName ())
            };
#endif
            
            // Add the path to HEADER_SEARCH_PATHS for all relevant targets.
            // This allows the Swift compiler to find the module.modulemap file.
            foreach (string target in targets) {
                proj.AddBuildProperty(target, "HEADER_SEARCH_PATHS", "$(SRCROOT)/Libraries/Plugins/iOS");
                // Ensure existing paths are inherited
                proj.AddBuildProperty(target, "HEADER_SEARCH_PATHS", "$(inherited)");
            }
        }

        private static void AddCopySPMResourceBundlesPhase (PBXProject proj) {
#if UNITY_2019_3_OR_NEWER
            string mainTargetGuid = proj.GetUnityMainTargetGuid ();
#else
            string mainTargetGuid = proj.TargetGuidByName (PBXProject.GetUnityTargetName ());
#endif

            string shellScript =
                "# Copy Airship SPM resource bundles into the app bundle so Bundle.module can find them.\n" +
                "DEST=\"${BUILT_PRODUCTS_DIR}/${PRODUCT_NAME}.app\"\n" +
                "FOUND=0\n" +
                "# Check the build products directory (standard SPM static library location)\n" +
                "for BUNDLE in \"${BUILT_PRODUCTS_DIR}\"/Airship_*.bundle; do\n" +
                "  [ -d \"$BUNDLE\" ] || continue\n" +
                "  echo \"Copying SPM resource bundle: $(basename \"$BUNDLE\") from BUILT_PRODUCTS_DIR\"\n" +
                "  cp -R \"$BUNDLE\" \"$DEST/\"\n" +
                "  FOUND=1\n" +
                "done\n" +
                "# Fallback: check inside UnityFramework.framework\n" +
                "if [ \"$FOUND\" = \"0\" ] && [ -d \"${BUILT_PRODUCTS_DIR}/UnityFramework.framework\" ]; then\n" +
                "  for BUNDLE in \"${BUILT_PRODUCTS_DIR}/UnityFramework.framework\"/Airship_*.bundle; do\n" +
                "    [ -d \"$BUNDLE\" ] || continue\n" +
                "    echo \"Copying SPM resource bundle: $(basename \"$BUNDLE\") from UnityFramework.framework\"\n" +
                "    cp -R \"$BUNDLE\" \"$DEST/\"\n" +
                "  done\n" +
                "fi\n";

            proj.AddShellScriptBuildPhase (
                mainTargetGuid,
                "Copy Airship SPM Resource Bundles",
                "/bin/sh",
                shellScript
            );
        }

        private static void CopyModuleMap (string buildPath, PBXProject proj) {
            string sourceDir = Path.Combine(Application.dataPath, "Plugins/iOS");
            string destinationDir = Path.Combine(buildPath, "Libraries/Plugins/iOS");

            if (!Directory.Exists (destinationDir)) {
                Directory.CreateDirectory (destinationDir);
            }

            // Copy module.modulemap
            string mapFileName = "module.modulemap";
            File.Copy(Path.Combine(sourceDir, mapFileName), 
                      Path.Combine(destinationDir, mapFileName), 
                      true); // 'true' overwrites if file exists
        }
#endif
    }
}
