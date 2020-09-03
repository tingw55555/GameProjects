using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshExplosion : MonoBehaviour, IRecyclableObjects
{

    private Rigidbody rb;
    private MeshRenderer mr;
    
    //private Collider co;

    private Vector3 startPos;
    private Quaternion startRot;


    //These two controls how many cubes the destroyable mesh breaks into
    //This number will be cubed because there are 3 dimensions. So, 2*2*2. There will be 8 pieces 
    private int piecesPerRow;


    private float delayVelocityDuration = 2f;
    private Vector3 brokenSize;

    private float explosionRadius;
    private float explosionForce;
    private float explosionUpward;
    private int score;

    private bool hasStartBeenSet;
    private float currentDelayTime;
    private bool hasExploded = false;

    private float minImpactVel = 0.2f;


    // Start is called before the first frame update
    void Start()
    {
        //ObjectPool.instance.InsertObjectToPool(RecyclableObjectTypes.BuildingPiece, gameObject);

        GatherInfo_FromBuildingManager();
        brokenSize = transform.lossyScale / piecesPerRow;


       

        //mr = GetComponent<MeshRenderer>();
        //co = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        mr = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {

        HandleColliderVelocity();


    }


    //Disable the velocity detection for a few seconds as they fall into places, 
    //which givies each other velocity in the process. ((We dont want them to explode!))
    public void HandleColliderVelocity()
    {
        if (currentDelayTime < delayVelocityDuration)
        {
            currentDelayTime += Time.deltaTime;

        }
        else
        {
            if (hasStartBeenSet == false)
            {
                startPos = transform.position;
                startRot = transform.rotation;

                hasStartBeenSet = true;
            }


            

            if (rb.velocity.magnitude > minImpactVel)
            {
                //Debug.Log(rb.velocity.magnitude);
                Explode();

            }
        }



    }


    public void GatherInfo_FromBuildingManager()
    {
        delayVelocityDuration = BuildingManager.instance.delayVelocityDetectionDuration;

        score = BuildingManager.instance.scorePerCube;

        piecesPerRow = BuildingManager.instance.piecesPerRow;

        explosionRadius = BuildingManager.instance.explosionRadius;
        explosionForce = BuildingManager.instance.explosionForce;
        explosionUpward = BuildingManager.instance.explosionUpward;
    }

    public void Explode()
    {
        if (hasExploded)
        {
            return;
        }

        hasExploded = true;

        GameManager.instance.OnPlayerGainScore(score);

        for (int x = 0; x < piecesPerRow; x++)
        {
            for (int y = 0; y < piecesPerRow; y++)
            {
                for (int z = 0; z < piecesPerRow; z++)
                {
                    CreatePieces(x, y, z);
                }
            }
        }


        DecreaseCubeCount();

    }


    public void DecreaseCubeCount()
    {
        transform.parent.parent.gameObject.GetComponent<RegenerativeBuilding>().OnCubeDestroyed();
        ShutDown();
        //ObjectPool.instance.Destroy(gameObject);

    }




    public void CreatePieces(int x, int y, int z)
    {
        Vector3 halfScale = transform.lossyScale / 2f;

        // Center position
        Vector3 startPos = transform.position;

        // Left side of object
        startPos -= transform.right * halfScale.x;
        // Bottom of object
        startPos -= transform.up * halfScale.y;
        // Front of object
        startPos -= transform.forward * halfScale.z;


        Vector3 newPosition = startPos;
        // Travel right
        newPosition += transform.right * (x * brokenSize.x + (brokenSize.x / 2f));

        // Travel up
        newPosition += transform.up * (y * brokenSize.y + (brokenSize.y / 2f));

        // Travel back
        newPosition += transform.forward * (z * brokenSize.z + (brokenSize.z / 2f));

        GameObject piece = ObjectPool.instance.GetInstance(RecyclableObjectTypes.BrokenPiece, newPosition, transform.rotation);
        piece.GetComponent<MeshRenderer>().material = mr.material;

        piece.transform.localScale = brokenSize;

        piece.GetComponent<Rigidbody>().AddForce((newPosition - PlayerInputController.instance.transform.position).normalized * explosionForce);

    }



    protected virtual void OnCollisionEnter(Collision collision)
    {
        //ToDo:
        //Add Audio
        //Add Visual Feedback
    }



    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void Init()
    {
        transform.position = startPos;
        transform.rotation = startRot;
        gameObject.SetActive(true);

        

        currentDelayTime = 0;
        hasExploded = false;

    }


    public void Init(Vector3 pos, Quaternion rotation) {}

    public bool IsCurrentlyActive()
    {
        return gameObject.activeSelf;
    }

    public void ShutDown()
    {
        gameObject.SetActive(false);

        //rb.useGravity = false;
        //rb.constraints = RigidbodyConstraints.FreezeAll;



    }

}
