/*
 Copyright 2016 Urban Airship and Contributors
*/

#if UNITY_ANDROID

using System;
using UnityEditor;
using System.Linq;
using UnityEngine;
using System.IO;

namespace UrbanAirship
{
	[InitializeOnLoad]
	public class UAAndroidAssetPostprocessor : AssetPostprocessor 
	{
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			if (importedAssets.Any(s => s.Contains("urbanairship")))
			{
				UpdateManifests();
			}
		}

		[MenuItem("Assets/Urban Airship/Update Manifests")]
		public static void Update()
		{
			UpdateManifests();

			EditorUtility.DisplayDialog("Urban Airship", "Manifests updated", "OK");
		}

		private static void UpdateManifests()
		{
			string[] libs = {
				Path.Combine(Application.dataPath, "Plugins/Android/urbanairship-plugin-lib"),
				Path.Combine(Application.dataPath, "Plugins/Android/urbanairship-sdk")
			};

			foreach (string lib in libs)
			{
				if (!Directory.Exists(lib))
				{
					UnityEngine.Debug.Log("Urban Airship library missing: " + lib);
					continue;
				}

				string original = Path.Combine(lib, "Orig_AndroidManifest.xml");
				string manifest = Path.Combine(lib, "AndroidManifest.xml");

				if (File.Exists(manifest))
				{
					File.Delete(manifest);
				}

				File.Copy(original, manifest);

				StreamReader sr = new StreamReader(manifest);
				string body = sr.ReadToEnd();
				sr.Close();

				body = body.Replace("${applicationId}", PlayerSettings.bundleIdentifier);

				using (var wr = new StreamWriter(manifest, false))
				{
					wr.Write(body);
				}
			}

			AssetDatabase.Refresh();

		}
	}
}

#endif
