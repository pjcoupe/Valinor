using UnityEngine;
using System.Collections;

public class FireBullet : MonoBehaviour {

    public GameObject bulletPrefab;
	int numberOfBullets = 2;
	float bulletMass = 0.4f;
	float bulletFireDelay = 0.1f;
	float interBulletDelay = 0.1f;
	float bulletSpeed = 30f;
	int maxSpreadDegrees = 30;
	bool isRandomWithinSpread = true;



    private Vector2 lastPoint;
	private Vector2 lastDirection;    
    private Vector2 lastForce;
    private Quaternion lastAngle; 
	private float currentBulletDelay = 0f;

	internal float maxAnglePerSecond = 1f;
	private Vector3 lastMousePosition;
	internal Vector3 thrusterAngle { get; private set; }
	internal Vector3 targetThrusterAngle { get; private set; }

    private Vector2 midPoint = new Vector2(0.5f, 0.5f); 

    
    
    // Use this for initialization
    void Start () {
       
       
    }

	void FixedUpdate()
	{
		if (thrusterAngle != targetThrusterAngle)
		{
			thrusterAngle = Vector3.MoveTowards(thrusterAngle, targetThrusterAngle, maxAnglePerSecond);
			PlayerInfo.thrusterTransform.rotation = Quaternion.Euler(PlayerMovement.facing < 0 ? -thrusterAngle : thrusterAngle);
            
        }
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
        bool pressing = Input.GetMouseButton(0);

        if (pressing)
        {
			Vector3 mousePos = Input.mousePosition;
			if (lastMousePosition != mousePos)
			{
				lastMousePosition = mousePos;
				Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition); 
				Vector3 orig = (mouse - PlayerInfo.thrusterTransform.position);	
				float rot_z = (Mathf.Atan2 (orig.y, orig.x) * Mathf.Rad2Deg) - 90;	
				targetThrusterAngle = new Vector3(0, 0, rot_z);
			}
        }   	

		if (pressing && instantiateNextBullet && targetThrusterAngle == thrusterAngle)
		{
			StartCoroutine(CreateBullets(Quaternion.Euler(thrusterAngle)));
    	}
	}

	IEnumerator CreateBullets(Quaternion adjusted)
	{
		float bulletSpreadDegrees = numberOfBullets == 1 ? 0 : maxSpreadDegrees / (numberOfBullets - 1);
		float startRotation = numberOfBullets == 1 ? 0 : maxSpreadDegrees / -2;
		float endRotation = numberOfBullets == 1 ? 0 : startRotation + (numberOfBullets-1)*bulletSpreadDegrees;
		Vector2 totalForce = Vector2.zero;
		for (int i = 0; i < numberOfBullets; i++)
		{
			Quaternion rot = transform.rotation;
			GameObject bullet = Instantiate(bulletPrefab, transform.position, adjusted) as GameObject;
			float bulletForce = bulletMass * bulletSpeed * bulletSpeed;
			if (isRandomWithinSpread)
			{
				float rand = UnityEngine.Random.Range(startRotation, endRotation);
				bullet.transform.Rotate (new Vector3(0, 0, rand));
			}
			else
			{
				bullet.transform.Rotate (new Vector3(0, 0, startRotation + i * bulletSpreadDegrees));
			}
			Vector2 vel = (Vector2)(bullet.transform.up * bulletSpeed);
			bullet.rigidbody2D.velocity =  vel; // +PlayerInfo.packRigidbody.velocity +
			PlayerInfo.packRigidbody.AddForce(bullet.transform.up * -bulletForce, ForceMode2D.Impulse);
			yield return new WaitForSeconds(interBulletDelay);
		}
		//PlayerInfo.packRigidbody.AddForce(transform.up * -bulletForce, ForceMode2D.Impulse);


	}
}
