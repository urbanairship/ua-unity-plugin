/*
 Copyright 2016 Urban Airship and Contributors
*/

using System;
using UnityEditor;
using System.Linq;
using UnityEngine;
using System.IO;

namespace UrbanAirship
{
	[InitializeOnLoad]
	public class UAAssetPostprocessor : AssetPostprocessor 
	{
		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			if (importedAssets.Any(s => s.Contains("urbanairship") || s.ToLower().Contains("airshipconfig")))
			{
				UAUtils.UpdateManifests();
				UnityEngine.Debug.Log ("Updated Urban Airship Manifests.");
				AssetDatabase.Refresh();
			}

			if(deletedAssets.Any(s => s.ToLower().Contains("airshipconfig")))
			{
				if (UAConfig.LoadConfig().Apply())
				{
					UnityEngine.Debug.Log ("Created Urban Airship config.");
				}
			}
		}


		[MenuItem("Window/Urban Airship/Update Android Manifests")]
		public static void Update()
		{
			UAUtils.UpdateManifests();
			AssetDatabase.Refresh();
			EditorUtility.DisplayDialog("Urban Airship", "Urban Airship Android Manifests updated with the current bundle ID.", "OK");
		}
	}
}
