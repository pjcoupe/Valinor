using UnityEngine;
using System.Collections;
using System;

public class RandomOrientation : MonoBehaviour {

	public bool randomRotation = true;
	public bool randomYFlip = true;
	public bool randomXFlip = true;
	public float randomXRangeScaleMin = 1f;
	public float randomXRangeScaleMax = 1f;
	public float randomYRangeScaleMin = 1f;
	public float randomYRangeScaleMax = 1f;
	public bool hasCollider = true;
	public float hasRigidbodyMass = 0f;
	public float friction = 1f;
	public int gridX = 1;
	public int gridY = 1;
	public string spriteSheet;
	public string spriteName;
	public int spriteNameUnderScoresVariation = 0;
	public string tagName = "Background";
	public string layerName = "Objects";
	public string sortingLayer = "Background";
	public int sortingOrder = 0;

	private SpriteRenderer r;
	// Use this for initialization
	void Start () {
		if (!string.IsNullOrEmpty(tagName))
		{
			gameObject.tag = tagName;
		}
		if (!string.IsNullOrEmpty(layerName))
		{
			gameObject.layer = LayerMask.NameToLayer(layerName);
		}
		UnityEngine.Random.seed = (int)(DateTime.Now.Ticks);
		if (randomRotation)
		{
			transform.eulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(0,4) * 90f);
		}
		float xScale = 1f, yScale = 1f, zScale = 1f;
		if (randomXFlip)
		{
			xScale = (UnityEngine.Random.Range(0, 2) * 2) - 1;
		}
		xScale = xScale * (UnityEngine.Random.Range(randomXRangeScaleMin,randomXRangeScaleMax));
		if (randomYFlip)
		{
			yScale = (UnityEngine.Random.Range(0, 2) * 2) - 1;
		}
		yScale = yScale * (UnityEngine.Random.Range(randomYRangeScaleMin,randomYRangeScaleMax));
		transform.localScale = new Vector3(xScale, yScale, zScale);
		if (GetComponent<Rigidbody2D>() == null && hasRigidbodyMass > 0)
		{
			gameObject.AddComponent<Rigidbody2D>();
			gameObject.GetComponent<Rigidbody2D>().mass = hasRigidbodyMass;
		}

		r= GetComponent<SpriteRenderer>();
		if (r == null)
		{
			r= gameObject.AddComponent<SpriteRenderer>();
		}
		if (!string.IsNullOrEmpty(sortingLayer))
		{
			r.sortingLayerName = sortingLayer;
		}
		r.sortingOrder = sortingOrder;

		if (!string.IsNullOrEmpty(spriteSheet) && !string.IsNullOrEmpty(spriteName) && spriteNameUnderScoresVariation > 0)
		{
			BulletLife.LoadSprites(spriteSheet);
			r.sprite = BulletLife.GetSprite(spriteName, UnityEngine.Random.Range(0, spriteNameUnderScoresVariation));
		}

		Collider2D coll = gameObject.GetComponent<Collider2D>();
		if (coll == null && hasCollider)
		{
			coll = gameObject.AddComponent<BoxCollider2D>();
		}
		
		coll.sharedMaterial = new PhysicsMaterial2D(gameObject.name+"Material");
		coll.sharedMaterial.friction = friction;

		float spriteWidth = Mathf.Abs(r.sprite.bounds.extents.x * 2 * xScale);
		float spriteHeight = Mathf.Abs(r.sprite.bounds.extents.y * 2 * yScale);
		if (gridX > 1 || gridY > 1)
		{
			int gX = gridX;
			int gY = gridY;
			gridX = 0;
			gridY = 0;

			GameObject neutral = new GameObject();
			string baseName = transform.name;
			neutral.name = baseName + "_@"+Mathf.RoundToInt(transform.position.x)+","+Mathf.RoundToInt(transform.position.y);
			for (int x = 0; x < gX; x++)
			{
				for (int y = 0; y < gY; y++)
				{
					string atBit = "_@"+x+","+y;
					if (x != 0 || y != 0)
					{
						RandomOrientation ro = Instantiate(this, new Vector3( transform.position.x + x * spriteWidth,
						                                              transform.position.y + y * spriteHeight,
						                                              transform.position.z), transform.rotation) as RandomOrientation;

						ro.transform.parent = neutral.transform;
						ro.name = baseName+atBit;
					}
					else
					{
						transform.parent = neutral.transform;
						transform.name = baseName + atBit;
                    }
				}
			}
		}

	}

}
