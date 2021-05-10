using UnityEngine;


public class SingleShot : Skill
{
	[Header("---------Reference---------")]
	public GameObject bulletEffect;
	public GameObject muzzleEffect;
	public Transform shootTransform;


	protected override void OnEnable()
	{
		base.OnEnable();
	}

	public override void Execute()
	{
		Projectile bullet = GameObjectUtil.Instantiate(bulletEffect, shootTransform.position,transform.rotation).GetComponent<Projectile>();
		bullet.SetDamage(damage);
		bullet.SetType(CombatType.Friendly);
		StartCoroutine(CoolDownCoroutine());
	}

	//Called in Animation Event
	public override void ShowMuzzleFlash()
	{
		GameObjectUtil.Instantiate(muzzleEffect, shootTransform.position, transform.rotation);
	}
}
