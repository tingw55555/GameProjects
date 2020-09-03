using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadBullet : Projectile
{
    private Vector2 moveDirection;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
    }

    //Move direction is set by the Enemy on instantiate 
    public void SetMoveDirection(Vector2 dir)
    {
      
        moveDirection = dir; 
    }



}
