/*
 Copyright 2016 Urban Airship and Contributors
*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UrbanAirship
{
	public class TagGroupEditor
	{
		private Action<string> onApply;
		private IList<TagGroupOperation> operations = new List<TagGroupOperation> ();


		internal TagGroupEditor (Action<string> onApply)
		{
			this.onApply = onApply;
		}

		public TagGroupEditor AddTag (string tagGroup, string tag)
		{
			AddTags (tagGroup, new List<string> (new [] { tag }));
			return this;
		}

		public TagGroupEditor AddTags (string tagGroup, ICollection<string> tags)
		{
			operations.Add (new TagGroupOperation ("add", tagGroup, tags));
			return this;
		}

		public TagGroupEditor RemoveTag (string tagGroup, string tag)
		{
			RemoveTags (tagGroup, new List<string> (new [] { tag }));
			return this;
		}

		public TagGroupEditor RemoveTags (string tagGroup, ICollection<string> tags)
		{
			operations.Add (new TagGroupOperation ("remove", tagGroup, tags));
			return this;
		}

		public void Apply ()
		{
			if (onApply != null) {
				JsonArray<TagGroupOperation> jsonArray = new JsonArray<TagGroupOperation>();
				jsonArray.values = operations.ToArray ();
				onApply (jsonArray.ToJson());
			}
		}

		[Serializable]
		internal class TagGroupOperation
		{
			[SerializeField]
			private string operation;

			[SerializeField]
			private string tagGroup;

			[SerializeField]
			private string[] tags;

			public TagGroupOperation (string operation, string tagGroup, ICollection<string> tags)
			{
				this.operation = operation;
				this.tagGroup = tagGroup;
				this.tags = tags.ToArray ();
			}
		}
	}
}

