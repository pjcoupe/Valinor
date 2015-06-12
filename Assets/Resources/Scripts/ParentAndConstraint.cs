using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class ParentAndConstraint : MonoBehaviour {

	public bool ConstrainXAxis = false;
	public bool ConstrainYAxis = false;
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
		float deltaX = (ConstrainXAxis ? 0 : transform.localPosition.x) - lastLocalPosition.x;
		float deltaY = (ConstrainYAxis ? 0 : transform.localPosition.y) - lastLocalPosition.y;

		Vector3 delta = new Vector3(deltaX, deltaY, 0);
		lastLocalPosition += delta;
		transform.localPosition = lastLocalPosition;
		if (RelativeParentTo != null)
		{
			Debug.Log("Changing "+RelativeParentTo.name + " to "+delta);
			RelativeParentTo.localPosition += delta;
		}
	}
}
