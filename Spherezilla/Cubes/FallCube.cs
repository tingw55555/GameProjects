using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallCube : MeshExplosion
{




    protected override void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Ground>())
        {

            Explode();
        }
    }

}
