using UnityEngine;
using System.Collections;
using System;


public class PlayerMovement : MonoBehaviour {

	internal static int facing { get; private set; }
	float changeDirSpeedThreshold = 1f;
	float sqrMaxVelocity = 1800f;
	private Vector3 acceleration;
	private Vector3 lastAcceleration;
	private bool accelerationValid = false;
	private DateTime lastChangeDir;

	void Start()
	{
		facing = 1;
	}

	void Update()
	{
		CameraFollow.View(transform);


	}

	void FixedUpdate () 
	{
		Vector3 speed = PlayerInfo.packRigidbody.velocity;
		float sqr = speed.sqrMagnitude;
		if (sqr > 10)
		{
			PlayerInfo.packRigidbody.drag = sqr / sqrMaxVelocity;
		}
		else
		{
			PlayerInfo.packRigidbody.drag = 0;
		}
		accelerationValid = Math3d.LinearAcceleration(out acceleration, transform.position, 5);
		if (accelerationValid)
		{
			lastAcceleration = acceleration;
			for (int i=0; i < 2; i++)
			{
				PlayerInfo.legs[i].eulerAngles = new Vector3(0,0,lastAcceleration.x);
				Debug.Log ("ACCEL "+lastAcceleration.x);
			}
		}
		
		//Debug.Log(" sp "+speed.magnitude );
		int speedSign = (int)Mathf.Sign(speed.x);
		//Debug.Log ("SpeedSign " + speedSign + " facing " + facing);
		if (speedSign != facing && Mathf.Abs(speed.x) > changeDirSpeedThreshold)
		{
			if (DateTime.Now.Subtract(lastChangeDir).TotalSeconds > 1)
			{
				//Debug.Log ("CHANGE DIR to "+speedSign + " time " + DateTime.Now.Second);
				facing = speedSign;
				Vector3 scale = transform.localScale;
				transform.localScale = new Vector3(-scale.x,scale.y,scale.z);
				scale = PlayerInfo.thrusterTransform.localScale;
				PlayerInfo.thrusterTransform.localScale = new Vector3(-scale.x,scale.y,scale.z);

				lastChangeDir = System.DateTime.Now;
			}
		}
		//Debug.Log ("Speed "+speed+" sqrMag "+sqr);
	}
}
