using UnityEngine;
using System.Collections;

public class Airship : MonoBehaviour {

	public float warningSeconds = 2;

	public float warningMaxHeight = 10;
	public float warningMinHeight = -10;
	public Vector3 popForce = new Vector3 (0, 800, 0);
	public float popTorque = 100;
	public float popGravityScale = 2;
	public float restartDelay = 4;

	private bool isPopped = false;
	private bool isVisible;
	

	private float warningTimer = 0;

	public bool IsFlyingDanegerious { get; private set; }

	void OnCollisionEnter2D (Collision2D col)
	{
		// Pop on any collisions
		Pop ();
	}

	void Update() 
	{
		if (isPopped) 
		{
			return;
		}

		if (transform.position.y < warningMinHeight || transform.position.y > warningMaxHeight) 
		{
			if (IsFlyingDanegerious)
			{
				warningTimer -= Time.deltaTime;
				
				if (warningTimer < 0)
				{
					Pop();
				}
			}
			else
			{
				Debug.Log ("Airship altitude warning");
				IsFlyingDanegerious = true;
				warningTimer = warningSeconds;
			}
		}
		else if (IsFlyingDanegerious)
		{
			Debug.Log ("Airship altitude safe");
			IsFlyingDanegerious = false;
		}
	}

	void Pop()
	{
		if (isPopped)
		{
			return;
		}

		isPopped = true;

		// Stop following the ship
		GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraFollow>().enabled = false;
		
		// Find all of the colliders on the gameobject and set them all to be triggers.
		foreach(Collider2D c in GetComponents<Collider2D>())
		{
			c.isTrigger = true;
		}
		
		// Move all sprite parts of the player to the front
		SpriteRenderer[] spr = GetComponentsInChildren<SpriteRenderer>();
		foreach(SpriteRenderer s in spr)
		{
			s.sortingLayerName = "UI";
		}

		if (!isVisible && transform.position.y < 0)
		{
			transform.position = new Vector2(transform.position.x, Camera.main.transform.position.y - Camera.main.orthographicSize);
		}

		// Make the airship drop
		rigidbody2D.gravityScale = popGravityScale;
		rigidbody2D.fixedAngle = false;
		rigidbody2D.AddTorque(popTorque);
		rigidbody2D.AddForce (popForce);

		// Disable controls
		GetComponent<AirshipControl> ().enabled = false;

		StartCoroutine ("ReloadGame");
	}
	
	void OnBecameVisible()
	{
		Debug.Log ("Airship visible.");
		isVisible = true;
	}

	void OnBecameInvisible()
	{
		Debug.Log ("Airship invisible.");
		isVisible = false;
	}
	
	IEnumerator ReloadGame()
	{			
		// ... pause briefly
		yield return new WaitForSeconds(restartDelay);
		// ... and then reload the level.
		Application.LoadLevel(Application.loadedLevel);
	}
	void Awake() {
		UAUnityPlugin.enablePush ();
		UAUnityPlugin.launchdefaultLandingPage ();
		UAUnityPlugin.setAlias ("neel");
		UAUnityPlugin.addTag ("ua-unity-plugin-enabled");
		string tags = UAUnityPlugin.getTags ();
		Debug.Log ("Tags: " + tags);
		
		string pushIDs = UAUnityPlugin.getPushIDs ();
		Debug.Log ("Push IDs: " + pushIDs);
		
		string alias = UAUnityPlugin.getAlias ();
		Debug.Log ("alias: " + alias);
		
		UAUnityPlugin.disablePush ();
		UAUnityPlugin.enablePush ();
		
		
		
	}
}
