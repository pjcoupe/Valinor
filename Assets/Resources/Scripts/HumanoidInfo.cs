using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public static class StaticDictionary
{
	private static Dictionary<string, object> dict;
	static StaticDictionary()
	{
		dict = new Dictionary<string,object>();
	}
	public static void Add(string name, object o)
	{
		dict[name] = o;
	}
	public static object Get(string name)
	{
		object o = null;
		dict.TryGetValue(name, out o);
		return o;
	}
}

public enum AttachToParentType
{
	Bottom_Centre_WithMinPivotOffset,
	Bottom_Centre_WithMaxPivotOffset,
	Top_Centre_WithMinPivotOffset,
	Top_Centre_WithMaxPivotOffset,
	Bottom_Centre,
	Top_Centre,
	Middle_Centre,
	Pivot,
	Middle_Right,
	Middle_Left,
	Bottom_Right,
	Bottom_Left,
	Top_Left,
	Top_Right,
	Custom
}

public enum BodyParts
{
	Bottom,
	Middle,
	Top,
	NeckBottom,
	NeckMiddle,
	NeckTop,
	Head,
	Jaw,
	RightShoulder,
	LeftShoulder,
	RightArm,
	LeftArm,
	RightThigh,
	LeftThigh,
	RightLeg,
	LeftLeg,
	RightHand,
	LeftHand,
	RightFoot,
	LeftFoot

}

public class BodyTemplateSpriteMap
{
	public Transform bodyTemplate;
	private SpriteRenderer _spriteRenderer;


	public Vector3 AttachToParent(BodyTemplateSpriteMap parent)
	{
		Vector3 localPos = CalcLocalPositionAttach(parent);
		//Debug.Log("Attaching "+bodyTemplate.name+" to "+parent.bodyTemplate.name+" localPos "+localPos);
		bodyTemplate.localPosition = localPos;
		return localPos;
	}

	public SpriteRenderer spriteRenderer
	{
		get
		{
			return _spriteRenderer;
		}
		set
		{
			if (value != _spriteRenderer)
			{
				_spriteRenderer = value;
			}
		}
	}
	private Vector3 CalcLocalPositionAttach(BodyTemplateSpriteMap parent)
	{
		if (parent != null && parent.spriteRenderer != null)
		{
			Vector3 parentSize = parent.size;
			Vector3 parentPivot = new Vector3(parentSize.x * parent.spriteRenderer.sprite.pivot.x / parent.spriteRenderer.sprite.rect.width,
			                                  parentSize.y * (parent.spriteRenderer.sprite.pivot.y / parent.spriteRenderer.sprite.rect.height),
			                    0);

			string attachName = _attachToParentType.ToString();
			bool attachUsingPivotOffset = attachName.Contains("PivotOffset");
			bool isMinPivotOffset = attachUsingPivotOffset && attachName.Contains("Min");

			float horiz = 0; // pivot Pt
			float vert = 0; // pivot Pt
			if (attachName.Contains("Left"))
			{
				horiz = 0 - parentPivot.x;
			}
			else if (attachName.Contains("Right"))
			{
				horiz = parentSize.x - parentPivot.x;
			}
			else if (attachName.Contains("Centre"))
			{
				horiz = (parentSize.x / 2f) - parentPivot.x;
			}
			bool top = false;
			if (attachName.Contains("Bottom"))
			{
				vert = 0 - parentPivot.y;
			}
			else if (attachName.Contains("Top"))
			{
				vert = parentSize.y - parentPivot.y;
				top = true;
			}
			else if (attachName.Contains("Middle"))
			{
				vert = (parentSize.y / 2f) - parentPivot.y;
			}

			if (attachUsingPivotOffset)
			{
				float pixely = _spriteRenderer.sprite.pivot.y / _spriteRenderer.sprite.rect.height;
				if (pixely < 0.5f)
				{
					if (!isMinPivotOffset)
					{
						pixely = 1f - pixely;
					}
				}
				else
				{
					if (isMinPivotOffset)
					{
						pixely = 1f - pixely;
					}
				}
				pivot = new Vector3(_size.x * _spriteRenderer.sprite.pivot.x / _spriteRenderer.sprite.rect.width,
				                    _size.y * pixely,
				                    0);

				if (top)
				{
					return new Vector3(horiz, vert - pivot.y, 0);
				}
				else
				{
					return new Vector3(horiz, vert + pivot.y, 0);
				}
			}

			return new Vector3(horiz, vert, 0);
		}
		return Vector3.zero;
	}
	public Vector3 size
	{
		get
		{
			return _size;
		}
		set
		{
			if (_size != value)
			{
				_size = value;
			}
		}
	}
	private Vector3 _size;
	private Vector3 pivot;

	public AttachToParentType attachToParentType
	{
		get
		{
			return _attachToParentType;
		}
		set
		{
			if (_attachToParentType != value)
			{
				_attachToParentType = value;
			}
		}
	}
	private AttachToParentType _attachToParentType;


	public BodyTemplateSpriteMap(Transform bodyTemplate, SpriteRenderer spriteRenderer, Vector3 size, AttachToParentType attachToParentType)
	{
		this.size = Vector3.zero;
		this.bodyTemplate = bodyTemplate;
		this.spriteRenderer = spriteRenderer;
		this.attachToParentType = attachToParentType;
		this.size = size;
	}
}

public class HumanoidInfo : MonoBehaviour {

	public float MyGravityScale { get { return rigidbody2D.gravityScale; } set { rigidbody2D.gravityScale = value; } }
	public bool stickyFeet { get; private set; }
	public int numberOfFeet { get; private set; }
	public int numberOfHands { get; private set; }
	private static bool initedStaticDict = false;
	public AnimatorRootMover rootMover;
	private bool rebuildNeeded = true;
	public float LeftLegSize { get; private set; }
	public float LeftArmSize { get; private set; }
	private ConstantForce2D constantForce2D;
	private Rigidbody2D rigidbody2D;

	public void Awake()
	{
		if (!initedStaticDict)
		{
			initedStaticDict = true;
			PhysicsMaterial2D[] all = Resources.LoadAll<PhysicsMaterial2D>("Physics Materials");
			foreach (var mat in all)
			{
				StaticDictionary.Add(mat.name, mat);
			}
		}
		rigidbody2D = gameObject.GetComponent<Rigidbody2D>();
		constantForce2D = gameObject.GetComponent<ConstantForce2D>();
		if (constantForce2D != null)
		{
			constantForce2D.force = Physics2D.gravity * rigidbody2D.mass;
		}
		numberOfFeet = numberOfHands = 2;
		stickyFeet = false;
	}

	public void ClearMyGravity()
	{
		constantForce2D.enabled = false;
	}

	private Quaternion _myGravityRotation;
	public Quaternion myGravityRotation
	{
		get
		{
			return _myGravityRotation;
		}
		set
		{
			constantForce2D.enabled = true;
			if (_myGravityRotation != value)
			{
				_myGravityRotation = value;
				Vector3 interim = (rigidbody2D.gravityScale * Physics2D.gravity * rigidbody2D.mass);
				constantForce2D.force = (-1f * interim) + (_myGravityRotation * interim);
			}
		}
	}

	public void FootHitObject(string name, CustomInfo hitInfo, float footAngle)
	{
		rootMover.AddFootCustomInfo(name, hitInfo, footAngle);
	}

	public void HandHitObject(string name, CustomInfo hitInfo, float handAngle)
	{
		rootMover.AddHandCustomInfo(name, hitInfo, handAngle);
	}

	public float scale 
	{
		get
		{
			return transform.localScale.y;
		}
		set
		{
			if (transform.localScale.y != value)
			{
				transform.localScale = new Vector3(facingLeft?value: -value, value, value);
			}
		}
	}

	public float Direction { get { return Mathf.Sign(transform.localScale.x); } }

	public bool ClimbOrClamberIsDown { get; set; }

	public bool facingLeft
	{
		get
		{
			return transform.localScale.x > 0;
		}
		set
		{
			if (value != (transform.localScale.x > 0))
			{

				transform.localScale = new Vector3(-scale,scale,scale);
			}
		}
	}
	

	public void Start()
	{

		dict = new Dictionary<string, BodyTemplateSpriteMap>((int)BodyParts.LeftFoot+2);
		foreach (BodyParts bodyPart in Enum.GetValues(typeof(BodyParts)))
		{
			string name = bodyPart.ToString();
			Vector3 size = Vector3.zero;
			AttachToParentType attach;
			switch (bodyPart)
			{
			case BodyParts.LeftArm:
			case BodyParts.RightArm:
				size = new Vector3(0.15f, 0.45f, 1f);
				attach = AttachToParentType.Bottom_Centre_WithMinPivotOffset;
				break;
			case BodyParts.RightLeg:
			case BodyParts.LeftLeg:
				size = new Vector3(0.2f, 0.50f, 1f);
				attach = AttachToParentType.Bottom_Centre_WithMinPivotOffset;
				break;
			case BodyParts.RightShoulder:
			case BodyParts.LeftShoulder:
				size = new Vector3(0.2f, 0.55f, 1f);
				attach = AttachToParentType.Top_Centre_WithMinPivotOffset;
				break;
			case BodyParts.RightThigh:
			case BodyParts.LeftThigh:
				size = new Vector3(0.25f, 0.55f, 1f);
				attach = AttachToParentType.Bottom_Centre_WithMinPivotOffset;
				break;
			case BodyParts.RightHand:
			case BodyParts.LeftHand:
				size = new Vector3(0.2f, 0.2f, 1f);
				attach = AttachToParentType.Bottom_Centre_WithMinPivotOffset;
				break;
			case BodyParts.RightFoot:
			case BodyParts.LeftFoot:
				size = new Vector3(0.33f, 0.2f, 1f);
				attach = AttachToParentType.Bottom_Centre_WithMinPivotOffset;
				break;
			case BodyParts.Bottom:
				size = new Vector3(0.4f, 0.4f, 1f);
				attach = AttachToParentType.Pivot;
				break;
			case BodyParts.Middle:
				size = new Vector3(0.4f, 0.4f, 1f);
				attach = AttachToParentType.Top_Centre;
				break;
			case BodyParts.Top:
				size = new Vector3(0.4f, 0.4f, 1f);
				attach = AttachToParentType.Top_Centre_WithMinPivotOffset;
				break;
			case BodyParts.NeckBottom:
				size = new Vector3(0.15f, 0.15f, 1f);
				attach = AttachToParentType.Top_Centre;
				break;
			case BodyParts.NeckMiddle:
				size = new Vector3(0.15f, 0.15f, 1f);
				attach = AttachToParentType.Top_Centre_WithMinPivotOffset;
				break;
			case BodyParts.NeckTop:
				size = new Vector3(0.15f, 0.15f, 1f);
				attach = AttachToParentType.Top_Centre_WithMinPivotOffset;
				break;
			case BodyParts.Head:
				size = new Vector3(0.3f, 0.3f, 1f);
				attach = AttachToParentType.Top_Centre;
				break;
			case BodyParts.Jaw:
				size = new Vector3(0.15f, 0.15f, 1f);
				attach = AttachToParentType.Pivot;
				break;
			default:
				attach = AttachToParentType.Custom;
				break;
			}

			dict.Add(bodyPart.ToString(), new BodyTemplateSpriteMap(null, null, size, attach));
		}
		PositionOrRotateOther[] bodyTemplate = GetComponentsInChildren<PositionOrRotateOther>();
		BodyTemplateSpriteMap map;
		foreach (var p in bodyTemplate)
		{
			if (dict.TryGetValue(p.name, out map))
			{
				map.bodyTemplate = p.transform;
			}
		}
		
		SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
		foreach (var spriteRenderer in spriteRenderers)
		{			
			if (dict.TryGetValue(spriteRenderer.name, out map))
			{
				map.spriteRenderer = spriteRenderer;
			}		
		}
		Rebuild();
	}

	private Dictionary<string,BodyTemplateSpriteMap> dict;

	public static Vector3 Vector3Divide(Vector3 v1, Vector3 v2)
	{
		return new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
	}
	public static Vector3 Vector3Multiply(Vector3 v1, Vector3 v2)
	{
		return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
	}

	public static Vector2 Vector2Divide(Vector2 v1, Vector2 v2)
	{
		return new Vector2(v1.x / v2.x, v1.y / v2.y);
	}
	public static Vector2 Vector2Multiply(Vector2 v1, Vector2 v2)
	{
		return new Vector2(v1.x * v2.x, v1.y * v2.y);
	}

	private void Rebuild()
	{
		rebuildNeeded = false;
		LeftArmSize = 0;
		LeftLegSize = 0;
		foreach (var pair in dict)
		{
			string name = pair.Key;
			BodyTemplateSpriteMap map = pair.Value;
			Vector3 spriteSize = map.spriteRenderer.sprite.bounds.size;
			map.spriteRenderer.transform.localScale = Vector3Divide(map.size, spriteSize);
			if (name == "Bottom")
			{
				continue;
			}

			BodyTemplateSpriteMap parentMap = null;
			if (dict.TryGetValue(map.bodyTemplate.parent.name, out parentMap))
			{
				Vector3 localPos = map.AttachToParent(parentMap);
				if (name.Contains("Left"))
				{
					if (name.Contains("Leg"))
					{
						LeftLegSize += Mathf.Abs(localPos.y);
					}
					else if (name.Contains("Foot"))
					{
						LeftLegSize += Mathf.Abs(localPos.y) + map.size.y;
					}
					else if (name.Contains("Arm"))
					{
						LeftArmSize += Mathf.Abs(localPos.y);
					}
					else if (name.Contains("Hand"))
					{
						LeftArmSize += Mathf.Abs(localPos.y) + map.size.y;
					}
					//else if (name.Contains("Thigh")) { }

					
				}
			}
		}
	}
	

	// Update is called once per frame
	void Update () {
		if (rebuildNeeded)
		{
			Rebuild();
		}
	}
}
