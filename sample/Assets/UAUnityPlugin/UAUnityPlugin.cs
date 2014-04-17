using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;

public class UAUnityPlugin
{

	// PUSH FUNCTION IMPORTS
	// import a single C-function from our plugin
	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_takeOff();
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
	private static extern string UAUnityPlugin_getPushIDs();
	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_launchdefaultLandingPage();

	//Location Function Imports
	[DllImport ("__Internal")]
	private static extern bool UAUnityPlugin_isLocationEnabled();
	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_enableLocation();
	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_disableLocation();
	[DllImport ("__Internal")]
	private static extern bool UAUnityPlugin_isForegroundLocationEnabled();
	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_enableForegroundLocation();
	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_disableForegroundLocation();
	[DllImport ("__Internal")]
	private static extern bool UAUnityPlugin_isBackgroundLocationEnabled();
	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_enableBackgroundLocation();
	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_disableBackgroundLocation();

	// Push Function Wrappers
	public static void TakeOff() {
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_takeOff();
		}
	}

	public static bool IsPushEnabled() {
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_isPushEnabled();
		}
	}

	public static void EnablePush() {
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_enablePush();
		}
	}

	public static void DisablePush() {
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_disablePush();
		}
	}

	
	public static string GetTags() {
		if(Application.platform != RuntimePlatform.OSXEditor) {
			return UAUnityPlugin_getTags();
		}
		return "";
	}

	
	public static void addTag(string tag) {
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_addTag(tag);
		}
	}
	
	public static void removeTag(string tag) {
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_removeTag(tag);
		}
	}
	
	public static string getAlias() {
		if(Application.platform != RuntimePlatform.OSXEditor) {
			return UAUnityPlugin_getAlias();
		}
		return "";
	}
	
	public static void setAlias(string alias) {
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_setAlias(alias);
		}
	}
	
	public static string getPushIDs() {
		if(Application.platform != RuntimePlatform.OSXEditor) {
			return UAUnityPlugin_getPushIDs();
		}
		return "";
	}

	public static void launchdefaultLandingPage() {
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_launchdefaultLandingPage();
		}
	}

	//Location Function Wrappers
	
	static bool isLocationEnabled(){
		if(Application.platform != RuntimePlatform.OSXEditor) {
			return UAUnityPlugin_isLocationEnabled();
		}
	}
	
	static void enableLocation(){
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_enableLocation();
		}
	}
	
	static void disableLocation(){
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_disableLocation();
		}
	}
	
	static bool isForegroundLocationEnabled(){
		if(Application.platform != RuntimePlatform.OSXEditor) {
			return UAUnityPlugin_isForegroundLocationEnabled();
		}
	}
	
	static void enableForegroundLocation(){
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_enableForegroundLocation();
		}
	}
	
	static void disableForegroundLocation(){
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_disableForegroundLocation();
		}
	}
	
	static bool isBackgroundLocationEnabled(){
		if(Application.platform != RuntimePlatform.OSXEditor) {
			return UAUnityPlugin_isBackgroundLocationEnabled();
		}
	}
	
	static void enableBackgroundLocation(){
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_enableBackgroundLocation();
		}
	}
	
	static void disableBackgroundLocation(){
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_disableBackgroundLocation();
		}
	}

}