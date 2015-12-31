using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class PositionOrRotateOther : MonoBehaviour {

	public bool rotateOnly = true;
	public Transform otherTransform;

	private Vector3 startPosition;
	// Use this for initialization
	void Start () {
		if (otherTransform)
		{
			otherTransform.position = transform.position;
			otherTransform.rotation = transform.rotation;
			startPosition = otherTransform.position;
		}
	}
	
	// Update is called once per frame
	void LateUpdate () {
		if (otherTransform)
		{
			if (!Application.isPlaying)
				Start();
			if (!rotateOnly)
			{
				otherTransform.position = transform.position;
			}
			else
			{
				otherTransform.position = startPosition;
			}
			otherTransform.rotation = transform.rotation;
		}
	}
}
