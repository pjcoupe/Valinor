using UnityEngine;
using System.Collections;

public enum MovementType
{
	None,
	Grounded_Skid,
	Grounded_Walk,
	Grounded_Run,
	Grounded_Clamber,
	Grounded_Climb,
	Rope_Climb,
	Grounded_Crawl,
	Grounded_Jump,
	Water_Swim,
	Water_Paddle,
	Air_Fly
}

public class AnimatorRootMover : MonoBehaviour {

	private HumanoidInfo humanoidInfo;
	private CustomInfo[] whatAmIWalkingOn;
	private CustomInfo[] whatIsMyHandTouching;

	private Rigidbody2D rbody;
	private Vector2 velocity;
	private MovementType movementType = MovementType.None;
	private float _animationSpeed = 1f;
	private Animator animator;
	private Vector2[] footHitNormal;
	private Vector2[] handHitNormal;
	private Transform root;


	public void AddFootCustomInfo(CustomInfo c)
	{
		string footName = c.myName;
		int i=footName.Contains("Left")?0:1;
		this.footHitNormal[i] = c.avgNormal;
		whatAmIWalkingOn[i].Add(c);

	}

	public void AddHandCustomInfo(CustomInfo c)
	{
		string handName = c.myName;
		int i=handName.Contains("Left")?0:1;
		this.handHitNormal[i] = c.avgNormal;
		whatIsMyHandTouching[i].Add(c);
		
	}

	public float AnimationSpeed
	{
		get
		{
			return _animationSpeed;
		}
		set
		{
			if (value != _animationSpeed)
			{
				_animationSpeed = value;
				animator.speed = _animationSpeed;
			}
		}

	}

	public void Awake()
	{
		root = transform.root;
		humanoidInfo = root.GetComponent<HumanoidInfo>();
		humanoidInfo.rootMover = this;
		rbody = root.GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();	
		whatAmIWalkingOn = new CustomInfo[humanoidInfo.numberOfFeet];
		whatIsMyHandTouching = new CustomInfo[humanoidInfo.numberOfHands];
		footHitNormal = new Vector2[humanoidInfo.numberOfFeet];
		handHitNormal = new Vector2[humanoidInfo.numberOfHands];
	}

	// SetMotion is called by animation event
	public void SetMotion(MovementType t)
	{
		if (t != movementType)
		{
			movementType = t;
			switch (t)
			{
			case MovementType.Grounded_Clamber:				
				float climbDir = humanoidInfo.ClimbOrClamberIsDown ? -4f : 4f;
				velocity = new Vector2(0,climbDir * humanoidInfo.scale * AnimationSpeed * humanoidInfo.LeftLegSize);
				break;
			case MovementType.Grounded_Run:
				velocity = new Vector2(-4f * humanoidInfo.scale * AnimationSpeed * humanoidInfo.LeftLegSize, 0);
				break;
			case MovementType.Grounded_Walk:
				velocity = new Vector2(-1f * humanoidInfo.scale * AnimationSpeed * humanoidInfo.LeftLegSize, 0);
				break;
			default:
				velocity = Vector2.zero;
				break;
			}
		}
	}


	private bool isGrounded = false;
	private bool doVelocity = false;
	private bool doRotation = false;
	private Vector3 angleAxis = Vector3.back;
	private Vector2 theFootHitNormal = Vector2.zero;
	private Vector2 theHandHitNormal = Vector2.zero;
	private int groundedFeetCount = 0;
	private int groundedHandCount = 0;
	private float nextClearTime;
	private const int NEXT_CLEAR_TIME_MSECS = 10;

	public void FixedUpdate()
	{
		bool sticky = humanoidInfo.stickyFeet;
		isGrounded = doVelocity = doRotation = false;
		float angularVelocity = 0;
		Vector2 normal = Vector2.zero;
		theFootHitNormal = theHandHitNormal = Vector2.zero;
		groundedFeetCount = groundedHandCount = 0;

		int i;
		for (i=0; i < humanoidInfo.numberOfFeet; i++)
		{
			if (whatAmIWalkingOn[i].isGrounded)
			{
				isGrounded = true;
				groundedFeetCount++;
				theFootHitNormal += footHitNormal[i];
			}
		}

		if (groundedFeetCount > 0)
		{
			theFootHitNormal /= groundedFeetCount; // take avg
			normal = theFootHitNormal;
		}
		else
		{
			// feet take priority over hands
			for (i=0; i < humanoidInfo.numberOfHands; i++)
			{
				if (whatIsMyHandTouching[i].isGrounded)
				{
					isGrounded = true;
					groundedHandCount++;
					theHandHitNormal += handHitNormal[i];
				}
			}
			if (groundedHandCount > 0)
			{
				theHandHitNormal /= groundedHandCount; // take avg
				normal = theHandHitNormal;
			}
		}
		float ninety = 0f;

		if (sticky)
		{
			if (isGrounded)
			{
				nextClearTime = Time.time + NEXT_CLEAR_TIME_MSECS;
				humanoidInfo.StickyFeet(normal);
			}
			else
			{
				if (Time.time > nextClearTime)
				{
					humanoidInfo.ClearMyGravity();
				}
			}
		}

		if (isGrounded)
		{
			float groundedAngle = Vector2.up.SignedAngle(normal);
			switch (movementType)
			{
				case MovementType.Grounded_Clamber:		
					doVelocity = true;
					doRotation = true;
					groundedAngle -= 90f;
					angleAxis = Vector3.back;
					break;

				case MovementType.Grounded_Run:
				case MovementType.Grounded_Walk:
					doVelocity = true;
					doRotation = sticky;
					angleAxis = Vector3.back;		
					break;
				default:
					break;
			}


			if (doVelocity)
			{
				Quaternion rot = Quaternion.AngleAxis(groundedAngle, angleAxis);			
				Vector2 angledVector = rot * velocity;
				rbody.velocity = humanoidInfo.Direction * angledVector;
			}
			if (doRotation)
			{
				rbody.rotation = -groundedAngle;
			}
		}
		else
		{

		}
		for (i=0; i < humanoidInfo.numberOfFeet; i++)
		{
			whatAmIWalkingOn[i] = default(CustomInfo);
		}
		for (i=0; i < humanoidInfo.numberOfHands; i++)
		{
			whatIsMyHandTouching[i] = default(CustomInfo);
		}

	}
}
