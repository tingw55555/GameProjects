using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public Text playerScoreText;

    public GameObject playerScoreUI;
    public GameObject titleText;
    public GameObject optionPopUp;



    private void Awake()
    {
        instance = this;

        //HideAllUI();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlayerScoreChange(float score)
    {
        playerScoreText.text = score.ToString();
    }

    public void ShowUI(GameObject uiToShow)
    {
        uiToShow.SetActive(true);
    }


    public void HideUI(GameObject uiToHide)
    {
        uiToHide.SetActive(false);

    }

    public void HideAllUI()
    {
        playerScoreUI.SetActive(false);
        titleText.SetActive(false);
        optionPopUp.SetActive(false);
    }


    public void ToggleUI(GameObject uiToToggle)
    {
        uiToToggle.SetActive(!uiToToggle.activeSelf);
    }



    public bool GetUIStatus(GameObject uiToCheck)
    {
        return uiToCheck.activeSelf;
    }
}
