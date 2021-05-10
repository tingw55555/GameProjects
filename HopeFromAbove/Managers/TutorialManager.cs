using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using UnityEngine;


public enum TutorialTask
{
	Welcome = 0,
	CamControl = 1,
	SelectUnit = 2,
	DrawPath = 3,
	ReliefCenterLanding = 4,
	CivilianLanding = 5,
	ShowLowResource = 6,
	DeliverMineToRefinery = 7,
	ControlSecondUnit = 8,
	MaterialToFactory = 9,
	LaunchMissiles = 10,
	Capital = 11,
}

public class TutorialManager : Singleton<TutorialManager>
{
	public List<ReliefHotSpot> civilianBuildings = new List<ReliefHotSpot>();
	public List<ProductionSpot> productionSpots = new List<ProductionSpot>();
	public MissileCommand missileCommand;
	public List<EnemyBase> enemies = new List<EnemyBase>();

	[Header("Current Task")]
	public TutorialTask currentTask;
	public bool completed;
	public float finalCompleteTime = 8f;
	public float defaultDelayTime = 2f;

	[Header("UI Reference")]
	public GameObject UI_WelcomeMSG;
	public GameObject UI_Contiune;

	public GameObject UI_winCondition;
	public GameObject UI_LoseConditionPanel;
	public GameObject UI_Lose_Aircraft;
	public GameObject UI_Lose_Capital;

	[Space(10)]
	public GameObject UI_CamControl;
	public GameObject UI_SelectUnit;
	public GameObject UI_DrawPath;
	public GameObject UI_ReliefCenterLanding;
	public GameObject UI_CivilianLanding;
	public GameObject UI_LowResAndWarning;
	public GameObject UI_MineToRefinery;
	public GameObject UI_ControlUnits;
	public GameObject UI_MaterialToFactory;
	public GameObject UI_LaunchMissile;
	public GameObject UI_Capital;
	public GameObject UI_Complete;


	private bool hasDrawnPath;
	public bool HasDrawnPath { get { return hasDrawnPath; } set { hasDrawnPath = value; } }

	
	private bool hasLanded;
	public bool HasLanded { get { return hasLanded; } set { hasLanded = value; } }

	[SerializeField]
	private int zonesLanded;
	public int ZonesLanded { get { return zonesLanded; } set { zonesLanded = value; } }

	private bool doubleTapped;
	public bool DoubleTapped { get { return doubleTapped; } set { doubleTapped = value; } }


	private bool unitSpawned;
	public bool UnitSpawned { get { return unitSpawned; } set { unitSpawned = value; } }


	private bool missileDelivered;
	public bool MissileDelivered { get { return missileDelivered; } set { missileDelivered = value; } }


	private bool missileLaunched;
	public bool MissileLaunched { get { return missileLaunched; } set { missileLaunched = value; } }

	private void Awake()
	{
		completed = false;
	}

	// Update is called once per frame
	void Update()
	{

		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (completed)
			{
				SetTask((TutorialTask)((int)currentTask + 1));
			}

		}

		if (currentTask == TutorialTask.CamControl)
		{
			if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || 
				Input.GetKeyDown(KeyCode.UpArrow)|| Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
			{
				ToggleContiune(true);
			}
		}

		if (currentTask == TutorialTask.SelectUnit)
		{
			if (Input.GetKeyDown(KeyCode.Alpha1)) ToggleContiune(true);
		}

		if (currentTask == TutorialTask.DrawPath)
		{
			//Called In PathCreator
			if (hasDrawnPath)
			{
				DelayContiune();
				hasDrawnPath = false;
			}

		}

		if (currentTask == TutorialTask.ReliefCenterLanding)
		{
			if (hasLanded)
			{
				ToggleContiune(true);
				hasLanded = false;

			}

		}

		if (currentTask == TutorialTask.CivilianLanding)
		{
			if (hasLanded)
			{
				DelayContiune();
				hasLanded = false;

			}
		}

		if (currentTask == TutorialTask.ShowLowResource)
		{
			if (zonesLanded >= 2)
			{
				ToggleContiune(true);
				zonesLanded = 0;
			}
		}

		if (currentTask == TutorialTask.DeliverMineToRefinery)
		{
			if (unitSpawned)
			{
				ToggleContiune(true);
				unitSpawned = false;
			}
		}

		if (currentTask == TutorialTask.ControlSecondUnit)
		{
			if (doubleTapped)
			{
				ToggleContiune(true);
				doubleTapped = false;
			}
		}

		if (currentTask == TutorialTask.MaterialToFactory)
		{
			if (missileDelivered)
			{
				ToggleContiune(true);
				missileDelivered = false;
			}
		}

		if (currentTask == TutorialTask.LaunchMissiles)
		{
			if (missileLaunched)
			{
				ToggleContiune(true);
				missileLaunched = false;
			}
		}

	}


	private void SetTask(TutorialTask newTask)
	{
		ToggleContiune(false);

		currentTask = newTask;

		switch (newTask)
		{
			case TutorialTask.CamControl:
				{
					//Hide the Previous UIs
					UI_WelcomeMSG.SetActive(false);

					//Show the Current Panel
					UI_CamControl.SetActive(true);
				}
				break;

			case TutorialTask.SelectUnit:
				{

					//Hide the Previous UIs
					UI_CamControl.SetActive(false);

					//Show the Current Panel
					UI_SelectUnit.SetActive(true);

				}
				break;

			case TutorialTask.DrawPath:
				{
					//Hide the Previous UIs
					UI_SelectUnit.SetActive(false);

					//Show the Current Panel
					UI_DrawPath.SetActive(true);
				}
				break;

			case TutorialTask.ReliefCenterLanding:
				{
					UI_DrawPath.SetActive(false);

					//First Relief Center
					productionSpots[0].gameObject.SetActive(true);
					productionSpots[0].ProductionInit();

					UI_ReliefCenterLanding.SetActive(true);
				}
				break;

			case TutorialTask.CivilianLanding:
				{
					UI_ReliefCenterLanding.SetActive(false);

					//First Civilian building
					civilianBuildings[0].gameObject.SetActive(true);
					civilianBuildings[0].CivilianInit();

					UI_winCondition.SetActive(true);
					UI_CivilianLanding.SetActive(true);
				}
				break;

			case TutorialTask.ShowLowResource:
				{
					UI_CivilianLanding.SetActive(false);

					Invoke("ShowInitialLoseCondition", defaultDelayTime);

					Invoke("ShowLowResCities", defaultDelayTime * 3);

					UI_LowResAndWarning.SetActive(true);

				}
				break;

			case TutorialTask.DeliverMineToRefinery:
				{
					UI_LowResAndWarning.SetActive(false);

					Invoke("ShowProductionSpots", defaultDelayTime * 2);

					UI_MineToRefinery.SetActive(true);
				}
				break;

			case TutorialTask.ControlSecondUnit:
				{
					UI_MineToRefinery.SetActive(false);

					UI_Lose_Aircraft.SetActive(true);

					UI_ControlUnits.SetActive(true);
				}
				break;

			case TutorialTask.MaterialToFactory:
				{
					UI_ControlUnits.SetActive(false);

					enemies[0].gameObject.SetActive(true);
					enemies[1].gameObject.SetActive(true);


					Invoke("ShowMissileBuildings", defaultDelayTime * 3);

					UI_MaterialToFactory.SetActive(true);



				}
				break;


			case TutorialTask.LaunchMissiles:
				{

					UI_MaterialToFactory.SetActive(false);

					UI_LaunchMissile.SetActive(true);				

				}
				break;


			case TutorialTask.Capital:
				{
					UI_LaunchMissile.SetActive(false);

					civilianBuildings[3].gameObject.SetActive(true);
					civilianBuildings[3].CivilianInit();

					UI_Capital.SetActive(true);
					UI_Lose_Capital.SetActive(true);
					Invoke("ShowTrainingComplete", finalCompleteTime);
				}
				break;

		}
	}




	private void OnEnable()
	{
		if (GameManager.instance.currentState == GameState.Tutorial)
		{
			UIManager.instance.ShowScreen(UIType.S_Tutorial);

			WelcomePlayer();
		}
	}

	private void WelcomePlayer()
	{
		completed = false;
		UI_WelcomeMSG.SetActive(true);

		//based on Welcome Animation
		Invoke("ShowContiune", 3f);

	}


	private void ToggleContiune(bool isActive)
	{
		UI_Contiune.SetActive(isActive);
		completed = isActive;
	}

	//So I can call this with Invoke
	private void ShowContiune()
	{
		UI_Contiune.SetActive(true);
		completed = true;
	}


	private void DelayContiune(float extraTime = 0)
	{
		if (extraTime != 0)
		{
			Invoke("ShowContiune", extraTime);
		}
		else
		{
			Invoke("ShowContiune", defaultDelayTime);
		}
	}



	private void ShowMissileBuildings()
	{
		productionSpots[3].gameObject.SetActive(true);
		productionSpots[3].ProductionInit();

		missileCommand.gameObject.SetActive(true);

	}


	private void ShowLowResCities()
	{
		//Struggling Civilian buildins
		civilianBuildings[1].gameObject.SetActive(true);
		civilianBuildings[1].CivilianInit();

		civilianBuildings[2].gameObject.SetActive(true);
		civilianBuildings[2].CivilianInit();
	}


	private void ShowProductionSpots()
	{
		productionSpots[1].gameObject.SetActive(true);
		productionSpots[1].ProductionInit();

		productionSpots[2].gameObject.SetActive(true);
		productionSpots[2].ProductionInit();
	}


	private void ShowInitialLoseCondition()
	{
		UI_winCondition.SetActive(false);
		UI_LoseConditionPanel.SetActive(true);
	}

	private void ShowTrainingComplete()
	{
		UI_Capital.SetActive(false);
		UI_Complete.SetActive(true);
	}

	private void SetInitialBuildingsActive()
	{
		civilianBuildings[0].gameObject.SetActive(true);

	}
}
