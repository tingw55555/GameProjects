using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class Character : BaseUnit
{

	private Color currentColour;
	private MeshRenderer mr;

	[Header("Prefab Reference")]
	public GameObject selector;
	public LineRenderer lR;

	[Header("Base Colour")]
	public Color hoveredColour;
	public Color canColour;
	public Color movedColour;

	[Header("Portraits and UI")] // this should be in the scriptable objects but some of them were checked out
	public Sprite Portrait_Small;
	public Sprite Portrait_Big;
	public Sprite Avatar;
	public Sprite Path_Move;
	public Sprite Path_Attack;
	public int UISlot = 0;
	public UICharacterSlot UISlotDisplay;

	[Header("Combat Attributes")]
	public SpiritAnimal moveData;
	public MartialDiscipline attackData;

	protected override void Awake()
	{
		mr = GetComponentInChildren<MeshRenderer>();

		base.Awake();
		type = UnitType.Friendly;

		selector.SetActive(false);
		SetCurrentColour();

		// this grabs the correct UI script for this character
		switch (UISlot)
		{
			case 1:
				UISlotDisplay = FindObjectOfType<UICharacterSlot_01>();
				UISlotDisplay.Parent = this;
				break;
			case 2:
				UISlotDisplay = FindObjectOfType<UICharacterSlot_02>();
				UISlotDisplay.Parent = this;
				break;
			case 3:
				UISlotDisplay = FindObjectOfType<UICharacterSlot_03>();
				UISlotDisplay.Parent = this;
				break;
			case 4:
				UISlotDisplay = FindObjectOfType<UICharacterSlot_04>();
				UISlotDisplay.Parent = this;
				break;
			default:
				Debug.LogError("Don't forget to set the UI slot for the character (1-4)");
				break;
		}
	}

	public virtual void Start()
	{
		// character in slot 1 is selected by default
		if (UISlot == 1)
		{
			//Debug.Log("Select me please");
			OnSelectedDisplayPath();
		}
	}

	private void OnEnable()
	{
		BaseTile.OnTileHover += DrawMoveLine;
	}

	private void OnDisable()
	{
		BaseTile.OnTileHover -= DrawMoveLine;
	}




	private void OnMouseEnter()
	{
		if (!GameManager.instance.playerTurn) return;

		mr.material.color = hoveredColour;

		if (!hasMoved)
		{
			selector.SetActive(true);
		}

		UISlotDisplay.OnHover();
	}

	private void OnMouseExit()
	{
		if (!GameManager.instance.playerTurn) return;

		mr.material.color = currentColour;

		if (UnitManager.instance.selectedCharacter != this)
		{
			selector.SetActive(false);
		}

		UISlotDisplay.OnHoverEnd();
	}


	private void OnMouseDown()
	{
		if (GlobalStat.isEditingMap) return;


		if (GameManager.instance.playerTurn)
		{
			TileDetector.instance.RaycastOnTile(true);

			//The unit can be selected if (1) they are not currently moving, (2) someone else isn't moving currently
			if (isMoving || !canMove)
			{
				return;
			}


			//Create path and show it here 
			OnSelectedDisplayPath();
		}

	}
	public void CreatePaths()
	{

		List<List<Vector2Int>> attackList = new List<List<Vector2Int>>();

		for (int i = 0; i < attackData.attackPaths.Length; i++)
		{
			attackList.Add(attackData.attackPaths[i].atkCoords.ToList());
		}

		//We create all the relative inside path controller by passing our postion & the coordinates of move & attack shapes
		pathController = new CharacterPathController(

			//Pass in our current position
			posInGrid,

			//Move Paths
			moveData.moveCoords,

			//Attack Paths
			attackList,

			false

		);

	}

	public void OnSelectedDisplayPath()
	{
		UnitManager.instance.SetNewSelectedCharacter(this);
		CreatePaths();
		GridManager.instance.UpdateCharacterMoveTiles(pathController);
	}


	public void CommittMove()
	{
		GridManager.instance.GetTile(posInGrid).ClearHeldObject();

		ResetVisual();

		isMoving = true;

		transform.SetParent(GridManager.instance.GetCombatGridParent());

		List<BaseTile> newPath = pathController.GetSelectedMovePath();

		StartCoroutine(MoveCoroutine(newPath));

	}

	IEnumerator MoveCoroutine(List<BaseTile> newPath)
	{
		if (!newPath.Any())
		{
			StopCoroutine(MoveCoroutine(null));
		}


		for (int i = 0; i < newPath.Count; i++)
		{
			float t = 0;

			while (t < 1)
			{
				transform.localPosition = Vector3.Lerp(transform.localPosition, GridManager.instance.GetTileLocalPosition(newPath[i].posInGrid), t);

				yield return null;

				t += Time.deltaTime / GlobalStat.moveDuration;
			}
		}

		posInGrid = newPath[newPath.Count - 1].posInGrid;

		transform.SetParent(newPath[newPath.Count - 1].transform);
		transform.localPosition = Vector3.zero;

		pathController.ClearSelectedPath();

		GridManager.instance.GetTile(posInGrid).SetHeldObject(this);

		hasMoved = true;

		ResetVisual();

		SetCurrentColour();

		Attack();

		yield return new WaitForSeconds(GlobalStat.resetMovingTime);

		isMoving = false;

	}

	public void Attack()
	{
		CombatManager.instance.QueueAttack(this, attackData);
		pathController.SpawnAttackOnPath();

	}


	public void DrawMoveLine(bool isDestination)
	{

		if (isDestination && UnitManager.instance.IsSelected(this))
		{
			if (pathController != null)
			{
				List<BaseTile> path = pathController.GetHoveredMovePath();

				lR.enabled = true;

				List<Vector3> newPos = new List<Vector3>();

				//So the line render is not inside to grid 
				Vector3 offset = path[0].transform.up * 0.5f;

				//Added Character's tile so the line extends from the character to destination
				newPos.Add(GridManager.instance.GetTile(posInGrid).transform.position + offset);

				foreach (BaseTile tile in path)
				{
					newPos.Add(tile.transform.position + offset);
				}

				lR.positionCount = newPos.Count;
				lR.SetPositions(newPos.ToArray());
			}
		}
	}

	//Selector & Line Render & UISlot
	public void ResetVisual()
	{
		selector.SetActive(false);
		lR.enabled = false;
	}


	public void SetCurrentColour()
	{
		if (hasMoved || isMoving)
		{
			currentColour = movedColour;
		}
		else
		{
			currentColour = canColour;
		}

		mr.material.color = currentColour;
	}

	public bool GetIsMoving()
	{
		return isMoving;
	}


	protected override void DestroyObject()
	{
		UnitManager.instance.RemoveCharacter(this);
		base.DestroyObject();
	}

	public override void TakeDamage(float dmg, bool collisonDmg)
	{
		if (!collisonDmg)
		{
			//AudioManager.instance.SetSFX(SFX.PlayerDamaged);
			AudioManager2.instance.PlayRandomSFX(SFX_Hit);
		}
		base.TakeDamage(dmg, collisonDmg);

		UISlotDisplay.UpdateHealth();
	}



}
