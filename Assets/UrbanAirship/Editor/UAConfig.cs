/* Copyright Airship and Contributors */

using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace UrbanAirship.Editor {

    [InitializeOnLoad]
    [Serializable]
    public class UAConfig {

        public enum LogLevel {
            Verbose = 0,
            Debug = 1,
            Info = 2,
            Warn = 3,
            Warning = -1,
            Error = 4,
            None = 5
        }

        public enum CloudSite {
            US = 0,
            EU = 1,
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
        public bool GenerateGoogleJsonConfig { get; set; }

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

        [SerializeField]
        public String Version { get; set; }

        [SerializeField]
        public CloudSite Site { get; set; }

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

        public UAConfig () {
            DevelopmentLogLevel = LogLevel.Debug;
            ProductionLogLevel = LogLevel.Error;
            GenerateGoogleJsonConfig = true;
            Version = PluginInfo.Version;
            Site = CloudSite.US;
        }

        public UAConfig (UAConfig config) {
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

            this.AndroidNotificationAccentColor = config.AndroidNotificationAccentColor;
            this.AndroidNotificationIcon = config.AndroidNotificationIcon;
            this.GenerateGoogleJsonConfig = config.GenerateGoogleJsonConfig;

            this.Site = config.Site;
        }

        public static UAConfig LoadConfig () {
            if (cachedInstance != null) {
                return new UAConfig (cachedInstance);
            }

            bool migratedConfig = false;
            try {
                if (File.Exists (filePath)) {
                    using (Stream fileStream = File.OpenRead (filePath)) {
                        XmlSerializer serializer = new XmlSerializer (typeof (UAConfig));
                        UAConfig config = (UAConfig) serializer.Deserialize (fileStream);
                        migratedConfig = config.Migrate ();
                        config.Validate ();
                        cachedInstance = config;
                    }
                }
            } catch (Exception e) {
                UnityEngine.Debug.Log ("UAConfig: Failed to load config: " + e.Message);
                File.Delete (filePath);
            }

            if (cachedInstance == null) {
                cachedInstance = new UAConfig ();
            }

            if (migratedConfig) {
                UnityEngine.Debug.Log ("UAConfig: saving config");
                SaveConfig(cachedInstance);
            }

            return new UAConfig (cachedInstance);
        }

        public static void SaveConfig (UAConfig config) {
            config.Validate ();
            using (Stream fileStream = File.Open (filePath, FileMode.Create)) {
                XmlSerializer serializer = new XmlSerializer (typeof (UAConfig));
                serializer.Serialize (fileStream, config);
            }

            cachedInstance = config;
        }

        public bool Apply () {
            if (IsValid) {
#if UNITY_IOS
                GenerateIOSAirshipConfig ();
#endif

#if UNITY_ANDROID
                GenerateAndroidAirshipConfig ();
                GenerateFirebaseConfig ();
#endif
                return true;
            }

            return false;
        }

        public void Validate () {
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

        public bool Migrate () {
             if (Version == null) {
                UnityEngine.Debug.Log ("UAConfig: migrating pre-versioned config to version " + PluginInfo.Version);
                GenerateGoogleJsonConfig = true;
                Version = PluginInfo.Version;
            } else if (Version != PluginInfo.Version) {
                UnityEngine.Debug.Log ("UAConfig: migrating from version " + Version + " to version " + PluginInfo.Version);
                Version = PluginInfo.Version;
            } else {
                UnityEngine.Debug.Log("UAConfig: no migration needed. Version already " + Version);
                return false;
            }

            // migrate to new log levels
            if (ProductionLogLevel == LogLevel.Warning) {
                UnityEngine.Debug.Log ("UAConfig: migrating obsolete Production Log Level = Warning to Warn");
                ProductionLogLevel = LogLevel.Warn;
            }
            if (DevelopmentLogLevel == LogLevel.Warning) {
                UnityEngine.Debug.Log ("UAConfig: migrating obsolete Development Log Level = Warning to Warn");
                DevelopmentLogLevel = LogLevel.Warn;
            }

            UnityEngine.Debug.Log ("UAConfig: migrated to version " + Version);

            return true;
        }

#if UNITY_IOS
        private void GenerateIOSAirshipConfig () {
            string plistPath = "Assets/Plugins/iOS/AirshipConfig.plist";
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

            rootDict.SetString ("site", Enum.GetName(Site));
            rootDict.SetBoolean ("inProduction", InProduction);

            PlistElementDict customConfig = rootDict.CreateDict ("customConfig");
            customConfig.SetBoolean ("notificationPresentationOptionAlert", NotificationPresentationOptionAlert);
            customConfig.SetBoolean ("notificationPresentationOptionBadge", NotificationPresentationOptionBadge);
            customConfig.SetBoolean ("notificationPresentationOptionSound", NotificationPresentationOptionSound);

            File.WriteAllText (plistPath, plist.WriteToString ());
        }
#endif

        private void GenerateFirebaseConfig () {
            string res = "Assets/Plugins/Android/urbanairship-resources/res/values";
            string json = "Assets/google-services.json";
            string xml = "Assets/Plugins/Android/urbanairship-resources/res/values/values.xml";

            if (!GenerateGoogleJsonConfig) {
                File.Delete (xml);
                return;
            }

            if (!Directory.Exists (res)) {
                Directory.CreateDirectory (res);
            }

            GoogleJson.FromPath (json).WriteXml (xml);
        }

        private void GenerateAndroidAirshipConfig () {
            string res = "Assets/Plugins/Android/urbanairship-resources/res";
            if (!Directory.Exists (res)) {
                Directory.CreateDirectory (res);
            }

            string xml = "Assets/Plugins/Android/urbanairship-resources/res/xml";
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

                xmlWriter.WriteAttributeString ("site", Enum.GetName(Site));
                xmlWriter.WriteAttributeString ("inProduction", (InProduction ? "true" : "false"));

                if (!String.IsNullOrEmpty (AndroidNotificationIcon)) {
                    xmlWriter.WriteAttributeString ("notificationIcon", AndroidNotificationIcon);
                }

                if (!String.IsNullOrEmpty (AndroidNotificationAccentColor)) {
                    xmlWriter.WriteAttributeString ("notificationAccentColor", AndroidNotificationAccentColor);
                }

                xmlWriter.WriteEndElement ();
                xmlWriter.WriteEndDocument ();
            }
        }

        private int IOSLogLevel (LogLevel loglevel) {
            switch (loglevel) {
                case LogLevel.Verbose:
                    return 5;
                case LogLevel.Debug:
                    return 4;
                case LogLevel.Info:
                    return 3;
                case LogLevel.Warn:
                case LogLevel.Warning:
                     return 2;
                case LogLevel.Error:
                    return 1;
                case LogLevel.None:
                    return 0;
            }

            return 0;
        }

        private string AndroidLogLevel (LogLevel loglevel) {
            return Enum.GetName (typeof (LogLevel), loglevel).ToLower ();
        }
    }
}
