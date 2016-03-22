/*
 Copyright 2015 Urban Airship and Contributors
*/

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UrbanAirship {

	[System.Serializable]
	class JsonArray<T>
	{
		public T[] values = null;


		public static JsonArray<T> FromJson (T jsonString)
		{
			string wrappedArray = string.Format ("{{ \"{0}\": {1}}}", "values", jsonString);
			return JsonUtility.FromJson<JsonArray<T>> (wrappedArray);
		}

		public IEnumerable<T> AsEnumerable () 
		{
			if (this.values == null) {
				return new T[0].AsEnumerable ();
			} else {
				return this.values.AsEnumerable ();
			}
		}

		public string ToJson ()
		{
			return JsonUtility.ToJson (this);
		}

	}

	[System.Serializable]
	public class CustomEvent
	{
		[SerializeField]
		private string eventName;
		[SerializeField]
		private string eventValue;
		[SerializeField]
		private string transactionId;
		[SerializeField]
		private string interactionType;
		[SerializeField]
		private string interactionId;
		[SerializeField]
		private Property[] properties;

		private List<Property> propertyList;


		public CustomEvent()
		{
			this.propertyList = new List<Property> ();
		}

		public string EventName {
			get { return eventName; }
			set { eventName = value; }
		}
		public decimal EventValue {
			get { return Decimal.Parse (eventValue); }
			set { eventValue = value.ToString (); }
		}

		public string TransactionId {
			get { return transactionId; }
			set { transactionId = value; }
		}

		public string InteractionType {
			get { return interactionType; }
			set { interactionType = value; }
		}

		public string InteractionId {
			get { return interactionId; }
			set { interactionId = value; }
		}

		public void AddProperty(string name, string value)
		{
			this.propertyList.Add (new Property ("s", name, value));
		}

		public void AddProperty(string name, double value)
		{
			this.propertyList.Add (new Property ("d", name, value));
		}

		public void AddProperty(string name, bool value)
		{
			this.propertyList.Add (new Property ("b", name, value));
		}

		public void AddProperty(string name, ICollection<string> value)
		{
			this.propertyList.Add (new Property ("sa", name, value));
		}

		public string ToJson ()
		{
			this.properties = this.propertyList.ToArray ();
			return JsonUtility.ToJson (this);
		}

		[Serializable]
		class Property
		{
			public string type;
			public string name;
			public string stringValue;
			public double doubleValue;
			public bool boolValue;
			public string[] stringArrayValue;

			public Property(string type, string name, System.Object value)
			{
				this.type = type;
				this.name = name;

				if (type == "s") {
					this.stringValue = (string) value;
				} else if (type == "d") {
					this.doubleValue = (double) value;
				} else if (type == "b") {
					this.boolValue = (bool) value;
				} else if (type == "sa") {
					ICollection<string> collection = (ICollection<string>) value;
					this.stringArrayValue = collection.ToArray ();
				}
			}
		}
	}

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

		public static bool BackgroundLocationEnabled {
			get {
				return plugin.BackgroundLocationEnabled;
			}
			set {
				plugin.BackgroundLocationEnabled = value;
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

		public static void addCustomEvent (CustomEvent customEvent)
		{
			if (plugin != null) {
				plugin.AddCustomEvent (customEvent.ToJson ());
			}
		}

		public static void DisplayMessageCenter ()
		{
			if (plugin != null) {
				plugin.DisplayMessageCenter ();
			}
		}

		public static TagGroupEditor EditNamedUserTagGroups ()
		{
			return new TagGroupEditor ((string payload) => {
				if (plugin != null) {
					plugin.EditNamedUserTagGroups(payload);
				}
			});
		}

		public static TagGroupEditor EditChannelTagGroups ()
		{
			return new TagGroupEditor ((string payload) => {
				if (plugin != null) {
					plugin.EditChannelTagGroups(payload);
				}
			});
		}
	}
}

