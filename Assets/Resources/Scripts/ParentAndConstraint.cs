using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ParentAndConstraint : MonoBehaviour {

	public bool LockXAxis = false;
	public bool LockYAxis = false;
	public Transform RelativeParentTo;
	private bool inited = false;

	// Use this for initialization
	void Start () 
	{
		if (!inited)
		{
			inited = true;
			lastLocalPosition = transform.localPosition;
		}
	}

	private Vector3 lastLocalPosition;
	// Update is called once per frame
	void LateUpdate () {
		if (!Application.isPlaying)
			Start();

		if (lastLocalPosition == transform.localPosition)
		{
			return;
		}
		Vector3 delta = transform.localPosition - lastLocalPosition;
		lastLocalPosition = transform.localPosition;
		delta = new Vector3(LockXAxis ? 0 : delta.x, LockYAxis ? 0 : delta.y, delta.z);


		Vector3 transformed = RelativeParentTo.TransformDirection(delta);
		RelativeParentTo.localPosition += transformed;
		/*
		float deltaX = (ConstrainXAxis ? 0 : transform.localPosition.x) - lastLocalPosition.x;
		float deltaY = (ConstrainYAxis ? 0 : transform.localPosition.y) - lastLocalPosition.y;

		Vector3 delta = new Vector3(deltaX, deltaY, 0);
		lastLocalPosition += delta;

		if (RelativeParentTo != null)
		{
			Debug.Log("Changing "+RelativeParentTo.name + " to "+delta);
			RelativeParentTo.localPosition += delta;
		}
		*/
	}
}
