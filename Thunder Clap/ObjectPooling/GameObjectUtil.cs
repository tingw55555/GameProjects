using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectUtil
{
    private static Dictionary<RecyclableObjects, ObjectPool> completeObjectPool = new Dictionary<RecyclableObjects, ObjectPool>();

    //This is how overload is made!
    /// <summary>
    /// This allows the caller to not have to put in a rotation on Instantiate
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    public static GameObject Instantiate(GameObject prefab, Vector3 pos)
    {
        return Instantiate(prefab, pos, Quaternion.identity);
    }

    
    /// <summary>
    /// Always call GameObjectUtil.instantiate instead of the usual one because it deals with recyclable objects.
    /// This allows the caller to set position and rotation on Instantiate.
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="pos"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public static GameObject Instantiate(GameObject prefab, Vector3 pos, Quaternion rotation)
    {
        GameObject instance = null;
        RecyclableObjects recyclableComponent = prefab.GetComponent<RecyclableObjects>();

        //We check if the object we wanna spawn is recyclable
        if (recyclableComponent != null)
        {
            //if it is, create makes sure we get its pool
            ObjectPool pool = GetObjectPool(recyclableComponent);
            
            //After linking the object I wanna spawn (the key) to its pool, 
            //I ask its pool to reactivate a previously disenabled object 
            //and reset its position & rotation before spawning it again 
            instance = pool.NextObject(pos, rotation).gameObject;

        }
        else
        {
            instance = GameObject.Instantiate(prefab, pos, rotation);
        }


        return instance;
    }


    /// <summary>
    /// Always call GameObjectUtil.destroy instead of the usual one because it deals with recyclable objects
    /// </summary>
    /// <param name="objectToDestroy"></param>
    public static void Destroy(GameObject objectToDestroy)
    {
        RecyclableObjects recyclable = objectToDestroy.GetComponent<RecyclableObjects>();

        //If the object we want to destroy is a recyclable object, we simply set it to inactive
        if (recyclable != null)
        {
            recyclable.ShutDown();

        }
        else
        {
            //if it's not a recyclable object, we just destroy it as usual 
            //NOTICE: it's "GameObject" destroy, not "GameObjectUtil"
            GameObject.Destroy(objectToDestroy);
        }


    }

    private static ObjectPool GetObjectPool(RecyclableObjects newObject)
    {
        
        ObjectPool newPool = null;

        //If I already have a pool for the object I want to spawn 
        if (completeObjectPool.ContainsKey(newObject))
        {
            //I add it to the existing pool 
            newPool = completeObjectPool[newObject];


        }
        else
        {
            //if I do not have a pool for the object I want to spawn (when it's first spawned)

            //I create a empty parent object in the hierarchy and set all subsquently spawned objects as its children
            //This way I keep the hierarchy clean
            GameObject newPoolParent = new GameObject(newObject.gameObject.name + "Pool");

            //Now, I make the empty object into a object pool object  
            newPool = newPoolParent.AddComponent<ObjectPool>();

            //I link this pool to the object I wanna spawn 
            newPool.recyclablePrefab = newObject;

            //And then I add to the key and the pool into the complete pool dictionary 
            completeObjectPool.Add(newObject, newPool);
        }

        //After all of that, I now return the pool
        return newPool;

    }


}
