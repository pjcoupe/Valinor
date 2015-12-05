using UnityEngine;
using System.Collections;

public class WeaponAttach : MonoBehaviour {

	public enum TargetType
	{
		NoneOrDirectlyAttached = 0,
		HasIKTargetDirectly = 1, // e.g. a chest weapon only needing a button press by left hand for example
		HasIKTargetIndirectly = 2,
	}
	public enum PivotEndTarget
	{
		None,
		RightHand,
		LeftHand,
		RightFoot,
		LeftFoot
	}

	// targetting type
	public float hasBallisticTargettingAboveDistanceSquared = 1000f;
	public float maxRangeSquared = 2500f;
	public float missileSpeed = 20f;
	public bool parentPivotToHigherWeightParent = true;
	public Transform target;
	public float trackingTimeDelay = 0.1f;
	public bool trackTarget = true;
	public float predictiveLeadSeconds = 0.1f;


	// aiming pivot for IK hand placement
	public string attachToBodyPart = "LeftHand"; // could be middle or Top for a chest mounted weapon and hands are just to press fire button!
	public TargetType rightHand = TargetType.HasIKTargetIndirectly;
	public TargetType leftHand = TargetType.NoneOrDirectlyAttached;
	public TargetType rightFoot = TargetType.NoneOrDirectlyAttached;
	public TargetType leftFoot = TargetType.NoneOrDirectlyAttached;
	public PivotEndTarget pivotEndTarget = PivotEndTarget.LeftHand;
	public string pivotBodyPartNameWeight1 = "LeftShoulder";
	public string pivotBodyPartNameWeight2;
	public float pivotWeight1Proportion = 0.5f;
	public float weaponAttachRotationOffset = 90f;

	public string pivotLengthExtentsNameStart = "LeftShoulder";
	public string pivotLengthExtentsNameEnd = "LeftHand";
	public float pivotLengthExtentsProportion = 1f;
	public float pivotLength { get; private set;}

	private SimpleCCD simpleCCD;

	// private
	private float pivotLengthExtentsMaxDistance = 0;
	private Transform weaponPivotStart;
	private Transform weaponPivotEnd;

	private Vector3 track1;
	private Vector3 track2;
	private bool attached = false;
	private bool attaching = false;

	private bool tracking = false;
	private HumanoidInfo humanoidInfo;

	private delegate Vector3 IKTargetPosition();
	private IKTargetPosition IK_RightHand;
	private IKTargetPosition IK_LeftHand;
	private IKTargetPosition IK_RightFoot;
	private IKTargetPosition IK_LeftFoot;


	private Transform weaponRightHand;
	private Transform weaponLeftHand;
	private Transform weaponRightFoot;
	private Transform weaponLeftFoot;


	private Transform actualIKRightHandTarget;
	private Transform actualIKLeftHandTarget;
	private Transform actualIKRightFootTarget;
	private Transform actualIKLeftFootTarget;

	private float slide = 0;
	public float Slide
	{
		get
		{
			return slide;
		}
		set
		{
			slide = Mathf.Clamp01(value);
			if (simpleCCD)
			{
				simpleCCD.slide = slide;
			}
		}
	}


	private Vector3 finalTarget
	{
		get
		{
			float diffMultiplier = predictiveLeadSeconds/trackingTimeDelay;

			Vector3 newTarget = track2 + (track2 - track1)  * diffMultiplier;
			TestSprite.testSprite.position = newTarget;
			return newTarget;
		}
		
	}
	// Use this for initialization
	void Start () {
		simpleCCD = transform.GetComponentInChildren<SimpleCCD>();

		// find hand targets
		Transform[] children = transform.GetComponentsInChildren<Transform>();

		foreach (Transform t in children)
		{
			weaponLeftHand = (t.name == "IK_LeftHand_Target")?t: weaponLeftHand;
			weaponLeftFoot = (t.name == "IK_LeftFoot_Target")?t: weaponLeftFoot;
			weaponRightHand = (t.name == "IK_RightHand_Target")?t: weaponRightHand;
			weaponRightFoot = (t.name == "IK_RightFoot_Target")?t: weaponRightFoot;
		}
		if (rightHand == TargetType.HasIKTargetDirectly && weaponRightHand)
		{
			IK_RightHand = delegate()
			{
				return weaponRightHand.position; 
			};
		}
		else if (rightHand == TargetType.HasIKTargetIndirectly && simpleCCD)
		{
			IK_RightHand = delegate()
			{
				return simpleCCD.currentTargetPosition; 
			};
		}
		if (rightFoot == TargetType.HasIKTargetDirectly && weaponRightFoot)
		{
			IK_RightFoot = delegate()
			{
				return weaponRightFoot.position; 
			};
		}
		else if (rightFoot == TargetType.HasIKTargetIndirectly && simpleCCD)
		{
			IK_RightFoot = delegate()
			{
				return simpleCCD.currentTargetPosition; 
			};
		}
		if (leftHand == TargetType.HasIKTargetDirectly && weaponLeftHand)
		{
			IK_LeftHand = delegate()
			{
				return weaponLeftHand.position; 
			};
		}
		else if (leftHand == TargetType.HasIKTargetIndirectly && simpleCCD)
		{
			IK_LeftHand = delegate()
			{
				return simpleCCD.currentTargetPosition; 
			};
		}
		if (leftFoot == TargetType.HasIKTargetDirectly && weaponLeftFoot)
		{
			IK_LeftFoot = delegate()
			{
				return weaponLeftFoot.position; 
			};
		}
		else if (leftFoot == TargetType.HasIKTargetIndirectly && simpleCCD)
		{
			IK_LeftFoot = delegate()
			{
				return simpleCCD.currentTargetPosition; 
			};
		}


		//check if already attached to a humanoid
		humanoidInfo = transform.root.GetComponentInChildren<HumanoidInfo>();
		if (humanoidInfo)
		{
			Attach (humanoidInfo.transform);
		}
		else if (PJCREMOVEAttachTo)
		{
			Attach(PJCREMOVEAttachTo);
		}
	}

	public Transform PJCREMOVEAttachTo;
	private Transform root;

	private bool IsNegativeX
	{
		get
		{
			return root.localScale.x < 0;
		}
	}

	private bool atAttachTimeWasNegative;
	public void Attach(Transform attachTo)
	{
		if (attachTo == null || attachTo.root == transform)
		{
			Debug.LogError("Can't attach to null or self");
			return;
		}
		attaching = true;

		root = attachTo.root;
		Transform[] children = attachTo.root.GetComponentsInChildren<Transform>();

		Transform weight1 = null, weight2 = null;
		pivotLength = 0;
		Transform attachPoint = null;
		foreach (Transform t in children)
		{
			if (t.name == attachToBodyPart)
			{
				attachPoint = t;
			}
			if (t.name == pivotLengthExtentsNameEnd)
			{

				pivotLengthExtentsMaxDistance = 0;
				t.VfxParentWalk(pivotLengthExtentsNameStart, (o) => { pivotLengthExtentsMaxDistance += o.localPosition.magnitude; });
				pivotLength = pivotLengthExtentsMaxDistance * pivotLengthExtentsProportion;

			}
			else if (t.name == "IK_Right_Hand_Target")
			{
				actualIKRightHandTarget = t;
			}
			else if (t.name == "IK_Left_Hand_Target")
			{
				actualIKLeftHandTarget = t;
			}
			else if (t.name == "IK_Right_Foot_Target")
			{
				actualIKRightFootTarget = t;
			}
			else if (t.name == "IK_Left_Foot_Target")
			{
				actualIKLeftFootTarget = t;		
			}
			if (weight1 == null & t.name == pivotBodyPartNameWeight1)
			{
				weight1 = t;
			}
			if (weight2 == null & t.name == pivotBodyPartNameWeight2)
			{
				weight2 = t;
			}
		}
		weight1 = weight1?? weight2;
		weight2 = weight2?? weight1;
		atAttachTimeWasNegative = IsNegativeX;
		if (weight1 != null)
		{
			weaponPivotStart = (new GameObject("WeaponPivotStart")).transform;
			weaponPivotStart.position = weight1.position * pivotWeight1Proportion + weight2.position * (1f - pivotWeight1Proportion);
			weaponPivotStart.localRotation = Quaternion.identity;

			if (parentPivotToHigherWeightParent && pivotWeight1Proportion >= 0.5f)
			{
				weaponPivotStart.parent = weight1.parent;
			}
			else
			{
				weaponPivotStart.parent = weight2.parent;
			}
			weaponPivotStart.localScale = Vector3.one;

			weaponPivotEnd = (new GameObject("WeaponPivotEnd")).transform;
			weaponPivotEnd.parent = weaponPivotStart;
			weaponPivotEnd.localScale = Vector3.one;
			weaponPivotEnd.localPosition = weaponPivotStart.TransformVector(new Vector3(atAttachTimeWasNegative?-pivotLength:pivotLength,0,0));

			weaponPivotEnd.rotation = Quaternion.identity;
		}
		if (attachPoint)
		{

			transform.parent = attachPoint;
			transform.localScale = Vector3.one;
			transform.localPosition = Vector3.zero;
			transform.localEulerAngles = new Vector3(0, 0, weaponAttachRotationOffset);
		}


		attached = true;
		attaching = false;
	}

	// Update is called once per frame
	void Update () {
		if (attached)
		{
			if (IK_RightHand != null && actualIKRightHandTarget != null)
			{
				actualIKRightHandTarget.position = IK_RightHand();
			}
			if (IK_LeftHand != null && actualIKLeftHandTarget != null)
			{
				actualIKLeftHandTarget.position = IK_LeftHand();
			}
			if (IK_RightFoot != null && actualIKRightFootTarget != null)
			{
				actualIKRightFootTarget.position = IK_RightFoot();
			}
			if (IK_LeftFoot != null && actualIKLeftFootTarget != null)
			{
				actualIKLeftFootTarget.position = IK_LeftFoot();
			}
			if (target && trackTarget)
			{
				if (!tracking)
				{
					tracking = true;
					track1 = target.position;
					StartCoroutine(TrackTarget(trackingTimeDelay));
				}
			}
		}
	}


	private float lastGoodAngle;
	IEnumerator TrackTarget(float delay)
	{
		yield return new WaitForSeconds(delay);
		if (target != null)
		{
			track2 = target.position;
			if (pivotEndTarget != PivotEndTarget.None)
			{
				float? absAng = RequiredAngleToHitTarget();
				if (absAng.HasValue)
				{
					float absAngle = absAng.GetValueOrDefault();
					if (float.IsNaN(absAngle))
					{
						absAngle = lastGoodAngle;
					}
					lastGoodAngle = absAngle;

					weaponPivotStart.eulerAngles = new Vector3(0,0,absAngle);

					switch (pivotEndTarget)
					{
					case PivotEndTarget.LeftHand:
						actualIKLeftHandTarget.position = weaponPivotEnd.position;

						break;
					case PivotEndTarget.LeftFoot:
						actualIKLeftFootTarget.position = weaponPivotEnd.position;
						break;
					case PivotEndTarget.RightHand:
						actualIKRightHandTarget.position = weaponPivotEnd.position;
						break;
					case PivotEndTarget.RightFoot:
						actualIKRightFootTarget.position = weaponPivotEnd.position;
						break;

					}
				}
			}
			tracking = false;

		}
		yield return null;
	}

	internal float? RequiredAngleToHitTarget()
	{
		Vector3 targetPos = finalTarget;
		bool isNeg = IsNegativeX;
		float x = targetPos.x - weaponPivotStart.position.x;
		float y = targetPos.y - weaponPivotStart.position.y;
		float? final = null;
		float distSquared = x*x + y*y;
		if (distSquared < maxRangeSquared)
		{
			if (distSquared > hasBallisticTargettingAboveDistanceSquared)
			{
				float speedSquared = missileSpeed * missileSpeed;
				float grav = LevelManager.levelGravityMagnitude;
				float underRoot = Mathf.Sqrt(speedSquared * speedSquared - (grav * (grav*x*x + 2*y*speedSquared)));

				if (isNeg)
				{
					final = (Mathf.Atan2((speedSquared - underRoot), (grav*-x)) * Mathf.Rad2Deg) + 2*transform.root.eulerAngles.z;
				}
				else
				{
					final = (Mathf.Atan2((speedSquared - underRoot), (grav*x)) * Mathf.Rad2Deg);
				}
			}
			else
			{
				Vector3 dir = new Vector3(x,y,0);
				final = Mathf.Atan2(dir.y,dir.x) * Mathf.Rad2Deg;
				//transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
			}
		}
		return final;
	}
}
