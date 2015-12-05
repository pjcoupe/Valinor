using UnityEngine;
using System.Collections;
using System;

public class FastMath
{
	public static float PI = (float)Math.PI;
	public static float SQUARED_PI = PI * PI;
	public static float HALF_PI = 0.5f * PI;
	public static float TWO_PI = 2.0f * PI;
	public static float THREE_PI_HALVES = TWO_PI - HALF_PI;
	
	private static float _sin_a = -4 / SQUARED_PI;
	private static float _sin_b = 4 / PI;
	private static float _sin_p = 9f / 40;
	
	private static float _asin_a = -0.0481295276831013447f;
	private static float _asin_b = -0.343835993947915197f;
	private static float _asin_c = 0.962761848425913169f;
	private static float _asin_d = 1.00138940860107040f;
	
	private static float _atan_a = 0.280872f;
	
	public static float Cos(float x)
	{
		return Sin(x + ((x > HALF_PI) ? -THREE_PI_HALVES : HALF_PI));
	}
	
	public static float Sin(float x)
	{
		x = _sin_a * x * Math.Abs(x) + _sin_b * x;
		return _sin_p * (x * Math.Abs(x) - x) + x;
	}
	
	public static float Tan(float x)
	{
		return Sin(x) / Cos(x);
	}
	
	public static float Asin(float x)
	{
		return x * (Math.Abs(x) * (Math.Abs(x) * _asin_a + _asin_b) + _asin_c) + Math.Sign(x) * (_asin_d - (float)Math.Sqrt(1 - x * x));
	}
	
	public static float Acos(float x)
	{
		return HALF_PI - Asin(x);
	}
	
	public static float atan(float x)
	{
		return (Math.Abs(x) < 1) ? x / (1 + _atan_a * x * x) : Math.Sign(x) * HALF_PI - x / (x * x + _atan_a);
	}
	
	public static float InverseSqrt(float x)
	{
		float xhalves = 0.5f * x;
		x = (float)BitConverter.Int64BitsToDouble(0x5FE6EB50C7B537AAL - (BitConverter.DoubleToInt64Bits((double)x) >> 1));
		return x * (1.5f - xhalves * x * x); // more iterations possible
	}
	
	public static float Sqrt(float x)
	{
		return x * InverseSqrt(x);
	}
}
public class FastMath2
{		
	private const int           SIZE                 = 1024;
	private const float        STRETCH            = Mathf.PI;
	// Output will swing from -STRETCH to STRETCH (default: Math.PI)
	// Useful to change to 1 if you would normally do "atan2(y, x) / Math.PI"
	
	// Inverse of SIZE
	private const int        EZIS            = -SIZE;
	private static readonly float[]    ATAN2_TABLE_PPY    = new float[SIZE + 1];
	private static readonly float[]    ATAN2_TABLE_PPX    = new float[SIZE + 1];
	private static readonly float[]    ATAN2_TABLE_PNY    = new float[SIZE + 1];
	private static readonly float[]    ATAN2_TABLE_PNX    = new float[SIZE + 1];
	private static readonly float[]    ATAN2_TABLE_NPY    = new float[SIZE + 1];
	private static readonly float[]    ATAN2_TABLE_NPX    = new float[SIZE + 1];
	private static readonly float[]    ATAN2_TABLE_NNY    = new float[SIZE + 1];
	private static readonly float[]    ATAN2_TABLE_NNX    = new float[SIZE + 1];
	
	static FastMath2()
	{
		for (int i = 0; i <= SIZE; i++)
		{
			float f = (float)i / SIZE;
			ATAN2_TABLE_PPY[i] = (float)(Mathf.Atan(f) * STRETCH / Mathf.PI);
			ATAN2_TABLE_PPX[i] = STRETCH * 0.5f - ATAN2_TABLE_PPY[i];
			ATAN2_TABLE_PNY[i] = -ATAN2_TABLE_PPY[i];
			ATAN2_TABLE_PNX[i] = ATAN2_TABLE_PPY[i] - STRETCH * 0.5f;
			ATAN2_TABLE_NPY[i] = STRETCH - ATAN2_TABLE_PPY[i];
			ATAN2_TABLE_NPX[i] = ATAN2_TABLE_PPY[i] + STRETCH * 0.5f;
			ATAN2_TABLE_NNY[i] = ATAN2_TABLE_PPY[i] - STRETCH;
			ATAN2_TABLE_NNX[i] = -STRETCH * 0.5f - ATAN2_TABLE_PPY[i];
		}
	}
	
	/**
     * ATAN2
     */
	
	public static float Atan2(float y, float x)
	{
		if (x >= 0)
		{
			if (y >= 0)
			{
				if (x >= y)
					return ATAN2_TABLE_PPY[(int)(SIZE * y / x + 0.5)];
				else
					return ATAN2_TABLE_PPX[(int)(SIZE * x / y + 0.5)];
			}
			else
			{
				if (x >= -y)
					return ATAN2_TABLE_PNY[(int)(EZIS * y / x + 0.5)];
				else
					return ATAN2_TABLE_PNX[(int)(EZIS * x / y + 0.5)];
			}
		}
		else
		{
			if (y >= 0)
			{
				if (-x >= y)
					return ATAN2_TABLE_NPY[(int)(EZIS * y / x + 0.5)];
				else
					return ATAN2_TABLE_NPX[(int)(EZIS * x / y + 0.5)];
			}
			else
			{
				if (x <= y) // (-x >= -y)
					return ATAN2_TABLE_NNY[(int)(SIZE * y / x + 0.5)];
				else
					return ATAN2_TABLE_NNX[(int)(SIZE * x / y + 0.5)];
			}
		}
	}

	public static float Sqrt2(float z)
	{
		if (z == 0) return 0;
		FloatIntUnion u;
		u.tmp = 0;
		u.f = z;
		u.tmp -= 1 << 23; /* Subtract 2^m. */
		u.tmp >>= 1; /* Divide by 2. */
		u.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */
		return u.f;
	}

	public static float Sqrt(float z)
	{
		if (z == 0) return 0;
		FloatIntUnion u;
		u.tmp = 0;
		float xhalf = 0.5f * z;
		u.f = z;
		u.tmp = 0x5f375a86 - (u.tmp >> 1);
		u.f = u.f * (1.5f - xhalf * u.f * u.f);
		return u.f * z;
	}

	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
	private struct FloatIntUnion
	{
		[System.Runtime.InteropServices.FieldOffset(0)]
		public float f;
		
		[System.Runtime.InteropServices.FieldOffset(0)]
		public int tmp;
	}
}
