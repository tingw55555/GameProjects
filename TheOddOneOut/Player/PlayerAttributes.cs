using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(PlayerInputController))]
public class PlayerAttributes : MonoBehaviour
{
    private PlayerInputController controller;
    public Animator anim;

    public float walkAccel =0.2f;

    public float minSpeed = 0.001f;
    public float maxSpeed = 0.018f;

    public float deccelerationRate = 0.03f;

    public float rotateSpeed = 20;

    public List<GameObject> disguises = new List<GameObject>();
    




    //public AudioSource audiplayer;
    //public AudioClip[] sfx;


    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
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


    public void PickADisguise()
    {
        int r = Random.Range(0, disguises.Count);

        for (int i = 0; i < disguises.Count; i++)
        {
            if (i != r)
            {
                disguises[i].SetActive(false);
            
            }else
            {
                disguises[i].SetActive(true);
            }
        }
    }

    public void PlayerAttackFeedBack()
    {
        //audiplayer.clip = sfx[0];
        //audiplayer.Play();
    }

    public void PlayerJumpFeedback()
    {
       
        //audiplayer.clip = sfx[1];
        //audiplayer.Play();
    }

}
