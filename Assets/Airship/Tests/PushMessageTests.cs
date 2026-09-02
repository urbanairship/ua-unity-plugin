/* Copyright Airship and Contributors */

using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using AirshipSDK;

namespace AirshipSDK.Tests {

    /// <summary>
    /// Guards the wire contract between the native push payload and PushMessage.
    ///
    /// The native layers send the framework proxy payload with `extras` split into
    /// parallel arrays, because Unity's JsonUtility has no dictionary support.
    /// </summary>
    [TestFixture]
    public class PushMessageTests {

        [Test]
        public void ParsesAlertAndExtras () {
            PushMessage message = PushMessage.FromJson (
                "{\"alert\":\"Hello\",\"extrasKeys\":[\"a\",\"b\"],\"extrasValues\":[\"1\",\"2\"]}");

            Assert.IsNotNull (message);
            Assert.AreEqual ("Hello", message.Alert);
            Assert.IsNotNull (message.Extras);
            Assert.AreEqual (2, message.Extras.Count);
            Assert.AreEqual ("1", message.Extras["a"]);
            Assert.AreEqual ("2", message.Extras["b"]);
        }

        /// A data-only push carries no alert. It must still reach the handler -- this is
        /// the case that was silently dropped when extras failed to deserialize.
        [Test]
        public void SilentPushWithOnlyExtrasIsNotDropped () {
            PushMessage message = PushMessage.FromJson (
                "{\"extrasKeys\":[\"custom\"],\"extrasValues\":[\"value\"]}");

            Assert.IsNotNull (message, "a data-only push must not be dropped");
            Assert.IsNull (message.Alert);
            Assert.AreEqual ("value", message.Extras["custom"]);
        }

        [Test]
        public void ParsesTitleAndNotificationId () {
            PushMessage message = PushMessage.FromJson (
                "{\"alert\":\"Hello\",\"title\":\"Greetings\",\"notificationId\":\"tag:7\"}");

            Assert.AreEqual ("Greetings", message.Title);
            Assert.AreEqual ("tag:7", message.NotificationId);
        }

        /// A notification can carry a title with no body. It used to be dropped, because
        /// the guard only looked at the alert and at a send ID nothing ever populated.
        [Test]
        public void TitleOnlyPushIsNotDropped () {
            PushMessage message = PushMessage.FromJson ("{\"title\":\"Greetings\"}");

            Assert.IsNotNull (message, "a title-only push must not be dropped");
            Assert.IsNull (message.Alert);
            Assert.AreEqual ("Greetings", message.Title);
        }

        /// A push that posted a notification always carries its id, even when Airship sent
        /// no extras and the alert came through empty.
        [Test]
        public void NotificationIdAloneIsNotDropped () {
            Assert.IsNotNull (PushMessage.FromJson ("{\"notificationId\":\"tag:7\"}"));
        }

        [Test]
        public void ExtrasAreNullWhenThePushCarriedNone () {
            PushMessage message = PushMessage.FromJson ("{\"alert\":\"Hello\"}");

            Assert.IsNotNull (message);
            Assert.IsNull (message.Extras);
        }

        /// Values that were not strings on the wire arrive as their JSON text.
        [Test]
        public void NonStringExtraValuesArriveAsJsonText () {
            PushMessage message = PushMessage.FromJson (
                "{\"alert\":\"a\",\"extrasKeys\":[\"nested\"],\"extrasValues\":[\"{\\\"k\\\":1}\"]}");

            Assert.AreEqual ("{\"k\":1}", message.Extras["nested"]);
        }

        [Test]
        public void MismatchedExtraArraysDoNotThrow () {
            PushMessage message = PushMessage.FromJson (
                "{\"alert\":\"a\",\"extrasKeys\":[\"a\",\"b\"],\"extrasValues\":[\"1\"]}");

            Assert.IsNotNull (message);
            Assert.AreEqual (1, message.Extras.Count);
            Assert.AreEqual ("1", message.Extras["a"]);
        }

        [Test]
        public void MissingExtraValuesDoNotThrow () {
            PushMessage message = PushMessage.FromJson (
                "{\"alert\":\"a\",\"extrasKeys\":[\"a\"]}");

            Assert.IsNotNull (message);
            Assert.AreEqual (0, message.Extras.Count);
        }

        [Test]
        public void EmptyPayloadReturnsNull () {
            Assert.IsNull (PushMessage.FromJson ("{}"));
        }

        /// Kotlin sends the literal string "null" when the proxy event carries no payload.
        [Test]
        public void NullLiteralPayloadReturnsNullInsteadOfThrowing () {
            Assert.IsNull (PushMessage.FromJson ("null"));
            Assert.IsNull (PushMessage.FromJson (""));
            Assert.IsNull (PushMessage.FromJson (null));
        }

        [Test]
        public void MalformedPayloadReturnsNullInsteadOfThrowing () {
            // FromJson logs the parse failure before returning null, and Unity fails any
            // test that emits an unexpected error log. Matched loosely because the rest of
            // the message is JsonUtility's own wording.
            LogAssert.Expect (LogType.Error, new Regex ("unable to parse push message"));

            Assert.IsNull (PushMessage.FromJson ("not json at all"));
        }
    }
}
