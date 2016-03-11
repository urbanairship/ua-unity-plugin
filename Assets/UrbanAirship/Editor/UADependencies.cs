/*
 Copyright 2016 Urban Airship and Contributors
*/

#if UNITY_ANDROID

using Google.JarResolver;
using UnityEditor;
using GooglePlayServices;


namespace UrbanAirship
{

	[InitializeOnLoad]
	public static class UADependencies
	{
		/// <summary>
		/// The name of your plugin.  This is used to create a settings file
		/// which contains the dependencies specific to your plugin.
		/// </summary>
		private static readonly string PluginName = "UrbanAirship";

		/// <summary>
		/// Initializes static members of the <see cref="SampleDependencies"/> class.
		/// </summary>
		static UADependencies()
		{
			PlayServicesSupport svcSupport = PlayServicesSupport.CreateInstance(
				PluginName,
				EditorPrefs.GetString("AndroidSdkRoot"),
				"ProjectSettings");

			svcSupport.DependOn("com.google.android.gms", "play-services-gcm", "8.4+");
			svcSupport.DependOn("com.android.support", "support-v4", "23.1+");
		}
	}
}

#endif
