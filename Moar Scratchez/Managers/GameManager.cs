using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    //public float currentLength;

    //public bool isPaused = false;
    public KeyCode keyToPress;

    //private bool skipTutorial = false;

    private bool inputLock = true;
    public GameState currentState;


    private bool inputTimerStarted = false;
    public float holdConfirmationDuration = 1.5f;
    private float holdingTime = 0;
    private float holdingThreshold = 0.1f;


    public float minimumHappiness;


    public enum GameState
    {
        Start,
        Entry,
        Tutorial,
        PettingTime,
        LevelSucceed,
        LevelFail,
        GameFinished,
    }


    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        EnterStartState();
    }

    // Update is called once per frame
    void Update()
    {

        if (!inputLock)
        {

            //Whenever the player lets go of Key, this will 
            // (1) check if they chose the tap or the hold option,
            // (2) Reset the visual feedback if they were holding (but did not complete)
            // (3) send them to the correct state based on their option and their current sequence.
            if (Input.GetKeyUp(keyToPress))
            {
                UIManager.instance.StopHoldSFX();

                //Controls game state
                HandleConfirmationInput(holdingTime >= holdConfirmationDuration);


                inputTimerStarted = false;

                //Reset UI fill meter
                holdingTime = 0;
                HandleHoldOption();

                //Reset Holding sound setting 
                UIManager.instance.StopHoldSFX();

            }

            //Whenever the player presses key, it starts the timer to detect whether they are holding
            if (Input.GetKeyDown(keyToPress))
            {
                inputTimerStarted = true;

            }

            //If the player is holding, this will
            // (1) keep track of how long they've been pressing
            // (2) talk to UI Manager and shows visual feedback
            if (Input.GetKey(keyToPress) && inputTimerStarted == true)
            {

                HandleHoldOption();
                holdingTime += Time.deltaTime;



            }


        }






    }

    public void HandleConfirmationInput(bool wasHolding)
    {

        switch (currentState)
        {
            case GameState.Start:
                {
                    ExitStartState();

                    EnterEntryState();
                }

                break;

            case GameState.Entry:
                {
                    ExitEntryState();
                    BeatManager.instance.PlayPositveSFX();

                    //if the player is holding 
                    if (wasHolding == true)
                    {
                        Debug.Log("Enter Petting State");
                        EnterPettingState();
                    }
                    else
                    {
                        Debug.Log("Enter Tutorial state");
                        EnterTutorialState();
                    }
                }

                break;

            case GameState.Tutorial:
                {
                    ExitTutorialState();
                    EnterPettingState();
                }
                break;

            case GameState.PettingTime:
                break;

            case GameState.LevelSucceed:

                if (wasHolding == true)
                {
                    Debug.Log("Player exiting game");

                }
                else
                {
                    //UIManager.instance.HideLevelPopup();

                    //Player goes to the next level
                    EnterPettingState();
                }

                break;

            case GameState.LevelFail:
                if (wasHolding == true)
                {
                    Debug.Log("Player exiting game");

                }
                else
                {
                    //UIManager.instance.HideLevelPopup();

                    //Player restart the current level 
                    EnterPettingState();
                }

                break;

            case GameState.GameFinished:
                break;

        }
    }


    public void HandleHoldOption()
    {
        switch (currentState)
        {
            case GameState.Start:
                break;

            case GameState.Entry:

                if (holdingTime > holdingThreshold && !UIManager.instance.isPlayingHoldingSFX)
                {
                    UIManager.instance.PlayHoldSFX();
                }

                UIManager.instance.SetHoldConfirmationFillAmount(holdingTime, holdConfirmationDuration);
                break;

            case GameState.Tutorial:
                break;


            case GameState.PettingTime:
                break;

            case GameState.LevelSucceed:

                break;

            case GameState.LevelFail:

                break;


            case GameState.GameFinished:

                Application.Quit(); 
                break;


        }
    }

    public void EnterStartState()
    {
        UnlockInput(gameObject);

        currentState = GameState.Start;
        UIManager.instance.ShowUI(UIManager.instance.startScreen);
    }
    public void ExitStartState()
    {
        UIManager.instance.HideUI(UIManager.instance.startScreen);
    }



    public void EnterEntryState()
    {
        LockInput(gameObject);
        UIManager.instance.SetHoldingOptionImage(HoldFillingImage.skipTutorialMeter);
        UIManager.instance.StartIntroducingCat();

        currentState = GameState.Entry;

    }
    public void ExitEntryState()
    {
        UIManager.instance.HideUI(UIManager.instance.skipTutorialPopup);
    }


    public void EnterTutorialState()
    {
        currentState = GameState.Tutorial;
        UIManager.instance.StartTutorial();
        LockInput(gameObject);
    }
    public void ExitTutorialState()
    {
        //currentState = GameState.PettingTime;
        UIManager.instance.HideUI(UIManager.instance.initialMessage);
    }


    public void EnterPettingState()
    {
        UIManager.instance.StopHoldSFX();
        UIManager.instance.HideLevelPopup();


        BeatManager.instance.LoadLevel();

        UIManager.instance.ShowUI(UIManager.instance.gameplayUI);

        currentState = GameState.PettingTime;
        UnlockInput(gameObject);

    }


    public void LockInput(GameObject locker)
    {
        inputLock = true;
        Debug.Log(gameObject.name + " has locked input");


    }

    public void UnlockInput(GameObject locker)
    {
        inputLock = false;
        Debug.Log(gameObject.name + " has unlocked input");
    }

    public void OnCurrentLevelFinished()
    {
        LockInput(gameObject);
        DetermineLevelProgression();

    }




    public void DetermineLevelProgression()
    {
        //if player succeeds 
        if (BeatManager.instance.currentHappeiness >= minimumHappiness)
        {
            //We check if there are more levels left in the game
            if (LevelManager.instance.CheckAndUpdateCurrentLevel())
            {

                currentState = GameState.LevelSucceed;

                //If there are more, show pop up 
                UIManager.instance.ShowLevelSucceedMSG();
                

                

            }
            else
            {
                currentState = GameState.GameFinished;

                UIManager.instance.ShowUI(UIManager.instance.endGame);
                //if not, we end the game

            }

        }
        //if player fails 
        else
        {
            currentState = GameState.LevelFail;

            UIManager.instance.ShowLevelFailMSG();

            

        }

    }




    public void CompleteLevels()
    {

        Debug.Log("Finished!");
    }


}
