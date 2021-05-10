using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MissileCommand : Location
{
	private FixedSpriteController sc;
	private TextMeshPro status;
	private AudioSource aS;

	[Header("Missile Command Settings")]
	public GameObject missilePrefab;
	public Transform launchPos;
	public Missile currentMissile;
	public int neededAmount = 1;
	public LayerMask enemyLayer;

	protected override void Awake()
	{
		sc = GetComponentInChildren<FixedSpriteController>();
		status = GetComponentInChildren<TextMeshPro>();
		aS = GetComponent<AudioSource>();
		base.Awake();

	}


	private void OnEnable()
	{
		GameManager.OnGameReady += CheckIfLaunchPossible;
	}

	private void OnDisable()
	{
		GameManager.OnGameReady -= CheckIfLaunchPossible;
	}


	// Start is called before the first frame update
	void Start()
	{
		sc.SMask.transform.localPosition = SpriteController.GetEndPos(sc, receivingBay.holdAmount, receivingBay.maxCapacity);
		UpdateStatusText();
	}




	private bool CanShoot()
	{
		return receivingBay.holdAmount != 0 && currentMissile == null;
	}

	public override void ReactTo_Receiving(float waitDuration)
	{
		sc.SMask.transform.localPosition = SpriteController.GetEndPos(sc, receivingBay.holdAmount, receivingBay.maxCapacity);

		rangeIndi.zoneText.text = "Landing Zone";

		UpdateStatusText();

		CheckIfLaunchPossible();
	}




	public void OnMissileDestroyed()
	{
		PlayExplosion();
		Invoke("CheckIfLaunchPossible", 0.5f);
	}

	public void CheckIfLaunchPossible()
	{
		if (CanShoot()) SpawnMissile();
	}

	private void PlayExplosion()
	{
		aS.Play();
	}

	private void SpawnMissile()
	{

		if (GameManager.instance.currentState == GameState.Tutorial && TutorialManager.instance.currentTask == TutorialTask.MaterialToFactory)
		{
			TutorialManager.instance.MissileDelivered = true;
		}

		rangeIndi.zoneText.text = "Landing Zone";

		Missile newMissile = Instantiate(missilePrefab, launchPos.position, Quaternion.identity).GetComponent<Missile>();
		//newMissile.RangeColourInit();
		newMissile.mc = this;
		currentMissile = newMissile;

		status.text = "Ready To Launch";
		
		receivingBay.holdAmount--;
		sc.SMask.transform.localPosition = SpriteController.GetEndPos(sc, receivingBay.holdAmount, receivingBay.maxCapacity);
	}

	private void Update()
	{
		if (currentMissile != null && !currentMissile.navAgent.hasPath)
		{

			if (Input.GetMouseButtonDown(1))
			{
				Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hitInfo = new RaycastHit();

				if (Physics.Raycast(mouseRay, out hitInfo, 100, enemyLayer))
				{
					if (GameManager.instance.currentState == GameState.Tutorial && TutorialManager.instance.currentTask == TutorialTask.LaunchMissiles)
					{
						TutorialManager.instance.MissileLaunched = true;
					}

					hitInfo.transform.GetComponent<FighterJet>().ShowTarget();
					currentMissile.FollowTarget(hitInfo.transform);
					UpdateStatusText();
				}

			}
		}
	}

	private void UpdateStatusText()
	{

		if (receivingBay.holdAmount != 0)
		{
			status.text = "Ready To Launch";

		}
		else
		{
			status.text = "Waiting For Missiles";
		}


	}


}
