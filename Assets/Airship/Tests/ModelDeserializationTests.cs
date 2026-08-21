/* Copyright Airship and Contributors */

using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using AirshipSDK;

namespace AirshipSDK.Tests {

    /// <summary>
    /// Covers models that arrive from the native layers. Unity's JsonUtility cannot map a
    /// JSON string onto an enum field, cannot instantiate abstract types and has no
    /// dictionary support, so every model here is shaped around those limits.
    /// </summary>
    [TestFixture]
    public class ModelDeserializationTests {

        // --- Notification status: string-valued enum ---

        [Test]
        public void NotificationPermissionStatusIsParsedFromItsStringValue () {
            PushNotificationStatus status = JsonUtility.FromJson<PushNotificationStatus> (
                "{\"isUserNotificationsEnabled\":true,\"isOptedIn\":true," +
                "\"notificationPermissionStatus\":\"denied\"}");

            Assert.IsTrue (status.isUserNotificationsEnabled);
            Assert.AreEqual (PermissionStatus.Denied, status.NotificationPermissionStatus);
        }

        [Test]
        public void NotificationPermissionStatusHandlesNotDetermined () {
            PushNotificationStatus status = JsonUtility.FromJson<PushNotificationStatus> (
                "{\"notificationPermissionStatus\":\"not_determined\"}");

            Assert.AreEqual (PermissionStatus.NotDetermined, status.NotificationPermissionStatus);
        }

        /// A missing or unrecognized status must not read as Granted.
        [Test]
        public void NotificationPermissionStatusDefaultsToNotDetermined () {
            PushNotificationStatus missing = JsonUtility.FromJson<PushNotificationStatus> (
                "{\"isOptedIn\":false}");
            Assert.AreEqual (PermissionStatus.NotDetermined, missing.NotificationPermissionStatus);
        }

        // --- Inbox messages ---

        [Test]
        public void InboxMessageExtrasComeFromTheParallelArrays () {
            InternalInboxMessage internalMessage = new InternalInboxMessage {
                id = "m1",
                title = "Hello",
                sentDate = 1700000000000L,
                isRead = true,
                extrasKeys = new List<string> { "k1", "k2" },
                extrasValues = new List<string> { "v1", "v2" }
            };

            InboxMessage message = new InboxMessage (internalMessage);

            Assert.AreEqual ("m1", message.id);
            Assert.AreEqual ("Hello", message.title);
            Assert.AreEqual (1700000000000L, message.sentDate);
            Assert.IsTrue (message.isRead);
            Assert.IsNotNull (message.extras);
            Assert.AreEqual ("v1", message.extras["k1"]);
            Assert.AreEqual ("v2", message.extras["k2"]);
        }

        [Test]
        public void InboxMessageWithoutExtrasHasNullExtras () {
            InboxMessage message = new InboxMessage (new InternalInboxMessage { id = "m1", title = "t" });

            Assert.IsNull (message.extras);
        }

        // --- Preference center config ---

        private const string PreferenceCenterJson =
            "{\"id\":\"pc-1\"," +
            "\"display\":{\"name\":\"Preferences\",\"description\":\"Manage them\"}," +
            "\"sections\":[" +
              "{\"type\":\"section\",\"id\":\"s1\",\"display\":{\"name\":\"Section one\"}," +
               "\"items\":[" +
                 "{\"type\":\"channel_subscription\",\"id\":\"i1\",\"subscription_id\":\"news\"," +
                  "\"display\":{\"name\":\"News\"}}," +
                 "{\"type\":\"contact_subscription\",\"id\":\"i2\",\"subscription_id\":\"promos\"," +
                  "\"scopes\":[\"app\",\"email\"],\"display\":{\"name\":\"Promos\"}," +
                  "\"conditions\":[{\"type\":\"notification_opt_in\",\"when_status\":\"opt_out\"}]}," +
                 "{\"type\":\"alert\",\"id\":\"i3\"," +
                  "\"display\":{\"name\":\"Enable push\",\"icon\":\"https://example.com/i.png\"}," +
                  "\"button\":{\"text\":\"Fix\",\"content_description\":\"Fix it\"}}]}," +
              "{\"type\":\"labeled_section_break\",\"id\":\"s2\",\"display\":{\"name\":\"Break\"}}]}";

        [Test]
        public void PreferenceCenterConfigDeserializes () {
            PreferenceCenterConfig config =
                JsonUtility.FromJson<PreferenceCenterConfig> (PreferenceCenterJson);

            Assert.IsNotNull (config);
            Assert.AreEqual ("pc-1", config.id);
            Assert.AreEqual ("Preferences", config.display.name);
            Assert.AreEqual ("Manage them", config.display.description);
            Assert.IsNotNull (config.sections, "sections must not be null");
            Assert.AreEqual (2, config.sections.Count);
        }

        [Test]
        public void PreferenceCenterSectionTypesAreParsed () {
            PreferenceCenterConfig config =
                JsonUtility.FromJson<PreferenceCenterConfig> (PreferenceCenterJson);

            Assert.AreEqual (PreferenceCenterSectionType.Section, config.sections[0].Type);
            Assert.AreEqual (PreferenceCenterSectionType.LabeledSectionBreak, config.sections[1].Type);
            Assert.AreEqual ("Section one", config.sections[0].display.name);
        }

        [Test]
        public void PreferenceCenterItemsAreParsedByType () {
            PreferenceCenterConfig config =
                JsonUtility.FromJson<PreferenceCenterConfig> (PreferenceCenterJson);
            List<PreferenceCenterItem> items = config.sections[0].items;

            Assert.AreEqual (3, items.Count);

            Assert.AreEqual (PreferenceCenterItemType.ChannelSubscription, items[0].Type);
            Assert.AreEqual ("news", items[0].SubscriptionId);
            Assert.AreEqual ("News", items[0].display.name);

            Assert.AreEqual (PreferenceCenterItemType.ContactSubscription, items[1].Type);
            Assert.AreEqual ("promos", items[1].SubscriptionId);
            Assert.AreEqual (new List<string> { "app", "email" }, items[1].scopes);

            Assert.AreEqual (PreferenceCenterItemType.Alert, items[2].Type);
            Assert.AreEqual ("Fix", items[2].button.text);
            Assert.AreEqual ("Fix it", items[2].button.ContentDescription);
            Assert.AreEqual ("https://example.com/i.png", items[2].display.icon);
        }

        [Test]
        public void PreferenceCenterConditionsAreParsed () {
            PreferenceCenterConfig config =
                JsonUtility.FromJson<PreferenceCenterConfig> (PreferenceCenterJson);
            PreferenceCenterCondition condition = config.sections[0].items[1].conditions[0];

            Assert.AreEqual (PreferenceCenterConditionType.NotificationOptIn, condition.Type);
            Assert.AreEqual (PreferenceCenterOptInStatus.OptOut, condition.WhenStatus);
        }

        /// Types added server-side after this plugin version must degrade, not throw.
        [Test]
        public void UnknownDiscriminatorsDegradeToUnknown () {
            PreferenceCenterConfig config = JsonUtility.FromJson<PreferenceCenterConfig> (
                "{\"id\":\"pc\",\"sections\":[{\"type\":\"brand_new_section\",\"id\":\"s\"}]}");

            Assert.AreEqual (PreferenceCenterSectionType.Unknown, config.sections[0].Type);
            Assert.AreEqual ("brand_new_section", config.sections[0].RawType);
        }

        // --- Inbox list icon ---

        [Test]
        public void InboxMessageCarriesTheListIconUrl () {
            InboxMessage message = new InboxMessage (new InternalInboxMessage {
                id = "m1",
                listIconUrl = "https://example.com/icon.png"
            });

            Assert.AreEqual ("https://example.com/icon.png", message.listIconUrl);
        }

        // --- Live Update: `content` is an arbitrary object on the wire ---

        [Test]
        public void LiveUpdateContentComesFromTheParallelArrays () {
            LiveUpdate[] updates = AirshipUtils.Deserialize<LiveUpdate[]> (
                "[{\"name\":\"game\",\"type\":\"Example\"," +
                "\"lastContentUpdateTimestamp\":\"2026-08-21T10:00:00Z\"," +
                "\"contentKeys\":[\"emoji\",\"score\"]," +
                "\"contentValues\":[\"trophy\",\"3\"]}]");

            Assert.AreEqual (1, updates.Length);
            Assert.AreEqual ("game", updates[0].name);
            Assert.AreEqual ("2026-08-21T10:00:00Z", updates[0].lastContentUpdateTimestamp);
            Assert.IsNotNull (updates[0].Content);
            Assert.AreEqual ("trophy", updates[0].Content["emoji"]);
            Assert.AreEqual ("3", updates[0].Content["score"]);
        }

        [Test]
        public void LiveUpdateWithoutContentHasNullContent () {
            LiveUpdate[] updates = AirshipUtils.Deserialize<LiveUpdate[]> (
                "[{\"name\":\"game\",\"type\":\"Example\"}]");

            Assert.IsNull (updates[0].Content);
        }

        // --- Live Activity: `attributes` and `content.state` are arbitrary objects ---

        [Test]
        public void LiveActivityAttributesAndStateComeFromTheParallelArrays () {
            LiveActivityInfo activity = AirshipUtils.Deserialize<LiveActivityInfo> (
                "{\"id\":\"a1\",\"attributesType\":\"Example\",\"state\":\"active\"," +
                "\"attributesKeys\":[\"name\"],\"attributesValues\":[\"Unity Test\"]," +
                "\"content\":{\"relevanceScore\":100," +
                "\"stateKeys\":[\"emoji\"],\"stateValues\":[\"trophy\"]}}");

            Assert.AreEqual ("a1", activity.id);
            Assert.AreEqual ("active", activity.state);
            Assert.AreEqual ("Unity Test", activity.Attributes["name"]);
            Assert.AreEqual (100d, activity.content.relevanceScore);
            Assert.AreEqual ("trophy", activity.content.State["emoji"]);
        }

        /// A non-string value arrives as its JSON text, matching the extras convention.
        [Test]
        public void LiveActivityNonStringStateValuesArriveAsJsonText () {
            LiveActivityInfo activity = AirshipUtils.Deserialize<LiveActivityInfo> (
                "{\"id\":\"a1\",\"content\":{\"stateKeys\":[\"nested\"]," +
                "\"stateValues\":[\"{\\\"k\\\":1}\"]}}");

            Assert.AreEqual ("{\"k\":1}", activity.content.State["nested"]);
        }
    }
}
