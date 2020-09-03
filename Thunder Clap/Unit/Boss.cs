using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : BaseUnit
{
    public GameObject entryPrefab;
    public GameObject attackPrefab;
    public Transform shootPoint;
    public AudioSource audioPlayer;

    public float collideDMG = 20;
    public float attackDelay;

    //Attack Pattern 1//
    //Spread Fire 
    public GameObject spreadFireBulletPrefab;
    private float startAngle = 90f, endAngle = 270f;
    public Vector2Int spreadFireAngleRange;
    public int spreadBulletAmount = 10;
    public float spreadBulletFlySpeed;
    public float spreadFiringFrequency = 0.5f;

    private bool spreadFireAroundFinished;
    private bool swingRight;
    private bool swingLeft;
    private bool moveToCenter;
    private bool moveBack;
    private float leftScreebEnd = -7;
    private float rightScreebEnd = 7;

    //Rotate Shot
    public GameObject rotateShotPrefab;
    public int rotateShotAmount = 5;
    public float rotateShotFireTime = 0.5f;
    public float rotateSpeed = 100;
    public float rotateShotOffset;
    public Transform rotateParent;


    private float rotateAngle;
    private float rotateAngleStep;
    private int currentRotateShotCount;

    //Spiral Shot
    public GameObject spiralShotPrefab;
    public float spiralShotSpeed;
    public int spiralAngleOffset = 10;
    public float spiralFrequency = 0.1f;
    public float spiralAngle;

    private bool isSpiralling = false;
    
    //Swing & Center Dash
    public Transform centerPosition;
    public float defaultMS;
    public float acceleratedMS;
    public float dashMaxTime;


    private float currentlyStayingTime;
    private Vector3 movedirection;
    private float distance;
    private float stopDistance = 0.1f;
    private Vector2 bulletMoveDirection;
    private Vector3 startPosition;

    //Attack Pattern 2 //
    //Star Fall
    public GameObject[] fallingObjects;
    public Vector2 spawnRange;
    public float startFallFrequency;
    public float starFallSpeed;
    public Transform upCenter;

    //Casacading Fall
    public float goToMidDuration = 1;

    public float cascadeObjectSpawnHeight = 1.5f;
    public float cascadingSpawnWaitDuration;
    public float casacadingFallSpeed = 30;
    public int currentCasacadingRound = 0;
    public float midCasacadeOffset = 0.5f;
    public float casacadingFallTime;
    public int cascadingPerSide = 3;
    public int casacadingOffset = 3;
    public int totalCascadeRound;
    public List<GameObject> casacadeObjectSpawned = new List<GameObject>();

    private int currentCascadingAmount;

    private float moveSpeed;
    private IEnumerator currentState;



    protected override void Start()
    {
        base.Start();
        moveSpeed = defaultMS;

        //EntryPrefab is the entry animation
        entryPrefab.SetActive(true);

        //The boss go into attack rotations after the delay
        Invoke("StartAttacking", attackDelay);



    }

    // Currently nothing is running on update 
    protected override void Update()
    {
        base.Update();

    }

    public void StartAttacking()
    {
        SetState(SpreadFireCoroutine());
    }

    private void SetState(IEnumerator newState)
    {

        //When we switch states, we have to make sure, to cancel the old state
        if (currentState != null)
        {
            StopCoroutine(currentState);
        }

        //We store the state so we can keep track which state we're currently in
        currentState = newState;
        StartCoroutine(currentState);
    }

    public void SetDestroyedState()
    {
        SetState(onDestroyed());
    }


    IEnumerator onDestroyed()
    {
        while (true)
        {
            transform.position = transform.position;
            yield return null;
        }
    }



    //Attack Pattern 1
    IEnumerator SpreadFireCoroutine()
    {
        //Make sure to cancel previous attack pattern 
        CancelInvoke("StarFall");

        //Start making the boss move, I chose to make it move right. 
        //Don't change this though, because knowing where the AI starts, I know when a full rotation is done 
        swingRight = true;

        //Because swingRight is true, at the start of the routine, 
        //the enemy is going to move across the screen. This is where I first called SpreadFire. 
        InvokeRepeating("SpreadFire", 0, spreadFiringFrequency);

        //Treat this loop as update!
        //At the end of Center move, when the boss finished one full rotation, it turns "spreadFireAroundFinished" = true
        //And so we break from this loop and move on to the next attack pattern 
        while (spreadFireAroundFinished == false)
        {
            SwingMove();
            CenterMove();

            yield return null;

        }

        //Next pattern!
        SetState(StarFallCoroutine());
    }

    //Attack Pattern 2
    IEnumerator StarFallCoroutine()
    {
        //Make sure to cancel previous attack pattern 
        CancelInvoke("SpreadFire");

        //Start the Star Fall pattern 
        InvokeRepeating("StarFall", 0, startFallFrequency);


        float currentMoveTime = 0;
        float currentCasacadingTime = 0;

        //Storage the current position so I can lerp to the next position 
        Vector3 currentPosition = transform.position;

        //Treat this as an update loop!
        while (true)
        {

            //The enemy is moving towards the designated location over a specific time 
            if (currentMoveTime < goToMidDuration)
            {
                MoveToUpCenter(currentPosition, currentMoveTime);
                currentMoveTime += Time.deltaTime;

            }
            else
            {
                //Once we makes sure the enemy is at the location when the time is over 
                transform.position = upCenter.position;

                //Now, we start a new Attack pattern - CascadingFall

                //if currentRound of CascadingFall is not not the final round
                if (currentCasacadingRound != totalCascadeRound)
                {

                    //if the enemy has not finished spawning all the casacading shots in the current round 
                    if (currentCascadingAmount < cascadingPerSide)
                    {
                        currentCasacadingTime += Time.deltaTime;

                        //it will wait this amount of time to spawn one casacading shot on each side 
                        if (currentCasacadingTime >= cascadingSpawnWaitDuration)
                        {
                            //This will add to currentCascadingAmount each time this is called
                            //We will break from this statement once we've spawned enough
                            CascadingFalll();

                            //Reset the timer after I spawn one 
                            currentCasacadingTime = 0;
                        }


                    }
                    else
                    {
                        //Once the enemy finished spawning all the shots in the current round 
                        for (int i = 0; i < casacadeObjectSpawned.Count; i++)
                        {
                            //It will give the casacading shot speed to make them fall one by one 
                            yield return new WaitForSeconds(casacadingFallTime);
                            casacadeObjectSpawned[i].GetComponent<Projectile>().moveSpeed = casacadingFallSpeed;
                        }

                        //After the current round of CascadingFall finished, 
                        //I reset "currentCascadingAmount" and clear the list to repeat the whole process again until the final round 
                        currentCascadingAmount = 0;
                        casacadeObjectSpawned.Clear();

                        yield return new WaitForSeconds(casacadingFallTime);

                        //After each round's finished, I add to the current round 
                        currentCasacadingRound++;
                    }


                }
                //When the enemy finishes the total number of rounds, it goes back to spreadFire 
                else
                {
                    //Reseting the currentCasacadingRound so the next time this pattern is called, it runs correctly 
                    currentCasacadingRound = 0;

                    //This is the bool used in SpreadFireCoroutine's while loop
                    spreadFireAroundFinished = false;

                    SetState(SpreadFireCoroutine());
                }

            }


            yield return null;
        }
    }


    public void SpiralShot()
    {
        float bulDirX = transform.position.x + Mathf.Sin((spiralAngle * Mathf.PI) / 180f);
        float bulDirY = transform.position.y + Mathf.Cos((spiralAngle * Mathf.PI) / 180f);

        Vector3 bulMoveVector = new Vector3(bulDirX, bulDirY, 0f);
        Vector2 bulDir = (bulMoveVector - transform.position).normalized;

        GameObject spiralShot = GameObjectUtil.Instantiate(spiralShotPrefab, transform.position, Quaternion.identity);
        spiralShot.GetComponent<SpiralShot>().SetMoveDirectionAndSpeed(bulDir, spiralShotSpeed);
        spiralShot.GetComponent<SpiralShot>().owner = this;



        spiralAngle += spiralAngleOffset;


    }


    public void StarFall()
    {
        //We can randomize the things falling 
        //int r = Random.Range(0, fallingObjects.Length);

        // y is 5 becacuse it's a offsecreen height. Z is 1 because everything needs to be 1 to be rendered by the current main camera
        GameObject obstaclePrefab = GameObjectUtil.Instantiate(fallingObjects[1], new Vector3(Random.Range(spawnRange.x, spawnRange.y), 5, 1), Quaternion.identity);

        //Make sure to always set the owner of a projectile, or else it collides with the object that spawns it and disappear right away
        obstaclePrefab.GetComponent<Projectile>().owner = this;

        //Give it the speed we set in this class so the bullets doesnt decide it
        obstaclePrefab.GetComponent<Projectile>().moveSpeed = starFallSpeed;

        //The bullet should also get its damage value from the boss class but I didnt do it :p
    }

    public void MoveToUpCenter(Vector3 startPos, float currentMoveTime)
    {

        transform.position = Vector3.Lerp(startPos, upCenter.position, currentMoveTime / goToMidDuration);

    }

    public void CascadingFalll()
    {
        //Spawn a shot in the middle. 
        //Because this runs before we add to the currentCascadingAmount so we will only spawn one in the middle even though this function is called multiple times  
        if (currentCascadingAmount == 0)
        {
            GameObject cascadingMid = GameObjectUtil.Instantiate(fallingObjects[0], new Vector3(0, midCasacadeOffset, 1), Quaternion.identity);

            //Set the speed to 0 so they dont fall down right away when we first spawn it  
            cascadingMid.GetComponent<Projectile>().moveSpeed = 0;
            cascadingMid.GetComponent<Projectile>().owner = this;

            //Add it to the list. We will add speed to the object in this list one by one 
            casacadeObjectSpawned.Add(cascadingMid);

        }

        currentCascadingAmount++;

        //if (currentCascadingAmount <= cascadingPerSide)
        //{


        //We spawn one shot on both side of the enemy 
        GameObject cascadingRight = GameObjectUtil.Instantiate(fallingObjects[0], new Vector3(currentCascadingAmount * casacadingOffset, cascadeObjectSpawnHeight, 1), Quaternion.identity);
        cascadingRight.GetComponent<Projectile>().moveSpeed = 0;

        GameObject cascadingLeft = GameObjectUtil.Instantiate(fallingObjects[0], new Vector3(currentCascadingAmount * -casacadingOffset, cascadeObjectSpawnHeight, 1), Quaternion.identity);
        cascadingLeft.GetComponent<Projectile>().moveSpeed = 0;


        casacadeObjectSpawned.Add(cascadingRight);
        casacadeObjectSpawned.Add(cascadingLeft);



        //}
        //else
        //{
        //    GameObject cascadingPrefab = GameObjectUtil.Instantiate(fallingObjects[0], new Vector3((currentCascadingAmount - totalCascadingAmount / 2) * -casacadingOffset, 3, 1), Quaternion.identity);
        //    casacadeObjectSpawned.Add(cascadingPrefab);
        //}

    }


    private void SpreadFire()
    {
        float angleStep = (endAngle - startAngle) / spreadBulletAmount;
        float angle = startAngle;

        for (int i = 0; i < spreadBulletAmount; i++)
        {
            //I learned this online. Can't say I understand it 100% :/
            float bulletDirX = transform.position.x + Mathf.Sin((angle * Mathf.PI) / 180f);
            float bulletDirY = transform.position.y + Mathf.Cos((angle * Mathf.PI) / 180f);

            //Makes sure we are using GameObjectUtil.Instantiate for recyclable objects (bullets)
            GameObject bulletShot = GameObjectUtil.Instantiate(spreadFireBulletPrefab, shootPoint.position);

            Vector3 bulletMoveVector = new Vector3(bulletDirX, bulletDirY, 0f);
            Vector2 bulletDirection = (bulletMoveVector - transform.position).normalized;

            SpreadBullet bulletComponent = bulletShot.GetComponent<SpreadBullet>();

            //Make sure to always set the owner of a projectile, or else it collides with the object that spawns it and disappear right away
            bulletComponent.owner = this;

            //Give it the speed we set in this class so the bullets doesnt decide it
            bulletComponent.moveSpeed = spreadBulletFlySpeed;
            bulletComponent.SetMoveDirection(bulletDirection);

            //The bullet should also get its damage value from the boss class but I didnt do it :p

            angle += angleStep;
        }
    }

    private void SwingMove()
    {

        //Everything in these two if statements only happens once 

        //When the boss is at the edge of the right screen, it will stop SpreadFire and move to a destinated spot  
        //Remember to lock this statement with the extra bool, so when the enemy is going to mid and back, it's not resetting its position
        if (transform.position.x >= rightScreebEnd && swingRight == true)
        {
            moveSpeed = acceleratedMS;

            //This is the bool that STOPs the current movement 
            swingRight = false;

            //This is the bool that STARTs the next movment 
            moveToCenter = true;

            //Cancel the fire 
            CancelInvoke("SpreadFire");

            //Storage current position so I can lerp the enemy from back from the destinated spot and make an infinite loop 
            startPosition = transform.position;

        }
        //This basically does the same thing, except now the enemy is on the edge of the left screen 
        else if (transform.position.x <= leftScreebEnd && swingLeft == true)
        {
            moveSpeed = acceleratedMS;

            swingLeft = false;
            moveToCenter = true;

            CancelInvoke("SpreadFire");


            startPosition = transform.position;

        }


        //These two statements happens every frame as long as the bools are true 
        if (swingRight)
        {
            transform.position = new Vector3(transform.position.x + moveSpeed * Time.deltaTime, transform.position.y, 1);

        }

        if (swingLeft)
        {
            transform.position = new Vector3(transform.position.x - moveSpeed * Time.deltaTime, transform.position.y, 1);
        }



    }


    public void CenterMove()
    {
        //Everything in here happens over multiple frames 
        //When this bool is on, the enemy has to be on the edge of the screen (left or right)
        if (moveToCenter)
        {
            //Now, I get the direction of the next location by using the formula: "Direction = final - intial"
            movedirection = centerPosition.position - transform.position;

            //I can also get the distance between the enemy and the destinated spot by grabbing its magnitude 
            distance = movedirection.magnitude;

            //Careful: Remember to play with the stopDistance before setting variable to private. 
            //If the enemy is moving too fast, it may not be able to stop 
            if (distance >= stopDistance)
            {
                //Remember to normalize direction
                transform.position += movedirection.normalized * moveSpeed * Time.deltaTime;

            }

            //When the enemy reaches the center position
            else
            {
                //We make it stop 
                transform.position = centerPosition.position;

                //Since everything in this if(MoveToCenter) Statement happens over multiple fames,
                //I have to lock this statement with a bool and only call InvokeRepeating once 
                if (isSpiralling == false)
                {
                    InvokeRepeating("SpiralShot", 0, spiralFrequency);
                    isSpiralling = true;
                }

                //Now that the enemy is staying in the center position
                //We start a new Attack Pattern, RotateShot



                //If the enemy has not spawn enough rotate shots 
                if (currentRotateShotCount < rotateShotAmount)
                {
                    currentlyStayingTime += Time.deltaTime;

                    //It will spawn one rotate shot every "rotateShotFireTime"
                    if (currentlyStayingTime >= rotateShotFireTime)
                    {
                        //we add 1 currentRotateShotCount every time this function is called 
                        RotateShotFire();

                        //reset it until currentRotateShotCount is the total rotateShotAmount
                        currentlyStayingTime = 0;
                    }


                }

                //if the enemy has spawned all the rotate shots 
                else if (currentRotateShotCount == rotateShotAmount)
                {
                    //Now, we break from this sequence 
                    moveToCenter = false;

                    //Reset the main gate keeper of this sequence 
                    currentRotateShotCount = 0;

                    //And the enemy starts a new move sequence 
                    moveBack = true;


                }

            }

        }


        //This runs over multiple frames 
        if (moveBack)
        {
            //Stop the InvokeRepeating 
            CancelInvoke("SpiralShot");

            //Resets its angle
            spiralAngle = 0;

            //And reset its gate keeping bool 
            isSpiralling = false;

            //We do the samething here as the MoveToCenter statement 
            //Except we are now going back where we came from. 
            //startPosition is set when the enemy first reach the edge of the screen 
            movedirection = transform.position - startPosition;
            distance = movedirection.magnitude;

            if (distance >= stopDistance)
            {
                transform.Translate(movedirection.normalized * moveSpeed * Time.deltaTime);
            }

            //Once we are back at the edge of the screen
            else
            {

                moveSpeed = defaultMS;

                //We make sure we are at the position 
                transform.position = startPosition;

                //we break from the this moveBack sequence
                moveBack = false;

                //if x is less than 0 (negative value), we know the boss is at the left side of screen 
                if (startPosition.x < 0)
                {
                    //So we swing right 
                    swingRight = true;

                    //We set the spreadFire angle accordingly 
                    startAngle = spreadFireAngleRange.x;

                    endAngle = spreadFireAngleRange.y;

                    //This is where one whole SpreadFire rotation is finished 
                    Debug.Log("One round has finished");

                    //Now we can break from the SpreadFire coroutine and go to StarFall coroutine
                    spreadFireAroundFinished = true;

                }
                else
                {

                    startAngle = spreadFireAngleRange.y;

                    endAngle = spreadFireAngleRange.x;

                    //This would be the first time the enemy gets to the right side of the screen
                    swingLeft = true;
                }



                //We know that we will be move somewhere when the code reaches here, so we can start firing across the screen
                //This is why before StarFall starts, the enemy fires SpreadShots once before being cancelled 
                InvokeRepeating("SpreadFire", 0, spreadFiringFrequency);

            }

        }



    }

    private void RotateShotFire()
    {
        //Makes sure we are using GameObjectUtil.Instantiate for recyclable objects (bullets)
        GameObject bulletShot = GameObjectUtil.Instantiate(rotateShotPrefab, new Vector3(rotateParent.position.x + rotateShotOffset, rotateParent.position.y, 1));

        //Rotate shots need to move with the boss so I set their parent to be a object on the boss 
        bulletShot.transform.SetParent(rotateParent);


        RotateShot bulletComponent = bulletShot.GetComponent<RotateShot>();

        //Give each spawned rotate shot a more informative bame 
        bulletComponent.gameObject.name = "rotateShot: " + currentRotateShotCount + " / " + rotateShotAmount;

        //Make sure to always set the owner of a projectile, or else it collides with the object that spawns it and disappear right away
        bulletComponent.owner = this;

        //This gives the rotateshot class a target to orbit around 
        bulletComponent.rotateparent = rotateParent;

        //Give it the speed we set in this class so the bullets doesnt decide it
        bulletComponent.moveSpeed = rotateSpeed;

        //The bullet should also get its damage value from the boss class but I didnt do it :p

        currentRotateShotCount++;


    }


    public override void TakeDamage(float damage)
    {

        //Provid Audio feedback 
        //The boss object only has onHit audio right now. 
        audioPlayer.Play();

        //This reduces health + trigger animation 
        base.TakeDamage(damage);

        // let the game manager know when the boss dies and it will transition to the win game state
        if (currentHealth <= 0)
        {
            GameManager.instance.Win();
        }
    }

}
