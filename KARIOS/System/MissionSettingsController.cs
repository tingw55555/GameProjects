using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class MissionSettingsController : MonoBehaviour
{

	[Header("---Mission Settings---")]
	[SerializeField] private int currentLevel;
	//Should be longer than fadeout
	[SerializeField] private float delayResetTime = 1f;

	[Header("---Level Info---")]
	public List<GameObject> maps = new List<GameObject>();
	public List<LevelData> enemySpawnData = new List<LevelData>();

	[Header("---Reference---")]
	public Transform combatMapParent;
	private GameObject currentMap;

	public static event Action<MapInfo, LevelData> OnNewMissionSettings;
	public static event Action<int> OnMissionStart;

	private void OnEnable()
	{
		MissionObjectiveController.OnMissionCompleteExit += DelayReset;
	}

	private void OnDisable()
	{

		MissionObjectiveController.OnMissionCompleteExit -= DelayReset;
	}

	// Start is called before the first frame update
	void Start()
	{
		InitializeMissionSettings();
	}

	//Wait for the scene to fade out and then destroy things 
	private void DelayReset()
	{
		StartCoroutine(DelayResetCoroutine());
	}

	IEnumerator DelayResetCoroutine()
	{
		float currentTime = 0;
		while (currentTime < 1)
		{
			yield return null;
			currentTime += Time.deltaTime / delayResetTime;
		}

		Destroy(currentMap);
		SetProgress();
	}


	private void SetProgress()
	{
		if (currentLevel + 1 >= enemySpawnData.Count)
		{
			//Debug.Log("You won");

			SceneManager.LoadSceneAsync((int)GameScenes.Victory);
		}
		else
		{
			currentLevel++;
			InitializeMissionSettings();
		}

	}

	private void InitializeMissionSettings()
	{
		if (maps.Count <= 0) return;
		
		currentMap = Instantiate(maps[Random.Range(0, maps.Count)], combatMapParent.position, Quaternion.identity, combatMapParent);

		OnNewMissionSettings?.Invoke(currentMap.GetComponent<MissionMap>().info, enemySpawnData[currentLevel]);
		OnMissionStart?.Invoke(currentLevel);

	}

}
