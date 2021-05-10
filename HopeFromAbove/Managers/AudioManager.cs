using System.Collections;
using System.Collections.Generic;
using UnityEngine;


	 

public class AudioManager : Singleton<AudioManager>
{
	public List<AudioClip> clips = new List<AudioClip>();
	private AudioSource aS;

	private void Awake()
	{
		aS = GetComponent<AudioSource>();
	}

	public void PlayMusic()
	{
		if (GameManager.instance.currentState == GameState.Start)
		{
			aS.clip = clips[0];
			aS.Play();
		}
		else if (GameManager.instance.currentState == GameState.Gameplay || GameManager.instance.currentState == GameState.Tutorial)
		{
			aS.clip = clips[1];
			aS.Play();
		}
	}



}
