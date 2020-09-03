using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class DamageScore : MonoBehaviour
{
    public Vector3 offset;
    public TextMeshPro scoreText;
    public float moveDuration;
    
    // Start is called before the first frame update
    void Start()
    {
        //This prefab will move to its offset when it's spawned 
        Tweener t = transform.DOMove(offset, moveDuration);
        t.SetRelative(true);

        //It will destroy itself when it reaches the offset
        t.OnComplete(OnTweenComplete);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTweenComplete()
    {
        GameObjectUtil.Destroy(gameObject);
    }

    //This is set on trigger 
    public void SetText(float damage)
    {
        scoreText.text = damage.ToString();
    }
}
