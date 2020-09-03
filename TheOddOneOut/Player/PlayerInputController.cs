using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[RequireComponent(typeof(PlayerAttributes))]
public class PlayerInputController : MonoBehaviour
{
    private PlayerAttributes attr;

    private PlayerInput inputAction;
    private Vector2 movementInput;
    private Vector3 playerMovementInput;
    private Vector3 velocity;
    //private Rigidbody rb;


    public delegate void OnPlayerInputReceived();
    public static OnPlayerInputReceived playerHorizontalInputReceived;
    public static OnPlayerInputReceived playerVerticalInputReceived;


    private Vector3 lastPos;


    private void Awake()
    {
        attr = GetComponent<PlayerAttributes>();

        inputAction = new PlayerInput();

        inputAction.Controls.Movement.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        inputAction.Controls.Movement.canceled += ctx => movementInput = Vector2.zero;

        //inputAction.Controls.PrimaryInput.performed += ctx => PrimaryInput();
        //inputAction.Controls.SecondaryInput.performed += ctx => SecondaryInput();

        inputAction.Controls.Start.performed += ctx => StartGameInput();
        //inputAction.Controls.Select.performed += ctx => OptionInput();

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

    }

    // Update is called once per frame
    void Update()
    {

        if (GameManager.instance.playerControl)
        {

            Move();

            if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow))
            {
                playerHorizontalInputReceived?.Invoke();
            }

            if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow))
            {
                playerVerticalInputReceived?.Invoke();
            }
        }

    }

    private void FixedUpdate()
    {

    }



    public void Move()
    {


        playerMovementInput = new Vector3(movementInput.x, 0, movementInput.y) * attr.walkAccel * Time.deltaTime;

        velocity += playerMovementInput;

        if (velocity.magnitude > attr.maxSpeed)
        {
            velocity = velocity.normalized * attr.maxSpeed;
        }


        if (playerMovementInput == Vector3.zero)
        {
            velocity -= velocity.normalized * Time.deltaTime * attr.deccelerationRate;

            if (velocity.magnitude < attr.minSpeed)
            {
                velocity = Vector3.zero;
            }

        }
        else
        {

            Quaternion targetRotation = Quaternion.LookRotation(velocity) * Quaternion.Euler(0, 0, 0);

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, attr.rotateSpeed * Time.deltaTime);

            transform.Translate(velocity, Space.World);
        }




        attr.anim.SetFloat("Speed", velocity.magnitude / attr.maxSpeed);




    }



    public void PrimaryInput()
    {

    }

    public void SecondaryInput()
    {

    }

    public void StartGameInput()
    {
        if (GameManager.instance.currentState == GameState.Start &&
            UIManager.instance.mainMenu.activeSelf)
        {
            GameManager.instance.OnPlayerStart();

        }

       
    }

    public void OptionInput()
    {

    }


}
