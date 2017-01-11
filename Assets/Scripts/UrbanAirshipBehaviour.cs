/*
 Copyright 2017 Urban Airship and Contributors
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UrbanAirship;

public class UrbanAirshipBehaviour : MonoBehaviour
{
	public string addTagOnStart;

	void Awake ()
	{
		UAirship.Shared.UserNotificationsEnabled = true;
	}

	void Start ()
	{

		if (!string.IsNullOrEmpty (addTagOnStart)) {
			UAirship.Shared.AddTag (addTagOnStart);
		}

		UAirship.Shared.OnPushReceived += OnPushReceived;
		UAirship.Shared.OnChannelUpdated += OnChannelUpdated;
		UAirship.Shared.OnDeepLinkReceived += OnDeepLinkReceived;
	}

	void OnDestroy ()
	{
		UAirship.Shared.OnPushReceived -= OnPushReceived;
		UAirship.Shared.OnChannelUpdated -= OnChannelUpdated;
		UAirship.Shared.OnDeepLinkReceived -= OnDeepLinkReceived;
	}

	void OnPushReceived(PushMessage message) {
		Debug.Log ("Received push! " + message.Alert);

		if (message.Extras != null) {
			foreach (KeyValuePair<string, string> kvp in message.Extras) {
				Debug.Log (string.Format ("Extras Key = {0}, Value = {1}", kvp.Key, kvp.Value));
			}
		}
	}

	void OnChannelUpdated(string channelId) {
		Debug.Log ("Channel updated: " + channelId);
	}

	void OnDeepLinkReceived(string deeplink) {
		Debug.Log ("Received deep link: " + deeplink);
	}
}
