/*
 Copyright 2016 Urban Airship and Contributors
*/

#if UNITY_IPHONE

using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor.iOS.Xcode;

namespace UrbanAirship
{

	public class UAIOSPostBuild
	{

		[PostProcessBuildAttribute(1)]
		public static void OnPostprocessBuild(BuildTarget target, string buildPath)
		{

			if (target != BuildTarget.iOS)
			{
				return;
			}

			try
			{
				UpdatePbxProject(buildPath + "/Unity-iPhone.xcodeproj/project.pbxproj");
				UpdateProjectPlist(buildPath + "/Info.plist");
				UnityEngine.Debug.Log("Finished Urban Airship iOS post build script.");
			} catch (System.Exception ex) {
				UnityEngine.Debug.Log("Urban Airship iOS SDK failed: " + ex.Message);
			}
		}

		private static void UpdatePbxProject(string projectPath)
		{
			PBXProject proj = new PBXProject();
			proj.ReadFromString(File.ReadAllText(projectPath));

			string[] targets = {
				proj.TargetGuidByName (PBXProject.GetUnityTargetName ()),
				proj.TargetGuidByName (PBXProject.GetUnityTestTargetName ())
			};

			foreach (string target in targets)
			{
				proj.AddBuildProperty(target, "OTHER_LDFLAGS", "$(inherited)");
				proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC -lz -lsqlite3");
			}

			File.WriteAllText(projectPath, proj.WriteToString());
		}

		private static void UpdateProjectPlist(string plistPath)
		{
			PlistDocument plist = new PlistDocument();
			plist.ReadFromString(File.ReadAllText(plistPath));

			PlistElementDict rootDict = plist.root;
			rootDict.CreateArray("UIBackgroundModes").AddString("remote-notification");

			File.WriteAllText(plistPath, plist.WriteToString());
		}
	}
}

#endif