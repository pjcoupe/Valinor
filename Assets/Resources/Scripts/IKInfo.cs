using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class IKInfo : MonoBehaviour {

	private Quaternion[] savedRotation;
	private Vector3[] savedPosition;

	public Transform[] bodyPartsToRestore;

	private Animator anim;

	public void Awake()
	{
		anim = GetComponent<Animator>();
		savedRotation = new Quaternion[bodyPartsToRestore.Length];
		savedPosition = new Vector3[bodyPartsToRestore.Length];
	}

	public void RestoreBum()
	{
		for (int i=0; i < bodyPartsToRestore.Length; i++)
		{
			bodyPartsToRestore[i].localPosition = savedPosition[i];
			bodyPartsToRestore[i].localRotation = savedRotation[i];

		}
	}

	public void OnGUI()
	{
		if (GUI.Button(new Rect(10, 10, 50, 20), "Run"))
		{
			anim.Play("Base.IK_Run_Base_1",0,0);
		}
	}

	public void SaveBum()
	{

		for (int i=0; i < bodyPartsToRestore.Length; i++)
		{
			savedPosition[i] = bodyPartsToRestore[i].localPosition;
			savedRotation[i] = bodyPartsToRestore[i].localRotation;
		}
	}
	
}
