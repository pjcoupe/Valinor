using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class RotateUncle : MonoBehaviour {


	private Transform[] parentOrUncle;

	void Awake()
	{
		lastLocalRotation = transform.localEulerAngles;

	}

	// Use this for initialization
	void Start () 
	{

		int parentOrUncleCount = transform.parent.parent.childCount;
		parentOrUncle = new Transform[parentOrUncleCount];
		for (int i=0; i < parentOrUncleCount; i++)
		{
			parentOrUncle[i] = transform.parent.parent.GetChild(i);
		}
	}
	
	private Vector3 lastLocalRotation = Vector3.zero;
	// Update is called once per frame
	void LateUpdate () {
		if (!Application.isPlaying)
			Start();
		Vector3 locRot = transform.localEulerAngles;

		if (lastLocalRotation == locRot)
		{

			return;
		}
		lastLocalRotation = locRot;
		Debug.Log("PJC rot unc");
		foreach (Transform t in parentOrUncle)
		{
			t.localEulerAngles = locRot;
		}

	}
}
