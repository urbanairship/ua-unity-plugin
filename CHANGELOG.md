# Unity Plugin ChangeLog

## Version 9.2.0 - August 31, 2022

Minor release that updates to the latest SDK versions.

### Changes
- Updated Android Airship SDK to 16.7.1
- Updated iOS Airship SDK to 16.9.2

## Version 9.1.0 - May 11, 2022

Minor release that updates to the latest SDK versions.

### Changes
- Updated Android Airship SDK to 16.4.0
- Updated iOS Airship SDK to 16.6.0

## Version 9.0.3 - April 6, 2022

Patch release that fixes an other iOS crash related to channel update event.

### Changes
- Fix iOS crash when channel is updated

## Version 9.0.2 - March 15, 2022

Patch release that fixes an iOS crash related to push registration.

### Changes
- Fix iOS crash during registration

## Version 9.0.1 - March 1, 2022

Patch release that fixes an iOS error related to a wrong method name.

### Changes
- Fix iOS plugin build error

## Version 9.0.0 - December 2, 2021

Major release that updates Airship Android SDK to 16.1.0 and iOS SDK to 16.1.1. This version adds the Privacy Manager and the Preference Center. The Location module
is removed.

### Changes
- Updated Android Airship SDK to 16.1.0
- Updated iOS Airship SDK to 16.1.1
- Added Privacy Manager support
- Removed the Location module

## Version 8.1.0 - January 15, 2020

Minor release that adds support for In-App Automation message limits and segments.

### Changes
- Updated iOS SDK to 14.2.2
- Updated Android SDK to 14.1.1

## Version 8.0.1 - October 1, 2020

Patch release fixing a crash related to channel update events in the iOS plugin. Apps currently
on version 8.0.0 should update, and when upgrading previous plugin versions to 8.x, 8.0.0 should
be avoided.  

## Version 8.0.0 - September 25, 2020

Major release that updates Airship Android SDK to 14.0.1 and iOS SDK to 14.1.2. Starting with SDK 14, all landing page and external urls are tested against a URL allow list. The easiest way to go back to previous behavior is to add the wildcard symbol * in the Scope Open in URL Allow List in the Airship Config window.

### Changes
- Xcode 12 is now required.
- Updated to latest iOS and Android SDK.
- Added config for URL allow lists.
- Fixed Android build issues with latest Unity.


## Version 7.0.1 - March 23, 2020
Patch addressing a regression in iOS SDK 13.1.0 causing channel tag loss
when upgrading from iOS SDK versions prior to 13.0.1. Apps upgrading from Unity plugin
6.2.0 or below should avoid plugin version 7.0.0 in favor of version 7.0.1.

- Updated iOS SDK to 13.1.1

## Version 7.0.0 - March 11, 2020
- Updated Android Airship SDK to 12.2.2.
- Updated iOS Airship SDK to 13.1.0.
- Added screen tracking.
- Added support for channel attributes.
- Added properties to control data collection.
- Added In-App Automation properties to pause/resume message displays and set the display interval.
- Added additional Message Center accessors.

## Version 6.2.0 - October 9, 2019
- Updated Android Airship SDK to 10.1.3.
- Updated Android Airship SDK to 11.1.2.
- Added support for EU cloud site.

## Version 6.1.1 - August 9, 2019
- Updated Android Airship SDK to 10.1.1

## Version 6.1.0 - August 6, 2019
- Updated Android Airship SDK to 10.1.0
- Updated iOS Airship SDK to 11.1.0
- Updated Jar Resolver to 1.2.124
- Fixed build issues with Android
- Removed setting sender ID from config. Sender ID will be pulled from the
  google-services.json file.

## Version 6.0.2 - April 19, 2019
- Updated Airship iOS SDK to 10.2.2

## Version 6.0.1 - March 14, 2019
Fixed a security issue within Android Urban Airship SDK, that could allow
trusted URL redirects in certain edge cases. All applications that are using
Urban Airship Unity plugin for Android version 5.0.0 - 6.0.0 should update
as soon as possible. For more details, please email security@urbanairship.com.

## Version 6.0.0 - November 20, 2018
- Updated Urban Airship iOS SDK to 10.0.3
- Updated Urban Airship Android SDK to 9.5.6
- Removed ability to set/get an alias. Use named user instead.

## Version 5.1.0 - August 8, 2018
- Updated Urban Airship iOS SDK to 9.3.3
- Updated Urban Airship Android SDK to 9.4.2
- Updated Google Play Resolver to 1.2.80

## Version 5.0.0 - May 22, 2018
- Updated Urban Airship iOS SDK to 9.1.0
- Updated Urban Airship Android SDK to 9.2.0
- Updated Google Play Resolver to 1.2.71
- Android now uses FCM APIs and requires google-services.json config file

## Version 4.4.0 - March 13, 2018
- Fixed tag groups adding quotes around tags on Android.
- Added OnPushOpened listener.

## Version 4.3.3 - October 24, 2017
- Updated Urban Airship Android SDK to 8.9.4
- Updated Urban Airship iOS SDK to 8.6.1
- Fixed conflict with Facebook Plugin due to Support Library 26.1.0.

## Version 4.3.2 - October 20, 2017
 - Updated Urban Airship Android SDK to 8.9.3
 - Fixed plugin not resolving the Urban Airship plugin lib for Android

## Version 4.3.1 - October 4, 2017
 - Fixed plugin adding files into `Assets/plugins` instead of `Assets/Plugins`

## Version 4.3.0 - October 2, 2017
 - Updated Urban Airship iOS SDK to 8.6.0
 - Updated Urban Airship Android SDK to 8.9.0
 - Added option to set the production and development GCM/FCM sender ID
 - Added script that cleans up old plugin installations
 - Android notification icon now supports the resource name instead of the
   resource reference to look up the resource at runtime
 - Fixed building with Unity's gradle build option
 - Fixed plugin warnings
 - Reverted use of CocoaPods for Urban Airship SDK installation

## Version 4.2.0 - August 10, 2017
 - Updated Urban Airship iOS SDK to 8.5.2
 - Updated Urban Airship Android SDK to 8.8.2

## Version 4.1.0 - June 22, 2017
 - Updated Urban Airship Android SDK to 8.6.0 (Android O support)
 - Updated Jar Resolver to 1.2.29

## Version 4.0.0 - May 31, 2017
 - Updated Urban Airship iOS SDK to 8.3.3
 - Updated Urban Airship Android SDK to 8.5.1
 - iOS now requires CocoaPods for plugin installation.

## Version 3.3.1 - January 26, 2017
 - Updated Urban Airship iOS Library to 8.1.6

## Version 3.3.0 - January 25, 2017
 - Removed platform-dependent build requirements
 - Updated the JarResolver dependency to 1.2.9
 - Added a method for retrieving the number of message center messages

## Version 3.2.2 - January 10, 2017
 - Fix null Extras in PushMessage on iOS

## Version 3.2.1 - December 8, 2016
 - Updated Urban Airship iOS Library to 8.1.4

## Version 3.2.0 - December 6, 2016
 - Updated Urban Airship iOS Library to 8.1.3
 - Updated Urban Airship Android library to 8.2.1

## Version 3.1.0 - October 4, 2016
 - Updated Urban Airship iOS Library to 8.0.2
 - Added support for associated identifiers
 - Added iOS 10 foreground presentation support

## Version 3.0.0 - September 26, 2016
 - Updated Urban Airship iOS Library to 8.0.1 (requires Xcode 8)
 - Updated Urban Airship Android library to 8.0.1 (requires android MinSdkVersion = 16)

## Version 2.3.0 - June 24, 2016
 - Updated Urban Airship Android library to 7.2.0
 - Updated the JarResolver dependency to 1.2.0
 - Removed the Android Application override. Existing installations may need to
   remove the UrbanAirshipApplication entry from Assets/Plugins/Android/AndroidManifest.xml.

## Version 2.2.0 - June 2, 2016
 - Updated Urban Airship iOS Library to 7.2.0
 - Updated Urban Airship Android library to 7.1.5

## Version 2.1.1 - May 11, 2016
 - Updated Urban Airship Android Library to 7.1.3
 - Updated Urban Airship iOS library to 7.1.2

## Version 2.1.0 - May 3, 2016
 - Updated Urban Airship Android Library to 7.1.2
 - Updated Urban Airship iOS library to 7.1.0

## Version 2.0.0 - March 23, 2016

### New Features
  - Updated interface to use C# Properties and Events
  - Full push payload is exposed through the PushMessage model object
  - Tags now return a list of strings instead of a JSON encoded string
  - Added channel registration events
  - Added Named User API
  - Added Tag Group API
  - Added Custom Event API
  - Added Message Center support
  - Added support to set the Android Notification icon and accent color

### Packaging Changes
 - Plugin is now prepackaged in a unitypackage file
 - GUI Editor to set Urban Airship Configuration (Window -> Urban Airship -> Settings)
 - Uses Google Play Services Jar Resolver Library for Android dependency management
 - Added generated API docs - http://docs.urbanairship.com/reference/libraries/unity/latest/

### Bug Fixes
 - Landing pages now pause iOS
 - Fixed iOS build issues

## Version 1.2.1 - January 4, 2016
 - Fix iOS post build processor in Unity 5.3.

## Version 1.2.0 - October 20, 2015
 - Updated Urban Airship iOS and Android Library to 6.3.0

## Version 1.1.1 - October 12, 2015
 - Fixed issue where iOS devices did not notify listeners when it received pushes.
 - Fixed issue where isPushEnabled was not checking the user notifications enabled flag on Android.

## Version 1.1.0 - August 28, 2015
 - Updated Urban Airship Android Library to 6.2.2
 - Updated Urban Airship iOS library to 6.2.0

## Version 1.0.1 - January 28, 2015
 - Updated Urban Airship Android Library to 5.1.5

## Version 1.0.0 - January 26, 2015
 - Initial plugin release
