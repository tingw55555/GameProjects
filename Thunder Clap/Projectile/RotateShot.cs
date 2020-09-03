using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateShot : Projectile
{
    public Transform rotateparent;

    // Start is called before the first frame update
    void Start()
    {
        //moveSpeed = 100;
        
    }

    // Update is called once per frame
    void Update()
    {
        OrbitAround();   
    }

    public void OrbitAround()
    {
        //Rotate parent is a child object of the boss parent object. 
        //Because of that, the boss parent cannot have ontrigger enter in its script. 
        //Or else, if rotate shot prefab hits player, the parent's trigger enter will trigger as well. Which makes the player take double damage
        transform.RotateAround(rotateparent.transform.position, Vector3.forward, moveSpeed * Time.deltaTime);
    }



    

}
