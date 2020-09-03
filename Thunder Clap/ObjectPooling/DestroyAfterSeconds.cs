using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour
{
    public float lifeTime;

    private void OnEnable()
    {
        StartCoroutine(DestroyAfterSecondsCoroutine());
    }

    private void OnDisable()
    {
       
    }


    // Update is called once per frame
    void Update()
    {

    }


   

    IEnumerator DestroyAfterSecondsCoroutine()
    {
        yield return new WaitForSeconds(lifeTime);
        GameObjectUtil.Destroy(gameObject);
    }
}
