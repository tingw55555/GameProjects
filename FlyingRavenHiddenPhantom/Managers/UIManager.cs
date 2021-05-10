using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public enum UIType
{
	S_MainMenu = 0,
	S_CharacterSelect = 1,
	S_Combat = 2,
	S_Level = 3,

	P_Options = 4,
	P_Defeat = 5,
	O_Tooltip = 6,
	O_TurnBanner = 7,

}

public enum UIEvents
{
	Pause = 0,

}

public class UIManager : Singleton<UIManager>
{


	public Dictionary<UIType, UIElement> uiElements = new Dictionary<UIType, UIElement>();
	private UIElement currentScreen;


	[Header("Display Settings")]
	public string playerTurnText;
	public string enemyTurnText;
	public float bannerShowTime = 1f; 


	[Header("Temporary Settings")]
	public AudioClip[] clips;
	public AudioSource audiSource;

	[Header("Canvas UI Reference")]
	public UIElement openingScreen;
	public TextMeshProUGUI combatTurn;
	public TextMeshProUGUI numOfTurnText;

	public TextMeshProUGUI levelCoordText;


	[Space(10)]
	public TextMeshProUGUI tt_Title;
	public TextMeshProUGUI tt_Oneliner;
	public Image tt_ThumbNail;


	private void Awake()
	{
		InitializeUI();
	}

	private void InitializeUI()
	{

		//This returns all child objects with a UI script component
		UIElement[] allScreens = GetComponentsInChildren<UIElement>(true);

		foreach (UIElement ui in allScreens)
		{
			//Enable all elements first if there are a lot of UI initialization later on
			//ui.ShowElement();

			ui.CloseElement();

			//We add the each UI elements to the dictionary
			uiElements.Add(ui.typeOfUI, ui);
		}

		currentScreen = openingScreen;
		currentScreen.ShowElement();
	}



	public void RenderElements(UIElement elementToSHow)
	{
		//If is Screen
		if (!elementToSHow.renderOnTop)
		{
			if (elementToSHow.showToolTip) { ShowToolTipElement(); }
			else { HideToolTipElement(); }

			//Close the previous Screen
			if (currentScreen != null)
			{
				currentScreen.CloseElement();
			}

			//Change current screen to a new one
			currentScreen = elementToSHow;
		}

		elementToSHow.ShowElement();

	}

	public void ShowToolTipElement()
	{
		uiElements[UIType.O_Tooltip].ShowElement();
	}

	public void HideToolTipElement()
	{
		uiElements[UIType.O_Tooltip].CloseElement();
	}



	public void ShowScreen(UIType uiType)
	{
		RenderElements(uiElements[uiType]);
	}


	public void HandleUIEvents(UIEvents newEvent)
	{
		switch (newEvent)
		{
			case UIEvents.Pause:
				{
					//Show Option if the Main Menu is closed 
					if (!uiElements[UIType.S_MainMenu].IsShowing())
					{
						uiElements[UIType.P_Options].ToggleUI();
					}

				}
				break;

		}
	}


	public void OnEndTurn()
	{
		AudioManager2.instance.Play_BTN_EndTurn();
		GameManager.instance.OnResolveCurrentTurn();
	}

	public void SetEndTurnButton(bool isInteractable)
	{
		uiElements[UIType.S_Combat].GetComponent<UScreen_Combat>().SetEndTurnButtonState(isInteractable);
	}

	public void OnBackToMainMenu()
	{
		uiElements[UIType.P_Options].ToggleUI();
		GameManager.instance.OnBackToMenu();
	}

	public void SetCombatTurnText()
	{
		if (GameManager.instance.playerTurn)
		{
			combatTurn.text = playerTurnText;

		}
		else
		{
			combatTurn.text = enemyTurnText;
		}

		

	}

	public void UpdateTurnNumberText(int currentTurn)
	{
		numOfTurnText.text = "Turn: " + currentTurn;
	}


	public void OnStartLevelButton()
	{
		//PlayButtonSound();
		AudioManager2.instance.Play_BTN_BeginPlay();
		GameManager.instance.OnEnterCombat();
	}


	public void PlayButtonSound()
	{
		audiSource.Play();
	}

	public void ShowToolTipContent(ToolTipData dataToShow)
	{
		tt_Title.text = dataToShow.title;
		tt_Oneliner.text = dataToShow.oneLinerDescription;
		tt_ThumbNail.sprite = dataToShow.icon;
	}

	public void ClearToolTipContent()
	{
		tt_Title.text = null;
		tt_Oneliner.text = null;
		tt_ThumbNail = null;
	}


	public void ShowBanner(bool isPlayerTurn)
	{
		uiElements[UIType.O_TurnBanner].GetComponent<TurnBanner>().SetBanner(isPlayerTurn);
		uiElements[UIType.S_Combat].GetComponent<UScreen_Combat>().SetEndTurnButtonState(false);
		ShowScreen(UIType.O_TurnBanner);

		Invoke("HideBanner", bannerShowTime);
	}

	public void HideBanner()
	{
		uiElements[UIType.O_TurnBanner].CloseElement();

		if (GameManager.instance.playerTurn)
		{
			uiElements[UIType.S_Combat].GetComponent<UScreen_Combat>().SetEndTurnButtonState(true);
		}

	}



}
