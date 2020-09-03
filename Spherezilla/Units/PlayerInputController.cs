using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerAttributes))]
public class PlayerInputController : MonoBehaviour
{

    public static PlayerInputController instance;
    private PlayerControls inputAction;
    private Vector2 movementInput;
    private Vector3 playerMovementInput;
    private Rigidbody rb;

    

    private PlayerAttributes playerAttributes;


    [HideInInspector]
    public bool onGround;

    //public float defaultColliderSize;
    //private SphereCollider co;







    private void Awake()
    {
        instance = this;
        //co = GetComponent<SphereCollider>();
        //defaultColliderSize = co.radius;

        playerAttributes = GetComponent<PlayerAttributes>();

        rb = GetComponent<Rigidbody>();




        inputAction = new PlayerControls();



        inputAction.playerControls.Movement.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        inputAction.playerControls.Movement.canceled += ctx => movementInput = Vector2.zero;

        inputAction.playerControls.Attack.performed += ctx => PrimaryInput();
        inputAction.playerControls.Jump.performed += ctx => SecondaryInput();

        inputAction.playerControls.Start.performed += ctx => StartGameInput();
        inputAction.playerControls.Option.performed += ctx => OptionInput();

    }



    private void OnEnable()
    {
        inputAction.Enable();
    }

    private void OnDisable()
    {
        inputAction.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {

        onGround = true;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        

        Move();



    }


    public void Move()
    {
        if (GameManager.instance.playerControl == true)
        {
            rb.useGravity = true;

            //Y is the upvector
            playerMovementInput = new Vector3(movementInput.x, 0, movementInput.y) * playerAttributes.walkSpeed * Time.deltaTime;


            if (playerMovementInput != Vector3.zero)
            {
                //Not too sure why I had to add the offset ¯\_(ツ)_/¯
                //add -90 offset to make the player move the same direction its looking at 
                Quaternion targetRotation = Quaternion.LookRotation(playerMovementInput) * Quaternion.Euler(0, -90, 0);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, playerAttributes.rotateSpeed * Time.deltaTime);

            }

            transform.Translate(playerMovementInput, Space.World);
        }


    }

    private void PrimaryInput()
    {
        if (GameManager.instance.currentState == GameManager.GameState.Pause)
        {
            Debug.Log("Restart Game");

            SceneManager.LoadScene("GameScene");
            //Application.Quit();
        }

        if (GameManager.instance.playerControl == true)
        {

            playerAttributes.PlayerAttackFeedBack();

            GameObject prefab = ObjectPool.instance.GetInstance(RecyclableObjectTypes.PlayerProjectile, playerAttributes.shootPoint.position, playerAttributes.shootPoint.rotation);
            prefab.GetComponent<Projectile>().moveSpeed = playerAttributes.projectileFlySpeed;
        }


    }





    private void SecondaryInput()
    {

        if (GameManager.instance.currentState == GameManager.GameState.Pause)
        {
            Debug.Log("leave game");
            Application.Quit();
        }



        if (GameManager.instance.playerControl == true)
        {
            if (onGround)
            {
                rb.velocity = new Vector3(0, playerAttributes.jumpHeight, 0);

                //IncreaseColliderRadius();

                onGround = false;

            }
        }

    }

    private void StartGameInput()
    {
        GameManager.instance.PlayerRollingIn();
        //Debug.Log("Start pressed");
    }

    private void OptionInput()
    {
        GameManager.instance.OnPlayerPause();
        //Debug.Log("Select pressed");
    }



    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.GetComponent<Ground>() || collision.gameObject.GetComponent<MeshExplosion>())
        {
            if (onGround == false)
            {
                //playerAttributes.shockwavePS.Play();

                playerAttributes.PlayerJumpFeedback();

                ExplodeNearByObject();

                onGround = true;

            }

        }

    }






    public void ExplodeNearByObject()
    {
        Ray ray = new Ray(transform.position, Vector3.forward);

        RaycastHit[] hitInfos = Physics.SphereCastAll(ray, playerAttributes.shockwaveRadius);


        foreach (RaycastHit hitInfo in hitInfos)
        {
            Rigidbody hitRB = hitInfo.transform.GetComponent<Rigidbody>();

            if (hitRB != null)
            {
                hitRB.AddExplosionForce(playerAttributes.shockwaveForce, transform.position,
                    playerAttributes.shockwaveRadius, playerAttributes.shockWaveUpwardMod,
                    ForceMode.Force);
            }

        }



    }

}
