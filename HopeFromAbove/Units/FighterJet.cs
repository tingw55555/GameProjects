using PathCreation;
using PathCreation.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterJet : BaseUnit
{
	private EnemyBase unitBase;

	[SerializeField]
	private GameObject targetAnimation;
	public float targetVisiableTime = 5f; 

	public void SetUnitBase(EnemyBase baseItCameFrom)
	{
		unitBase = baseItCameFrom;
	}


	public void Alertbase()
	{
		CancelInvoke();
		unitBase.OnJetDestroyed(GetComponent<PathFollower>().pathCreator);
	}

	public void ShowTarget()
	{
		targetAnimation.SetActive(true);
		Invoke("HideTarget", targetVisiableTime);
	}

	public void HideTarget()
	{
		targetAnimation.SetActive(false);
	}

}
