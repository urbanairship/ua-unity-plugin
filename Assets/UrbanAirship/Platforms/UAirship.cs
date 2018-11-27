/*
 Copyright 2018 Urban Airship and Contributors
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UrbanAirship {

    /// <summary>
    /// The primary manager class for the Urban Airship plugin.
    /// </summary>
    public class UAirship {
        private IUAirshipPlugin plugin = null;

        /// <summary>
        /// Push received event handler.
        /// </summary>
        public delegate void PushReceivedEventHandler (PushMessage message);

        /// <summary>
        /// Occurs when a push is received.
        /// </summary>
        public event PushReceivedEventHandler OnPushReceived;

        /// <summary>
        /// Push opened event handler.
        /// </summary>
        public delegate void PushOpenedEventHandler (PushMessage message);

        /// <summary>
        /// Occurs when a push is opened.
        /// </summary>
        public event PushOpenedEventHandler OnPushOpened;

        /// <summary>
        /// Deep link received event handler.
        /// </summary>
        public delegate void DeepLinkReceivedEventHandler (string deeplink);

        /// <summary>
        /// Occurs when a deep link is received.
        /// </summary>
        public event DeepLinkReceivedEventHandler OnDeepLinkReceived;

        /// <summary>
        /// Channel update event handler.
        /// </summary>
        public delegate void ChannelUpdateEventHandler (string channelId);

        /// <summary>
        /// Occurs when the channel updates.
        /// </summary>
        public event ChannelUpdateEventHandler OnChannelUpdated;

        private static UAirship sharedAirship = new UAirship ();

        /// <summary>
        /// Gets the shared UAirship instance.
        /// </summary>
        /// <value>The shared UAirship instance.</value>
        public static UAirship Shared {
            get {
                return sharedAirship;
            }
        }

        private UAirship () {
            if (Application.isEditor) {
                plugin = new StubbedPlugin ();
            } else {
#if UNITY_ANDROID
                plugin = new UAirshipPluginAndroid ();
#elif UNITY_IOS
                plugin = new UAirshipPluginIOS ();
#else
                plugin = new StubbedPlugin ();
#endif
            }

            GameObject gameObject = new GameObject ("[UrbanAirshipListener]");
            gameObject.AddComponent<UrbanAirshipListener> ();

            MonoBehaviour.DontDestroyOnLoad (gameObject);
            plugin.Listener = gameObject;
        }

        /// <summary>
        /// Determines whether user notifications are enabled.
        /// </summary>
        /// <value><c>true</c> if user notifications are enabled; otherwise, <c>false</c>.</value>
        public bool UserNotificationsEnabled {
            get {
                return plugin.UserNotificationsEnabled;
            }
            set {
                plugin.UserNotificationsEnabled = value;
            }
        }

        /// <summary>
        /// Gets the tags currently set for the device.
        /// </summary>
        /// <value>The tags.</value>
        public IEnumerable<string> Tags {
            get {
                string tagsAsJson = plugin.Tags;
                JsonArray<string> jsonArray = JsonArray<string>.FromJson (tagsAsJson);
                return jsonArray.AsEnumerable ();
            }
        }

        /// <summary>
        /// Gets the channel identifier associated with the device.
        /// </summary>
        /// <value>The channel identifier.</value>
        public string ChannelId {
            get {
                return plugin.ChannelId;
            }
        }

        /// <summary>
        /// Determines whether location is enabled.
        /// </summary>
        /// <value><c>true</c> if location is enabled; otherwise, <c>false</c>.</value>
        public bool LocationEnabled {
            get {
                return plugin.LocationEnabled;
            }
            set {
                plugin.LocationEnabled = value;
            }
        }

        /// <summary>
        /// Determine whether background location is allowed.
        /// </summary>
        /// <value><c>true</c> if background location is allowed; otherwise, <c>false</c>.</value>
        public bool BackgroundLocationAllowed {
            get {
                return plugin.BackgroundLocationAllowed;
            }
            set {
                plugin.BackgroundLocationAllowed = value;
            }
        }

        /// <summary>
        /// Gets or sets the named user identifier.
        /// </summary>
        /// <value>The named user identifier.</value>
        public string NamedUserId {
            get {
                return plugin.NamedUserId;
            }
            set {
                plugin.NamedUserId = value;
            }
        }

        /// <summary>
        /// Gets the last received deep link.
        /// </summary>
        /// <returns>The deep link.</returns>
        /// <param name="clear">If set to <c>true</c> clear the stored deep link after accessing it.</param>
        public string GetDeepLink (bool clear = true) {
            return plugin.GetDeepLink (clear);
        }

        /// <summary>
        /// Gets the last stored incoming push message.
        /// </summary>
        /// <returns>The push message.</returns>
        /// <param name="clear">If set to <c>true</c> clear the stored push message after accessing it.</param>
        public PushMessage GetIncomingPush (bool clear = true) {
            string jsonPushMessage = plugin.GetIncomingPush (clear);
            if (String.IsNullOrEmpty (jsonPushMessage)) {
                return null;
            }

            PushMessage pushMessage = PushMessage.FromJson (jsonPushMessage);
            return pushMessage;
        }

        /// <summary>
        /// Adds the provided device tag.
        /// </summary>
        /// <param name="tag">The tag.</param>
        public void AddTag (string tag) {
            plugin.AddTag (tag);
        }

        /// <summary>
        /// Removes the provided device tag.
        /// </summary>
        /// <param name="tag">The tag.</param>
        public void RemoveTag (string tag) {
            plugin.RemoveTag (tag);
        }

        /// <summary>
        /// Adds a custom event.
        /// </summary>
        /// <param name="customEvent">The custom event.</param>
        public void AddCustomEvent (CustomEvent customEvent) {
            plugin.AddCustomEvent (customEvent.ToJson ());
        }

        /// <summary>
        /// Associate a custom identifier.
        /// Previous identifiers will be replaced by the new identifiers each time AssociateIdentifier is called.
        /// It is a set operation.
        /// </summary>
        /// <param name="key">The custom key for the identifier.</param>
        /// <param name="identifier">The value of the identifier, or `null` to remove the identifier.</param>
        public void AssociateIdentifier (string key, string identifier) {
            plugin.AssociateIdentifier (key, identifier);
        }

        /// <summary>
        /// Displays the message center.
        /// </summary>
        public void DisplayMessageCenter () {
            plugin.DisplayMessageCenter ();
        }

        /// <summary>
        /// Gets the number of unread messages for the message center.
        /// </summary>
        public int MessageCenterUnreadCount {
            get {
                return plugin.MessageCenterUnreadCount;
            }
        }

        /// <summary>
        /// Gets the number of messages for the message center.
        /// </summary>
        public int MessageCenterCount {
            get {
                return plugin.MessageCenterCount;
            }
        }

        /// <summary>
        /// Returns an editor for named user tag groups.
        /// </summary>
        /// <returns>A TagGroupEditor for named user tag groups.</returns>
        public TagGroupEditor EditNamedUserTagGroups () {
            return new TagGroupEditor ((string payload) => {
                plugin.EditNamedUserTagGroups (payload);
            });
        }

        /// <summary>
        /// Returns an editor for channel tag groups.
        /// </summary>
        /// <returns>A TagGroupEditor for channel tag groups.</returns>
        public TagGroupEditor EditChannelTagGroups () {
            return new TagGroupEditor ((string payload) => {
                plugin.EditChannelTagGroups (payload);
            });
        }

        private class UrbanAirshipListener : MonoBehaviour {
            void OnPushReceived (string payload) {
                PushReceivedEventHandler handler = UAirship.Shared.OnPushReceived;

                if (handler == null) {
                    return;
                }

                PushMessage pushMessage = PushMessage.FromJson (payload);
                if (pushMessage != null) {
                    handler (pushMessage);
                }
            }

            void OnPushOpened (string payload) {
                PushOpenedEventHandler handler = UAirship.Shared.OnPushOpened;

                if (handler == null) {
                    return;
                }

                PushMessage pushMessage = PushMessage.FromJson (payload);
                if (pushMessage != null) {
                    handler (pushMessage);
                }
            }

            void OnDeepLinkReceived (string deeplink) {
                DeepLinkReceivedEventHandler handler = UAirship.Shared.OnDeepLinkReceived;

                if (handler != null) {
                    handler (deeplink);
                }
            }

            void OnChannelUpdated (string channelId) {
                ChannelUpdateEventHandler handler = UAirship.Shared.OnChannelUpdated;

                if (handler != null) {
                    handler (channelId);
                }
            }
        }
    }
}
