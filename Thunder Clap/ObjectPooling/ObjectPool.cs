using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;

    public RecyclableObjects recyclablePrefab;

    private List<RecyclableObjects> recyclableObjectsPool = new List<RecyclableObjects>();

    /// <summary>
    /// This is where more recyclable objects are spawned when there aren't already enough of them
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    private RecyclableObjects CreateInstance(Vector3 pos, Quaternion rotation)
    {
        RecyclableObjects clone = GameObject.Instantiate(recyclablePrefab);
        clone.transform.position = pos;
        clone.transform.rotation = rotation;
        clone.transform.parent = transform;
        
        //This is where I populate the list in which I can recycle from 
        recyclableObjectsPool.Add(clone);

        return clone;

    }

    /// <summary>
    /// This reactivate an objects that is previously set to false ((Recycling the previously spawned objects)
    /// It also resets the object's position & rotation
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public RecyclableObjects NextObject(Vector3 pos, Quaternion rotation)
    {
        RecyclableObjects instance = null;

        //It loops through all the objects it has 
        foreach (RecyclableObjects recyclePrefab in recyclableObjectsPool)
        {
            //Find the ones that are not being used
            //And resets position and rotation 
            if (recyclePrefab.gameObject.activeSelf != true)
            {
                instance = recyclePrefab;
                instance.transform.position = pos;
                instance.transform.rotation = rotation;
            }
        }

        //If there aren't enough objects to recycle from the list 
        //I spawn new ones 
        if (instance == null)
        {
            instance = CreateInstance(pos, rotation);
        }

        //Set object to active, in case it's a reycled object (which remains inactive until now)
        instance.Restart();

        //And then give it to whoever that wants to instantiate it 
        return instance;
    }



    private void Awake()
    {
        instance = this;
    }



}
