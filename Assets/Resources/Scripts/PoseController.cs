using UnityEngine;
using System.Collections;
using System.Collections.Generic;

enum BodyPart
{
	None,
	Helmet,
	Eye,
	Nose,
	Ear,
	Mouth,
	Head,
	Neck,
	Body,
	Shoulder,
	Arm,
	Hand,
	Thigh,
	Leg,
	Foot,
	DetectGround,
	Max
}
enum BodyPosition
{
	None,
	Left,
	Right,
	Upper,
	Middle,
	Lower,
	Max
}
enum Uprightness
{
	Standing,
	Crouching,
	Lying
}
enum Motion
{
	None,
	Walking,
	Running
}

enum Fighting
{
	None,
	Shielding,
	Fighting,
	FiringBow,
	Shooting, // not done
	Throwing
}


enum WeaponType
{
	None,
	OneHanded_HandToHand,
	OneHanded_Shield,
	TwoHanded_SmallThrowingWeapon,
	OneHanded_LargeThrowingWeapon,
	TwoHanded_Bow,
	OneHanded_StrikingWeapon,
	TwoHanded_StrikingWeapon,
	OneHanded_ShootingWeapon,
	TwoHanded_ShootingWeapon
}

class InitialAngles
{
	public string name;
	public BodyPart bodyPart;
	public BodyPosition bodyPosition;
	public Transform transform;
}
public class PoseController : MonoBehaviour {
	Animator animator;
	InitialAngles[] initialAngles;
	InitialAngles Head;
	InitialAngles MiddleBody;
	InitialAngles LeftFoot;
	InitialAngles RightFoot;
	InitialAngles LeftHand;
	InitialAngles RightHand;
	InitialAngles DetectGround;


	private int layerMask = 0;
	Motion motion = Motion.None;
	Fighting fighting = Fighting.None;
	Uprightness uprightness = Uprightness.Standing;


	// Use this for initialization
	void Start () {
		layerMask = LayerMask.GetMask( new string[]{ "Player", "Objects" } );
		animator = GetComponent<Animator>();
		Transform[] bodyParts = this.GetComponentsInChildren<Transform>();
		initialAngles = new InitialAngles[bodyParts.Length];
		for (int i=0; i < bodyParts.Length; i++)
		{
			initialAngles[i] = new InitialAngles();
			initialAngles[i].transform = bodyParts[i].transform;
			initialAngles[i].name = bodyParts[i].name;
			initialAngles[i].bodyPart = BodyPart.None;
			initialAngles[i].bodyPosition = BodyPosition.None;
			for (BodyPart j=BodyPart.None + 1; j < BodyPart.Max; j++)
			{
				if (initialAngles[i].name.Contains(j.ToString()))
				{
					initialAngles[i].bodyPart = j;
					break;
				}
			}
			for (BodyPosition j=BodyPosition.None + 1; j < BodyPosition.Max; j++)
			{
				if (initialAngles[i].name.Contains(j.ToString()))
				{
					initialAngles[i].bodyPosition = j;
					break;
				}
			}
			BodyPart bodyPart = initialAngles[i].bodyPart;
			BodyPosition bodyPosition = initialAngles[i].bodyPosition;
			switch (bodyPart)
			{
			case BodyPart.Hand:
				if (bodyPosition == BodyPosition.Left)
				{
					LeftHand = initialAngles[i];
				}
				else
				{
					RightHand = initialAngles[i];
				}
				break;
			case BodyPart.Foot:
				if (bodyPosition == BodyPosition.Left)
				{
					LeftFoot = initialAngles[i];
				}
				else
				{
					RightFoot = initialAngles[i];
				}
				break;
			case BodyPart.Head:
				Head = initialAngles[i];
				break;
			case BodyPart.Body:
				if (bodyPosition == BodyPosition.Middle)
				{
					MiddleBody = initialAngles[i];				
				}
				break;
			case BodyPart.DetectGround:
				DetectGround = initialAngles[i];
				break;
			}
		}
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(10, 10, 50, 20),"Run"))
		{
			animator.SetTrigger("Run");
		}
		else if (GUI.Button(new Rect(10, 30, 50, 20),"Walk"))
		{
			animator.SetTrigger("Walk");
		}
		else if (GUI.Button(new Rect(10, 50, 50, 20),"Stop"))
		{
			animator.SetTrigger("Stop");
		}
		else if (GUI.Button(new Rect(10, 70, 50, 20),"Shoot"))
		{


			animator.SetTrigger("Bow1030");
		}
	}

	private bool calcPlayerTargeting = false;
	public float playerTargetDelay = 1f;
	public float maxTargetingDistance = 40f;
	// Update is called once per frame
	void Update () {
		if (!calcPlayerTargeting)
		{
			calcPlayerTargeting = true;
			StartCoroutine(CalcPlayerTargeting(playerTargetDelay));
		}


	}

	IEnumerator CalcPlayerTargeting(float delaySecs)
	{
		yield return new WaitForSeconds(delaySecs);
		Vector3 orig = Head.transform.position;
		Vector3 dest = PlayerInfo.playerTransform.position;
		RaycastHit2D hit = Physics2D.Linecast(orig, dest, layerMask);
		Debug.DrawLine(orig,dest);
		if (hit.collider != null) 
		{
			//Debug.Log("Centroid " + hit.centroid + " Dist "+hit.distance + " coll " + hit.collider.name);
		}
		else
		{
			//Debug.Log("nothing layermask " + layerMask);
		}
		calcPlayerTargeting = false;
	}
}
