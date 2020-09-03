using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    private Animator mainMenuAnim;

    [Header("UI Objects")]
    public GameObject mainMenu;
    public GameObject readyPopup;
    public GameObject infoPanel;
    public GameObject wrongGuess;
    public GameObject playerIndicator;
    public GameObject CatcherwinScreen;
    public GameObject OddOneWinScreen;

    [Header("UI Text")]
    public Text countDown;
    public Text timerText;
    public Text attemptsRemaining;
    public Text oddOneCatured;

    [Header("UI Timing")]
    public float mainMenuFadeOutTime = 1.2f;
    public float indicatorDisappearTime = 2f;



    private void Awake()
    {
        mainMenuAnim = mainMenu.GetComponent<Animator>();

        readyPopup.SetActive(false);
        CatcherwinScreen.SetActive(false);
        OddOneWinScreen.SetActive(false);
        infoPanel.SetActive(false);




        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.instance.currentState == GameState.Play)
        {
            UpdateCountDown();
        }
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);

    }

    public void SetMainMenuExitAnimation()
    {
        mainMenuAnim.SetTrigger("FadeOut");

        Invoke("HideMainMenu", mainMenuFadeOutTime);
    }

    public void ToggleReadyPopUP(bool active)
    {
        readyPopup.SetActive(active);

    }

    public void HideMainMenu()
    {
        mainMenu.SetActive(false);
        
    }

    public void OnPlayerReadyButtonPressed()
    {
        ToggleReadyPopUP(false);
        ToggleInfoPanel(true);

        GameManager.instance.OnPlayerReadyToPlay();
    }

    public void ToggleInfoPanel(bool active)
    {
        infoPanel.SetActive(active);
    }



    public void UpdateCountDown()
    {
        if (!GameManager.instance.catcherActive)
        {
            int catcherWaitTime = Mathf.FloorToInt(GameManager.instance.currentCatcherWaitTime);

            if ((catcherWaitTime - indicatorDisappearTime) <= 0)
            {
                TogglePlayerIndicator(false);
            }

            if (catcherWaitTime <= 0)
            {
                AudioManager.instance.OnCatcherWakeUp();
                timerText.text = "Catcher Wake up please!!";
                countDown.text = " ";
                return;
            }

            timerText.text = "Catcher Ready in";
            countDown.text = catcherWaitTime + "s";
        }
        else
        {
            int currentPlayTime = Mathf.FloorToInt(GameManager.instance.currentPlayTime);

            if (currentPlayTime <= 0)
            {
                timerText.text = "The Odd One has failed!";
            }
            
            timerText.text = "The Odd one fails in ";
            countDown.text = currentPlayTime + "s";

        }


    }

    public void UpdateCatcherAttempt()
    {
        attemptsRemaining.text = "Catcher Attempts Remaining: " + GameManager.instance.attemptsRemaining.ToString();
    }

    public void UpdateOddOneCapturedCount()
    {
        oddOneCatured.text = "Odd One Objective Captured: " + GameManager.instance.currentCapturedObjects.ToString();
    }

    public void TogglePlayerIndicator(bool active)
    {
        playerIndicator.SetActive(active);
    }

    public void OnReplayButtonPressed()
    {
        GameManager.instance.OnReplay();
    }

    public void ShowWrongGuess()
    {
        wrongGuess.SetActive(true);
        Invoke("HideWrongGuess", 0.5f);
    }

    public void HideWrongGuess()
    {
        wrongGuess.SetActive(false);
    }
}
