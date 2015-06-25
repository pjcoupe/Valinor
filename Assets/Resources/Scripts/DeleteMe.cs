using UnityEngine;
using System.Collections;

public class DeleteMe : MonoBehaviour {

	// Use this for initialization
	void Start () {

		Debug.Log(AngleTo360(-23.5f));
		Debug.Log(AngleTo360(-423.5f));
		Debug.Log(AngleTo360(23.5f));
		Debug.Log(AngleTo360(423.5f));

		Debug.Log(AngleTo360_(-23.5f));
		Debug.Log(AngleTo360_(-423.5f));
		Debug.Log(AngleTo360_(23.5f));
		Debug.Log(AngleTo360_(423.5f));
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	float AngleTo360(float angle)
	{
		return Mathf.Abs((angle % 360) + 360) % 360;
	}

	float AngleTo360_(float angle)
	{
		return ((angle % 360) + 360) % 360;
	}
}
