/*
 Copyright 2016 Urban Airship and Contributors
*/

#if UNITY_ANDROID

using Google.JarResolver;
using UnityEditor;
using GooglePlayServices;

namespace UrbanAirship.Editor
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
				EditorPrefs.GetString ("AndroidSdkRoot"),
				"ProjectSettings");

			svcSupport.DependOn ("com.google.android.gms", "play-services-gcm", "8.4+");
			svcSupport.DependOn ("com.android.support", "support-v4", "23.1+");


			// Resolve dependency on load. Only resolve the dependency if we are using the ResolverVer1_1
			// and automatic resolution is enabled.
			ResolverVer1_1 resolverv1_1 = new ResolverVer1_1 ();
			IResolver resolver = PlayServicesResolver.RegisterResolver (resolverv1_1);
			if (resolver != null &&  resolver.Version() == resolverv1_1.Version() && resolver.AutomaticResolutionEnabled ()) {
				resolver.DoResolution (svcSupport, "Assets/Plugins/Android", HandleOverwriteConfirmation);
				AssetDatabase.Refresh ();
			}
		}

		static bool HandleOverwriteConfirmation(Dependency oldDep, Dependency newDep)
		{
			return (oldDep.BestVersion == newDep.BestVersion);
		}
	}
}

#endif
