using UnityEngine;
using System.Collections;

public class FireArrow : MonoBehaviour {

	static AudioClip fireArrow;
	AudioSource Audio;
	internal float arrowSpeed = 25f;
	internal PoseController pose;
	private Animator animator;
	// Use this for initialization
	void Awake () {
		animator = GetComponent<Animator>();
		if (fireArrow == null)
		{
			fireArrow = Resources.Load<AudioClip>("Audio/BowDrawAndShoot");
			BulletLife.LoadSprites("Sprites/Goblin");
		}
		Transform[] children = GetComponentsInChildren<Transform>();
		childArrow = transform;
		foreach (Transform child in children)
		{
			Transform parent = child.parent;
			if (parent != null && parent.name.Contains("LeftHand"))
			{
				childArrow = child;
			}
		}
		Audio = GetComponent<AudioSource>();
		pose = gameObject.GetComponent<PoseController>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private bool playingBowStretch;
	private bool firing;
	private Transform childArrow = null;


	public void PlayBowStretch(int oclock)
	{
	
		if (!playingBowStretch && fireArrow != null && fireArrow.loadState == AudioDataLoadState.Loaded && !Audio.isPlaying)
		{

			if (!firing)
			{
				firing = true;
				/*
				float aim = 6f * 5f * ((oclock / 100) - 9);

				float sign = Mathf.Sign(transform.lossyScale.x);
				float requiredAngle = pose.RequiredAngleToHitTarget(arrowSpeed);
				float targetRelativeXSign = Mathf.Sign(transform.position.x - pose.target.position.x) * sign;
				//float targetAngle = Vector2.Angle(Vector2.up, pose.target.position - transform.position) * -targetRelativeXSign;
				float poseAngle = sign * ((transform.rotation.eulerAngles.z + aim) % 360);

				if (targetRelativeXSign < 0)
				{
					pose.ChangeDirection();
				}

				while (Mathf.Abs(poseAngle) > 180f)
				{
					poseAngle += -360 * Mathf.Sign(poseAngle);
				}

				float finalAngle = poseAngle;
				//bool abortShort = pose.AbortShot(poseAngle, requiredAngle, out finalAngle);
				if (sign < 0)
				{
					finalAngle = 180f - finalAngle;
				}
				else
				{
					finalAngle = -finalAngle;
				}
				*/
				playingBowStretch = true;
				Audio.PlayOneShot(fireArrow);
				StartCoroutine(Fire(0.2f, pose.target, null, 1f, arrowSpeed, 20f, 3f, true));

			}
		}
		else
		{
			//Debug.Log("NOT Play Bow Stretch");
		}

	}


	public IEnumerator Fire(float delayBeforeFiring, Transform target, DamageType damageType, float mass, float speed, float lifeSeconds, float destroyAfterImpactSeconds, bool stickOnImpact)
	{

		yield return new WaitForSeconds(delayBeforeFiring);
		Vector3 position = childArrow.position;
		float finalAngle = pose.lastRequiredAngle;
		Vector3 scale = childArrow.lossyScale;
		pose.firedArrow = true;

		float sign = Mathf.Sign(transform.lossyScale.x);
		if (sign < 0)
		{
			finalAngle =  finalAngle - 180f;
			scale = new Vector3(-scale.x,scale.y,scale.z);
		}

		float rotation = finalAngle;

		Transform[] children = GetComponentsInChildren<Transform>();
		foreach (Transform child in children)
		{
			Transform parent = child.parent;
			if (parent != null && parent.name.Contains("LeftHand"))
			{
				BulletLife bl = BulletLife.CreateBullet("NonPlayerBullet",
				                                        "NonPlayerBullets",
				                                        position,
				                                        rotation, 
				                                        scale,
				                                        "Goblin_arrow", 
				                                        mass, 
				                                        speed, 
				                                        lifeSeconds, 
				                                        Vector3.zero,
				                                        1,
				                                        0,
				                                        null, 
				                                        true,
				                                        5f,
				                                        "Character",
				                                        0,
				                                        1f,
				                                        0
				                                        );
				break;
			}
		}


		playingBowStretch = false;
		firing = false;
		yield return null;
	}
}
