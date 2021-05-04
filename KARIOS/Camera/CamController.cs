using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{

	public bool isTesting = true;

	[Header("Reference")]
	public MapBounds mapArea;
	
	[Header("Panning Settings")]
	public bool enableEdgePan;
	public float mousePanSpeed = 0.5f;

	[Header("Following Settings")]
	public bool canSnap = true;
	public Transform characterToFollow;
	public Vector3 offset;


	public event Action OnFinishedMovingToPlayer;

	private void Awake()
	{
		MissionObjectiveController.OnPlayerSpawned += GetPlayerReference;
	}
	private void Start()
	{
		if (!isTesting)
		{
			Cursor.lockState = CursorLockMode.Confined;

		}

	}


	private void OnDisable()
	{
		MissionObjectiveController.OnPlayerSpawned -= GetPlayerReference;
	}

	private void GetPlayerReference(GameObject playerPos)
	{
		characterToFollow = playerPos.transform;
		SnapToPlayer();
	}

	// Update is called once per frame
	void Update()
	{
		Pan();
		Handle_FollowPlayer();

	}

	/// <summary>
	/// Listens to the input to snap camera To Player location. 
	/// It needs an initial location to work. 
	/// </summary>
	private void Handle_FollowPlayer()
	{
		if (Input.GetKey(KeyCode.Space) && canSnap)
		{
			SnapToPlayer();
		}
	}

	void Pan()
	{

		if (enableEdgePan)
		{
			float x = ((Input.mousePosition.x / Screen.width) * 2) - 1;
			float y = ((Input.mousePosition.y / Screen.height) * 2) - 1;

			//Pan by mouse on sides
			if (x < -0.8f || x > 0.8f || y < -0.8f || y > 0.8f)
			{
				//Vector3 direction = (new Vector3(x, 0, y)).normalized;
				Vector3 dir = (transform.right * x) + (transform.up * y);
				dir = Vector3.ProjectOnPlane(dir.normalized, Vector3.up);

				Debug.DrawLine(transform.position, transform.position + (dir * 5), Color.red);

				Vector3 newPos = transform.position + (dir * mousePanSpeed * Time.deltaTime);

				//We clamp the camera to stay inside the bounds at all times
				if (mapArea.bounds.Contains(newPos))
				{
					transform.position = newPos;
				}
			}

		}



		
	}

	public void SnapToPlayer()
	{
		if (characterToFollow == null) return;
		transform.position = GetPlayerPos();
	}

	/// <summary>
	/// Get Player position inside of the camera bounds area.
	/// </summary>
	/// <returns></returns>
	private Vector3 GetPlayerPos()
	{
		return mapArea.bounds.ClosestPoint(characterToFollow.position + offset);
	}


	/// <summary>
	/// Take over the camera and use coroutine to move it to focus on the Player. 
	/// </summary>
	/// <param name="duration"></param>
	public void MoveToPlayer(float duration)
	{
		StartCoroutine(MoveCoroutine(duration));
	}

	IEnumerator MoveCoroutine(float duration)
	{
		canSnap = false;
		float currentTime = 0;
		Vector3 startPos = transform.position;
		Vector3 endPos = GetPlayerPos();

		while (currentTime < 1)
		{
			transform.position = Vector3.Lerp(startPos, endPos, currentTime);

			yield return null;

			currentTime += Time.deltaTime / duration;
		}

		SnapToPlayer();
		OnFinishedMovingToPlayer?.Invoke();
		canSnap = true;

	}


}
