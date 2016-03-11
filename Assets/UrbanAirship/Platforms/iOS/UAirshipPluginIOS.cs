/*
 Copyright 2015 Urban Airship and Contributors
*/

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class UAirshipPluginIOS : IUAirshipPlugin
{

	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_addListener(string listener);
	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_removeListener(string listener);
	

	[DllImport ("__Internal")]
	private static extern string UAUnityPlugin_getDeepLink(bool clear);


	// PUSH FUNCTION IMPORTS
	// import a single C-function from our plugin
	[DllImport ("__Internal")]
	private static extern  string UAUnityPlugin_getIncomingPush(bool clear);
	[DllImport ("__Internal")]
	private static extern bool UAUnityPlugin_isPushEnabled();
	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_enablePush();
	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_disablePush();
	[DllImport ("__Internal")]
	private static extern string UAUnityPlugin_getTags();
	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_addTag(string tag);
	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_removeTag(string tag);
	[DllImport ("__Internal")]
	private static extern string UAUnityPlugin_getAlias();
	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_setAlias(string alias);
	[DllImport ("__Internal")]
	private static extern string UAUnityPlugin_getChannelId();

	//Location Function Imports
	[DllImport ("__Internal")]
	private static extern bool UAUnityPlugin_isLocationEnabled();
	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_enableLocation();
	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_disableLocation();

	[DllImport ("__Internal")]
	private static extern bool UAUnityPlugin_isBackgroundLocationEnabled();
	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_enableBackgroundLocation();
	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_disableBackgroundLocation();
	
	public string GetDeepLink(bool clear)
	{
		return UAUnityPlugin_getDeepLink(clear);
	}

	public void AddListener(GameObject gameObject) 
	{
		UAUnityPlugin_addListener(gameObject.name);
	}
	
	public void RemoveListener(GameObject gameObject)
	{
		UAUnityPlugin_removeListener (gameObject.name);
	}
	
	public string GetIncomingPush(bool clear)
	{
		return UAUnityPlugin_getIncomingPush (clear);
	}
	

	public bool IsPushEnabled()
	{
		return UAUnityPlugin_isPushEnabled();
	}
	
	public void EnablePush()
	{
		UAUnityPlugin_enablePush();
	}
	
	public void DisablePush()
	{
		UAUnityPlugin_disablePush();
	}
	
	public void AddTag(string tag)
	{
		UAUnityPlugin_addTag(tag);
	}
	
	public void RemoveTag(string tag)
	{
		UAUnityPlugin_removeTag(tag);
	}

	public string GetTags()
	{
		return UAUnityPlugin_getTags();
	}
	
	public void SetAlias(string alias)
	{
		UAUnityPlugin_setAlias(alias);
	}
	
	public string GetAlias()
	{
		return UAUnityPlugin_getAlias();
	}

	public string GetChannelId()
	{
		return UAUnityPlugin_getChannelId ();
	}

	// Location
	
	public bool IsLocationEnabled()
	{
		return UAUnityPlugin_isLocationEnabled ();
	}
	
	public void EnableLocation ()
	{
		UAUnityPlugin_enableLocation ();
	}
	
	public void DisableLocation()
	{
		UAUnityPlugin_disableLocation ();
	}
	
	public bool IsBackgroundLocationEnabled()
	{
		return UAUnityPlugin_isBackgroundLocationEnabled();
	}
	
	public void EnableBackgroundLocation()
	{
		UAUnityPlugin_enableBackgroundLocation();
	}
	
	public void DisableBackgroundLocation()
	{
		UAUnityPlugin_disableBackgroundLocation ();
	}
}