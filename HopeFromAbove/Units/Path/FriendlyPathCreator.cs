using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;




public class FriendlyPathCreator : MonoBehaviour
{
	private List<Vector3> points = new List<Vector3>();

	//By the defualt, Path Mover with the AgentID A is the current Agent. (Set on Start)
	private AgentID currentAgentID;
	public PathMover currentMover;

	private bool isValidPoint = false;

	public static Action<IEnumerable<Vector3>, AgentID> OnNewPathCreated;

	[Header("One Time Settings")]
	public LayerMask terrain;
	public LayerMask unit;
	public GameObject startMarker;

	[Header("Path Mover Settings")]
	public float minPointDistance = 1f;
	public float maxPointDistance = 5f;

	private float defaultMaxPointDistance = 5f;


	// Update is called once per frame
	void Update()
	{
		HandleInput();

	}

	private void HandleInput()
	{
		Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hitInfo = new RaycastHit();

		if (Input.GetMouseButtonDown(0))
		{
			if (Physics.Raycast(mouseRay, out hitInfo, 100, unit))
			{
				PathMover newMover = hitInfo.transform.GetComponent<PathMover>();

				if (newMover != null)
				{
					newMover.RegisterAsMovingAgent();
				}
			}

			StartNewPath(mouseRay, hitInfo);
		}

		//Draw patth
		if (Input.GetMouseButton(0))
		{
			DrawPath(GetMousePoint(mouseRay, hitInfo));
		}
		//Mover start moving
		else if (Input.GetMouseButtonUp(0))
		{
			RemoveBackTrack();
			MovePathAgent();
		}
	}

	//Start a new path if new starting point is close enough
	private void StartNewPath(Ray mouseRay, RaycastHit hitInfo)
	{

		if (GetDistanceBetween_MousePointAndCurrentMover(mouseRay, hitInfo) <= maxPointDistance)
		{
			isValidPoint = true;

			if (currentMover != null) currentMover.RangeIndicator.SetValid();

			points.Clear();

			Instantiate(startMarker, GetMousePoint(mouseRay, hitInfo), Quaternion.Euler(0, 180, 0));
		}
		else
		{
			isValidPoint = false;
			if (currentMover != null) currentMover.RangeIndicator.SetInvalid();
			return;
		}


	}


	private void DrawPath(Vector3 clickedPos)
	{

		if (CheckIfValidPointWithinList(clickedPos) && isValidPoint)
		{
			if (clickedPos != Vector3.zero)
			{
				points.Add(clickedPos);
			}


			//We update this right away so we can see the line as we draw them 
			if (currentMover != null)
			{
				currentMover.PatheLine.positionCount = points.Count;
				currentMover.PatheLine.SetPositions(points.ToArray());
			}
		}
	}

	private Vector3 GetMousePoint(Ray mouseRay, RaycastHit hitInfo)
	{
		if (Physics.Raycast(mouseRay, out hitInfo, 100, terrain))
		{
			Vector3 pos = hitInfo.point;
			return pos;
		}

		return Vector3.zero;
	}

	public void RemoveBackTrack()
	{
		float closestDist = Mathf.Infinity;

		int pointIdx = -1;

		if (points.Any())
		{
			for (int i = 0; i < points.Count; i++)
			{
				if (currentMover == null) return;

				float distance = Vector3.Distance(points[i], currentMover.transform.position);

				if (closestDist > distance)
				{
					closestDist = distance;
					pointIdx = i+1;
				}

			}

			for (int r = 0; r < pointIdx; r++)
			{
				if (points.Count > pointIdx)
				{
					points.RemoveAt(0);
				}
			}
		}

	}


	private void MovePathAgent()
	{

		if (isValidPoint) OnNewPathCreated?.Invoke(points, currentAgentID);

		if (GameManager.instance.currentState == GameState.Tutorial && TutorialManager.instance.currentTask == TutorialTask.DrawPath)
		{
			TutorialManager.instance.HasDrawnPath = true;
		}
	}


	private float GetDistanceBetween_MousePointAndCurrentMover(Ray mouseRay, RaycastHit hitInfo)
	{
		if (Physics.Raycast(mouseRay, out hitInfo, 100, terrain))
		{
			if (currentMover != null)
			{
				Vector3 pos = hitInfo.point;

				return Vector3.Distance(currentMover.transform.position, pos);
			}
		}

		return Mathf.Infinity;
	}




	private bool CheckIfValidPointWithinList(Vector3 pointToCheck)
	{
		//if point list is empty
		if (!points.Any())
		{
			if (currentMover != null)
			{
				return Vector3.Distance(currentMover.transform.position, pointToCheck) <= maxPointDistance;
			}
		}

		//if there are any points
		return Vector3.Distance(points.Last(), pointToCheck) > minPointDistance;
	}

	public void SetCurrentAgent(PathMover selectedAgent)
	{
		if (currentMover != null)
		{
			currentMover.De_RegisterAsMovingAgent();
		}

		currentMover = selectedAgent;
		currentAgentID = currentMover.agentID;

	}


	[ContextMenu("No MaxDistance")]
	public void SetNoMaxDistance()
	{
		maxPointDistance = Mathf.Infinity;
	}


	[ContextMenu("Use Default MaxDistance")]
	public void SetDefaultMaxDistance()
	{
		maxPointDistance = defaultMaxPointDistance;
	}

	public void RemoveCurrentAgent(PathMover agentToRemove)
	{
		if (currentMover == agentToRemove)
		{
			currentMover = null;
		}
	}

}
