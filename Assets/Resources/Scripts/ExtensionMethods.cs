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
}
