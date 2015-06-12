using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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
	internal bool isEnemyBullets;
	internal bool impacted;
	internal bool stickOnImpact;
	internal Transform target;
	internal float homingTurnRateDegreesPerSecond;
	internal float destroyAfterImpactSeconds;
    internal float totalLifeSeconds;
	internal float mass;
	internal Vector2 initialVelocity;
	internal DamageType damageType;
	internal bool isPrefabDontDestroy;
	internal int bulletAnimationFrames = 1;
	internal string bulletSpriteName = "Player_Bullet_1";
	internal float animationChangeRateSecs = 0.25f;
	internal float spinDegreesPerSecond;
	internal float gravityScale = 1f;
	internal string sortingLayer = "Character";
	internal int orderInLayer = 1;


	internal SpriteRenderer spriteRenderer { get; private set; }
	private static Dictionary<string, Sprite> allSprites;
	private static List<string> loadedSpritePaths;

	public static Sprite GetSprite(string name, int frame = 0)
	{
		Sprite ret = null;
		if (frame > 0)
		{
			name += new string('_', frame);
		}
		if (allSprites != null )
		{
			allSprites.TryGetValue(name, out ret);
		}
		return ret;
	}

	public static void LoadSprites(string assetPath)
	{
		if (allSprites == null)
		{
			allSprites = new Dictionary<string, Sprite>();
			loadedSpritePaths = new List<string>();
		}
		if (!loadedSpritePaths.Contains(assetPath))
		{
			loadedSpritePaths.Add(assetPath);
			Sprite[] sprites = Resources.LoadAll<Sprite>(assetPath);
			foreach (var sprite in sprites)
			{
				allSprites[sprite.name] = sprite;
			}
		}
	}

	void Awake()
	{
		isPrefabDontDestroy = false;
	}

	// Use this for initialization
	void Start () 
	{
		if (bulletAnimationFrames > 1)
		{
			animating = true;
			StartCoroutine(Animate());
		}
		if (!isPrefabDontDestroy && totalLifeSeconds > 0)
		{
        	Destroy(this, totalLifeSeconds);
		}
		if (target != null && homingTurnRateDegreesPerSecond > 0)
		{
			StartCoroutine(Homing ());
		}
		UpdateDirection(initialVelocity);
	}
	private int currentFrame = 0;
	private bool animating;


	internal void UpdateDirection(Vector3 direction)
	{

		Vector3 euler = Quaternion.LookRotation(direction).eulerAngles;
		if (direction.x > 0)
		{
			transform.eulerAngles = new Vector3(0,0,180f - euler.x);  
		}
		else
		{
			transform.eulerAngles = new Vector3(0,0, euler.x);
		}

	}

	int updateDirCounter = 0;
	Vector2 lastVel;
	void FixedUpdate()
	{
		if (rigidbody2D != null)
		{
			Vector2 vel = rigidbody2D.velocity;
			bool updateAngle = Mathf.Sign(vel.x) != Mathf.Sign(lastVel.x) || Mathf.Sign(vel.y) != Mathf.Sign(lastVel.y) || updateDirCounter == 0;
			updateDirCounter = (updateDirCounter + 1) % 5;
			//Debug.Log ("Update bullet direction "+updateAngle);
			if (updateAngle && vel != Vector2.zero)
			{
				UpdateDirection(vel);			
			}
			lastVel = vel;
		}
	}

	IEnumerator Homing()
	{

		yield return new WaitForSeconds(0.1f);

	}

	IEnumerator Animate()
	{
		while (animating)
		{
			yield return new WaitForSeconds(animationChangeRateSecs);
			currentFrame = (currentFrame + 1) % bulletAnimationFrames;
			spriteRenderer.sprite = GetSprite(bulletSpriteName, currentFrame);
		}
		yield return null;
		
	}
	
    void OnCollisionEnter2D(Collision2D coll) 
	{
		//Debug.Log ("Bullet "+name+" tag "+tag+" hit "+coll.gameObject.name+" tag "+ coll.gameObject.tag);
		if (!impacted)
		{
			impacted = true;

			if (destroyAfterImpactSeconds > 0)
			{
				Destroy(this, destroyAfterImpactSeconds);
			}
			if (stickOnImpact && coll.transform.name != "Pack")
			{
				float size = gameObject.collider2D.bounds.size.x * -0.5f;
				transform.position += transform.right  * size;

				Transform p = coll.transform;
				while (p != null && p.tag == "DeleteMe")
				{
					p = p.parent;
				}

				Destroy(gameObject.rigidbody2D);
				Destroy(gameObject.collider2D);

				GameObject empty = new GameObject();
				empty.name = "DeleteMe";
				empty.tag = "DeleteMe";
				transform.parent = empty.transform;
				empty.transform.parent = p;
				spriteRenderer.sortingLayerName = "Background";
				spriteRenderer.sortingOrder = 10;

			}
		}
		//Debug.Log("hit " + coll.gameObject.name);        
    }

	void OnDestroy()
	{
		//Debug.Log("Destroying bullet "+name);
		Transform parent = transform.parent;
		if (parent != null && parent.tag == "DeleteMe")
		{
			Destroy(parent.gameObject);
		}
		Destroy(gameObject);
	}

	private static BulletLife bulletPrefab;

	internal static BulletLife CreateBasicBullet(string tag, 
	                                             string layer, 
	                                             Vector3 pos,
	                                             float rot,
	                                             Vector3 locScale,
			                                       float mass,
			                                       float speed,
			                                       string spriteName,	                                       
			                                       float lifeSecs,
			                                       Vector3 postInstantiateRotation)
	{
		if (bulletPrefab == null)
		{
			bulletPrefab = Instantiate(Resources.Load("Prefabs/PlayerBullet", typeof(BulletLife))) as BulletLife;
			bulletPrefab.isPrefabDontDestroy = true;
			bulletPrefab.gameObject.SetActive(false);
			bulletPrefab.name = "BulletLife Prefab";
		}
		BulletLife bullet = Instantiate(bulletPrefab) as BulletLife;
		bullet.transform.position = pos;
		bullet.transform.eulerAngles = new Vector3(0, 0, rot);
		bullet.transform.localScale  = locScale;
		bullet.name = tag;
		bullet.tag = tag;
		bullet.gameObject.layer = LayerMask.NameToLayer(layer);
		bullet.isPrefabDontDestroy = false;
		bullet.gameObject.SetActive(true);
		bullet.spriteRenderer = bullet.gameObject.GetComponent<SpriteRenderer>();
		bullet.totalLifeSeconds = lifeSecs;
		bullet.mass = mass;
		bullet.bulletSpriteName = spriteName;
		if (postInstantiateRotation != Vector3.zero)
		{
			bullet.transform.Rotate (postInstantiateRotation);
		}
		bullet.initialVelocity = (Vector2)(bullet.transform.right * -speed);
		bullet.spriteRenderer.sprite = GetSprite(spriteName);
		bullet.rigidbody2D.mass = mass;
		bullet.rigidbody2D.gravityScale = 1f;
		bullet.gameObject.AddComponent<BoxCollider2D>();
		bullet.spriteRenderer.sortingLayerName = "Character";
		bullet.spriteRenderer.sortingOrder = 0;
		bullet.destroyAfterImpactSeconds = 0.1f;
		bullet.rigidbody2D.velocity = bullet.initialVelocity;

		return bullet;
	}

	internal static BulletLife CreateBullet(string tag, 
	                                        string layer,
	                                         Vector3 pos,
		                                     float rot,
	                                         Vector3 locScale,
		                                     string spriteName,
		                                     float mass,
		                                     float speed,
	                                         float lifeSecs,
		                                     Vector3 postInstantiateRotation,		                                     
	                                         int frames = 1,
	                                         float frameChangeRateSecs = 0,
	                                         DamageType damageType = null,
	                                         bool stickOnImpact = false,
	                                         float destroyAfterImpactSeconds = 0.1f,
	                                         string sortingLayer = "Character",
	                                         int sortingOrder = 0,
	                                         float gravityScale = 1f,
	                                         float spinDegreesPerSecond = 0
	                                         )
	{
		BulletLife bullet = CreateBasicBullet(tag, layer, pos, rot, locScale, mass, speed, spriteName, lifeSecs, postInstantiateRotation);
		bullet.stickOnImpact = stickOnImpact;
		bullet.damageType = damageType;
		bullet.bulletAnimationFrames = frames;
		bullet.animationChangeRateSecs = frameChangeRateSecs;
		bullet.destroyAfterImpactSeconds = destroyAfterImpactSeconds;
		bullet.spriteRenderer.sortingLayerName = sortingLayer;
		bullet.spriteRenderer.sortingOrder = sortingOrder;
		bullet.rigidbody2D.gravityScale = gravityScale;
		if (spinDegreesPerSecond != 0)
		{
			bullet.rigidbody2D.angularVelocity = spinDegreesPerSecond;
			bullet.rigidbody2D.angularDrag = 0f;
		}
		return bullet;
	}

}
