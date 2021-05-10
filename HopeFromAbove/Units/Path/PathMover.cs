using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using System.Linq;

public enum AgentID
{
	A = 1,
	B = 2,
	C = 3,
	D = 4,
	F = 5,

	unusable = 10,
	Missile = 11,
}


[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(LineRenderer))]
public class PathMover : MonoBehaviour
{
	public static event Action<AgentID, Color, bool> onBindUnitColour;
	public static event Action<int> OnPathpointsReceived;

	private NavMeshAgent navAgent;
	private TransportAirCraft ac;


	private FriendlyPathCreator pc;
	public FriendlyPathCreator PC { set { pc = value; } }

	private Queue<Vector3> pathPoints = new Queue<Vector3>();
	private bool isSelected;
	public bool IsSelected { get { return isSelected; } }

	private bool canMove = true;

	public bool GetMoveStatus() { return canMove; }


	private LineRenderer pathLine;
	public LineRenderer PatheLine { get { return pathLine; } }

	[SerializeField]
	private Material pathMat;
	private Material unitPathMat;
	[SerializeField]
	private SpriteRenderer unitShadow;



	private RangeIndicator rangeIndicator;
	public RangeIndicator RangeIndicator { get { return rangeIndicator; } }

	[Header("RunTime Values")]
	public AgentID agentID;
	public Color unitColour;

	[Header("Nav Agent Settings")]
	public float stoppingDistance = 0.5f;

	private void Awake()
	{
		navAgent = GetComponent<NavMeshAgent>();
		pathLine = GetComponent<LineRenderer>();
		//Change the colour of the unit's range
		rangeIndicator = GetComponentInChildren<RangeIndicator>();

		//TODO: Optimize
		pc = FindObjectOfType<FriendlyPathCreator>();

		if (agentID != AgentID.Missile)
		{
			ac = GetComponent<TransportAirCraft>();

			//Change the shadow to unit colour
			//unitShadow = transform.GetChild(2).GetComponent<SpriteRenderer>();
		}
	}

	private void Start()
	{
		if (agentID == AgentID.A)
		{
			RegisterAsMovingAgent(false);
		}

	}

	public void UnitColourInit(Color newUnitClour)
	{

		unitPathMat = new Material(pathMat);
		unitColour = newUnitClour;
		unitPathMat.color = unitColour;
		pathLine.material = unitPathMat;

		unitShadow.color = unitColour;

		rangeIndicator.SetUnitColour(unitPathMat);
		rangeIndicator.SetValid();

	}

	public void MissileColourInit(Color missileColour)
	{
		unitPathMat = new Material(pathMat);
		unitPathMat.color = missileColour;
		

		rangeIndicator.SetUnitColour(unitPathMat);
		rangeIndicator.SetValid();
	}

	public void SetAgentID(AgentID ID)
	{
		agentID = ID;
	}

	public void ConnectHotKey(bool isActive = true)
	{
		//change the colour of the unit on Hotkey
		onBindUnitColour?.Invoke(agentID, unitColour, isActive);
	}

	private void OnEnable()
	{
		FriendlyPathCreator.OnNewPathCreated += SetPoints;
		HotKey.onHotKeyClicked += RegisterByHotKey;
		HotKey.onHotKeyDoubleClicked += SnapCamToSelf;

	}

	private void OnDisable()
	{
		FriendlyPathCreator.OnNewPathCreated -= SetPoints;
		HotKey.onHotKeyClicked -= RegisterByHotKey;
		HotKey.onHotKeyDoubleClicked -= SnapCamToSelf;
	}


	// Update is called once per frame
	private void Update()
	{
		UpdatePathing();
	}

	private void UpdatePathing()
	{
		if (canMove)
		{
			if (pc != null)
			{
				if (pc.currentMover != this && pathPoints.Count > 1)
				{
					pathLine.positionCount = pathPoints.Count;
					pathLine.SetPositions(pathPoints.ToArray());
				}
			}

			if (ShouldSetDestination())
			{

				navAgent.SetDestination(pathPoints.Dequeue());

				if (pathPoints.Count == 0)
				{
					pathLine.positionCount = 0;

					if (agentID == AgentID.Missile)
					{
						Destroy(gameObject);
					}
				}
			}
		}


	}

	public void SetMoveStatus(bool movable)
	{
		canMove = movable;
		navAgent.enabled = movable;

	}


	private bool ShouldSetDestination()
	{
		//If there arent no points, the agent won't move 
		if (pathPoints.Count == 0)
		{
			return false;
		}

		if (navAgent.hasPath == false || navAgent.remainingDistance < stoppingDistance) return true;

		return false;

	}


	private void SetPoints(IEnumerable<Vector3> points, AgentID agentToMove)
	{
		if (agentToMove != agentID)
		{
			return;
		}

		pathPoints = new Queue<Vector3>(points);

		if (agentID == AgentID.Missile)
		{
			OnPathpointsReceived?.Invoke(pathPoints.Count);
		}
	}



	public void RegisterAsMovingAgent(bool playSound = true)
	{
		if (pc == null) return;

		pc.SetCurrentAgent(this);
		rangeIndicator.SetValid();
		rangeIndicator.ShowIndicator();


		if (playSound && ac != null) ac.PlaySelectedSound();

		isSelected = true;
	}

	public void RegisterByHotKey(int hotkey)
	{
		if (hotkey == (int)agentID)
		{
			RegisterAsMovingAgent();
		}
	}

	public void SnapCamToSelf(int hotkey)
	{
		if (hotkey == (int)agentID)
		{
			CamControls.instance.SnapToPosition(transform.position);
		}
	}


	public void De_RegisterAsMovingAgent()
	{
		pc.RemoveCurrentAgent(this);
		rangeIndicator.HideIndicator();
		isSelected = false;
	}

	//This is pretty stupid but it will work for now!!
	public void HideRangeIndi(float seconds)
	{
		Invoke("HideIndi", seconds);
	}

	private void HideIndi()
	{
		rangeIndicator.HideIndicator();
	}

	public void CancelHideIndi()
	{
		CancelInvoke();
	}


	public void ClearPath()
	{
		pathPoints.Clear();
	}

}
