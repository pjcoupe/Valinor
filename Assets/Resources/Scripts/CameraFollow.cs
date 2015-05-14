using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {
	private static int direction = -1;
	private const float minDistance = 5f;
	private const float maxDistance = 30f;
	private static float distance = 5f;
	private static Transform playerCameraTransform;
	private static Vector3 position;
	private static Quaternion looking;

	private static AudioSource audio;

	public static void PlayOneShot(AudioClip clip, float volumeScale = 1f, float delay = 0f)
	{
		if (clip != null && clip.isReadyToPlay)
		{
			if (delay == 0)
			{
				audio.PlayOneShot(clip, volumeScale);
			}
			else
			{
				audio.clip = clip;
				audio.volume = volumeScale;
				audio.PlayDelayed(delay);
			}
		}
	}

	void Awake()
	{
		audio = GetComponent<AudioSource>();
	}

	void Start()
	{
		playerCameraTransform = this.transform;
	}

	static internal void View(Transform t)
	{
		playerCameraTransform.position = new Vector3(t.position.x,t.position.y, distance * direction);
	}

	static internal void Save()
	{
		position = playerCameraTransform.position;
		looking = playerCameraTransform.rotation;
	}

	static internal void Restore()
	{
		playerCameraTransform.position=position;
		playerCameraTransform.rotation=looking;
	}

	
}
