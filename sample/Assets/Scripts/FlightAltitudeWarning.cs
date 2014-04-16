using UnityEngine;
using System.Collections;

public class FlightAltitudeWarning : MonoBehaviour {

	public string lowWarning;
	public string highWarning;

	private GUIText label;
	private Airship airship;

	void Awake () {
		label = GetComponent<GUIText> ();
		airship = GameObject.FindGameObjectWithTag ("Player").GetComponent<Airship> ();
		label.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (airship.IsFlyingDanegerious)
		{
			label.enabled = true;
			label.text = airship.transform.position.y > 0 ? highWarning : lowWarning;
		}
		else
		{
			label.enabled = false;
		}
	}
}
