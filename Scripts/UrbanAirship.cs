/*
 Copyright 2015 Urban Airship and Contributors
*/

using UnityEngine;
using System.Collections;

public class UrbanAirship : MonoBehaviour {

	public string addTagOnStart;

	void Awake()
	{
		UAirship.EnablePush ();
	}

	void Start()
	{
		UAirship.AddListener (gameObject);

		if (!string.IsNullOrEmpty(addTagOnStart)) 
		{
			UAirship.AddTag(addTagOnStart);
		}

		CheckDeepLink ();
	}

	void OnDestroy()
	{
		UAirship.RemoveListener (gameObject);
	}

	void OnPushReceived(string payload)
	{
		Debug.Log ("Unity received push! " + payload);
	}

	void OnApplicationPause(bool pauseStatus) {
		if (!pauseStatus) 
		{
			CheckDeepLink();
		}
	}

	void CheckDeepLink()
	{
		Debug.Log ("Checking for deeplink.");

		string deepLink = UAirship.GetDeepLink();
		if (!string.IsNullOrEmpty(deepLink))
		{
			Debug.Log ("Launched with deeplink! " + deepLink);

			// Handle any deep links here
		}
	}
}