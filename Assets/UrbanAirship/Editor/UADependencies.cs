/*
 Copyright 2016 Urban Airship and Contributors
*/

using System;
using System.Collections.Generic;
using UnityEditor;
using UrbanAirship;

namespace UrbanAirship.Editor
{

	[InitializeOnLoad]
	public class UADependencies : AssetPostprocessor
	{

		/// <summary>
		/// The name of your plugin.  This is used to create a settings file
		/// which contains the dependencies specific to your plugin.
		/// </summary>
		private static readonly string PluginName = "UrbanAirship";

		/// <summary>Instance of the PlayServicesSupport resolver</summary>
		public static object svcSupport;

		static UADependencies()
		{
			RegisterDependencies();
		}

		public static void RegisterDependencies() {
			#if UNITY_ANDROID
			RegisterAndroidDependencies();
			#elif UNITY_IOS
			RegisterIOSDependencies();
			#endif
		}

		/// <summary>
		/// Registers the android dependencies.
		/// </summary>
		public static void RegisterAndroidDependencies() {

			// Setup the resolver using reflection as the module may not be
			// available at compile time.
			Type playServicesSupport = Google.VersionHandler.FindClass(
				"Google.JarResolver", "Google.JarResolver.PlayServicesSupport");

			if (playServicesSupport == null) {
				return;
			}

			svcSupport = svcSupport ?? Google.VersionHandler.InvokeStaticMethod(
				playServicesSupport, "CreateInstance",
				new object[] {
					PluginName,
					EditorPrefs.GetString("AndroidSdkRoot"),
					"ProjectSettings"
				});

			Google.VersionHandler.InvokeInstanceMethod(
				svcSupport, "DependOn",
				new object[] { "com.android.support", "support-v4", PluginInfo.AndroidSupportLibVersion },
				namedArgs: new Dictionary<string, object>() {
					{"packageIds", new string[] { "extra-android-m2repository" } }
				});

			Google.VersionHandler.InvokeInstanceMethod(
				svcSupport, "DependOn",
				new object[] { "com.google.android.gms", "play-services-gcm", PluginInfo.AndroidPlayServicesVersion },
				namedArgs: new Dictionary<string, object>() {
					{"packageIds", new string[] { "extra-android-m2repository" } }
				});
		}

		/// <summary>
		/// Registers the IOS dependencies.
		/// </summary>
		public static void RegisterIOSDependencies() {

			// Setup the resolver using reflection as the module may not be
			// available at compile time.
			Type iosResolver = Google.VersionHandler.FindClass(
				"Google.IOSResolver", "Google.IOSResolver");

			if (iosResolver == null) {
				return;
			}

			// Dependencies for iOS are added by referring to CocoaPods.  The libraries and frameworkds are
			//  and added to the Unity project, so they will automatically be included.
			//
			// This example add the GooglePlayGames pod, version 5.0 or greater, disabling bitcode generation.

			Google.VersionHandler.InvokeStaticMethod(
				iosResolver, "AddPod",
				new object[] { "UrbanAirship-iOS-SDK" },
				namedArgs: new Dictionary<string, object>() {
					{ "version", PluginInfo.IOSAirsipVersion },
				});
		}

		// Handle delayed loading of the dependency resolvers.
		private static void OnPostprocessAllAssets(
			string[] importedAssets, string[] deletedAssets,
			string[] movedAssets, string[] movedFromPath) {
			foreach (string asset in importedAssets) {
				if (asset.Contains("IOSResolver") ||
					asset.Contains("JarResolver")) {
					RegisterDependencies();
					break;
				}
			}
		}
	}
}
