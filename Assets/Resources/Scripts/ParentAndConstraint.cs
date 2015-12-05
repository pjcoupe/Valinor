using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ParentAndConstraint : MonoBehaviour {

	public bool LockXAxis = false;
	public bool LockYAxis = false;
	public float LockedXSpeed = 2f;
	public float LockedYSpeed = 2f;
    
	public Transform RelativeParentTo;
	private bool inited = false;
	private Vector3 lastLocalRotation = Vector3.zero;
	private Transform[] parentOrUncle;
	private Transform myroot;
	private Vector3 IKOrigLocalPos;
	private Vector3 RelParentToOrigLocalPos;


	void Awake()
	{
		IKOrigLocalPos = transform.localPosition;
		RelParentToOrigLocalPos = RelativeParentTo.localPosition;
		myroot = transform.parent.parent;
		lastLocalPosition = IKOrigLocalPos;
		lastLocalRotation = transform.localEulerAngles;		
	}

	// Use this for initialization
	void Start () 
	{
		if (!inited)
		{
			inited = true;
			lastLocalPosition = transform.localPosition;
		}
		int parentOrUncleCount = myroot.childCount;
		parentOrUncle = new Transform[parentOrUncleCount];
		for (int i=0; i < parentOrUncleCount; i++)
		{
			parentOrUncle[i] = myroot.GetChild(i);
		}
	}

	private Vector3 lastLocalPosition;
	// Update is called once per frame
	// When LockX or LockY Then the frame rate means MULTIPLIER OF LOCKED X or Y Speed (it does not mean displacement anymore if locked) therefore set it to 1f on each frame in animation
	void LateUpdate () {
		if (!Application.isPlaying)
			Start();

		// test
		Vector3 delta = transform.localPosition;
		lastLocalPosition = delta;
		delta.Scale(myroot.localScale);
		Vector3 adjustedForLocksDelta = new Vector3((LockXAxis ? 0: delta.x),(LockYAxis ? 0: delta.y),delta.z);
		Vector3 rootTransformDelta = delta - adjustedForLocksDelta;

		Vector3 newLocalPos = RelParentToOrigLocalPos + adjustedForLocksDelta;
		RelativeParentTo.localPosition = newLocalPos;
		if (rootTransformDelta != Vector3.zero && Application.isPlaying)
		{
			rootTransformDelta.Scale(new Vector3(LockedXSpeed, LockedYSpeed, 1f));

			//myroot.localPosition += new Vector3(-2f,0,0) * Time.deltaTime;
			//myroot.Translate(new Vector3(-2f,0,0) * Time.deltaTime, Space.Self);
			myroot.Translate(rootTransformDelta * Time.deltaTime, Space.Self);

        }

		//test end
		/*
		if (lastLocalPosition != transform.localPosition)
		{
			Vector3 delta = transform.localPosition - lastLocalPosition;
			lastLocalPosition = transform.localPosition;
			delta = new Vector3(LockXAxis ? 0 : delta.x, LockYAxis ? 0 : delta.y, delta.z);
			Vector3 transformed = RelativeParentTo.TransformDirection(delta);
			RelativeParentTo.localPosition += transformed;
		}
		*/

		Vector3 locRot = transform.localEulerAngles;
		
		if (lastLocalRotation != locRot)
		{
			lastLocalRotation = locRot;

			foreach (Transform t in parentOrUncle)
			{
				t.localEulerAngles = locRot;
			}
		}
	}
}
