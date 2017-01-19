/*
 Copyright 2017 Urban Airship and Contributors
*/

#if UNITY_ANDROID

using UnityEditor;
using System;
using System.Collections.Generic;

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

      private static readonly string GCMVersion = "9.8.0";
      private static readonly string SupportV4Version = "25.0.1";

      static UADependencies()
      {
         RegisterDependencies();
      }

      public static void RegisterDependencies()
      {
         Type playServicesSupport = Google.VersionHandler.FindClass("Google.JarResolver", "Google.JarResolver.PlayServicesSupport");
         if (playServicesSupport == null)
         {
            return;
         }

         object svcSupport = Google.VersionHandler.InvokeStaticMethod(playServicesSupport, "CreateInstance",
            new object[] { PluginName, EditorPrefs.GetString("AndroidSdkRoot"), "ProjectSettings" });

         Google.VersionHandler.InvokeInstanceMethod(
            svcSupport, "DependOn",
            new object[] {
               "com.google.android.gms",
               "play-services-gcm",
               GCMVersion },
            namedArgs: new Dictionary<string, object>() {
               {"packageIds", new string[] { "extra-google-m2repository" } }
            });

         Google.VersionHandler.InvokeInstanceMethod(
            svcSupport, "DependOn",
            new object[] {
               "com.android.support",
               "support-v4",
               SupportV4Version });
      }

      private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
      {
         foreach (string asset in importedAssets)
         {
            if (asset.Contains("IOSResolver") || asset.Contains("JarResolver"))
            {
               RegisterDependencies();
               break;
            }
         }
      }
   }
}

#endif
