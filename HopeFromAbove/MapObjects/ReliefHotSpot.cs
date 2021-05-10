using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class ReliefHotSpot : Location
{
	public static event Action OnCivRegister;
	public static event Action<int> OnReachingPeaceTime;
	public static event Action<EnemyBase> onCivilianBecomeEnemy;

	private ChangingSpriteController sc;
	private TextMeshPro text;

	[Header("Civilian Building Settings")]
	public float consumeSpeedPerRes = 8f;
	public float consumeCountDown = 0;
	public int lowWarningAmount = 5;

	private bool isReceiving = false;
	private bool isLosingHope = false;

	public float peaceTime = 10;
	public float surrenderTime = 30;

	public float willSurrenderIn = 0;

	public bool isCapital = false;

	public Vector2Int InitialSupply = new Vector2Int(12, 6);
	private int receiveAmount = 0;

	protected override void Awake()
	{
		sc = GetComponentInChildren<ChangingSpriteController>();
		text = GetComponentInChildren<TextMeshPro>();

		base.Awake();

	}

	private void OnEnable()
	{
		GameManager.OnGameReady += Register;
	}

	private void OnDisable()
	{
		GameManager.OnGameReady -= Register;
	}

	private void Register()
	{
		OnCivRegister?.Invoke();
		CivilianInit();
	}

	public void CivilianInit()
	{
		receivingBay.holdAmount = Random.Range(InitialSupply.x, InitialSupply.y);
		
		sc.SMask.transform.localPosition = SpriteController.GetEndPos(sc, receivingBay.holdAmount, receivingBay.maxCapacity);

		if (receivingBay.holdAmount > 0)
		{
			if (receivingBay.holdAmount != receivingBay.maxCapacity)
			{
				sc.SetConsumeColour();
				SetStatusText("Remaining Relief Package: " + receivingBay.holdAmount + "/" + receivingBay.maxCapacity);
			}
			StartCoroutine(ConsumeSupply());
		}
		else
		{
			StartCoroutine(WaitForSupply());
		}
	}

	IEnumerator ConsumeSupply()
	{

		Vector3 startPos = sc.SMask.transform.localPosition;

		if (receivingBay.holdAmount >= receivingBay.maxCapacity)
		{
			SetStatusText("Has Stablized");
			sc.SetPeaceColour();
			yield return new WaitForSeconds(peaceTime);
		}

		float totallCountDown = receivingBay.holdAmount * consumeSpeedPerRes;
		float countDown = totallCountDown;
		consumeCountDown = consumeSpeedPerRes;

		while (countDown > 0 || receivingBay.holdAmount > 0)
		{
			countDown -= Time.deltaTime;
			consumeCountDown -= Time.deltaTime;

			UpdateConsumingState();

			sc.SMask.transform.localPosition = Vector3.Lerp(startPos, sc.depletedPos, 1 - (countDown / totallCountDown));

			yield return null;
		}

		StartCoroutine(WaitForSupply());
	}


	IEnumerator WaitForSupply()
	{
		ShowFeedBack_SurrenderWarning();

		isLosingHope = true;

		willSurrenderIn = surrenderTime;

		while (willSurrenderIn > 0)
		{
			sc.SMask.transform.localPosition = Vector3.Lerp(sc.depletedPos, sc.fullPos, 1 - (willSurrenderIn / surrenderTime));

			willSurrenderIn -= Time.deltaTime;

			yield return null;
		}

		if (!isReceiving)
		{
			OnSurrender();
		}
	}



	//Hold Amount is set in the Location script, which is before this coroutine is called. This is mostly for visual
	IEnumerator LoadingSupply(float waitDuration)
	{
		ShowFeedBack_ReceivingHelp();

		float waitTime = 0;

		Vector3 startPos = isLosingHope ? sc.depletedPos : sc.SMask.transform.localPosition;
		Vector3 fillPos = SpriteController.GetEndPos(sc, receivingBay.holdAmount, receivingBay.maxCapacity);
		//Debug.Log(waitDuration);

		while (waitTime < waitDuration)
		{
			rangeIndi.zoneText.text = "Finish Unloading in: " + ((int)waitDuration - (int)waitTime);

			sc.SMask.transform.localPosition = Vector3.Lerp(startPos, fillPos, waitTime / waitDuration);

			waitTime += Time.deltaTime;

			yield return null;

		}

		FullFilled();

	}

	private void ShowFeedBack_ReceivingHelp()
	{
		sc.SetConsumeColour();
		sc.HideWarning();
		SetStatusText("Is Receiving Help");
	}

	private void UpdateConsumingState()
	{
		if (consumeCountDown <= 0)
		{
			sc.SetConsumeColour();
			receivingBay.holdAmount--;
			consumeCountDown = consumeSpeedPerRes;
			SetStatusText("Remaining Relief Package: " + receivingBay.holdAmount + "/" + receivingBay.maxCapacity);


		}


		CheckIfLowOnResource();

	}


	private void ShowFeedBack_SurrenderWarning()
	{
		sc.SetDepleteColour();
		sc.ShowWarning();
		SFXManager.instance.PlaySFX(SFX.SurrenderWarning);
		SetStatusText("Is losing Hope");
	}




	private void CheckIfLowOnResource()
	{
		if (!sc.IsShowingLowOnResource() && receivingBay.holdAmount <= lowWarningAmount)
		{
			sc.ToggleLowPulse(true);
			SFXManager.instance.PlaySFX(SFX.LowResource);
			return;
		}

		if (receivingBay.holdAmount > lowWarningAmount)
		{
			sc.ToggleLowPulse(false);
		}

	}



	public override void ReactTo_Receiving(float waitDuration)
	{
		isReceiving = true;
		StartCoroutine(LoadingSupply(waitDuration));
	}

	public void FullFilled()
	{
		isReceiving = false;
		isLosingHope = false;
		rangeIndi.zoneText.text = "Landing Zone";

		CheckIfLowOnResource();

		CheckIfMaxedOut();

		StartCoroutine(ConsumeSupply());
	}

	private void CheckIfMaxedOut()
	{
		if (receivingBay.holdAmount >= receivingBay.maxCapacity)
		{
			OnReachingPeaceTime?.Invoke(receiveAmount);
		}
	}

	protected override void GetReceivingAmount(int newAmount)
	{
		receiveAmount = 0;
		receiveAmount = newAmount;

	}

	protected override void OnSurrender()
	{
		SFXManager.instance.PlaySFX(SFX.CityFall);

		if (isCapital)
		{
			GameManager.instance.OnLose(LoseCondition.CapitalFall);
		}

		PrepareToDefect();
	}


	private void PrepareToDefect()
	{
		GameManager.instance.OnBuildingFall();

		text.text = "Defected";

		rangeIndi.GiveupTerritority(transform.position);

		gameObject.AddComponent(typeof(EnemyBase));

		sc.ToggleLowPulse(false);

		EnemyBase newEnemy = GetComponent<EnemyBase>();

		newEnemy.enabled = false;

		onCivilianBecomeEnemy?.Invoke(newEnemy);

		Destroy(this);

	}

	public void SetStatusText(string msg)
	{
		text.text = msg;
	}

	public void AddToTotalCiv(int total)
	{
		total++;
	}



}
