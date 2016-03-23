/*
 Copyright 2016 Urban Airship and Contributors
*/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UrbanAirship
{
	/// <summary>
	/// A push message model object.
	/// </summary>
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

		/// <summary>
		/// Gets the alert text.
		/// </summary>
		/// <value>The alert text.</value>
		public string Alert {
			get { return this.alert; }
		}

		/// <summary>
		/// Gets the push identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public string Identifier {
			get { return this.identifier; }
		}

		/// <summary>
		/// Gets the key value extras sent with the push.
		/// </summary>
		/// <remarks>Non-string extra values are encoded as JSON strings.</remarks>
		/// <value>The extras.</value>
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

