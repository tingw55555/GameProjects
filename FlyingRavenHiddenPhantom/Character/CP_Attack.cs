using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


public class CP_Attack : CharacterPath 
{
	

	public CP_Attack(Vector2Int[] coords, bool isEnemy)
	{
		this.isEnemy = isEnemy;

		foreach (Vector2Int coord in coords)
		{
			if (!AddTile(coord))
			{
				break;
			}
		}

	}

	public override bool AddTile(Vector2Int newTile)
	{
		if (GridManager.instance.IsValid(newTile))
		{
			path.Add(GridManager.instance.GetTile(newTile));
			return true;
		}
		return false;

	}


	public override void ShowPath()
	{
		foreach (BaseTile tile in path)
		{
			tile.AttackHighlight(isEnemy);

		}

		isShowing = true;
	}

	public async void SpawnAttackEffect()
	{

		for (int i = 0; i < path.Count && Application.isPlaying; i++)
		{
			CombatManager.instance.SpawnAttackOnTile(path[i]);
			
			await Task.Delay((int)(GlobalStat.attackDuration * 1000f));
		}

		CombatManager.instance.AttackFinished();

	}


	public override void HideAll()
	{
		foreach (BaseTile tile in path)
		{
			tile.RemoveHighlight(isEnemy, true);
		}
	}

}
