/*
 Copyright 2015 Urban Airship and Contributors
*/

#if UNITY_IPHONE

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UrbanAirship {

	class UAirshipPluginIOS : IUAirshipPlugin
	{

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_setListener (string listener);
		[DllImport ("__Internal")]
		private static extern string UAUnityPlugin_getDeepLink (bool clear);

		[DllImport ("__Internal")]
		private static extern  string UAUnityPlugin_getIncomingPush (bool clear);

		[DllImport ("__Internal")]
		private static extern bool UAUnityPlugin_getUserNotificationsEnabled ();

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_setUserNotificationsEnabled (bool enabled);

		[DllImport ("__Internal")]
		private static extern string UAUnityPlugin_getTags ();

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_addTag (string tag);

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_removeTag (string tag);

		[DllImport ("__Internal")]
		private static extern string UAUnityPlugin_getAlias ();

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_setAlias (string alias);

		[DllImport ("__Internal")]
		private static extern string UAUnityPlugin_getChannelId ();

		//Location Function Imports
		[DllImport ("__Internal")]
		private static extern bool UAUnityPlugin_isLocationEnabled ();

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_setLocationEnabled (bool enabled);

		[DllImport ("__Internal")]
		private static extern bool UAUnityPlugin_isBackgroundLocationAllowed ();

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_setBackgroundLocationAllowed (bool allowed);

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_addCustomEvent (string customEvent);

		[DllImport ("__Internal")]
		private static extern string UAUnityPlugin_getNamedUserID ();

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_setNamedUserID (string namedUserID);

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_displayMessageCenter ();

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_editNamedUserTagGroups (string payload);

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_editChannelTagGroups (string payload);

		public bool UserNotificationsEnabled {
			get {
				return UAUnityPlugin_getUserNotificationsEnabled ();
			}
			set {
				UAUnityPlugin_setUserNotificationsEnabled (value);
			}
		}

		public string Tags {
			get {
				return UAUnityPlugin_getTags ();
			}
		}

		public string Alias {
			get {
				return UAUnityPlugin_getAlias ();
			}
			set {
				UAUnityPlugin_setAlias (value);
			}
		}

		public string ChannelId {
			get {
				return UAUnityPlugin_getChannelId ();
			}
		}

		public bool LocationEnabled {
			get {
				return UAUnityPlugin_isLocationEnabled ();
			}
			set {
				UAUnityPlugin_setLocationEnabled (value);
			}
		}

		public bool BackgroundLocationAllowed {
			get {
				return UAUnityPlugin_isBackgroundLocationAllowed ();
			}
			set {
				UAUnityPlugin_setBackgroundLocationAllowed (value);
			}
		}

		public string NamedUserId {
			get {
				return UAUnityPlugin_getNamedUserID ();
			}

			set {
				UAUnityPlugin_setNamedUserID (value);
			}
		}

		public GameObject Listener {
			set {
				UAUnityPlugin_setListener (value.name);
			}
		}

		public string GetDeepLink (bool clear)
		{
			return UAUnityPlugin_getDeepLink (clear);
		}

		public string GetIncomingPush (bool clear)
		{
			return UAUnityPlugin_getIncomingPush (clear);
		}

		public void AddTag (string tag)
		{
			UAUnityPlugin_addTag (tag);
		}

		public void RemoveTag (string tag)
		{
			UAUnityPlugin_removeTag (tag);
		}

		public void AddCustomEvent (string customEvent)
		{
			UAUnityPlugin_addCustomEvent (customEvent);
		}

		public void DisplayMessageCenter ()
		{
			UAUnityPlugin_displayMessageCenter ();
		}

		public void EditNamedUserTagGroups (string payload)
		{
			UAUnityPlugin_editNamedUserTagGroups (payload);
		}

		public void EditChannelTagGroups (string payload)
		{
			UAUnityPlugin_editChannelTagGroups (payload);
		}
	}
}

#endif

	