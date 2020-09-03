using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissZone : Zones
{
    //public AudioSource hiss;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        BeatManager.instance.DestroyCurrentNote();

        BeatManager.instance.ApplyPenality();


    }
}
