using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIBehaviourSystem : MonoBehaviour, iDamagable
{
	[HideInInspector] public CombatType combatType;
	public EnemyType enemyType;

	protected NavMeshAgent agent;
	protected Animator anim;
	protected Collider co;


	//Behaviours
	protected MoveBehaviour bMove;
	protected WarningBehaviour bWarning;
	protected AttackBehaviour bAttack;
	protected RecoverBehaviour bRecover;
	protected DieBehaviour bDie;

	[Header("Reference")]
	public GameObject damageText;
	protected HealthBar hpBar;

	[Header("Unit Settings")]
	public float currentHealth = 100;
	public float maxHealth = 100;

	[Header("RunTime Value")]
	public Transform playerTransform;
	public bool isDead = false;
	public bool isAttacking = false;
	public bool isActive = true;

	public static event Action<Transform> OnUnitDie;



	protected virtual void Awake()
	{
		combatType = CombatType.Hostile;
		GetAllComponents();
		Initialize_AllBehaviours();
	}

	protected virtual void OnEnable()
	{
		CharacterHealth.OnPlayerDies -= OnPlayerDies;
	}

	protected virtual void OnDisable()
	{
		CharacterHealth.OnPlayerDies -= OnPlayerDies;
	}

	//Overriden in specific enemy classes
	protected virtual void OnPlayerDies()
	{
		isActive = false;

	}

	protected void GetAllComponents()
	{
		co = GetComponent<Collider>();
		hpBar = GetComponentInChildren<HealthBar>();

		//Optional
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();
	}


	protected void Initialize_AllBehaviours()
	{
		bMove = GetComponent<MoveBehaviour>();
		bWarning = GetComponent<WarningBehaviour>();
		bAttack = GetComponent<AttackBehaviour>();
		bRecover = GetComponent<RecoverBehaviour>();
		bDie = GetComponent<DieBehaviour>();

	}

	protected virtual void Start()
	{
		currentHealth = maxHealth;
		StartWalking();
	}

	protected virtual void StartWalking()
	{

	}

	protected virtual void Update()
	{
		CheckingForNextBehaviour();
	}

	protected virtual void CheckingForNextBehaviour()
	{
		if (isDead) return;

		if (!isActive) return;

	}


	public bool CanDamage(CombatType incomingAttackType)
	{
		if (incomingAttackType == combatType) return false;

		return true;
	}

	public virtual void TakeDamage(float damage)
	{
	
		MovingText dmgText = Instantiate(damageText, transform.position, Quaternion.identity).GetComponent<MovingText>();
		dmgText.SetText(damage);

		if (isDead) return;

		currentHealth -= damage;
		hpBar.UpdateHealthBar(maxHealth, currentHealth);

		if (currentHealth <= 0) Die();
	}

	protected virtual void Die()
	{
		OnUnitDie?.Invoke(transform);
		isDead = true;
		isActive = false;
		bDie.Die();

	}

	protected void DestroyAllBehaviours()
	{
		Destroy(bMove);
		Destroy(bWarning);
		Destroy(bAttack);
		Destroy(bRecover);
		Destroy(bDie);
	}

	


}
	
