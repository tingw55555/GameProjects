using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MissionObjectiveController : MonoBehaviour
{

	[Header("Mission TIme")]
	private float currentMissionProgress = 0;
	private float missionTime = 300;
	private bool missionOvertime = false;


	/// ////////////////////


	[Header("---Reference---")]
	public GameObject playerPrefab;
	public GameObject spawnerPrefab;
	private GameObject playerClone;
	private Transform playerSpawn;
	[SerializeField] private List<EnemySpawner> spawners = new List<EnemySpawner>();
	[SerializeField] private KeyCode keyToProcceed;

	[Header("---Enemy Count---")]
	[SerializeField] private int maxNumEnemies = 0;
	public int currentDeadEnemies = 0;

	[Header("RunTime Value")]
	[SerializeField] private bool missionStarted = false;
	[SerializeField] private bool missionCompleted = false;

	public static event Action<GameObject> OnPlayerSpawned;
	public static event Action OnEnemiesCleared;
	public static event Action<float, float> OnEnemyCountUpdate;
	public static event Action OnMissionAbort;
	public static event Action OnMissionCompleteExit;


	private void Awake()
	{
		MissionSettingsController.OnNewMissionSettings += InitializeCombat;
	}

	private void OnEnable()
	{
		Enemy.OnUnitDie += OnEnemyDie;
	}

	private void OnDisable()
	{
		Enemy.OnUnitDie -= OnEnemyDie;
		MissionSettingsController.OnNewMissionSettings -= InitializeCombat;
	}


	private void Update()
	{
		CheckIfMissionEnd();
	}

	private void CheckIfMissionEnd()
	{
		if (missionStarted)
		{
			//If Player is alive
			if (playerClone != null)
			{
				if (missionCompleted)
				{
					MissionComplete_Exit();
				}
			}
			//If Player is dead
			else
			{
				MissionFail_Restart();
			}
		}
	}


	//Not Using it right now. There's no time limit in combat. 
	private void Handle_OverTime()
	{
		//If NOT overtime
		if (currentMissionProgress > 0)
		{
			if (!missionCompleted)
			{
				TimeProgresses();
			}
			else
			{
				MissionComplete_Exit();
			}

		}
		else
		{
			if (!missionOvertime)
			{
				missionOvertime = true;
				OnMissionAbort?.Invoke();
			}
		}
	}

	private void MissionComplete_Exit()
	{
		if (Input.GetKeyDown(keyToProcceed))
		{
			missionStarted = false;
			OnMissionCompleteExit?.Invoke();

		}

	}
	public void OnAbandonMission()
	{
		OnMissionAbort?.Invoke();
	}


	private void MissionFail_Restart()
	{
		if (Input.GetKeyDown(keyToProcceed))
		{
			SceneManager.LoadSceneAsync((int)GameScenes.Combat);
		}
	}



	private void TimeProgresses()
	{
		currentMissionProgress = Mathf.Clamp(currentMissionProgress - Time.deltaTime, 0, missionTime);
	}

	private void InitializeCombat(MapInfo mapInfo, LevelData enemyData)
	{
		missionCompleted = false;
		ResetNumbers();
		ResetSpawners();

		playerSpawn = mapInfo.playerSpawnPos;
		SpawnPlayer();

		if (enemyData != null)
		{
			CreateEnemySpawners(enemyData, mapInfo);
		}

		GetMaxEnemyAmount();
		OnPlayerSpawned?.Invoke(playerClone);
		missionStarted = true;


	}

	private void GetMaxEnemyAmount()
	{
		foreach (EnemySpawner s in spawners)
		{
			maxNumEnemies += s.GetNumberOfEnemiesToSpawn();
		}

		OnEnemyCountUpdate?.Invoke(currentDeadEnemies, maxNumEnemies);
	}

	private void ResetSpawners()
	{
		foreach (EnemySpawner s in spawners)
		{
			if (s != null)
			{
				Destroy(s.gameObject);
			}
		}

		spawners.Clear();
	}

	/// <summary>
	/// Create enemy spawners based on the number of enemy types included in the level data 
	/// (a mission is consisted of many level data). 
	/// Every spawner is responsible for spawning one type of enemy. 
	/// </summary>
	/// <param name="enemyData"></param>
	/// <param name="mapInfo"></param>
	private void CreateEnemySpawners(LevelData enemyData, MapInfo mapInfo)
	{
		for (int i = 0; i < enemyData.missionData.Length; i++)
		{
			EnemySpawner s = Instantiate(spawnerPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<EnemySpawner>();
			List<Transform> spawnList = FilterSpawnList(enemyData.missionData[i].type, mapInfo);

			s.InitializeEnemySpawner(enemyData.missionData[i], spawnList);

			spawners.Add(s);

		}

	}

	private List<Transform> FilterSpawnList(EnemyType type, MapInfo mapInfo)
	{
		List<Transform> spawnList = new List<Transform>();
		switch (type)
		{
			case EnemyType.Mech:
				spawnList = mapInfo.basicSpawnPos;
				break;
			case EnemyType.FireBug:
				spawnList = mapInfo.airSpawnPos;
				break;
			case EnemyType.OminTank:
				spawnList = mapInfo.hiddenSpawnPos;
				break;

			default:
				spawnList = mapInfo.basicSpawnPos;
				break;
		}

		return spawnList;
	}

	private void SpawnPlayer()
	{
		if (playerClone == null)
		{
			playerClone = Instantiate(playerPrefab, playerSpawn.position, Quaternion.identity);
		}
		else
		{
			playerClone.transform.position = playerSpawn.position;
		}


	}

	private void ResetNumbers()
	{
		maxNumEnemies = 0;
		currentDeadEnemies = 0;
		currentMissionProgress = missionTime;
	}

	private void OnEnemyDie(Transform enemy = null)
	{
		currentDeadEnemies++;

		OnEnemyCountUpdate?.Invoke(currentDeadEnemies, maxNumEnemies);

		if (currentDeadEnemies == maxNumEnemies)
		{
			missionCompleted = true;
			OnEnemiesCleared?.Invoke();
		}
	}





}
