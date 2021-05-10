using PathCreation;
using PathCreation.Examples;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyBase : Location
{



	public float timeBeforeStrike = 2.5f;

	public PathCreator premisePath;
	public PathCreator newOuterPath;
	public GameObject jetPrefab;

	private GameObject patrolingJet;
	public GameObject PatrollingJet { get { return patrolingJet; } }

	private PathFollower jetPathFollower;
	public PathFollower JetPathFollower { get { return jetPathFollower; } set { jetPathFollower = value; } }


	//This should be less than enemy path manager's new random path time
	public Vector2 delayPatrolTime = new Vector2(15f, 25f);

	protected override void Awake()
	{
		needCargo = false;
		base.Awake();
		locationType = LocationType.Nointeraction;

	}

	private void OnEnable()
	{
		GameManager.OnGameReady += AddEnemyToEnemyManager;

		if (jetPrefab != null && premisePath != null)
		{
			if (patrolingJet == null)
			{
				SpawnPatrolingFighterJet();
				HidePatrolWithDelay();
			}
		}
	}


	private void OnDisable()
	{
		GameManager.OnGameReady -= AddEnemyToEnemyManager;
	}

	private void AddEnemyToEnemyManager()
	{
		EnemyPathManager.instance.AddEnemy(this);
		//Debug.Log("Add");
	}

	private void SpawnPatrolingFighterJet(PathCreator newPath = null)
	{
		patrolingJet = Instantiate(jetPrefab, transform);
		patrolingJet.GetComponent<FighterJet>().SetUnitBase(this);

		jetPathFollower = patrolingJet.GetComponent<PathFollower>();

		if (newPath == null)
		{
			jetPathFollower.pathCreator = premisePath;
		}
		else
		{
			jetPathFollower.pathCreator = newPath;
		}

	}

	public void OnJetDestroyed(PathCreator recyclePath)
	{
		EnemyPathManager.instance.OnEnemyBaseRefreshed(this, recyclePath);
		StartCoroutine(WaitingToSpawn());
	}

	private IEnumerator WaitingToSpawn()
	{	
		
		yield return new WaitForSeconds(EnemyPathManager.instance.assignNewPathTime*2);

		SpawnPatrolingFighterJet();
	}




	public void SetNewPatrolPath(PathCreator newPath)
	{
		newOuterPath = newPath;

		if (patrolingJet != null)
		{
			HidePatrolWithDelay();
			jetPathFollower.pathCreator = newOuterPath;
		}
		else
		{
			SpawnPatrolingFighterJet(newOuterPath);
			HidePatrolWithDelay();
		}
	}


	public bool CheckIfCircling()
	{
		return patrolingJet !=null && jetPathFollower.pathCreator == premisePath;
	}


	private void HidePatrolWithDelay()
	{
		CancelInvoke("ShowPatrol");
		patrolingJet.SetActive(false);
		Invoke("ShowPatrol", Random.Range(delayPatrolTime.x, delayPatrolTime.y));
	}

	private void HidePatrol()
	{
		CancelInvoke("ShowPatrol");
		patrolingJet.SetActive(false);
		Invoke("ShowPatrol", 0);
	}

	private void ShowPatrol()
	{
		patrolingJet.SetActive(true);
	}

	private void OnTriggerEnter(Collider other)
	{
		TransportAirCraft ac = other.GetComponent<TransportAirCraft>();

		if (ac != null)
		{
			ac.ExposedToEnemies(timeBeforeStrike);
		}
	}


	protected override void OnTriggerExit(Collider other)
	{
		TransportAirCraft ac = other.GetComponent<TransportAirCraft>();

		if (ac != null)
		{
			ac.SafeFromEnemies();
		}
	}


	private void SetEnemyRandomPath(PathCreator newPC)
	{
		jetPathFollower.pathCreator = newPC;
	}


}
