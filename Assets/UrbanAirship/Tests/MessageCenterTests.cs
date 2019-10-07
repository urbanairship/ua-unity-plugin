using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;
using UrbanAirship;

namespace Tests
{
    public class MessageCenterTests
    {
        private UAirship sharedAirship;
        private IUAirshipPlugin mockPlugin;

        [SetUp]
        public void Setup ()
        {
            mockPlugin = Substitute.For<IUAirshipPlugin>();
            sharedAirship = new UAirship(mockPlugin);
            UAirship.sharedAirship = sharedAirship;
        }

        [Test]
        public void TestUserNotificationsEnabled ()
        {
            sharedAirship.UserNotificationsEnabled = true;
            mockPlugin.Received().UserNotificationsEnabled = true;
        }

        [Test]
        public void TestUserNotificationsDisabled ()
        {
            sharedAirship.UserNotificationsEnabled = false;
            mockPlugin.Received().UserNotificationsEnabled = false;
        }

        [Test]
        public void TestTags ()
        {
            var expectedTagsAsJson = @"[ ""abc"", ""def"" ]";
            mockPlugin.Tags.Returns(expectedTagsAsJson);

            List<string> expectedTagsAsList = new List<string>()
            {
                "abc",
                "def"
            };

            Assert.AreEqual(expectedTagsAsList, sharedAirship.Tags);
        }

        [Test]
        public void TestChannelId ()
        {
            var expectedChannelId = "channel-id";

            mockPlugin.ChannelId.Returns(expectedChannelId);

            Assert.AreEqual(expectedChannelId, sharedAirship.ChannelId);
        }

        [Test]
        public void TestLocationEnabled ()
        {
            sharedAirship.LocationEnabled = true;
            mockPlugin.Received().LocationEnabled = true;
        }

        [Test]
        public void TestLocationDisabled ()
        {
            sharedAirship.LocationEnabled = false;
            mockPlugin.Received().LocationEnabled = false;
        }

        [Test]
        public void BackgroundLocationAllowed ()
        {
            sharedAirship.BackgroundLocationAllowed = true;
            mockPlugin.Received().BackgroundLocationAllowed = true;
        }

        [Test]
        public void BackgroundLocationNotAllowed ()
        {
            sharedAirship.BackgroundLocationAllowed = false;
            mockPlugin.Received().BackgroundLocationAllowed = false;
        }

        [Test]
        public void TestGetNamedUserId ()
        {
            var expectedNamedUserId = "JohnDoe";

            mockPlugin.NamedUserId.Returns(expectedNamedUserId);

            Assert.AreEqual(expectedNamedUserId, sharedAirship.NamedUserId);
        }

        [Test]
        public void TestSetNamedUserId ()
        {
            var expectedNamedUserId = "JohnDoe";

            sharedAirship.NamedUserId = expectedNamedUserId;

            mockPlugin.Received().NamedUserId = expectedNamedUserId;
        }

        [Test]
        public void TestGetDeepLink ()
        {
            var expectedDeepLink = "/deep/link";

            mockPlugin.GetDeepLink(false).Returns(expectedDeepLink);

            Assert.AreEqual(expectedDeepLink, sharedAirship.GetDeepLink(false));

            mockPlugin.GetDeepLink(true).Returns(expectedDeepLink);

            Assert.AreEqual(expectedDeepLink, sharedAirship.GetDeepLink(true));

            Received.InOrder(() =>
            {
                mockPlugin.GetDeepLink(false);
                mockPlugin.GetDeepLink(true);
            });
        }

        [Test]
        public void TestMessageCenterUnreadCount ()
        {
            var expectedUnreadCount = 99;

            mockPlugin.MessageCenterUnreadCount.Returns(expectedUnreadCount);

            Assert.AreEqual(expectedUnreadCount, sharedAirship.MessageCenterUnreadCount);
        }

        [Test]
        public void TestMessageCenterCount ()
        {
            var expectedCount = 55;

            mockPlugin.MessageCenterCount.Returns(expectedCount);

            Assert.AreEqual(expectedCount, sharedAirship.MessageCenterCount);
        }

        [Test]
        public void TestInboxMessagesNoMessages ()
        {
            var expectedMessagesAsJson = @"[
            ]";
            mockPlugin.InboxMessages().Returns(expectedMessagesAsJson);

            var expectedMessagesAsList = new List<InboxMessage>()
            {
            };

            Assert.AreEqual(expectedMessagesAsList, sharedAirship.InboxMessages());
        }

        [Test]
        public void TestInboxMessagesOneMessage ()
        {
            var expectedMessagesAsJson = @"[
                {
                    ""sentDate"": 1547670565000,
                    ""id"": ""ahpPUBnNEemxogJC_nHsWw"",
                    ""title"": ""Title of the message"",
                    ""isRead"": true,
                    ""isDeleted"": false,
                    ""extrasKeys"": [
                      ""com.urbanairship.listing.field1"",
                      ""com.urbanairship.listing.field2"",
                      ""com.urbanairship.listing.template""
                    ],
                    ""extrasValues"":[
                      """",
                      """",
                      ""text""
                    ]
                }
            ]";
            mockPlugin.InboxMessages().Returns(expectedMessagesAsJson);

            var expectedMessagesAsList = new List<InboxMessage>()
            {
                new InboxMessage(
                    id: @"ahpPUBnNEemxogJC_nHsWw",
                    title: "Title of the message",
                    sentDate: 1547670565000,
                    isRead: true,
                    isDeleted: false,
                    extras: new Dictionary<string, string>()
                    {
                        { "com.urbanairship.listing.field1", "" },
                        { "com.urbanairship.listing.field2", "" },
                        { "com.urbanairship.listing.template", "text" }
                    }
                )
            };

            Assert.AreEqual(expectedMessagesAsList, sharedAirship.InboxMessages());
        }

        [Test]
        public void TestInboxMessagesMultipleMessages ()
        {
            var expectedMessagesAsJson = @"[
                {
                    ""sentDate"": 1547670565000,
                    ""id"": ""ahpPUBnNEemxogJC_nHsWw"",
                    ""title"": ""Title of the message"",
                    ""isRead"": true,
                    ""isDeleted"": false,
                    ""extrasKeys"": [
                      ""com.urbanairship.listing.field1"",
                      ""com.urbanairship.listing.field2"",
                      ""com.urbanairship.listing.template""
                    ],
                    ""extrasValues"":[
                      """",
                      """",
                      ""text""
                    ]
                },
                {
                    ""sentDate"": 10,
                    ""id"": ""anidentifier"",
                    ""title"": ""Title"",
                    ""isRead"": false,
                    ""isDeleted"": true,
                    ""extrasKeys"": [
                    ],
                    ""extrasValues"":[
                    ]
                },
                {
                    ""sentDate"": 200,
                    ""id"": ""anotheridentifier"",
                    ""title"": ""Another Title"",
                    ""isRead"": false,
                    ""isDeleted"": false
                }
            ]";
            mockPlugin.InboxMessages().Returns(expectedMessagesAsJson);

            var expectedMessagesAsList = new List<InboxMessage>()
            {
                new InboxMessage(
                    id: @"ahpPUBnNEemxogJC_nHsWw",
                    title: "Title of the message",
                    sentDate: 1547670565000,
                    isRead: true,
                    isDeleted: false,
                    extras: new Dictionary<string, string>()
                    {
                        { "com.urbanairship.listing.field1", "" },
                        { "com.urbanairship.listing.field2", "" },
                        { "com.urbanairship.listing.template", "text" }
                    }
                ),
                new InboxMessage(
                    id: @"anidentifier",
                    title: "Title",
                    sentDate: 10,
                    isRead: false,
                    isDeleted: true,
                    extras: null
                ),
                new InboxMessage(
                    id: @"anotheridentifier",
                    title: "Another Title",
                    sentDate: 200,
                    isRead: false,
                    isDeleted: false,
                    extras: null
                )
            };

            Assert.AreEqual(expectedMessagesAsList, sharedAirship.InboxMessages());
        }

        [Test]
        public void TestMarkInboxMessageRead ()
        {
            string messageId = "AMessageId";
            sharedAirship.MarkInboxMessageRead(messageId);
            mockPlugin.Received().MarkInboxMessageRead(messageId);
        }

        [Test]
        public void TestDeleteInboxMessage ()
        {
            string messageId = "AMessageId";
            sharedAirship.DeleteInboxMessage(messageId);
            mockPlugin.Received().DeleteInboxMessage(messageId);
        }

        [Test]
        public void TestDisplayMessageCenter ()
        {
            sharedAirship.DisplayMessageCenter();
            mockPlugin.Received().DisplayMessageCenter();
        }

        [Test]
        public void TestDisplayInboxMessage ()
        {
            string messageId = "AMessageId";

            sharedAirship.DisplayInboxMessage(messageId, false);
            mockPlugin.Received().DisplayInboxMessage(messageId, false);

            sharedAirship.DisplayInboxMessage(messageId, true);
            mockPlugin.Received().DisplayInboxMessage(messageId, true);
        }

        [Test]
        public void TestRefreshInbox ()
        {
            sharedAirship.RefreshInbox();
            mockPlugin.Received().RefreshInbox();
        }

        [Test]
        public void TestSetAutoLaunchDefaultMessageCenter ()
        {
            sharedAirship.SetAutoLaunchDefaultMessageCenter(true);
            mockPlugin.Received().SetAutoLaunchDefaultMessageCenter(true);

            mockPlugin.ClearReceivedCalls();

            sharedAirship.SetAutoLaunchDefaultMessageCenter(false);
            mockPlugin.Received().SetAutoLaunchDefaultMessageCenter(false);
        }

        [Test]
        public void TestOnInboxUpdated ()
        {
            var wasCalled = false;
            sharedAirship.OnInboxUpdated += (messageUnreadCount, messageCount) =>
            {
                Assert.AreEqual(messageUnreadCount, 1);
                Assert.AreEqual(messageCount, 2);
                wasCalled = true;
            };

            var counts = @"{ ""unread"": 1, ""total"": 2 }";
            UAirship.UrbanAirshipListener listener = sharedAirship.gameObject.GetComponent<UAirship.UrbanAirshipListener>() as UAirship.UrbanAirshipListener;
            listener.OnInboxUpdated(counts);

            Assert.True(wasCalled);
        }

        [Test]
        public void TestOnShowInboxSpecificMessage ()
        {
            var wasCalled = false;
            sharedAirship.OnShowInbox += (theMessageId) =>
            {
                Assert.AreEqual(theMessageId, "AMessageId");
                wasCalled = true;
            };

            var messageId = "AMessageId";
            UAirship.UrbanAirshipListener listener = sharedAirship.gameObject.GetComponent<UAirship.UrbanAirshipListener>() as UAirship.UrbanAirshipListener;
            listener.OnShowInbox(messageId);

            Assert.True(wasCalled);
        }

        [Test]
        public void TestOnShowInboxAllMessages ()
        {
            var wasCalled = false;
            sharedAirship.OnShowInbox += (theMessageId) =>
            {
                Assert.IsNull(theMessageId);
                wasCalled = true;
            };

            string messageId = "";
            UAirship.UrbanAirshipListener listener = sharedAirship.gameObject.GetComponent<UAirship.UrbanAirshipListener>() as UAirship.UrbanAirshipListener;
            listener.OnShowInbox(messageId);

            Assert.True(wasCalled);
        }
    }
}
