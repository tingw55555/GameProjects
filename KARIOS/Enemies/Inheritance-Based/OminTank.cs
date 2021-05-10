using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OminTank : Enemy
{
	[Header("Reference")]
	public GameArea mapArea;

	[Header("Stat Settings")]
	public float startAttackDistance = 3;
	public float startUpDelay = 0.8f;
	public float boundAvoidDistance = 2;
	public float minDistance = 5f;
	public float firingTime = 3;
	public float fireInterval = 0.5f;

	[Header("RunTime Value")]
	[SerializeField] private bool isAttacking = false;
	[SerializeField] private bool isWandering = false;
	[SerializeField] private CCNeonFlash[] lightBalls;

	private Vector3 targetPos;
	private float yOffset = 1f;

	protected override void Awake()
	{
		base.Awake();
		mapArea = FindObjectOfType<GameArea>();
	}

	protected override void Start()
	{
		base.Start();
		GetNewWanderingPosition();
	}


	private void Update()
	{

		if (!isDead && playerPos != null)
		{
			HandleBehaviour();
		}

	}

	/// <summary>
	/// Move to target position
	/// </summary>
	private void Fly()
	{
		if (SFXManager.instance != null && muzzlePrefab.activeSelf) SFXManager.instance.OminMove();

		muzzlePrefab.SetActive(false);
		float moveStep = moveSpeed * Time.deltaTime;
		transform.position = Vector3.MoveTowards(transform.position, targetPos, moveStep);
		transform.rotation = Quaternion.LookRotation(targetPos - transform.position, Vector3.up);

	}

	/// <summary>
	/// Find a position away from the player within the game bounds
	/// </summary>
	/// <returns></returns>
	private Vector3 GetPlayerRepellPos()
	{
		Vector3 dir = GetDirectionToPlayer();
		Vector3 validPoint = GetValidPoint((dir * minDistance) + transform.position);

		return validPoint;

	}
	
	private bool IfCloseToBounds()
	{
		float leftMostX = mapArea.bounds.center.x - mapArea.bounds.extents.x + boundAvoidDistance;
		float rightMostX = mapArea.bounds.center.x + mapArea.bounds.extents.x - boundAvoidDistance;
		float botMostY = mapArea.bounds.center.z - mapArea.bounds.extents.z + boundAvoidDistance;
		float topMostY = mapArea.bounds.center.z + mapArea.bounds.extents.z - boundAvoidDistance;

		if (transform.position.x >= rightMostX || transform.position.x < leftMostX
			|| transform.position.z >= topMostY || transform.position.z < botMostY)
		{
			return true;
		}

		return false;

	}

	private Vector3 GetDirectionToPlayer()
	{
		return (transform.position - playerPos.position).normalized;
	}

	/// <summary>
	/// Clamp the vector3 within the map bounds
	/// </summary>
	/// <param name="newPos"></param>
	/// <returns></returns>
	private Vector3 GetValidPoint(Vector3 newPos)
	{
		Vector3 endPoint = mapArea.bounds.ClosestPoint(newPos);
		return new Vector3(endPoint.x, yOffset, endPoint.z);
	}

	/// <summary>
	/// Handle whether it attacks or avoids based on its distance from the player. 
	/// </summary>
	private void HandleBehaviour()
	{
		if (CanAttack())
		{
			StartAttacking();
		}
		else
		{
			if (isAttacking) return;
			StartAvoiding();
		}

	}

	/// <summary>
	/// Find and move to the new target position away from the player or the map bounds. 
	/// </summary>
	private void StartAvoiding()
	{
		if (!isWandering)
		{
			DetermineWhereToGo();
		}
		else
		{
			//If it IS avoidng, it checks against the minDistance to update the target pos 
			if (Vector3.Distance(transform.position, targetPos) < minDistance)
			{
				isWandering = false;
			}

		}

		Fly();
	}

	/// <summary>
	/// Check the distance between self and player. Return true if it's far enough (based on StartAttackDistance)
	/// </summary>
	/// <returns></returns>
	private bool CanAttack()
	{
		//If OminTank is far away from the Player
		if (Vector3.Distance(transform.position, playerPos.position) > startAttackDistance)
		{
			return true;
		}

		return false;
	}

	/// <summary>
	/// Check if it's too close to the map bounds and find a new target position
	/// If not, find a random position away from the player. 
	/// </summary>
	private void DetermineWhereToGo()
	{
		if (IfCloseToBounds())
		{
			GetNewWanderingPosition();
		}
		else
		{
			targetPos = GetPlayerRepellPos();
		}
	}

	/// <summary>
	/// If it's not already attacking, it will start the AttackCoroutine and shoot bullets as it rotates. 
	/// </summary>
	private void StartAttacking()
	{
		if (!isAttacking)
		{
			isAttacking = true;
			StartCoroutine(AttackCoroutine());
		}
	}

	/// <summary>
	/// Find a random position away from the map bounds within the bounds
	/// </summary>
	private void GetNewWanderingPosition()
	{
		Vector3 randomPos = new Vector3(Random.Range(-mapArea.bounds.extents.x, mapArea.bounds.extents.x), yOffset,
											   Random.Range(-mapArea.bounds.extents.z, mapArea.bounds.extents.z));
		Vector3 validPoint = randomPos + new Vector3(mapArea.bounds.center.x, 0, mapArea.bounds.center.z);

		targetPos = validPoint;

		isWandering = true;
	}

	IEnumerator AttackCoroutine()
	{
		float currentTime = 0;
		float waitToFireTime = 0;

		muzzlePrefab.SetActive(true);
		yield return new WaitForSeconds(startUpDelay);

		while (currentTime < 1)
		{
			float rotStep = rotSpeed * Time.deltaTime;
			transform.Rotate(transform.up, rotStep);

			if (waitToFireTime >= fireInterval)
			{
				BasicAttack();
				waitToFireTime = 0;
			}


			yield return null;

			waitToFireTime += Time.deltaTime / fireInterval;
			currentTime += Time.deltaTime / firingTime;

		}

		isAttacking = false;

	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(transform.position, startAttackDistance);
	}


	protected override void Die()
	{
		StopAllCoroutines();

		foreach (CCNeonFlash l in lightBalls)
		{
			l.SetSwitchModeDead();
		}

		muzzlePrefab.SetActive(false);

		base.Die();



	}


}
