using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    //[HideInInspector]
    public float moveSpeed;
    private bool didCollide;
    public BaseUnit owner;
    public int attackDamage;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    //Because I recycle projectile objects so I need to reset this bool either on enable or on disenable 
    //I just chose to do both, in case of any unexpected behavioir 
    private void OnEnable()
    {
        didCollide = false;
    }


    private void OnDisable()
    {
        didCollide = false;
    }

    public void Move()
    {
        //This uses local position because projectiles are almost always a child object of its object pool parent 
        transform.localPosition += (transform.up * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //This is called when Player hits the tutorial destroyable UI
        if (collision.GetComponent<DestroyableUI>() != null)
        {
            collision.GetComponent<DestroyableUI>().OnUIHit();
            GameObjectUtil.Destroy(gameObject);
        }


        
        if (collision.GetComponent<BaseUnit>() != null && collision.GetComponent<BaseUnit>() != owner)
        {
            //This bool check makes sure this cannot be trigger twice 
            if (didCollide)
            {
                return;
            }

            didCollide = true;

            collision.GetComponent<BaseUnit>().TakeDamage(attackDamage);
            GameObjectUtil.Destroy(gameObject);


        }

        
        //if (collision.GetComponent<BossCollider>() != null && collision.GetComponent<BaseUnit>() != owner)
        //{
        //    //This bool check makes sure this cannot be trigger twice 
        //    if (didCollide)
        //    {
        //        return;
        //    }

        //    didCollide = true;

        //    collision.GetComponent<BaseUnit>().TakeDamage(attackDamage);
        //    GameObjectUtil.Destroy(gameObject);
        //}



    }


}
