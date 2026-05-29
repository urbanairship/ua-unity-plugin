/* Copyright Airship and Contributors */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable annotations

namespace AirshipSDK
{

    /// <summary>
    /// Airship Message Center.
    /// </summary>
    public class AirshipMessageCenter
    {

        private IAirshipPlugin plugin;

        internal AirshipMessageCenter(IAirshipPlugin plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Gets the number of unread messages for the message center asynchronously using a coroutine.
        /// This method does not block Unity's main thread.
        /// </summary>
        /// <param name="onComplete">Callback invoked with the unread count when the operation completes.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator GetUnReadCount(Action<int> onComplete, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => plugin.Call<int>("getUnreadCount"),
                onComplete,
                onError
            );
        }

        /// <summary>
        /// Gets the inbox messages asynchronously using a coroutine.
        /// This method does not block Unity's main thread.
        /// </summary>
        /// <param name="onComplete">Callback invoked with the messages when the operation completes.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator GetMessages(Action<IEnumerable<InboxMessage>> onComplete, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => {
            var inboxMessages = new List<InboxMessage>();
            string inboxMessagesAsJson = plugin.Call<string>("getMessages");
            InternalInboxMessage[] internalInboxMessages = JsonArray<InternalInboxMessage>.FromJson(inboxMessagesAsJson).values;
            // Unity's JsonUtility doesn't support embedded dictionaries - constructor will create the extras dictionary
            foreach (InternalInboxMessage internalInboxMessage in internalInboxMessages)
            {
                inboxMessages.Add(new InboxMessage(internalInboxMessage));
            }
                    return (IEnumerable<InboxMessage>)inboxMessages;
                },
                onComplete,
                onError
            );
        }

        /// <summary>
        /// Mark an inbox message as having been read.
        /// </summary>
        /// <param name="messageId">The messageId for the message.</param>
        public void MarkMessageRead(string messageId)
        {
            plugin.Call("markMessageRead", messageId);
        }

        /// <summary>
        /// Delete an inbox message.
        /// </summary>
        /// <param name="messageId">The messageId for the message.</param>
        public void DeleteMessage(string messageId)
        {
            plugin.Call("deleteMessage", messageId);
        }

        /// <summary>
        /// Refreshes the inbox asynchronously using a coroutine.
        /// This method does not block Unity's main thread.
        /// </summary>
        /// <param name="onComplete">Optional callback invoked when the operation completes.</param>
        /// <param name="onError">Optional callback invoked if an error occurs.</param>
        /// <returns>A coroutine that can be started with StartCoroutine.</returns>
        public IEnumerator RefreshInbox(Action onComplete = null, Action<Exception> onError = null)
        {
            yield return AirshipCoroutineHelper.RunAsync(
                () => plugin.Call("refreshMessages"),
                onComplete,
                onError
            );
        }

        /// <summary>
        /// Sets the default behavior when the message center is launched from a push notification.
        /// </summary>
        /// <param name="enabled"><c>true</c> to automatically launch the default message center. If <c>false</c> the message center must be manually launched by the app.</param>
        public void SetAutoLaunchDefaultMessageCenter(bool enabled)
        {
            plugin.Call("setAutoLaunchDefaultMessageCenter", enabled);
        }

        /// <summary>
        /// Requests to display the Message Center.
        /// 
        /// Will either emit an event to display the Message Center if auto launch message center is disabled, or display the OOTB message center.
        /// </summary>
        /// <param name="messageId">Optional message Id.</param>
        public void Display(string? messageId)
        {
            plugin.Call("displayMessageCenter", messageId);
        }

        /// <summary>
        /// Dismisses the OOTB message center if displayed.
        /// </summary>
        public void Dismiss()
        {
            plugin.Call("dismissMessageCenter");
        }

        /// <summary>
        /// Overlays the message view. Should be used to display the actual message body in a custom Message Center.
        /// </summary>
        /// <param name="messageId">The message Id.</param>
        public void ShowMessageView(string messageId)
        {
            plugin.Call("showMessageView", messageId);
        }

        /// <summary>
        /// Overlays the message center regardless if auto launch Message Center is enabled or not.
        /// </summary>
        /// <param name="messageId">Optional message Id.</param>
        public void ShowMessageCenter(string? messageId)
        {
            plugin.Call("showMessageCenter", messageId);
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

        internal InboxMessage(string id, string title, long sentDate, bool isRead, bool isDeleted, Dictionary<string, string> extras)
        {
            this.id = id;
            this.title = title;
            this.sentDate = sentDate;
            this.isRead = isRead;
            this.isDeleted = isDeleted;
            this.extras = extras;
        }

        public InboxMessage(InternalInboxMessage internalInboxMessage)
        {
            sentDate = internalInboxMessage.sentDate;
            id = internalInboxMessage.id;
            title = internalInboxMessage.title;
            isRead = internalInboxMessage.isRead;
            isDeleted = internalInboxMessage.isDeleted;

            if (internalInboxMessage.extrasKeys != null && internalInboxMessage.extrasKeys.Count > 0)
            {
                // Unity's JsonUtility doesn't support embedded dictionaries - create the extras dictionary manually
                extras = new Dictionary<string, string>();
                for (int index = 0; index < internalInboxMessage.extrasKeys.Count; index++)
                {
                    extras[internalInboxMessage.extrasKeys[index]] = internalInboxMessage.extrasValues[index];
                }
            }
        }

        public override bool Equals(object other)
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

        public override int GetHashCode()
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
    public class InternalInboxMessage {
        public string id;
        public string title;
        public long sentDate;
        public bool isRead;
        public bool isDeleted;
        public List<string> extrasKeys;
        public List<string> extrasValues;
    }

    [Serializable]
    public class MessageCounts {
        public uint unread;
        public uint total;
    }
}