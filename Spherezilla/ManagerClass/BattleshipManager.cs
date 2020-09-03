using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BattleshipSpawns
{
    public string positionName;
    public Vector3 spawnPosition;
    public int yRotation;

}

public class BattleshipManager : MonoBehaviour
{
    public static BattleshipManager instance;
    public List<BattleshipSpawns> battleshipSpawnPositions;
    public int forwardPostion;
    
    //public List<Vector3> spawnOffsets;


    private void Awake()
    {
        instance = this;
    }

    public BattleshipSpawns GetSpawnPosition()
    {
        int r = Random.Range(0, battleshipSpawnPositions.Count);


        BattleshipSpawns info = battleshipSpawnPositions[r];
        battleshipSpawnPositions.Remove(info);

        return info;


    }

    public bool CheckSpawnPositionAvaliablity()
    {
        return battleshipSpawnPositions.Count != 0;
    }

}
