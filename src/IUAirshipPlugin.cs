/*
 Copyright 2015 Urban Airship and Contributors
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

interface IUAirshipPlugin
{

	void AddListener(GameObject gameObject);

	void RemoveListener(GameObject gameObject);

	// Push 
	string GetDeepLink(bool clear);

	string GetIncomingPush(bool clear);
	
	bool IsPushEnabled();

	void EnablePush();

	void DisablePush();

	void AddTag(string tag);

	void RemoveTag(string tag);

	string GetTags();

	void SetAlias(string alias);

	string GetAlias();

	string GetChannelId();


	// Location

	bool IsLocationEnabled();

	void EnableLocation();

	void DisableLocation();

	bool IsBackgroundLocationEnabled();

	void EnableBackgroundLocation();

	void DisableBackgroundLocation();
}

