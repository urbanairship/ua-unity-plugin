UA Unity ChangeLog
==================

Version 4.3.3 - October 24, 2017
================================
- Updated Urban Airship Android SDK to 8.9.4
- Updated Urban Airship iOS SDK to 8.6.1
- Fixed conflict with Facebook Plugin due to Support Library 26.1.0.

Version 4.3.2 - October 20, 2017
================================
 - Updated Urban Airship Android SDK to 8.9.3
 - Fixed plugin not resolving the Urban Airship plugin lib for Android

Version 4.3.1 - October 4, 2017
===============================
 - Fixed plugin adding files into `Assets/plugins` instead of `Assets/Plugins`

Version 4.3.0 - October 2, 2017
===============================
 - Updated Urban Airship iOS SDK to 8.6.0
 - Updated Urban Airship Android SDK to 8.9.0
 - Added option to set the production and development GCM/FCM sender ID
 - Added script that cleans up old plugin installations
 - Android notification icon now supports the resource name instead of the
   resource reference to look up the resource at runtime
 - Fixed building with Unity's gradle build option
 - Fixed plugin warnings
 - Reverted use of CocoaPods for Urban Airship SDK installation

Version 4.2.0 - August 10, 2017
===============================
 - Updated Urban Airship iOS SDK to 8.5.2
 - Updated Urban Airship Android SDK to 8.8.2

Version 4.1.0 - June 22, 2017
=============================
 - Updated Urban Airship Android SDK to 8.6.0 (Android O support)
 - Updated Jar Resolver to 1.2.29

Version 4.0.0 - May 31, 2017
============================
 - Updated Urban Airship iOS SDK to 8.3.3
 - Updated Urban Airship Android SDK to 8.5.1
 - iOS now requires CocoaPods for plugin installation.

Version 3.3.1 - January 26, 2017
================================
 - Updated Urban Airship iOS Library to 8.1.6

Version 3.3.0 - January 25, 2017
================================
 - Removed platform-dependent build requirements
 - Updated the JarResolver dependency to 1.2.9
 - Added a method for retrieving the number of message center messages

Version 3.2.2 - January 10, 2017
================================
 - Fix null Extras in PushMessage on iOS

Version 3.2.1 - December 8, 2016
================================
 - Updated Urban Airship iOS Library to 8.1.4

Version 3.2.0 - December 6, 2016
================================
 - Updated Urban Airship iOS Library to 8.1.3
 - Updated Urban Airship Android library to 8.2.1

Version 3.1.0 - October 4, 2016
===============================
 - Updated Urban Airship iOS Library to 8.0.2
 - Added support for associated identifiers
 - Added iOS 10 foreground presentation support

Version 3.0.0 - September 26, 2016
==================================
 - Updated Urban Airship iOS Library to 8.0.1 (requires Xcode 8)
 - Updated Urban Airship Android library to 8.0.1 (requires android MinSdkVersion = 16)

Version 2.3.0 - June 24, 2016
=============================
 - Updated Urban Airship Android library to 7.2.0
 - Updated the JarResolver dependency to 1.2.0
 - Removed the Android Application override. Existing installations may need to
   remove the UrbanAirshipApplication entry from Assets/Plugins/Android/AndroidManifest.xml.

Version 2.2.0 - June 2, 2016
============================
 - Updated Urban Airship iOS Library to 7.2.0
 - Updated Urban Airship Android library to 7.1.5

Version 2.1.1 - May 11, 2016
============================
 - Updated Urban Airship Android Library to 7.1.3
 - Updated Urban Airship iOS library to 7.1.2

Version 2.1.0 - May 3, 2016
===========================
 - Updated Urban Airship Android Library to 7.1.2
 - Updated Urban Airship iOS library to 7.1.0

Version 2.0.0 - March 23, 2016
==============================

New Features
------------
  - Updated interface to use C# Properties and Events
  - Full push payload is exposed through the PushMessage model object
  - Tags now return a list of strings instead of a JSON encoded string
  - Added channel registration events
  - Added Named User API
  - Added Tag Group API
  - Added Custom Event API
  - Added Message Center support
  - Added support to set the Android Notification icon and accent color

Packaging Changes
-----------------
 - Plugin is now prepackaged in a unitypackage file
 - GUI Editor to set Urban Airship Configuration (Window -> Urban Airship -> Settings)
 - Uses Google Play Services Jar Resolver Library for Android dependency management
 - Added generated API docs - http://docs.urbanairship.com/reference/libraries/unity/latest/

Bug Fixes
---------
 - Landing pages now pause iOS
 - Fixed iOS build issues

Version 1.2.1 - January 4, 2016
===============================
 - Fix iOS post build processor in Unity 5.3.

Version 1.2.0 - October 20, 2015
================================
 - Updated Urban Airship iOS and Android Library to 6.3.0

Version 1.1.1 - October 12, 2015
================================
 - Fixed issue where iOS devices did not notify listeners when it received pushes.
 - Fixed issue where isPushEnabled was not checking the user notifications enabled flag on Android.

Version 1.1.0 - August 28, 2015
================================
 - Updated Urban Airship Android Library to 6.2.2
 - Updated Urban Airship iOS library to 6.2.0

Version 1.0.1 - January 28, 2015
================================
 - Updated Urban Airship Android Library to 5.1.5

Version 1.0.0 - January 26, 2015
================================
 - Initial plugin release
