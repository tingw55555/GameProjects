using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using Chronos;

public class EnemySpawner : MonoBehaviour
{
	private Dictionary<EnemyType, Enemy> enemyDictionary = new Dictionary<EnemyType, Enemy>();
	private Transform recordedSpawn;

	[Header("References")]
	public Enemy enemyToSpawn;
	public Transform playerTransform;
	public GameObject spawnParticles;

	[Header("Combat Info")]
	public List<Transform> spawnPoints = new List<Transform>();
	public List<Enemy> enemiesInLevel = new List<Enemy>();

	[Header("Spawner Settings")]
	public float initialWaitTime = 1.5f;
	public int numToSpawn = 3;
	public float spawnWaitTime = 1.5f;
	public float spawnEffectTime = 0.5f;


	private void Awake()
	{
		InitializeDictionary();

		//Randomly select the next spawn location. 
		if (spawnPoints.Count > 1)
		{
			recordedSpawn = GetRandomTransform(spawnPoints);
		}
	}

	private void OnEnable()
	{
		MissionObjectiveController.OnPlayerSpawned += MissionStart;
	}

	private void OnDisable()
	{
		MissionObjectiveController.OnPlayerSpawned -= MissionStart;
	}


	private void InitializeDictionary()
	{
		Enemy[] enemies = Resources.LoadAll<Enemy>("Enemies");

		for (int i = 0; i < enemies.Length; i++)
		{
			if (!enemyDictionary.ContainsKey(enemies[i].enemyType))
			{
				enemyDictionary.Add(enemies[i].enemyType, enemies[i]);
			}
		}
	}

	/// <summary>
	/// Called by Mission Objective Controller. 
	/// (1) Grab the enemy type to spawn. (2) How many to spawn. 
	/// (3) Spawn delay for each enemy. (4) Grab the possible spawn locations.
	/// </summary>
	/// <param name="info"></param>
	/// <param name="spawnPos"></param>
	public void InitializeEnemySpawner(SpawnInfo info, List<Transform> spawnPos)
	{
		enemyToSpawn = enemyDictionary[info.type];
		numToSpawn = info.numberPerWave;
		spawnWaitTime = info.spawnWaitTime;

		spawnPoints.AddRange(spawnPos);

	}

	public int GetNumberOfEnemiesToSpawn()
	{
		return numToSpawn;
	}


	/// <summary>
	/// On Mission start: (1) Get player transfrom, (2) Clear enemy list, (3) start spawning
	/// </summary>
	/// <param name="player"></param>
	private void MissionStart(GameObject player)
	{
		playerTransform = player.transform;

		enemiesInLevel.Clear();

		StartCoroutine(WaveSpawn());
	}


	/// <summary>
	/// Spawn enemies with delay & particle effects 
	/// </summary>
	/// <returns></returns>
	IEnumerator WaveSpawn()
	{
		yield return new WaitForSeconds(initialWaitTime);

		while (enemiesInLevel.Count < numToSpawn && playerTransform != null)
		{
			yield return new WaitForSeconds(spawnWaitTime);

			SelectNewSpawnPoint();
			CreateSpawnEffect();

			yield return new WaitForSeconds(spawnEffectTime);

			SpawnEnemy();

		}
	}

	/// <summary>
	/// Select a new spawn point excluding the currently selected spawn point 
	/// </summary>
	private void SelectNewSpawnPoint()
	{
		if (spawnPoints.Count > 1)
		{
			Transform newSpawn = GetRandomTransform(spawnPoints, recordedSpawn);
			recordedSpawn = newSpawn;
		}
		else
		{
			recordedSpawn = spawnPoints[0];
		}
	}


	private void CreateSpawnEffect()
	{
		if (spawnParticles != null)
		{
			SimpleParticles sp = GameObjectUtil.Instantiate(spawnParticles, recordedSpawn.position, Quaternion.identity).GetComponent<SimpleParticles>();
			sp.SetInitialYOffset();
		}
	}

	private void SpawnEnemy()
	{
		Enemy testEnemy = Instantiate(enemyToSpawn, recordedSpawn.position, recordedSpawn.rotation, transform);
		enemiesInLevel.Add(testEnemy);

		testEnemy.GetPlayerTransform(playerTransform);

	}


	private Transform GetRandomTransform(List<Transform> listToSearch)
	{
		return listToSearch[Random.Range(0, listToSearch.Count)];

	}

	/// <summary>
	/// Get random transform from a list while exclude a specific transform
	/// </summary>
	/// <param name="listToSearch"></param>
	/// <param name="transformToIgore"></param>
	/// <returns></returns>
	private Transform GetRandomTransform(List<Transform> listToSearch, Transform transformToIgore)
	{
		List<Transform> tempSpawnList = new List<Transform>(listToSearch);

		//Remove the old spawn
		tempSpawnList.Remove(transformToIgore);

		return tempSpawnList[Random.Range(0, tempSpawnList.Count)];

	}


}
