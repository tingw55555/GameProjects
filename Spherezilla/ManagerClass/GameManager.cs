using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool usingStateMachine;

    public AudioSource audioPlayer;
    public GameState currentState;
    public GameState previousState;

    private bool canStartGame = false;
    [HideInInspector]
    public bool playerControl = true;
    public int playerTotalScore;
    public int playerScoreAfterArmyArrived;

    public Camera cam;
    public Vector3 camStartPos;
    public Vector3 camEndPos;
    private bool openingCamMovement;
    private float currentCamPanTime;
    public float camPanDuration;

    public GameObject openingSphere;
    public Vector3 openingSphereStartPos;
    public Vector3 openingSphereEndPos;
    public float sphereMoveDuration;

    public Vector3 playerStartPos;
    public Vector3 playerEndPos;
    public float playerRollinTime;

    public int armyArrival_ScoreThreshHold;
    public int moreShips_ScoreThreshHold;
    public int currentShips;
    public bool isArmyComing = false;

    public GameObject battleShipPrefab;



    private Sequence sphereMoveIn;
    //private Tween playerRollIn;


    public enum GameState
    {
        Start,
        DestructionTime,
        Pause,
        Restart,
    }


    private void Awake()
    {
        instance = this;
    }


    // Start is called before the first frame update
    void Start()
    {
        sphereMoveIn = DOTween.Sequence();

        openingSphere.transform.position = openingSphereStartPos;
        Tween moveTween = openingSphere.transform.DOMove(openingSphereEndPos, sphereMoveDuration);

        sphereMoveIn.Append(moveTween);

        CanvasGroup titleText = UIManager.instance.titleText.GetComponent<CanvasGroup>();
        titleText.alpha = 0;
        Tween fadeTween = titleText.DOFade(1, 0.5f);

        sphereMoveIn.Append(fadeTween);

        PlayerInputController.instance.transform.position = playerStartPos;
        PlayerInputController.instance.transform.rotation = new Quaternion(0, 0, 360, 0);






        if (usingStateMachine)
        {
            SetState(GameState.Start);
        }
        else
        {
            openingSphere.SetActive(false);
            UIManager.instance.HideUI(UIManager.instance.playerScoreUI);
        }


        //Debug.Log("Count: " + FindObjectsOfType<MeshExplosion>().Length);

    }

    // Update is called once per frame
    void Update()
    {

        OpeningCameraSequence();
    }


    public void SetState(GameState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case GameState.Start:
                {
                    Time.timeScale = 1;
                    canStartGame = false;
                    playerControl = false;

                    playerTotalScore = 0;
                    playerScoreAfterArmyArrived = 0;
                    currentShips = 0;
                    isArmyComing = false;

                    cam.transform.position = camStartPos;

                    openingSphere.SetActive(true);
                    UIManager.instance.ShowUI(UIManager.instance.titleText);

                    openingCamMovement = true;


                }
                break;


            case GameState.DestructionTime:
                {
                    Time.timeScale = 1;
                    playerControl = true;

                    UIManager.instance.ShowUI(UIManager.instance.playerScoreUI);
                    UIManager.instance.HideUI(UIManager.instance.optionPopUp);

                }
                break;

            case GameState.Pause:
                {
                    Time.timeScale = 0;
                    UIManager.instance.ShowUI(UIManager.instance.optionPopUp);
                }
                break;

            case GameState.Restart:
                {
                    Time.timeScale = 1;

                    playerTotalScore = 0;
                    playerScoreAfterArmyArrived = 0;
                    currentShips = 0;
                    isArmyComing = false;

                    UIManager.instance.OnPlayerScoreChange(playerTotalScore);




                    PlayerInputController.instance.transform.position = playerStartPos;
                    PlayerInputController.instance.transform.rotation = new Quaternion(0, 0, 360, 0);
                    IntroducePlayer();

                }
                break;

        }


    }




    public void OpeningCameraSequence()
    {

        if (openingCamMovement)
        {
            currentCamPanTime += Time.deltaTime;

            cam.transform.position = Vector3.Lerp(camStartPos, camEndPos, currentCamPanTime / camPanDuration);

            if (currentCamPanTime >= camPanDuration)
            {
                cam.transform.position = camEndPos;
                openingCamMovement = false;
                canStartGame = true;

                sphereMoveIn.Play();
            }

        }

    }



    public void PlayerRollingIn()
    {
        if (currentState == GameState.Start && canStartGame)
        {
            sphereMoveIn.PlayBackwards();

            sphereMoveIn.OnRewind(IntroducePlayer);

        }

    }

    public void OnPlayerPause()
    {
        if (currentState != GameState.Pause && canStartGame == true)
        {
            previousState = currentState;
            SetState(GameState.Pause);

        }
        else if (currentState == GameState.Pause && canStartGame == true)
        {
            UIManager.instance.HideUI(UIManager.instance.optionPopUp);
            SetState(previousState);
        }
    }


    public void OnPlayerReset()
    {
        playerControl = false;
        SetState(GameState.Restart);
    }


    public void IntroducePlayer()
    {
        Tween playerRollIn = PlayerInputController.instance.transform.DOMove(playerEndPos, playerRollinTime);
        Tween playerRotate = PlayerInputController.instance.transform.DORotate(Vector3.zero, playerRollinTime);
        playerRollIn.OnComplete(PlayerEnterDestructionTime);
        playerRollIn.Play();
        playerRotate.Play();
    }

    public void PlayerEnterDestructionTime()
    {
        SetState(GameState.DestructionTime);
    }



    public void OnPlayerGainScore(int score)
    {
        audioPlayer.Play();

        if (currentState == GameState.DestructionTime)
        {
            playerTotalScore += score;

            UIManager.instance.OnPlayerScoreChange(playerTotalScore);

            //if (playerTotalScore >= armyArrival_ScoreThreshHold && isArmyComing == false)
            //{
            //    //OnArmyFirstArrive();
            //    SpawnExtraShip();
            //    isArmyComing = true;

            //}

            //if (isArmyComing)
            //{
            //    playerScoreAfterArmyArrived += score;

            //    if (playerScoreAfterArmyArrived >= moreShips_ScoreThreshHold)
            //    {
            //        SpawnExtraShip();
            //    }
            //}
        }

    }


    public void OnArmyFirstArrive()
    {
        Debug.Log("City alarm has been triggered!");

        SpawnExtraShip();
    }

    public void SpawnExtraShip()
    {

        if (BattleshipManager.instance.CheckSpawnPositionAvaliablity())
        {
            playerScoreAfterArmyArrived = 0;
            currentShips += 1;

            BattleshipSpawns spawnInfo = BattleshipManager.instance.GetSpawnPosition();

            GameObject newShip = Instantiate(battleShipPrefab, spawnInfo.spawnPosition, Quaternion.Euler(0, spawnInfo.yRotation, 0));

            Debug.Log("A ship is coming! Currently there are " + currentShips + " in the scene");


        }

    }



}
