using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum MovePattern
{
    AllRandom,
    EvenRandom,
    OddRandom,
    HorizontalFollower,
    VerticalFollower,
}

public class MannequinUnit : MonoBehaviour
{
    public MovePattern movePattern;
    public List<Transform> allAvaliableMoveTargets;

    private Vector3 currentTarget;
    public bool hasATarget = false;
    public int moveTargetIndex = 0;


    private NavMeshAgent agent;
    private Animator anim;

    public float currentTime;
    public Vector2 idleTimeRange;

    private void OnEnable()
    {
        if (movePattern == MovePattern.HorizontalFollower)
        {
            PlayerInputController.playerHorizontalInputReceived += OnPlayerInput;
        }

        if (movePattern == MovePattern.VerticalFollower)
        {
            PlayerInputController.playerVerticalInputReceived += OnPlayerInput;
        }


    }

    private void OnDisable()
    {
        if (movePattern == MovePattern.HorizontalFollower)
        {
            PlayerInputController.playerHorizontalInputReceived -= OnPlayerInput;
        }

        if (movePattern == MovePattern.VerticalFollower)
        {
            PlayerInputController.playerVerticalInputReceived -= OnPlayerInput;
        }

    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

    }

    // Start is called before the first frame update
    void Start()
    {

        SelectNewDestination();

    }

    // Update is called once per frame
    void Update()
    {


        anim.SetFloat("Speed", (agent.velocity.magnitude / agent.speed));

        if (hasATarget && Vector3.Distance(transform.position, currentTarget) <= agent.stoppingDistance)
        {
            hasATarget = false;
            StartCoroutine(IdlePeriod());
        }


    }


    public void SelectNewDestination()
    {
        int index = 0;
       
        if (movePattern == MovePattern.EvenRandom)
        {
            index = GetSpecificIndex(false);
         
        }
        else if (movePattern == MovePattern.OddRandom)
        {
            index = GetSpecificIndex(true);
        }
        else 
        {
            index = GetRandomIndex();
        }

        moveTargetIndex = index;
        currentTarget = allAvaliableMoveTargets[index].position;
        agent.SetDestination(currentTarget);

        hasATarget = true;

    }

    public int GetSpecificIndex(bool getOdd)
    {
        int r = (Random.Range(0, Mathf.FloorToInt(allAvaliableMoveTargets.Count / 2.0f))) * 2;

        if (getOdd)
        {
            r -= 1;
        }

        if (r < 0)
        {
            r = 0;
        }
        else if (r >= allAvaliableMoveTargets.Count)
        {
            r = allAvaliableMoveTargets.Count - 1;
        }

        
        return r;

    }

    public int GetRandomIndex() 
    {
        int r = Random.Range(0, allAvaliableMoveTargets.Count);

        if (allAvaliableMoveTargets[r].position == currentTarget)
        {
            r += 1;

            if (r >= allAvaliableMoveTargets.Count)
            {
                r = 0;
            }
        }

        return r;

    }

    IEnumerator IdlePeriod()
    {
        float idleTime = Random.Range(idleTimeRange.x, idleTimeRange.y);
        currentTime = 0;
        //Debug.Log(idleTime);

        while (currentTime < idleTime)
        {
            currentTime += Time.deltaTime;
            yield return null;

        }

        SelectNewDestination();

    }


    private void OnPlayerInput()
    {

        SelectNewDestination();

    }

}
