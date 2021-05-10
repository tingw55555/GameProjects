using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterController : MonoBehaviour
{

	[HideInInspector] public CombatType type;
	private Camera cam;
	private Animator anim;
	private Rigidbody rb;
	private GameArea gameArea;
	private GameObject landingMarkInstance;
	private CharacterHealth health;

	private Vector3 targetPos = Vector3.zero;
	private Quaternion finalRot = Quaternion.identity;
	private Vector3 lastPos;

	//Initialized Awake
	private KeyCode Key_Move = KeyCode.Mouse1;
	private KeyCode Key_BasicAttack = KeyCode.Q;
	private KeyCode Key_Skill01 = KeyCode.W;

	[Header("------Character Reference------")]
	//[SerializeField] private SkillNames basicAttack;
	//[SerializeField] private SkillNames skill01;
	//private Skill[] allSkills;
	[SerializeField] private Skill skill_BA;
	[SerializeField] private Skill skill_01;
	[SerializeField] private CharacterStats stats;

	[Header("------General Reference------")]
	public Transform shootPos;
	public CharacterSkillCanvas skillCanvas;
	public GameObject landingMarkPrefab;
	public TrailRenderer trail;
	public LayerMask ground;

	[Header("------Run time Value-------")]
	private bool inControl = true;
	public bool InControl { get { return inControl; } }
	private bool isUsingSkills = false;



	[SerializeField] private bool isStunned = false;

	// Events
	public static event Action<float> CharacterCasting;


	//public float releaseAngle = 10f;
	//public float releasePower = 10f;


	private void Awake()
	{
		inControl = true;
		type = CombatType.Friendly;
		GetReferences();
		InitializeInput();
		SpawnLandingMark();
		SetSkills();
		health.InitializeHealth(stats.maxHealth, type);

	}


	private void InitializeInput()
	{
		Key_Move = InputScheme.instance.Key_Move;
		Key_BasicAttack = InputScheme.instance.Key_BasicAttack;
		Key_Skill01 = InputScheme.instance.Key_Skill01;
	}

	private void GetReferences()
	{
		cam = Camera.main;
		gameArea = FindObjectOfType<GameArea>();
		rb = GetComponent<Rigidbody>();
		anim = GetComponent<Animator>();
		health = GetComponent<CharacterHealth>();
		//allSkills = GetComponents<Skill>();
	}

	private void SetSkills()
	{
		skill_BA.skillType = CharacterKit.BasicAttack;
		skill_01.skillType = CharacterKit.Skill01;
	}

	private void OnEnable()
	{
		MissionObjectiveController.OnMissionAbort += OnMissionAbort;
		skill_01.OnSkillEnd += ResetSkill01;
		skill_BA.OnSkillEnd += Reset_BasicAttack;
	}

	private void OnDisable()
	{
		MissionObjectiveController.OnMissionAbort -= OnMissionAbort;
		skill_01.OnSkillEnd -= ResetSkill01;
		skill_BA.OnSkillEnd -= Reset_BasicAttack;
	}

	private void Update()
	{

		if (inControl)
		{
			if (isStunned) return;

			if (CanReceiveInput())
			{
				Handle_BasicAttack();
				Handle_Skill01();
			}

			Handle_ChoosingMoveDestination();

			Handle_MovingToDestination();

			CheckIfAtDestination();
		}

	}

	private void LateUpdate()
	{
		lastPos = transform.position;
	}



	/// <summary>
	/// Stops reading input should the combat stop.
	/// </summary>
	private void OnMissionAbort()
	{
		inControl = false;
		ToggleAnim_Walk(false);
	}
	private bool CanReceiveInput()
	{
		return !isUsingSkills && inControl;
	}


	/// <summary>
	/// Set character rotation, lock in attack state & trigger character animation.
	/// </summary>
	private void ConfirmBasicAttack()
	{
		ToggleIndicator(false);
		SetPlayerRotation();
		TriggerAnim_BasicAttack();

	}
	private void Handle_BasicAttack()
	{
		if (skill_BA.CanUse)
		{
			if (Input.GetKey(Key_BasicAttack))
			{
				ShowSkillShotUI();
			}
			else if (Input.GetKeyUp(Key_BasicAttack))
			{
				ConfirmBasicAttack();
			}


		}

	}

	//Called in Animation Event 
	public void BasicAttack()
	{
		skill_BA.Execute();
		Reset_BasicAttack();
	}

	private void SpawnLandingMark()
	{
		landingMarkInstance = Instantiate(landingMarkPrefab, transform.position, Quaternion.identity);
		ToggleLandingMark(false);
	}

	public void ToggleLandingMark(bool active)
	{
		landingMarkInstance.SetActive(active);
	}



	public void ToggleIndicator(bool active)
	{
		skillCanvas.Set_IndicatorVisiblity(active);
	}

	public void SetLandingMarkPosition(Vector3 targetPos)
	{
		landingMarkInstance.transform.position = targetPos;
		ToggleLandingMark(true);
	}

	private void Handle_Skill01()
	{
		//Actual Dash function is called in animation event
		if (Input.GetKeyDown(Key_Skill01) && skill_01.CanUse)
		{
			ToggleIndicator(false);
			SetPlayerRotation();
			TriggerAnim_Skill01();
		}
	}

	//Called in Anim event system
	public void UseSkill01()
	{
		StopMoving();
		SetPlayerRotation();
		skill_01.Execute();

	}

	//Called in animation event
	public void ShowCastingTime(float castingTime)
	{
		CharacterCasting?.Invoke(castingTime);
	}

	private void ShowSkillShotUI()
	{
		ToggleIndicator(true);
		ShowSkillShotDirection();
	}

	private void ShowSkillShotDirection()
	{
		Vector3 mousePos = GetMousePosition();
		Vector3 dirToMousePos = (mousePos - transform.position).normalized;
		float angleToMouse = Vector3.SignedAngle(Vector3.forward, dirToMousePos, Vector3.up);

		skillCanvas.Set_IndicatorRot(Quaternion.Euler(90, 0, -angleToMouse));
	}

	private void TriggerAnim_BasicAttack()
	{
		Toggle_isUsingSkill(true);

		ToggleAnim_Walk(false);
		anim.SetTrigger(skill_BA.skillType.ToString());
	}

	private void TriggerAnim_Skill01()
	{
		Toggle_isUsingSkill(true);
		ToggleAnim_Walk(false);
		anim.SetTrigger(skill_01.skillType.ToString());
	}

	private void ToggleAnim_Walk(bool canWalk)
	{
		anim.SetBool("Walk", canWalk);
	}






	private void LaunchProjectile()
	{
		Ray camRay = cam.ScreenPointToRay(GetMousePosition());
		RaycastHit hitInfo;
		if (Physics.Raycast(camRay, out hitInfo, 1000f, ground))
		{
			//landingMarkPrefab.SetActive(true);
			//landingMarkPrefab.transform.position = hitInfo.point;

			//transform.rotation = Quaternion.LookRotation(endingPos);
			if (Input.GetKeyDown(KeyCode.E))
			{

			}
		}
		else
		{
			ToggleLandingMark(false);
		}
	}


	private void SetPlayerRotation()
	{
		transform.rotation = GetRotationToMouse();
	}

	public void SetCharacterControl(bool control)
	{
		inControl = control;
		if (inControl == false)
		{
			ToggleLandingMark(false);
		}
	}


	private void Handle_ChoosingMoveDestination()
	{
		if (Input.GetKey(Key_Move) || Input.GetKeyDown(Key_Move))
		{
			GetTargetPos();
		}
	}

	/// <summary>
	/// Check if the character is close to its stop point. If so, stops walking. 
	/// </summary>
	private void CheckIfAtDestination()
	{
		if (Vector3.Distance(transform.position, targetPos) < 0.001f)
		{
			StopMoving();
			//makes sure that the character snap to their final direction
			transform.rotation = finalRot;
		}
	}

	private void Handle_MovingToDestination()
	{

		if (targetPos != Vector3.zero && !isUsingSkills)
		{
			ToggleAnim_Walk(true);

			MoveToTarget();

			RotateToTarget();

		}
	}


	/// <summary>
	/// Get Rotation To mouse In Screen Space. 
	/// </summary>
	/// <returns></returns>
	private Quaternion GetRotationToMouse()
	{
		Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);
		RaycastHit hitInfo = new RaycastHit();

		Quaternion lookRot = Quaternion.identity;

		if (Physics.Raycast(mouseRay, out hitInfo, 1000, ground))
		{
			Vector3 mousePos = hitInfo.point;
			Vector3 lookDir = (mousePos - transform.position).normalized;
			lookRot = Quaternion.LookRotation(lookDir, Vector3.up);
		}

		return lookRot;
	}

	/// <summary>
	/// Get Rotation To mouse in World Space where camera angle offsets are taken into account. 
	/// </summary>
	/// <returns></returns>
	private Quaternion GetRotationToMouse_CameraAligned()
	{
		Plane firePosPlane = new Plane(Vector3.up, shootPos.position);
		Ray newMouseRay = new Ray(cam.transform.position, GetMousePosition() - cam.transform.position);

		Vector3 shootDir = Vector3.zero;

		if (firePosPlane.Raycast(newMouseRay, out float dist))
		{
			Vector3 adjustedShootPos = newMouseRay.GetPoint(dist);
			//Debug.DrawLine(adjustedShootPos, adjustedShootPos + Vector3.up * 0.5f, Color.yellow, 5);

			shootDir = (adjustedShootPos - shootPos.position).normalized;
		}

		return Quaternion.LookRotation(shootDir, Vector3.up);
	}


	private Vector3 GetMousePosition()
	{
		Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);
		RaycastHit hitInfo = new RaycastHit();
		Vector3 mousePos = Vector3.zero;

		if (Physics.Raycast(mouseRay, out hitInfo, 1000, ground))
		{
			mousePos = hitInfo.point;
		}

		return mousePos;
	}

	/// <summary>
	/// Get the position of the mouse within game bounds and set it to the new character destination. 
	/// </summary>
	private void GetTargetPos()
	{
		if (isUsingSkills) return;
		targetPos = gameArea.bounds.ClosestPoint(GetMousePosition());

		SetLandingMarkPosition(targetPos);
	}

	/// <summary>
	/// Rotate the character to the destination. 
	/// </summary>
	private void RotateToTarget()
	{
		float rotStep = stats.rotSpeed * Time.deltaTime;
		Vector3 dir = targetPos - transform.position;
		Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);

		if (finalRot != lookRot && lookRot != Quaternion.identity)
		{
			finalRot = lookRot;
		}

		transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotStep);
	}

	/// <summary>
	/// Translate the character to the destination. 
	/// </summary>
	private void MoveToTarget()
	{
		float moveStep = stats.moveSpeed * Time.deltaTime;
		transform.position = Vector3.MoveTowards(transform.position, targetPos, moveStep);
	}

	/// <summary>
	/// Translate the character to a spefic position with a specific speed. 
	/// </summary>
	private void MoveToTarget(float speed, Vector3 endPos)
	{
		float moveStep = speed * Time.deltaTime;
		transform.position = Vector3.MoveTowards(transform.position, endPos, moveStep);
	}

	private void StopMoving()
	{
		targetPos = Vector3.zero;
		ToggleAnim_Walk(false);
		ToggleLandingMark(false);
	}

	private void ResetCharacterState()
	{
		Toggle_isUsingSkill(false);
		ResetTriggers();
	}

	private void ResetTriggers()
	{
		anim.ResetTrigger(skill_BA.GetSkillType().ToString());
		anim.ResetTrigger(skill_01.GetSkillType().ToString());
	}

	//Callled in animation event 
	public void ResetSkill01()
	{
		Toggle_isUsingSkill(false);
		anim.ResetTrigger(skill_01.GetSkillType().ToString());
	}

	//Callled in animation event 
	public void Reset_BasicAttack()
	{
		Toggle_isUsingSkill(false);
		anim.ResetTrigger(skill_BA.GetSkillType().ToString());
	}

	public Vector3 GetVelocity() => (transform.position - lastPos) / Time.deltaTime;

	private void Toggle_isUsingSkill(bool active) => isUsingSkills = active;



	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.GetComponent<Wall>())
		{
			targetPos = Vector3.zero;
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (collision.gameObject.GetComponent<Wall>())
		{
			targetPos = Vector3.zero;
			ToggleAnim_Walk(false);
		}
	}

	public void PlayFootStep()
	{
		SFXManager.instance.PlayerFootstep();
	}

	//Called in animation event
	private void ResetStun()
	{
		SetCharacterControl(true);
		isStunned = false;
	}

	//Called in animation event
	public void HitTheGround()
	{
		SFXManager.instance.PlayerStunned();
	}

}
