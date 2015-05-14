using UnityEngine;
using System.Collections;

public class IsGrounded : MonoBehaviour {

	private Collider2D[] colliders;

	// Use this for initialization
	void Start () {
		string name = gameObject.name;
		colliders = this.GetComponents<Collider2D>();
	}
	
	public bool isGrounded { 
		get
		{
			if (touching != null)
			{			
				return touching.layer == LayerMask.NameToLayer("Objects");
			}
			return false;
		}
	}


	public GameObject touching { get; private set; }

	public GameObject triggering { get; private set; }

	void OnCollisionEnter2D(Collision2D coll)
	{
		touching = coll.gameObject;
	}

	void OnCollisionStay2D(Collision2D coll)
	{
		touching = coll.gameObject;
	}

	void OnCollisionExit2D(Collision2D coll)
	{
		touching = null;
	}

	void OnTriggerEnter2D(Collider2D coll)
	{
		triggering = coll.gameObject;
	}
	
	void OnTriggerStay2D(Collider2D coll)
	{
		triggering = coll.gameObject;
	}
	
	void OnTriggerExit2D(Collider2D coll)
	{
		triggering = null;
	}
}
