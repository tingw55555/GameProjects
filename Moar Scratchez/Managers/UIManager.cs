using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum HoldFillingImage
{
    skipTutorialMeter = 0,
}


public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public Canvas canvas;
    public Text scoreUI;

    public GameObject initialMessage;

    public GameObject startScreen;
    public GameObject[] tutorialInfo;
    public float waitTimePerImage;
    private float tutorialAnimationWaitTime = 13;
    public GameObject skipTutorialPopup;

    public GameObject cat;
    public AudioSource catAudioPlayer;
    public AudioClip holdSfx;
    public float catWalkTime;
    public Transform catEndPosition;
    private Vector3 catCurrentPosition;

    [HideInInspector]
    public bool isPlayingHoldingSFX = false;

    public Animator pawIcon;
    private Image currentHoldOption;

    public GameObject gameplayUI;
    public Image[] fillImage;


    public GameObject hitPrompt;

    public GameObject[] failedSequence;
    public GameObject[] succeedSequence;

    public List<GameObject> levelPopup = new List<GameObject>();

    public Slider catHappinessSlider; 

    public GameObject endGame;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateHappiness();
    }

    // Update is called once per frame
    void Update()
    {
        if (BeatManager.instance.withinHitZone)
        {
            hitPrompt.SetActive(true);
        }
        else
        {
            hitPrompt.SetActive(false);


        }
    }


    public void UpdateHappiness()
    {
        catHappinessSlider.value = BeatManager.instance.currentHappeiness;

        //scoreUI.text = BeatManager.instance.currentHappeiness.ToString();
    }

    public void PawHitAnimation()
    {
        pawIcon.SetTrigger("Hit");
    }


    public void PawMissAnimation()
    {
        pawIcon.SetTrigger("Miss");
    }


    public void ShowUI(GameObject uiToShow)
    {

        if (uiToShow.activeSelf != true)
        {
            uiToShow.SetActive(true);
        }
        else
        {
            Debug.Log(uiToShow.name + " is already active");
        }

    }

    public void HideUI(GameObject uiTohide)
    {
        if (uiTohide.activeSelf != false)
        {
            uiTohide.SetActive(false);

        }
        else
        {
            Debug.Log(uiTohide.name + " is already hidden");
        }


    }



    public void HideLevelPopup()
    {

        for (int i = 0; i < levelPopup.Count; i++)
        {
            levelPopup[i].SetActive(false);
        }
    }

    

    public void ShowLevelSucceedMSG()
    {
        StartCoroutine(LevelSucceedCoroutine());
    }

    public void ShowLevelFailMSG()
    {
        StartCoroutine(LevelFailCoroutine());
    }


    IEnumerator LevelSucceedCoroutine()
    {
        levelPopup.Clear();

        for (int i = 0; i < succeedSequence.Length; i++)
        {
            yield return new WaitForSeconds(waitTimePerImage);
            succeedSequence[i].SetActive(true);

            levelPopup.Add(succeedSequence[i]);
        }

        //yield return new WaitForSeconds(waitTimePerImage);
        GameManager.instance.UnlockInput(gameObject);

    }

   
    IEnumerator LevelFailCoroutine()
    {
        levelPopup.Clear();

        for (int i = 0; i < failedSequence.Length; i++)
        {
            yield return new WaitForSeconds(waitTimePerImage);
            failedSequence[i].SetActive(true);

            levelPopup.Add(failedSequence[i]);
        }

        //yield return new WaitForSeconds(waitTimePerImage);
        GameManager.instance.UnlockInput(gameObject);
    }


    public void StartTutorial()
    {
        StartCoroutine(ShowTutorialCoroutine());
    }

    IEnumerator ShowTutorialCoroutine()
    {


        for (int i = 0; i < tutorialInfo.Length; i++)
        {
            yield return new WaitForSeconds(waitTimePerImage);
            tutorialInfo[i].SetActive(true);

        }

        yield return new WaitForSeconds(tutorialAnimationWaitTime);
        GameManager.instance.UnlockInput(gameObject);


    }



    public void StartIntroducingCat()
    {
        catCurrentPosition = cat.transform.localPosition;

        //At the end of this coroutine,  it calls show contorl coroutine if player did not skip tutorial. 
        StartCoroutine(IntroduceCatCoroutine());

    }

    IEnumerator IntroduceCatCoroutine()
    {

        catAudioPlayer.Play();

        float t = 0;

        while (t < catWalkTime)
        {
            t += Time.deltaTime;

            cat.transform.localPosition = Vector3.Lerp(catCurrentPosition, catEndPosition.localPosition, t / catWalkTime);

            yield return null;
        }


        ShowUI(skipTutorialPopup);

        GameManager.instance.UnlockInput(gameObject);

    }


    public void SetHoldingOptionImage(HoldFillingImage holdOption)
    {

        currentHoldOption = fillImage[(int)holdOption];
    }

    public void SetHoldConfirmationFillAmount(float fillTime, float duration)
    {


        currentHoldOption.fillAmount = fillTime / duration;
    }

    IEnumerator PlayHoldSFXCoroutine()
    {
        while (isPlayingHoldingSFX)
        {
            catAudioPlayer.clip = holdSfx;
            catAudioPlayer.Play();
            yield return new WaitForSeconds(0.2f);
        }

    }




    public void PlayHoldSFX()
    {
        isPlayingHoldingSFX = true;
        StartCoroutine(PlayHoldSFXCoroutine());

    }

    public void StopHoldSFX()
    {
        isPlayingHoldingSFX = false;
        StopCoroutine(PlayHoldSFXCoroutine());
    }

}
