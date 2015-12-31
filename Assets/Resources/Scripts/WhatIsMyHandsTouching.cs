	using UnityEngine;
using System.Collections;

public class WhatIsMyHandsTouching : MonoBehaviour {
	
	private HumanoidInfo humanoidInfo;
	private Transform root;
	private int groundedCollisionCount = 0;
	private Vector2 lastNormal;
	
	
	void Awake()
	{
		humanoidInfo = transform.root.GetComponent<HumanoidInfo>();
	}
	
	void OnCollisionEnter2D(Collision2D other)
	{
		Collide (other.transform, other.contacts[0].normal, false);
	}
	
	void OnCollisionStay2D(Collision2D other)
	{
		Collide (other.transform, other.contacts[0].normal, true);
	}
	
	void OnTriggerEnter2D(Collider2D other)
	{
		Collide(other.transform, lastNormal, false);	
	}
	
	void OnTriggerStay2D(Collider2D other)
	{
		Collide(other.transform, lastNormal, true);		
	}
	
	void OnTriggerExit2D(Collider2D other)
	{
		CollideExit(other.transform);
	}
	
	private void CollideExit(Transform other)
	{
		CustomInfo c = other.GetCustomInfo(transform.name, Vector2.zero);
		if (c.isGrounded && groundedCollisionCount > 0)
		{
			groundedCollisionCount--;
			if (groundedCollisionCount <= 0)
			{
				groundedCollisionCount = 0;
			}
		}
	}
	
	void OnCollisionExit2D(Collision2D other)
	{
		CollideExit(other.transform);
	}
	
	private void Collide(Transform other, Vector2 normal, bool isStay)
	{
		lastNormal = normal;
		CustomInfo customInfo = other.GetCustomInfo(transform.name, normal);
		humanoidInfo.HandHitObject(customInfo);
		if (customInfo.isGrounded)
		{
			if (!isStay)
			{
				groundedCollisionCount++;
			}
		}
	}

}
