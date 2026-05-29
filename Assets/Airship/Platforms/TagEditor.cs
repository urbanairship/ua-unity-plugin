/* Copyright Airship and Contributors */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AirshipSDK {
    /// <summary>
    /// An editor for tags.
    /// </summary>
    public class TagEditor {
        private Action<string> onApply;
        private IList<TagOperation> operations = new List<TagOperation> ();

        internal TagEditor (Action<string> onApply) {
            this.onApply = onApply;
        }

        /// <summary>
        /// Adds the provided tag.
        /// </summary>
        /// <returns>The tag editor.</returns>
        /// <param name="tag">The tag to add.</param>
        public TagEditor AddTag (string tag) {
            AddTags (new List<string> (new [] { tag }));
            return this;
        }

        /// <summary>
        /// Adds the provided tags.
        /// </summary>
        /// <returns>The tag editor.</returns>
        /// <param name="tags">The tags to add.</param>
        public TagEditor AddTags (ICollection<string> tags) {
            operations.Add (new TagOperation ("add", tags));
            return this;
        }

        /// <summary>
        /// Removes the provided tag.
        /// </summary>
        /// <returns>The tag editor.</returns>
        /// <param name="tag">The tag to remove.</param>
        public TagEditor RemoveTag (string tag) {
            RemoveTags (new List<string> (new [] { tag }));
            return this;
        }

        /// <summary>
        /// Removes the provided tags.
        /// </summary>
        /// <returns>The tag editor.</returns>
        /// <param name="tags">The tags to remove.</param>
        public TagEditor RemoveTags (ICollection<string> tags) {
            operations.Add (new TagOperation ("remove", tags));
            return this;
        }

        /// <summary>
        /// Applies pending changes.
        /// </summary>
        public void Apply () {
            if (onApply != null) {
                JsonArray<TagOperation> jsonArray = new JsonArray<TagOperation> ();
                jsonArray.values = operations.ToArray ();
                onApply (jsonArray.ToJson ());
            }
        }

        [Serializable]
        internal class TagOperation {
#pragma warning disable
            // Used for JSON encoding/decoding

            [SerializeField]
            private string operationType;

            [SerializeField]
            private string[] tags;
#pragma warning restore

            public TagOperation (string operation, ICollection<string> tags) {
                this.operationType = operation;
                this.tags = tags.ToArray ();
            }
        }
    }
}
