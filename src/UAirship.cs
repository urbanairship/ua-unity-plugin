/*
 Copyright 2015 Urban Airship and Contributors
*/

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class UAirship
{

#if UNITY_ANDROID
	private static IUAirshipPlugin plugin = new UAirshipPluginAndroid();
#elif UNITY_IPHONE
	private static IUAirshipPlugin plugin = new UAirshipPluginIOS();
#else
	private static IUAirshipPlugin plugin = null;
#endif

	public static void AddListener(GameObject gameObject)
	{
		if (plugin != null)
		{
			plugin.AddListener(gameObject);
		}
	}

	public static void RemoveListener(GameObject gameObject) 
	{
		if (plugin != null)
		{
			plugin.RemoveListener(gameObject);
		}
	}

	public static string GetDeepLink(bool clear = true)
	{
		if (plugin != null) {
			return plugin.GetDeepLink(clear);
		}
		return null;
	}

	public static string GetIncomingPush(bool clear = true)
	{
		if (plugin != null)
		{
			return plugin.GetIncomingPush(clear);
		}
		return null;
	}
	
	public static bool IsPushEnabled()
	{
		if (plugin != null)
		{
			return plugin.IsPushEnabled();
		}
		return false;
	}
	
	public static void EnablePush()
	{
		if (plugin != null)
		{
			plugin.EnablePush();
		}
	}
	
	public static void DisablePush()
	{
		if (plugin != null)
		{
			plugin.DisablePush();
		}
	}
	
	public static void AddTag(string tag)
	{
		if (plugin != null)
		{
			plugin.AddTag(tag);
		}
	}
	
	public static void RemoveTag(string tag)
	{
		if (plugin != null)
		{
			plugin.RemoveTag(tag);
		}
	}
	
	// TODO: Decode the json array of tags
	public static string GetTags()
	{
		if (plugin != null)
		{
			return plugin.GetTags();
		}
		return null;
	}
	
	public static void SetAlias(string alias)
	{
		if (plugin != null)
		{
			plugin.SetAlias(alias);
		}
	}
	
	public static string GetAlias()
	{
		if (plugin != null)
		{
			return plugin.GetAlias();
		}
		return null;
	}

	public static string GetChannelId()
	{
		if (plugin != null)
		{
			return plugin.GetChannelId();
		}
		return null;
	}
	
	// Location
	
	public static bool IsLocationEnabled()
	{
		if (plugin != null)
		{
			return plugin.IsLocationEnabled();
		}
		return false;
	}
	
	public static void EnableLocation ()
	{
		if (plugin != null)
		{
			plugin.EnableLocation();
		}
	}
	
	public static void DisableLocation()
	{
		if (plugin != null)
		{
			plugin.DisableLocation();
		}
	}
	
	public static bool IsBackgroundLocationEnabled()
	{
		if (plugin != null)
		{
			return plugin.IsBackgroundLocationEnabled();
		}
		return false;
	}
	
	public static void EnableBackgroundLocation()
	{
		if (plugin != null)
		{
			plugin.EnableBackgroundLocation();
		}
	}
	
	public void DisableBackgroundLocation()
	{
		if (plugin != null)
		{
			plugin.DisableBackgroundLocation();
		}	
	}
}

