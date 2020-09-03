using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCollider : MonoBehaviour
{
    //This should be called body part collider
    //Attach this to a specifical body part (child object of the Boss parent) and it will work

    //The main boss class shouldn't have ontrigger enter 
    //or else if the children are triggered, the parent would be trigger as well.
    public int handSmackDmg;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Jet>() != null)
        {
            //Debug.Log(collision.gameObject.name);
            collision.GetComponent<Jet>().TakeDamage(handSmackDmg);
        }

        
    }
}
