using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum LocationType
{
	OnlyReceive = 0,
	OnlyProduce = 1,
	Exchange = 2,
	Nointeraction = 3,
	ReceiveDesposite = 4,
}



public class Location : BaseMapObjects
{

	public LocationType locationType;

	protected RangeIndicator rangeIndi;
	protected SphereCollider sphereCol;

	public AgentID currentAC;

	[Header("Hover to see tooltip")]
	[Tooltip("First one is the Receiving Bay, second one is the Extracting Bay. By default both bays are the same since most buildings only take or only produce.")]
	public List<CargoBay> cargoBays = new List<CargoBay>();

	protected CargoBay receivingBay, extractingBay;

	[Space(5)]
	public float waitTimePerRes = 0.5f;

	[Header("Exchange Building Settings")]
	public bool requiresResource = false;
	public float waitTimePerRaw = 0.8f;
	public int rawToRefineRatio = 5;

	protected bool needCargo = true;

	protected virtual void Awake()
	{
		rangeIndi = GetComponentInChildren<RangeIndicator>();
		sphereCol = GetComponent<SphereCollider>();

		rangeIndi.LocationInit(sphereCol);

		currentAC = AgentID.unusable;

		if (needCargo)
		{
			//By default both bays are the same, this is overriden by Refinery
			receivingBay = cargoBays[0];
			extractingBay = cargoBays[0];
		}

	}

	protected virtual void OnSurrender()
	{
		Debug.Log(gameObject.name + ": has surrendered");
	}


	/// <summary>
	/// This is called when TAC comes to "Give" resources 
	/// </summary>
	public virtual bool ReceiveLoad(CargoController acCargo, AgentID acID, float landingDuration, out float unloadTime, out CargoBay cargoBayToUnload)
	{
		int desiredAmount = 0;
		int unloadAmount = 0;
		cargoBayToUnload = null;
		unloadTime = 0;

		if (locationType == LocationType.OnlyProduce || locationType == LocationType.Nointeraction) return false;

		desiredAmount = receivingBay.GetRemainingCapacity();
		if (desiredAmount <= 0 || receivingBay.holdResource == ResourceType.None) return false;


		for (int i = 0; i < acCargo.cargoBays.Count; i++)
		{
			if (acCargo.cargoBays[i].holdResource == receivingBay.holdResource && acCargo.cargoBays[i].holdAmount > 0)
			{
				//Central Command needs to keep producing even if it's receiving Materials
				if (GetComponent<CentralCommand>() == null)
				{
					StopAllCoroutines();
				}
					currentAC = acID;


				cargoBayToUnload = acCargo.cargoBays[i];
				int carryingAmount = cargoBayToUnload.holdAmount;

				for (int r = 0; r < desiredAmount && carryingAmount > 0; r++)
				{
					unloadAmount++;
					carryingAmount--;
				}


				GetReceivingAmount(unloadAmount);

				acCargo.GetChangedAmount(unloadAmount, false);


				cargoBayToUnload.holdAmount = carryingAmount;
				receivingBay.holdAmount += unloadAmount;


				unloadTime = requiresResource ? unloadAmount * waitTimePerRaw : unloadAmount * waitTimePerRes;

				StartCoroutine(ToReceive(landingDuration, unloadTime));


				return true;
			}
		}

		return false;
	}


	/// <summary>
	/// This is called when TAC comes to "Take" resources 
	/// </summary>
	public virtual bool ExtractLoad(CargoController acCargo, AgentID acID, float landingDuration, out float loadTime, out CargoBay cargoBayToload)
	{
		cargoBayToload = null;
		loadTime = 0;

		if (locationType == LocationType.OnlyReceive || locationType == LocationType.Nointeraction) return false;
		if (extractingBay.holdAmount <= 0 || extractingBay.holdResource == ResourceType.None) return false;

		//if the AC has the resource already and still have capacity 
		for (int i = 0; i < acCargo.cargoBays.Count; i++)
		{

			if (acCargo.cargoBays[i].holdResource == extractingBay.holdResource && acCargo.cargoBays[i].holdAmount < acCargo.cargoBays[i].maxCapacity)
			{
				StopAllCoroutines();
				currentAC = acID;

				cargoBayToload = acCargo.cargoBays[i];

				int loadedAmount = 0;
				int remainingCapacaity = cargoBayToload.maxCapacity - cargoBayToload.holdAmount;

				for (int r = 0; r < extractingBay.holdAmount && remainingCapacaity > 0; r++)
				{
					loadedAmount++;
					remainingCapacaity--;
				}

				acCargo.GetChangedAmount(loadedAmount, true);

				cargoBayToload.holdAmount = cargoBayToload.holdAmount + loadedAmount;
				extractingBay.holdAmount -= loadedAmount;

				loadTime = loadedAmount * waitTimePerRes;

				//cargoBayToload.SetCargoBayVisuals(extractingBay.holdResource);
				StartCoroutine(ToExtract(landingDuration, loadTime));


				return true;
			}
		}


		//Basically exactly the same code just checking if the AC has empty space & update CargoBay visual
		//To many things needed to be passed around so I just copied the code and be done with it
		for (int i = 0; i < acCargo.cargoBays.Count; i++)
		{

			if (acCargo.cargoBays[i].holdResource == ResourceType.None)
			{
				StopAllCoroutines();
				currentAC = acID;

				cargoBayToload = acCargo.cargoBays[i];
				cargoBayToload.holdResource = extractingBay.holdResource;
				int loadedAmount = 0;
				int remainingCapacaity = cargoBayToload.maxCapacity;

				for (int r = 0; r < extractingBay.holdAmount && remainingCapacaity > 0; r++)
				{
					loadedAmount++;
					remainingCapacaity--;
				}

				acCargo.GetChangedAmount(loadedAmount, true);

				cargoBayToload.holdAmount = loadedAmount;
				extractingBay.holdAmount -= loadedAmount;

				//cargoBayToload.SetCargoBayVisuals(extractingBay.holdResource);
				loadTime = loadedAmount * waitTimePerRes;

				StartCoroutine(ToExtract(landingDuration, loadTime));

				return true;
			}
		}



		return false;
	}

	public virtual Vector3 GetLandingSpot()
	{
		return rangeIndi.transform.position;
	}


	IEnumerator ToReceive(float landingDuration, float waitDuration)
	{
		rangeIndi.zoneText.text = "Is Landing";
		yield return new WaitForSeconds(landingDuration);
		ReactTo_Receiving(waitDuration);

	}

	IEnumerator ToExtract(float landingDuration, float waitDuration)
	{
		rangeIndi.zoneText.text = "Is Landing";
		yield return new WaitForSeconds(landingDuration);
		ReactTo_Extracting(waitDuration);

	}

	public virtual void ReactTo_Receiving(float waitDuration)
	{

	}



	public virtual void ReactTo_Extracting(float waitDuration)
	{

	}

	protected virtual void GetReceivingAmount(int receivedAmount)
	{

	}


	protected virtual void OnTriggerExit(Collider other)
	{
		currentAC = AgentID.unusable;
	}


}
