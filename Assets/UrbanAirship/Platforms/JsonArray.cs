/*
 Copyright 2016 Urban Airship and Contributors
 */

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UrbanAirship
{
	[System.Serializable]
	class JsonArray<T>
	{
		public T[] values = null;

		public static JsonArray<T> FromJson (string jsonString)
		{
			string wrappedArray = string.Format ("{{ \"{0}\": {1}}}", "values", jsonString);
			return JsonUtility.FromJson<JsonArray<T>> (wrappedArray);
		}

		public IEnumerable<T> AsEnumerable () 
		{
			if (this.values == null) {
				return new T[0].AsEnumerable ();
			} else {
				return this.values.AsEnumerable ();
			}
		}

		public string ToJson ()
		{
			return JsonUtility.ToJson (this);
		}
	}
}

