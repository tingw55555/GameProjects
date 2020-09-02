using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hold : NoteController
{
    public Image bar;
    
    public int reduceAmount = 10;
    public Vector2Int holdDurationRange;
    public Collider2D endOfBar;

    [HideInInspector]
    public bool isFinished = false;
    public bool stopMoving = false;

    //public float holdTime;
    //public float holdDuration;


    //private void Start()
    //{
    //    InitializeBar();
    //}




    protected override void Update()
    {
        if (stopMoving == false)
        {
            base.Update();
        }


        //if (Input.GetKeyDown(keyToPress))
        //{

        //    if (hitZone)
        //    {
        //        stopMoving = true;
        //    }
        //    else
        //    {
        //        //gameObject.SetActive(false);
        //        BeatManager.instance.NoteMiss(gameObject);

        //    }

        //}


        if (stopMoving == true)
        {

            ShorteningBar();

            BeatManager.instance.AddHoldingHappiness();



        }


        //    if (Input.GetKeyUp(BeatManager.instance.keyToPress) && endHold != true)
        //    {
        //        stopMoving = false;
        //        BeatManager.instance.NoteMiss();
        //    }

        //}


    }




    public override void InitializeBar()
    {
        SetRandomHoldDuration();
        SetBarLength();

    }


    public void SetRandomHoldDuration()
    {
        int r = Random.Range(holdDurationRange.x, holdDurationRange.y);

        
        barLength = r;

    }


    public void SetBarLength()
    {
        //Change bar length based on hold duration
        bar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, barLength);

        //no height offset
        endOfBar.offset = new Vector3(barLength, 0);
    }


    public void ShorteningBar()
    {
        barLength += Mathf.FloorToInt(speed * Time.deltaTime);

        if (barLength <= 0)
        {

            isFinished = true;
            BeatManager.instance.NoteHit();

            return;

        }

        SetBarLength();



        //Debug.Log(barLength);


    }



}
