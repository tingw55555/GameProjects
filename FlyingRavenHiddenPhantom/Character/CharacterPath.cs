using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPath 
{

	protected List<BaseTile> path = new List<BaseTile>();
	protected bool isHovered = false;
	protected bool isShowing = false;
	public bool isEnemy = false;


	public void SetHovered(bool val)
	{
		isHovered = val;
	}

	public bool GetIsHovered()
	{
		return isHovered;
	}

	public bool GetIsShowing()
	{
		return isShowing;
	}

	public virtual void ShowPath()
	{
		foreach (BaseTile tile in path)
		{
			tile.MoveHighlight();

		}

		isShowing = true;
	}

	public BaseTile GetDestination()
	{
		if (path.Count > 0)
		{
			return path[path.Count - 1];
		}

		return null;
	}

	public void ShowDestination()
	{
		if (GetDestination() != null)
		{
			GetDestination().MoveHighlight();
		}
	}

	public void HidePath()
	{
		for (int i = 0; i < path.Count - 1; i++)
		{
			path[i].RemoveHighlight(isEnemy);
		}

		isShowing = false;
	}

	public virtual void HideAll()
	{
		foreach (BaseTile tile in path)
		{
			tile.RemoveHighlight(isEnemy);
		}
	}

	public void ClearPathInfo()
	{
		foreach (BaseTile tile in path)
		{
			tile.ClearTileInfo();
			tile.RemoveHighlight(isEnemy);
		}

		path.Clear();
	}


	public virtual bool AddTile(Vector2Int newTile)
	{
		if (GridManager.instance.IsValidMoveTarget(newTile))
		{
			path.Add(GridManager.instance.GetTile(newTile));
			return true;
		}
		return false;

	}

	public List<BaseTile> GetPath()
	{
		
		return path;
	}
}
