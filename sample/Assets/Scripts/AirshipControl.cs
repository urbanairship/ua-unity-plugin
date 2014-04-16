using UnityEngine;
using System.Collections;

public class AirshipControl : MonoBehaviour 
{
	public Vector2 moveForce = new Vector2 (100, 100);
	public Vector2 maxVelocity = new Vector2 (5, 5);
	public Vector2 minVelocity = new Vector2 (1, -3);

	public float maxAngle = 45;
	public float minAngle = -30;

	void FixedUpdate ()
	{
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
}
