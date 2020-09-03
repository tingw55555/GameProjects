using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class Jet : BaseUnit
{

    private PlayerControls controls;
    private Vector2 move;
    public GameObject jetFiring;
    private float scoreShowingTime;
    public float ShowDuration;
    public int attackDamage = 20;


    public Transform boss;
    public float flySpeedModifier;
    public GameObject bullet;
    public Transform shootPoint;
    public float normalAttackShotSpeed;

    public AudioClip[] sfx;
    public AudioSource audiPlayer;


    private void Awake()
    {
        controls = new PlayerControls();

        //Lambda Expressions. ctx = context ¯\_(ツ)_/¯
        controls.Gameplay.NormalAttack.performed += ctx => NormalAttack();

        controls.Gameplay.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += ctx => move = Vector2.zero;



    }

    //Remember to enable Unity's input system or else the controls wont work! 
    private void OnEnable()
    {
        controls.Enable();
    }


    private void OnDisable()
    {
        controls.Disable();

    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (GameManager.instance.playerControl)
        {
            Vector2 playerPosition = new Vector2(move.x, move.y) * flySpeedModifier * Time.deltaTime;
            transform.Translate(playerPosition, Space.World);


            if (GameManager.instance.win == false)
            {
                //Give the player object a look target and rotate accordingly 
                transform.LookAt(boss, -Vector3.forward);
                transform.Rotate(90, 0, 0);
            }

        }


    }


    // LateUpdate is called after Update. its useful to order script execution 
    void LateUpdate()
    {

    }

    //This function is mapped to the primary key on Awake 
    //so I'm using it as a general function....dont judge me!
    void NormalAttack()
    {
        //Lock this off with a bool. So I can control when the player's input does anything 
        if (GameManager.instance.playerControl)
        {
            //if the player has been defeated, and is pressing the primary key
            //this let the game manager know they wanna restart
            if (GameManager.instance.defeated == true)
            {
                GameManager.instance.OnRestart();
            }

            //Provide sound feedback 
            audiPlayer.clip = sfx[0];
            audiPlayer.Play();

            //Provid visual firing feeback. 
            //I don't need to turn this prefab off because it deactivates itself after seconds 
            //With the "destroyed after seconds" & "recylcableObject" scripts //
            jetFiring.SetActive(true);

            //Spawns the bullet
            GameObject bulletPrefab = GameObjectUtil.Instantiate(bullet, shootPoint.position, shootPoint.rotation);
            Projectile bulletShot = bulletPrefab.GetComponent<Projectile>();
            
            //Set its attack damage from the player 
            bulletShot.attackDamage = attackDamage;

            //Bullets damage targets that are not its owner
            bulletShot.owner = this;
            
            //Give it speed or else it wont fly!
            bulletShot.moveSpeed = normalAttackShotSpeed;

        }



    }

    public override void TakeDamage(float damage)
    {
        //Provid Audio feedback 
        audiPlayer.clip = sfx[1];
        audiPlayer.Play();

        //This reduces health + trigger animation 
        base.TakeDamage(damage);

        //let the game manager know when the player dies and it will transition to the restart game state 
        if (currentHealth <= 0)
        {
            GameManager.instance.OnPlayerDeath();
        }

    }




}
