/*
 Copyright 2016 Urban Airship and Contributors
*/

#if UNITY_ANDROID

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace UrbanAirship {

	class UAirshipPluginAndroid : IUAirshipPlugin
	{

		private AndroidJavaObject androidPlugin;

		public UAirshipPluginAndroid ()
		{
			try {
				using (AndroidJavaClass pluginClass = new AndroidJavaClass ("com.urbanairship.unityplugin.UnityPlugin")) {
					androidPlugin = pluginClass.CallStatic<AndroidJavaObject> ("shared");
				}
			} catch (Exception) {
				Debug.LogError ("UAirship plugin not found");
			}
		}

		public bool UserNotificationsEnabled {
			get {
				return Call<bool> ("getUserNotificationsEnabled");
			}
			set {
				Call ("setUserNotificationsEnabled",  value);
			}
		}

		public string Tags {
			get {
				return Call<string> ("getTags");
			}
		}

		public string Alias {
			get {
				return Call<string> ("getAlias");
			}
			set {
				Call ("setAlias", value);
			}
		}

		public string ChannelId {
			get {
				return Call<string> ("getChannelId");
			}
		}

		public bool LocationEnabled {
			get {
				return Call<bool> ("isLocationEnabled");
			}
			set {
				Call ("setLocationEnabled", value);
			}
		}

		public bool BackgroundLocationAllowed {
			get {
				return Call<bool> ("isBackgroundLocationAllowed");
			}
			set {
				Call ("setBackgroundLocationAllowed", value);
			}
		}

		public string NamedUserId {
			get {
				return Call<string> ("getNamedUserId");
			}

			set {
				Call ("setNamedUserId", value);
			}
		}

		public GameObject Listener {
			set {
				Call ("setListener", value.name);
			}
		}

		public string GetIncomingPush (bool clear)
		{
			return Call<string> ("getIncomingPush", clear);
		}

		public string GetDeepLink (bool clear)
		{
			return Call<string> ("getDeepLink", clear);
		}

		public void AddTag (string tag)
		{
			Call ("addTag", tag);
		}

		public void RemoveTag (string tag)
		{
			Call ("removeTag", tag);
		}

		public void AddCustomEvent (string customEvent)
		{
			Call ("addCustomEvent", customEvent);
		}

		public void AssociateIdentifier (string key, string identifier)
		{
			Call ("associateIdentifier", key, identifier);
		}

		public void DisplayMessageCenter ()
		{
			Call ("displayMessageCenter");
		}

		public void EditNamedUserTagGroups (string payload)
		{
			Call ("editNamedUserTagGroups", payload);
		}

		public void EditChannelTagGroups (string payload)
		{
			Call ("editChannelTagGroups", payload);
		}

		private void Call (string method, params object[] args)
		{
			if (androidPlugin != null) {
				androidPlugin.Call (method, args);
			}
		}

		private T Call<T> (string method, params object[] args)
		{
			if (androidPlugin != null) {
				return androidPlugin.Call<T> (method, args);
			}
			return default (T);
		}
	}
}

#endif
