using UnityEngine;
using System.Collections;

public class FlyingObsticle : MonoBehaviour {

	public Vector2 maxCycleVelocity;
	public Vector2 minCycleVelocity;
	public float cycleTime;

	private float time;

	void Awake ()
	{
		enabled = false;
	}

	void OnBecameVisible ()
	{
		enabled = true;
	}

	void OnBecameInvisible ()
	{
		Destroy (this);
	}

	void Update ()
	{
		time = (time + Time.deltaTime) % cycleTime;

		float t = time / cycleTime;
		float vx = t * (maxCycleVelocity.x - minCycleVelocity.x) + minCycleVelocity.x;
		float vy = t * (maxCycleVelocity.y - minCycleVelocity.y) + minCycleVelocity.y;

		rigidbody2D.velocity = new Vector2 (vx, vy);
	}
}
