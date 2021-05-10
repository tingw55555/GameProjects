using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class bDie_NavAgent : DieBehaviour
{
	public override void Die()
	{
		co.enabled = false;

		//Disable Pathing
		agent.isStopped = true;
		agent.enabled = false;

		TriggerAnimations();

	}


	private void OnEnable()
	{
		co = GetComponent<Collider>();
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();
	}

}
