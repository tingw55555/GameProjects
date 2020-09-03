using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public GameObject[] obstacles;

    public bool isActive = true;
    public Transform obstacleParent;
    public Vector2 spawnRange;
    public BaseUnit boss;

    //This is a test script. It is not being used. 

    // Start is called before the first frame update
    void Start()
    {
        if (isActive)
        {
            InvokeRepeating("SpawnObstacle", 1, 2);
        }

    }

    // Update is called once per frame
    void Update()
    {


    }



    public void SpawnObstacle()
    {
        int r = Random.Range(0, obstacles.Length);

       GameObject obstaclePrefab = GameObjectUtil.Instantiate(obstacles[r], new Vector3(Random.Range(spawnRange.x, spawnRange.y), 5, 1), Quaternion.identity);
        obstaclePrefab.GetComponent<Projectile>().owner = boss;
    }
}
