/*
 Copyright 2015 Urban Airship and Contributors
*/

#if UNITY_ANDROID

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace UrbanAirship {

	public class UAirshipPluginAndroid : IUAirshipPlugin
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

		public bool PushEnabled {
			get {
				return Call<bool> ("isPushEnabled");
			}
			set {
				if (value) {
					Call ("enablePush");
				} else {
					Call ("disablePush");
				}
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
				if (value) {
					Call ("enableLocation");
				} else {
					Call ("disableLocation");
				}
			}
		}

		public bool BackgroundLocationEnabled {
			get {
				return Call<bool> ("isBackgroundLocationEnabled");
			}
			set {
				if (value) {
					Call ("enableBackgroundLocation");
				} else {
					Call ("disableBackgroundLocation");
				}
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

		public string GetDeepLink (bool clear)
		{
			return Call<string> ("getDeepLink", clear);
		}


		public string GetIncomingPush (bool clear)
		{
			return Call<string> ("getIncomingPush", clear);
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
