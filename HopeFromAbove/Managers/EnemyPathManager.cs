using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using PathCreation;
using PathCreation.Examples;


public class EnemyPathManager : Singleton<EnemyPathManager>
{

	public List<PathCreator> normalAssetPaths = new List<PathCreator>();
	public List<PathCreator> hardAssetPaths = new List<PathCreator>();
	public List<PathCreator> brutalAssetPaths = new List<PathCreator>();

	public List<PathCreator> normalPaths;
	public List<PathCreator> hardPaths;
	public List<PathCreator> brutalPaths;

	public List<EnemyBase> enemyBase = new List<EnemyBase>();

	public float assignNewPathTime = 25f;


	private void Awake()
	{
		GameManager.OnGameReady += ClearList;

		normalPaths = new List<PathCreator>(normalAssetPaths);
		hardPaths = new List<PathCreator>(hardAssetPaths);
		brutalPaths = new List<PathCreator>(brutalAssetPaths);

		//Debug.Log("Clear list");
	}


	public void AddEnemy(EnemyBase newEnemy)
	{
		enemyBase.Add(newEnemy);
	}

	public void OnEnemyBaseRefreshed(EnemyBase baseThatLost, PathCreator recycledPath)
	{
		foreach (EnemyBase b in enemyBase)
		{
			if (b == baseThatLost)
			{
				return;
			}
		}


		AddEnemy(baseThatLost);

		RecyclePathInMap(recycledPath);

		//Debug.Log("Enemy recycled");


	}

	private void RecyclePathInMap(PathCreator recycledPath)
	{
		switch (GameManager.instance.map)
		{
			case MapLevel.Normal:
				normalPaths.Add(recycledPath);
				break;

			case MapLevel.Hard:
				hardPaths.Add(recycledPath);
				break;

			case MapLevel.Brutal:
				brutalPaths.Add(recycledPath);
				break;
		}
	}



	private PathCreator GetNewPath()
	{
		PathCreator newPath = null;
		int r = 0;

		switch (GameManager.instance.map)
		{
			case MapLevel.Normal:
				r = Random.Range(0, normalPaths.Count);
				newPath = normalPaths[r];
				normalPaths.Remove(newPath);
				break;

			case MapLevel.Hard:
				r = Random.Range(0, hardPaths.Count);
				newPath = hardPaths[r];
				hardPaths.Remove(newPath);
				break;

			case MapLevel.Brutal:
				r = Random.Range(0, hardPaths.Count);
				newPath = brutalPaths[r];
				brutalPaths.Remove(newPath);
				break;
			
		}



		return newPath;
	}



	public void SetNewPatrollingPath()
	{
		for (int i = 0; i < enemyBase.Count; i++)
		{
			if (enemyBase[i].CheckIfCircling())
			{
				EnemyBase currentEnemybase = enemyBase[i];
				currentEnemybase.SetNewPatrolPath(GetNewPath());
				enemyBase.Remove(currentEnemybase);

				//Debug.Log("A new path has been assigned");

				return;

			}
		}

		//Debug.Log("No Enemies to assign");

	}

	public void AssignNewPathPeriodically()
	{
		StartCoroutine(WaitToAssign());
	}


	private IEnumerator WaitToAssign()
	{

		SetNewPatrollingPath();

		yield return new WaitForSeconds(assignNewPathTime);

		//Debug.Log("Waiting for next period");
		AssignNewPathPeriodically();


	}

	private void ClearList()
	{
		enemyBase.Clear();


		foreach (EnemyBase eB in enemyBase)
		{
			if (eB == null)
			{
				enemyBase.Remove(eB);
			}
		}

	}


}
