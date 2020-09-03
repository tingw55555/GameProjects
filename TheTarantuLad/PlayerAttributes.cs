using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(PlayerInputController))]
public class PlayerAttributes : MonoBehaviour
{
    private PlayerInputController controller;
    
    public float walkSpeed;
    public float jumpHeight;
    public float swingSpeed = -120.0f;
    public float playerDeceleration = 0.5f;

    //public float swingRange = 2.0f;

    public float notSwingableDistance = 1f;


    public AudioSource audiplayer;
    public AudioClip[] sfx;


    private void Awake()
    {
        controller = GetComponent<PlayerInputController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayerAttackFeedBack()
    {
        audiplayer.clip = sfx[0];
        audiplayer.Play();
    }

    public void PlayerJumpFeedback()
    {
       
        audiplayer.clip = sfx[1];
        audiplayer.Play();
    }

}
