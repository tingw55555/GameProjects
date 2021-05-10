using UnityEngine;

public class bWarning_Default : WarningBehaviour
{
	[Header("References")]
	public GameObject warningEffect;
	public Vector3 warningEffectOffset = Vector3.zero;

	public override void Alert(Vector3 warningPos)
	{
		if (warningEffect != null)
		{
			GameObjectUtil.Instantiate(warningEffect, warningPos + warningEffectOffset, Quaternion.identity);
		}

		if (anim != null)
		{
			TriggerAnimations();
		}

	}
	private void OnEnable()
	{
		anim = GetComponent<Animator>();
	}

}
