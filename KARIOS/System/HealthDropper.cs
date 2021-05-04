using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthDropper : MonoBehaviour
{
	[Header("References")]
	public GameArea gameArea;
	public GameObject healthPackPefab;
	public FloatObject playerHealth;

	[Header("Setting")]
	public float yOffset = 1f;
	[Range(0, 1)]
	//This heals based on player max health
	public float healModifier = 0.5f;
	[Range(0, 100)]
	public float dropChance = 75;

	private void OnEnable()
	{
		Enemy.OnUnitDie += DropHealthPack;
	}

	private void OnDisable()
	{
		Enemy.OnUnitDie -= DropHealthPack;
	}

	private void DropHealthPack(Transform enemy)
	{
		if (Random.Range(0, 100) < dropChance)
		{
			//Vector3 dropPos = gameArea.bounds.ClosestPoint(
			//	new Vector3(player.position.x + Random.Range(dropOffset.x, dropOffset.y),
			//	player.position.y, player.position.z
			//	+ Random.Range(dropOffset.x, dropOffset.y)));


			HealthPack pack = Instantiate(healthPackPefab, new Vector3(enemy.position.x, yOffset, enemy.position.z), Quaternion.identity, transform).GetComponent<HealthPack>();
			pack.OnInit(playerHealth.MaxValue * healModifier);
		}



	}

}
