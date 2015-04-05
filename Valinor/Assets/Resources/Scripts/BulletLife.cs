using UnityEngine;
using System;
using System.Collections;

public class BulletLife : MonoBehaviour {

    public float totalLifeSeconds = 2f;


	// Use this for initialization
	void Start () {
        Destroy(gameObject,totalLifeSeconds);
        //rigidbody2D.AddForce(initialDirection * initialForce, ForceMode2D.Impulse);
	}
	
    void OnCollisionEnter2D(Collision2D coll) {
		Destroy(gameObject);
		//Debug.Log("hit " + coll.gameObject.name);
        
    }

    void Update ()
    {
  
    }
}
