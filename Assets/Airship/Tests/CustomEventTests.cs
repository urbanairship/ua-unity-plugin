/* Copyright Airship and Contributors */

using System.Collections.Generic;
using NUnit.Framework;
using AirshipSDK;

namespace AirshipSDK.Tests {

    /// <summary>
    /// Guards the custom event wire contract. The framework proxy reads `eventValue` only
    /// when it is a JSON number and `properties` only when it is a JSON object; anything
    /// else is silently discarded rather than rejected.
    /// </summary>
    [TestFixture]
    public class CustomEventTests {

        [Test]
        public void EventValueIsSerializedAsANumber () {
            CustomEvent customEvent = new CustomEvent ();
            customEvent.EventName = "purchase";
            customEvent.EventValue = 12.99m;

            Assert.AreEqual (
                "{\"eventName\":\"purchase\",\"eventValue\":12.99}",
                customEvent.ToJson ());
        }

        [Test]
        public void PropertiesAreSerializedAsAnObject () {
            CustomEvent customEvent = new CustomEvent ();
            customEvent.EventName = "purchase";
            customEvent.AddProperty ("category", "shoes");
            customEvent.AddProperty ("count", 2.0);
            customEvent.AddProperty ("member", true);
            customEvent.AddProperty ("tags", new List<string> { "a", "b" });

            Assert.AreEqual (
                "{\"eventName\":\"purchase\",\"properties\":{" +
                "\"category\":\"shoes\"," +
                "\"count\":2," +
                "\"member\":true," +
                "\"tags\":[\"a\",\"b\"]}}",
                customEvent.ToJson ());
        }

        [Test]
        public void OptionalFieldsAreOmittedWhenUnset () {
            CustomEvent customEvent = new CustomEvent ();
            customEvent.EventName = "view";

            Assert.AreEqual ("{\"eventName\":\"view\"}", customEvent.ToJson ());
        }

        [Test]
        public void InteractionAndTransactionAreIncludedWhenSet () {
            CustomEvent customEvent = new CustomEvent ();
            customEvent.EventName = "view";
            customEvent.TransactionId = "tx-1";
            customEvent.InteractionType = "url";
            customEvent.InteractionId = "https://example.com";

            Assert.AreEqual (
                "{\"eventName\":\"view\"," +
                "\"transactionId\":\"tx-1\"," +
                "\"interactionType\":\"url\"," +
                "\"interactionId\":\"https://example.com\"}",
                customEvent.ToJson ());
        }

        [Test]
        public void NamesAndValuesAreEscaped () {
            CustomEvent customEvent = new CustomEvent ();
            customEvent.EventName = "say \"hi\"";
            customEvent.AddProperty ("back\\slash", "line\nbreak");

            Assert.AreEqual (
                "{\"eventName\":\"say \\\"hi\\\"\"," +
                "\"properties\":{\"back\\\\slash\":\"line\\nbreak\"}}",
                customEvent.ToJson ());
        }

        [Test]
        public void NonFiniteDoublePropertyIsRejected () {
            CustomEvent customEvent = new CustomEvent ();
            customEvent.EventName = "bad";

            Assert.Throws<System.FormatException> (
                () => customEvent.AddProperty ("nan", double.NaN));
            Assert.Throws<System.FormatException> (
                () => customEvent.AddProperty ("inf", double.PositiveInfinity));
        }
    }
}
