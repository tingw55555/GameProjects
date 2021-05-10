using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
	Start = 0,
	Tutorial = 1,
	Gameplay = 2,
	Pause = 3,
	Defeat = 4,
	Win = 5,

}

public enum MapLevel
{
	Normal = 0,
	Hard = 1,
	Brutal = 2,

}


public class GameManager : Singleton<GameManager>
{
	public static event Action OnGameReady;

	public GameState currentState;
	public MapLevel map;
	private bool enemyPathChange = false;
	
	[SerializeField]
	private GameState previousState;

	[SerializeField]
	private FriendlyPathCreator pc;

	[Space(10)]
	[SerializeField]
	private int currentCivLocations;
	[SerializeField]
	private int totalCivLocations;

	[SerializeField]
	private float currentPower = 0.35f;
	public float addedPowerPerAmount = 0.005f;

	[Space(10)]
	public List<GameObject> maps = new List<GameObject>();

	[Space(10)]
	[SerializeField]
	private CamControls camContorl;
	[SerializeField]
	private GameObject tutorialMap;
	public TutorialManager tM;

	private void Start()
	{
		SetState(currentState);
		ReliefHotSpot.OnCivRegister += AddCiv;
		ReliefHotSpot.OnReachingPeaceTime += OnAddingToNationalPower;
		Missile.OnEnemyJetDestroyed += OnAddingToNationalPower;
	}

	private void SetState(GameState newState)
	{
		if (currentState != newState)
		{
			currentState = newState;
		}

		AudioManager.instance.PlayMusic();

		switch (newState)
		{
			case GameState.Start:
				//Time.timeScale = 0;
				currentPower = 0.35f;
				PathingStatus(false);

				foreach (GameObject map in maps)
				{
					map.SetActive(false);
				}

				tutorialMap.SetActive(false);

				enemyPathChange = false;

				break;

			case GameState.Tutorial:
				PathingStatus(true);
				UIManager.instance.ShowScreen(UIType.S_Tutorial);
				break;

			case GameState.Gameplay:
				PathingStatus(true);
				UIManager.instance.ShowScreen(UIType.S_GamePlay);

				

				break;

			case GameState.Pause:
				Time.timeScale = 0;
				PathingStatus(false);
				break;

			case GameState.Defeat:
				Time.timeScale = 0;
				PathingStatus(false);

				break;

			case GameState.Win:
				UIManager.instance.ShowScreen(UIType.S_Victory);
				Time.timeScale = 0;
				break;



		}

	}

	private void Update()
	{
		//Debug.Log("update");


		if (Input.GetKeyDown(KeyCode.Escape))
		{
			

			if (currentState == GameState.Gameplay)
			{

				previousState = currentState;
				SetState(GameState.Pause);
				UIManager.instance.HandleUIEvents(UIEvents.GamePause);
			}
			else if (currentState == GameState.Tutorial)
			{
				previousState = currentState;
				SetState(GameState.Pause);
				UIManager.instance.HandleUIEvents(UIEvents.TutorialPause);
			}
			else if (currentState == GameState.Pause)
			{
				Time.timeScale = 1;

				if (previousState == GameState.Gameplay)
				{
					UIManager.instance.HandleUIEvents(UIEvents.GamePause);
				}
				else
				{
					UIManager.instance.HandleUIEvents(UIEvents.TutorialPause);
				}

				SetState(previousState);

			}
		}

	}


	public void OnTutorialStart()
	{
		tutorialMap.SetActive(true);
		SetState(GameState.Tutorial);
		tM.gameObject.SetActive(true);

		OnGameReady?.Invoke();
		InitializeGame();
	}

	public void OnGameStart(int mapIndex)
	{
		map = (MapLevel)mapIndex;
		maps[mapIndex].SetActive(true);
		SetState(GameState.Gameplay);


		//This make sure everything needs to be initialized does it in the right time. 
		//Instead of start or awake because there's a tutorial
		//(1) Relief HotSpot add to the total number before the UI renders it. 
		//(2) Production Spots start producing at the right time
		//(3) Spawn the ACs first and then register it on Hotkey
		OnGameReady?.Invoke();
		InitializeGame();

		//EnemyPathManager.instance.SetNewEnemyPath();
	}

	public void InitializeGame()
	{
		Time.timeScale = 1;

		if (UIManager.instance != null)
		{
			UIManager.instance.SetNationPower(currentPower);

			//I'm setting the Defected City Limit here. Which is written in this way : current Defected City Count / half of the total civ count
			UIManager.instance.SetDefectedCityCount(totalCivLocations - currentCivLocations, Mathf.FloorToInt(((float)totalCivLocations / 2.0f)));

		}

		camContorl.enabled = true;

	}

	public void PathingStatus(bool active)
	{
		pc.enabled = active;
	}

	public void OnAddingToNationalPower(int addedAmount)
	{
		currentPower += addedAmount * addedPowerPerAmount;

		if (GameManager.instance.currentState == GameState.Gameplay)
		{
			UIManager.instance.UpdateNationPower(addedAmount * addedPowerPerAmount, true);
		}
		else if (GameManager.instance.currentState == GameState.Tutorial)
		{
			UIManager.instance.ShowTutorialCVParent();
		}


		if (currentPower >= 1)
		{
			SetState(GameState.Win);

			return;

		}



	}

	public void OnLose(LoseCondition condition)
	{
		SetState(GameState.Defeat);
		UIManager.instance.OnDefeat(condition);
	}

	private void AddCiv()
	{
		currentCivLocations++;
		totalCivLocations = currentCivLocations;
	}

	public void OnBuildingFall()
	{
		currentCivLocations--;

		UIManager.instance.SetDefectedCityCount(totalCivLocations - currentCivLocations, Mathf.FloorToInt((float)totalCivLocations / 2.0f));

		if ((float)currentCivLocations < Mathf.FloorToInt((float)totalCivLocations / 2.0f))
		{

			if (currentState == GameState.Gameplay)
			{
				OnLose(LoseCondition.LostTerritory);

			}
			else if (currentState == GameState.Tutorial)
			{
				OnLose(LoseCondition.FailTutorial);
			}
		}

	}



	public void ChangeEnemyJetPaths()
	{
		if (GameManager.instance.currentState == GameState.Tutorial)
		{
			return;
		}

		EnemyPathManager.instance.AssignNewPathPeriodically();
		enemyPathChange = true;
	}


}

public enum LoseCondition
{

	CapitalFall = 0,
	LostTerritory = 1,
	NoMoreAC = 2,
	FailTutorial = 3,


}



