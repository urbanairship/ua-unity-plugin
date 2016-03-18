/*
 Copyright 2015 Urban Airship and Contributors
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UrbanAirship {

	interface IUAirshipPlugin
	{

		bool PushEnabled {
			get;
			set;
		}

		string Tags {
			get;
		}

		string Alias {
			get;
			set;
		}

		string ChannelId {
			get;
		}

		bool LocationEnabled {
			get;
			set;
		}

		bool BackgroundLocationEnabled {
			get;
			set;
		}

		string NamedUserId {
			get;
			set;
		}

		void AddListener (GameObject gameObject);

		void RemoveListener (GameObject gameObject);

		string GetDeepLink (bool clear);

		string GetIncomingPush (bool clear);


		void AddTag (string tag);

		void RemoveTag (string tag);
	}
}

