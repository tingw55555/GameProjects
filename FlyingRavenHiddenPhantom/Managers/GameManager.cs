using System.Collections;
using System.Collections.Generic;

using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
	Start,
	Level,
	Combat,
	Lose,
}

public class GameManager : Singleton<GameManager>
{
	[SerializeField]
	private GameState currentState;
	[SerializeField]
	private Vector2Int currentCoordinate;

	public int currentTurn = 0;
	public int currentWave = 0;

	[Header("In milloSeconds")]
	public float bannerWaitTime = 1f;
	public float lastAttackWaitTime = 0.5f;

	public bool playerTurn = true;




	//Events


	public GameState CurrentState { get { return currentState; } }
	public int currentColumn { get { return currentCoordinate.x; } }
	public int currentNode { get { return currentCoordinate.y; } }



	void Start()
	{
		SetState(CurrentState);
	}

	private void OnEnable()
	{
		InputManager.Input_OnPausePressed += OnPausePressed;
		InputManager.Input_CreateNewLevel += OnCreateNewLevel;

		EncounterNode.hoveringNode += OnHoverObjectsWithToolTip;
	}

	private void OnDisable()
	{
		InputManager.Input_OnPausePressed -= OnPausePressed;
		InputManager.Input_CreateNewLevel -= OnCreateNewLevel;

		EncounterNode.hoveringNode -= OnHoverObjectsWithToolTip;
	}



	public void SetState(GameState newState)
	{
		currentState = newState;

		switch (newState)
		{
			case GameState.Start:
				//AudioManager.instance.SetMusic(BGMusicTypes.Menu);
				//AudioManager2.instance.MainMenu();

				//SetCurrentProgression(Vector2Int.zero);

				//EncounterManager.instance.CreateTree();
				//EncounterManager.instance.ToggleEncounterTree();
				break;

			case GameState.Combat:

				//AudioManager2.instance.Combat();

				playerTurn = false;

				//EncounterManager.instance.HideEncounterTree();

				UIManager.instance.ShowScreen(UIType.S_Combat);

				CreateGrid();

				UnitManager.instance.SpawnUnits();
				SpawnManager.instance.InitialSpawn();

				OnStartANewTurn();


				break;


			case GameState.Lose:
				SetPlayerControl(false);
				//AudioManager.instance.SetMusic(BGMusicTypes.Level);

				//AudioManager2.instance.Defeat();

				UIManager.instance.ShowScreen(UIType.P_Defeat);

				break;



			case GameState.Level:
				break;

		}
	}

	public void OnPausePressed()
	{
		UIManager.instance.HandleUIEvents(UIEvents.Pause);
	}

	public void OnHoverObjectsWithToolTip(ToolTipData dataToShow)
	{
		if (currentState == GameState.Combat || currentState == GameState.Level)
		{
			UIManager.instance.ShowToolTipContent(dataToShow);
		}
	}

	public void OnExitObjectsWithToolTip()
	{
		UIManager.instance.ClearToolTipContent();
	}

	public void OnCreateNewLevel()
	{
		if (currentState == GameState.Level)
		{
			currentCoordinate = Vector2Int.zero;
			//EncounterManager.instance.CreateTree();
		}
	}

	public void OnNewGameButton()
	{
		UIManager.instance.ShowScreen(UIType.S_CharacterSelect);
	}


	public void OnEnterCombat()
	{
		SetState(GameState.Combat);

	}

	public void OnBackToMenu()
	{
		SceneManager.LoadSceneAsync("Main");
	}

	public void OnPlayerTurnFinished()
	{
		//Some attacks have two paths 
		//and they will each call combat manager when they finished attacking
		//Which leads to calling this function twice
		if (!playerTurn)
		{
			return;
		}

		OnResolveCurrentTurn();

	}



	public void OnResolveCurrentTurn()
	{
		
		StartCoroutine(ResolvingTurnCoroutine());
	}

	IEnumerator ResolvingTurnCoroutine()
	{
		playerTurn = false;
		
		//Wait for the last character to finish its attack
		yield return new WaitForSeconds(lastAttackWaitTime);

		UnitManager.instance.HideSelectedCharacterDestination();
		
		UIManager.instance.SetEndTurnButton(false);


		UIManager.instance.SetCombatTurnText();

		UIManager.instance.ShowBanner(false);

		//Wait for the last character to finish its attack
		yield return new WaitForSeconds(bannerWaitTime);

		UnitManager.instance.EnemyAttack();


	}


	public void AddTurn()
	{
		currentTurn++;
	}


	public void OnStartANewTurn()
	{
		AddTurn();
		StartCoroutine(NewTurnCoroutine());
	}


	IEnumerator NewTurnCoroutine()
	{

		if (currentTurn != 1) yield return new WaitForSeconds(lastAttackWaitTime);

		if (currentTurn != 1) UIManager.instance.ShowBanner(true);

		UIManager.instance.SetCombatTurnText();
		UIManager.instance.UpdateTurnNumberText(currentTurn);

		//AudioManager.instance.SetSFX(SFX.TurnStart);
		AudioManager2.instance.PlayUI(AudioManager2.instance.EVENT_TurnStart);

		UnitManager.instance.ShowEnemyAttackPaths();

		if (currentTurn != 1) yield return new WaitForSeconds(bannerWaitTime);

		SpawnManager.instance.TurnResolved();

		playerTurn = true;
		UIManager.instance.SetCombatTurnText();
		UIManager.instance.SetEndTurnButton(true);
		UnitManager.instance.ResetMoves();

	}


	public void OnNodePressed(EncounterType encounter, Vector2Int nodeCoord)
	{
		Debug.Log("Node is Pressed");
	}

	public void CreateGrid()
	{
		GridManager.instance.Initialize_Grid();
	}

	public void OnAllCharactersDead()
	{
		SetState(GameState.Lose);
	}



	public void OnActiveNodePress(EncounterType encounter)
	{
		switch (encounter)
		{
			case EncounterType.Combat:
				{
					OnEnterCombat();
				}
				break;

			case EncounterType.UnknownEvent:
				{

				}
				break;

			case EncounterType.Merchant:
				{

				}
				break;

		}
	}


	public void SetPlayerControl(bool hasControl)
	{
		playerTurn = hasControl;
	}


	//public void OnExitCombat()
	//{
	//	if (!disableUI)
	//	{
	//		SetState(GameState.Level);

	//		GridManager.instance.DestroyGrid();

	//		EncounterManager.instance.ToggleEncounterTree();
	//		EncounterManager.instance.Deactivate(currentColumn);
	//		AddColumn();

	//		UIManager.instance.SetCoordinates(UIType.S_Level, currentCoordinate);
	//		UIManager.instance.ShowScreen(UIType.S_Level);
	//	}

	//}

	//public void AddColumn()
	//{
	//	currentCoordinate.x++;
	//}

	//public void SetCurrentProgression(Vector2Int coord)
	//{
	//	currentCoordinate = coord;
	//}


}
