using UnityEngine;
using System;
using System.Collections;

public class DamageType
{
	public float explosiveDamage = 0f;
	public float fireDamage = 0f;
	public float iceDamage = 0f;
	public float electricalDamage = 0f;
	public float stunDamage = 0f;
	public float spiritDamage = 0f;
	public float sonicDamage = 0f;
	public float poisonDamage = 0f;
	public float radiationDamage = 0f;
	public float switchLoyaltyDamage = 0f;
	public float healthStealPercentage = 0f;
	public float healReverseDamage = 0f;
	public float repairReverseDamage = 0f;
	public float acidDamage = 0f;

	public float penetrationPower = 1f;
}

public class BulletLife : MonoBehaviour {
	public bool isEnemyBullets;
	public bool impacted;
	public bool stickOnImpact;
	public Transform target;
	public float homingTurnRateDegreesPerSecond;
	public bool destroyOnImpact;
    public float totalLifeSeconds;
	public float mass;
	public Vector2 velocity;
	public DamageType damageType;
	public bool isPrefabDontDestroy;

	void Awake()
	{
		isPrefabDontDestroy = false;

	}

	// Use this for initialization
	void Start () 
	{
		if (!isPrefabDontDestroy && totalLifeSeconds > 0)
		{
        	Destroy(gameObject,totalLifeSeconds);
		}
		if (target != null && homingTurnRateDegreesPerSecond > 0)
		{
			StartCoroutine(Homing ());
		}
		rigidbody2D.velocity = velocity;
        //rigidbody2D.AddForce(initialDirection * initialForce, ForceMode2D.Impulse);
	}

	void FixedUpdate()
	{
		if (!impacted && rigidbody2D.velocity != Vector2.zero)
		{

			rigidbody2D.rotation = Quaternion.LookRotation(rigidbody2D.velocity).eulerAngles.x; 
			//Debug.Log("vel "+rigidbody2D.velocity+" rot " +rigidbody2D.rotation+ " euler "+transform.eulerAngles+" destEuler "+Quaternion.LookRotation(rigidbody2D.velocity).eulerAngles);
		}
	}

	IEnumerator Homing()
	{

		yield return new WaitForSeconds(0.1f);

	}

    void OnCollisionEnter2D(Collision2D coll) 
	{
		if (!impacted)
		{
			impacted = true;

			if (destroyOnImpact)
			{
				Destroy(gameObject);
			}
			else if (stickOnImpact && coll.transform.name != "Pack")
			{
				Debug.Log("HIT "+coll.transform.name);
				float size = gameObject.collider2D.bounds.size.x * -0.25f;
				transform.position += transform.right  * size;
				//transform.Rotate(new Vector3(0,0, UnityEngine.Random.Range(-20f,20f)));
				Vector3 beforeScale = transform.localScale;

				Destroy(gameObject.rigidbody2D);
				Destroy (gameObject.collider2D);
				transform.parent = coll.transform;

			}
		}
		//Debug.Log("hit " + coll.gameObject.name);
        
    }
	
}
