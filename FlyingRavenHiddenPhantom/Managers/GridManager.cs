using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : Singleton<GridManager>
{
	private int xDim, yDim;

	private BaseTile[,] grid;

	private Vector3 startPosition;

	private CharacterPathController currentPathController;



	[Space(10)]
	public BaseTile baseTilePrefab;
	public Transform combatGridParent;

	private void OnEnable()
	{
		BaseTile.OnTileHover += OnHoverTile;
		BaseTile.OnTileExit += OnExitTile;

	}

	private void OnDisable()
	{
		BaseTile.OnTileHover -= OnHoverTile;
		BaseTile.OnTileExit -= OnExitTile;
	}

	private void CreateGrid()
	{
		grid = new BaseTile[xDim, yDim];
		float size;
		size = baseTilePrefab.transform.localScale.x;

		startPosition = new Vector3(-((float)(xDim * size) / 2.0f) + (size / 2.0f), 0,
									-((float)(yDim * size) / 2.0f) + (size / 2.0f));

		for (int x = 0; x < xDim; x++)
		{
			for (int y = 0; y < yDim; y++)
			{
				BaseTile tile = Instantiate(baseTilePrefab, combatGridParent);

				tile.gameObject.name = y + "-" + x;

				tile.transform.localPosition = new Vector3(startPosition.x + (x * size), 0, startPosition.z + (y * size));

				tile.posInGrid = new Vector2Int(x, y);

				grid[x, y] = tile;

			}
		}

	}



	public void Initialize_Grid()
	{
		xDim = GlobalStat.xDim;
		yDim = GlobalStat.yDim;
		CreateGrid();
	}

	public void DestroyGrid()
	{

		for (int x = 0; x < xDim; x++)
		{
			for (int y = 0; y < yDim; y++)
			{
				Destroy(grid[x, y].gameObject);

			}
		}


	}

	public BaseTile GetTile(Vector2Int nextTile)
	{
		if (!IsValid(nextTile))
		{
			return null;
		}

		return grid[nextTile.x, nextTile.y];
	}

	public bool IsValid(Vector2Int coord)
	{
		return coord.x >= 0 && coord.y >= 0
			   && coord.x < xDim && coord.y < yDim;
	}

	public bool IsValidMoveTarget(Vector2Int coord)
	{
		if (IsValid(coord))
		{
			return !GetTile(coord).GetIsHolding();
		}

		return false;
	}


	public Transform GetCombatGridParent()
	{
		return combatGridParent;
	}

	public Vector3 GetTileLocalPosition(Vector2Int coord)
	{
		return grid[coord.x, coord.y].transform.localPosition;
	}


	public void OnHoverTile(bool isDestination)
	{
		if (isDestination)
		{
			currentPathController?.ShowHoveredPath();
		}
	}

	public void OnExitTile(bool isDestination)
	{
		if (isDestination)
		{
			currentPathController?.HideHoveredPath();
		}
	}



	public void UpdateCharacterMoveTiles(CharacterPathController newController)
	{

		currentPathController?.HideAllPaths();

		currentPathController = newController;

		currentPathController.ShowAllDestinations();
	}


	public void ClearCurrentPathController()
	{
		currentPathController = null;
	}


}
