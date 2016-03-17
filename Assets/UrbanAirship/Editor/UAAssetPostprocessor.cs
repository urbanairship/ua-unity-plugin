/*
 Copyright 2016 Urban Airship and Contributors
*/

using System;
using UnityEditor;
using System.Linq;
using UnityEngine;
using System.IO;

namespace UrbanAirship.Editor
{
	[InitializeOnLoad]
	public class UAAssetPostprocessor : AssetPostprocessor
	{
		static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			if (importedAssets.Select (s => s.ToLower ()).Any (s => s.Contains ("urbanairship") || s.Contains ("airshipconfig") || s.Contains ("airship_config"))) {
				UAUtils.UpdateManifests ();
				UnityEngine.Debug.Log ("Updated Urban Airship Manifests.");
				AssetDatabase.Refresh ();
			}

			if (deletedAssets.Select (s => s.ToLower ()).Any (s => s.Contains ("airship") || s.Contains ("airship_config"))) {
				if (UAConfig.LoadConfig ().Apply ()) {
					UnityEngine.Debug.Log ("Created Urban Airship config.");
					AssetDatabase.Refresh ();
				}
			}
		}


		[MenuItem ("Window/Urban Airship/Update Android Manifests")]
		public static void Update ()
		{
			UAUtils.UpdateManifests ();
			AssetDatabase.Refresh ();
			EditorUtility.DisplayDialog ("Urban Airship", "Urban Airship Android Manifests updated with the current bundle ID.", "OK");
		}
	}
}
