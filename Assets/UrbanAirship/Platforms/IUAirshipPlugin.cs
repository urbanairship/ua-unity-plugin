/*
 Copyright 2016 Urban Airship and Contributors
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UrbanAirship {

	interface IUAirshipPlugin
	{
		bool UserNotificationsEnabled {
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

		bool BackgroundLocationAllowed {
			get;
			set;
		}

		string NamedUserId {
			get;
			set;
		}

		GameObject Listener {
			set;
		}

		string GetDeepLink (bool clear);

		string GetIncomingPush (bool clear);

		void AddTag (string tag);

		void RemoveTag (string tag);

		void AddCustomEvent (string customEvent);

		void AssociateIdentifier (string key, string identifier);

		void DisplayMessageCenter ();
		
		int UnreadCount {
			get;
		}

		void EditNamedUserTagGroups (string payload);

		void EditChannelTagGroups (string payload);
	}
}

