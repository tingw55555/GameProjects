using UnityEngine;
using System;
using System.Collections;

public class Character : Unit
{
	private LocomotionController loco;
	private Animator anim;
	private Rigidbody rb;

	[Header("General Reference")]
	public GameObject landingMarkPrefab;
	[HideInInspector] public GameObject landingMark;

	[Header("Skill Particles")]
	public GameObject timeBomb;
	public GameObject dash;

	[Header("Stats")]
	public float maxHealth = 1125;
	public FloatObject healthObject;
	public float lowHealthThreshold = 0.35f;
	[Space(5)]
	public float dashTime = 0.6f;
	public float dashDistance = 3f;
	public float dashSpeed = 50;

	public bool isStunned = false;

	
	// Events
	public static event Action OnPlayerDies;
	public static event Action<float> OnPlayerTakeDamage;
	public static event Action<float> OnPlayerHeal;



	protected override void Awake()
	{
		base.Awake();

		InitializeCharacter();

		SpawnLandingMark();

	}

	

	private void Start()
	{
		InitializeHealth();
	}

	private void OnEnable()
	{
		loco.Confirm_BasicAttack += TriggerAnim_BasicAttack;
		loco.Confirm_Dash += TriggerAnim_Dash;
		loco.change_WalkState += TriggerAnim_Walking;
	}

	private void OnDisable()
	{
		loco.Confirm_BasicAttack -= TriggerAnim_BasicAttack;
		loco.Confirm_Dash -= TriggerAnim_Dash;
		loco.change_WalkState -= TriggerAnim_Walking;
	}
	
	private void SpawnLandingMark()
	{
		landingMark = Instantiate(landingMarkPrefab, transform.position, Quaternion.identity);
		landingMark.SetActive(false);
	}

	private void InitializeCharacter()
	{
		anim = GetComponent<Animator>();
		loco = GetComponent<LocomotionController>();
		rb = GetComponent<Rigidbody>();
	}

	/// <summary>
	/// Make sure character's current health is set to max when combat starts.
	/// I'm using a health objects in case I want to reference it from multiple places
	/// </summary>
	public void InitializeHealth()
	{
		healthObject.MaxValue = maxHealth;
		healthObject.Value = maxHealth;
	}

	public void SetLandingMarkPosition(Vector3 targetPos)
	{
		landingMark.transform.position = targetPos;
		landingMark.SetActive(true);
	}

	public void TriggerAnim_Idle()
	{
		TriggerAnim_Walking(false);
	}

	private void TriggerAnim_Dash()
	{
		TriggerAnim_Walking(false);
		anim.SetTrigger("Dash");
	}

	public void TriggerAnim_Walk()
	{
		anim.SetBool("Walk", true);
	}

	private void TriggerAnim_BasicAttack()
	{
		TriggerAnim_Walking(false);
		anim.SetTrigger("Attack");
	}

	private void TriggerAnim_Walking(bool canWalk)
	{
		anim.SetBool("Walk", canWalk);
	}

	public void ResetTriggers()
	{
		anim.ResetTrigger("Attack");
		anim.ResetTrigger("Dash");
	}

	private void TriggerAnim_Stunned(bool isStunned)
	{
		loco.SetCharacterControl(false);
		TriggerAnim_Walking(false);
		anim.SetBool("IsStunned", isStunned);
	}

	protected override void BasicAttack()
	{
		base.BasicAttack();
		loco.ResetAttack();

	}

	/// <summary>
	/// Character will heal after touching the healing packs. 
	/// It also shows feedback
	/// </summary>
	/// <param name="healAmount"></param>
	public void Heal(float healAmount)
	{
		healthObject.Value += healAmount;
		healthObject.Value = Mathf.Clamp(healthObject.Value, 0, maxHealth);

		MovingText movingText = Instantiate(damageText, transform.position, Quaternion.identity).GetComponent<MovingText>();
		movingText.SetText(healAmount, true);
		
		//Notify the UI listener to show heal amount on screen
		OnPlayerHeal?.Invoke(healAmount);
	}


	public override void TakeDamage(float damage)
	{
		base.TakeDamage(damage);

		healthObject.Value -= damage;

		OnPlayerTakeDamage?.Invoke(damage);

		if (healthObject.Value <= 0)
		{
			OnPlayerDies?.Invoke();
			Destroy(gameObject);
		}
	}


	public override void Stunned(float timeFrozen)
	{
		isStunned = true;
		TriggerAnim_Stunned(true);

		MovingText movingText = Instantiate(damageText, transform.position, Quaternion.identity).GetComponent<MovingText>();
		movingText.SetText("Stunned");

		StartCoroutine(FreezingCoroutine(timeFrozen));
	}

	IEnumerator FreezingCoroutine(float timeFrozen)
	{
		float currentTime = 0;
		float formerSpeed = moveSpeed;
		moveSpeed = 0;

		while (currentTime < 1)
		{
			yield return null;
			currentTime += Time.deltaTime / timeFrozen;

		}
		moveSpeed = formerSpeed;
		TriggerAnim_Stunned(false);

	}

	//Called in animation event
	private void ResetStun()
	{
		loco.SetCharacterControl(true);
		isStunned = false;
	}

	//Called in animation event
	public void HitTheGround()
	{
		SFXManager.instance.PlayerStunned();
	}



}
