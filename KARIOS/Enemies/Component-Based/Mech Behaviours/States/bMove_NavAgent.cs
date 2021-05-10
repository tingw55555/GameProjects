using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Behaviour/Move/NavAgent")]
public class bMove_NavAgent : MoveBehaviour
{
	
	private void OnEnable()
	{
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<Animator>();
	}

	public override void Move()
	{
		agent.SetDestination(targetPos);
		agent.isStopped = false;

		TriggerAnimations();
	}

	

	public override void Stop()
	{
		agent.isStopped = true;
	}
}
