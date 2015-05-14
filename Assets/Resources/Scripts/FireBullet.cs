using UnityEngine;
using System.Collections;

public class FireBullet : MonoBehaviour {

	private BulletLife bulletPrefab = null;
	const float unrealisticMultiplier = 100f;
	 DamageType damageType;
	 int numberOfBullets = 2;
	 float bulletMass = 0.01f;
	 float bulletSpeed = 15f;
	 float spreadDegrees = 10f;
	 float bulletLife = 0.2f;
	 float regulatedMaxForce = 3000;

	private float bulletFireDelay = 0.1f;
	private float interBulletDelay = 0.1f;

	private int maxNumberOfBullets = 20;
	private float maxBulletMass = 1f;
	private float maxBulletSpeed = 50f;
	private float maxBulletLife = 5f;
	private int maxSpreadDegrees = 60;

	private float @speedMultiplier = 1f;
	private float endSpeedMultiplier = 0;
	public void SetSpeedMultiplier(float speedMultiplier, float durationSecs)
	{
		if (speedMultiplier != @speedMultiplier)
		{
			@speedMultiplier = speedMultiplier;

		}
		endSpeedMultiplier = @speedMultiplier == 1f ? 0 : Time.time + durationSecs;
	}
	private float @numberOfBulletsMultiplier = 1f;
	private float endNumberOfBulletsMultiplier = 0;
	public void SetNumberOfBulletsMultiplier(float numberOfBulletsMultiplier, float durationSecs)
	{
		if (numberOfBulletsMultiplier != @numberOfBulletsMultiplier)
		{
			@numberOfBulletsMultiplier = numberOfBulletsMultiplier;
			
		}
		endNumberOfBulletsMultiplier = @numberOfBulletsMultiplier == 1f ? 0 : Time.time + durationSecs;
	}
	private float @spreadMultiplier = 1f;
	private float endSpreadMultiplier = 0;
	public void SetSpreadMultiplier(float spreadMultiplier, float durationSecs)
	{
		if (spreadMultiplier != @spreadMultiplier)
		{
			@spreadMultiplier = spreadMultiplier;		
		}
		endSpreadMultiplier = @spreadMultiplier == 1f ? 0 : Time.time + durationSecs;
	}


	private float @bulletLifeMultiplier = 1f;
	private float endBulletLifeMultiplier = 0;
	public void SetBulletLifeMultiplier(float bulletLifeMultiplier, float durationSecs)
	{
		if (bulletLifeMultiplier != @bulletLifeMultiplier)
		{
			@bulletLifeMultiplier = bulletLifeMultiplier;		
		}
		endBulletLifeMultiplier = @bulletLifeMultiplier == 1f ? 0 : Time.time + durationSecs;
	}

	private float @bulletMassMultiplier = 1f;
	private float endBulletMassMultiplier = 0;
	public void SetBulletMassMultiplier(float bulletMassMultiplier, float durationSecs)
	{
		if (bulletMassMultiplier != @bulletMassMultiplier)
		{
			@bulletMassMultiplier = bulletMassMultiplier;		
		}
		endBulletMassMultiplier = @bulletMassMultiplier == 1f ? 0 : Time.time + durationSecs;
	}

	public float GetBulletLife()
	{ 
		return Mathf.Min (maxBulletLife, bulletLife * @bulletLifeMultiplier);
	}

	public float GetBulletSpeed()
	{ 
		return Mathf.Min (maxBulletSpeed, bulletSpeed);
	}

	public float GetSpreadDegrees()
	{
		return Mathf.Min (maxSpreadDegrees, spreadDegrees * @spreadMultiplier);
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
		bulletPrefab = Instantiate(Resources.Load("Prefabs/ThrustBullet", typeof(BulletLife))) as BulletLife;
		bulletPrefab.isPrefabDontDestroy = true;
		bulletPrefab.gameObject.SetActive(false);
		bulletPrefab.name = "BulletLife Prefab";

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
		Quaternion adjusted = Quaternion.Euler(thrusterAngle);
		float t = Time.time;
		if (endNumberOfBulletsMultiplier > 0 && t > endNumberOfBulletsMultiplier)
		{
			endNumberOfBulletsMultiplier = 0;
			numberOfBulletsMultiplier = 1f;
		}
		if (endSpreadMultiplier > 0 && t > endSpreadMultiplier)
		{
			endSpreadMultiplier = 0;
			spreadMultiplier = 1f;
		}
		if (endBulletLifeMultiplier > 0 && t > endBulletLifeMultiplier)
		{
			endBulletLifeMultiplier = 0;
			bulletLifeMultiplier = 1f;
		}
		if (endBulletMassMultiplier > 0 && t > endBulletMassMultiplier)
		{
			endBulletMassMultiplier = 0;
			bulletMassMultiplier = 1f;
		}
		if (endSpeedMultiplier > 0 && t > endSpeedMultiplier)
		{
			endSpeedMultiplier = 0;
			speedMultiplier = 1f;
		}
		int numBullets = Mathf.RoundToInt(numberOfBullets * numberOfBulletsMultiplier);
		float spreadDeg = GetSpreadDegrees();
		float bullMass = bulletMass * bulletMassMultiplier;
		float speed = GetBulletSpeed();
		float life = GetBulletLife();

		float bulletSpreadDegrees = numBullets == 1 ? 0 : spreadDeg / (numBullets - 1);
		float startRotation = numBullets == 1 ? 0 : spreadDeg / -2;
		float endRotation = numBullets == 1 ? 0 : startRotation + (numBullets-1) * bulletSpreadDegrees;
		float bulletDilution = 1f / numberOfBulletsMultiplier;
		float totalForcePerSecond = unrealisticMultiplier * bulletDilution * ((float)numBullets / interBulletDelay) * (bulletMass * bulletSpeed * bulletSpeed );
		//Debug.Log("PJC REMOVE totalForce "+totalForcePerSecond+" numBul " + numBullets + " interBD " + interBulletDelay + " Energy " + (bulletMass * bulletSpeed * bulletSpeed) +" mass "+bulletMass + " speed "+bulletSpeed);
		float bulletForce = bulletMass * bulletSpeed * bulletSpeed;

		for (int i = 0; i < numBullets; i++)
		{
			Quaternion rot = transform.rotation;
			BulletLife bullet = Instantiate(bulletPrefab, transform.position, adjusted) as BulletLife;
			bullet.isPrefabDontDestroy = false;
			bullet.gameObject.SetActive(true);
			bullet.damageType = damageType;
			bullet.totalLifeSeconds = life;
			bullet.mass = bullMass;
			if (isRandomWithinSpread)
			{
				float rand = UnityEngine.Random.Range(startRotation, endRotation);
				bullet.transform.Rotate (new Vector3(0, 0, rand));
			}
			else
			{
				bullet.transform.Rotate (new Vector3(0, 0, startRotation + i * bulletSpreadDegrees));
			}
			bullet.velocity = (Vector2)(bullet.transform.up * speed);

			PlayerInfo.packRigidbody.AddForce(unrealisticMultiplier * bulletDilution * bullet.transform.up * -bulletForce, ForceMode2D.Impulse);
			yield return new WaitForSeconds(interBulletDelay);
		}

	}
}
