/* Copyright Airship and Contributors */

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace AirshipSDK.Editor {
    [InitializeOnLoad]
    public class AirshipConfigEditor : EditorWindow {
        private AirshipConfig config;

        void OnEnable () {
            config = AirshipConfig.LoadConfig ();
            config.Apply ();
        }

        void OnGUI () {
            minSize = new Vector2 (800, 640);

            CreateSection ("Production", () => {
                config.ProductionAppKey = EditorGUILayout.TextField ("App Key", config.ProductionAppKey);
                config.ProductionAppSecret = EditorGUILayout.TextField ("App Secret", config.ProductionAppSecret);
                config.ProductionLogLevel = (AirshipConfig.LogLevel) EditorGUILayout.EnumPopup ("Log Level", config.ProductionLogLevel);
            });

            CreateSection ("Development", () => {
                config.DevelopmentAppKey = EditorGUILayout.TextField ("App Key", config.DevelopmentAppKey);
                config.DevelopmentAppSecret = EditorGUILayout.TextField ("App Secret", config.DevelopmentAppSecret);
                config.DevelopmentLogLevel = (AirshipConfig.LogLevel) EditorGUILayout.EnumPopup ("Log Level", config.DevelopmentLogLevel);
            });

            CreateSection ("Common", () => {
                config.InProduction = EditorGUILayout.Toggle ("inProduction", config.InProduction);
                config.Site = (AirshipConfig.CloudSite) EditorGUILayout.EnumPopup ("Cloud Site", config.Site);
            });

            CreateSection ("URL Allow List", () => {
                config.UrlAllowList = EditorGUILayout.TextField ("Scope All", config.UrlAllowList);
                config.UrlAllowListScopeOpenURL = EditorGUILayout.TextField ("Scope Open", config.UrlAllowListScopeOpenURL);
                config.UrlAllowListScopeJavaScriptInterface = EditorGUILayout.TextField ("Scope JS Interface", config.UrlAllowListScopeJavaScriptInterface);
            });

            CreateSection ("Data Collection", () => {
                config.EnabledFeatures = EditorGUILayout.TextField ("Enabled Features", config.EnabledFeatures);
            });

            CreateSection ("Android Settings", () => {
                config.GenerateGoogleJsonConfig = EditorGUILayout.Toggle ("Process google-service", config.GenerateGoogleJsonConfig);
                config.AndroidNotificationAccentColor = EditorGUILayout.TextField ("Notification Accent Color", config.AndroidNotificationAccentColor);
                config.AndroidNotificationIcon = EditorGUILayout.TextField ("Notification Icon", config.AndroidNotificationIcon);

                GUILayout.Space (5);

                GUILayout.Label ("Notification icon must be the name of a drawable in the project, e.g., " +
                    "app_icon, ic_dialog_alert. Drawables can be added " +
                    "in either the Assets/Plugins/Android/airship-resources.androidlib/res/drawable* directory or by " +
                    "providing a new Android library project.", EditorStyles.wordWrappedMiniLabel);
            });

            CreateSection ("iOS Foreground Presentation Options", () => {
                config.NotificationPresentationOptionAlert = EditorGUILayout.Toggle ("Alert", config.NotificationPresentationOptionAlert);
                config.NotificationPresentationOptionBadge = EditorGUILayout.Toggle ("Badge", config.NotificationPresentationOptionBadge);
                config.NotificationPresentationOptionSound = EditorGUILayout.Toggle ("Sound", config.NotificationPresentationOptionSound);
            });

            if (!config.IsConfigured) {
                EditorGUILayout.HelpBox (
                    "No app credentials configured. Config files will not be generated. " +
                    "You can configure Airship at runtime via Airship.Shared.TakeOff() instead.",
                    MessageType.Info);
            }

            GUILayout.FlexibleSpace ();

            GUILayout.BeginHorizontal ();

            if (GUILayout.Button ("Cancel")) {
                Close ();
            }

            GUILayout.FlexibleSpace ();

            if (GUILayout.Button ("Save")) {
                try {
                    config.Validate ();

                    UnityEngine.Debug.Log ("Saving Airship config.");

                    config.ApplyFirebaseConfig ();
                    config.Apply ();
                    AirshipConfig.SaveConfig (config);

                    AssetDatabase.Refresh ();
                    Close ();
                } catch (Exception e) {
                    EditorUtility.DisplayDialog ("Airship", "Unable to save config. Error: " + e.Message, "Ok");
                }
            }

            GUILayout.FlexibleSpace ();

        }

        private void CreateSection (string sectionTitle, Action body) {
            GUILayout.Label (sectionTitle, EditorStyles.boldLabel);
            GUILayout.BeginHorizontal ();
            GUILayout.Space (20);
            GUILayout.BeginVertical ();

            body ();

            GUILayout.EndVertical ();
            GUILayout.Space (5);
            GUILayout.EndHorizontal ();
            GUILayout.Space (20);
        }
    }
}
