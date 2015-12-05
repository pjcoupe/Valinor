using UnityEngine;
using System.Collections;
using System.Collections.Generic;


enum BodyPart
{
	None,
	Helmet,
	Eye,
	Nose,
	Ear,
	Mouth,
	Head,
	Neck,
	Body,
	Shoulder,
	Arm,
	Hand,
	Thigh,
	Leg,
	Foot,
	DetectGround,
	Max
}
enum BodyPosition
{
	None,
	Left,
	Right,
	Upper,
	Middle,
	Lower,
	Max
}
enum Uprightness
{
	Standing,
	Crouching,
	Lying
}
enum Motion
{
	None,
	Walking,
	Running
}

enum Fighting
{
	None,
	Shielding,
	Fighting,
	FiringBow,
	Shooting, // not done
	Throwing
}

enum State
{
	Dormant,
	Idling,
	Patrolling,
	Fighting,
	Hiding,
	Falling,
	Dying
}

enum WeaponType
{
	None,
	OneHanded_HandToHand,
	OneHanded_Shield,
	TwoHanded_SmallThrowingWeapon,
	OneHanded_LargeThrowingWeapon,
	TwoHanded_Bow,
	OneHanded_StrikingWeapon,
	TwoHanded_StrikingWeapon,
	OneHanded_ShootingWeapon,
	TwoHanded_ShootingWeapon
}

class InitialAngles
{
	public string name;
	public BodyPart bodyPart;
	public BodyPosition bodyPosition;
	public Transform transform;
	public Sprite sprite;
}
public class PoseController : MonoBehaviour {
	Animator animator;
	InitialAngles[] initialAngles;
	InitialAngles Head;
	InitialAngles BodyMiddle;
	InitialAngles BodyLower;
	InitialAngles BodyUpper;
	InitialAngles LeftFoot;
	InitialAngles RightFoot;
	InitialAngles LeftHand;
	InitialAngles RightHand;
	InitialAngles DetectGround;
	InitialAngles RightThigh;
	InitialAngles LeftThigh;
	InitialAngles RightLeg;
	InitialAngles LeftLeg;

	private int layerMask = 0;
	internal Motion motion = Motion.None;
	internal Fighting fighting = Fighting.None;
	internal Uprightness uprightness = Uprightness.Standing;
	
	internal float Direction { get { return Mathf.Sign(transform.parent.localScale.x); } }
	
	private Transform mainParent;
	private State state = State.Idling;
	public float chanceOfPatrolPerStateDecision = 0.001f;
	internal float firingAccuracyVariationDegrees = 5f;
	internal float maxAngleDiffBeforeAbortingShot = 15f;

	public float sightPerceptionRange = (float)((10 * 2) ^ 2);
	public int maxAmmoCount = -1;
	public int volleyCount = 6; // how many bullets in this rapid volley
	public float nextVolleyBulletDelay = 5f; // delay in between fast volley bullets
	public float newVolleyReloadDelay = 20f; // when volley is expended how long till next volley starts

	private int currentVolleyCount = 0;
	private int ammoUsed = 0;
	private bool reloading;
	private bool checkingPlayerInRange = false;
	public float playerTargetDelay = 1f;
	private float retargetDelay = 0.1f;
	private bool retargeting;
	public bool targetOutOfRange = true;
	private float nextAllowedShotTime = 0;
	public Vector2 lowerLeftBoundPatrolArea = new Vector2(-70f,0);
	public Vector2 upperRightBoundPatrolArea = new Vector2(70f,40f);
	public float stateMachineDecisionDelay = 0.5f;
	public Vector2 patrolTargetPosition;
	private Rigidbody2D mainRigidBody;

	public bool PlayerInPerceptionRange
	{
		get
		{
			return Vector2.Distance(PlayerInfo.playerPosition, transform.position) < sightPerceptionRange;
		}
	}

	private Transform _target;
	private Transform _targetRoot;
	public Transform target
	{
		get
		{
			return _target;
		}

		set
		{
			if (_target != value)
			{
				_target = value;
				_targetRoot = GetRootParent(_target);
			}
		}
	}

	internal float arrowSpeed = 25f;
	private int lastOclock;
	public float lastRequiredAngle {get; private set; }
	private bool _firedArrow;
	public bool firedArrow 
	{
		private get { return _firedArrow; }
		set
		{
			if (_firedArrow != value)
			{
				_firedArrow = value;
				if (_firedArrow)
				{
					ammoUsed++;
					currentVolleyCount++;
				}
			}
		}
	}
	public bool firingArrow {get; private set; }
	public bool abortFire;

	private Transform GetRootParent(Transform t)
	{
		while (t.parent != null)
		{
			t = t.parent;
		}
		return t;
	}

	private float totalLegLength;
	// Use this for initialization
	void Awake () {
		mainRigidBody = transform.parent.GetComponent<Rigidbody2D>();
		Random.seed = (int)(System.DateTime.Now.Ticks % int.MaxValue);
		mainParent = transform;
		while (mainParent.parent != null)
		{
			mainParent = mainParent.parent;
		}
		layerMask = LayerMask.GetMask( new string[]{ "Player", "Objects" } );
		animator = GetComponent<Animator>();

		Transform[] bodyParts = this.GetComponentsInChildren<Transform>();
		initialAngles = new InitialAngles[bodyParts.Length];
		for (int i=0; i < bodyParts.Length; i++)
		{
			SpriteRenderer sr = bodyParts[i].GetComponent<SpriteRenderer>();

			initialAngles[i] = new InitialAngles();
			if (sr != null)
			{
				initialAngles[i].sprite = sr.sprite;
            }
			initialAngles[i].transform = bodyParts[i].transform;
			initialAngles[i].name = bodyParts[i].name;
			initialAngles[i].bodyPart = BodyPart.None;
			initialAngles[i].bodyPosition = BodyPosition.None;
			for (BodyPart j=BodyPart.None + 1; j < BodyPart.Max; j++)
			{
				if (initialAngles[i].name.Contains(j.ToString()))
				{
					initialAngles[i].bodyPart = j;
					break;
				}
			}
			for (BodyPosition j=BodyPosition.None + 1; j < BodyPosition.Max; j++)
			{
				if (initialAngles[i].name.Contains(j.ToString()))
				{
					initialAngles[i].bodyPosition = j;
					break;
				}
			}
			BodyPart bodyPart = initialAngles[i].bodyPart;
			BodyPosition bodyPosition = initialAngles[i].bodyPosition;
			switch (bodyPart)
			{
			case BodyPart.Hand:
				if (bodyPosition == BodyPosition.Left)
				{
					LeftHand = initialAngles[i];
				}
				else
				{
					RightHand = initialAngles[i];
				}
				break;
			case BodyPart.Foot:
				if (bodyPosition == BodyPosition.Left)
				{
					LeftFoot = initialAngles[i];
				}
				else
				{
					RightFoot = initialAngles[i];
				}
				break;
			case BodyPart.Thigh:
				if (bodyPosition == BodyPosition.Left)
				{
					LeftThigh = initialAngles[i];
				}
				else
                {
                    RightThigh = initialAngles[i];
                }
                break;
			case BodyPart.Leg:
				if (bodyPosition == BodyPosition.Left)
				{
					LeftLeg = initialAngles[i];
				}
				else
				{
					RightLeg = initialAngles[i];
                }
                break;
            case BodyPart.Head:
                Head = initialAngles[i];
				break;
			case BodyPart.Body:
				if (bodyPosition == BodyPosition.Middle)
				{
					BodyMiddle = initialAngles[i];				
				}
				else if (bodyPosition == BodyPosition.Upper)
				{
					BodyUpper = initialAngles[i];
				}
				else if (bodyPosition == BodyPosition.Lower)
				{
					BodyLower = initialAngles[i];
                }
				break;
			case BodyPart.DetectGround:
				DetectGround = initialAngles[i];
				break;
			}
		}
		AnimationInfo();
		float thighLen = 1f / RightThigh.sprite.pixelsPerUnit * RightThigh.sprite.textureRect.height;
		float legLen = 1f / RightLeg.sprite.pixelsPerUnit * RightLeg.sprite.textureRect.height;
		float footLen = 1f / RightFoot.sprite.pixelsPerUnit * RightFoot.sprite.textureRect.height;
		totalLegLength = transform.lossyScale.y * (thighLen + legLen + footLen);
		//Debug.LogError("PJC REMOVE thigh="+thighLen+" leg="+legLen+" foot="+footLen+" TOTAL="+totalLegLength);
	}

	void Start()
	{
		target = PlayerInfo.playerTransform;

	}

	private static Dictionary<string, AnimationClip> clips;

	private void AnimationInfo()
	{
	}

	internal void ChangeDirection()
	{
		mainParent.localScale = new Vector3(mainParent.localScale.x * -1f, mainParent.localScale.y, mainParent.localScale.z);
	}

	internal float RequiredAngleToHitTarget(float speed)
	{
		float x = target.position.x - transform.position.x;
		float y = target.position.y - transform.position.y;

		float speedSquared = speed * speed;
		float grav = 10f;//closestRigidbody2D.gravityScale;
		float underRoot = Mathf.Sqrt(speedSquared * speedSquared - (grav * (grav*x*x + 2*y*speedSquared)));
		float toArcTan = (speedSquared - underRoot) / (grav*x);
		float final = Mathf.Atan(toArcTan) * Mathf.Rad2Deg;
		return final;
	}


	void OnGUI()
	{
		/*
		if (GUI.Button(new Rect(10, 10, 50, 20),"Run"))
		{
			TriggerAnimationEvent("Run");
		}
		else if (GUI.Button(new Rect(10, 30, 50, 20),"Walk"))
		{
			TriggerAnimationEvent("Walk");
		}
		else if (GUI.Button(new Rect(10, 50, 50, 20),"Stop"))
		{
			TriggerAnimationEvent("Stop");
		}
		else if (GUI.Button(new Rect(10, 70, 90, 20),"Shoot 6"))
		{
			TriggerAnimationEvent("Bow6");
		}
		else if (GUI.Button(new Rect(10, 90, 90, 20),"Shoot 7"))
		{
			TriggerAnimationEvent("Bow7");
		}
		else if (GUI.Button(new Rect(10, 110, 90, 20),"Shoot 8"))
		{
			TriggerAnimationEvent("Bow8");
		}
		else if (GUI.Button(new Rect(10, 130, 90, 20),"Shoot 9"))
		{
			TriggerAnimationEvent("Bow9");
		}
		else if (GUI.Button(new Rect(10, 150, 90, 20),"Shoot 10"))
		{
			TriggerAnimationEvent("Bow10");
		}
		else if (GUI.Button(new Rect(10, 170, 90, 20),"Shoot 11"))
		{
			TriggerAnimationEvent("Bow11");
		}
		else if (GUI.Button(new Rect(10, 190, 90, 20),"Shoot 12"))
		{
			TriggerAnimationEvent("Bow12");
		}
		*/
	}

	public void Idle()
	{
		Debug.Log("Set Idle");
		motion = Motion.None;
		fighting = Fighting.None;
		animator.SetTrigger("Stop");
	}


	void FixedUpdate()
	{
		if (motion == Motion.Running)
		{
			if (targetRelativeXSign < 0)
			{
				ChangeDirection();
			}
			mainRigidBody.transform.Translate(new Vector2(-Direction * 3 * totalLegLength,0) * Time.fixedDeltaTime);

			//mainRigidBody.velocity = new Vector2(-Direction * 3 * totalLegLength,0);
        }
	}

	// Update is called once per frame
	void Update () {
		//Debug.Log ("OutOfRng "+targetOutOfRange+" firing "+firingArrow + " fired "+firedArrow+" volley "+currentVolleyCount+" amoUsed "+ammoUsed+" reloading "+reloading);

		if (targetOutOfRange)
		{
			//Debug.Log("targetOutOfRange");
			// target out of range do occassional checks only
			if (!checkingPlayerInRange)
			{
				checkingPlayerInRange = true;
				StartCoroutine(CheckForPlayerInRange(playerTargetDelay));
			}
		}
		else // target in range check if we are firing etc
		{
			if (firedArrow)
			{
				//Debug.Log("targetInRange firedArrow");
				 // fired an arrow so have to wait for reload
				if (!reloading)
				{
					reloading = true;
					if (currentVolleyCount >= volleyCount)
					{
						currentVolleyCount = 0;
						StartCoroutine(Reload(newVolleyReloadDelay));
					}
					else
					{
						StartCoroutine(Reload (nextVolleyBulletDelay));
					}
				}
				if (motion != Motion.Running && allIdle)
				{
					motion = Motion.Running;
					animator.SetTrigger("Run");
				}
			}
			else
			{

				// target in range
				if (firingArrow) // firing (pulling back and aiming but NOT yet fired so time to retarget is available				
				{
					//Debug.Log("targetInRange not firedArrow firingArrow retargeting " +retargeting);
					if (!retargeting)
					{
						retargeting = true;
						StartCoroutine(PrepareToFireArrow(false, retargetDelay));	
					}
				}
				else 
				{
					//Debug.Log("targetInRange not firedArrow not firingArrow");
					if (!retargeting)
					{
						retargeting = true;
						StartCoroutine(AllowedToFire(retargetDelay));
					}
				}
			}
		}

	}

	private IEnumerator Reload(float reloadTime)
	{
		//Debug.Log ("Reload");
		motion = Motion.None;
		fighting = Fighting.None;
		yield return new WaitForSeconds(reloadTime);
		reloading = false;
		firedArrow = false;
		firingArrow = false;
	}


	private bool allIdle
	{
		get
		{
			return true; // PJC to do
		}

	}

	private bool AnyObjectBetweenMeAndTarget()
	{

		Vector3 orig = Head.transform.position;
		Vector3 dest = target.position;
		RaycastHit2D hit = Physics2D.Linecast(orig, dest, layerMask);
		//Debug.DrawLine(orig,dest);
		if (hit.collider != null)
		{
			Transform t = GetRootParent(hit.collider.transform);
			if (t == _targetRoot)
			{
				return false;
			}
			else
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerator AllowedToFire(float checkDelay)
	{
		yield return new WaitForSeconds(checkDelay);
		if (maxAmmoCount > -1 && ammoUsed >= maxAmmoCount)
		{
			//Debug.Log("Out of ammo");
		}
		else if (!allIdle || motion != Motion.None)
		{
			//Debug.Log("Not idle so stopping");
			motion = Motion.None;
			animator.SetTrigger("Stop");
		}
		else if (AnyObjectBetweenMeAndTarget()) 
		{
			//Debug.Log("Blocked by object");
		}
		else
		{
			//Debug.Log("Allowed to fire");
			yield return StartCoroutine(PrepareToFireArrow(true, 0));
		}
		retargeting = false;
		yield return null;
	}

	IEnumerator CheckForPlayerInRange(float delaySecs)
	{
		yield return new WaitForSeconds(delaySecs);
		targetOutOfRange = !PlayerInPerceptionRange;
		checkingPlayerInRange = false;
	}

	private float targetRelativeXSign { get { return Mathf.Sign(transform.position.x - target.position.x) * Mathf.Sign(transform.lossyScale.x); } }

    IEnumerator PrepareToFireArrow(bool isInitial, float delaySecs)
	{
		float aim;
		if (isInitial)
		{
			if (!allIdle)
			{
				animator.SetTrigger("Stop");
			}
		}
		yield return new WaitForSeconds(delaySecs);
		if (!abortFire)
		{
			fighting = Fighting.FiringBow;
			motion = Motion.None;

			float sign = Mathf.Sign(transform.lossyScale.x);
			if (targetRelativeXSign < 0)
			{
				ChangeDirection();
			}
			float requiredAngle = RequiredAngleToHitTarget(arrowSpeed);

			aim = (requiredAngle - transform.rotation.eulerAngles.z) * -sign;
			int oclock = Mathf.RoundToInt((aim / 30f) + 9f);

			if (!firingArrow && oclock >=6 && oclock <= 12)
			{
				lastOclock = oclock;
				lastRequiredAngle = requiredAngle;
				//Debug.Log ("Firing initiated");
				animator.SetTrigger("Bow"+lastOclock);
				firingArrow = true;
			}
			else if (!firedArrow)
			{

				if (lastOclock == oclock)
				{
					lastOclock = oclock;
					lastRequiredAngle = requiredAngle;
					//animator.CrossFade("Aim_Bow_"+oclock+"oclock",0f);
				}
			}
			//Debug.Log("Req=" + lastRequiredAngle + " Aim = "+aim + " oclock="+ lastOclock);
		}
		else
		{
			abortFire = false;
			Idle();
		}
		retargeting = false;
		yield return null;
	}
}
