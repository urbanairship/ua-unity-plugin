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
	
	public UAirshipPluginAndroid() {
		try {
			using(AndroidJavaClass pluginClass = new AndroidJavaClass("com.urbanairship.unityplugin.UnityPlugin")) {
				androidPlugin  = pluginClass.CallStatic<AndroidJavaObject>("shared");
			}
		} 
		catch (Exception) {
			Debug.LogError("UAirship plugin not found");
		}
	}

	public string GetDeepLink(bool clear)
	{
		return Call<string> ("getDeepLink", clear);
	}

	public void AddListener(GameObject gameObject) 
	{
		Call("addListener", gameObject.name);
	}
	
	public void RemoveListener(GameObject gameObject)
	{
		Call("removeListener", gameObject.name);
	}
	
	public string GetIncomingPush(bool clear)
	{
		return Call<string>("getIncomingPush", clear);
	}
	
	public bool IsPushEnabled()
	{
		return Call<bool>("isPushEnabled");
	}
	
	public void EnablePush()
	{
		Call("enablePush");
	}
	
	public void DisablePush()
	{
		Call("disablePush");
	}
	
	public void AddTag(string tag)
	{
		Call("addTag", tag);
	}
	
	public void RemoveTag(string tag)
	{
		Call("removeTag", tag);
	}
	
	public string GetTags()
	{
		return Call<string>("getTags");
	}
	
	public void SetAlias(string alias)
	{
		Call("setAlias", alias);
	}
	
	public string GetAlias()
	{
		return Call<string>("getAlias");
	}

	public string GetChannelId()
	{
		return Call<string>("getChannelId");
	}
	
	// Location
	
	public bool IsLocationEnabled()
	{
		return Call<bool>("isLocationEnabled");
	}
	
	public void EnableLocation ()
	{
		Call("enableLocation");
	}
	
	public void DisableLocation()
	{
		Call("disableLocation");
	}
	
	public bool IsBackgroundLocationEnabled()
	{
		return Call<bool>("isBackgroundLocationEnabled");
	}

	public void EnableBackgroundLocation()
	{
		Call("enableBackgroundLocation");
	}
	
	public void DisableBackgroundLocation()
	{
		Call("disableBackgroundLocation");
	}

	private void Call(string method, params object[] args)
	{
		if (androidPlugin != null)
		{
			androidPlugin.Call(method, args);
		}
	}

	private T Call<T>(string method, params object[] args) 
	{
		if (androidPlugin != null)
		{
			return androidPlugin.Call<T>(method, args);
		}
		return default (T);
	}
}

#endif
