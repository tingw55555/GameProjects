using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(PlayerAttributes))]
public class PlayerInputController : MonoBehaviour
{
    //I put all the tweakable values in attributes.
    private PlayerAttributes attributes;

    public Transform rope;
    public Transform shootPoint;
    public Transform currentSwingPoint;
    public Transform ropeVelocityReader;

    public List<Transform> swingPoints = new List<Transform>();
    

    private PlayerInput inputAction;
    private Rigidbody2D rb;
    private Collider2D co;
    private Collider2D groundDetectionCo;

    private Vector2 movementInput;
    private Vector3 playerMovementInput;

    private float xVelocity = 0;

    private float swingDirection = 1;
    private float moveDirection = 1;
    private float launchFactor = 0.008f;

    private float swingSpeed;
    public float swingLimit = 165;



    [HideInInspector]
    public bool onGround;
    [HideInInspector]
    public bool swingFinished = true;
    private bool isSwinging = false;

    private float minVelo = -90;
    private float maxVelo = -150;

    public bool hasMoved = false;

    [Header("Animation")]
    public SpriteRenderer PlayerRenderer;
    public List<Sprite> Animation_Current;
    public int Animation_Index = 0;
    public float Animation_Speed = 0.1f;

    public List<Sprite> Idle;
    public List<Sprite> Walk_Right;
    public List<Sprite> Walk_Left;
    public List<Sprite> Airtime;
    
    IEnumerator Animate()
    {
        while (true)
        {
            yield return new WaitForSeconds(Animation_Speed);

            if (Animation_Current != null)
            {

                Animation_Index++;
                if (Animation_Index >= Animation_Current.Count)
                    Animation_Index = 0;

                PlayerRenderer.sprite = Animation_Current[Animation_Index];
            }
        }
    }

    public void SwitchAnimation(List<Sprite> pNewAnim)
    {
        Animation_Current = pNewAnim;
        Animation_Index = 0;
        PlayerRenderer.sprite = Animation_Current[Animation_Index];
    }

    private void Awake()
    {
        attributes = GetComponent<PlayerAttributes>();

        inputAction = new PlayerInput();

        inputAction.Controls.Movement.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        inputAction.Controls.Movement.canceled += ctx => movementInput = Vector2.zero;

        inputAction.Controls.PrimaryInput.performed += ctx => PrimaryInput();

        rb = GetComponent<Rigidbody2D>();
        co = GetComponent<Collider2D>();
        groundDetectionCo = GetComponentInChildren<Collider2D>();

        //inputAction.Controls.SecondaryInput.performed += ctx => SecondaryInput();
        inputAction.Controls.Start.performed += ctx => StartInput();
        //inputAction.Controls.Select.performed += ctx => OptionInput();

        StartCoroutine(Animate());
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
        //Debug.Log(ropeVelocityReader.velocity);

        Move();

        Swing();

        //if (!swingFinished)
        //{
        //    RaycastHit2D groundRay = Physics2D.Raycast(feet.position, Vector2.down, 0.1f);
        //    if (groundRay.collider != null)
        //    {
        //        if (groundRay.collider.GetComponent<Road>() || groundRay.collider.GetComponent<Platform>())
        //        {
        //            swingFinished = true;
        //        }
        //    }

        //}

        //Debug.DrawRay(feet.position, Vector3.down * 0.1f, Color.red);





    }

    private void Swing()
    {
        if (isSwinging)
        {
            transform.RotateAround(currentSwingPoint.position, Vector3.forward, (swingDirection * swingSpeed * Time.deltaTime));
            rope.RotateAround(currentSwingPoint.position, Vector3.forward, (swingDirection * swingSpeed * Time.deltaTime));

            //makes sure player sprite doesn't rotate as it translates itself with RotateAround  
            transform.rotation = Quaternion.identity;

            //Debug.Log(rope.rotation.eulerAngles.z);

            if (rope.rotation.eulerAngles.z < (360 - swingLimit) || rope.rotation.eulerAngles.z > (180 + swingLimit))
            {
                StopSwinging();
            }
        }
    }

    public void Move()
    {
        //if I'm moving the stick, I set move direction & velocity 
        if (movementInput.x != 0)
        {
            if (!hasMoved)
            {
                hasMoved = true;
                GameManager.instance.splashScreen.SetActive(false);
            }

            xVelocity = Mathf.Abs(movementInput.x);
            moveDirection = movementInput.x > 0 ? 1 : -1;

            if (moveDirection == 1)
                SwitchAnimation(Walk_Right);
            else
                SwitchAnimation(Walk_Left);

        }
        //if I'm jumping, I gradually lose speed 
        else if (!onGround)
        {
            xVelocity -= attributes.playerDeceleration * Time.deltaTime;
            if (xVelocity < 0) xVelocity = 0;

            SwitchAnimation(Airtime);
        }
        //I'm in on the ground and not moving stick, I stop immediately so I don't slide
        else
        {
            xVelocity = 0;

            SwitchAnimation(Idle);
        }

        if (GameManager.instance.playerControl)
        {
            float speed = swingFinished ? attributes.walkSpeed : attributes.walkSpeed * 0;
            playerMovementInput = new Vector3(xVelocity * moveDirection, 0, 0) * speed * Time.deltaTime;
            transform.Translate(playerMovementInput, Space.World);



        }
    }


    public void PrimaryInput()
    {
        if (GameManager.instance.playerControl == true)
        {

            if (!hasMoved)
            {
                hasMoved = true;
                GameManager.instance.splashScreen.SetActive(false);
            }

            if (onGround && rb.velocity.y == 0)
            {
                //Jumping
                rb.velocity = new Vector3(0, attributes.jumpHeight, 0);

                onGround = false;


            }
            // if pressed again while in the air and there's a swing point close by
            else if (!isSwinging)
            {
                //this is the index of the closest valid swing point. Set to -1 to indicate no valid point to start with. 
                int closestSwingPoint = -1;
                float closestDistance = float.MaxValue;
                float distance;

                for (int i = 0; i < swingPoints.Count; i++)
                {
                    distance = Vector3.Distance(transform.position, swingPoints[i].position);
                    //Debug.Log(distance);

                    if (distance < closestDistance && swingPoints[i].position.y > transform.position.y && distance > attributes.notSwingableDistance)
                    {

                        closestDistance = distance;
                        closestSwingPoint = i;
                    }

                }

                //If the swing point is valid
                if (closestSwingPoint != -1)
                {

                    swingFinished = false;

                    //Variable = condition, ? true : false
                    swingDirection = swingPoints[closestSwingPoint].position.x > transform.position.x ? -1 : 1;
                    Vector2 currentSpeed = new Vector2(xVelocity, rb.velocity.y);

                    swingSpeed = Mathf.Clamp(attributes.swingSpeed * xVelocity, minVelo, maxVelo); //(rope.localScale.x/ attributes.minDistance) *; //* currentSpeed.magnitude;

                    currentSwingPoint = swingPoints[closestSwingPoint];
                    ConnectRopeToPlayer(currentSwingPoint);

                    //make sure the player doesn't fall mid-swing
                    rb.velocity = Vector3.zero;
                    rb.gravityScale = 0;

                    //So player doesnt hit other platforms and do weird stuff mid-swing 
                    co.enabled = false;
                    groundDetectionCo.enabled = false;

                    //The player can't move freely when swing
                    GameManager.instance.playerControl = false;
                    isSwinging = true;

                    onGround = false;

                }

            }

        }
        //if press while swinging, stop swinging 
        else if (isSwinging)
        {
            StopSwinging();

        }
    }

    private void StopSwinging()
    {
        isSwinging = false;
        onGround = true;
        GameManager.instance.playerControl = true;

        rb.gravityScale = 1;
        co.enabled = true;
        groundDetectionCo.enabled = true;

        //Flying off monumentum 
        rb.velocity = ropeVelocityReader.up * attributes.swingSpeed * launchFactor * rope.localScale.x;

        rope.gameObject.SetActive(false);
    }

    private void ConnectRopeToPlayer(Transform swingPoint)
    {
        //We show a rope that is as long as the distance between the player and the swing point  
        rope.localScale = new Vector3(Vector3.Distance(transform.position, swingPoint.position), 0.05f, 1);

        //Because the pitviot of the rope is in the middle, we want to set the position to the half the distance. 
        rope.position = Vector3.Lerp(swingPoint.position, shootPoint.position, 0.5f);

        //Make sure the rope is rotated correctly so it connects the player and the swingpoint 
        rope.LookAt(swingPoint);
        rope.Rotate(0, 90, 0);


        rope.gameObject.SetActive(true);
    }

    public void SecondaryInput()
    {

    }

    public void StartInput()
    {
        //GameManager.instance.OnStartPressed();
    }

    public void OptionInput()
    {

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<SwingPoint>())
        {
            swingPoints.Add(collision.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<SwingPoint>())
        {
            swingPoints.Remove(collision.transform);
        }
    }


    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(transform.position, attributes.swingRange);
    //}

}
