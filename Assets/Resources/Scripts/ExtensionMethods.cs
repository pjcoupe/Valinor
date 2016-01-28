using UnityEngine;
using System.Collections;
using System;

public struct CustomInfo
{
	private const int GROUNDED_INFO_POS = 0;
	private const int NORMAL_INFO_POS = 1;
	public string myName { get; private set; }
	public string hitName { get; private set; }
	public Vector2 avgNormal { get; private set; }
	public Quaternion avgNormalAngle { get; private set; }
	private int addCount;
	public bool isStatic;

	// be sure to add them |= to Add(..)
	public bool isGrounded { get; private set; } // G######### 
	public bool useNormalsFromCollider  { get; private set; } // instead of from rotation angle 

	public CustomInfo(string myName, string hitName, Vector2 avgNormal)
	{
		this.myName = myName;
		this.hitName = hitName;
		this.avgNormal = avgNormal;
		addCount = 0;
		string[] s = myName.Split('_');
		if (s.Length >= 2)
		{
			Generate(s[1]);
		}
	}

	public void Add(CustomInfo other)
	{
		addCount++;
		hitName += "|"+other.hitName;
		avgNormal = other.avgNormal;
		isGrounded |= other.isGrounded; // PJC TO DO ADD OTHER isGrounded like bools when I add them

	}

	public void Generate(string s)
	{
		if (s != null && s.Length >= 10)
		{
			char grounded = s[GROUNDED_INFO_POS];
			isGrounded =  grounded == 'G';
			switch (s[NORMAL_INFO_POS])
			{
			case '0':
			case '1': //15
			case '2'://30
			case '3'://45
			case '4'://60
			case '5'://75
			case '6'://90
			case '7'://105
			case '8'://120
			case '9'://135
				avgNormalAngle = Quaternion.AngleAxis(15f * (s[NORMAL_INFO_POS]-'0'), Vector3.forward);
				avgNormal = avgNormalAngle * Vector2.up;
				break;
			case 'A'://150
			case 'B'://165
			case 'C'://180
			case 'D'://195
			case 'E'://210
			case 'F'://225
			case 'G'://240
			case 'H'://255
			case 'I'://270
			case 'J'://285
			case 'K'://300
			case 'L'://315
			case 'M'://330
			case 'N'://345
				avgNormalAngle = Quaternion.AngleAxis(15f * (s[NORMAL_INFO_POS]-'A' + 10), Vector3.forward);
				avgNormal = avgNormalAngle * Vector2.up;
				break;
			default:
				useNormalsFromCollider = true;
				break;
			}
			isStatic = true;
		}

	}
}

public static class ExtensionMethods
{

	//public static 
	public static CustomInfo GetCustomInfo(this Transform t, string myName, Vector2 normal)
	{
		return new CustomInfo(t.name,myName,normal);
	}

	public static CustomInfo GetCustomInfo(this GameObject t, string myName, Vector2 normal)
	{
		return new CustomInfo(t.name,myName,normal);

	}

	public static float SignedAngle (this Vector3 a, Vector3 b)
	{
		return Vector3.Angle (a, b) * Mathf.Sign (Vector3.Dot (Vector3.back, Vector3.Cross (a, b)));
	}

	public static float SignedAngle (this Vector2 a, Vector2 b)
	{
		return Vector2.Angle (a, b) * Mathf.Sign (Vector3.Dot (Vector3.back, Vector3.Cross (a, b)));
    }


	public static bool passedRandom(float chance)
	{
		return UnityEngine.Random.value < chance;
	}

	public static Vector2 GetMinimumVelocity(this Transform t, Transform target)
	{
		return t.GetMinimumVelocity(target.position);
	}
	public static Vector2 GetMinimumVelocity(this Transform t, Vector2 target)
	{
		Vector2 targetPos = target - (Vector2)t.position;
		float x = Mathf.Abs(targetPos.x);
		float y = Mathf.Abs(targetPos.y);
		float g = LevelManager.levelGravityMagnitude;
		float time = Mathf.Sqrt(2f*y / g);
		float vy = targetPos.y > 0 ? g * time : 0;
		float vx = x / time;
		float vxAlternate = Mathf.Sqrt(x*g);

		Vector2 v = new Vector2(Mathf.Sign(targetPos.x)*(vx < vxAlternate ? vx : vxAlternate), vy);

		return v;
	}

	public static float ClampMinus180To180(this float f)
	{
		while (Mathf.Abs(f) > 180f)
		{
			f = f - 360f * (Mathf.Sign(f));
		}
		return f;
	}

	public static Quaternion GetTrajectory(this Transform t, Transform target, float maxVelocity, out bool outOfRange, bool calculateEvenIfOutOfRange = true)
	{
		return t.GetTrajectory(target.position,maxVelocity,out outOfRange, calculateEvenIfOutOfRange);
	}
	public static Quaternion GetTrajectory(this Transform t, Vector2 target, float maxVelocity, out bool outOfRange, bool calculateEvenIfOutOfRange = true)
	{
		float x = target.x - t.position.x;
		float y = target.y - t.position.y;
		float g = LevelManager.levelGravityMagnitude;
		float v2 = maxVelocity * maxVelocity;
		float underRoot = Mathf.Sqrt(v2*v2 - g * (g*x*x + 2f*y*v2));

		float chosenAngle = (target.y > t.position.y) ? Mathf.Atan2(v2 + underRoot, g*x) : Mathf.Atan2(v2 - underRoot, g*x);
		if (float.IsNaN(chosenAngle))
		{
			outOfRange = true;
			if (calculateEvenIfOutOfRange)
			{
				v2 = 1000000f;
				underRoot = Mathf.Sqrt(v2*v2 - g * (g*x*x + 2f*y*v2));
				float big = Mathf.Atan2(v2 + underRoot, g*x);
				float small = Mathf.Atan2(v2 - underRoot, g*x);
				if (big > Mathf.PI/2f && small < -Mathf.PI/2f)
				{
					small += (Mathf.PI * 2f);
				}
				chosenAngle = (big + small) / 2f;
			}
			else
			{
				return Quaternion.identity;
			}
		}
		else
		{
			outOfRange = false;
		}
		chosenAngle *= Mathf.Rad2Deg;
		Quaternion rot = Quaternion.AngleAxis(chosenAngle, Vector3.forward);
		return rot;
	}
}
