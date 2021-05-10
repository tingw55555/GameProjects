using UnityEngine;
using UnityEngine.AI;

public abstract class BaseBehaviour : MonoBehaviour
{
	//References
	protected Animator anim;
	protected NavMeshAgent agent;
	protected Collider co;

	[Header("Animation Settings")]
	public string[] trueAnimBool = new string[1];
	public string[] falseAnimBool = new string[1];

	public void TriggerAnimations()
	{
		if (trueAnimBool.Length > 0)
		{
			for (int i = 0; i < trueAnimBool.Length; i++)
			{
				anim.SetBool(trueAnimBool[i], true);
			}
		}

		if (falseAnimBool.Length > 0)
		{
			for (int i = 0; i < falseAnimBool.Length; i++)
			{
				anim.SetBool(falseAnimBool[i], false);
			}
		}
	}

	public void DeactivateTrueAnim()
	{
		if (trueAnimBool.Length > 0)
		{
			for (int i = 0; i < trueAnimBool.Length; i++)
			{
				anim.SetBool(trueAnimBool[i], false);
			}
		}
	}

	public void ActivateFalseAnim()
	{
		if (falseAnimBool.Length > 0)
		{
			for (int i = 0; i < falseAnimBool.Length; i++)
			{
				anim.SetBool(falseAnimBool[i], true);
			}
		}
	}
}
