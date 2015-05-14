using UnityEngine;
using System.Collections;

public class FireArrow : MonoBehaviour {

	AudioClip fireArrow;
	AudioSource audio;

	// Use this for initialization
	void Awake () {
		fireArrow = Resources.Load<AudioClip>("Audio/BowDrawAndShoot");
		audio = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private bool playingBowStretch;
	private bool firing;
	public void PlayBowStretch()
	{
		//Debug.Log ("PLAYBOW");
		if (!playingBowStretch && fireArrow != null && fireArrow.isReadyToPlay && !audio.isPlaying)
		{
			playingBowStretch = true;
			audio.PlayOneShot(fireArrow);
			if (!firing)
			{
				firing = true;
				StartCoroutine(Fire(0.2f, PlayerInfo.playerTransform,new DamageType(), 1f, new Vector2(-10f,10f), 20f, false, true));
			}
		}
		else
		{
			//Debug.Log("NOT Play Bow Stretch");
		}

	}


	public IEnumerator Fire(float delayBeforeFiring, Transform target, DamageType damageType, float mass, Vector2 velocity, float lifeSeconds, bool destroyOnImpact, bool stickOnImpact)
	{
		BulletLife bl = null;
		yield return new WaitForSeconds(delayBeforeFiring);
		GameObject arrow;

		Transform[] children = GetComponentsInChildren<Transform>();
		foreach (Transform child in children)
		{
			Transform parent = child.parent;
			if (parent != null && parent.name.Contains("LeftHand"))
			{

				arrow = Instantiate(child.gameObject,child.position, child.rotation) as GameObject;
				arrow.SetActive(true);
				arrow.transform.localScale = child.lossyScale;
				if (arrow.rigidbody2D == null)
				{
					Rigidbody2D r = arrow.AddComponent<Rigidbody2D>();
				}
				BoxCollider2D c = arrow.GetComponent<BoxCollider2D>();
				if (c == null)
				{
					c = arrow.AddComponent<BoxCollider2D>();
				}

				SpriteRenderer sr = arrow.GetComponent<SpriteRenderer>();
				if (sr != null)
				{
					sr.enabled = true;
				}
				bl = arrow.GetComponent<BulletLife>();
				if (bl == null)
				{
					bl = arrow.AddComponent<BulletLife>();
				}


				break;
			}
		}
		bl.mass = mass;
		bl.destroyOnImpact = destroyOnImpact;
		bl.damageType = damageType;
		bl.totalLifeSeconds = lifeSeconds;
		bl.velocity = velocity;
		bl.stickOnImpact = true;

		playingBowStretch = false;
		firing = false;
		yield return null;
	}
}
