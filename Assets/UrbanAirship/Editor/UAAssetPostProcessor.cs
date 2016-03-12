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
			if (importedAssets.Any(s => s.Contains("urbanairship")) || deletedAssets.Any(s => s.ToLower().Contains("airshipconfig")))
			{

				try
				{
					UAUtils.UpdateManifests();
					UAConfig.Instance.Apply();
					AssetDatabase.Refresh();			
				}
				catch (Exception e)
				{
					// ignore
				}

				AssetDatabase.Refresh();
			}
		}
	}
}
