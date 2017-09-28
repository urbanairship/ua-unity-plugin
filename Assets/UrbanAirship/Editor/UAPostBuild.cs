/*
 Copyright 2017 Urban Airship and Contributors
*/
using System;


using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Diagnostics;
using System.IO;
using System.Linq;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace UrbanAirship.Editor
{

	public class UAPostBuild
	{

		[PostProcessBuildAttribute(1)]
		public static void OnPostProcessBuild(BuildTarget target, string buildPath)
		{
			if (!UAConfig.LoadConfig().IsValid)
			{
				EditorUtility.DisplayDialog("Urban Airship", "Urban Airship not configured. Set the app credentials in Window -> Urban Airship -> Settings", "OK");
			}

#if UNITY_IOS
			if (target == BuildTarget.iOS)
			{
				UpdatePbxProject(buildPath + "/Unity-iPhone.xcodeproj/project.pbxproj", buildPath);
				UpdateProjectPlist(buildPath + "/Info.plist");
			}
#endif

			UnityEngine.Debug.Log("Finished Urban Airship post build steps.");
		}

#if UNITY_IOS
		private static void UpdatePbxProject(string projectPath, string buildPath)
		{
			PBXProject proj = new PBXProject();
			proj.ReadFromString(File.ReadAllText(projectPath));

			string[] targets = {
				proj.TargetGuidByName(PBXProject.GetUnityTargetName()),
				proj.TargetGuidByName(PBXProject.GetUnityTestTargetName())
			};

			string airshipConfig = Path.Combine(buildPath, "AirshipConfig.plist");
			if (File.Exists(airshipConfig)) {
				File.Delete(airshipConfig);
			}

			File.Copy(Path.Combine(Application.dataPath, "Plugins/iOS/AirshipConfig.plist"), airshipConfig);
			string airshipGUID = proj.AddFile("AirshipConfig.plist", "AirshipConfig.plist", PBXSourceTree.Source);

			foreach (string target in targets)
			{
				proj.AddFileToBuild(target, airshipGUID);
			}

			File.WriteAllText(projectPath, proj.WriteToString());
		}

		private static void UpdateProjectPlist(string plistPath)
		{
			PlistDocument plist = new PlistDocument();
			plist.ReadFromString(File.ReadAllText(plistPath));


			PlistElementDict rootDict = plist.root;
			if (rootDict ["UIBackgroundModes"] == null) {
				rootDict.CreateArray ("UIBackgroundModes");
			}

			PlistElementArray backgroundModes = rootDict ["UIBackgroundModes"].AsArray ();
			if (backgroundModes.values.Find ((m) => "remote-data" == m.AsString ()) == null) {
				backgroundModes.AddString("remote-data");
			}

			File.WriteAllText(plistPath, plist.WriteToString());
		}
#endif
	}
}
