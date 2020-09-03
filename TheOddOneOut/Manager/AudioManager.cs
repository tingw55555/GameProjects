using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioClip[] clips;
    public AudioSource audioS;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPositiveFeedback()
    {
        audioS.clip = clips[0];
        audioS.Play();
    }

    public void OnCatcherWakeUp()
    {
        audioS.clip = clips[1];
        audioS.Play();
    }

    public void OnObjectiveDisappear()
    {
        audioS.clip = clips[2];
        audioS.Play();
    }
}
