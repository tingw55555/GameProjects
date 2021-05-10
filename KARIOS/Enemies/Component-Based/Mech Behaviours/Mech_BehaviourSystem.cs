using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Mech_BehaviourSystem : AIBehaviourSystem
{
	private Vector3 targetPos;
	public bool useLead = true;
	private float minRotBeforeShoot = 5;
	private float rotSpeed = 300;
	public float rotOffset = 15;

	[SerializeField] private Vector2 leadModifier = new Vector2(1.7f, 2.2f);
	[SerializeField] private Vector2 standShootTime = new Vector2(.45f, 1.5f);
	[SerializeField] private Vector2 idleTime = new Vector2(0.8f, 0f);

	protected override void Awake()
	{
		base.Awake();
	}

	// Start is called before the first frame update
	protected override void Start()
	{
		base.Start();

	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	protected override void OnPlayerDies()
	{
		base.OnPlayerDies();
		StopAllCoroutines();
		anim.SetBool("isAttacking", false);
		anim.SetBool("isWalking", false);
	}

	protected override void StartWalking()
	{
		base.StartWalking();

		if (playerTransform != null && isActive)
		{
			bMove.targetPos = playerTransform.position;
			bMove.Move();
		}

	}


	protected override void Update()
	{
		CheckingForNextBehaviour();
	}


	protected override void CheckingForNextBehaviour()
	{
		base.CheckingForNextBehaviour();

		if (agent.hasPath && playerTransform != null)
		{
			if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && agent.isStopped == false)
			{
				bMove.Stop();
				bWarning.Alert(Vector3.zero);
			}
		}

	}

	protected override void Die()
	{
		base.Die();
		StopAllCoroutines();

	}


	//Called in animation event
	public void Aim()
	{
		if (isDead || !isActive) return;
		StartCoroutine(AimCoroutine());
	}

	IEnumerator AimCoroutine()
	{
		Quaternion targetRotation = DetermineShootAngle();

		while (Mathf.Abs(transform.rotation.eulerAngles.y - targetRotation.eulerAngles.y) > minRotBeforeShoot && isActive)
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotSpeed * Time.deltaTime);
			yield return null;
		}

		//transform.eulerAngles = new Vector3(transform.rotation.eulerAngles.x,
		//	targetRotation.eulerAngles.y, transform.rotation.eulerAngles.z);
		bAttack.Attack();
	}

	//Called in animation event
	public void Shoot()
	{
		if (isDead || !isActive) return;
		StartCoroutine(ShootCoroutine());

	}

	IEnumerator ShootCoroutine()
	{
		//Shooting Time
		yield return new WaitForSeconds(Random.Range(standShootTime.x, standShootTime.y));

		bAttack.DeactivateTrueAnim();
		yield return new WaitForSeconds(Random.Range(idleTime.x, idleTime.y));

		StartWalking();
	}

	private Quaternion DetermineShootAngle()
	{
		if (playerTransform != null)
		{
			TargetLeadCalculation();
			return Quaternion.LookRotation(((useLead ? targetPos : playerTransform.position) - transform.position), Vector3.up) * Quaternion.Euler(0, rotOffset, 0);
		}

		return Quaternion.identity;
	}

	/// <summary>
	/// Calculate where the player character would be based on its current move speed when the mech takes the next shot.
	/// </summary>
	private void TargetLeadCalculation()
	{
		Vector3 V = playerTransform.GetComponent<CharacterController>().GetVelocity() * Random.Range(leadModifier.x, leadModifier.y);
		Vector3 D = playerTransform.position - transform.position;
		float A = V.sqrMagnitude - Mathf.Pow(14, 2);
		float B = 2 * Vector3.Dot(D, V);
		float C = D.sqrMagnitude;
		if (A >= 0)
		{
			targetPos = playerTransform.position;
		}
		else
		{
			float rt = Mathf.Sqrt(B * B - 4 * A * C);
			float dt1 = (-B + rt) / (2 * A);
			float dt2 = (-B - rt) / (2 * A);
			float dt = (dt1 < 0 ? dt2 : dt1);
			targetPos = playerTransform.position + V * dt;

			Debug.DrawLine(targetPos, targetPos + (Vector3.up * 3), Color.red, 4);
		}
	}



	//Called in animation event
	//Last thing that will be called after this enemy is dead.
	private void OnFall()
	{
		SFXManager.instance.OnMechDie();
		Destroy(this);
	}
}
