/*
 Copyright 2015 Urban Airship and Contributors
*/

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UrbanAirship {

	public class UAirshipPluginIOS : IUAirshipPlugin
	{

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_addListener (string listener);

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_removeListener (string listener);


		[DllImport ("__Internal")]
		private static extern string UAUnityPlugin_getDeepLink (bool clear);


		// PUSH FUNCTION IMPORTS
		// import a single C-function from our plugin
		[DllImport ("__Internal")]
		private static extern  string UAUnityPlugin_getIncomingPush (bool clear);

		[DllImport ("__Internal")]
		private static extern bool UAUnityPlugin_isPushEnabled ();

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_enablePush ();

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_disablePush ();

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
		private static extern void UAUnityPlugin_enableLocation ();

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_disableLocation ();

		[DllImport ("__Internal")]
		private static extern bool UAUnityPlugin_isBackgroundLocationEnabled ();

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_enableBackgroundLocation ();

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_disableBackgroundLocation ();

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_addCustomEvent (string customEvent);

		[DllImport ("__Internal")]
		private static extern string UAUnityPlugin_getNamedUserID ();

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_setNamedUserID (string namedUserID);

		[DllImport ("__Internal")]
		private static extern void UAUnityPlugin_displayMessageCenter ();

		public bool PushEnabled {
			get {
				return UAUnityPlugin_isPushEnabled ();
			}
			set {
				if (value) {
					UAUnityPlugin_enablePush ();
				} else {
					UAUnityPlugin_disablePush ();
				}
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
				if (value) {
					UAUnityPlugin_enableLocation ();
				} else {
					UAUnityPlugin_disableLocation ();
				}
			}
		}

		public bool BackgroundLocationEnabled {
			get {
				return UAUnityPlugin_isBackgroundLocationEnabled ();
			}
			set {
				if (value) {
					UAUnityPlugin_enableBackgroundLocation ();
				} else {
					UAUnityPlugin_disableBackgroundLocation ();
				}
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

		public string GetDeepLink (bool clear)
		{
			return UAUnityPlugin_getDeepLink (clear);
		}

		public void AddListener (GameObject gameObject)
		{
			UAUnityPlugin_addListener (gameObject.name);
		}

		public void RemoveListener (GameObject gameObject)
		{
			UAUnityPlugin_removeListener (gameObject.name);
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
	}
}