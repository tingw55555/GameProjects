using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class CharacterPathController
{
	private List<CharacterPaths> paths = new List<CharacterPaths>();


	private CharacterPaths currentPath;
	private bool isEnemyAP;

	public struct CharacterPaths
	{
		public CP_Move movePath;
		public CP_Attack[] attackPaths;

		public CharacterPaths(CP_Move newMovePath, List<List<Vector2Int>> attackCoords, bool isEnemy)
		{
			movePath = newMovePath;
			attackPaths = new CP_Attack[attackCoords.Count];

			for (int i = 0; i < attackCoords.Count; i++)
			{
				attackPaths[i] = new CP_Attack(attackCoords[i].ToArray(), isEnemy);
			}
		}
		public void ShowAttack()
		{
			foreach (CP_Attack atkPath in attackPaths)
			{
				atkPath.ShowPath();
			}
		}


		public void ShowPath()
		{
			movePath.ShowPath();

			foreach (CP_Attack atkPath in attackPaths)
			{
				atkPath.ShowPath();
			}
		}

		public void HidePath()
		{
			movePath.HidePath();

			foreach (CP_Attack atkPath in attackPaths)
			{
				atkPath.HideAll();
			}
		}

		public void HideAll()
		{
			movePath.HideAll();

			foreach (CP_Attack atkPath in attackPaths)
			{
				atkPath.HideAll();
			}
		}

		public void SpawnAttack()
		{
			foreach (CP_Attack atkPath in attackPaths)
			{
				atkPath.SpawnAttackEffect();
			}
		}

	}

	public CharacterPathController(Vector2Int characterPos, Vector2Int[] moveCoords, List<List<Vector2Int>> attackCoords, bool isEnemy)
	{
		isEnemyAP = isEnemy;

		Vector2Int[] forwardCoords = new Vector2Int[moveCoords.Length];
		Vector2Int[] rightCoords = new Vector2Int[moveCoords.Length];
		Vector2Int[] leftCoords = new Vector2Int[moveCoords.Length];
		Vector2Int[] backCoords = new Vector2Int[moveCoords.Length];

		List<List<Vector2Int>> forwardAttackCoords = new List<List<Vector2Int>>();
		List<List<Vector2Int>> backAttackCoords = new List<List<Vector2Int>>();
		List<List<Vector2Int>> rightAttackCoords = new List<List<Vector2Int>>();
		List<List<Vector2Int>> leftAttackCoords = new List<List<Vector2Int>>();

		for (int i = 0; i < moveCoords.Length; i++)
		{
			forwardCoords[i] = characterPos + new Vector2Int(moveCoords[i].x, moveCoords[i].y);
			backCoords[i] = characterPos + new Vector2Int(-moveCoords[i].x, -moveCoords[i].y);

			leftCoords[i] = characterPos + new Vector2Int(moveCoords[i].y, -moveCoords[i].x);
			rightCoords[i] = characterPos + new Vector2Int(-moveCoords[i].y, moveCoords[i].x);

		}

		for (int i = 0; i < attackCoords.Count; i++)
		{
			List<Vector2Int> fwdTmp = new List<Vector2Int>();
			List<Vector2Int> bckTmp = new List<Vector2Int>();
			List<Vector2Int> rgtTmp = new List<Vector2Int>();
			List<Vector2Int> lftTmp = new List<Vector2Int>();

			for (int j = 0; j < attackCoords[i].Count; j++)
			{
				fwdTmp.Add(new Vector2Int(forwardCoords[forwardCoords.Length - 1].x, forwardCoords[forwardCoords.Length - 1].y) +
					new Vector2Int(attackCoords[i][j].x, attackCoords[i][j].y));

				bckTmp.Add(new Vector2Int(backCoords[backCoords.Length - 1].x, backCoords[backCoords.Length - 1].y) +
					new Vector2Int(-attackCoords[i][j].x, -attackCoords[i][j].y));

				lftTmp.Add(new Vector2Int(leftCoords[leftCoords.Length - 1].x, leftCoords[leftCoords.Length - 1].y) +
					new Vector2Int(attackCoords[i][j].y, -attackCoords[i][j].x));

				rgtTmp.Add(new Vector2Int(rightCoords[rightCoords.Length - 1].x, rightCoords[rightCoords.Length - 1].y) +
					 new Vector2Int(-attackCoords[i][j].y, attackCoords[i][j].x));
			}

			forwardAttackCoords.Add(fwdTmp);
			backAttackCoords.Add(bckTmp);

			leftAttackCoords.Add(lftTmp);
			rightAttackCoords.Add(rgtTmp);
		}

		CP_Move cpFwd = new CP_Move(forwardCoords);

		if (!isEnemy)
		{
			CP_Move cpBck = new CP_Move(backCoords);
			CP_Move cpLft = new CP_Move(leftCoords);
			CP_Move cpRgt = new CP_Move(rightCoords);

			backAttackCoords = cpBck.GetDestination() != null ? AdjustAttackCoords(cpBck, backCoords, backAttackCoords) : new List<List<Vector2Int>>();
			leftAttackCoords = cpLft.GetDestination() != null ? AdjustAttackCoords(cpLft, leftCoords, leftAttackCoords) : new List<List<Vector2Int>>();
			rightAttackCoords = cpRgt.GetDestination() != null ? AdjustAttackCoords(cpRgt, rightCoords, rightAttackCoords) : new List<List<Vector2Int>>();
			forwardAttackCoords = cpFwd.GetDestination() != null ? AdjustAttackCoords(cpFwd, forwardCoords, forwardAttackCoords) : new List<List<Vector2Int>>();

			paths.Add(new CharacterPaths(cpBck, backAttackCoords, isEnemy));
			paths.Add(new CharacterPaths(cpLft, leftAttackCoords, isEnemy));
			paths.Add(new CharacterPaths(cpRgt, rightAttackCoords, isEnemy));
		}


		paths.Add(new CharacterPaths(cpFwd, forwardAttackCoords, isEnemy));

	}


	private List<List<Vector2Int>> AdjustAttackCoords(CP_Move cp, Vector2Int[] moveCoords, List<List<Vector2Int>> atkCoords)
	{
		Vector2Int fwdDiff = cp.GetDestination().posInGrid - moveCoords[moveCoords.Length - 1];

		for (int i = 0; i < atkCoords.Count; i++)
		{
			for (int j = 0; j < atkCoords[i].Count; j++)
			{
				atkCoords[i][j] += fwdDiff;
			}
		}

		return atkCoords;
	}


	public void ShowAttackPath()
	{
		foreach (CharacterPaths path in paths)
		{
			path.ShowAttack();
		}
	}


	public void ShowAllDestinations()
	{
		foreach (CharacterPaths path in paths)
		{
			path.movePath.ShowDestination();
		}
	}

	public void HideAllPaths()
	{
		foreach (CharacterPaths path in paths)
		{
			path.HideAll();
		}
	}

	public void ClearSelectedPath()
	{
		currentPath.movePath.ClearPathInfo();
		currentPath.HideAll();
	}

	public void ClearAllPathInfo()
	{
		foreach (CharacterPaths p in paths)
		{
			p.movePath.ClearPathInfo();
			p.HideAll();
		}
	}

	public void HideHoveredPath()
	{
		foreach (CharacterPaths path in paths)
		{
			if (path.movePath.GetIsShowing())
			{
				path.HidePath();
				break;
			}

		}
	}


	//Show move and attack path when hovering over destination 
	public void ShowHoveredPath()
	{
		foreach (CharacterPaths path in paths)
		{
			if (path.movePath.GetIsHovered())
			{
				
				path.ShowPath();
				break;
			}

		}
	}

	//To committ move and translat
	public List<BaseTile> GetSelectedMovePath()
	{
		List<BaseTile> selectedPath = null;

		for (int i = 0; i < paths.Count; i++)
		{
			if (paths[i].movePath.GetIsHovered())
			{
				selectedPath = paths[i].movePath.GetPath();
				currentPath = paths[i];
			}
			else
			{
				paths[i].movePath.ClearPathInfo();
			}
		}

		return selectedPath;
	}

	//To draw Line
	public List<BaseTile> GetHoveredMovePath()
	{
		List<BaseTile> hoveredPath = null;

		for (int i = 0; i < paths.Count; i++)
		{
			if (paths[i].movePath.GetIsHovered())
			{
				hoveredPath = paths[i].movePath.GetPath();

				break;
			}

		}

		

		return hoveredPath;
	}


	public void SpawnAttackOnPath()
	{
		if (isEnemyAP)
		{
			if (paths.Count > 0)
			{
				paths[0].SpawnAttack();
			}

			return;
		}


		currentPath.SpawnAttack();
	}

}
