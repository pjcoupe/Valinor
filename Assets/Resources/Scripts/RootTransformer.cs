using UnityEngine;
using System.Collections;

public class RootTransformer : MonoBehaviour {

	private Transform root;
	private Vector3 lastPosition;
	private Vector3 lastRotation;

	public float x;
	public float y;
	public Vector3 position;
	public Vector3 relativeRotation;
	private bool loopIsAtStart;

	public void LoopIsAtStart()
	{
		loopIsAtStart = true;
	}

	// Use this for initialization
	void Start () {
		root = transform.root;
	}
	
	// Update is called once per frame
	void LateUpdate () {
		//root.localPosition += new Vector3(-0.77f,0,0) * Time.fixedDeltaTime;
		Debug.Log("PJC REMOVE x "+ x);

	}
}
