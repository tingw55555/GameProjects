using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using TMPro;

public class ProductionSpot : Location
{
	private FixedSpriteController sc;
	private TextMeshPro _productionText;
	private TextMeshPro _rawSupplytext;


	[Header("Production Building Settings")]
	public float producingTime = 1;
	public float nextProductIn = 0;

	private IEnumerator currentCoroutine;





	protected override void Awake()
	{
		base.Awake();

		sc = GetComponentInChildren<FixedSpriteController>();
		_productionText = GetComponentInChildren<TextMeshPro>();


		if (requiresResource)
		{
			extractingBay = cargoBays[1];
			_rawSupplytext = transform.GetChild(2).GetComponent<TextMeshPro>();
			SetDefualt_RawSupply_Text();
		}

	}


	private void OnEnable()
	{
		GameManager.OnGameReady += ProductionInit;
	}

	private void OnDisable()
	{
		GameManager.OnGameReady -= ProductionInit;
	}



	public void ProductionInit()
	{
		sc.SMask.transform.localPosition = SpriteController.GetEndPos(sc, extractingBay.holdAmount, extractingBay.maxCapacity);

		sc.SetColour();

		Set_ProductionStatus_Text("Producing " + extractingBay.holdResource.ToString());

		StartProducing();
	}

	public void StartProducing()
	{
		rangeIndi.zoneText.text = "Landing Zone";

		if (requiresResource)
		{
			StartCoroutine(ExchangeCoroutine());
		}
		else
		{
			StartCoroutine(ProduceCoroutine());
		}
	}



	IEnumerator ProduceCoroutine()
	{
		if (extractingBay.holdAmount >= extractingBay.maxCapacity)
		{
			Set_ProductionStatus_Text(extractingBay.holdResource.ToString() + ": " + extractingBay.holdAmount.ToString() + "/" + extractingBay.maxCapacity.ToString());
			yield break;
		}

		float totalProductionTime = producingTime * (extractingBay.maxCapacity - extractingBay.holdAmount + 1);
		float currentProductionTime = 0;
		nextProductIn = 0;

		Vector3 startpos = sc.SMask.transform.localPosition;

		while (currentProductionTime < totalProductionTime)
		{
			currentProductionTime += Time.deltaTime;
			nextProductIn += Time.deltaTime;

			if (nextProductIn >= producingTime)
			{
				extractingBay.holdAmount++;
				nextProductIn = 0;


				Set_ProductionStatus_Text(extractingBay.holdResource.ToString() + ": " + extractingBay.holdAmount.ToString() + "/" + extractingBay.maxCapacity.ToString());
			}

			sc.SMask.transform.localPosition = Vector3.Lerp(startpos, sc.fullPos, currentProductionTime / totalProductionTime);


			yield return null;

		}

		Set_ProductionStatus_Text(extractingBay.holdResource.ToString() + ": " + extractingBay.holdAmount.ToString() + "/" + extractingBay.maxCapacity.ToString());

	}


	IEnumerator ExchangeCoroutine()
	{

		//Refinery will stop if: Material has maxed out 
		if (extractingBay.holdAmount >= extractingBay.maxCapacity)
		{
			Set_ProductionStatus_Text(extractingBay.holdResource.ToString() + ": " + extractingBay.holdAmount.ToString() + "/" + extractingBay.maxCapacity.ToString());
			yield break;
		}


		//Refinery will stop if: has less Ore than it needs to keep producing
		if (receivingBay.holdAmount < rawToRefineRatio)
		{
			SetDeficient_Production_Text();
			SetDeficient_RawSupply_Text();
			yield break;
		}

		float totalProductionTime = producingTime * ((float)(receivingBay.holdAmount + 1) / (float)rawToRefineRatio);
		float currentProductionTime = 0;
		nextProductIn = 0;

		Vector3 startpos = sc.SMask.transform.localPosition;

		Vector3 endPos = SpriteController.GetEndPos(sc, (int)((float)receivingBay.holdAmount / (float)rawToRefineRatio), extractingBay.maxCapacity);

		//currentProductionTime < totalProductionTime &&

		while (receivingBay.holdAmount >= rawToRefineRatio && extractingBay.holdAmount < extractingBay.maxCapacity)
		{
			if (nextProductIn >= producingTime)
			{
				receivingBay.holdAmount -= rawToRefineRatio;
				extractingBay.holdAmount++;
				nextProductIn = 0;

				Set_RawSupplyStatus_Text(receivingBay.holdResource.ToString() + " : " + receivingBay.holdAmount.ToString() + "/" + receivingBay.maxCapacity.ToString());
				Set_ProductionStatus_Text(extractingBay.holdResource.ToString() + ": " + extractingBay.holdAmount.ToString() + "/" + extractingBay.maxCapacity.ToString());
			}

			currentProductionTime += Time.deltaTime;
			nextProductIn += Time.deltaTime;

			sc.SMask.transform.localPosition = Vector3.Lerp(startpos, endPos, currentProductionTime / totalProductionTime);

			yield return null;

		}

		if (extractingBay.holdAmount >= extractingBay.maxCapacity)
		{
			Set_RawSupplyStatus_Text("Production Hult. " + extractingBay.holdResource + " Storage Full");
			sc.SMask.transform.localPosition = sc.fullPos;
		}
		else if (receivingBay.holdAmount < rawToRefineRatio)
		{
			SetDeficient_RawSupply_Text();
		}
		else if (receivingBay.holdAmount >= rawToRefineRatio)
		{
			Set_ProductionStatus_Text(extractingBay.holdResource.ToString() + ": " + extractingBay.holdAmount.ToString() + "/" + extractingBay.maxCapacity.ToString());
		}

	}



	IEnumerator ExtractCoroutine(float extractDuration)
	{
		float waitTime = 0;
		Vector3 startPos = sc.SMask.transform.localPosition;
		Vector3 endPos = SpriteController.GetEndPos(sc, extractingBay.holdAmount, extractingBay.maxCapacity);

		SetTransport_ProductionText();

		while (waitTime < extractDuration)
		{
			rangeIndi.zoneText.text = "Finish loading in: " + ((int)extractDuration - (int)waitTime);

			sc.SMask.transform.localPosition = Vector3.Lerp(startPos, endPos, waitTime / extractDuration);
			waitTime += Time.deltaTime;

			yield return null;

		}

		StartProducing();

	}

	IEnumerator SupplyCoroutine(float unloadDuration)
	{
		float waitTime = 0;

		while (waitTime < unloadDuration)
		{
			Set_RawSupplyStatus_Text("Finish loading " + receivingBay.holdResource + " in: " + ((int)unloadDuration - (int)waitTime));

			waitTime += Time.deltaTime;

			yield return null;

		}

		SetDefualt_RawSupply_Text();
		Set_ProductionStatus_Text(extractingBay.holdResource.ToString() + ": " + extractingBay.holdAmount.ToString() + "/" + extractingBay.maxCapacity.ToString());
		StartProducing();
	}



	private void Set_ProductionStatus_Text(string msg)
	{
		_productionText.text = msg;
	}
	private void Set_RawSupplyStatus_Text(string msg)
	{
		_rawSupplytext.text = msg;
	}

	//This states the Raw to Refine Ratio
	private void SetDefualt_RawSupply_Text()
	{
		_rawSupplytext.text = "Every " + rawToRefineRatio + " " + receivingBay.holdResource + " produces 1 " + extractingBay.holdResource;
	}

	private void SetTransport_ProductionText()
	{
		_productionText.text = "Production Hault. Is Transporting.";
	}


	//This shows there's not enough raw for production to contiune
	private void SetDeficient_Production_Text()
	{
		_productionText.text = "Production Hault. not Enough " + receivingBay.holdResource;
	}

	//This shows how much more raw is needed to create 1 refine
	public void SetDeficient_RawSupply_Text()
	{
		_rawSupplytext.text = "Require at least : " + (rawToRefineRatio - receivingBay.holdAmount) + " " + receivingBay.holdResource + " to continue production";
	}

	public override void ReactTo_Receiving(float waitDuration)
	{

		if (requiresResource)
		{

			StartCoroutine(SupplyCoroutine(waitDuration));
		}
	}


	public override void ReactTo_Extracting(float waitDuration)
	{
		StartCoroutine(ExtractCoroutine(waitDuration));
	}


}
