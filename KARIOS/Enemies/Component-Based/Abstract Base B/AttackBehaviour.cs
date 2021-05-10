using UnityEngine;

public abstract class AttackBehaviour : BaseBehaviour
{	
	[Header("References")]
	public Transform firePos;
	public GameObject attackPrefab;
	public GameObject muzzlePrefab;

	[Header("Stats")]
	public float damage = 50;


    public abstract void Attack();


	public virtual void BasicAttack()
	{
		Projectile bullet = GameObjectUtil.Instantiate(attackPrefab, firePos.position, transform.rotation).GetComponent<Projectile>();
		bullet.SetDamage(damage);
		bullet.SetType(CombatType.Hostile);
	}

	public virtual void ShowMuzzleFlash()
	{
		GameObjectUtil.Instantiate(muzzlePrefab, firePos.position, transform.rotation);
	}
}
