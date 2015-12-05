using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {

	public static int currentLevel = 1;

	public static string levelName = "Test";

	private static bool recalcGravity = true;


	private static Quaternion _gravityRotationAngle;
	public static Quaternion gravityRotationAngle
	{
		get
		{	
			if (recalcGravity)
			{
				RecalcGravity();
			}
			return _gravityRotationAngle;
		}

	}
	public static Vector2 levelGravity
	{
		get
		{
			return Physics2D.gravity;
		}
		set
		{
			Physics2D.gravity = value;
			RecalcGravity();
		}
	}

	private static void RecalcGravity()
	{
		_gravityRotationAngle = Quaternion.identity;
		_gravityRotationAngle.SetFromToRotation(Vector3.down, Physics2D.gravity);
	
		recalcGravity = false;
	}
	public static float levelGravityMagnitude
	{
		get
		{
			return Physics2D.gravity.magnitude;
		}

	}

}
