using UnityEngine;
using System.Collections;

public class DrawGrid : MonoBehaviour {

	void OnDrawGizmos()
	{
		for (int x=-10;x<10;x++)
		{
			for (int y=-10;y<10;y++)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireCube(new Vector3(x*2,y*2,0), new Vector3(2,2,0));
			}
		}
	}
}
