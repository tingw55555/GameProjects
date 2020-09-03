using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableCube : MeshExplosion
{



    protected override void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerInputController>() || collision.gameObject.GetComponent<Projectile>())
        {

            Explode();
        }
    }



}
