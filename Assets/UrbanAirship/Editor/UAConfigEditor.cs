/*
 Copyright 2016 Urban Airship and Contributors
*/

using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace UrbanAirship.Editor
{
	[InitializeOnLoad]
	public class UAConfigEditor : EditorWindow
	{
		private UAConfig config;

		void OnEnable ()
		{
			config = UAConfig.LoadConfig ();
		}

		void OnGUI ()
		{

			CreateSection ("Production", () => {
				config.ProductionAppKey = EditorGUILayout.TextField ("App Key", config.ProductionAppKey);
				config.ProductionAppSecret = EditorGUILayout.TextField ("App Secret", config.ProductionAppSecret);
				config.ProductionLogLevel = (UAConfig.LogLevel) EditorGUILayout.EnumPopup("Log level:", config.ProductionLogLevel);
			});


			CreateSection ("Development", () => {
				config.DevelopmentAppKey = EditorGUILayout.TextField ("App Key", config.DevelopmentAppKey);
				config.DevelopmentAppSecret = EditorGUILayout.TextField ("App Secret", config.DevelopmentAppSecret);
				config.DevelopmentLogLevel = (UAConfig.LogLevel) EditorGUILayout.EnumPopup("Log level:", config.DevelopmentLogLevel);
			});

			CreateSection ("Android", () => {
				config.GCMSenderId = EditorGUILayout.TextField ("GCM Sender ID", config.GCMSenderId);
				config.AndroidNotificationAccentColor = EditorGUILayout.TextField ("Notification Accent Color", config.AndroidNotificationAccentColor);
				config.AndroidNotificationIcon = EditorGUILayout.TextField ("Notification Icon", config.AndroidNotificationIcon);

				GUILayout.Space (5);

				GUILayout.Label ("Notification icon must be a reference to a drawable in the project, e.g., " +
				"@drawable/app_icon, @android:drawable/ic_dialog_alert. Drawables can be added " +
				"in either the Assets/Plugins/Android/urbanairship-plugin-lib/res directory or by " +
				"providing a new Android library project.", EditorStyles.wordWrappedMiniLabel);
			});


			config.InProduction = EditorGUILayout.Toggle ("inProduction", config.InProduction);

			GUILayout.FlexibleSpace ();

			GUILayout.BeginHorizontal ();

			if (GUILayout.Button ("Cancel")) {
				Close ();
			}

			GUILayout.FlexibleSpace ();

			if (GUILayout.Button ("Save")) {
				try {
					config.Validate ();

					UnityEngine.Debug.Log ("Saving Urban Airship config.");

					config.Apply ();
					UAConfig.SaveConfig (config);

					AssetDatabase.Refresh ();
					Close ();
				} catch (Exception e) {
					EditorUtility.DisplayDialog ("Urban Airship", "Unable to save config. Erorr: " + e.Message, "Ok");
				}
			}

			GUILayout.FlexibleSpace ();

		}

		private void CreateSection (string sectionTitle, Action body)
		{
			GUILayout.Label (sectionTitle, EditorStyles.boldLabel);
			GUILayout.BeginHorizontal ();
			GUILayout.Space (15);
			GUILayout.BeginVertical ();

			body ();

			GUILayout.EndVertical ();
			GUILayout.Space (5);
			GUILayout.EndHorizontal ();
			GUILayout.Space (15);
		}
	}
}

