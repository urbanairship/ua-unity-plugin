/*
 Copyright 2015 Urban Airship and Contributors
*/

using UnityEngine;
using System.Collections;
using UrbanAirship;

public class UrbanAirshipBehaviour : MonoBehaviour
{
	public string addTagOnStart;

	void Awake ()
	{
		UAirship.UserNotificationsEnabled = true;
	}

	void Start ()
	{

		if (!string.IsNullOrEmpty (addTagOnStart)) {
			UAirship.AddTag (addTagOnStart);
		}

		UAirship.OnPushReceived += OnPushReceived;
		CheckDeepLink ();
	}

	void OnDestroy ()
	{
		UAirship.OnPushReceived -= OnPushReceived;
	}

	void OnApplicationPause (bool pauseStatus)
	{
		if (!pauseStatus) {
			CheckDeepLink ();
		}
	}

	void OnPushReceived(PushMessage message) {
		Debug.Log ("Unity received push! " + message);
	}
	
	void CheckDeepLink ()
	{
		Debug.Log ("Checking for deeplink.");

		string deepLink = UAirship.GetDeepLink (true);
		if (!string.IsNullOrEmpty (deepLink)) {
			Debug.Log ("Launched with deeplink! " + deepLink);
			// Assume everything is a Bonus level for now
			Application.LoadLevel ("Bonus");
		}
	}
}