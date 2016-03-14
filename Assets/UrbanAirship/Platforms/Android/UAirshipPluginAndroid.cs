/*
 Copyright 2015 Urban Airship and Contributors
*/

#if UNITY_ANDROID

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;


public class UAirshipPluginAndroid : IUAirshipPlugin
{

	private AndroidJavaObject androidPlugin;

	public UAirshipPluginAndroid ()
	{
		try {
			using (AndroidJavaClass pluginClass = new AndroidJavaClass ("com.urbanairship.unityplugin.UnityPlugin")) {
				androidPlugin = pluginClass.CallStatic<AndroidJavaObject> ("shared");
			}
		} catch (Exception) {
			Debug.LogError ("UAirship plugin not found");
		}
	}

	public bool PushEnabled {
		get {
			return Call<bool> ("isPushEnabled");
		}
		set {
			if (value) {
				Call ("enablePush");
			} else {
				Call ("disablePush");
			}
		}
	}

	public string Tags {
		get {
			return Call<string> ("getTags");
		}
	}

	public string Alias {
		get {
			return Call<string> ("getAlias");
		}
		set {
			Call ("setAlias", value);
		}
	}

	public string ChannelId {
		get {
			return Call<string> ("getChannelId");
		}
	}

	public bool LocationEnabled {
		get {
			return Call<bool> ("isLocationEnabled");
		}
		set {
			if (value) {
				Call ("enableLocation");
			} else {
				Call ("disableLocation");
			}
		}
	}

	public bool BackgroundLocationEnabled {
		get {
			return Call<bool> ("isBackgroundLocationEnabled");
		}
		set {
			if (value) {
				Call ("enableBackgroundLocation");
			} else {
				Call ("disableBackgroundLocation");
			}
		}
	}

	public string GetDeepLink (bool clear)
	{
		return Call<string> ("getDeepLink", clear);
	}

	public void AddListener (GameObject gameObject)
	{
		Call ("addListener", gameObject.name);
	}

	public void RemoveListener (GameObject gameObject)
	{
		Call ("removeListener", gameObject.name);
	}

	public string GetIncomingPush (bool clear)
	{
		return Call<string> ("getIncomingPush", clear);
	}

	public void AddTag (string tag)
	{
		Call ("addTag", tag);
	}

	public void RemoveTag (string tag)
	{
		Call ("removeTag", tag);
	}

	private void Call (string method, params object[] args)
	{
		if (androidPlugin != null) {
			androidPlugin.Call (method, args);
		}
	}

	private T Call<T> (string method, params object[] args)
	{
		if (androidPlugin != null) {
			return androidPlugin.Call<T> (method, args);
		}
		return default (T);
	}
}

#endif
