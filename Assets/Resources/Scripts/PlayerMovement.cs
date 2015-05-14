using UnityEngine;
using System.Collections;
using System;


public class PlayerMovement : MonoBehaviour {

	internal static int facing { get; private set; }
	float changeDirSpeedThreshold = 1f;
	float sqrMaxVelocity = 1800f;

	private DateTime lastChangeDir;
	private const int maxVels = 16;
	private int velIndex = 0;
	private Vector2[] vels = new Vector2[maxVels];
	private float lastHeadAngle = 0;
	private float lastThighAngle = 0;
	private float lastLegAngle = 0;
	private float lastArmAngle = 0;
	private float lastShoulderAngle = 0;
	private IsGrounded[] bootsGrounded = null;

	private Vector2 acceleration = Vector2.zero;
	private float LegAngle 
	{ 
		get
		{
			return facing * acceleration.x;

		}
	}
	private float ArmAngle 
	{ 
		get
		{
			return facing * acceleration.x;
			
		}
	}
	private float HeadAngle 
	{ 
		get
		{
			return -facing * acceleration.x;
			
		}
	}
	private float ThighAngle 
	{ 
		get
		{
			return  acceleration.y;
			
		}
	}
	private float ShoulderAngle 
	{ 
		get
		{
			return  acceleration.y;
			
		}
	}

	void Start()
	{
		facing = 1;
		bootsGrounded = gameObject.GetComponentsInChildren<IsGrounded>();
	}

	Vector2 lastVel = Vector2.zero;
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
		
		int speedSign = (int)Mathf.Sign(speed.x);
		if (speedSign != facing && Mathf.Abs(speed.x) > changeDirSpeedThreshold)
		{
			if (DateTime.Now.Subtract(lastChangeDir).TotalSeconds > 1)
			{
				facing = speedSign;
				Vector3 scale = transform.localScale;
				transform.localScale = new Vector3(-scale.x,scale.y,scale.z);
				scale = PlayerInfo.thrusterTransform.localScale;
				PlayerInfo.thrusterTransform.localScale = new Vector3(-scale.x,scale.y,scale.z);
				lastChangeDir = System.DateTime.Now;
			}
		}
		float headAngle = HeadAngle;
		if (headAngle != lastHeadAngle)
		{
			//Debug.Log("arm "+armAngle);
			headAngle = Mathf.Clamp(headAngle,-10,10f);
			PlayerInfo.head.localEulerAngles = new Vector3(0, 0, headAngle);
			lastHeadAngle = headAngle;
		}
		float armAngle = ArmAngle;
		if (armAngle != lastArmAngle)
		{
			//Debug.Log("arm "+armAngle);
			armAngle = Mathf.Clamp(armAngle,-10,10f);
			for (int i=0; i < 2; i++)
			{
				PlayerInfo.arms[i].localEulerAngles = new Vector3(0, 0, armAngle);
			}
			lastArmAngle = armAngle;
		}
		float legAngle = LegAngle;
		if (legAngle != lastLegAngle)
		{
			//Debug.Log("LEG "+legAngle);
			legAngle = Mathf.Clamp(legAngle,-45f,90f);
			for (int i=0; i < 2; i++)
			{
				PlayerInfo.legs[i].localEulerAngles = new Vector3(0, 0, legAngle);
			}
			lastLegAngle = legAngle;
		}

		float thighAngle = ThighAngle;
		if (thighAngle != lastThighAngle)
		{
			//Debug.Log("thigh "+thighAngle);
			thighAngle = Mathf.Clamp(thighAngle,-30f,30f);
			for (int i=0; i < 2; i++)
			{
				PlayerInfo.thighs[i].localEulerAngles = new Vector3(0, 0, thighAngle);
			}
			lastThighAngle = thighAngle;
		}
		float shoulderAngle = ShoulderAngle;
		if (shoulderAngle != lastShoulderAngle)
		{
			//Debug.Log("thigh "+thighAngle);
			shoulderAngle = Mathf.Clamp(thighAngle,0f,20f);
			for (int i=0; i < 2; i++)
			{
				PlayerInfo.shoulders[i].localEulerAngles = new Vector3(0, 0, shoulderAngle);
			}
			lastShoulderAngle = shoulderAngle;
		}

		Vector2 vel = PlayerInfo.packRigidbody.velocity;
		vels[velIndex] = vel;
		Vector2 oldVel = Vector2.zero, newVel = Vector2.zero;
		int oldVelIndex = (velIndex + 1 + (maxVels / 2)) % maxVels;
		int newVelIndex = (velIndex + 1) % maxVels;
		for (int i=0; i < maxVels / 2; i++)
		{
			oldVel += vels[oldVelIndex];
			oldVelIndex = (oldVelIndex + 1) % maxVels;
			newVel += vels[newVelIndex];
			newVelIndex = (newVelIndex + 1) % maxVels;
		}
		oldVel /= (maxVels / 2);
		newVel /= (maxVels / 2);
		velIndex = (velIndex + 1) % maxVels;
		Vector2 changeInVelocity = (newVel - oldVel);
		float deltaTime = 3 * Time.fixedDeltaTime;
		acceleration = changeInVelocity / deltaTime;

		bool grounded = false;
		foreach (var bootGrounded in bootsGrounded)
		{
			grounded = grounded || bootGrounded.isGrounded;
			if (grounded)
			{
				acceleration = Vector2.zero;
				break;
			}
		}
		//Debug.Log("acc "+acceleration + " vel="+rigidbody2D.velocity + " grounded "+grounded);

	}
}
