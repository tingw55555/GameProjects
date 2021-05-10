using PathCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurrenderManager : MonoBehaviour
{
	

	public PathCreator premisePath;
	public GameObject jetPrefab;

	private void Awake()
	{
		ReliefHotSpot.onCivilianBecomeEnemy += InitializeNewEnemy;
	}

	private void InitializeNewEnemy(EnemyBase newEnemy)
	{
		newEnemy.premisePath = Instantiate(premisePath, newEnemy.gameObject.transform);
		newEnemy.premisePath.transform.localPosition = Vector3.zero;

		newEnemy.jetPrefab = jetPrefab;

		newEnemy.enabled = true; 
	}


}
