using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;


public class Missile : BaseUnit
{

	public static event Action<int> OnEnemyJetDestroyed;

	[HideInInspector]
	public NavMeshAgent navAgent;
	[HideInInspector]
	public MissileCommand mc;
	[HideInInspector]
	public Collider col;

	private Transform target;
	public GameObject explosionPrefab;
	public int nationalPowerMod = 10;

	[SerializeField]
	private AudioSource aS;

	public AudioClip launchSound;

	private void OnEnable()
	{
		//PathMover.OnPathpointsReceived += DeterminedVaildPath;
		navAgent = GetComponent<NavMeshAgent>();
		col = GetComponent<Collider>();
		
		//So that our AC cant push it around
		col.enabled = false;
		navAgent.enabled = false;

		transform.position = new Vector3(transform.position.x, 0, transform.position.z);

	}



	private void OnDestroy()
	{
		if (mc != null)
		{
			mc.OnMissileDestroyed();
		}
	}


	public void FollowTarget(Transform targetToFollow)
	{
		target = targetToFollow;
		StartCoroutine(ChaseEnemyCoroutine());

	}

	public void EnableColision()
	{
		col.enabled = true;
		navAgent.enabled = true;
		
	}

	IEnumerator ChaseEnemyCoroutine()
	{
		aS.PlayOneShot(launchSound);
		EnableColision();

		while (true)
		{
			if (target.gameObject != null || target.gameObject.activeSelf != false)
			{
				navAgent.SetDestination(target.position);
			}
			else
			{
				Destroy(gameObject);
			}

			yield return new WaitForFixedUpdate();
		}
	}


	private void OnCollisionEnter(Collision collision)
	{
		FighterJet enemy = collision.gameObject.GetComponent<FighterJet>();

		if (enemy != null)
		{
			OnEnemyJetDestroyed?.Invoke(nationalPowerMod);
			Instantiate(explosionPrefab, collision.transform.position, transform.rotation);
			enemy.Alertbase();
			StopAllCoroutines();
			Destroy(gameObject);
			Destroy(enemy.gameObject);
		}
	}



}
