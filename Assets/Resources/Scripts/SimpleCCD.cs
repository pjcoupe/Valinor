using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


internal struct SecondJointRelLengthAndDistance
{
	public int secondJointRelLengthTimes100;
	public Dictionary<byte, Vector2> cachedRotations;
	public int lastLevelUsed;
}

public enum IKType
{
	Foot,
	Hand,
	Bend,
	HeadLook
}

[ExecuteInEditMode]
public class SimpleCCD : MonoBehaviour
{
	private int iterations = 5;
	
	[Range(0f, 1f)]
	public float targetProportion = 1;
	public IKType ikType;
	public int rotationParents = 0;
	public float rotationOffset = 0;
	public bool iterationMethod = false;
	public bool isFoot = false;
	public static bool muteAll = false;
	public Transform target;
	private bool relativeToParent = true;
	public Transform alternateTarget;
	public Transform targetMax;
	public Transform endTransform;
	public bool restoreDefaults = false;
	private Transform root;
	private bool doneInit;

	public Transform[] endTransformMissiles = new Transform[0];

	public Node[] angleLimits = new Node[0];
	
	[System.Serializable]
	public class Node
	{
		public Transform Transform;
		public float min;
		public float max;
	}

	[Range(0,1f)]
	public float slide;

	private const int maxListElements = 1;
	private int jointCacheIndex = -1;


	void OnValidate()
	{
		// min & max has to be between -180 ... 180
		foreach (var node in angleLimits)
		{
			node.min = Mathf.Clamp (node.min,-180, 180);
			node.max = Mathf.Clamp (node.max,-180, 180);
		}
	}

	private float maxDistance = 0;
	private float minDistance = 0;
	private float distanceSpread = 0;
	private LockFootRotation lockEndTransformRotation = null;
	private float endNodeDistance, firstNodeDistance;
	private IKCalculator iKCalculator;

	private HumanoidInfo humanoidInfo = null;
	public Transform mirrorYEndTransform;


	void Start()
	{
		root = transform.root;
		humanoidInfo = root.GetComponent<HumanoidInfo>();	

		if (endTransform == null)
		{
			return;
		}

		Init ();
    }
   
	private void Init()
	{
		if (target != null)
		{
			iKCalculator = target.GetComponent<IKCalculator>();
		}
		
		// Cache optimization

		lockEndTransformRotation = target.gameObject.GetComponent<LockFootRotation>();
		
		
		Transform node = endTransform.parent;
		float scaleF = Mathf.Abs(transform.root.localScale.x);
		endNodeDistance = endTransform.localPosition.magnitude * scaleF;
		firstNodeDistance = 1f;
		maxDistance = endNodeDistance;
		while (true)
		{
			if (node == null || node == transform)
				break;
			firstNodeDistance = node.localPosition.magnitude * scaleF;
			maxDistance += firstNodeDistance;
			node = node.parent;
		}        
		minDistance = Mathf.Abs(firstNodeDistance - endNodeDistance);
		
		distanceSpread = maxDistance - minDistance;

		doneInit = true;
	}
    
	private Vector3 lastTargetPosition;
	private Vector3 lastTransformPosition;
	private int lastDistFloor = -1, lastDistCeil = -1;
	Vector2 lowAngle = Vector2.zero, hiAngle = Vector2.zero;

	public Vector3 currentTargetPosition { get; private set; }


	private void CalculateBendAngle(float dist, float signedAngle)
	{	
		Transform child = endTransform.parent;
		float direction = humanoidInfo.Direction;
		float a2, b2, abTimes2, a2Plusb2, a2Minusb2, d2, cosD, cosB, B, D;
		a2 = firstNodeDistance * firstNodeDistance;
		b2 = endNodeDistance * endNodeDistance;
		abTimes2 = 2f * firstNodeDistance * endNodeDistance;
		a2Plusb2 = a2 + b2;
		a2Minusb2 = a2 - b2;

		d2 = dist * dist;
		cosB = (a2Minusb2 + d2) / (2f * firstNodeDistance*dist);
		cosD = (a2Plusb2 - d2) / abTimes2;

		B = Mathf.Acos(cosB) * Mathf.Rad2Deg;
		D = Mathf.Acos(cosD) * Mathf.Rad2Deg;

		float rootEulerZ = root.eulerAngles.z;
		float eulerZ, childEulerZ;

		if (isFoot)
		{
			if (humanoidInfo.Direction > 0)
			{
				eulerZ = -rootEulerZ -B - (direction * signedAngle);
				childEulerZ = 180f - D;
			}
			else
			{
				eulerZ = rootEulerZ -B - (direction * signedAngle);
				childEulerZ = 180f - D;
			}
		}
		else
		{
			if (humanoidInfo.Direction > 0)
			{
				eulerZ = -rootEulerZ +B - (direction * signedAngle);
				childEulerZ = 180f + D;
			}
			else
			{
				eulerZ = rootEulerZ +B - (direction * signedAngle);
				childEulerZ = 180f + D;
			}
		}
		if (float.IsNaN(eulerZ))
		{
			eulerZ = 0;
		}
		if (float.IsNaN(childEulerZ))
		{
			childEulerZ = 0;
		}
		childEulerZ = childEulerZ.ClampMinus180To180();
		eulerZ = eulerZ.ClampMinus180To180();
		if (true)
		foreach (var limit in angleLimits)
		{
			float minLimit = limit.min;
			float maxLimit = limit.max;
			if (limit.Transform == transform)
			{			
				minLimit = (minLimit + rootEulerZ).ClampMinus180To180();
				maxLimit = (maxLimit + rootEulerZ).ClampMinus180To180();

				maxLimit += maxLimit <= minLimit ? 360f : 0;
				eulerZ += (eulerZ < minLimit)? 360f: (eulerZ > maxLimit)?-360f:0;
				eulerZ = eulerZ % 360f;
				eulerZ = Mathf.Clamp(eulerZ, minLimit, maxLimit);

			}
			else if (limit.Transform == child)
			{
				childEulerZ = Mathf.Clamp(childEulerZ, minLimit, maxLimit);
			}
		}

		transform.eulerAngles = new Vector3(0, 0, eulerZ);
		child.localEulerAngles = new Vector3(0,0, childEulerZ);

    }
    void LateUpdate()
    {


        if (muteAll || (target == null && alternateTarget == null) || endTransform == null)
			return;

		if (!Application.isPlaying)
			Start();
		else
		{
			if (iKCalculator != null && humanoidInfo != null && !humanoidInfo.doneRebuild)
			{
				return;
			}
		}

		Vector3 targetPosition;
		Vector3 adjustedAnimatedTargetPosition = Vector3.zero;

		if (target != null)
		{
			if (iKCalculator != null && targetProportion > 0)
			{
				if (!Application.isPlaying)
				{
					switch (ikType)
					{
					case IKType.Foot:
					case IKType.Hand:				
						adjustedAnimatedTargetPosition = iKCalculator.StartNode.position + root.rotation * (Vector3)(iKCalculator.position*maxDistance);
						break;
					case IKType.Bend:
					case IKType.HeadLook:
						adjustedAnimatedTargetPosition = iKCalculator.StartNode.position + (Vector3)(iKCalculator.position*maxDistance);
						break;
					}
				}
				else
				{
					adjustedAnimatedTargetPosition = iKCalculator.StartNode.position + (root.rotation * (Vector3)(iKCalculator.position*maxDistance));

				}
			}
			else
			{
				adjustedAnimatedTargetPosition = target.position;
			}
		}
		if (name == "RightThigh")
		{


		}
		Vector3 t1 = target == null ? alternateTarget.position : adjustedAnimatedTargetPosition;
		Vector3 t2 = alternateTarget == null ? adjustedAnimatedTargetPosition : alternateTarget.position;
		targetPosition = Vector3.Lerp(t2,t1, targetProportion);

		currentTargetPosition = targetMax? Vector3.Lerp(targetPosition, targetMax.position, slide):targetPosition;
		Vector3 currentTransformPosition = transform.position;
		if (currentTargetPosition == lastTargetPosition && lastTransformPosition == currentTransformPosition)
		{
			return;
		}
		lastTargetPosition = currentTargetPosition;
		lastTransformPosition = currentTransformPosition;
		Vector3 dist = currentTransformPosition - currentTargetPosition;
		float distMagnitude = dist.magnitude;


		if (humanoidInfo != null && !iterationMethod)
		{
			float signedAngle = SignedAngle(Vector3.up, dist);
			float adjustedSignedAngle = signedAngle;
			if (Application.isPlaying)
			{
				adjustedSignedAngle -= root.eulerAngles.z;
			}
			CalculateBendAngle(Mathf.Clamp(distMagnitude, minDistance,maxDistance), adjustedSignedAngle ); //SignedAngle(Vector3.up, dist)
			if (name == "RightThigh")
			{
				Debug.Log("PJC "+name+" pos "+ transform.position+ " rootZ "+ root.eulerAngles.z+" signedAngle "+signedAngle+" adj "+adjustedSignedAngle+" dist "+dist);
				
			}
			return; 
		}

		if (distMagnitude > maxDistance)
		{
			dist = Vector3.ClampMagnitude(dist, maxDistance);
			distMagnitude = maxDistance;
		}
		else if (distMagnitude < minDistance)
		{
			dist = Vector3.Normalize(dist) * minDistance;
			distMagnitude = minDistance;
		}
		TargetPosition = currentTransformPosition - dist;


		int i = 0;

		while (i < iterations)
		{
			CalculateIK();
			i++;
		}


		if (lockEndTransformRotation != null && lockEndTransformRotation.jointToLock != null)
		{
			lockEndTransformRotation.jointToLock.localEulerAngles = new Vector3(0, 0, lockEndTransformRotation.angle);
		}
	}

	private Vector3 saveTransformPos, saveEndTransformPos;
	private Vector3 saveTargetPos;
	void CalculateIK()
	{		

		Transform nodeMirror;
		Transform node = endTransform.parent;
		float direction;
		if (humanoidInfo != null)
		{
			direction = humanoidInfo.Direction;
		}
		else
		{
			direction = Mathf.Sign(transform.root.localScale.x);
		}
		float changedAngle = 0;
		while (true)
		{
			changedAngle = RotateTowardsTarget (direction, node);

			if (node == transform)
				break;

			node = node.parent;
		}
	
		if (mirrorYEndTransform != null)
		{
			nodeMirror = mirrorYEndTransform;
			node = endTransform;
			while (true)
			{

				nodeMirror.localRotation = node.localRotation;
				node = node.parent;
				nodeMirror = nodeMirror.parent;
				if (node == null || node == transform)
				{
					Vector3 loc = transform.localEulerAngles;
					nodeMirror.localEulerAngles = new Vector3(180f, loc.y, loc.z);
					break;
				}
				
			}
		}
		saveTransformPos = transform.position;
		saveEndTransformPos = endTransform.position;
		saveTargetPos = target.position;

	}

	private Vector3 _targetPosition;
	
	private Vector3 TargetPosition 
	{ 
		get
		{
			return _targetPosition;
		}
		set
		{
			_targetPosition = value;
			foreach (Transform t in endTransformMissiles)
			{
				t.position = value;
			}

		}
	}
	
	float RotateTowardsTarget(float signAndDamping, Transform transform)
	{		
		Vector2 toTarget = TargetPosition - transform.position;
		Vector2 toEnd = endTransform.position - transform.position;
		
		// Calculate how much we should rotate to get to the target
		float angle = -((SignedAngle(toEnd, toTarget) * signAndDamping) - transform.eulerAngles.z);
		angle = (angle + rotationOffset) % 360;
		float changeAngle = (angle - transform.eulerAngles.z) % 360;
	    transform.eulerAngles = new Vector3(0, 0, angle);

		// Take care of angle limits 
		foreach (var node in angleLimits)
		{
			if (node.Transform != transform)
			{
				continue;
			}
			// Clamp angle in local space
			float localZ = transform.localEulerAngles.z;
			if (localZ > 180)
			{
				localZ -= 360;
			}
			if (localZ < node.min)
			{
				transform.localEulerAngles = new Vector3(0,0,node.min);
			}
			else if (localZ > node.max)
			{
				transform.localEulerAngles = new Vector3(0,0,node.max);
			}
		}
		return changeAngle;
	}
	
	public static float SignedAngle (Vector3 a, Vector3 b)
	{
		float angle = Vector3.Angle (a, b);
		float sign = Mathf.Sign (Vector3.Dot (Vector3.back, Vector3.Cross (a, b)));
		
		return angle * sign;
	}
	
	float ClampAngle (float angle, float min, float max)
	{
		angle = ((angle % 360) + 360) % 360;
		return Mathf.Clamp(angle, min, max);
	}
}
