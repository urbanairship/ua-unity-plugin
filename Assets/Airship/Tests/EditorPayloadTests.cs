/* Copyright Airship and Contributors */

using System.Collections.Generic;
using NUnit.Framework;
using AirshipSDK;

namespace AirshipSDK.Tests {

    /// <summary>
    /// Pins the payloads the editor classes hand to the native layers. These key names are
    /// the framework proxy's contract (TagOperation, AttributeOperation,
    /// SubscriptionListOperation), so a rename here silently stops the operation applying.
    /// </summary>
    [TestFixture]
    public class EditorPayloadTests {

        private string captured;

        [SetUp]
        public void Setup () {
            captured = null;
        }

        // --- Attributes ---

        [Test]
        public void StringAttributeIsQuotedAndTyped () {
            new AttributeEditor (payload => captured = payload)
                .SetAttribute ("city", "Paris")
                .Apply ();

            Assert.AreEqual (
                "[{\"action\":\"set\",\"key\":\"city\",\"value\":\"Paris\",\"type\":\"string\"}]",
                captured);
        }

        /// The proxy requires number and date values to be JSON numbers, not strings.
        [Test]
        public void NumberAttributeIsUnquoted () {
            new AttributeEditor (payload => captured = payload)
                .SetAttribute ("age", 41)
                .Apply ();

            Assert.AreEqual (
                "[{\"action\":\"set\",\"key\":\"age\",\"value\":41,\"type\":\"number\"}]",
                captured);
        }

        [Test]
        public void RemoveAttributeOmitsValueAndType () {
            new AttributeEditor (payload => captured = payload)
                .RemoveAttribute ("city")
                .Apply ();

            Assert.AreEqual ("[{\"action\":\"remove\",\"key\":\"city\"}]", captured);
        }

        /// A raw quote or backslash used to produce malformed JSON, which made the native
        /// parsers reject the whole batch rather than the single bad operation.
        [Test]
        public void AttributeKeysAndValuesAreEscaped () {
            new AttributeEditor (payload => captured = payload)
                .SetAttribute ("say\"what", "back\\slash")
                .Apply ();

            Assert.AreEqual (
                "[{\"action\":\"set\",\"key\":\"say\\\"what\",\"value\":\"back\\\\slash\",\"type\":\"string\"}]",
                captured);
        }

        [Test]
        public void MultipleAttributeOperationsAreBatched () {
            new AttributeEditor (payload => captured = payload)
                .SetAttribute ("a", "1")
                .RemoveAttribute ("b")
                .Apply ();

            Assert.AreEqual (
                "[{\"action\":\"set\",\"key\":\"a\",\"value\":\"1\",\"type\":\"string\"}," +
                "{\"action\":\"remove\",\"key\":\"b\"}]",
                captured);
        }

        // --- Tags ---

        [Test]
        public void TagOperationUsesOperationTypeAndTags () {
            new TagEditor (payload => captured = payload)
                .AddTag ("one")
                .RemoveTags (new List<string> { "two", "three" })
                .Apply ();

            Assert.AreEqual (
                "{\"values\":[" +
                "{\"operationType\":\"add\",\"tags\":[\"one\"]}," +
                "{\"operationType\":\"remove\",\"tags\":[\"two\",\"three\"]}]}",
                captured);
        }

        [Test]
        public void TagGroupOperationIncludesTheGroup () {
            new TagGroupEditor (payload => captured = payload)
                .AddTag ("loyalty", "gold")
                .Apply ();

            Assert.AreEqual (
                "{\"values\":[{\"operationType\":\"add\",\"group\":\"loyalty\",\"tags\":[\"gold\"]}]}",
                captured);
        }

        // --- Subscription lists ---

        [Test]
        public void SubscriptionListOperationUsesActionAndListId () {
            new SubscriptionListEditor (payload => captured = payload)
                .Subscribe ("news")
                .Unsubscribe ("promos")
                .Apply ();

            Assert.AreEqual (
                "{\"values\":[" +
                "{\"action\":\"subscribe\",\"listId\":\"news\"}," +
                "{\"action\":\"unsubscribe\",\"listId\":\"promos\"}]}",
                captured);
        }

        [Test]
        public void ScopedSubscriptionListOperationIncludesTheScope () {
            new ScopedSubscriptionListEditor (payload => captured = payload)
                .Subscribe ("news", SubscriptionScope.EMAIL)
                .Apply ();

            Assert.AreEqual (
                "{\"values\":[{\"action\":\"subscribe\",\"listId\":\"news\",\"scope\":\"email\"}]}",
                captured);
        }
    }
}
