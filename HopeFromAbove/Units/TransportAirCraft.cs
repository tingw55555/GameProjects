using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum ResourceType
{
	None = 0,
	ReliefPackage = 1,
	Ore = 2,
	Material = 3,
	Missile = 4,

}


[RequireComponent(typeof(PathMover))]
public class TransportAirCraft : BaseUnit
{
	public static event Action<int> RemoveAgentFromTrackList;

	private PathMover pm;
	private CargoController cc;
	private AudioSource aS;
	public List<AudioClip> acSounds = new List<AudioClip>();
	public GameObject explosionPrefab;

	[Space(10)]
	public GameObject targeted;
	[Space(10)]




	public float unitRangeDisappearTime = 3f;
	public float landingDuration = 1;

	[Header("RunTime Value")]
	[SerializeField]
	private float landingTime = 0;
	[SerializeField]
	private float waitTime = 0;


	[Space(10)]
	public bool hasCollided = false;
	[SerializeField]
	private bool isExposed = false;
	[SerializeField]
	private float exposedTime = 0;


	[Space(10)]
	private Location currentLocation;

	private Vector3 lastPos;
	private Vector3 currentVel;

	private void Awake()
	{

		pm = GetComponent<PathMover>();
		cc = GetComponentInChildren<CargoController>();
		aS = GetComponent<AudioSource>();
		lastPos = transform.position;

	}

	public void TACInit(Color newColour, AgentID newID)
	{
		pm.UnitColourInit(newColour);
		pm.SetAgentID(newID);
		pm.ConnectHotKey();

		if (cc != null)
		{
			cc.CargoBayInit();
		}
		else
		{
			Debug.LogError("Make sure to add cargo controller and cargoBay script on the TAC.");
		}

	}


	private void OnCollisionEnter(Collision collision)
	{
		TransportAirCraft otherAC = collision.gameObject.GetComponent<TransportAirCraft>();

		FighterJet enemyFJ = collision.gameObject.GetComponent<FighterJet>();
		Missile missile = collision.gameObject.GetComponent<Missile>();

		if (enemyFJ != null || missile != null)
		{
			DestroyAC();
		}

		if (hasCollided) return;


		if (otherAC != null)
		{
			otherAC.hasCollided = true;

			Vector3 OtherAChitDirection = (transform.position - otherAC.transform.position).normalized;
			float OtherACimpact = Vector3.Dot(otherAC.currentVel.normalized, OtherAChitDirection);

			Vector3 myHitDirection = -OtherAChitDirection;
			float myACImpact = Vector3.Dot(currentVel.normalized, myHitDirection);


			if (myACImpact > OtherACimpact)
			{
				otherAC.DestroyAC();
				hasCollided = false;
			}
			else
			{
				DestroyAC();
			}
		}


	}

	public void DestroyAC()
	{
		pm.ConnectHotKey(false);
		RemoveAgentFromTrackList?.Invoke((int)pm.agentID - 1);
		//SFXManager.instance.PlaySFX(SFX.Crush);
		Instantiate(explosionPrefab, transform.position, transform.rotation);
		Destroy(gameObject);
	}


	private void Update()
	{
		currentVel = (transform.position - lastPos) / Time.deltaTime;
		lastPos = transform.position;
	}

	private void OnTriggerEnter(Collider other)
	{
		Location location = other.GetComponent<Location>();

		DetectLandingZone(location);
	}

	private void DetectLandingZone(Location location)
	{
		if (location != null)
		{
			//If the landing spot is not occupied
			if (location.currentAC == AgentID.unusable || location.currentAC == pm.agentID)
			{
				float waitDuration = 0;
				CargoBay cargoBayToCheck = null;

				currentLocation = location;

				if (location.ReceiveLoad(cc, pm.agentID, landingDuration, out waitDuration, out cargoBayToCheck))
				{
					StartLanding(location.GetLandingSpot(), waitDuration, cargoBayToCheck);

				}
				else if (location.ExtractLoad(cc, pm.agentID, landingDuration, out waitDuration, out cargoBayToCheck))
				{
					StartLanding(location.GetLandingSpot(), waitDuration, cargoBayToCheck);
				}
			}

		}

	}


	public void StartLanding(Vector3 landingSpot, float waitDuration, CargoBay cargoBayToCheck)
	{
		pm.SetMoveStatus(false);
		StartCoroutine(LandingCoroutine(landingSpot, waitDuration, cargoBayToCheck));
	}

	IEnumerator LandingCoroutine(Vector3 landingSpot, float waitDuration, CargoBay cargoBayToCheck)
	{
		Vector3 currentPos = transform.position;
		Quaternion currentRot = transform.rotation;
		landingTime = 0;

		while (landingTime < landingDuration)
		{
			transform.position = Vector3.Lerp(currentPos, landingSpot, landingTime / landingDuration);
			transform.rotation = Quaternion.Slerp(currentRot, Quaternion.identity, landingTime / landingDuration);
			yield return null;

			landingTime += Time.deltaTime;
		}

		transform.position = landingSpot;
		transform.rotation = Quaternion.identity;
		StartCoroutine(WaitCoroutine(waitDuration, cargoBayToCheck));

	}


	IEnumerator WaitCoroutine(float waitDuration, CargoBay cargoBayToCheck)
	{
		cc.SpawnChangedAmountText();
		cargoBayToCheck.SetCargoBayVisuals(cargoBayToCheck.holdResource);

		waitTime = 0;

		Vector3 startPos = cargoBayToCheck.SMask.transform.localPosition;
		Vector3 fillpos = SpriteController.GetEndPos(cargoBayToCheck, cargoBayToCheck.holdAmount, cargoBayToCheck.maxCapacity);

		while (waitTime < waitDuration)
		{
			cargoBayToCheck.SMask.transform.localPosition = Vector3.Lerp(startPos, fillpos, waitTime / waitDuration);
			yield return null;

			waitTime += Time.deltaTime;

		}
		cargoBayToCheck.UpdateCargoBayVisual_IfEmpty();


		pm.SetMoveStatus(true);

		if (currentLocation.requiresResource || currentLocation.GetComponent<ReliefHotSpot>() !=null)
		{
			DetectLandingZone(currentLocation);

		}



	}




	public void ExposedToEnemies(float timeBeforeHit)
	{
		isExposed = true;
		TargetStatus(isExposed);
		StartCoroutine(ExposedCoroutine(timeBeforeHit));
	}

	public void SafeFromEnemies()
	{
		isExposed = false;
		TargetStatus(isExposed);
	}

	IEnumerator ExposedCoroutine(float timeBeforeHit)
	{
		exposedTime = 0;

		while (isExposed && exposedTime < timeBeforeHit)
		{
			exposedTime += Time.deltaTime;
			yield return null;
		}

		if (isExposed)
		{
			DestroyAC();
		}

	}

	public void TargetStatus(bool isActive)
	{
		targeted.SetActive(isActive);
	}
	private void OnMouseEnter()
	{
		CancelHideIndi();
		pm.RangeIndicator.ShowIndicator();
	}

	private void OnMouseExit()
	{
		if (!pm.IsSelected) pm.RangeIndicator.HideIndicator();

		HideIndicatorInSeconds();

	}

	//This is pretty stupid but it will work for now!!
	private void HideIndicatorInSeconds()
	{
		pm.HideRangeIndi(unitRangeDisappearTime);

	}

	private void CancelHideIndi()
	{
		pm.CancelInvoke();
	}


	public void PlaySelectedSound()
	{
		aS.PlayOneShot(acSounds[0]);
	}


}
