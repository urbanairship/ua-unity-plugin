/* Copyright Airship and Contributors */

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
        internal GameObject gameObject = null;

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

        /// <summary>
        /// Inbox update event handler.
        /// </summary>
        public delegate void InboxUpdatedEventHandler (uint messageUnreadCount, uint messageCount);

        /// <summary>
        /// Occurs when the inbox updates.
        /// </summary>
        public event InboxUpdatedEventHandler OnInboxUpdated;

        /// <summary>
        /// Show inbox event handler.
        /// </summary>
        public delegate void ShowInboxEventHandler (string messageId);

        /// <summary>
        /// Occurs when the app needs to show the inbox.
        /// </summary>
        public event ShowInboxEventHandler OnShowInbox;

        internal static UAirship sharedAirship = new UAirship();

        /// <summary>
        /// Gets the shared UAirship instance.
        /// </summary>
        /// <value>The shared UAirship instance.</value>
        public static UAirship Shared {
            get {
                return sharedAirship;
            }
        }

        /// <summary>
        /// Creates a UAirship instance with a test plugin.
        /// Used only for testing.
        /// </summary>
        /// <param name="testPlugin">The test plugin.</param>
        internal UAirship (object testPlugin)
        {
            plugin = (UrbanAirship.IUAirshipPlugin)testPlugin;

            init();
        }

        /// <summary>
        /// Creates a UAirship instance.
        /// </summary>]
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

            init();
        }

        /// <summary>
        /// Initialize a UAirship instance.
        /// </summary>]
        private void init() { 
            gameObject = new GameObject ("[UrbanAirshipListener]");
            gameObject.AddComponent<UrbanAirshipListener> ();

            UnityEngine.Object.DontDestroyOnLoad(gameObject);
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
        /// Displays an inbox message.
        /// </summary>
        /// <param name="messageId">The messageId for the message.</param>
        public void DisplayInboxMessage (string messageId)
        {
            plugin.DisplayInboxMessage(messageId);
        }

        /// <summary>
        /// Refreshes the inbox.
        /// </summary>
        public void RefreshInbox () {
            plugin.RefreshInbox();
        }

        /// <summary>
        /// Gets the inbox messages.
        /// </summary>
        /// <value>An enumberable list of InboxMessage objects.</value>
        public IEnumerable<InboxMessage> InboxMessages () {
            var inboxMessages = new List<InboxMessage>();

            string inboxMessagesAsJson = plugin.InboxMessages();
            _InboxMessage[] internalInboxMessages = JsonArray<_InboxMessage>.FromJson(inboxMessagesAsJson).values;

            // Unity's JsonUtility doesn't support embedded dictionaries - constructor will create the extras dictionary
            foreach (_InboxMessage internalInboxMessage in internalInboxMessages)
            {
                inboxMessages.Add(new InboxMessage(internalInboxMessage));
            }
            return inboxMessages;
        }

        /// <summary>
        /// Mark an inbox message as having been read.
        /// </summary>
        /// <param name="messageId">The messageId for the message.</param>
        public void MarkInboxMessageRead (string messageId)
        {
            plugin.MarkInboxMessageRead(messageId);
        }

        /// <summary>
        /// Delete an inbox message.
        /// </summary>
        /// <param name="messageId">The messageId for the message.</param>
        public void DeleteInboxMessage (string messageId)
        {
            plugin.DeleteInboxMessage(messageId);
        }

        /// <summary>
        /// Sets the default behavior when the message center is launched from a push notification.
        /// </summary>
        /// <param name="enabled"><c>true</c> to automatically launch the default message center. If <c>false</c> the message center must be manually launched by the app.</param>

        public void SetAutoLaunchDefaultMessageCenter (bool enabled)
        {
            plugin.SetAutoLaunchDefaultMessageCenter(enabled);
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

        internal class UrbanAirshipListener : MonoBehaviour {
            void OnPushReceived (string payload) {
                PushReceivedEventHandler handler = UAirship.Shared.OnPushReceived;

                if (handler == null) {
                    return;
                }

                PushMessage pushMessage = PushMessage.FromJson(payload);
                if (pushMessage != null) {
                    handler(pushMessage);
                }
            }

            void OnPushOpened (string payload) {
                PushOpenedEventHandler handler = UAirship.Shared.OnPushOpened;

                if (handler == null) {
                    return;
                }

                PushMessage pushMessage = PushMessage.FromJson(payload);
                if (pushMessage != null) {
                    handler(pushMessage);
                }
            }

            void OnDeepLinkReceived (string deeplink) {
                DeepLinkReceivedEventHandler handler = UAirship.Shared.OnDeepLinkReceived;

                if (handler != null) {
                    handler(deeplink);
                }
            }

            void OnChannelUpdated (string channelId) {
                ChannelUpdateEventHandler handler = UAirship.Shared.OnChannelUpdated;

                if (handler != null) {
                    handler(channelId);
                }
            }

            internal void OnInboxUpdated (string counts) {
                InboxUpdatedEventHandler handler = UAirship.Shared.OnInboxUpdated;

                MessageCounts messageCounts = JsonUtility.FromJson<MessageCounts>(counts);

                if (handler != null)
                {
                    handler(messageCounts.unread, messageCounts.total);
                }

            }

            internal void OnShowInbox (string messageId) {
                ShowInboxEventHandler handler = UAirship.Shared.OnShowInbox;

                if (handler != null)
                {
                    if (messageId.Length == 0)
                    {
                        handler(null);
                    }
                    else
                    {
                        handler(messageId);
                    }
                }
            }
        }
    }

    public class InboxMessage
    {
        public readonly string id;
        public readonly string title;
        public readonly long sentDate;
        public readonly bool isRead;
        public readonly bool isDeleted;
        public readonly Dictionary<string, string> extras;

        internal InboxMessage (string id, string title, long sentDate, bool isRead, bool isDeleted, Dictionary<string, string> extras)
        {
            this.id = id;
            this.title = title;
            this.sentDate = sentDate;
            this.isRead = isRead;
            this.isDeleted = isDeleted;
            this.extras = extras;
        }

        public InboxMessage (_InboxMessage _inboxMessage)
        {
            sentDate = _inboxMessage.sentDate;
            id = _inboxMessage.id;
            title = _inboxMessage.title;
            isRead = _inboxMessage.isRead;
            isDeleted = _inboxMessage.isDeleted;

            if (_inboxMessage.extrasKeys != null && _inboxMessage.extrasKeys.Count > 0)
            {
                // Unity's JsonUtility doesn't support embedded dictionaries - create the extras dictionary manually
                extras = new Dictionary<string, string>();
                for (int index = 0; index < _inboxMessage.extrasKeys.Count; index++)
                {
                    extras[_inboxMessage.extrasKeys[index]] = _inboxMessage.extrasValues[index];
                }
            }
        }

        public override bool Equals (object other)
        {
            var that = other as InboxMessage;

            if (that == null)
            {
                return false;
            }

            if (this.id != that.id)
            {
                return false;
            }
            if (this.title != that.title)
            {
                return false;
            }
            if (this.sentDate != that.sentDate)
            {
                return false;
            }
            if (this.isRead != that.isRead)
            {
                return false;
            }
            if (this.isDeleted != that.isDeleted)
            {
                return false;
            }
            if ((this.extras == null ^ that.extras == null) ||
                ((this.extras != that.extras) &&
                 (this.extras.Count != that.extras.Count || this.extras.Except(that.extras).Any())))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode ()
        {
            unchecked
            {
                var hashCode = (id != null ? id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (title != null ? title.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ sentDate.GetHashCode();
                hashCode = (hashCode * 397) ^ isRead.GetHashCode();
                hashCode = (hashCode * 397) ^ isDeleted.GetHashCode();
                hashCode = (hashCode * 397) ^ (extras != null ? extras.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    [Serializable]
    public class _InboxMessage
    {
        public string id;
        public string title;
        public long sentDate;
        public bool isRead;
        public bool isDeleted;
        public List<string> extrasKeys;
        public List<string> extrasValues;
    }

    [Serializable]
    public class MessageCounts
    {
        public uint unread;
        public uint total;
    }
}
