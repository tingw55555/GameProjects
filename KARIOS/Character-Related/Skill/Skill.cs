using System;
using System.Collections;
using UnityEngine;

public enum SkillNames
{
	ArcaneEssence = 0,
	TemporalWarp = 1,

}


public class Skill : MonoBehaviour
{

	[Header("------Cool Down Object--------")]
	public FloatObject cdObject;

	[Header("------Settings----------")]
	public float coolDown = 0.8f;
	public float damage = 60;
	[HideInInspector]public CharacterKit skillType;

	private bool _canUse;
	public bool CanUse { get { return _canUse; } set { _canUse = value; } }

	public event Action OnSkillEnd;

	public virtual void Execute() { }

	public virtual void ShowMuzzleFlash() { }

	protected IEnumerator CoolDownCoroutine()
	{
		_canUse = false;
		cdObject.Value = 0;
		while (cdObject.Value < 1)
		{
			yield return null;
			cdObject.Value += Time.deltaTime / cdObject.MaxValue;
		}

		cdObject.Value = 1;
		_canUse = true;
	}


	public CharacterKit GetSkillType()
	{
		return skillType;
	}

	protected void InitializeCoolDown()
	{
		cdObject.MaxValue = coolDown;
		cdObject.Value = 1;
		_canUse = true;

	}

	protected virtual void OnEnable()
	{
		InitializeCoolDown();
	}

	protected void NotifyOnSkillEnd()
	{
		OnSkillEnd?.Invoke();
	}



}
