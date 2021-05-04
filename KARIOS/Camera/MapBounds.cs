using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapBounds : MonoBehaviour
{
	public Bounds bounds;

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(bounds.center, bounds.size);
	}

}
