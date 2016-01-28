using UnityEngine;
using System.Collections;

public class FireBullet : MonoBehaviour {

	const float unrealisticMultiplier = 100f;
	 DamageType damageType;
	 int numberOfBullets = 2;
	 float bulletMass = 0.01f;
	 float bulletSpeed = 15f;
	 float spreadDegrees = 10f;
	 float bulletLife = 2f;
	 float regulatedMaxForce = 3000;

	private float bulletFireDelay = 0.1f;
	private float interBulletDelay = 0.1f;

	private int maxNumberOfBullets = 20;
	private float maxBulletMass = 1f;
	private float maxBulletSpeed = 50f;
	private float maxBulletLife = 5f;
	private int maxSpreadDegrees = 60;

	private float _speedMultiplier = 1f;
	private float endSpeedMultiplier = 0;
	public void SetSpeedMultiplier(float speedMultiplier, float durationSecs)
	{
		if (speedMultiplier != _speedMultiplier)
		{
			_speedMultiplier = speedMultiplier;

		}
		endSpeedMultiplier = _speedMultiplier == 1f ? 0 : Time.time + durationSecs;
	}
	private float _numberOfBulletsMultiplier = 1f;
	private float endNumberOfBulletsMultiplier = 0;
	public void SetNumberOfBulletsMultiplier(float numberOfBulletsMultiplier, float durationSecs)
	{
		if (numberOfBulletsMultiplier != _numberOfBulletsMultiplier)
		{
			_numberOfBulletsMultiplier = numberOfBulletsMultiplier;
			
		}
		endNumberOfBulletsMultiplier = _numberOfBulletsMultiplier == 1f ? 0 : Time.time + durationSecs;
	}
	private float _spreadMultiplier = 1f;
	private float endSpreadMultiplier = 0;
	public void SetSpreadMultiplier(float spreadMultiplier, float durationSecs)
	{
		if (spreadMultiplier != _spreadMultiplier)
		{
			_spreadMultiplier = spreadMultiplier;		
		}
		endSpreadMultiplier = _spreadMultiplier == 1f ? 0 : Time.time + durationSecs;
	}


	private float _bulletLifeMultiplier = 1f;
	private float endBulletLifeMultiplier = 0;
	public void SetBulletLifeMultiplier(float bulletLifeMultiplier, float durationSecs)
	{
		if (bulletLifeMultiplier != _bulletLifeMultiplier)
		{
			_bulletLifeMultiplier = bulletLifeMultiplier;		
		}
		endBulletLifeMultiplier = _bulletLifeMultiplier == 1f ? 0 : Time.time + durationSecs;
	}

	private float _bulletMassMultiplier = 1f;
	private float endBulletMassMultiplier = 0;
	public void SetBulletMassMultiplier(float bulletMassMultiplier, float durationSecs)
	{
		if (bulletMassMultiplier != _bulletMassMultiplier)
		{
			_bulletMassMultiplier = bulletMassMultiplier;		
		}
		endBulletMassMultiplier = _bulletMassMultiplier == 1f ? 0 : Time.time + durationSecs;
	}

	public float GetBulletLife()
	{ 
		return Mathf.Min (maxBulletLife, bulletLife * _bulletLifeMultiplier);
	}

	public float GetBulletSpeed()
	{ 
		return Mathf.Min (maxBulletSpeed, bulletSpeed);
	}

	public float GetSpreadDegrees()
	{
		return Mathf.Min (maxSpreadDegrees, spreadDegrees * _spreadMultiplier);
	}
	bool isRandomWithinSpread = true;


    private Vector2 lastPoint;
	private Vector2 lastDirection;    
    private Vector2 lastForce;
    private Quaternion lastAngle; 
	private float currentBulletDelay = 0f;


	internal float maxAnglePerSecond = 720f;
	private Vector3 lastMousePosition;
	internal Vector3 thrusterAngle { get; private set; }
	internal Vector3 targetThrusterAngle { get; private set; }

    private Vector2 midPoint = new Vector2(0.5f, 0.5f); 

    void Awake()
	{
		BulletLife.LoadSprites("Sprites/Player");

	}
    // Use this for initialization
    void Start () {
       
       
    }

	public static Vector3 RotateTo(Vector3 fromAngle, Vector3 toAngle, float maxAnglePerSecond)
	{

		if (fromAngle != toAngle)
		{
			if (toAngle.z - fromAngle.z > 180f)
			{
				toAngle = new Vector3(0, 0, toAngle.z - 360f);
			}
			else if (fromAngle.z - toAngle.z > 180f)
			{
				fromAngle = new Vector3(0, 0, fromAngle.z - 360f);
			}

			fromAngle = Vector3.MoveTowards(fromAngle, toAngle, maxAnglePerSecond);
			if (fromAngle.z < 0)
			{
				fromAngle = new Vector3(0, 0, fromAngle.z + 360f);
			}
		}
		return fromAngle;
	}

	void FixedUpdate()
	{

		if (thrusterAngle.z != targetThrusterAngle.z)
		{
			thrusterAngle = RotateTo(thrusterAngle, targetThrusterAngle, maxAnglePerSecond * Time.fixedDeltaTime);
			/*
			if (targetThrusterAngle.z - thrusterAngle.z > 180f)
			{
				targetThrusterAngle = new Vector3(0, 0, targetThrusterAngle.z - 360f);
			}
			else if (thrusterAngle.z - targetThrusterAngle.z > 180f)
			{
				thrusterAngle = new Vector3(0, 0, thrusterAngle.z - 360f);
			}
			{
			thrusterAngle = Vector3.MoveTowards(thrusterAngle, targetThrusterAngle, maxAnglePerSecond * Time.fixedDeltaTime);
			if (thrusterAngle.z < 0)
				thrusterAngle = new Vector3(0, 0, thrusterAngle.z + 360f);
			}
			*/
			PlayerInfo.thrusterTransform.rotation = Quaternion.Euler(PlayerMovement.facing < 0 ? -thrusterAngle : thrusterAngle);
            
        }
		//Debug.Log("thr "+thrusterAngle +" targ "+targetThrusterAngle);
    }
    // Update is called once per frame
	void Update () 
    {

        bool instantiateNextBullet = false;
		currentBulletDelay += Time.deltaTime;
		if (currentBulletDelay > bulletFireDelay)
		{
			currentBulletDelay = 0;
			instantiateNextBullet = true;
		}
		bool pressing = Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(1);
		//Debug.Log("PJC REMOVE pressing "+pressing);
        if (pressing)
        {
			Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
			Vector3 orig = (mouse - PlayerInfo.thrusterTransform.position);	
			float rot_z = (Mathf.Atan2 (orig.y, orig.x) * Mathf.Rad2Deg) - 90;	
			if (rot_z < 0)
			{
				rot_z += 360f;
			}
			targetThrusterAngle = new Vector3(0, 0, rot_z);			
        }   	

		if (pressing && instantiateNextBullet ) //&& targetThrusterAngle == thrusterAngle)
		{
			StartCoroutine(CreateBullets());
    	}
	}

	IEnumerator CreateBullets()
	{
		float adjusted = thrusterAngle.z - 90f;
		float t = Time.time;
		if (endNumberOfBulletsMultiplier > 0 && t > endNumberOfBulletsMultiplier)
		{
			endNumberOfBulletsMultiplier = 0;
			_numberOfBulletsMultiplier = 1f;
		}
		if (endSpreadMultiplier > 0 && t > endSpreadMultiplier)
		{
			endSpreadMultiplier = 0;
			_spreadMultiplier = 1f;
		}
		if (endBulletLifeMultiplier > 0 && t > endBulletLifeMultiplier)
		{
			endBulletLifeMultiplier = 0;
			_bulletLifeMultiplier = 1f;
		}
		if (endBulletMassMultiplier > 0 && t > endBulletMassMultiplier)
		{
			endBulletMassMultiplier = 0;
			_bulletMassMultiplier = 1f;
		}
		if (endSpeedMultiplier > 0 && t > endSpeedMultiplier)
		{
			endSpeedMultiplier = 0;
			_speedMultiplier = 1f;
		}
		int numBullets = Mathf.RoundToInt(numberOfBullets * _numberOfBulletsMultiplier);
		float spreadDeg = GetSpreadDegrees();
		float bullMass = bulletMass * _bulletMassMultiplier;
		float speed = GetBulletSpeed();
		float life = GetBulletLife();

		float bulletSpreadDegrees = numBullets == 1 ? 0 : spreadDeg / (numBullets - 1);
		float startRotation = numBullets == 1 ? 0 : spreadDeg / -2;
		float endRotation = numBullets == 1 ? 0 : startRotation + (numBullets-1) * bulletSpreadDegrees;
		float bulletDilution = 1f / _numberOfBulletsMultiplier;
		float totalForcePerSecond = unrealisticMultiplier * bulletDilution * ((float)numBullets / interBulletDelay) * (bulletMass * bulletSpeed * bulletSpeed );
		//Debug.Log("PJC REMOVE totalForce "+totalForcePerSecond+" numBul " + numBullets + " interBD " + interBulletDelay + " Energy " + (bulletMass * bulletSpeed * bulletSpeed) +" mass "+bulletMass + " speed "+bulletSpeed);
		float bulletForce = bulletMass * bulletSpeed * bulletSpeed;

		for (int i = 0; i < numBullets; i++)
		{
			Vector3 postInstantiationRotation;
			if (isRandomWithinSpread)
			{
				float rand = UnityEngine.Random.Range(startRotation, endRotation);
				postInstantiationRotation = new Vector3(0, 0, rand);
			}
			else
			{
				postInstantiationRotation = new Vector3(0, 0, startRotation + i * bulletSpreadDegrees);
			}
			//Instantiate(bulletPrefab, transform.position, adjusted) as BulletLife;
			BulletLife bullet = BulletLife.CreateBullet(
				"PlayerBullet",
				"PlayerBullets",
				transform.position,
				adjusted,
				transform.localScale,
				"Player_Bullet_1",
				bullMass,
				speed,
				life,
				postInstantiationRotation,
				2,
				0.01f,
				null,
				false,
				0.1f,
				"Character",
				0,
				1f,
				0f
				);

			PlayerInfo.packRigidbody.AddForce(unrealisticMultiplier * bulletDilution * bullet.transform.right * bulletForce, ForceMode2D.Impulse);
			yield return new WaitForSeconds(interBulletDelay);
		}

	}
}
