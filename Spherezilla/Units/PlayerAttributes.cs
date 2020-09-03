using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInputController))]
public class PlayerAttributes : MonoBehaviour
{
    public float walkSpeed;
    public float rotateSpeed;

    public float jumpHeight;
    public float projectileFlySpeed;

    public float shockwaveRadius;
    public float shockwaveForce;
    public float shockWaveUpwardMod;


    public AudioSource audiplayer;
    public AudioClip[] sfx;

    public Transform shootPoint;
    public GameObject projectile;

    public ParticleSystem shockwavePS;
    


    public void PlayerAttackFeedBack()
    {
        audiplayer.clip = sfx[0];
        audiplayer.Play();
    }

    public void PlayerJumpFeedback()
    {
        shockwavePS.Play();

        audiplayer.clip = sfx[1];
        audiplayer.Play();
    }

}
