using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject bossInfo;
    public GameObject playerInfo;
    public GameObject title;

    public Image respawnWaitBar;

    public GameObject winMessage;
    public GameObject defeatedMessage;

    public Image playerHealthBar;
    public Image bossHealthBar;

    public Jet playerRef;
    public Boss bossRef;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        //These functions just update the visual but do not have gameplay implications, as UI should be. 
        UpdatePlayerHealthBar();

        UpdateBossHealthBar();

    }

    //This is called by Game Manager when entering Gameplay state 
    public void ShowHpBars()
    {
        bossInfo.SetActive(true);
        playerInfo.SetActive(true);
    }

    public void UpdatePlayerHealthBar()
    {
        playerHealthBar.fillAmount = playerRef.currentHealth / playerRef.maxHealth;
    }

    public void UpdateBossHealthBar()
    {
        bossHealthBar.fillAmount = bossRef.currentHealth / bossRef.maxHealth;
    }

    public void ShowTitle()
    {
        title.SetActive(true);
    }

    public void HideTitle()
    {
        title.SetActive(false);
    }

    //This is called before ShowRestartFill is called so it always resets the fill amount 
    public void ShowDefeatMessage()
    {
        defeatedMessage.SetActive(true);
        respawnWaitBar.fillAmount = 0;
        
    }


    public void ShowRestartFill(float currentTime, float duration)
    {
        respawnWaitBar.fillAmount = currentTime / duration;
    }

    public void HideDefeatMessage()
    {
        defeatedMessage.SetActive(false);
    }


    public void ShowWinMessage()
    {
        winMessage.SetActive(true);
    }
}
