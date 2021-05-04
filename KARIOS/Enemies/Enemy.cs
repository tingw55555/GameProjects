using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
	Mech = 0,
	FireBug = 1,
	OminTank = 2,
}



public class Enemy : Unit
{
	protected Collider co;
	protected HealthBar hpBar;
	public EnemyType enemyType;

	[Header("Unit Stats")]
	public float maxHealth = 100;
	[HideInInspector] public float currentHealth = 100;


	[HideInInspector] public bool isDead = false;
	public Transform playerPos;
	public SkinnedMeshRenderer[] bodyMr;

	public static event Action<Transform> OnUnitDie;

	protected override void Awake()
	{
		base.Awake();
		InitializeEnemy();
	}

	private void InitializeEnemy()
	{
		hpBar = GetComponent<HealthBar>();
		co = GetComponent<Collider>();

		bodyMr = GetComponentsInChildren<SkinnedMeshRenderer>();
	}

	protected virtual void Start()
	{
		currentHealth = maxHealth;

	}

	protected virtual void OnEnable()
	{
		type = UnitType.Hostile;
		Character.OnPlayerDies += OnPlayerDies;
		MissionObjectiveController.OnMissionAbort += OnPlayerDies;
	}


	protected virtual void OnDisable()
	{

		Character.OnPlayerDies -= OnPlayerDies;
		MissionObjectiveController.OnMissionAbort -= OnPlayerDies;
	}


	//Overriden in specific enemy classes
	protected virtual void OnPlayerDies()
	{

	}

	//The game currently doesnt use patrols 
	public virtual void OnInit(List<Transform> patrolPoints)
	{

	}

	/// <summary>
	/// Enemies get player transform from enemy spawner
	/// </summary>
	/// <param name="target"></param>
	public virtual void OnInit(Transform target)
	{
		playerPos = target;
	}

	public override void TakeDamage(float damage)
	{
		base.TakeDamage(damage);

		if (isDead) return;

		currentHealth -= damage;

		if (hpBar != null)
		{
			hpBar.UpdateHealthBar(maxHealth, currentHealth);
		}

		if (currentHealth <= 0)
		{
			Die();
		}

	}

	protected virtual void Die()
	{
		OnUnitDie?.Invoke(transform);
		co.enabled = false;
		isDead = true;

		foreach (SkinnedMeshRenderer mr in bodyMr)
		{
			mr.material.SetFloat("_OtlWidth", 0);
		}
	}


}
