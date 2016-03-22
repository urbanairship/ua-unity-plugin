/*
 Copyright 2016 Urban Airship and Contributors
*/

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace UrbanAirship
{
	[System.Serializable]
	public class CustomEvent
	{
		[SerializeField]
		private string eventName;
		[SerializeField]
		private string eventValue;
		[SerializeField]
		private string transactionId;
		[SerializeField]
		private string interactionType;
		[SerializeField]
		private string interactionId;
		[SerializeField]
		private Property[] properties;

		private List<Property> propertyList;


		public CustomEvent()
		{
			this.propertyList = new List<Property> ();
		}

		public string EventName {
			get { return eventName; }
			set { eventName = value; }
		}
		public decimal EventValue {
			get { return Decimal.Parse (eventValue); }
			set { eventValue = value.ToString (); }
		}

		public string TransactionId {
			get { return transactionId; }
			set { transactionId = value; }
		}

		public string InteractionType {
			get { return interactionType; }
			set { interactionType = value; }
		}

		public string InteractionId {
			get { return interactionId; }
			set { interactionId = value; }
		}

		public void AddProperty(string name, string value)
		{
			this.propertyList.Add (new Property ("s", name, value));
		}

		public void AddProperty(string name, double value)
		{
			this.propertyList.Add (new Property ("d", name, value));
		}

		public void AddProperty(string name, bool value)
		{
			this.propertyList.Add (new Property ("b", name, value));
		}

		public void AddProperty(string name, ICollection<string> value)
		{
			this.propertyList.Add (new Property ("sa", name, value));
		}

		public string ToJson ()
		{
			this.properties = this.propertyList.ToArray ();
			return JsonUtility.ToJson (this);
		}

		[Serializable]
		class Property
		{
			public string type;
			public string name;
			public string stringValue;
			public double doubleValue;
			public bool boolValue;
			public string[] stringArrayValue;

			public Property(string type, string name, System.Object value)
			{
				this.type = type;
				this.name = name;

				if (type == "s") {
					this.stringValue = (string) value;
				} else if (type == "d") {
					this.doubleValue = (double) value;
				} else if (type == "b") {
					this.boolValue = (bool) value;
				} else if (type == "sa") {
					ICollection<string> collection = (ICollection<string>) value;
					this.stringArrayValue = collection.ToArray ();
				}
			}
		}
	}
}

