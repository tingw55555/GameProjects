using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : Skill
{
	private GameArea gameArea;

	[Header("---------Stats--------")]
	public float dashDistance = 5;
	public float dashTime = 0.3f;
	public float dashSpeed = 15;

	[Space(10)]
	public TrailRenderer trail;

	private void Awake()
	{
		GameArea.GetGameArea += GetGameArea;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	protected void OnDisable()
	{
		GameArea.GetGameArea -= GetGameArea;
	}

	public override void Execute()
	{
		ToggleTrail(true);
		StartCoroutine(DashCoroutine());
	}


	IEnumerator DashCoroutine()
	{

		//To get a point in the distance with look rotation & distance 
		//Vector3 endPoint = gameArea.bounds.ClosestPoint(GetRotationToMouse() * (transform.forward * ch.dashDistance) + transform.position) ;
		Vector3 endPoint = gameArea.bounds.ClosestPoint((transform.forward * dashDistance) + transform.position);

		if (SFXManager.instance != null) SFXManager.instance.PlayerDash();

		float startTime = Time.time;
		while (Time.time < startTime + dashTime)
		{
			MoveToTarget(dashSpeed, endPoint);
			yield return null;
		}

		ToggleTrail(false);
		StartCoroutine(CoolDownCoroutine());
		NotifyOnSkillEnd();
	}

	/// <summary>
	/// Translate the character to a spefic position with a specific speed. 
	/// </summary>
	private void MoveToTarget(float speed, Vector3 endPos)
	{
		float moveStep = speed * Time.deltaTime;
		transform.position = Vector3.MoveTowards(transform.position, endPos, moveStep);
	}

	public void ToggleTrail(bool active)
	{
		trail.enabled = active;
	}

	private void GetGameArea(GameArea area)
	{
		gameArea = area;
	}

}
