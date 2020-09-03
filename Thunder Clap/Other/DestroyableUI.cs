using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableUI : MonoBehaviour
{
    public Animator anim;
    public int maxHP = 10;
    public int currentHp;
    public int dmgPerBullet;
    public AudioSource audioPlayer;
    public AudioClip[] sfx;

    // Start is called before the first frame update
    void Start()
    {
        
        currentHp = maxHP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //This is called whenever player's bullet hits it
    public void OnUIHit()
    {
        //Set Audio and Animation feedback
        audioPlayer.clip = sfx[1];
        audioPlayer.Play(); 
        anim.SetTrigger("OnHit");

        //Reduce health
        currentHp -= dmgPerBullet;

        if (currentHp == 0)
        {
            transform.gameObject.SetActive(false);
            
            //let the game manager know it's been destroyed and control the game state from there
            GameManager.instance.OnUIBoxDestroyed();
        }


    }
}
