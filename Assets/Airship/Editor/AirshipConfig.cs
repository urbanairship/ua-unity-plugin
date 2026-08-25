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

namespace AirshipSDK.Editor {

    [InitializeOnLoad]
    [Serializable]
    public class AirshipConfig {

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

        private static readonly string filePath = "ProjectSettings/Airship.xml";
        private static readonly string legacyFilePath = "ProjectSettings/UrbanAirship.xml";
        private static AirshipConfig cachedInstance;

        [field: SerializeField]
        public string ProductionAppKey { get; set; }

        [field: SerializeField]
        public string ProductionAppSecret { get; set; }

        [field: SerializeField]
        public LogLevel ProductionLogLevel { get; set; }

        [field: SerializeField]
        public string DevelopmentAppKey { get; set; }

        [field: SerializeField]
        public string DevelopmentAppSecret { get; set; }

        [field: SerializeField]
        public LogLevel DevelopmentLogLevel { get; set; }

        [field: SerializeField]
        public bool GenerateGoogleJsonConfig { get; set; }

        [field: SerializeField]
        public bool NotificationPresentationOptionAlert { get; set; }

        [field: SerializeField]
        public bool NotificationPresentationOptionBadge { get; set; }

        [field: SerializeField]
        public bool NotificationPresentationOptionSound { get; set; }

        [field: SerializeField]
        public bool InProduction { get; set; }

        [field: SerializeField]
        public String UrlAllowList { get; set; }

        [field: SerializeField]
        public String UrlAllowListScopeOpenURL { get; set; }

        [field: SerializeField]
        public String UrlAllowListScopeJavaScriptInterface { get; set; }

        [field: SerializeField]
        public String EnabledFeatures { get; set; }

        [field: SerializeField]
        public String AndroidNotificationIcon { get; set; }

        [field: SerializeField]
        public String AndroidNotificationAccentColor { get; set; }

        [field: SerializeField]
        public String Version { get; set; }

        [field: SerializeField]
        public CloudSite Site { get; set; }

        /// <summary>
        /// Whether any app credentials have been configured.
        /// Returns false when all key/secret fields are empty, meaning the user
        /// needs to configure Airship via TakeOff instead of the editor config files.
        /// </summary>
        public bool IsConfigured {
            get {
                bool hasProd = !string.IsNullOrEmpty (ProductionAppKey) || !string.IsNullOrEmpty (ProductionAppSecret);
                bool hasDev = !string.IsNullOrEmpty (DevelopmentAppKey) || !string.IsNullOrEmpty (DevelopmentAppSecret);
                return hasProd || hasDev;
            }
        }

        public bool IsValid {
            get { return ValidationError == null; }
        }

        /// <summary>
        /// The reason the config is not valid, or <c>null</c> when it is.
        /// </summary>
        /// <remarks>
        /// <see cref="Validate"/> names the specific field that is missing. Reducing that to
        /// a bare bool left callers with nothing to tell the developer beyond "not
        /// configured", which is the same message whichever field is wrong.
        /// </remarks>
        public string ValidationError {
            get {
                try {
                    Validate ();
                    return null;
                } catch (Exception e) {
                    return e.Message;
                }
            }
        }

        public AirshipConfig () {
            DevelopmentLogLevel = LogLevel.Debug;
            ProductionLogLevel = LogLevel.Error;
            GenerateGoogleJsonConfig = true;
            Version = PluginInfo.Version;
            Site = CloudSite.US;
        }

        public AirshipConfig (AirshipConfig config) {
            this.ProductionAppKey = config.ProductionAppKey;
            this.ProductionAppSecret = config.ProductionAppSecret;
            this.ProductionLogLevel = config.ProductionLogLevel;

            this.DevelopmentAppKey = config.DevelopmentAppKey;
            this.DevelopmentAppSecret = config.DevelopmentAppSecret;
            this.DevelopmentLogLevel = config.DevelopmentLogLevel;

            this.InProduction = config.InProduction;

            this.UrlAllowList = config.UrlAllowList;
            this.UrlAllowListScopeOpenURL = config.UrlAllowListScopeOpenURL;
            this.UrlAllowListScopeJavaScriptInterface = config.UrlAllowListScopeJavaScriptInterface;

            this.EnabledFeatures = config.EnabledFeatures;

            this.NotificationPresentationOptionAlert = config.NotificationPresentationOptionAlert;
            this.NotificationPresentationOptionBadge = config.NotificationPresentationOptionBadge;
            this.NotificationPresentationOptionSound = config.NotificationPresentationOptionSound;

            this.AndroidNotificationAccentColor = config.AndroidNotificationAccentColor;
            this.AndroidNotificationIcon = config.AndroidNotificationIcon;
            this.GenerateGoogleJsonConfig = config.GenerateGoogleJsonConfig;

            this.Site = config.Site;
        }

        public static AirshipConfig LoadConfig () {
            if (cachedInstance != null) {
                return new AirshipConfig (cachedInstance);
            }

            if (!File.Exists (filePath) && File.Exists (legacyFilePath)) {
                UnityEngine.Debug.Log ("AirshipConfig: migrating config from " + legacyFilePath + " to " + filePath);
                File.Move (legacyFilePath, filePath);
            }

            bool migratedConfig = false;
            try {
                if (File.Exists (filePath)) {
                    using (Stream fileStream = File.OpenRead (filePath)) {
                        XmlSerializer serializer = new XmlSerializer (typeof (AirshipConfig));
                        AirshipConfig config = (AirshipConfig) serializer.Deserialize (fileStream);
                        migratedConfig = config.Migrate ();
                        config.Validate ();
                        cachedInstance = config;
                    }
                }
            } catch (Exception e) {
                UnityEngine.Debug.Log ("AirshipConfig: Failed to load config: " + e.Message);
                File.Delete (filePath);
            }

            if (cachedInstance == null) {
                cachedInstance = new AirshipConfig ();
            }

            if (migratedConfig) {
                UnityEngine.Debug.Log ("AirshipConfig: saving config");
                SaveConfig(cachedInstance);
            }

            return new AirshipConfig (cachedInstance);
        }

        public static void SaveConfig (AirshipConfig config) {
            config.Validate ();
            using (Stream fileStream = File.Open (filePath, FileMode.Create)) {
                XmlSerializer serializer = new XmlSerializer (typeof (AirshipConfig));
                serializer.Serialize (fileStream, config);
            }

            cachedInstance = config;
        }

        public bool Apply () {
            if (!IsConfigured) {
                CleanUpGeneratedConfigs ();
                return false;
            }

            if (IsValid) {
#if UNITY_IOS
                GenerateIOSAirshipConfig ();
#endif

#if UNITY_ANDROID
                GenerateAndroidLib();
                GenerateAndroidAirshipConfig ();
#endif
                return true;
            }

            return false;
        }

        public void Validate () {
            if (!IsConfigured) {
                return;
            }

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
                UnityEngine.Debug.Log ("AirshipConfig: migrating pre-versioned config to version " + PluginInfo.Version);
                GenerateGoogleJsonConfig = true;
                Version = PluginInfo.Version;
            } else if (Version != PluginInfo.Version) {
                UnityEngine.Debug.Log ("AirshipConfig: migrating from version " + Version + " to version " + PluginInfo.Version);
                Version = PluginInfo.Version;
            } else {
                UnityEngine.Debug.Log("AirshipConfig: no migration needed. Version already " + Version);
                return false;
            }

            // migrate to new log levels
            if (ProductionLogLevel == LogLevel.Warning) {
                UnityEngine.Debug.Log ("AirshipConfig: migrating obsolete Production Log Level = Warning to Warn");
                ProductionLogLevel = LogLevel.Warn;
            }
            if (DevelopmentLogLevel == LogLevel.Warning) {
                UnityEngine.Debug.Log ("AirshipConfig: migrating obsolete Development Log Level = Warning to Warn");
                DevelopmentLogLevel = LogLevel.Warn;
            }

            UnityEngine.Debug.Log ("AirshipConfig: migrated to version " + Version);

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

            rootDict.SetString ("site", Site.ToString());
            rootDict.SetBoolean ("inProduction", InProduction);

            if (!String.IsNullOrEmpty(UrlAllowList))
            {
                PlistElementArray urlAllowListConfig = rootDict.CreateArray("URLAllowList");
                foreach (string url in UrlAllowList.Split(','))
                {
                    urlAllowListConfig.AddString(url);
                }
            }

            if (!String.IsNullOrEmpty(UrlAllowListScopeOpenURL))
            {
                PlistElementArray urlAllowListScopeOpenURLConfig = rootDict.CreateArray("URLAllowListScopeOpenURL");
                foreach (string url in UrlAllowListScopeOpenURL.Split(','))
                {
                    urlAllowListScopeOpenURLConfig.AddString(url);
                }
            }

            if (!String.IsNullOrEmpty(UrlAllowListScopeJavaScriptInterface))
            {
                PlistElementArray urlAllowListScopeJavaScriptInterfaceConfig = rootDict.CreateArray("URLAllowListScopeJavaScriptInterface");
                foreach (string url in UrlAllowListScopeJavaScriptInterface.Split(','))
                {
                    urlAllowListScopeJavaScriptInterfaceConfig.AddString(url);
                }
            }

            if (!String.IsNullOrEmpty(EnabledFeatures))
            {
                PlistElementArray enabledFeaturesConfig = rootDict.CreateArray("enabledFeatures");
                foreach (string feature in EnabledFeatures.Split(','))
                {
                    enabledFeaturesConfig.AddString(feature);
                }
            }

            PlistElementDict customConfig = rootDict.CreateDict ("customConfig");
            customConfig.SetBoolean ("notificationPresentationOptionAlert", NotificationPresentationOptionAlert);
            customConfig.SetBoolean ("notificationPresentationOptionBadge", NotificationPresentationOptionBadge);
            customConfig.SetBoolean ("notificationPresentationOptionSound", NotificationPresentationOptionSound);

            File.WriteAllText (plistPath, plist.WriteToString ());
        }
#endif

        /// <summary>
        /// Generates an Androidlib for generated Airship resources.
        /// </summary>
        private void GenerateAndroidLib () {
            string androidlib = "Assets/Plugins/Android/airship-resources.androidlib";
            if (!Directory.Exists (androidlib)) {
                Directory.CreateDirectory (androidlib);
            }

            string manifest = "Assets/Plugins/Android/airship-resources.androidlib/AndroidManifest.xml";
            if (File.Exists (manifest)) {
                return;
            }
            
            using (XmlWriter xmlWriter = XmlWriter.Create (Path.Combine (androidlib, "AndroidManifest.xml"))) {
                xmlWriter.WriteStartDocument ();
                xmlWriter.WriteStartElement ("manifest");
                xmlWriter.WriteAttributeString ("xmlns", "android", null, "http://schemas.android.com/apk/res/android");
                xmlWriter.WriteEndElement ();
                xmlWriter.WriteEndDocument ();
            }
        }

        /// <summary>
        /// Converts google-services.json into Android string resources for FCM.
        /// Controlled solely by the "Process google-service" setting and runs
        /// independently of the Airship app credentials.
        /// It works whether the app is configured through the editor or at runtime via TakeOff.
        /// When the setting is disabled, any previously generated resource is removed.
        /// </summary>
        public void ApplyFirebaseConfig () {
            string res = "Assets/Plugins/Android/airship-resources.androidlib/res/values";
            string json = "Assets/google-services.json";
            string xml = "Assets/Plugins/Android/airship-resources.androidlib/res/values/values.xml";

            if (!GenerateGoogleJsonConfig) {
                if (File.Exists (xml)) {
                    File.Delete (xml);
                }
                return;
            }

            if (!File.Exists (json)) {
                UnityEngine.Debug.LogWarning ("AirshipConfig: 'Process google-service' is enabled but " +
                    json + " was not found. Skipping Firebase (google-services) resource generation.");
                return;
            }

            GenerateAndroidLib ();

            if (!Directory.Exists (res)) {
                Directory.CreateDirectory (res);
            }

            GoogleJson googleJson = GoogleJson.FromPath (json);
            if (googleJson != null) {
                googleJson.WriteXml (xml);
            }
        }

        private void GenerateAndroidAirshipConfig () {
            string res = "Assets/Plugins/Android/airship-resources.androidlib/res";
            if (!Directory.Exists (res)) {
                Directory.CreateDirectory (res);
            }

            string xml = "Assets/Plugins/Android/airship-resources.androidlib/res/xml";
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

                xmlWriter.WriteAttributeString ("site", Site.ToString());
                xmlWriter.WriteAttributeString ("inProduction", (InProduction ? "true" : "false"));

                if (!String.IsNullOrEmpty(UrlAllowList))
                {
                    xmlWriter.WriteAttributeString ("urlAllowList", UrlAllowList.ToString());
                }

                if (!String.IsNullOrEmpty(UrlAllowListScopeOpenURL))
                {
                    xmlWriter.WriteAttributeString ("urlAllowListScopeOpenURL", UrlAllowListScopeOpenURL.ToString());
                }

                if (!String.IsNullOrEmpty(UrlAllowListScopeJavaScriptInterface))
                {
                    xmlWriter.WriteAttributeString ("urlAllowListScopeJavaScriptInterface", UrlAllowListScopeJavaScriptInterface.ToString());
                }

                if (!String.IsNullOrEmpty(EnabledFeatures))
                {
                    xmlWriter.WriteAttributeString ("enabledFeatures", EnabledFeatures.ToString());
                }

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

        private void CleanUpGeneratedConfigs () {
            string androidConfig = "Assets/Plugins/Android/airship-resources.androidlib/res/xml/airship_config.xml";
            if (File.Exists (androidConfig)) {
                File.Delete (androidConfig);
            }

            string iosConfig = "Assets/Plugins/iOS/AirshipConfig.plist";
            if (File.Exists (iosConfig)) {
                File.Delete (iosConfig);
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
