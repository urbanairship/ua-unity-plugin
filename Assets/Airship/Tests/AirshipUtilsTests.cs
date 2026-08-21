/* Copyright Airship and Contributors */

using System.Collections.Generic;
using NUnit.Framework;
using AirshipSDK;

namespace AirshipSDK.Tests {

    /// <summary>
    /// Covers the shared serialization helpers the hand-built payloads depend on.
    /// </summary>
    [TestFixture]
    public class AirshipUtilsTests {

        [Test]
        public void EscapesQuotesAndBackslashes () {
            Assert.AreEqual ("say \\\"hi\\\"", AirshipUtils.EscapeJsonString ("say \"hi\""));
            Assert.AreEqual ("a\\\\b", AirshipUtils.EscapeJsonString ("a\\b"));
        }

        [Test]
        public void EscapesWhitespaceControlCharacters () {
            Assert.AreEqual ("a\\nb", AirshipUtils.EscapeJsonString ("a\nb"));
            Assert.AreEqual ("a\\rb", AirshipUtils.EscapeJsonString ("a\rb"));
            Assert.AreEqual ("a\\tb", AirshipUtils.EscapeJsonString ("a\tb"));
            Assert.AreEqual ("a\\bb", AirshipUtils.EscapeJsonString ("a\bb"));
            Assert.AreEqual ("a\\fb", AirshipUtils.EscapeJsonString ("a\fb"));
        }

        /// The rest of the C0 range has no short escape and must be emitted as \uXXXX,
        /// otherwise the native JSON parsers reject the whole payload.
        [Test]
        public void EscapesRemainingControlCharactersAsUnicode () {
            string startOfHeading = "a" + (char) 0x01 + "b";
            string unitSeparator = "a" + (char) 0x1f + "b";

            Assert.AreEqual ("a\\u0001b", AirshipUtils.EscapeJsonString (startOfHeading));
            Assert.AreEqual ("a\\u001fb", AirshipUtils.EscapeJsonString (unitSeparator));
        }

        [Test]
        public void LeavesOrdinaryTextAlone () {
            Assert.AreEqual ("Hello, world", AirshipUtils.EscapeJsonString ("Hello, world"));
        }

        [Test]
        public void ToJsonStringQuotesAndHandlesNull () {
            Assert.AreEqual ("\"a\"", AirshipUtils.ToJsonString ("a"));
            Assert.AreEqual ("null", AirshipUtils.ToJsonString (null));
            Assert.AreEqual ("\"\"", AirshipUtils.ToJsonString (""));
        }

        [Test]
        public void ParseEnumMatchesTheAttributeStringValue () {
            Assert.AreEqual (
                PermissionStatus.Denied,
                AirshipUtils.ParseEnum ("denied", PermissionStatus.NotDetermined));
            Assert.AreEqual (
                PermissionStatus.NotDetermined,
                AirshipUtils.ParseEnum ("not_determined", PermissionStatus.Granted));
        }

        [Test]
        public void ParseEnumFallsBackForMissingOrUnknownValues () {
            Assert.AreEqual (
                PermissionStatus.NotDetermined,
                AirshipUtils.ParseEnum (null, PermissionStatus.NotDetermined));
            Assert.AreEqual (
                PermissionStatus.NotDetermined,
                AirshipUtils.ParseEnum ("", PermissionStatus.NotDetermined));
            Assert.AreEqual (
                PermissionStatus.NotDetermined,
                AirshipUtils.ParseEnum ("something_new", PermissionStatus.NotDetermined));
        }

        [Test]
        public void DeserializeHandlesPrimitives () {
            Assert.IsTrue (AirshipUtils.Deserialize<bool> ("true"));
            Assert.AreEqual (42, AirshipUtils.Deserialize<int> ("42"));
            Assert.AreEqual (7L, AirshipUtils.Deserialize<long> ("7"));
            Assert.AreEqual ("abc", AirshipUtils.Deserialize<string> ("\"abc\""));
        }

        /// Arrays come back empty rather than null so callers can enumerate directly.
        [Test]
        public void DeserializeReturnsEmptyArraysForEmptyResponses () {
            Assert.IsNotNull (AirshipUtils.Deserialize<string[]> (null));
            Assert.AreEqual (0, AirshipUtils.Deserialize<string[]> (null).Length);
            Assert.AreEqual (0, AirshipUtils.Deserialize<string[]> ("").Length);
            Assert.AreEqual (0, AirshipUtils.Deserialize<string[]> ("{}").Length);
        }

        [Test]
        public void DeserializeParsesStringArrays () {
            Assert.AreEqual (
                new string[] { "a", "b" },
                AirshipUtils.Deserialize<string[]> ("[\"a\",\"b\"]"));
        }

        [Test]
        public void DeserializeParsesEnumArraysFromStringValues () {
            AuthorizedNotificationSetting[] settings =
                AirshipUtils.Deserialize<AuthorizedNotificationSetting[]> ("[\"alert\",\"lock_screen\"]");

            Assert.AreEqual (
                new AuthorizedNotificationSetting[] {
                    AuthorizedNotificationSetting.Alert,
                    AuthorizedNotificationSetting.LockScreen
                },
                settings);
        }

        [Test]
        public void SerializeSkipsNullMembersAndStringifiesEnums () {
            string json = AirshipUtils.Serialize (new AirshipConfig {
                site = Site.EU,
                inProduction = true
            });

            Assert.AreEqual ("{\"site\":\"eu\",\"inProduction\":true}", json);
        }

        // --- PairFlattenedObject: the stand-in for JsonUtility's missing dictionaries ---

        [Test]
        public void PairsKeysWithValues () {
            var paired = AirshipUtils.PairFlattenedObject (
                new List<string> { "a", "b" },
                new List<string> { "1", "2" });

            Assert.AreEqual (2, paired.Count);
            Assert.AreEqual ("1", paired["a"]);
            Assert.AreEqual ("2", paired["b"]);
        }

        [Test]
        public void PairingReturnsNullWhenNoKeysWereSent () {
            Assert.IsNull (AirshipUtils.PairFlattenedObject (null, null));
            Assert.IsNull (AirshipUtils.PairFlattenedObject (new List<string> (), null));
        }

        /// A truncated payload should cost the trailing entries, not throw.
        [Test]
        public void PairingToleratesAShorterValuesList () {
            var paired = AirshipUtils.PairFlattenedObject (
                new List<string> { "a", "b" },
                new List<string> { "1" });

            Assert.AreEqual (1, paired.Count);
            Assert.AreEqual ("1", paired["a"]);
        }

        [Test]
        public void PairingToleratesMissingValues () {
            var paired = AirshipUtils.PairFlattenedObject (new List<string> { "a" }, null);

            Assert.AreEqual (0, paired.Count);
        }
    }
}
