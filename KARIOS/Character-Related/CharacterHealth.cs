using System;
using UnityEngine;

public class CharacterHealth : MonoBehaviour, iDamagable
{
	[SerializeField] private FloatObject healthObject;
	[SerializeField] private GameObject damageText;

	private CombatType type;
	public static event Action OnPlayerDies;
	/// <summary>
	/// Make sure character's current health is set to max when combat starts.
	/// I'm using a health objects in case I want to reference it from multiple places
	/// </summary>
	public void InitializeHealth(float max, CombatType type)
	{
		healthObject.UpdateBothValues(max, max);
	}

	private MovingText SpawnMovingText()
	{
		return Instantiate(damageText, transform.position, Quaternion.identity).GetComponent<MovingText>();
	}

	/// <summary>
	/// Check if the victim is the same type as the aggressor. 
	/// Will return false if that's the case. 
	/// </summary>
	/// <param name="incomingAttackType"></param>
	/// <returns></returns>
	public bool CanDamage(CombatType incomingAttackType)
	{
		if (incomingAttackType == type) return false;

		return true;
	}

	/// <summary>
	/// Called by Others. Will Show feedback. Will trigger death if no health
	/// </summary>
	/// <param name="damage"></param>
	public void TakeDamage(float damage)
	{
		if (damageText) SpawnMovingText().SetText(damage);

		healthObject.Reduce(damage);

		if (healthObject.Value <= 0)
		{
			OnPlayerDies?.Invoke();
			Destroy(gameObject);
		}
	}

	/// <summary>
	/// Called by others.
	/// Character will heal after touching the healing packs. 
	/// It also shows feedback
	/// </summary>
	/// <param name="healAmount"></param>
	public void Heal(float healAmount)
	{
		healthObject.Add(healAmount);
		healthObject.Value = Mathf.Clamp(healthObject.Value, 0, healthObject.MaxValue);
		SpawnMovingText().SetText(healAmount, true);

	}



}
