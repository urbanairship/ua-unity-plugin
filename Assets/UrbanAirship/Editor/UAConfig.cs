/*
 Copyright 2017 Urban Airship and Contributors
*/

using System;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using UnityEditor.iOS.Xcode;
using UnityEditor;
using System.Xml;

namespace UrbanAirship.Editor
{
	[Serializable]
	public class UAConfig
	{

		public enum LogLevel
		{
			Verbose = 0,
			Debug = 1,
			Info = 2,
			Warn = 3,
			Error = 4,
			None = 5
		}

		private static readonly string filePath = "ProjectSettings/UrbanAirship.xml";
		private static UAConfig cachedInstance;

		[SerializeField]
		public string ProductionAppKey { get; set; }

		[SerializeField]
		public string ProductionAppSecret { get; set; }

		[SerializeField]
		public LogLevel ProductionLogLevel { get; set; }

		[SerializeField]
		public string DevelopmentAppKey { get; set; }

		[SerializeField]
		public string DevelopmentAppSecret { get; set; }

		[SerializeField]
		public LogLevel DevelopmentLogLevel { get; set; }

		[SerializeField]
		public string GCMSenderId { get; set; }

		[SerializeField]
		public bool NotificationPresentationOptionAlert { get; set; }

		[SerializeField]
		public bool NotificationPresentationOptionBadge { get; set; }

		[SerializeField]
		public bool NotificationPresentationOptionSound { get; set; }

		[SerializeField]
		public bool InProduction { get; set; }

		[SerializeField]
		public String AndroidNotificationIcon { get; set; }

		[SerializeField]
		public String AndroidNotificationAccentColor { get; set; }

		public bool IsValid {
			get {
				try {
					Validate ();
					return true;
				} catch (Exception) {
					return false;
				}
			}
		}

		public UAConfig ()
		{
			DevelopmentLogLevel = LogLevel.Debug;
			ProductionLogLevel = LogLevel.Error;
		}

		public UAConfig (UAConfig config)
		{
			this.ProductionAppKey = config.ProductionAppKey;
			this.ProductionAppSecret = config.ProductionAppSecret;
			this.ProductionLogLevel = config.ProductionLogLevel;

			this.DevelopmentAppKey = config.DevelopmentAppKey;
			this.DevelopmentAppSecret = config.DevelopmentAppSecret;
			this.DevelopmentLogLevel = config.DevelopmentLogLevel;

			this.InProduction = config.InProduction;

			this.NotificationPresentationOptionAlert = config.NotificationPresentationOptionAlert;
			this.NotificationPresentationOptionBadge = config.NotificationPresentationOptionBadge;
			this.NotificationPresentationOptionSound = config.NotificationPresentationOptionSound;

			this.GCMSenderId = config.GCMSenderId;
			this.AndroidNotificationAccentColor = config.AndroidNotificationAccentColor;
			this.AndroidNotificationIcon = config.AndroidNotificationIcon;
		}

		public static UAConfig LoadConfig ()
		{
			try {
				if (File.Exists (filePath)) {
					using (Stream fileStream = File.OpenRead (filePath)) {
						XmlSerializer serializer = new XmlSerializer (typeof(UAConfig));
						UAConfig config = (UAConfig)serializer.Deserialize (fileStream);
						config.Validate ();
						cachedInstance = config;
					}
				}
			} catch (Exception e) {
				UnityEngine.Debug.Log ("Failed to load UAConfig: " + e.Message);
				File.Delete (filePath);
			}

			if (cachedInstance == null) {
				cachedInstance = new UAConfig ();
			}

			return new UAConfig (cachedInstance);
		}

		public static void SaveConfig (UAConfig config)
		{
			config.Validate ();
			using (Stream fileStream = File.Open (filePath, FileMode.Create)) {
				XmlSerializer serializer = new XmlSerializer (typeof(UAConfig));
				serializer.Serialize (fileStream, config);
			}

			cachedInstance = config;
		}

		public bool Apply ()
		{
			if (IsValid) {
				GenerateIOSAirshipConfig ();
				GenerateAndroidAirshipConfig ();
				return true;
			}

			return false;
		}

		public void Validate ()
		{
			if (InProduction) {
				if (string.IsNullOrEmpty (ProductionAppKey)) {
					throw new Exception ("Production App Key missing.");
				}

				if (string.IsNullOrEmpty (ProductionAppSecret)) {
					throw new Exception ("Production App Secret missing.");
				}
			} else {
				if (string.IsNullOrEmpty (DevelopmentAppKey)) {
					throw new Exception ("Development App Key missing.");
				}

				if (string.IsNullOrEmpty (DevelopmentAppSecret)) {
					throw new Exception ("Development App Secret missing.");
				}
			}
		}


		private void GenerateIOSAirshipConfig ()
		{
			string plistPath = Path.Combine (Application.dataPath, "Plugins/iOS/AirshipConfig.plist");
			if (File.Exists (plistPath)) {
				File.Delete (plistPath);
			}

			PlistDocument plist = new PlistDocument ();

			PlistElementDict rootDict = plist.root;

			if (!String.IsNullOrEmpty (ProductionAppKey) && !String.IsNullOrEmpty (ProductionAppSecret)) {
				rootDict.SetString ("productionAppKey", ProductionAppKey);
				rootDict.SetString ("productionAppSecret", ProductionAppSecret);
				rootDict.SetInteger ("productionLogLevel", IOSLogLevel (ProductionLogLevel));
			}

			if (!String.IsNullOrEmpty (DevelopmentAppKey) && !String.IsNullOrEmpty (DevelopmentAppSecret)) {
				rootDict.SetString ("developmentAppKey", DevelopmentAppKey);
				rootDict.SetString ("developmentAppSecret", DevelopmentAppSecret);
				rootDict.SetInteger ("developmentLogLevel", IOSLogLevel (DevelopmentLogLevel));
			}

			rootDict.SetBoolean ("inProduction", InProduction);

			PlistElementDict customConfig = rootDict.CreateDict ("customConfig");
			customConfig.SetBoolean ("notificationPresentationOptionAlert", NotificationPresentationOptionAlert);
			customConfig.SetBoolean ("notificationPresentationOptionBadge", NotificationPresentationOptionBadge);
			customConfig.SetBoolean ("notificationPresentationOptionSound", NotificationPresentationOptionSound);

			File.WriteAllText (plistPath, plist.WriteToString ());
		}

		private void GenerateAndroidAirshipConfig ()
		{
			string res = Path.Combine (Application.dataPath, "Plugins/Android/urbanairship-plugin-lib/res");
			if (!Directory.Exists (res)) {
				Directory.CreateDirectory (res);
			}

			string xml = Path.Combine (Application.dataPath, "Plugins/Android/urbanairship-plugin-lib/res/xml");
			if (!Directory.Exists (xml)) {
				Directory.CreateDirectory (xml);
			}

			using (XmlWriter xmlWriter = XmlWriter.Create (Path.Combine (xml, "airship_config.xml"))) {
				xmlWriter.WriteStartDocument ();
				xmlWriter.WriteStartElement ("AirshipConfigOptions");

				if (!String.IsNullOrEmpty (ProductionAppKey) && !String.IsNullOrEmpty (ProductionAppSecret)) {
					xmlWriter.WriteAttributeString ("productionAppKey", ProductionAppKey);
					xmlWriter.WriteAttributeString ("productionAppSecret", ProductionAppSecret);
					xmlWriter.WriteAttributeString ("productionLogLevel", AndroidLogLevel (ProductionLogLevel));

				}

				if (!String.IsNullOrEmpty (DevelopmentAppKey) && !String.IsNullOrEmpty (DevelopmentAppSecret)) {
					xmlWriter.WriteAttributeString ("developmentAppKey", DevelopmentAppKey);
					xmlWriter.WriteAttributeString ("developmentAppSecret", DevelopmentAppSecret);
					xmlWriter.WriteAttributeString ("developmentLogLevel", AndroidLogLevel (DevelopmentLogLevel));
				}

				xmlWriter.WriteAttributeString ("inProduction", (InProduction ? "true" : "false"));

				if (!String.IsNullOrEmpty (AndroidNotificationIcon)) {
					xmlWriter.WriteAttributeString ("notificationIcon", AndroidNotificationIcon);
				}

				if (!String.IsNullOrEmpty (AndroidNotificationAccentColor)) {
					xmlWriter.WriteAttributeString ("notificationAccentColor", AndroidNotificationAccentColor);
				}

				if (!String.IsNullOrEmpty (GCMSenderId)) {
					xmlWriter.WriteAttributeString ("gcmSender", GCMSenderId);
				}

				xmlWriter.WriteEndElement ();
				xmlWriter.WriteEndDocument ();
			}
		}

		private int IOSLogLevel (LogLevel loglevel)
		{
			switch (loglevel) {
			case LogLevel.Verbose:
				return 5;
			case LogLevel.Debug:
				return 4;
			case LogLevel.Info:
				return 3;
			case LogLevel.Warn:
				return 2;
			case LogLevel.Error:
				return 1;
			case LogLevel.None:
				return 0;
			}

			return 0;
		}

		private string AndroidLogLevel (LogLevel loglevel)
		{
			return Enum.GetName (typeof(LogLevel), loglevel).ToLower ();
		}
	}
}
