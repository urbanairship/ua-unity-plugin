/* Copyright Urban Airship and Contributors */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace UrbanAirship.Editor {

    public class GoogleJson {

        private const string GcmDefaultSenderId = "gcm_defaultSenderId";
        private const string FirebaseDatabaseUrl = "firebase_database_url";
        private const string ProjectId = "project_id";
        private const string GoogleStorageBucket = "google_storage_bucket";
        private const string GoogleAppId = "google_app_id";
        private const string GoogleApiKey = "google_api_key";
        private const string GoogleCrashReportingApiKey = "google_crash_reporting_api_key";
        private const string DefaultWebClientId = "default_web_client_id";
        private const int WebClientType = 3;

        private Config config;

        private GoogleJson (Config config) {
            this.config = config;
        }

        public static GoogleJson FromPath (string path) {
            string json = System.IO.File.ReadAllText (path);
            Config config = JsonUtility.FromJson<Config> (json);
            return new GoogleJson (config);
        }

        public void WriteXml (string path) {
            Dictionary<string, string> map = new Dictionary<string, string> ();
            map[GcmDefaultSenderId] = config.project_info.project_number;
            map[FirebaseDatabaseUrl] = config.project_info.firebase_url;
            map[ProjectId] = config.project_info.project_id;
            map[GoogleStorageBucket] = config.project_info.storage_bucket;

            Client client = config.client.First ();
            if (client != null) {
                map[GoogleAppId] = client.client_info.mobilesdk_app_id;

                ApiKey apiKey = client.api_key.First ();
                if (apiKey != null) {
                    map[GoogleApiKey] = apiKey.current_key;
                    map[GoogleCrashReportingApiKey] = apiKey.current_key;
                }

                OauthClient webClient = client.oauth_client.Where (c => c.client_type == WebClientType).First ();
                if (webClient != null) {
                    map[DefaultWebClientId] = webClient.client_id;
                }
            }

            XmlWriterSettings settings = new XmlWriterSettings ();
            settings.Indent = true;
            settings.NewLineOnAttributes = true;

            using (XmlWriter xmlWriter = XmlWriter.Create (path, settings)) {
                xmlWriter.WriteStartDocument ();
                xmlWriter.WriteStartElement ("resources");

                foreach (KeyValuePair<string, string> item in map) {
                    xmlWriter.WriteStartElement ("string");
                    xmlWriter.WriteAttributeString ("translatable", "false");
                    xmlWriter.WriteAttributeString ("name", item.Key);
                    xmlWriter.WriteValue (item.Value);
                    xmlWriter.WriteEndElement ();
                }

                xmlWriter.WriteEndElement ();
                xmlWriter.WriteEndDocument ();
            }
        }
    }

    [System.Serializable]
    public class Config {
        [SerializeField]
        public ProjectInfo project_info;

        [SerializeField]
        public List<Client> client;

        [SerializeField]
        public string configuration_version;
    }

    [System.Serializable]
    public class ProjectInfo {
        [SerializeField]
        public string project_number;

        [SerializeField]
        public string firebase_url;

        [SerializeField]
        public string project_id;

        [SerializeField]
        public string storage_bucket;
    }

    [System.Serializable]
    public class Client {
        [SerializeField]
        public ClientInfo client_info;

        [SerializeField]
        public List<OauthClient> oauth_client;

        [SerializeField]
        public List<ApiKey> api_key;

        [SerializeField]
        public Services services;
    }

    [System.Serializable]
    public class ClientInfo {
        [SerializeField]
        public string mobilesdk_app_id;

        [SerializeField]
        public AndroidInfo android_client_info;
    }

    [System.Serializable]
    public class OauthClient {
        [SerializeField]
        public string client_id;

        [SerializeField]
        public int client_type;

        [SerializeField]
        public AndroidInfo android_info;
    }

    [System.Serializable]
    public class Services {
        [SerializeField]
        public Service analytics_service;

        [SerializeField]
        public Service appinvite_service;

        [SerializeField]
        public Service ads_service;

        [SerializeField]
        public string configuration_version;

    }

    [System.Serializable]
    public class Service {
        [SerializeField]
        public int status;
    }

    [System.Serializable]
    public class AndroidInfo {
        [SerializeField]
        public string package_name;

        [SerializeField]
        public string certificate_hash;
    }

    [System.Serializable]
    public class ApiKey {
        [SerializeField]
        public string current_key;
    }
}
