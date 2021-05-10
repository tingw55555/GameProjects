using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using TMPro;
using UnityEngine;

public class CargoBay : SpriteController
{
	private TextMeshPro amountText;
	
	//[Header("=====Note: Only TAC is using Cargo's Pos settings=====")]

	public ResourceType holdResource;


	public int holdAmount;
	public int maxCapacity = 10;

	[Space(10)]
	public Color depleteColour = new Color(192, 5, 5, 255);

	[Space(10)]
	public Sprite reliefPackageIcon;
	public Color reliefColour = new Color(95, 212, 57, 255);

	[Space(10)]
	public Sprite oreIcon;
	public Color oreColour = new Color(81, 106, 149, 255);

	[Space(10)]
	public Sprite materialIcon;
	public Color materialColour;


	[Space(10)]
	public Sprite missileIcon;
	public Color missileColour;

	protected override void Awake()
	{
		base.Awake();

		if (useVisual)
		{
			amountText = GetComponentInChildren<TextMeshPro>();
		}

	}


	public void Init()
	{
		sMask.transform.localPosition = GetEndPos(this, holdAmount, maxCapacity);
		SetCargoBayVisuals(holdResource);


	}

	public void SetCargoBayVisuals(ResourceType newResource)
	{
		holdResource = newResource;

		switch (holdResource)
		{
			case ResourceType.None:
				baseIcon.sprite = null;
				resourceFillIcon.sprite = null;
				amountText.text = null;
				break;

			case ResourceType.ReliefPackage:
				baseIcon.sprite = reliefPackageIcon;
				baseIcon.color = Color.white;

				resourceFillIcon.sprite = reliefPackageIcon;
				resourceFillIcon.color = reliefColour;
				
				amountText.text = holdAmount.ToString();
				break;

			case ResourceType.Ore:
				baseIcon.sprite = oreIcon;
				baseIcon.color = Color.white;
				
				resourceFillIcon.sprite = oreIcon;
				resourceFillIcon.color = oreColour;
				
				amountText.text = holdAmount.ToString();
				break;

			case ResourceType.Material:
				baseIcon.sprite = materialIcon;
				baseIcon.color = materialColour;

				resourceFillIcon.sprite = materialIcon;
				resourceFillIcon.color = materialColour;

				amountText.text = holdAmount.ToString();
				break;

			case ResourceType.Missile:
				baseIcon.sprite = missileIcon;
				baseIcon.color = missileColour;
				
				resourceFillIcon.sprite = missileIcon;
				resourceFillIcon.color = missileColour;
				
				amountText.text = holdAmount.ToString();
				break;



		}
	}

	public void UpdateCargoBayVisual_IfEmpty()
	{
		if (holdAmount == 0)
		{
			SetCargoBayVisuals(ResourceType.None);
			return;
		}


	}

	public void SetUnloadColour()
	{
		resourceFillIcon.color = depleteColour;
	}


	[ContextMenu("Set Default Colours")]
	public void SetDefaultColours()
	{
		depleteColour = new Color(192, 5, 5, 255);
		reliefColour = new Color(95, 212, 57, 255);
		oreColour = new Color(81, 106, 149, 255);

	}

	[ContextMenu("Set Default Mask Position")]
	public void SetDefaultMaskPosition()
	{
		fullPos = new Vector3(0, 0, 1.3f);
		depletedPos = new Vector3(0, 0, 0.35f);
	}


	public int GetRemainingCapacity()
	{
		return maxCapacity - holdAmount;
	}
}
