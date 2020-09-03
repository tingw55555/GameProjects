using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegenerativeBuilding : MonoBehaviour
{
    public GameObject midParent;
    //private BoxCollider co;

    //public float moveDistance;
    private float currentRegenWaitTime;
    private float buildingRegenTime;

    private bool isWaitingToRegen = false;

    public int totalCubeCount;
    private int currentCubeCount;

    public bool playerOverlap;


    // Start is called before the first frame update
    void Start()
    {
        //co = GetComponent<BoxCollider>();
        //moveDistance = co.size.y + co.center.y;

        buildingRegenTime = BuildingManager.instance.buildingRegenTime;



        GetTotalCubesOfBuilding();
    }

    // Update is called once per frame
    void Update()
    {
        if (isWaitingToRegen)
        {
            currentRegenWaitTime += Time.deltaTime;

            if (currentRegenWaitTime >= buildingRegenTime && playerOverlap == false)
            {
                Regenerate();

            }
        }
    }

    public void GetTotalCubesOfBuilding()
    {
        totalCubeCount = midParent.transform.childCount;
        currentCubeCount = totalCubeCount;
    }

    public void OnCubeDestroyed()
    {
        currentCubeCount -= 1;


        if (currentCubeCount <= 0)
        {
            //DisplaceBuilding();

            isWaitingToRegen = true;
        }

    }




    public void Regenerate()
    {
        isWaitingToRegen = false;
        currentRegenWaitTime = 0;
        currentCubeCount = totalCubeCount;

        foreach (Transform child in midParent.transform)
        {
            child.gameObject.GetComponent<IRecyclableObjects>().Init();

        }
    }

    //public void DisplaceBuilding()
    //{
    //    midParent.transform.position = new Vector3(0, -moveDistance, 0);
    //}


    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerInputController>())
        {
            playerOverlap = true;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerInputController>())
        {
            playerOverlap = false;
        }
    }

}
