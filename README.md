# Urban Airship Unity Plugin

A Unity plugin that integrates the iOS and Android Urban Airship SDK.

[ ![Download](https://api.bintray.com/packages/urbanairship/unity/unity-plugin/images/download.svg) ](https://bintray.com/urbanairship/unity/unity-plugin/_latestVersion)

### Contributing Code
We accept pull requests! If you would like to submit a pull request, please fill out and submit our
[Contributor License Agreement](https://docs.google.com/forms/d/e/1FAIpQLScErfiz-fXSPpVZ9r8Di2Tr2xDFxt5MgzUel0__9vqUgvko7Q/viewform).

One of our engineers will verify receipt of the agreement before approving your pull request.

### Requirements
 - Unity 5
 - iOS - Xcode 10+ for iOS
 - Android - Android SDK installed and updated (requires Android MinSdkVersion = 16)

### Resources:
 - [Getting started guide](http://docs.urbanairship.com/platform/unity.html)
 - [API Docs](http://docs.urbanairship.com/reference/libraries/unity/latest/)
 - [Migration Guide](Documentation/migration-guide.md)

### Third Party Packages
 - Google Play Services Jar Resolver Library for Unity - Apache License

## Quickstart
1. [Download](https://bintray.com/urbanairship/unity/unity-plugin/_latestVersion) the latest plugin
2. Import Urban Airship unitypackage: In Unity, Assets -> Import Package -> Custom Package
3. Configure Urban Airship: In Unity, Window -> Urban Airship -> Settings

An example script is provided in 'Scripts/UrbanAirshipBehaviour.cs'. Import into your app's
scripts and attach it to a game object in each scene. The script shows a very basic
integration with Urban Airship.

### iOS
Enable Push Notifications in the project editor's Capabilities pane:

![Xcode's Project Editor Capabilities Pane](unity-enable-push.png)

To add support for iOS 10 notification attachments, you will need to create a
notification service extension. Detailed steps can be found
[here](http://docs.urbanairship.com/platform/unity.html#setup).

### Android
Download google-services.json into the `Assets` directory from the application's firebase console.
