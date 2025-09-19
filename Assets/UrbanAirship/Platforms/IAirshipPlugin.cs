/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UrbanAirship {

    internal interface IAirshipPlugin {

        GameObject Listener {
            set;
        }

        void Call (string method, params object[] args);

        T Call<T> (string method, params object[] args);
    }

    internal class StubbedAirshipPlugin : IAirshipPlugin {
        public GameObject Listener { set; private get; }
        public void Call (string method, params object[] args) {}
        public T Call<T> (string method, params object[] args) { return default(T); }
    }

    #if UNITY_ANDROID

    internal class AirshipPluginAndroid : IAirshipPlugin {
        
        private AndroidJavaObject androidPlugin;

        public AirshipPluginAndroid () {
            try {
                using (AndroidJavaClass pluginClass = new AndroidJavaClass ("com.urbanairship.unityplugin.UnityPlugin")) {
                    androidPlugin = pluginClass.CallStatic<AndroidJavaObject> ("shared");
                }
            } catch (Exception e) {
                Debug.LogError ("Airship plugin not found : " + e);
            }
        }

        public void Call (string method, params object[] args) {
            if (androidPlugin != null) {
                androidPlugin.Call (method, args);
            }
        }

        public T Call<T> (string method, params object[] args) {
            if (androidPlugin != null) {
                return androidPlugin.Call<T> (method, args);
            }
            return default(T);
        }

        public GameObject Listener {
            set {
                Call ("setListener", value.name);
            }
        }
    }

    #endif

    #if UNITY_IOS

    internal class AirshipPluginiOS : IAirshipPlugin{

        [DllImport ("__Internal")]
        private static extern void UnityPlugin_call (string method, string args);

        public void Call (string method, params object[] args) {
            UnityPlugin_call(method, JsonUtility.ToJson(args));
        }

        public T Call<T> (string method, params object[] args) {
            
            return default(T);
        }

        public GameObject Listener {
            set {
                Call ("setListener", value.name);
            }
        }
    }

    #endif
    
}
