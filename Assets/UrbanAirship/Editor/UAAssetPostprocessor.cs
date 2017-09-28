/*
 Copyright 2017 Urban Airship and Contributors
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
			if (deletedAssets.Select (s => s.ToLower ()).Any (s => s.Contains ("airship") || s.Contains ("airship_config"))) {
				if (UAConfig.LoadConfig ().Apply ()) {
					UnityEngine.Debug.Log ("Created Urban Airship config.");
					AssetDatabase.Refresh ();
				}
			}
		}
	}
}
