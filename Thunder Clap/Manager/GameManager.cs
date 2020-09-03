using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private float camPanTime;
    public float camPanDuration;

    public GameObject bossPrefab;
    public GameObject playerPrefab;
    public Transform respawnSpot;

    public GameObject explosionPrefab;
    public float bossDestroyedWaitDuration = 4;
    private float bossDestroyedWaitTime;

    public SpriteRenderer playerSprite;
    public Collider2D playerCo;
    public Jet playerComponent;

    //public SpriteRenderer EnemySprite;
    public Boss bossComponent;

    public Camera introCam;
    public Camera mainCam;
    public Vector3 camStartPos;
    public Vector3 camEndPos;

    public float currentWaitTime;
    public float respawnDelayTime;

    //Referenced by other scripts but should not be touched 
    [HideInInspector]
    public bool defeated = false;
    [HideInInspector]
    public bool win = false;
    [HideInInspector]
    public bool showBoss;
    [HideInInspector]
    public GameState currentState;
    [HideInInspector]
    public bool moveCam;

    public GameObject tutorialBox;



    public bool playerControl;


    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Main camera is where screen boundary is set so I turn it off when I'm moving the intro camera
        mainCam.gameObject.SetActive(false);

        introCam.transform.position = camStartPos;

        SetState(GameState.Start);
    }

    public enum GameState
    {
        Start,
        Gameplay,
        Restart,
        Win,
    }


    public void SetState(GameState newState)
    {
        Debug.Log("Entering state : " + newState);
        currentState = newState;

        //These states only happen once (when Setstate is called)
        
        //Unlike the Boss's states, in which a state is a coroutine //
        //Every while loop is treated as the Update function //
        switch (currentState)
        {
            case GameState.Start:
                {
                    //Boss object in the hierachy should be set to false before playing. 
                    //Hide boss through code on run time somehow mess up its routine ;O
                    //HideBoss();


                    UIManager.instance.ShowTitle();
                    
                    //When moveCam is true, the camera will lerp to its endpost on update 
                    moveCam = true;
                    
                    //Lock player's input while the intro camera is moving 
                    playerControl = false;


                }
                break;


            case GameState.Gameplay:
                {
                    //Shows gameplayUI & introduce the boss 
                    OnGamePlayStart();

                    //Reset some settings, in case the player gets here from the restart state 
                    UIManager.instance.HideDefeatMessage();
                    defeated = false;

                }
                break;

            case GameState.Restart:
                {
                    //I added a UI buffer that makes player wait for a second before restarting
                    //This resets that timer  
                    currentWaitTime = 0;

                    //Lock player input until the timer is up 
                    playerControl = false;

                    UIManager.instance.ShowDefeatMessage();
                }
                break;

            case GameState.Win:
                {
                    Destroy(bossPrefab);
                    UIManager.instance.ShowWinMessage();
                }
                break;

        }
    }






    // Update is called once per frame
    void Update()
    {
        //This happens after setting the Start game state 
        if (moveCam)
        {
            camPanTime += Time.deltaTime;

            introCam.transform.position = Vector3.Lerp(camStartPos, camEndPos, camPanTime / camPanDuration);

            //When the intro cam lerps to the end position, we make sure it's set to the end position before swtiching to main camera 
            if (camPanTime >= camPanDuration)
            {
                introCam.transform.position = camEndPos;
                mainCam.transform.position = camEndPos;

                mainCam.gameObject.SetActive(true);
                introCam.gameObject.SetActive(false);

                //Now I can turn this statement off 
                moveCam = false;

                //Show player sprite and unlock the control
                ShowPlayer();
                playerControl = true;

                //Show their target 
                tutorialBox.SetActive(true);

            }
        }

        //This happens after the CurrentWaitTime has been resetted 
        if (currentState == GameState.Restart)
        {
            //Add a Time Buffer here before restarting. 
            //So players can't just quick click through the restart menu if they happen to be smashing the buttons
            currentWaitTime += Time.deltaTime;

            //To separate functionality from UI:
            //UI Manager takes care of the visual indication of the wait while the game manager keeps track of time 
            UIManager.instance.ShowRestartFill(currentWaitTime, respawnDelayTime);

            //When the timer is up
            if (currentWaitTime >= respawnDelayTime)
            {
                //Now I can give the player controls back. 
                //Once they press their primary button, we transition back to gameplay state and break off from this statement.
                playerControl = true;
            }

        }

        //After the player wins, the boss's destoryed animation will run. 
        //This sets a wait time before we destroy the boss's prefab  
        //Checking two bools to make sure this is only called once 
        if (win && currentState != GameState.Win)
        {
            bossDestroyedWaitTime += Time.deltaTime;

            if (bossDestroyedWaitTime >= bossDestroyedWaitDuration)
            {
                SetState(GameState.Win);
            }
        }

    }

    //Called when the player is destroyed 
    public void OnPlayerDeath()
    {
        //Check if defeat is false so this can only trigger once 
        if (defeated == false)
        {
            //Enter the proper state when the player dies 
            SetState(GameState.Restart);

            //Now I can turn this statement off
            //Also, the player is checking this bool to call OnRestart
            defeated = true;

            //Don't destroy the player object and re-instantiate it, just turn the relevant things off. 
            playerSprite.enabled = false;
            playerCo.enabled = false;
        }

    }


    //This is called after the player died, waited for respawn timer and then pressed the primary button. 
    public void OnRestart()
    {
        //
        SetState(GameState.Gameplay);
       
        //Respawn them at the same place 
        playerPrefab.transform.position = respawnSpot.position;

        //turn the relevant things back on 
        playerSprite.enabled = true;
        playerCo.enabled = true;
        
        //Reset the health of both parties 
        playerComponent.currentHealth = playerComponent.maxHealth;
        bossComponent.currentHealth = bossComponent.maxHealth;

    }

    public void Win()
    {
        //Have a bool check so this can only trigger once 
        if (win == false)
        {
            //Stop the boss's routine.  
            bossComponent.SetDestroyedState();
            //Show the destroyed animation 
            explosionPrefab.SetActive(true);

            //Now I can turn this statement off 
            win = true;
        }


    }

    //This is where the game transition to gameplay state
    //After the player destroy the tutorial hit target 
    public void OnUIBoxDestroyed()
    {
        UIManager.instance.HideTitle();
        SetState(GameState.Gameplay);
    }

    public void ShowPlayer()
    {
        playerPrefab.SetActive(true);
    }


    public void OnGamePlayStart()
    {
        //this shows both the player and the boss's hp
        UIManager.instance.ShowHpBars();

        bossPrefab.SetActive(true);

    }

    public void HideBoss()
    {
        bossPrefab.SetActive(false);
    }

}
