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

[ExecuteInEditMode]
public class SimpleCCD : MonoBehaviour
{
	private int iterations = 5;
	
	[Range(0.01f, 1)]
	public float damping = 1;
	public float rotationOffset = 0;
	public bool isFoot = false;
	public bool iterationMethod = false;
	public static bool muteAll = false;
	public Transform target;
	public Transform endTransform;
	public bool restoreDefaults = false;

	private static List<SecondJointRelLengthAndDistance> jointCache;

	public Node[] angleLimits = new Node[0];

	Vector3 defaultRelativeTargetPosition;
	List<Node> defaultPositions = null;
	Dictionary<Transform, Node> nodeCache = null; 
	[System.Serializable]
	public class Node
	{
		public Transform Transform;
		public float min;
		public float max;
	}

	private int GetJointCacheIndex()
	{
		// normalise
		int secondJointRelLengthTimes100 = Mathf.RoundToInt(endNodeDistance * 100 / firstNodeDistance);
		int lowestLevel = 999;
		int lowestLevelIndex = 0;
		for (int i=0; i < jointCache.Count; i++)
		{
			int level = jointCache[i].lastLevelUsed;
			int secJoint = jointCache[i].secondJointRelLengthTimes100;
			if (level <= lowestLevel || secJoint == 0)
			{
				lowestLevel = level;
				lowestLevelIndex = i;
			}
			if (secJoint != 0 && secJoint == secondJointRelLengthTimes100)
			{
				return i;
			}
		}
		// not found so add one
		/*
		firstNodeDistance = 1f;
		endNodeDistance = 3f;
		minDistance = 2f;
		maxDistance = 4f;
*/

		float a2, b2, abTimes2, a2Plusb2, a2Minusb2, d2, maxMinusMinDist, cosD, cosB, B, D;
		a2 = firstNodeDistance * firstNodeDistance;
		b2 = endNodeDistance * endNodeDistance;
		abTimes2 = 2 * firstNodeDistance * endNodeDistance;
		a2Plusb2 = a2 + b2;
		a2Minusb2 = a2 - b2;

		SecondJointRelLengthAndDistance j = jointCache[lowestLevelIndex];

		maxMinusMinDist = maxDistance - minDistance;
		for (byte i=0; i <= 99; i++)
		{
			float dist = minDistance + (maxMinusMinDist * (float)i/100f);
			d2 = dist * dist;
			cosB = (a2Minusb2 + d2) / (2*firstNodeDistance*dist);
			cosD = (a2Plusb2 - d2) / abTimes2;
			B = Mathf.Acos(cosB) * Mathf.Rad2Deg;
			D = Mathf.Acos(cosD) * Mathf.Rad2Deg;

			j.cachedRotations[i] = new Vector2(B, D);
			//Debug.Log("PJC "+ transform.name+"  firstNode="+firstNodeDistance+" lastNode="+endNodeDistance+" i="+i+" dist = "+dist+" minDist="+minDistance+" maxDist="+maxDistance+" arm="+D+" should="+B);
		}
		j.cachedRotations[100] = new Vector2(0, 180f);
		j.lastLevelUsed = LevelManager.currentLevel;
		j.secondJointRelLengthTimes100 = secondJointRelLengthTimes100;
		return lowestLevelIndex;
	}

	private const int maxListElements = 10;
	private int jointCacheIndex = -1;
	static SimpleCCD()
	{
		if (jointCache == null || jointCache.Count == 0)
		{
			jointCache = new List<SecondJointRelLengthAndDistance>(maxListElements); // PJC change this to bigger values as add more bad guy types with diff arm/leg ratio length
			for (int i=0; i < maxListElements; i++)
			{
				SecondJointRelLengthAndDistance j = new SecondJointRelLengthAndDistance();
				j.cachedRotations = new Dictionary<byte, Vector2>(101);
				j.secondJointRelLengthTimes100 = 0;
				jointCache.Add(j);
			}
		}
	}

	void OnValidate()
	{
		// min & max has to be between 0 ... 360
		foreach (var node in angleLimits)
		{
			node.min = Mathf.Clamp (node.min,0, 360);
			node.max = Mathf.Clamp (node.max,0, 360);
		}
	}

	public void RestoreDefaultPositions()
	{
		foreach (Node n in defaultPositions)
		{
			n.Transform.localEulerAngles = new Vector3(0, 0, n.min);
		}
		target.position = transform.position - defaultRelativeTargetPosition;
	}

	private float maxDistance = 0;
	private float minDistance = 0;
	private float distanceSpread = 0;
	private LockFootRotation lockEndTransformRotation = null;
	private float endNodeDistance, firstNodeDistance;
	private SecondJointRelLengthAndDistance myJoint;

	void Start()
	{

		// Cache optimization
		defaultPositions = new List<Node>();
		defaultRelativeTargetPosition = transform.position - target.position;
		nodeCache = new Dictionary<Transform, Node>(angleLimits.Length);
		foreach (var node1 in angleLimits)
		{
			if (!nodeCache.ContainsKey(node1.Transform))
			{
				nodeCache.Add(node1.Transform, node1);
			}
		}
		lockEndTransformRotation = target.gameObject.GetComponent<LockFootRotation>();

		Transform node = endTransform.parent;
		endNodeDistance = endTransform.localPosition.magnitude;
		firstNodeDistance = 0;
		maxDistance = endNodeDistance;
		while (true)
		{
			Node angLimit = null;
			nodeCache.TryGetValue(node, out angLimit);

			Node def = new Node();
			def.Transform = node;
			float minOrMax = ClampAngle(node.localEulerAngles.z,0,360);
			minOrMax = Mathf.Max ((angLimit==null)? 0: angLimit.min, minOrMax);
			minOrMax = Mathf.Min ((angLimit == null)? 360: angLimit.max, minOrMax);
			def.min = minOrMax;
			def.max = minOrMax;
			defaultPositions.Add(def);
			if (node == transform)
				break;
			firstNodeDistance = node.localPosition.magnitude;
			maxDistance += firstNodeDistance;
			node = node.parent;
        }        
		minDistance = Mathf.Abs(firstNodeDistance - endNodeDistance);
		distanceSpread = maxDistance - minDistance;
		if (!iterationMethod)
		{
			jointCacheIndex = GetJointCacheIndex();
			myJoint = jointCache[jointCacheIndex];
		}
		//Debug.Log("PJC REMOVE  " + transform.name+ " MIN "+minDistance+" max "+maxDistance);
    }
   
    
	private Vector3 lastTargetPosition;
	private Vector3 lastTransformPosition;
    void LateUpdate()
    {

        if (muteAll || target == null || endTransform == null)
			return;

		if (!Application.isPlaying)
			Start();

		if (restoreDefaults)
		{
			restoreDefaults = false;
			RestoreDefaultPositions();
		}
		Vector3 currentTargetPosition = target.position;
		Vector3 currentTransformPosition = transform.position;
		if (currentTargetPosition == lastTargetPosition && lastTransformPosition == currentTransformPosition)
		{
			return;
		}
		lastTargetPosition = currentTargetPosition;
		lastTransformPosition = currentTransformPosition;
		Vector3 dist = currentTransformPosition - currentTargetPosition;
		float distMagnitude = dist.magnitude;
		if (distMagnitude > maxDistance)
		{
			dist = Vector3.ClampMagnitude(dist, maxDistance);
			distMagnitude = maxDistance;
		}
		else if (distMagnitude < minDistance)
		{
			dist = Vector3.ClampMagnitude(dist, minDistance);
			distMagnitude = minDistance;
		}
		if (iterationMethod)
		{
			int i = 0;
			Debug.Log("Calculating "+transform.name + "rot before ="+transform.localEulerAngles);
			TargetPosition = currentTransformPosition - dist;
			while (i < iterations)
			{
				CalculateIK();
				i++;
			}
			Debug.Log("AFTER "+transform.name + "rot ="+transform.localEulerAngles);
		}
		else
		{
			float rootYRot = Mathf.Sign(transform.root.lossyScale.x);

			int distLookup = Mathf.RoundToInt( 100f * (distMagnitude - minDistance) / distanceSpread);
			float signedAngle = Vector3.Angle (Vector2.up, dist);
			float signX = Mathf.Sign(dist.x);
			float signY = Mathf.Sign(dist.y);

			if (signX < 0)
			{
				signedAngle = -signedAngle;
			}
			Debug.Log("Sign X"+signX+" SignY "+signY + " SIGNED "+signedAngle + " transformRot "+transform.name+" "+transform.parent.eulerAngles.z);
			float angle = rootYRot * target.eulerAngles.z + signedAngle +transform.parent.eulerAngles.z;

			TargetPosition = transform.position - dist;

			myJoint.lastLevelUsed = LevelManager.currentLevel;
			Vector2 angles = myJoint.cachedRotations[(byte)distLookup];


			float mainAngle, childAngle;
			if (isFoot)
			{
				if (rootYRot == 1f)
				{
					mainAngle = -angle - angles.x;
					childAngle = -180f - angles.y;
				}
				else
				{
					mainAngle = angle - angles.x;
					childAngle = 180f - angles.y;
				}
			}
			else
			{
				if (rootYRot == 1f)
				{
					mainAngle = -angle + angles.x;
					childAngle = 180f + angles.y;
				}
				else
				{
					mainAngle = angle + angles.x;
					childAngle = -180f + angles.y;
				}
			}
			transform.localEulerAngles = new Vector3(0,0, mainAngle + rotationOffset);
			endTransform.parent.localEulerAngles = new Vector3(0,0, childAngle);
		}

		if (lockEndTransformRotation != null && lockEndTransformRotation.jointToLock != null)
		{
			lockEndTransformRotation.jointToLock.localEulerAngles = new Vector3(0, 0, lockEndTransformRotation.angle);
		}
	}

	void CalculateIK()
	{		
		Transform node = endTransform.parent;
		float signAndDamping =  Mathf.Sign(transform.root.localScale.x) * damping;
		while (true)
		{
			RotateTowardsTarget (signAndDamping, node);

			if (node == transform)
				break;

			node = node.parent;
		}
	}

	private Vector3 TargetPosition { get; set; }
	
	void RotateTowardsTarget(float signAndDamping, Transform transform)
	{		
		Vector2 toTarget = TargetPosition - transform.position;
		Vector2 toEnd = endTransform.position - transform.position;
		
		// Calculate how much we should rotate to get to the target
		float angle = -((SignedAngle(toEnd, toTarget) * signAndDamping) - transform.eulerAngles.z);
		
		// Take care of angle limits 
		if (nodeCache.ContainsKey(transform))
		{
			// Clamp angle in local space
			var node = nodeCache[transform];
			float parentRotation = transform.parent ? transform.parent.eulerAngles.z : 0;
			angle -= parentRotation;
			if (lockEndTransformRotation != null && transform == lockEndTransformRotation.jointToLock)
			{
				angle = ClampAngle(angle, lockEndTransformRotation.angle, lockEndTransformRotation.angle);
			}
			else
			{
				angle = ClampAngle(angle, node.min, node.max);
			}

			angle += parentRotation;
		}
		angle = angle % 360;
		transform.eulerAngles = new Vector3(0, 0, angle);
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
