using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoteController : MonoBehaviour
{
    public Image icon;
    public int barLength;

    public NoteType currentType;

    public float speed;

    public bool hitZone;
    public bool canBePressed;


    //public bool isHold;



    // Start is called before the first frame update
    void Start()
    {
        hitZone = false;
        canBePressed = false;
    }

    // Update is called once per frame
    protected virtual void Update()
    {

        Move();




    }

    public void Move()
    {
        transform.localPosition += (Vector3.right * speed * Time.deltaTime);
    }



    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    //When the note enters the zones, it can be pressed. 
    //    //But it will result in a miss if the note is not in the hit zone

    //    //MissZone miss = collision.GetComponent<MissZone>();
    //    HitZone hit = collision.GetComponent<HitZone>();

    //    BeatManager.instance.canBePressed = true;

    //    if (hit != null)
    //    {
    //        hitZone = true;
    //    }

    //}


    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    BeatManager.instance.canBePressed = false;
    //    hitZone = false;
    //}



    public virtual void InitializeBar() { }
}



