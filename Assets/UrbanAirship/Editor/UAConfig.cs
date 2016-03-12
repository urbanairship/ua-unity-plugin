/*
 Copyright 2016 Urban Airship and Contributors
*/

using System;
using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using UnityEditor.iOS.Xcode;
using UnityEditor;

namespace UrbanAirship
{
	[Serializable]
	public class UAConfig
	{
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


		private static readonly string filePath = "ProjectSettings/UrbanAirship.xml";

		private static UAConfig instace = null;
		public static UAConfig Instance
		{
			get
			{
				if (instace == null)
				{
					try
					{
						if (File.Exists(filePath))
						{
							using (Stream fileStream = File.OpenRead(filePath))
							{
								XmlSerializer serializer = new XmlSerializer(typeof(UAConfig));
								instace = (UAConfig)serializer.Deserialize(fileStream);
							}
						}
						else
						{
							instace = new UAConfig();
						}
					}
					catch(Exception e)
					{
						File.Delete(filePath);
						instace = new UAConfig();
					}

				}

				return instace;
			}
		}

		public void Apply()
		{
			Validate();

			CreateIOSAirshipConfig();
			CreateAndroidAirshipConfig();
		}

		public void Save()
		{
			Validate();

			using (Stream fileStream =  File.Open(filePath, FileMode.Create)) 
			{
				XmlSerializer serializer = new XmlSerializer(typeof(UAConfig));
				serializer.Serialize(fileStream, this);
			}
		}

		public void Validate()
		{
			if (InProduction)
			{
				if (string.IsNullOrEmpty (ProductionAppKey))
				{
					throw new Exception ("Production App Key missing.");
				}

				if (string.IsNullOrEmpty (ProductionAppSecret))
				{
					throw new Exception ("Production App Secret missing.");
				}
			}
			else
			{
				if (string.IsNullOrEmpty (DevelopmentAppKey))
				{
					throw new Exception ("Development App Key missing.");
				}

				if (string.IsNullOrEmpty (DevelopmentAppSecret))
				{
					throw new Exception ("Development App Secret missing.");
				}
			}
		}

		private void CreateIOSAirshipConfig()
		{
			string plistPath = Path.Combine(Application.dataPath, "Plugins/iOS/AirshipConfig.plist");
			if (File.Exists(plistPath))
			{
				File.Delete(plistPath);
			}

			PlistDocument plist = new PlistDocument();

			PlistElementDict rootDict = plist.root;

			if (!String.IsNullOrEmpty(ProductionAppKey) && !String.IsNullOrEmpty(ProductionAppSecret))
			{
				rootDict.SetString("productionAppKey", ProductionAppKey);
				rootDict.SetString ("productionAppSecret", ProductionAppSecret);
			}
				
			if (!String.IsNullOrEmpty(DevelopmentAppKey) && !String.IsNullOrEmpty(DevelopmentAppSecret))
			{
				rootDict.SetString("developmentAppKey", DevelopmentAppKey);
				rootDict.SetString("developmentAppSecret", DevelopmentAppSecret);
			}

			rootDict.SetBoolean("inProduction", InProduction);


			File.WriteAllText(plistPath, plist.WriteToString());
		}

		private void CreateAndroidAirshipConfig()
		{
			string assetsDirecotry = Path.Combine (Application.dataPath, "Plugins/Android/assets");
			if (!Directory.Exists(assetsDirecotry))
			{
				Directory.CreateDirectory(assetsDirecotry);
			}

			using (StreamWriter sw = new StreamWriter(Path.Combine(assetsDirecotry, "airshipconfig.properties"))) {

				if (!String.IsNullOrEmpty(ProductionAppKey) && !String.IsNullOrEmpty(ProductionAppSecret))
				{
					sw.WriteLine("productionAppKey = " + ProductionAppKey);
					sw.WriteLine("productionAppSecret = " + ProductionAppSecret);
				}

				if (!String.IsNullOrEmpty(DevelopmentAppKey) && !String.IsNullOrEmpty(DevelopmentAppSecret))
				{
					sw.WriteLine("developmentAppKey = " + DevelopmentAppKey);
					sw.WriteLine("developmentAppSecret = " + DevelopmentAppSecret);
				}

				if (!String.IsNullOrEmpty(GCMSenderId))
				{
					sw.WriteLine("gcmSender = " + GCMSenderId);
				}

				sw.WriteLine("inProduction = " + (InProduction ? "true" : "false"));
			}
		}
	}
}

