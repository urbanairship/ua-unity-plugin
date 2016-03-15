/*
 Copyright 2015 Urban Airship and Contributors
*/

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace UrbanAirship {

	public class UAirship
	{

		#if UNITY_ANDROID
			private static IUAirshipPlugin plugin = new UAirshipPluginAndroid ();
		#elif UNITY_IPHONE
			private static IUAirshipPlugin plugin = new UAirshipPluginIOS();
		#else
			private static IUAirshipPlugin plugin = null;
		#endif

		public static bool PushEnabled {
			get {
				return plugin.PushEnabled;
			}
			set {
				plugin.PushEnabled = value;
			}
		}

		// TODO: Decode the json array of tags
		public static string Tags {
			get {
				return plugin.Tags;
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

		public static bool BackgroundLocationEnabled {
			get {
				return plugin.BackgroundLocationEnabled;
			}
			set {
				plugin.BackgroundLocationEnabled = value;
			}
		}

		public static void AddListener (GameObject gameObject)
		{
			if (plugin != null) {
				plugin.AddListener (gameObject);
			}
		}

		public static void RemoveListener (GameObject gameObject)
		{
			if (plugin != null) {
				plugin.RemoveListener (gameObject);
			}
		}

		public static string GetDeepLink (bool clear = true)
		{
			if (plugin != null) {
				return plugin.GetDeepLink (clear);
			}
			return null;
		}

		public static string GetIncomingPush (bool clear = true)
		{
			if (plugin != null) {
				return plugin.GetIncomingPush (clear);
			}
			return null;
		}

		public static void AddTag (string tag)
		{
			if (plugin != null) {
				plugin.AddTag (tag);
			}
		}

		public static void RemoveTag (string tag)
		{
			if (plugin != null) {
				plugin.RemoveTag (tag);
			}
		}
	}
}

