/*
 Copyright 2018 Urban Airship and Contributors
*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UrbanAirship {
    /// <summary>
    /// An editor for tag groups.
    /// </summary>
    public class TagGroupEditor {
        private Action<string> onApply;
        private IList<TagGroupOperation> operations = new List<TagGroupOperation> ();

        internal TagGroupEditor (Action<string> onApply) {
            this.onApply = onApply;
        }

        /// <summary>
        /// Adds the provided tag.
        /// </summary>
        /// <returns>The tag group editor.</returns>
        /// <param name="tagGroup">The associated tag group.</param>
        /// <param name="tag">The tag to add.</param>
        public TagGroupEditor AddTag (string tagGroup, string tag) {
            AddTags (tagGroup, new List<string> (new [] { tag }));
            return this;
        }

        /// <summary>
        /// Adds the provided tags.
        /// </summary>
        /// <returns>The tag group editor.</returns>
        /// <param name="tagGroup">The associated tag group.</param>
        /// <param name="tags">The tags to add.</param>
        public TagGroupEditor AddTags (string tagGroup, ICollection<string> tags) {
            operations.Add (new TagGroupOperation ("add", tagGroup, tags));
            return this;
        }

        /// <summary>
        /// Removes the provided tag.
        /// </summary>
        /// <returns>The tag group editor.</returns>
        /// <param name="tagGroup">The associated tag group.</param>
        /// <param name="tag">The tag to remove.</param>
        public TagGroupEditor RemoveTag (string tagGroup, string tag) {
            RemoveTags (tagGroup, new List<string> (new [] { tag }));
            return this;
        }

        /// <summary>
        /// Removes the provided tags.
        /// </summary>
        /// <returns>The tag group editor.</returns>
        /// <param name="tagGroup">The associated tag group.</param>
        /// <param name="tags">The tags to remove.</param>
        public TagGroupEditor RemoveTags (string tagGroup, ICollection<string> tags) {
            operations.Add (new TagGroupOperation ("remove", tagGroup, tags));
            return this;
        }

        /// <summary>
        /// Applies pending changes.
        /// </summary>
        public void Apply () {
            if (onApply != null) {
                JsonArray<TagGroupOperation> jsonArray = new JsonArray<TagGroupOperation> ();
                jsonArray.values = operations.ToArray ();
                onApply (jsonArray.ToJson ());
            }
        }

        [Serializable]
        internal class TagGroupOperation {
#pragma warning disable
            // Used for JSON encoding/decoding

            [SerializeField]
            private string operation;

            [SerializeField]
            private string tagGroup;

            [SerializeField]
            private string[] tags;
#pragma warning restore

            public TagGroupOperation (string operation, string tagGroup, ICollection<string> tags) {
                this.operation = operation;
                this.tagGroup = tagGroup;
                this.tags = tags.ToArray ();
            }
        }
    }
}