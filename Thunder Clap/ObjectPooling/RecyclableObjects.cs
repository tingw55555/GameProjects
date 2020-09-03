using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecyclableObjects : MonoBehaviour
{
    public void Restart()
    {
        gameObject.SetActive(true);
    }


    public void ShutDown()
    {
        gameObject.SetActive(false);
    }

}


