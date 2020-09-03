using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenPiece : MonoBehaviour, IRecyclableObjects
{
    //public Material mat;

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void Init() { }

    public void Init(Vector3 pos, Quaternion rotation)
    {
        //gameObject.GetComponent<MeshRenderer>().material = mat;
        transform.position = pos;
        transform.rotation = rotation;
        gameObject.SetActive(true);
    }

    public bool IsCurrentlyActive()
    {
        return gameObject.activeSelf;
    }

    public void ShutDown()
    {
        gameObject.SetActive(false); 
    }
}
