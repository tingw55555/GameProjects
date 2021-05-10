using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SFX
{
    Crush = 0,
    CityFall = 1,
	ACSPrepawn = 2,
	AddToPower = 3,
	Loading = 4,
	Unloading = 5,
	LowResource = 6,
	SurrenderWarning = 7,
	ACSpawned = 8,
}


public class SFXManager : Singleton<SFXManager>
{
	public List<AudioClip> clips = new List<AudioClip>();
	private AudioSource aS;

	[SerializeField]
	private GameObject soundCubePrefab;

	private void Awake()
	{
		aS = GetComponent<AudioSource>();
	}

	public void PlaySFX(SFX newSFX)
	{
		AudioSource soundCube = Instantiate(soundCubePrefab, transform).GetComponent<AudioSource>();
		soundCube.PlayOneShot(clips[(int)newSFX]);

	}



}
