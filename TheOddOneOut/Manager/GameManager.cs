using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Start,
    Play,
    Replay,

}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameState currentState;

    [Header("Character Set up")]
    public List<Transform> oddOneSpawnPoints = new List<Transform>();
    public List<Transform> oddOneObjectives = new List<Transform>();
    public PlayerAttributes playerAttributes;
    public GameObject oddOneRef;
    public GameObject oddOneDisguise;
    public LayerMask unitMask;


    public bool catcherActive = false;
    public bool playerControl = false;
    private bool objectiveAssigned = false;
    private bool playerSpawned = false;
    private bool isReady = false;

    [Header("Wait Times")]
    public float playerSpawnWaitTime = 0.5f;
    public float currentSpawnWaitTime = 0;

    public float catcherWaitTime = 6f;
    public float currentCatcherWaitTime = 0;

    public float playTime = 200f;
    public float currentPlayTime = 0;

    [Header("Game Settings")]
    public int maxOddOneObjectives = 3;
    public int currentCapturedObjects = 0;

    public int maxCatcherAttempts = 3;
    public int attemptsRemaining = 0;




    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        SetState(GameState.Start);
    }

    // Update is called once per frame
    void Update()
    {

        if (currentState == GameState.Play && isReady)
        {
            SpawnPlayerAtRandomLocation();

            CountDownCatcherWaitTime();

            if (catcherActive)
            {
                if (!objectiveAssigned)
                {
                    SelectNewObjectives();
                }

                currentPlayTime -= Time.deltaTime;

                if (currentPlayTime > 0)
                {
                    if (currentCapturedObjects == maxOddOneObjectives)
                    {
                        Debug.Log("Odd one captures all the objectives");
                        OnOddOneWin();
                    }

                    if (attemptsRemaining != 0)
                    {
                        HandleCatcherInput();
                    }
                    else
                    {
                        Debug.Log("The catcher used all its attempts");
                        OnOddOneWin();
                    }

                }
                else
                {
                    Debug.Log("Odd one ran out of time");
                    OnCatcherWin();
                }


            }

        }



    }

    private void HandleCatcherInput()
    {
        //This creates a Ray object, starting at our mouse position, pointing into the world
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hitInfo;

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, unitMask))
            {
                attemptsRemaining--;

                if (hitInfo.collider.gameObject.GetComponent<PlayerInputController>() != null)
                {
                    Debug.Log("Busted!");
                    OnCatcherWin();
                }
                else
                {
                    Debug.Log("Keep Looking!");
                    UIManager.instance.ShowWrongGuess();
                }

                UIManager.instance.UpdateCatcherAttempt();

            }
        }
    }

    private void CountDownCatcherWaitTime()
    {
        if (!catcherActive)
        {
            currentCatcherWaitTime -= Time.deltaTime;

            if (currentCatcherWaitTime < 0)
            {
                catcherActive = true;
            }
        }
    }

    private void SpawnPlayerAtRandomLocation()
    {
        if (!playerSpawned)
        {
            currentSpawnWaitTime += Time.deltaTime;

            if (currentSpawnWaitTime >= playerSpawnWaitTime)
            {
                playerAttributes.PickADisguise();

                int r = Random.Range(0, oddOneSpawnPoints.Count);
                oddOneRef.transform.position = oddOneSpawnPoints[r].position;

                oddOneDisguise.SetActive(true);
                UIManager.instance.TogglePlayerIndicator(true);
                playerSpawned = true;


                playerControl = true;
            }
        }
    }

    public void SetState(GameState newState)
    {
        currentState = newState;
        switch (currentState)
        {
            case GameState.Start:

                UIManager.instance.ShowMainMenu();
                oddOneDisguise.SetActive(false);
                break;

            case GameState.Play:
                UIManager.instance.ToggleReadyPopUP(true);
                currentSpawnWaitTime = 0;
                currentCatcherWaitTime = catcherWaitTime;
                currentPlayTime = playTime;

                attemptsRemaining = maxCatcherAttempts;
                UIManager.instance.UpdateCatcherAttempt();

                currentCapturedObjects = 0;
                UIManager.instance.UpdateOddOneCapturedCount();

                DeactivateAllObjectives();



                break;

            case GameState.Replay:
                UIManager.instance.ToggleInfoPanel(false);
                isReady = false;
                catcherActive = false;
                playerSpawned = false;
                objectiveAssigned = false;


                break;


        }

    }

    private void DeactivateAllObjectives()
    {
        for (int i = 0; i < oddOneObjectives.Count; i++)
        {
            //oddOneObjectives[i].GetComponentInChildren<ObjectiveMarker>().gameObject.SetActive(false);

            ObjectiveMarker objective = oddOneObjectives[i].GetComponentInChildren<ObjectiveMarker>();

            if (objective != null)
            {
                objective.gameObject.SetActive(false);

            }

        }
    }

    private void SelectNewObjectives()
    {
        List<Transform> currentObjectives = new List<Transform>(oddOneObjectives);

        for (int j = 0; j < maxOddOneObjectives; j++)
        {
            int r = Random.Range(0, currentObjectives.Count);
            ObjectiveMarker objective = currentObjectives[r].GetComponentInChildren<ObjectiveMarker>(true);

            if (objective != null)
            {
                objective.gameObject.SetActive(true);
                currentObjectives.Remove(currentObjectives[r]);

            }
            else
            {
                Debug.Log("objective null");
            }

        }

        objectiveAssigned = true;
    }

    public void OnPlayerStart()
    {
        AudioManager.instance.OnPositiveFeedback();
        UIManager.instance.SetMainMenuExitAnimation();
        SetState(GameState.Play);

    }

    public void OnPlayerReadyToPlay()
    {
        AudioManager.instance.OnPositiveFeedback();
        isReady = true;
    }

    public void OnCatcherWin()
    {
        SetState(GameState.Replay);

        UIManager.instance.CatcherwinScreen.SetActive(true);

        oddOneDisguise.SetActive(false);
        playerControl = false;
    }

    public void OnOddOneWin()
    {
        SetState(GameState.Replay);

        UIManager.instance.OddOneWinScreen.SetActive(true);
        oddOneDisguise.SetActive(false);
        playerControl = false;
    }

    public void OnReplay()
    {
        UIManager.instance.CatcherwinScreen.SetActive(false);
        UIManager.instance.OddOneWinScreen.SetActive(false);
        SetState(GameState.Play);
    }

    public void OnOddOneCapture()
    {
        currentCapturedObjects++;
    }
}
