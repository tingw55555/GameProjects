using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chronos;

public enum UnitType
{
	Protagonist,
	Hostile,

}


public class Unit : MonoBehaviour
{

	public UnitType type;

	[Header("References")]
	public Transform firePos;
	public GameObject damageText;

	[Header("Particles")]
	public GameObject basicAttackPrefab;
	public GameObject muzzlePrefab;

	[Header("Base Stats")]
	public float moveSpeed = 4.5f;
	public float rotSpeed = 20f;



	protected virtual void Awake()
	{

	}

	//Called In animation event
	protected virtual void BasicAttack()
	{
		DamageParticle bullet = GameObjectUtil.Instantiate(basicAttackPrefab, firePos.position, transform.rotation).GetComponent<DamageParticle>();
		bullet.SetOwner(this);
	}

	//Called In animation event
	protected virtual void ShowMuzzleFlash()
	{
		GameObjectUtil.Instantiate(muzzlePrefab, firePos.position, transform.rotation);
	}

	//Called In animation event
	protected virtual void ShowMuzzleFlash(Vector3 pos)
	{
		GameObjectUtil.Instantiate(muzzlePrefab, pos, transform.rotation);
	}


	//Overriden by character and enemy types
	public virtual void TakeDamage(float damage)
	{
		if (damageText)
		{
			MovingText dmgText = Instantiate(damageText, transform.position, Quaternion.identity).GetComponent<MovingText>();
			dmgText.SetText(damage);
		}
	}

	//Only used by player character right now
	public virtual void Stunned(float timeFrozen)
	{

	}






}
