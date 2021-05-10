using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class CP_Move : CharacterPath
{

	public CP_Move(Vector2Int[] coords)
	{
		foreach (Vector2Int coord in coords)
		{
			//Check is the tile on this coord is valid
			if (!AddTile(coord))
			{
				break;
			}
		}

		if (path.Count > 0)
		{
			GetDestination().SetDestination(this);
		}


	}

	public override void HideAll()
	{
		foreach (BaseTile tile in path)
		{
			tile.RemoveHighlight(isEnemy, false);
		}
	}


}
