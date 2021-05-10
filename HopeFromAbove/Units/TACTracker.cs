using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TACTracker : MonoBehaviour
{

	[System.Serializable]
	public struct TAC
	{
		public string agentName;
		public AgentID agentID;
		
		public Color acColour;
		public TransportAirCraft ac;
	}

	[Header("RunTime Value")]
	public TAC[] airCraftUnits;


	private void OnEnable()
	{
		TransportAirCraft.RemoveAgentFromTrackList += RemoveAgent;
	}

	private void OnDisable()
	{
		TransportAirCraft.RemoveAgentFromTrackList -= RemoveAgent;
	}

	public AgentID GetAgentID()
	{
		foreach (TAC ac in airCraftUnits)
		{
			if (ac.ac == null)
			{
				return ac.agentID;
			}
		}

		return AgentID.unusable;

	}


	public bool IsFull()
	{
		foreach (TAC ac in airCraftUnits)
		{
			if (ac.ac == null)
			{
				return false;
			}
		}

		return true;

	}


	public void AddAgent(int acIdx, AgentID id, Color unitColor, TransportAirCraft ac)
	{
		TAC newAgent = airCraftUnits[acIdx];

		newAgent.agentName = ac.gameObject.name;
		newAgent.acColour = unitColor;
		newAgent.ac = ac;

		airCraftUnits[acIdx] = newAgent;

	}

	public void RemoveAgent(int agentIdx)
	{
		airCraftUnits[agentIdx].ac = null;

		if (CheckIfNoAgentsLeft())
		{
			GameManager.instance.OnLose(LoseCondition.NoMoreAC);
		}
	}


	public bool CheckIfNoAgentsLeft()
	{
		foreach (TAC ac in airCraftUnits)
		{
			if (ac.ac != null)
			{
				return false;
			}
		}

		return true;
	}


	public int GetNumberOfUnitSpawned()
	{
		int currentUnit = 0;

		foreach (TAC ac in airCraftUnits)
		{
			if (ac.ac != null)
			{
			  currentUnit++;
			}
		}

		return currentUnit;
	}


}
