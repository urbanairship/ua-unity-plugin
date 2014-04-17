using UnityEngine;
using System;
using System.Runtime.InteropServices;

public class UAUnityPlugin
{
	// import a single C-function from our plugin
	[DllImport ("__Internal")]
	private static extern void UAUnityPlugin_takeOff();
	
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

	public static void takeOff() {
		// it won't work in Editor, so don't run it there
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_takeOff();
		}
	}

	public static void enablePush() {
		// it won't work in Editor, so don't run it there
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_enablePush();
		}
	}

	public static void disablePush() {
		// it won't work in Editor, so don't run it there
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_disablePush();
		}
	}

	
	public static string getTags() {
		// it won't work in Editor, so don't run it there
		if(Application.platform != RuntimePlatform.OSXEditor) {
			return UAUnityPlugin_getTags();
		}
		return "";
	}

	
	public static void addTag(string tag) {
		// it won't work in Editor, so don't run it there
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_addTag(tag);
		}
	}
	
	public static void removeTag(string tag) {
		// it won't work in Editor, so don't run it there
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_removeTag(tag);
		}
	}
	
	public static string getAlias() {
		// it won't work in Editor, so don't run it there
		if(Application.platform != RuntimePlatform.OSXEditor) {
			return UAUnityPlugin_getAlias();
		}
		return "";
	}
	
	public static void setAlias(string alias) {
		// it won't work in Editor, so don't run it there
		if(Application.platform != RuntimePlatform.OSXEditor) {
			UAUnityPlugin_setAlias(alias);
		}
	}
	
	public static string getPushIDs() {
		// it won't work in Editor, so don't run it there
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

}