using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CentralCommand : Location
{
	[SerializeField]
	private FixedSpriteController sc;
	private TextMeshPro hangerText;
	private AircraftSpawner spawner;


	[Header("Central Command Settings")]
	public int neededAmount = 10;
	public float unitProductionTime = 15f;
	public float currentProductionTime = 0;
	public float soundNotificationTime = 5f;
	private bool hasNotified;
	public bool isCurrentlyBuilding;

	public List<AudioClip> sfx = new List<AudioClip>();
	public AudioSource aS;

	protected override void Awake()
	{
		hangerText = GetComponentInChildren<TextMeshPro>();
		base.Awake();
		spawner = GetComponent<AircraftSpawner>();
	}

	private void OnEnable()
	{
		
		GameManager.OnGameReady += OnGameStartInit;
	}

	private void OnDisable()
	{
		GameManager.OnGameReady -= OnGameStartInit;
	}

	private void OnGameStartInit()
	{
		sc = GetComponentInChildren<FixedSpriteController>();
		sc.SMask.transform.localPosition = SpriteController.GetEndPos(sc, receivingBay.holdAmount, neededAmount);
		UpdateHangerText();
		if (CanBuild()) StartProducingUnit();
	}

	private bool CanBuild()
	{
		return receivingBay.holdAmount >= neededAmount && !isCurrentlyBuilding;
	}

	public void ResetNeeds()
	{
		receivingBay.holdAmount -= neededAmount;
		hasNotified = false;
		isCurrentlyBuilding = false;
		UpdateHangerText();

		spawner.WhenSecondUnitSpawned();

		if (CanBuild()) StartProducingUnit();
	}


	public void UpdateHangerText()
	{
		if (isCurrentlyBuilding) return;

		if (CanBuild())
		{
			hangerText.text = "Ready To Build";
			rangeIndi.zoneText.text = "Currently In Production. Do not Require Material";
		}
		else
		{
			hangerText.text = "Require Material: " + receivingBay.holdAmount + "/" + neededAmount;
			rangeIndi.zoneText.text = "Landing Zone";
		}

	}


	public override void ReactTo_Receiving(float waitDuration)
	{
		sc.SMask.transform.localPosition = SpriteController.GetEndPos(sc, receivingBay.holdAmount, neededAmount);

		UpdateHangerText();

		rangeIndi.zoneText.text = "Landing Zone";

		if (CanBuild() && !isCurrentlyBuilding) StartProducingUnit();

	}


	private void StartProducingUnit()
	{
		isCurrentlyBuilding = true;
		StartCoroutine(ProducingCoroutine());
	}

	IEnumerator ProducingCoroutine()
	{
		currentProductionTime = unitProductionTime;

		while (currentProductionTime > 0)
		{
			if (!hasNotified)
			{
				if (currentProductionTime <= soundNotificationTime)
				{
					PlayPreSpawnSound();
					hasNotified = true;
				}
			}


			hangerText.text = "Producing New Aircarft: " + (int)currentProductionTime;

			sc.SMask.transform.localPosition = Vector3.Lerp(sc.fullPos, sc.depletedPos, 1 - (currentProductionTime / unitProductionTime));

			currentProductionTime -= Time.deltaTime;

			yield return null;
		}

		spawner.SpawnUnit();

		if (GameManager.instance.currentState == GameState.Tutorial
			&& TutorialManager.instance.currentTask == TutorialTask.DeliverMineToRefinery)
		{
			TutorialManager.instance.UnitSpawned = true;
		}

		PlaySpawnedSound();
		ResetNeeds();

	}

	private void PlaySpawnedSound()
	{
		aS.clip = sfx[1];
		aS.Play();

	}

	private void PlayPreSpawnSound()
	{
		aS.clip = sfx[0];
		aS.Play();
	}

}
