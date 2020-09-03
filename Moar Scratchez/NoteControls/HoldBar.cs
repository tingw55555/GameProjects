using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldBar : MonoBehaviour
{
    public Hold holdNote;


    private void OnTriggerEnter2D(Collider2D other)
    {
        HitZone hit = other.GetComponent<HitZone>();

        if (hit != null)
        {
            holdNote.isFinished = true;
        }
    }

}
