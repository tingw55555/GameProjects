using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bAttack_AnimToShoot : AttackBehaviour
{
	public override void Attack()
	{
		TriggerAnimations();
	}

	public void Stop()
	{
		DeactivateTrueAnim();
	}

	private void OnEnable()
	{
		anim = GetComponent<Animator>();
	}
}
