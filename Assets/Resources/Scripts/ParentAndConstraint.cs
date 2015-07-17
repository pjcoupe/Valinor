using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ParentAndConstraint : MonoBehaviour {

	public bool LockXAxis = false;
	public bool LockYAxis = false;
	public Transform RelativeParentTo;
	private bool inited = false;
	private Vector3 lastLocalRotation = Vector3.zero;
	private Transform[] parentOrUncle;

	void Awake()
	{
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
		int parentOrUncleCount = transform.parent.parent.childCount;
		parentOrUncle = new Transform[parentOrUncleCount];
		for (int i=0; i < parentOrUncleCount; i++)
		{
			parentOrUncle[i] = transform.parent.parent.GetChild(i);
		}
	}

	private Vector3 lastLocalPosition;
	// Update is called once per frame
	void LateUpdate () {
		if (!Application.isPlaying)
			Start();

		if (lastLocalPosition != transform.localPosition)
		{
			Vector3 delta = transform.localPosition - lastLocalPosition;
			lastLocalPosition = transform.localPosition;
			delta = new Vector3(LockXAxis ? 0 : delta.x, LockYAxis ? 0 : delta.y, delta.z);
			Vector3 transformed = RelativeParentTo.TransformDirection(delta);
			RelativeParentTo.localPosition += transformed;
		}


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
