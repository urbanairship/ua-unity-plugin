using UnityEngine;
using System.Collections;

public class AirshipControl : MonoBehaviour 
{
	public Vector2 moveForce = new Vector2 (100, 100);
	public Vector2 maxVelocity = new Vector2 (5, 5);
	public Vector2 minVelocity = new Vector2 (1, -3);

	public float maxAngle = 45;
	public float minAngle = -30;
	private bool isPopped = false;
	
	void Awake()
	{

	}
	
	void Update()
	{

	}

	void OnBecameInvisible()
	{
		Debug.Log ("Airship Invisible");

		if (isPopped) 
		{
			Debug.Log ("Airship Destroyed");
			StartCoroutine("ReloadGame");
		}
	}

	void FixedUpdate ()
	{
		if (isPopped) 
		{
			return;
		}

		if (Input.GetMouseButton (0) || Input.touchCount > 0) 
		{
			// Add force
			rigidbody2D.AddForce (moveForce);
		}

		// Clamp the velocity
		rigidbody2D.velocity = new Vector2 (Mathf.Clamp (rigidbody2D.velocity.x, minVelocity.x, maxVelocity.x), 
		                                    Mathf.Clamp (rigidbody2D.velocity.y, minVelocity.y, maxVelocity.y));

		
		// Rotate the airship based on current vetical velocity
		float yVelocityPercent = (rigidbody2D.velocity.y - minVelocity.y) / (maxVelocity.y - minVelocity.y);
		float angle = (maxAngle - minAngle) * yVelocityPercent + minAngle;
		transform.rotation = Quaternion.Euler(0, 0, angle);
	}

	void OnCollisionEnter2D (Collision2D col)
	{
		if (!isPopped)
		{

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
			
			// Make the airship drop
			rigidbody2D.gravityScale = 2;
			rigidbody2D.fixedAngle = false;
			rigidbody2D.AddTorque(10);
			
			// Set the isPopped flag
			isPopped = true;
		}
	
	}

	IEnumerator ReloadGame()
	{			
		// ... pause briefly
		yield return new WaitForSeconds(2);
		// ... and then reload the level.
		Application.LoadLevel(Application.loadedLevel);
	}
}
