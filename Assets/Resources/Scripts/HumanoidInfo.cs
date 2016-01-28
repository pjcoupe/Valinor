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


	public void AttachToParent(BodyTemplateSpriteMap parent)
	{
		Vector3 localPos = CalcLocalPositionAttach(parent);
		//Debug.Log("Attaching "+bodyTemplate.name+" to "+parent.bodyTemplate.name+" localPos "+localPos);
		bodyTemplate.localPosition = Vector3.Scale(localPos,parent.size);
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

	private const float BOXCOLLIDER_MIN_THRESHHOLD = 0.06f;
	private const float FALL_THRESHHOLD = 1f;
	public float MyGravityScale { get { return rigidbody2D.gravityScale; } set { rigidbody2D.gravityScale = value; } }
	public bool stickyFeet { get; private set; }
	public int numberOfFeet { get; private set; }
	public int numberOfHands { get; private set; }
	private static bool initedStaticDict = false;
	private AnimatorRootMover rootMover;
	private bool rebuildNeeded = true;
	public Transform jumpTarget;
	public float jumpFactor = 2f;
	public float fallHeightWillingness = 1f;
	public float jumpOutOfRangeLikelihood = 1f;
	public Vector3 fallStart { get; set; }
	public Vector3 jumpStart { get; set; }
	public bool isFalling { get; set; }
	private bool _isGrounded = false;
	public bool isGrounded 
	{
		get
		{
			return _isGrounded;
		}
		set
		{
			if (_isGrounded != value)
			{
				_isGrounded = value;
			}
			if (value)
			{
				lastGroundedPosition = transform.position;

			}
		}
	}
	public Vector3 lastGroundedPosition { get; private set; }
	public float fallThreshhold = FALL_THRESHHOLD;
	public float LeftLegSize { get; private set; }
	public float RightLegSize { get; private set; }
	public float LeftArmSize { get; private set; }
	public float RightArmSize { get; private set; }
	public float BottomHeight { get; private set; }
	public float HeadHeight { get; private set; }

	private ConstantForce2D constantForce2D;
	private Rigidbody2D rigidbody2D;
	private Vector2 forceOfGravityOnMe;
	private float forceOfGravityOnMeMagnitude;
	private List<SpriteRenderer> leftSprites;
	private List<SpriteRenderer> rightSprites;

	public string DetermineAnimatorTrigger()
	{
		return "Stand";
	}

	public void Awake()
	{
		SpriteRenderer[] leftOrRightSprites = GetComponentsInChildren<SpriteRenderer>();
		leftSprites = new List<SpriteRenderer>();
		rightSprites = new List<SpriteRenderer>();
		foreach (var sprite in leftOrRightSprites)
		{
			if (sprite.name.Contains("Left"))
			{
				leftSprites.Add(sprite);
			}
			else if (sprite.name.Contains("Right"))
			{
				rightSprites.Add(sprite);
			}
		}
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
		forceOfGravityOnMe = Physics2D.gravity * rigidbody2D.mass;
		forceOfGravityOnMeMagnitude = forceOfGravityOnMe.magnitude;
		if (constantForce2D != null)
		{
			constantForce2D.enabled = false;
		}
		numberOfFeet = numberOfHands = 2;
		stickyFeet = false;
		Init ();
	}

	public void ClearMyGravity()
	{
		constantForce2D.enabled = false;	
	}

	public void StickyFeet(Vector2 normal)
	{
		constantForce2D.relativeForce = new Vector2(0, -forceOfGravityOnMeMagnitude);
		constantForce2D.enabled = true;
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

	public float Direction { get { return facingLeft ? 1f: -1f; } }

	public bool ClimbOrClamberIsDown { get; set; }


	private bool _facingLeft = true;
	public bool facingLeft
	{
		get
		{
			return _facingLeft;
		}
		set
		{
			if (value != _facingLeft)
			{
				_facingLeft = value;
				foreach (var sprite in leftSprites)
				{
					sprite.sortingOrder += _facingLeft?-100:100;
				}
				foreach (var sprite in rightSprites)
				{
					sprite.sortingOrder += _facingLeft?100:-100;
				}
				transform.localScale = new Vector3(scale * (_facingLeft?1f:-1f),scale,scale);
			}
		}
	}
	
	public static float originalArmLength { get; private set; }
	public static float originalLegLength { get; private set; }
	public static float originalHeight { get; private set; }
	public static float originalBendHeight { get; private set; }
	private static bool doneStaticLengthCalculations = false;

	private Transform _bottom;
	private Transform _leftFoot;
	private Transform _rightFoot;
	private Transform _leftLeg;
	private Transform _rightLeg;
	private Transform _leftArm;
	private Transform _rightArm;
	private Transform _leftThigh;
	private Transform _rightThigh;
	private Transform _rightHand;
	private Transform _leftHand;
	private Transform _rightShoulder;
	private Transform _leftShoulder;
	private Transform _middle;
	private Transform _top;
	private Transform _neckBottom;
	private Transform _neckMiddle;
	private Transform _neckTop;
	private Transform _head;	
	private Transform _jaw;	


	void Init()
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
				if (!doneStaticLengthCalculations)
				{
					switch (p.name)
					{
					case "Jaw":
						_jaw = p.transform;
						break;
					case "Head":
						_head = p.transform;
						break;
					case "Bottom":
						_bottom = p.transform;
						break;
					case "Top":
						_top = p.transform;
						break;
					case "Middle":
						_middle = p.transform;
						break;
					case "NeckBottom":
						_neckBottom = p.transform;
						break;
					case "NeckMiddle":
						_neckMiddle = p.transform;
						break;
					case "NeckTop":
						_neckTop = p.transform;
						break;
					case "RightThigh":
						_rightThigh = p.transform;
						break;					
					case "RightArm":					
						_rightArm = p.transform;
						break;
					case "RightHand":
						_rightHand = p.transform;
						break;
					case "RightShoulder":
						_rightShoulder = p.transform;
						break;
					case "RightLeg":
						_rightLeg = p.transform;
						break;
					case "RightFoot":
						_rightFoot = p.transform;
						break;
					case "LeftThigh":
						_leftThigh = p.transform;
						break;					
					case "LeftArm":
						_leftArm = p.transform;
						break;
					case "LeftHand":
						_leftHand = p.transform;
						break;
					case "LeftShoulder":
						_leftShoulder = p.transform;
						break;
					case "LeftLeg":
						_leftLeg = p.transform;
						break;
					case "LeftFoot":
						_leftFoot = p.transform;
						break;
					}
				}
			}
		}
		if (!doneStaticLengthCalculations)
		{
			originalHeight = _head.position.y - _leftLeg.position.y;
			originalBendHeight = _neckBottom.position.y - _middle.position.y;
			originalArmLength = _leftShoulder.position.y - _leftHand.position.y;
			originalLegLength = _leftThigh.position.y - _leftFoot.position.y;
		}
		doneStaticLengthCalculations = true;

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

	public bool doneRebuild { get; private set; }
	private void Rebuild()
	{
		float bottomMost = 9999f;
		float topMost = -9999f;
		float leftMost = 9999f;
		float rightMost = -9999f;

		rebuildNeeded = false;
		LeftArmSize = 0;
		LeftLegSize = 0;
		RightArmSize = 0;
		RightLegSize = 0;
		BottomHeight = 0;
		HeadHeight = 0;
		Transform bot = null;
		foreach (var pair in dict)
		{
			string name = pair.Key;
			BodyTemplateSpriteMap map = pair.Value;
			Sprite sprite = map.spriteRenderer.sprite;

			Bounds bound = sprite.bounds;

			Transform t = map.spriteRenderer.transform;
			t.localScale = map.size;
			BoxCollider2D coll = t.GetComponent<BoxCollider2D>();

			bool resizeXCollider = bound.size.x*t.lossyScale.x < BOXCOLLIDER_MIN_THRESHHOLD;
			bool resizeYCollider = bound.size.y*t.lossyScale.y < BOXCOLLIDER_MIN_THRESHHOLD;
			bool resize = resizeXCollider || resizeYCollider;
			if (coll != null)
			{
				GameObject.DestroyImmediate(coll);
			}
			coll = t.gameObject.AddComponent<BoxCollider2D>();
			if (resize)
			{
				float xSize = Mathf.Max(BOXCOLLIDER_MIN_THRESHHOLD / t.lossyScale.x, bound.size.x);
				float ySize = Mathf.Max(BOXCOLLIDER_MIN_THRESHHOLD / t.lossyScale.y, bound.size.y);
				coll.size = new Vector3(xSize,ySize,bound.size.z);
			}
			coll.sharedMaterial = (PhysicsMaterial2D)StaticDictionary.Get("Character_Slipping");

			if (!doneRebuild)
			{
				float bottom = t.position.y + bound.min.y;
				if (bottom < bottomMost)
				{
					bottomMost = bottom;

				}
				float top = t.position.y + bound.max.y;
				if (top > topMost)
				{
					topMost = top;

				}
				float left = t.position.x + bound.min.x;
				if (left < leftMost)
				{
					leftMost = left;

				}
				float right = t.position.x + bound.max.x;
				if (right > rightMost)
				{
					rightMost = right;

				}
			}
			if (name == "Bottom")
			{
				bot = map.bodyTemplate;
				continue;
			}

			BodyTemplateSpriteMap parentMap = null;
			if (dict.TryGetValue(map.bodyTemplate.parent.name, out parentMap))
			{
				map.AttachToParent(parentMap);
				if (name.Contains("Left"))
				{
					if (name.Contains("Leg"))
					{
						LeftLegSize -= map.bodyTemplate.localPosition.magnitude;
					}
					else if (name.Contains("Foot"))
					{
						LeftLegSize -= map.bodyTemplate.localPosition.magnitude;
						LeftLegSize += t.localScale.y * bound.extents.y;
					}
					else if (name.Contains("Arm"))
					{
						LeftArmSize -= map.bodyTemplate.localPosition.magnitude;

					}
					else if (name.Contains("Hand"))
					{
						LeftArmSize -= map.bodyTemplate.localPosition.magnitude;
						LeftArmSize += t.localScale.y * bound.extents.y;
					}
				}
				else if (name.Contains("Right"))
				{
					if (name.Contains("Leg"))
					{
						RightLegSize -= map.bodyTemplate.localPosition.magnitude;
					}
					else if (name.Contains("Foot"))
					{
						RightLegSize -= map.bodyTemplate.localPosition.magnitude;
						RightLegSize += t.localScale.y * bound.extents.y;
					}
					else if (name.Contains("Arm"))
					{
						RightArmSize -= map.bodyTemplate.localPosition.magnitude;
					}
					else if (name.Contains("Hand"))
					{
						RightArmSize -= map.bodyTemplate.localPosition.magnitude;
						RightArmSize += t.localScale.y * bound.extents.y;
					}
					else if (name.Contains("Thigh"))
					{
						BottomHeight = map.bodyTemplate.localPosition.magnitude;
					}
				}
				else if (name.Contains("Neck"))
				{
					HeadHeight += map.bodyTemplate.localPosition.magnitude;
				}
				else if (name.Contains("Middle"))
				{
					HeadHeight += map.bodyTemplate.localPosition.magnitude;
				}
				else if (name.Contains("Top"))
				{
					HeadHeight += map.bodyTemplate.localPosition.magnitude;
				}
				else if (name.Contains("Head"))
				{
					HeadHeight += map.bodyTemplate.localPosition.magnitude;
					HeadHeight += t.localScale.y * bound.extents.y;
                }
			}
		}
		RightArmSize = Mathf.Abs(RightArmSize);
		LeftArmSize = Mathf.Abs(LeftArmSize);
		RightLegSize = Mathf.Abs(RightLegSize);
		LeftLegSize = Mathf.Abs(LeftLegSize);
		BottomHeight += RightLegSize;
		if (bot != null)
		{
			bot.localPosition = new Vector3(0, BottomHeight,0);
		}
		HeadHeight += BottomHeight;

		doneRebuild = true;
	}
	

	// Update is called once per frame
	void Update () {
		if (rebuildNeeded)
		{
			Rebuild();
		}
	}

	void OnGUI()
	{
		if (GUI.Button(new Rect(0,0,100,100),"rebuild"))
		{
			rebuildNeeded = true;
		}
		if (GUI.Button(new Rect(110,0,100,100),"dir"))
		{
			facingLeft = ! facingLeft;
		}
	}
}
