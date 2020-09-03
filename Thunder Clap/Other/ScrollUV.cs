using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollUV : MonoBehaviour
{
    public MeshRenderer mr;
    public Vector2 offset;
    public float scrollYSpeedMod = 10;
    public float scrollXSpeedMod = 5;

    // Start is called before the first frame update
    void Start()
    {

        offset = mr.material.mainTextureOffset;


    }

    // Update is called once per frame
    void Update()
    {
        
        
        //This line makes the background scroll by itself 
        offset.y += Time.deltaTime / scrollYSpeedMod;
        offset.x += Time.deltaTime / scrollXSpeedMod;

        mr.material.mainTextureOffset = offset;
    }
}
