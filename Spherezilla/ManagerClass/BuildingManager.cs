using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager instance;

    public float delayVelocityDetectionDuration = 2f;
    public float explosionRadius = 50f;
    public float explosionForce = 100f;
    public float explosionUpward = 1f;

    public float buildingRegenTime = 3f;
    
    public int scorePerCube = 100;
    
    public int piecesPerRow = 2;



    private void Awake()
    {
        instance = this;
    }
}
