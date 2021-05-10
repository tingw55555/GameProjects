using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : Singleton<CombatManager>
{
	public MartialDiscipline atkData;
	public BaseUnit currentAttackingUnit;

	public void QueueAttack(BaseUnit newAttackingUnit, MartialDiscipline newAtkData)
	{
		currentAttackingUnit = newAttackingUnit;
		atkData = newAtkData;

	}

	public void SpawnAttackOnTile(BaseTile tileToAttack)
	{
		if (!tileToAttack.GetIsAttacking())
		{
			BaseObject objectOnTile = tileToAttack.GetHeldObject();

			if (objectOnTile != null)
			{
				objectOnTile.TakeDamage(atkData.dmgOnHit, false);

				if (atkData.displacementType == DisplacementType.Push)
				{
					if (!objectOnTile.isImmobile)
					{
						PushObject(objectOnTile);
					}

				}
			}

			GameObjectUtil.Instantiate(atkData.effect, tileToAttack.transform.position, tileToAttack.transform.rotation);
			tileToAttack.SetIsAttacking();
		}

	}

	public void PushObject(BaseObject pushedTarget)
	{
		Vector2 pushDir = ((Vector2)pushedTarget.posInGrid - (Vector2)currentAttackingUnit.posInGrid).normalized;
		Vector2Int finalDir = Vector2Int.zero;

		if (Mathf.Abs(pushDir.x) > Mathf.Abs(pushDir.y))
		{
			finalDir.x = pushDir.x <= 0 ? -1 : 1;

			if (Mathf.Abs(finalDir.x) > 0.89f)
			{

				finalDir.y = 0;
			}
			else
			{
				finalDir.y = pushDir.y <= 0 ? -1 : 1;
			}


		}
		else
		{
			finalDir.y = pushDir.y <= 0 ? -1 : 1;

			if (Mathf.Abs(finalDir.y) > 0.89f)
			{

				finalDir.x = 0;
			}
			else
			{
				finalDir.x = pushDir.x <= 0 ? -1 : 1;
			}


		}

		BaseTile nextTile = GridManager.instance.GetTile(finalDir + pushedTarget.posInGrid);

		if (nextTile != null)
		{
			if (nextTile.GetIsHolding())
			{
				pushedTarget.TakeDamage(atkData.dmgOnCollison, true);
				BaseObject slammedObject = nextTile.GetHeldObject();
				slammedObject.TakeDamage(atkData.dmgOnCollison, true);
				
				AudioManager2.instance.PlayRandomSFX(slammedObject.SFX_Slam);
			}

			pushedTarget.MoveTo(nextTile);
		}

	}


	public void AttackFinished()
	{
		UnitManager.instance.EnableCharactersMove();
	}



}
