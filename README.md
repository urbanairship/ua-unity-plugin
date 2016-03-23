# Urban Airship Unity Plugin

A Unity plugin that integrates the iOS and Android Urban Airship SDK.

### Contributing Code
We accept pull requests! If you would like to submit a pull request, please fill out and submit a
Code Contribution Agreement (http://docs.urbanairship.com/contribution-agreement.html).

### Requirements
 - Unity 5
 - iOS - Xcode 7+ for iOS
 - Android - Android SDK installed and updated

### Resources:
 - [Getting started guide](http://docs.urbanairship.com/platform/unity.html)
 - [API Docs](http://docs.urbanairship.com/reference/libraries/unity/latest/)

### Third Party Packages
 - Google Play Services Jar Resolver Library for Unity - Apache License

## Setup
1. [Download](https://bintray.com/urbanairship/unity/unity-plugin/_latestVersion) the latest plugin
2. Import Urban Airship unitypackage: In Unity, Assets -> Import Package -> Custom Package
3. Configure Urban Airship: In Unity, Window -> Urban Airship -> Settings

An example script is provided in 'Scripts/UrbanAirshipBehaviour.cs'. Import into your app's scripts and attach it to a game object in each scene. The script shows a very basic integration with Urban Airship.


## Building the Plugin
1. Install doxygen, Android SDK, Xcode, and Unity
2. Build the plugin with `./gradlew clean build`

Docs will be available in `docs/build/html` and a unitypackage will be created in `build/`. If an error occurs, the unity.log
file is available in the build directory.
