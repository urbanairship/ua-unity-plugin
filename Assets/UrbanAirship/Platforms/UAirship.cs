/*
 Copyright 2015 Urban Airship and Contributors
*/

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UrbanAirship {

	public class UAirship
	{
		private static UrbanAirshipListener listener;
		private static IUAirshipPlugin plugin = null;

		public delegate void PushReceivedEventHandler(PushMessage message);
		public static event PushReceivedEventHandler OnPushReceived;

		static UAirship() {
			if (Application.isEditor) {
				plugin = new StubbedPlugin ();
			} else {
				#if UNITY_ANDROID
				plugin = new UAirshipPluginAndroid ();
				#elif UNITY_IPHONE
				plugin = new UAirshipPluginIOS ();
				#else
				plugin = new StubbedPlugin ();
				#endif
			}

			GameObject gameObject = new GameObject("[UrbanAirshipListener]");
			listener = gameObject.AddComponent<UrbanAirshipListener>();
			MonoBehaviour.DontDestroyOnLoad(gameObject);
			plugin.Listener = gameObject;
		}

		public static bool UserNotificationsEnabled {
			get {
				return plugin.UserNotificationsEnabled;
			}
			set {
				plugin.UserNotificationsEnabled = value;
			}
		}

		public static IEnumerable<string> Tags {
			get {
				string tagsAsJson = plugin.Tags;
				JsonArray<string> jsonArray = JsonArray<string>.FromJson (tagsAsJson);
				return jsonArray.AsEnumerable ();
			}
		}

		public static string Alias {
			get {
				return plugin.Alias;
			}
			set {
				plugin.Alias = value;
			}
		}

		public static string ChannelId {
			get {
				return plugin.ChannelId;
			}
		}

		public static bool LocationEnabled {
			get {
				return plugin.LocationEnabled;
			}
			set {
				plugin.LocationEnabled = value;
			}
		}

		public static bool BackgroundLocationAllowed {
			get {
				return plugin.BackgroundLocationAllowed;
			}
			set {
				plugin.BackgroundLocationAllowed = value;
			}
		}

		public static string NamedUserId {
			get {
				return plugin.NamedUserId;
			}
			set {
				plugin.NamedUserId = value;
			}
		}

		public static string GetDeepLink (bool clear = true)
		{
			return plugin.GetDeepLink (clear);
		}

		public static PushMessage GetIncomingPush (bool clear = true)
		{
			string jsonPushMessage = plugin.GetIncomingPush (clear);
			if (String.IsNullOrEmpty(jsonPushMessage)) {
				return null;
			}

			PushMessage pushMessage = PushMessage.FromJson (jsonPushMessage);
			return pushMessage;
		}

		public static void AddTag (string tag)
		{
			plugin.AddTag (tag);
		}

		public static void RemoveTag (string tag)
		{
			plugin.RemoveTag (tag);
		}

		public static void AddCustomEvent (CustomEvent customEvent)
		{
			plugin.AddCustomEvent (customEvent.ToJson ());
		}

		public static void DisplayMessageCenter ()
		{
			plugin.DisplayMessageCenter ();
		}

		public static TagGroupEditor EditNamedUserTagGroups ()
		{
			return new TagGroupEditor ((string payload) => {
				plugin.EditNamedUserTagGroups(payload);
			});
		}

		public static TagGroupEditor EditChannelTagGroups ()
		{
			return new TagGroupEditor ((string payload) => {
				plugin.EditChannelTagGroups(payload);
			});
		}

		class UrbanAirshipListener : MonoBehaviour {
			void OnPushReceived(string payload) {
				PushReceivedEventHandler handler = UAirship.OnPushReceived;

				if (handler == null) {
					return;
				}

				PushMessage pushMessage = PushMessage.FromJson (payload);
				if (pushMessage != null) {
					handler(pushMessage);
				}
			}
		}
	}
}

