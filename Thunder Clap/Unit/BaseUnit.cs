using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BaseUnit : MonoBehaviour
{
    protected Animator anim;
    protected SpriteRenderer sprite;
    public GameObject dmgScore;
    public Color damgedColor;
    public Vector2 dmgScoreOffset;

    [HideInInspector]
    public float currentHealth;

    public float maxHealth;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        currentHealth = maxHealth;
    }

    // Update is called once per frame
    protected virtual void Update()
    {

        

    }


    public virtual void TakeDamage(float damage)
    {
      
        currentHealth -= damage;

        //Spawn the score text prefab 
        DamageScore scoreText = GameObjectUtil.Instantiate(dmgScore, transform.position).GetComponent<DamageScore>();

        //Give each unit their own colour 
        scoreText.GetComponent<TextMeshPro>().color = damgedColor;
        
        //And their own offset 
        scoreText.offset = new Vector3(dmgScoreOffset.x, dmgScoreOffset.y, 1);
        
        scoreText.SetText(damage);

        //Provid Animation feedback
        //Right now, the boss doesn't have a TakeDamage animation
        anim.SetTrigger("OnTakeDamage");


        

    }

}
