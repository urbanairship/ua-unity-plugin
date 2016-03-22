/*
 Copyright 2016 Urban Airship and Contributors
*/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UrbanAirship
{
	[Serializable]
	public class PushMessage
	{
		[SerializeField]
		private string alert;
		[SerializeField]
		private string identifier;
		[SerializeField]
		private Extra[] extras;

		private Dictionary<string, string> extrasDictionary;

		[Serializable]
		class Extra
		{
			public string key;
			public string value;
		}

		public string Alert {
			get { return this.alert; }
		}

		public string Identifier {
			get { return this.identifier; }
		}

		public Dictionary<string, string> Extras {
			get {
				if (extras == null) {
					return null;
				}

				if (this.extrasDictionary == null) {
					this.extrasDictionary = new Dictionary<string, string> ();
					foreach (Extra extra in extras) {
						this.extrasDictionary.Add (extra.key, extra.value);
					}
				}

				return this.extrasDictionary;
			}
		}

		public static PushMessage FromJson (string jsonString)
		{
			PushMessage pushMessage = JsonUtility.FromJson<PushMessage> (jsonString);
			if (pushMessage.Alert == null && pushMessage.Identifier == null && pushMessage.Extras == null) {
				return null;
			}
			return pushMessage;
		}
	}
}

