/*
 Copyright 2016 Urban Airship and Contributors
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
		private static readonly string filePath = "ProjectSettings/UrbanAirship.xml";
		private static UAConfig cachedInstance;

		[SerializeField]
		public string ProductionAppKey { get; set; }

		[SerializeField]
		public string ProductionAppSecret { get; set; }

		[SerializeField]
		public string DevelopmentAppKey { get; set; }

		[SerializeField]
		public string DevelopmentAppSecret { get; set; }

		[SerializeField]
		public string GCMSenderId { get; set; }

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
		}

		public UAConfig (UAConfig config)
		{
			this.ProductionAppKey = config.ProductionAppKey;
			this.ProductionAppSecret = config.ProductionAppSecret;
			this.DevelopmentAppKey = config.DevelopmentAppKey;
			this.DevelopmentAppSecret = config.DevelopmentAppSecret;
			this.InProduction = config.InProduction;
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
			}
				
			if (!String.IsNullOrEmpty (DevelopmentAppKey) && !String.IsNullOrEmpty (DevelopmentAppSecret)) {
				rootDict.SetString ("developmentAppKey", DevelopmentAppKey);
				rootDict.SetString ("developmentAppSecret", DevelopmentAppSecret);
			}

			rootDict.SetBoolean ("inProduction", InProduction);


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
				xmlWriter.WriteStartElement ("UrbanAirshipConfig");

				xmlWriter.WriteAttributeString ("xmlns:app", "http://schemas.android.com/apk/res-auto");

				if (!String.IsNullOrEmpty (ProductionAppKey) && !String.IsNullOrEmpty (ProductionAppSecret)) {
					xmlWriter.WriteAttributeString ("app:urbanAirshipProductionAppKey", ProductionAppKey);
					xmlWriter.WriteAttributeString ("app:urbanAirshipProductionAppSecret", ProductionAppSecret);
				}

				if (!String.IsNullOrEmpty (DevelopmentAppKey) && !String.IsNullOrEmpty (DevelopmentAppSecret)) {
					xmlWriter.WriteAttributeString ("app:urbanAirshipDevelopmentAppKey", DevelopmentAppKey);
					xmlWriter.WriteAttributeString ("app:urbanAirshipDevelopmentAppSecret", DevelopmentAppSecret);
				}

				xmlWriter.WriteAttributeString ("app:urbanAirshipInProduction", (InProduction ? "true" : "false"));

				if (!String.IsNullOrEmpty (AndroidNotificationIcon)) {
					xmlWriter.WriteAttributeString ("app:urbanAirshipNotificationIcon", AndroidNotificationIcon);
				}

				if (!String.IsNullOrEmpty (AndroidNotificationAccentColor)) {
					xmlWriter.WriteAttributeString ("app:urbanAirshipNotificationAccentColor", AndroidNotificationAccentColor);
				}

				if (!String.IsNullOrEmpty (GCMSenderId)) {
					xmlWriter.WriteAttributeString ("app:urbanAirshipGcmSender", GCMSenderId);
				}

				xmlWriter.WriteEndElement ();
				xmlWriter.WriteEndDocument ();
			}
		}
	}
}

