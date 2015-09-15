Urban Airship Unity Plugin
==========================
A Unity plugin that integrates the iOS and Android Urban Airship SDK.

Contributing Code
-----------------
We accept pull requests! If you would like to submit a pull request, please fill out and submit a
Code Contribution Agreement (http://docs.urbanairship.com/contribution-agreement.html).

Third Party Packages
--------------------
 - mod_pbxproj.py - Apache License, Copyright 2012 Calvin Rien

Requirements
------------
 - Unity 5

Setup (Builds both Android and iOS plugins)
-------------------------------------------
1. Update gradle.properties with your app's configuration.
2. Install the Android SDK with the latest support repo and libaries installed. (Required to build the plugin, even if you are just using iOS)
3. Build the plugin with `./gradlew build`
4. Copy the output of build/unity-plugin/ into your unity app.

An example script is provided in 'Scripts/UrbanAirship.cs'. Import into your app's scripts and attach it to a game object in each scene. The script
shows a very basic integration with Urban Airship.

Known Issues
------------
- Landing Page does not pause the game on iOS.
- Analytics is not instrumented for Pre-ICS Android devices.
- The Unity Plugin will receive the entire push payload on iOS, while Android only sends the alert message.
- GetTags() is json encoded string.  

Supported Features
------------------
- Tags
- Aliases
- Push
- Location
- In-app messages
- Interactive notifications
- Urban Airship Actions - including landing pages

Currently Unsupported Features
------------------------------
- Named users
- Tag groups
- Custom events

Plugin Interface
---------------
The main plugin script can be found Assets/Plugins/UAirship.cs. It works for iOS and Android and will safely no-op on other platforms.

**public static void AddListener(GameObject gameObject)**
 - Adds a game object to listen for push notification that were received in the foreground.  The game object needs to implement OnPushReceived(string payload).

**public static void RemoveListener(GameObject gameObject)**
 - Removes a game object from receiveing push notifications.

**public static string GetDeepLink(bool clear = true)**
 - Gets the last triggered deep link if availble.
 - clear: whether or not to clear the deep link.

**public static string GetIncomingPush(bool clear = true)**
- Gets the launch push if available.
- clear: whether or not to clear the push.

**public static bool IsPushEnabled()**
- Checks if push is enabled or not.

**public static void EnablePush()**
- Enables push notificaitons.

**public static void DisablePush()**
- Disables push notifications.

**public static void AddTag(string tag)**
- Adds a tag.

**public static void RemoveTag(string tag)**
- Removes a tag.

**public static string GetTags()**
- Gets the current device tags.
- The tags will be a json encoded array.

**public static	void SetAlias(string alias)**
- Sets the alias.

**public static	string GetAlias()**
- Gets the alias.

**public static	string GetChannelId()**
- Gets the channel ID for device.

**public static	bool IsLocationEnabled()**
- Checks if location is enabled or not.

**public static	void EnableLocation()**
- Enables foreground location.

**public static	void DisableLocation()**
- Disables foreground location.

**public static	bool IsBackgroundLocationEnabled()**
- Checks if background location is enabled or not.

**public static	void EnableBackgroundLocation()**
- Enables background location.

**public static	void DisableBackgroundLocation()**
- Disables background location.

Project Structure
-----------------
- *android-plugin*: The Android native unity plugin.
- *ios-plugin*: The iOS native unity plugin.
- *src*: The common unity plugin source.
- *Scripts*: Example scripts for the plugin.


