/*
 Copyright 2016 Urban Airship and Contributors
*/

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UrbanAirship {

	/// <summary>
	/// The primary manager class for the Urban Airship plugin.
	/// </summary>
	public class UAirship
	{
		private static UrbanAirshipListener listener;
		private static IUAirshipPlugin plugin = null;

		/// <summary>
		/// Push received event handler.
		/// </summary>
		public delegate void PushReceivedEventHandler(PushMessage message);

		/// <summary>
		/// Occurs when a push is received.
		/// </summary>
		public static event PushReceivedEventHandler OnPushReceived;

		/// <summary>
		/// Channel update event handler.
		/// </summary>
		public delegate void ChannelUpdateEventHandler(string channelId);

		/// <summary>
		/// Occurs when the channel updates.
		/// </summary>
		public static event ChannelUpdateEventHandler OnChannelUpdated;

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

		/// <summary>
		/// Determinines whether user notifications are enabled.
		/// </summary>
		/// <value><c>true</c> if user notifications are enabled; otherwise, <c>false</c>.</value>
		public static bool UserNotificationsEnabled {
			get {
				return plugin.UserNotificationsEnabled;
			}
			set {
				plugin.UserNotificationsEnabled = value;
			}
		}

		/// <summary>
		/// Gets the tags currently set for the device.
		/// </summary>
		/// <value>The tags.</value>
		public static IEnumerable<string> Tags {
			get {
				string tagsAsJson = plugin.Tags;
				JsonArray<string> jsonArray = JsonArray<string>.FromJson (tagsAsJson);
				return jsonArray.AsEnumerable ();
			}
		}

		/// <summary>
		/// Gets or sets the alias for the device.
		/// </summary>
		/// <value>The alias.</value>
		public static string Alias {
			get {
				return plugin.Alias;
			}
			set {
				plugin.Alias = value;
			}
		}

		/// <summary>
		/// Gets the channel identifier associated with the device.
		/// </summary>
		/// <value>The channel identifier.</value>
		public static string ChannelId {
			get {
				return plugin.ChannelId;
			}
		}

		/// <summary>
		/// Determines whether location is enabled.
		/// </summary>
		/// <value><c>true</c> if location is enabled; otherwise, <c>false</c>.</value>
		public static bool LocationEnabled {
			get {
				return plugin.LocationEnabled;
			}
			set {
				plugin.LocationEnabled = value;
			}
		}

		/// <summary>
		/// Determine whether background location is allowed.
		/// </summary>
		/// <value><c>true</c> if background location is allowed; otherwise, <c>false</c>.</value>
		public static bool BackgroundLocationAllowed {
			get {
				return plugin.BackgroundLocationAllowed;
			}
			set {
				plugin.BackgroundLocationAllowed = value;
			}
		}

		/// <summary>
		/// Gets or sets the named user identifier.
		/// </summary>
		/// <value>The named user identifier.</value>
		public static string NamedUserId {
			get {
				return plugin.NamedUserId;
			}
			set {
				plugin.NamedUserId = value;
			}
		}

		/// <summary>
		/// Gets the last received deep link.
		/// </summary>
		/// <returns>The deep link.</returns>
		/// <param name="clear">If set to <c>true</c> clear the stored deep link after accessing it.</param>
		public static string GetDeepLink (bool clear = true)
		{
			return plugin.GetDeepLink (clear);
		}

		/// <summary>
		/// Gets the last stored incoming push message.
		/// </summary>
		/// <returns>The push message.</returns>
		/// <param name="clear">If set to <c>true</c> clear the stored push message after accessing it.</param>
		public static PushMessage GetIncomingPush (bool clear = true)
		{
			string jsonPushMessage = plugin.GetIncomingPush (clear);
			if (String.IsNullOrEmpty(jsonPushMessage)) {
				return null;
			}

			PushMessage pushMessage = PushMessage.FromJson (jsonPushMessage);
			return pushMessage;
		}

		/// <summary>
		/// Adds the provided device tag.
		/// </summary>
		/// <param name="tag">The tag.</param>
		public static void AddTag (string tag)
		{
			plugin.AddTag (tag);
		}

		/// <summary>
		/// Removes the provided device tag.
		/// </summary>
		/// <param name="tag">The tag.</param>
		public static void RemoveTag (string tag)
		{
			plugin.RemoveTag (tag);
		}

		/// <summary>
		/// Adds a custom event.
		/// </summary>
		/// <param name="customEvent">The custom event.</param>
		public static void AddCustomEvent (CustomEvent customEvent)
		{
			plugin.AddCustomEvent (customEvent.ToJson ());
		}

		/// <summary>
		/// Displays the message center.
		/// </summary>
		public static void DisplayMessageCenter ()
		{
			plugin.DisplayMessageCenter ();
		}

		/// <summary>
		/// Returns an editor for named user tag groups.
		/// </summary>
		/// <returns>A TagGroupEditor for named user tag groups.</returns>
		public static TagGroupEditor EditNamedUserTagGroups ()
		{
			return new TagGroupEditor ((string payload) => {
				plugin.EditNamedUserTagGroups(payload);
			});
		}

		/// <summary>
		/// Returns an editor for channel tag groups.
		/// </summary>
		/// <returns>A TagGroupEditor for channel tag groups.</returns>
		public static TagGroupEditor EditChannelTagGroups ()
		{
			return new TagGroupEditor ((string payload) => {
				plugin.EditChannelTagGroups(payload);
			});
		}

		class UrbanAirshipListener : MonoBehaviour {
			void OnPushReceived (string payload) {
				PushReceivedEventHandler handler = UAirship.OnPushReceived;

				if (handler == null) {
					return;
				}

				PushMessage pushMessage = PushMessage.FromJson (payload);
				if (pushMessage != null) {
					handler (pushMessage);
				}
			}

			void OnChannelUpdated (string channelId) {
				ChannelUpdateEventHandler handler = UAirship.OnChannelUpdated;

				if (handler != null) {
					handler (channelId);
				}
			}
		}
	}
}

