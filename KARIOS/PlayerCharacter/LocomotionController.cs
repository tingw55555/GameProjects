using System;
using System.Collections;
using UnityEngine;


public class LocomotionController : MonoBehaviour
{
	private Character ch;
	private Camera cam;
	private Rigidbody rb;
	public GameArea gameArea;

	[Header("Run time Value")]
	[HideInInspector] public Vector3 targetPos = Vector3.zero;
	[HideInInspector] public Quaternion finalRot = Quaternion.identity;
	private Vector3 lastPos;
	public bool isAttacking = false;
	public bool isDashing = false;
	[SerializeField] private bool inControl = true;
	public bool InControl { get { return InControl; } }

	[Header("Character Inputs")]
	public KeyCode basicAttack = KeyCode.Q;
	public KeyCode MoveSkill = KeyCode.W;

	[Header("References")]
	public LayerMask ground;
	public TrailRenderer trail;
	public CharacterSkillCanvas skillCanvas;

	public static event Action<float> CharacterCasting;

	public event Action Confirm_BasicAttack;
	public event Action Confirm_Dash;
	public event Action<bool> change_WalkState;

	//public float releaseAngle = 10f;
	//public float releasePower = 10f;


	private void Awake()
	{
		InitializeLocomotionController();
	}

	private void InitializeLocomotionController()
	{
		cam = Camera.main;
		inControl = true;
		gameArea = FindObjectOfType<GameArea>();
		ch = GetComponent<Character>();
		rb = GetComponent<Rigidbody>();
	}

	private void OnEnable()
	{
		MissionObjectiveController.OnMissionAbort += OnMissionAbort;
	}

	private void OnDisable()
	{
		MissionObjectiveController.OnMissionAbort -= OnMissionAbort;

	}

	private void Update()
	{

		if (inControl)
		{
			if (ch.isStunned) return;

			Handle_BasicAttack();

			//LaunchProjectile();

			Handle_ChoosingMoveDestination();

			Handle_Dash();

			Handle_MovingToDestination();

			CheckIfAtDestination();
		}

	}

	private void LateUpdate()
	{
		lastPos = transform.position;
	}

	public Vector3 GetVelocity() => (transform.position - lastPos) / Time.deltaTime;

	/// <summary>
	/// Stops reading input should the combat stop.
	/// </summary>
	private void OnMissionAbort()
	{
		inControl = false;
		ch.TriggerAnim_Idle();
	}

	private void Handle_Dash()
	{
		//Actual Dash function is called in animation event
		if (Input.GetKeyDown(KeyCode.W))
		{
			if (isAttacking || isDashing) return;
			SetPlayerRotation();
			LockDash();
			Confirm_Dash?.Invoke();
		}
	}


	private void Handle_BasicAttack()
	{
		if (Input.GetKey(KeyCode.Q))
		{
			ShowSkillShotUI();

		}
		else if (Input.GetKeyUp(KeyCode.Q) && !isDashing)
		{
			ConfirmBasicAttack();
		}


	}

	/// <summary>
	/// Set character rotation, lock in attack state & trigger character animation.
	/// </summary>
	private void ConfirmBasicAttack()
	{
		skillCanvas.SetBAIndicator_Visiblity(false);

		if (!isAttacking)
		{
			SetPlayerRotation();
			LockAttack();
			Confirm_BasicAttack?.Invoke();
		}
	}


	//Called in Anim event system
	public void Dash()
	{
		skillCanvas.SetBAIndicator_Visiblity(false);
		StartCoroutine(DashCoroutine());
	}


	IEnumerator DashCoroutine()
	{
		StopMoving();
		SetPlayerRotation();
		//To get a point in the distance with look rotation & distance 
		//Vector3 endPoint = gameArea.bounds.ClosestPoint(GetRotationToMouse() * (transform.forward * ch.dashDistance) + transform.position) ;
		Vector3 endPoint = gameArea.bounds.ClosestPoint((transform.forward * ch.dashDistance) + transform.position);

		if (SFXManager.instance != null) SFXManager.instance.PlayerDash();

		float startTime = Time.time;
		while (Time.time < startTime + ch.dashTime)
		{
			MoveToTarget(ch.dashSpeed, endPoint);

			yield return null;
		}

	}

	//Called in animation event
	public void ShowCastingTime(float castingTime)
	{
		CharacterCasting?.Invoke(castingTime);
	}


	private void ShowSkillShotUI()
	{
		skillCanvas.SetBAIndicator_Visiblity(true);
		ShowSkillShotDirection();
	}


	private void ShowSkillShotDirection()
	{
		Vector3 mousePos = GetMousePosition();
		Vector3 dirToMousePos = (mousePos - transform.position).normalized;
		float angleToMouse = Vector3.SignedAngle(Vector3.forward, dirToMousePos, Vector3.up);

		skillCanvas.SetBAIndicator_Rotation(Quaternion.Euler(90, 0, -angleToMouse));
	}



	private void LaunchProjectile()
	{
		Ray camRay = cam.ScreenPointToRay(GetMousePosition());
		RaycastHit hitInfo;
		if (Physics.Raycast(camRay, out hitInfo, 1000f, ground))
		{
			ch.landingMarkPrefab.SetActive(true);
			ch.landingMarkPrefab.transform.position = hitInfo.point;

			//transform.rotation = Quaternion.LookRotation(endingPos);
			if (Input.GetKeyDown(KeyCode.E))
			{

			}
		}
		else
		{
			ch.landingMarkPrefab.SetActive(false);
		}
	}


	private void SetPlayerRotation()
	{
		transform.rotation = GetRotationToMouse();
	}


	private void LockDash()
	{
		isDashing = true;
		trail.enabled = true;

	}

	public void SetCharacterControl(bool control)
	{
		inControl = control;
		if (inControl == false)
		{
			ch.landingMark.SetActive(false);
		}
	}


	private void LockAttack()
	{
		isAttacking = true;
	}

	private void Handle_ChoosingMoveDestination()
	{
		if (Input.GetMouseButtonDown(1) || Input.GetMouseButton(1))
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

		if (targetPos != Vector3.zero && !isAttacking)
		{
			ch.TriggerAnim_Walk();

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
	public Quaternion GetRotationToMouse_CameraAligned()
	{
		Plane firePosPlane = new Plane(Vector3.up, ch.firePos.position);
		Ray newMouseRay = new Ray(cam.transform.position, GetMousePosition() - cam.transform.position);

		Vector3 shootDir = Vector3.zero;

		if (firePosPlane.Raycast(newMouseRay, out float dist))
		{
			Vector3 adjustedShootPos = newMouseRay.GetPoint(dist);
			//Debug.DrawLine(adjustedShootPos, adjustedShootPos + Vector3.up * 0.5f, Color.yellow, 5);

			shootDir = (adjustedShootPos - ch.firePos.position).normalized;
		}

		return Quaternion.LookRotation(shootDir, Vector3.up);
	}


	public Vector3 GetMousePosition()
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
		if (isAttacking) return;
		targetPos = gameArea.bounds.ClosestPoint(GetMousePosition());

		ch.SetLandingMarkPosition(targetPos);
	}

	/// <summary>
	/// Rotate the character to the destination. 
	/// </summary>
	private void RotateToTarget()
	{
		float rotStep = ch.rotSpeed * Time.deltaTime;
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
	public void MoveToTarget()
	{
		float moveStep = ch.moveSpeed * Time.deltaTime;
		transform.position = Vector3.MoveTowards(transform.position, targetPos, moveStep);
	}

	/// <summary>
	/// Translate the character to a spefic position with a specific speed. 
	/// </summary>
	public void MoveToTarget(float speed, Vector3 endPos)
	{
		float moveStep = speed * Time.deltaTime;
		transform.position = Vector3.MoveTowards(transform.position, endPos, moveStep);
	}

	public void StopMoving()
	{
		change_WalkState?.Invoke(false);
		targetPos = Vector3.zero;

		ch.landingMark.SetActive(false);
	}

	public void ResetAttack()
	{
		isAttacking = false;
		isDashing = false;
		ch.ResetTriggers();
	}

	public void ResetDash()
	{
		skillCanvas.SetBAIndicator_Visiblity(false);
		isDashing = false;
		isAttacking = false;
		trail.enabled = false;
		ch.ResetTriggers();
	}


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
			change_WalkState?.Invoke(false);
		}
	}



}
