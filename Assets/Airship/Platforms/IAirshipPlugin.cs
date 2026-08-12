/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AirshipSDK {

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
        // Goes through Deserialize so an unsupported platform sees the same empty
        // response handling as a real one, rather than a null array.
        public T Call<T> (string method, params object[] args) { return AirshipUtils.Deserialize<T>(null); }
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

        /// Internal method to make a Java Array with an array of String values,
        /// to be used with the PrivacyManager methods.
        public AndroidJavaObject MakeJavaArray(string[] values) {
            if (values == null) {
                return null;
            }

            // Create a Java String[] array using reflection
            AndroidJavaClass arrayClass = new AndroidJavaClass("java.lang.reflect.Array");
            AndroidJavaObject arrayObject = arrayClass.CallStatic<AndroidJavaObject>("newInstance", new AndroidJavaClass("java.lang.String"), values.Length);

            for (int i = 0; i < values.Length; i++) {
                arrayClass.CallStatic("set", arrayObject, i, new AndroidJavaObject("java.lang.String", values[i]));
            }

            return arrayObject;
        }

        public void Call (string method, params object[] args) {
            if (androidPlugin != null) {
                androidPlugin.Call (method, SerializeArgs(args));
            }
        }

        public T Call<T> (string method, params object[] args) {
            if (androidPlugin != null) {
                return androidPlugin.Call<T> (method, SerializeArgs(args));
            }
            return default(T);
        }

        private static object[] SerializeArgs(object[] args) {
            if (args == null || args.Length == 0) return args;
            var result = new object[args.Length];
            for (int i = 0; i < args.Length; i++) {
                result[i] = SerializeIfNeeded(args[i]);
            }
            return result;
        }

        private static object SerializeIfNeeded(object arg) {
            if (arg == null) return null;
            Type type = arg.GetType();
            if (type.IsPrimitive || arg is string || arg is decimal || arg is AndroidJavaObject) return arg;
            return AirshipUtils.Serialize(arg);
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

        [Serializable]
        private class CallResponse {
            public string result;
            public string error;
        }

        [DllImport ("__Internal")]
        private static extern IntPtr UnityPlugin_call (string method, string argsJson);

        [DllImport ("libc")]
        private static extern void free (IntPtr ptr);

        /// <summary>
        /// Calls the native plugin, parses the response envelope, and returns
        /// the inner result JSON string. Throws on native errors.
        /// </summary>
        private static string CallNative (string method, string argsJson) {
            IntPtr ptr = UnityPlugin_call(method, argsJson);
            if (ptr == IntPtr.Zero) {
                throw new Exception("Airship: null response from native for method " + method);
            }
            string responseJson = Marshal.PtrToStringUTF8(ptr);
            free(ptr);

            CallResponse response = JsonUtility.FromJson<CallResponse>(responseJson);
            if (!string.IsNullOrEmpty(response.error)) {
                throw new Exception(response.error);
            }
            return response.result;
        }

        public void Call (string method, params object[] args) {
            string argsJson = SerializeArgs(args);
            CallNative(method, argsJson);
        }

        public T Call<T> (string method, params object[] args) {
            string argsJson = SerializeArgs(args);
            string resultJson = CallNative(method, argsJson);

            if (string.IsNullOrEmpty(resultJson) || resultJson == "null" || resultJson == "{}") {
                return default(T);
            }

            return AirshipUtils.Deserialize<T>(resultJson);
        }

        public GameObject Listener {
            set {
                Call ("setListener", value.name);
            }
        }

        public static string SerializeArgs(params object[] args) {
            if (args == null || args.Length == 0) {
                return "[]";
            }

            var sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < args.Length; i++) {
                if (i > 0) sb.Append(",");
                sb.Append(AirshipUtils.SerializeValue(args[i]));
            }
            sb.Append("]");
            return sb.ToString();
        }
    }

    #endif
    
}
