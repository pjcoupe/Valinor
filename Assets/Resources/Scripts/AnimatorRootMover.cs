using UnityEngine;
using System.Collections;

public enum HorizontalMovement
{
	Back= -1,
	None,
	Forward
}

public enum VerticalMovement
{
	Down = -1,
	None,
	Up
}

public enum RotationMovement
{
	AntiClockwise = -1,
	None,
	Clockwise
}

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
	Air_Fly,
	Air_Jump,
	Stationary_Animation,
	Moving_Animation
}

public class AnimatorRootMover : MonoBehaviour {

	private const float RUN_FACTOR = 4f;
	private const float WALK_FACTOR = 1f;
	private const float STANDING_JUMP_FACTOR = 6f;
	private HumanoidInfo humanoidInfo;
	private CustomInfo[] whatAmIWalkingOn;
	private CustomInfo[] whatIsMyHandTouching;

	private Rigidbody2D rbody;
	private Vector2 velocity;

	public bool needGroundBeforeVelocityApplied;
	public bool groundedSinceSetMotion;
	public MovementType movementType = MovementType.None;
	public HorizontalMovement horizontalMovement = HorizontalMovement.None;
	public RotationMovement rotationMovement = RotationMovement.None;
	public VerticalMovement verticalMovement = VerticalMovement.None;

	private float _animationSpeed = 1f;
	private Animator animator;
	private Vector2[] footHitNormal;
	private Vector2[] handHitNormal;
	private Transform root;


	private string currentTrigger = null;
	public void SetTrigger(string stringInput)
	{
		if (currentTrigger != stringInput)
		{		
			if (currentTrigger != null)
			{
				animator.ResetTrigger(currentTrigger);
			}
			animator.SetTrigger(stringInput);
			currentTrigger = stringInput;
		}
	}

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

	public void FootHitObject(CustomInfo hitInfo)
	{
		AddFootCustomInfo(hitInfo);
	}
	
	public void HandHitObject(CustomInfo hitInfo)
	{
		AddHandCustomInfo(hitInfo);
	}

	public void Awake()
	{
		root = transform.root;
		humanoidInfo = root.GetComponent<HumanoidInfo>();	
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
		groundedSinceSetMotion = false;
		if (t != movementType)
		{
			movementType = t;
			float horizontalVelocity = 0, verticalVelocity = 0;
			switch (t)
			{
			case MovementType.Grounded_Run:
				needGroundBeforeVelocityApplied = true;
				horizontalVelocity = -RUN_FACTOR * humanoidInfo.scale * AnimationSpeed * Mathf.Max(humanoidInfo.LeftLegSize,humanoidInfo.RightLegSize);
				break;
			case MovementType.Grounded_Walk:
				needGroundBeforeVelocityApplied = true;
				horizontalVelocity = -WALK_FACTOR * humanoidInfo.scale * AnimationSpeed * Mathf.Max(humanoidInfo.LeftLegSize,humanoidInfo.RightLegSize);
				break;				
			case MovementType.Grounded_Jump:
				if (humanoidInfo.isGrounded)
				{
					Vector2 target = humanoidInfo.jumpTarget == null ?(Vector2)root.position + new Vector2(0, humanoidInfo.HeadHeight):(Vector2)humanoidInfo.jumpTarget.position;
					bool outOfRange = false;
					float maxVel = STANDING_JUMP_FACTOR * humanoidInfo.jumpFactor;
					bool jumpEvenIfOutOfRange = ExtensionMethods.passedRandom(humanoidInfo.jumpOutOfRangeLikelihood);
					float minVel = transform.GetMinimumVelocity(target).magnitude;

					float vel = Mathf.Min (minVel,maxVel);
					Debug.Log("JUMP min "+minVel+" max "+maxVel+" final "+vel);
					Quaternion rot = root.GetTrajectory(target, vel, out outOfRange, jumpEvenIfOutOfRange);

					if (outOfRange && !jumpEvenIfOutOfRange)
					{
						animator.SetTrigger(humanoidInfo.DetermineAnimatorTrigger());
						horizontalVelocity = velocity.x;
						verticalVelocity = velocity.y;
					}
					else
					{
						velocity = rot * new Vector3(vel, 0, 0);			
							//Quaternion rot = root.GetTrajectory((target+new Vector2(-1f,0)), STANDING_JUMP_FACTOR);
						//velocity = rot * new Vector3(0, STANDING_JUMP_FACTOR, 0);
						horizontalVelocity = velocity.x;
						verticalVelocity = velocity.y;//STANDING_JUMP_FACTOR * humanoidInfo.scale * AnimationSpeed * humanoidInfo.LeftLegSize;

						humanoidInfo.jumpStart = transform.position;
					}
				}
				else
				{
					Debug.Log("JUMP ABORTED NOT GROUNDED");
					animator.SetTrigger(humanoidInfo.DetermineAnimatorTrigger());
					horizontalVelocity = velocity.x;
					verticalVelocity = velocity.y;
				}
				break;
			case MovementType.Grounded_Clamber:	
				needGroundBeforeVelocityApplied = true;
				float climbDir = humanoidInfo.ClimbOrClamberIsDown ? -4f : 4f;
				verticalVelocity = climbDir * humanoidInfo.scale * AnimationSpeed * Mathf.Max(humanoidInfo.LeftArmSize,humanoidInfo.RightArmSize, humanoidInfo.RightLegSize, humanoidInfo.LeftLegSize);
                break;
			case MovementType.Stationary_Animation:
				break; // ie zero
			case MovementType.Moving_Animation:
			default:
				horizontalVelocity = velocity.x;
				verticalVelocity = velocity.y;
				break;
			}

			velocity = new Vector2(horizontalVelocity, verticalVelocity);
			Debug.Log("PJC SET MOTION "+t+" vel "+velocity);
		}
	}
	
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
		humanoidInfo.isGrounded = doVelocity = doRotation = false;
		float angularVelocity = 0;
		Vector2 normal = Vector2.zero;
		theFootHitNormal = theHandHitNormal = Vector2.zero;
		groundedFeetCount = groundedHandCount = 0;

		int i;
		for (i=0; i < humanoidInfo.numberOfFeet; i++)
		{
			if (whatAmIWalkingOn[i].isGrounded)
			{
				humanoidInfo.isGrounded = true;
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
					humanoidInfo.isGrounded = true;
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

		if (sticky)
		{
			if (humanoidInfo.isGrounded)
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
		float groundedAngle = 0;
		if (humanoidInfo.isGrounded)
		{
			groundedSinceSetMotion = true;

			groundedAngle = Vector2.up.SignedAngle(normal);
			switch (movementType)
			{
			case MovementType.Grounded_Jump:
				doVelocity = true;					
				angleAxis = Vector3.back;		
				break;
			case MovementType.Grounded_Clamber:		
			case MovementType.Grounded_Run:
			case MovementType.Grounded_Walk:
				doVelocity = true;
				doRotation = sticky;
				angleAxis = Vector3.back;		
				break;
			default:
				break;
			}
		}
		else
		{

		}
		// movements not dependent on whether grounded or not
		switch (movementType)
		{
		default:
			break;
		}
		if (doVelocity && (!needGroundBeforeVelocityApplied || (needGroundBeforeVelocityApplied && groundedSinceSetMotion)))
		{
			Quaternion rot = Quaternion.AngleAxis(groundedAngle, angleAxis);			
			Vector2 angledVector = rot * velocity;
			rbody.velocity = humanoidInfo.Direction * angledVector;
		}
		if (doRotation)
		{
			rbody.rotation = -groundedAngle;
		}
		for (i=0; i < humanoidInfo.numberOfFeet; i++)
		{
			whatAmIWalkingOn[i] = default(CustomInfo);
		}
		for (i=0; i < humanoidInfo.numberOfHands; i++)
		{
			whatIsMyHandTouching[i] = default(CustomInfo);
		}

		float yVelocity = rbody.velocity.y;
		humanoidInfo.isFalling = (yVelocity < humanoidInfo.fallThreshhold);
	}
}
