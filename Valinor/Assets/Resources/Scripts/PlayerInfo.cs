using UnityEngine;
using System.Collections;


public class PlayerInfo : MonoBehaviour {

    public static float maxThrust = 100f;
    public static HingeJoint2D[] hingeJoints { get; private set; }
    public static SpringJoint2D[] springJoints { get; private set; }
    public static Rigidbody2D[] rigidbodies { get; private set; }
    public static Rigidbody2D packRigidbody { get; private set; }
    public static Collider2D[] colliders { get; private set; }
    public static Transform[] transforms { get; private set; }

    public static Transform thrusterTransform { get; private set; }
	public static Transform thrusterEndTransform { get; private set; }
	public static Transform[] thighs { get; private set; }
	public static Transform[] legs { get; private set; }
	public static Transform[] shoulders { get; private set; }
	public static Transform[] arms { get; private set; }


	public static Transform playerTransform { get; private set; }

	// Use this for initialization
	void Start () {
		playerTransform = transform;
        transforms = gameObject.GetComponentsInChildren<Transform>();
        hingeJoints = gameObject.GetComponentsInChildren<HingeJoint2D>();
        springJoints = gameObject.GetComponentsInChildren<SpringJoint2D>();
        rigidbodies = gameObject.GetComponentsInChildren<Rigidbody2D>();
        colliders = gameObject.GetComponentsInChildren<Collider2D>();
		thighs = new Transform[2];
		legs = new Transform[2];
		shoulders = new Transform[2];
		arms = new Transform[2];
        foreach (var rigidbody in rigidbodies)
        {
            if (rigidbody.gameObject.name == "Player")
            {
				packRigidbody = rigidbody;
            }
        }
        foreach (var trans in transforms)
        {
			int index = trans.gameObject.name.Contains("Right") ? 0 : 1;
            if (trans.gameObject.name == "Thruster")
            {
                thrusterTransform = trans;
            }
			else if (trans.gameObject.name == "ThrusterEnd")
			{
				thrusterEndTransform = trans;
			}
			else if (trans.gameObject.name.Contains("Thigh"))
			{
				thighs[index] = trans;
			}
			else if (trans.gameObject.name.Contains("Leg"))
			{
				legs[index] = trans;
			}
			else if (trans.gameObject.name.Contains("Shoulder"))
			{
				shoulders[index] = trans;
			}
			else if (trans.gameObject.name.Contains("Arm"))
			{
				arms[index] = trans;
			} 
        }
	}

}
