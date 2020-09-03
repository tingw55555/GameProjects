using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour, IRecyclableObjects
{
    public float moveSpeed;

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void Init() {}

    public void Init(Vector3 pos, Quaternion rotation)
    {
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.right * moveSpeed * Time.deltaTime;
    }
}
