using UnityEngine;
using Chronos;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using System;
using Random = UnityEngine.Random;

public class Mech : Enemy
{
	private NavMeshAgent agent;
	private Animator anim;
	private Rigidbody rb;

	private float minRotBeforeShoot = 5;
	private bool isAttacking = true;
	private Vector3 targetPos;

	[Header("---Glow Settings---")]
	[SerializeField] private SkinnedMeshRenderer helmetMat;
	[ColorUsageAttribute(true, true)]
	[SerializeField] private Color shootWarning;
	[ColorUsageAttribute(true, true)]
	[SerializeField] private Color normalHelmetC;

	[Header("---One Time Settings---")]
	//Attack waits for this to finish first
	[SerializeField] private float colourChangingTime = 0.6f;
	public bool useLead = false;
	//Mech's animation makes the character always face slightly left or right, 
	//Adding this offset makes sure it shoots where I want it to. 
	public float rotYOffSet = 15;


	[Header("---Randomized Stats---")]
	[SerializeField] private Vector2 stoppingDistance;
	[SerializeField] private Vector2 leadModifier;
	[SerializeField] private Vector2 idleTime;
	[SerializeField] private Vector2 standShootTime;



	protected override void Awake()
	{
		base.Awake();

		InitializeMech();
		InitializeAgents();

	}

	private void InitializeMech()
	{
		anim = GetComponent<Animator>();
		rb = GetComponent<Rigidbody>();
	}

	private void InitializeAgents()
	{
		agent = GetComponent<NavMeshAgent>();
		agent.speed = moveSpeed;
		agent.angularSpeed = rotSpeed;
		agent.stoppingDistance = Random.Range(stoppingDistance.x, stoppingDistance.y);
	}

	protected override void Start()
	{
		base.Start();
		StartWalking();

	}

	void Update()
	{
		CheckIfWithinAttackRange();
	}


	protected override void BasicAttack()
	{
		base.BasicAttack();

	}


	public override void TakeDamage(float damage)
	{
		base.TakeDamage(damage);
	}

	/// <summary>
	/// Stops its routine when player dies. 
	/// </summary>
	protected override void OnPlayerDies()
	{
		//Stop all Coroutines dont work if theres a while loop :(
		//I have to use a bool to get the AI out of the loop
		isAttacking = false;
		StopAllCoroutines();
		SetIdleAnimation();

		base.OnPlayerDies();
	}

	private void SetIdleAnimation()
	{
		anim.SetBool("isAttacking", false);
		anim.SetBool("isWalking", false);
	}

	/// <summary>
	/// Calculate where the player character would be based on its current move speed when the mech takes the next shot.
	/// </summary>
	private void TargetLeadCalculation()
	{
		Vector3 V = playerPos.GetComponent<LocomotionController>().GetVelocity() * Random.Range(leadModifier.x, leadModifier.y);
		Vector3 D = playerPos.position - transform.position;
		float A = V.sqrMagnitude - Mathf.Pow(14, 2);
		float B = 2 * Vector3.Dot(D, V);
		float C = D.sqrMagnitude;
		if (A >= 0)
		{
			targetPos = playerPos.position;
		}
		else
		{
			float rt = Mathf.Sqrt(B * B - 4 * A * C);
			float dt1 = (-B + rt) / (2 * A);
			float dt2 = (-B - rt) / (2 * A);
			float dt = (dt1 < 0 ? dt2 : dt1);
			targetPos = playerPos.position + V * dt;

			Debug.DrawLine(targetPos, targetPos + (Vector3.up * 3), Color.red, 4);
		}
	}

	protected override void Die()
	{
		base.Die();

		isAttacking = false;

		StopAllCoroutines();

		DisableCollision();

		DisablePathing();

		SetDieAnimation();

		helmetMat.material.SetColor("_EmissionColor", Color.black);


	}
	//Called in animation event
	private void OnFall()
	{
		SFXManager.instance.OnMechDie();
	}

	private void DisableCollision()
	{
		GetComponent<Collider>().enabled = false;
	}

	private void SetAttackAnimation()
	{
		anim.SetBool("isAttacking", true);
		anim.SetBool("isWalking", false);
	}
	private void SetDieAnimation()
	{
		anim.SetBool("Die", true);

		anim.SetBool("isAttacking", false);
		anim.SetBool("isWalking", false);
	}

	/// <summary>
	/// Set navAgent destination and trigger animation. 
	/// </summary>
	private void StartWalking()
	{
		if (playerPos != null)
		{
			agent.SetDestination(playerPos.position);
			agent.isStopped = false;
			anim.SetBool("isWalking", true);
		}
	}

	/// <summary>
	/// After setting the player as its destination, this will check if the mech is within attack range everyframe
	/// Once it's in range, it will start the PreAttack Warning, which will call the Shoot coroutine after. 
	/// </summary>
	private void CheckIfWithinAttackRange()
	{
		if (isDead) return;

		if (!isAttacking) return;


		if (agent.hasPath && playerPos != null)
		{
			if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && agent.isStopped == false)
			{
				StartCoroutine(WarningGlow());
			}
		}
	}

	/// <summary>
	/// After the PreAttack wanring (StartUp Frame), it will start the shoot coroutine. 
	/// </summary>
	/// <returns></returns>
	IEnumerator WarningGlow()
	{
		agent.isStopped = true;

		float currentTime = 0;

		while (currentTime < 1 && isAttacking && !isDead)
		{
			helmetMat.material.SetColor("_EmissionColor", Color.Lerp(normalHelmetC, shootWarning, currentTime));

			yield return null;

			currentTime += Time.deltaTime / colourChangingTime;
		}

		StartCoroutine(Shoot());
	}

	/// <summary>
	/// Shoot Coroutine is called after PreAttack Warning is over.
	/// This consists of (1) rotate the Mech to the shoot direction, (2) Shoot for a certain duration, 
	/// (3) become idle for a certain duration, (4) starts Walking again. 
	/// </summary>
	/// <returns></returns>
	IEnumerator Shoot()
	{

		//Rotate to target//
		Quaternion targetRotation = Quaternion.identity;

		if (playerPos != null)
		{
			TargetLeadCalculation();
			targetRotation = Quaternion.LookRotation(((useLead ? targetPos : playerPos.position) - transform.position), Vector3.up) * Quaternion.Euler(0, rotYOffSet, 0);
		}

		while (Mathf.Abs(transform.rotation.eulerAngles.y - targetRotation.eulerAngles.y) > minRotBeforeShoot && isAttacking)
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);
			yield return null;

		}
		//////////////////////

		SetAttackAnimation();

		yield return new WaitForSeconds(Random.Range(standShootTime.x, standShootTime.y));

		anim.SetBool("isAttacking", false);

		helmetMat.material.SetColor("_EmissionColor", normalHelmetC);

		yield return new WaitForSeconds(Random.Range(idleTime.x, idleTime.y));

		StartWalking();


	}


	private void DisablePathing()
	{
		agent.isStopped = true;
		agent.enabled = false;
	}



}
