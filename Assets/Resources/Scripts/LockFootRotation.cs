using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class LockFootRotation : MonoBehaviour {

	public float angle = 250f;
	public Transform jointToLock = null;

	public void LateUpdate()
	{
		if (jointToLock != null)
		{
			jointToLock.localEulerAngles = new Vector3(0,0,angle);
		}
	}
}
