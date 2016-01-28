using UnityEngine;

public static class GameObjectUtils
{
	/*
	myGameObject.VfxWalk(o => o.hideFlags = HideFlags.HideAndDontSave);
 
	or another example with more robust syntax:

 	myGameObject.VfxWalk((o) => { Debug.Log(o.name); });
	 */
	public static void VfxWalk(this GameObject o, System.Action<GameObject> f)
	{
		f(o);
		
		int numChildren = o.transform.childCount;
		
		for (int i = 0; i < numChildren; ++i)
		{
			o.transform.GetChild(i).gameObject.VfxWalk(f);
		}
	}
	public static void VfxWalk(this Transform o, System.Action<Transform> f)
	{
		f(o);
		
		int numChildren = o.childCount;
		
		for (int i = 0; i < numChildren; ++i)
		{
			o.GetChild(i).VfxWalk(f);
		}
	}

	public static void VfxParentWalk(this Transform o, string stopAtNameOrNull, System.Action<Transform> f)
	{
		f(o);
		Transform p = o.parent;
		if (p && p.name != stopAtNameOrNull)
		{
			p.VfxParentWalk(stopAtNameOrNull, f);
		}
	}


}