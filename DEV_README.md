# Building the plugin

To build the plugin from source, do the following:

1. Install doxygen, Android SDK, Xcode, and Unity
2. Build the plugin with `./gradlew build`

Docs will be available in `docs/build/html` and a unitypackage will be created in `build/`. If an error occurs, the unity.log
file is available in the build directory.

# Testing the plugin

Test the plugin the same way a customer would: build the `unitypackage`, import it into a
fresh project, and run the example behaviour on a device.

1. **Build the unitypackage**

   Run `./gradlew build` (see above). This produces `build/airship-<version>.unitypackage`,
   where `<version>` is the `version` value from [airship.properties](airship.properties).

2. **Create an empty test app**

   In Unity Hub, create a new empty project.

3. **Import the plugin**

   In the test project, go to `Assets -> Import Package -> Custom Package` and select the
   `build/airship-<version>.unitypackage` you just built. Import everything. This also
   imports the example script at `Assets/Scripts/AirshipBehaviour.cs` and the External
   Dependency Manager.

4. **Configure Airship**

   Either open `Window -> Airship -> Settings` and enter your app key/secret, or set them
   directly in the `TakeOff` call inside `Assets/Scripts/AirshipBehaviour.cs` (the example
   ships with placeholder credentials that should be replaced with your app keys).

5. **Add the example behaviour to a scene**

   In the test scene, create an empty GameObject (or select an existing one such as the Main
   Camera), then `Add Component -> AirshipBehaviour`. The behaviour calls `TakeOff` and
   exercises the main features (channel, tags, analytics, push, message center, etc.).

6. **Build and run on a device**

   - iOS: enable the Push Notifications capability and set up signing/provisioning, then
     build and run on a device.
   - Android: add a `google-services.json` to the `Assets` directory before building, then
     build and run on a device.

   Watch the device logs for the channel ID and the `Debug.Log` output from
   `AirshipBehaviour` to confirm the integration is working.

# Updating versions

All versions are centralized in [airship.properties](airship.properties).

### Plugin version

Update `version` in [airship.properties](airship.properties). It is substituted into
`PluginInfo.Version`, `AirshipDependencies.xml`, and the exported unitypackage file name.

When bumping the proxy, also confirm the matching Android `compileSdk`/`targetSdk`/`minSdk`
and Kotlin requirements still satisfy the new proxy release, and update them in
`airship.properties` if needed.
