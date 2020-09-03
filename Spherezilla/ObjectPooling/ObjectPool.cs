using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IRecyclableObjects
{
    bool IsCurrentlyActive();
    GameObject GetGameObject();

    void Init(Vector3 pos, Quaternion rotation);

    void Init();

    void ShutDown();
}


public enum RecyclableObjectTypes
{
    BrokenPiece = 0,
    //BuildingPiece = 1,
    PlayerProjectile = 1,

}



public class ObjectPool : MonoBehaviour
{

    [System.Serializable]
    public class PoolParameters
    {
        public string poolName;
        public RecyclableObjectTypes type;
        public GameObject prefab;
        public Transform hierarchyParent;
        public int poolSize;



    }

    public static ObjectPool instance;

    private Dictionary<RecyclableObjectTypes, List<IRecyclableObjects>> allObjectPools = new Dictionary<RecyclableObjectTypes, List<IRecyclableObjects>>();

    public List<PoolParameters> poolParameters = new List<PoolParameters>();


    private void Awake()
    {
        instance = this;

        foreach (PoolParameters poolParameter in poolParameters)
        {
            allObjectPools.Add(poolParameter.type, new List<IRecyclableObjects>());

            for (int i = 0; i < poolParameter.poolSize; i++)
            {
                IRecyclableObjects newObject = CreateNewInstance(poolParameter.type).GetComponent<IRecyclableObjects>();
                newObject.ShutDown();
                allObjectPools[poolParameter.type].Add(newObject);

            }
        }

    }


    public GameObject GetInstance(RecyclableObjectTypes typeToSpawn, Vector3 pos, Quaternion rotation)
    {
        if (allObjectPools.ContainsKey(typeToSpawn))
        {
            List<IRecyclableObjects> pool = allObjectPools[typeToSpawn];

            GameObject newObject = GetNextObject(pool);

            if (newObject == null)
            {
                newObject = CreateNewInstance(typeToSpawn);

                pool.Add(newObject.GetComponent<IRecyclableObjects>());
            }

            newObject.GetComponent<IRecyclableObjects>().Init(pos, rotation);

            return newObject;

        }

        return null;


    }


    public GameObject GetNextObject(List<IRecyclableObjects> pool)
    {
        foreach (IRecyclableObjects recyclableObject in pool)
        {
            if (!recyclableObject.IsCurrentlyActive())
            {
                return recyclableObject.GetGameObject();
            }
        }

        return null;

    }


    private GameObject CreateNewInstance(RecyclableObjectTypes typeToCreate)
    {
        return GameObject.Instantiate(poolParameters[(int)typeToCreate].prefab, poolParameters[(int)typeToCreate].hierarchyParent);

    }


    public void Destroy(GameObject objectToDestroy)
    {

        IRecyclableObjects recyclableComponent = objectToDestroy.GetComponent<IRecyclableObjects>();

        if (recyclableComponent != null)
        {
            recyclableComponent.ShutDown();

        }


    }


    public void InsertObjectToPool(RecyclableObjectTypes typeToAdd, GameObject objectToInsert)
    {
        if (allObjectPools.ContainsKey(typeToAdd))
        {
            List<IRecyclableObjects> pool = allObjectPools[typeToAdd];

            pool.Add(objectToInsert.GetComponent<IRecyclableObjects>());
        }
    }

}
