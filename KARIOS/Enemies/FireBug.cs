using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FireBug : Enemy
{
	public bool testing = false;
	//isAttacking is used to pause enemy action when the mission is over (player dies)
	private bool isAttacking = true;

	[Header("Position Settings")]
	public float airLevel = 30;
	public float groundLevel = 1.15f;
	public Vector2 landingOffset;

	[Header("Settings")]
	public Vector2 airTime;
	public float changingHeightTime = 1f;
	public float groundIdleTime = 5f;
	public float warningLandTime = 0.5f;



	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		co.enabled = false;

		if (!testing)
		{
			if (transform.position.y >= airLevel)
			{
				StartCoroutine(FindPlayer());
			}
			else
			{
				StartCoroutine(ChangeHeight(airLevel));
			}
		}
	}


	/// <summary>
	/// Change Height, either air level or ground level. 
	/// </summary>
	/// <param name="yOffset"></param>
	/// <returns></returns>
	IEnumerator ChangeHeight(float yOffset)
	{
		float currentTime = 0;
		bool showWarning = false;
		Vector3 startPos = transform.position;
		Vector3 endPos = FilterEndPosition(yOffset);


		while (currentTime < 1 && isAttacking)
		{
			transform.position = Vector3.Lerp(startPos, endPos, currentTime);

			showWarning = ShowLandingPosition(yOffset, currentTime, showWarning, endPos);

			yield return null;

			currentTime += Time.deltaTime / changingHeightTime;
		}

		if (isAttacking)
		{

			if (yOffset == groundLevel)
			{
				StartCoroutine(WaitingToGoUp());
				BasicAttack();
			}
			else
			{
				StartCoroutine(FindPlayer());
			}

		}

	}

	private bool ShowLandingPosition(float yOffset, float currentTime, bool showWarning, Vector3 endPos)
	{
		if (yOffset == groundLevel && currentTime >= warningLandTime && !showWarning)
		{
			showWarning = true;
			ShowMuzzleFlash(endPos);
		}

		return showWarning;
	}

	/// <summary>
	/// Based on the yOffset (e.g. Air level), it will return the proper endPos (e.g. Ground level)
	/// </summary>
	/// <param name="yOffset"></param>
	/// <returns></returns>
	private Vector3 FilterEndPosition(float yOffset)
	{
		Vector3 endPos;
		if (yOffset == groundLevel)
		{
			endPos = new Vector3(transform.position.x + Random.Range(landingOffset.x, landingOffset.y), yOffset, transform.position.z + Random.Range(landingOffset.x, landingOffset.y));
		}
		else
		{
			endPos = new Vector3(transform.position.x, yOffset, transform.position.z);
		}

		return endPos;
	}

	/// <summary>
	/// Follows the player's position while in air. After a short duration, it will land at that location. 
	/// </summary>
	/// <returns></returns>
	IEnumerator FindPlayer()
	{
		float currentTime = 0;
		float currentAirTime = Random.Range(airTime.x, airTime.y);
		while (currentTime < 1 && playerPos != null && isAttacking)
		{
			transform.position = new Vector3(playerPos.position.x, transform.position.y, playerPos.position.z);

			yield return null;

			currentTime += Time.deltaTime / currentAirTime;

		}

		if (isAttacking) StartCoroutine(ChangeHeight(groundLevel));

	}

	/// <summary>
	/// Staying Idle at ground level. After a short duration, it will go up. 
	/// </summary>
	/// <returns></returns>
	IEnumerator WaitingToGoUp()
	{
		co.enabled = true;
		float currentTime = 0;

		while (currentTime < 1 && isAttacking)
		{

			currentTime += Time.deltaTime / groundIdleTime;
			yield return null;
		}

		co.enabled = false;

		if (isAttacking) StartCoroutine(ChangeHeight(airLevel));

	}


	public override void TakeDamage(float damage)
	{
		base.TakeDamage(damage);
	}


	protected override void Die()
	{
		base.Die();

		BasicAttack();
		Deactivate();
	}

	private void Deactivate()
	{
		isAttacking = false;
		StopAllCoroutines();
	}

	protected override void OnPlayerDies()
	{
		base.OnPlayerDies();
		Deactivate();
	}


}
