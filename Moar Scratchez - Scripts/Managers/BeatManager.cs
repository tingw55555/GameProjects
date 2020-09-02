
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    public static BeatManager instance;

    public GameObject[] notePrefabs;
    public GameObject musicBox;

    public AudioClip positiveSFX;
    public AudioClip negativeSFX;
    //public AudioClip holdSFX;
    public AudioSource sfxPlayer;


    private int numOfTap;
    private int numOfHold;
    private int currentNoteInOrder = 1;

    //public int totalScore;
    //public int scorePerNote;
    private NoteType currentNoteType;

    public KeyCode keyToPress;




    private int totalNumOfNotes;
    public Queue<GameObject> noteCombination = new Queue<GameObject>();
    public Queue<GameObject> spawnNotes = new Queue<GameObject>();

    public GameObject currentActiveNote;
    public NoteType currentActiveNoteType;
    private Hold currentHoldNote;

    public bool active;

    private float delay = 1;
    //public float speedModifer = 2;


    private float notePixelSize = 10.24f;
    private float checkingRangeMod = 10;

    public float currentHappeiness = 0;
    private float happinessPerhit;
    private float pentalityPerMiss;
    private float holdingHappiness;

    public bool isHolding = false;

    private bool currentLevelFinished = false;

    //Public variables that are used by manager classes but should not be touched 
    [HideInInspector]
    public Transform noteCheckPos;
    [HideInInspector]
    public bool withinHitZone = false;
    [HideInInspector]
    public bool ignoreNote = true;
    [HideInInspector]
    public bool holdingTimerStart = false;
    [HideInInspector]
    public Transform hitZone;
    [HideInInspector]
    public Transform startPoint;



    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {

    }


    void Update()
    {

        if (GameManager.instance.currentState == GameManager.GameState.PettingTime)
        {
            CheckifNoteWithinHitZone();

            if (Input.GetKeyUp(keyToPress))
            {
                holdingTimerStart = false;

                if (isHolding == true)
                {

                    if (currentHoldNote != null && !currentHoldNote.isFinished)
                    {
                        NoteMiss();
                        UIManager.instance.StopHoldSFX();
                    }
                }

                isHolding = false;
            }

            if (Input.GetKeyDown(keyToPress))
            {

                if (ignoreNote == false)
                {
                    if (withinHitZone)
                    {
                        holdingTimerStart = true;

                        if (currentActiveNote.GetComponent<NoteController>().currentType == NoteType.Tap)
                        {
                            NoteHit();
                        }
                        else if (currentActiveNote.GetComponent<NoteController>().currentType == NoteType.Hold)
                        {
                            currentHoldNote = currentActiveNote.GetComponent<Hold>();
                            isHolding = true;
                            SnapHoldNoteToHitZone();

                        }

                    }
                    else
                    {
                        if (currentActiveNote != null)
                        {
                            NoteMiss();

                        }

                    }
                }

            }

            if (noteCombination.Count == 0)
            {
                CheckIfFinished();
            }
        }

    }

    public void SnapHoldNoteToHitZone()
    {
        UIManager.instance.PlayHoldSFX();
        currentHoldNote.stopMoving = true;

        currentActiveNote.GetComponent<RectTransform>().SetParent(hitZone);
        currentActiveNote.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
    }


    public void LoadLevel()
    {
        currentHappeiness = 0;
        UIManager.instance.UpdateHappiness();

        active = true;
        currentLevelFinished = false;

        happinessPerhit = LevelManager.instance.GetLevelInfo().happinessPerNoteHit;
        pentalityPerMiss = LevelManager.instance.GetLevelInfo().penaltyPerNoteMiss;
        holdingHappiness = LevelManager.instance.GetLevelInfo().happinessWhileHold;


        GenerateNoteCombination();
        SetMusic();


    }


    public void CheckIfFinished()
    {

        if (spawnNotes.Count == 0 && currentActiveNote == null)
        {
            if (currentLevelFinished == false)
            {
                currentLevelFinished = true;
                //When the list is empty. Let the game manager know, which will send a message to all manager classes 
                //instead of controlling the level transition in here
                withinHitZone = false;
                GameManager.instance.OnCurrentLevelFinished();
                musicBox.GetComponent<AudioSource>().Stop();

            }

        }
    }

    public void GenerateNoteCombination()
    {
        //Reset the list before we add to it again
        noteCombination.Clear();

        //Get the info from level manager (which is set in inspector)
        numOfTap = LevelManager.instance.GetLevelInfo().numOf_Tap;
        numOfHold = LevelManager.instance.GetLevelInfo().numOf_Hold;

        totalNumOfNotes = LevelManager.instance.GetTotalNumOfNotes();

        //Loop through the total number of notes 
        for (int i = 0; i < totalNumOfNotes; i++)
        {
            //And randomize their order in the list 
            currentNoteType = DetermineNote();

            //if we have already generated enough taps/holds, we set the note to the opposite
            if (currentNoteType == NoteType.Tap)
            {
                if (numOfTap == 0)
                {
                    currentNoteType = NoteType.Hold;
                }
                else
                {
                    numOfTap -= 1;

                }

            }
            else if (currentNoteType == NoteType.Hold)
            {
                if (numOfHold == 0)
                {
                    currentNoteType = NoteType.Tap;
                }
                else
                {
                    numOfHold -= 1;
                }
            }

            //Finally, add whatever note generated to the list 
            noteCombination.Enqueue(notePrefabs[(int)currentNoteType]);
        }

        StartSpawningNotes();

    }



    public NoteType DetermineNote()
    {
        int r = Random.Range(0, 100);

        if (r >= 50)
        {
            return NoteType.Tap;

        }
        else
        {
            return NoteType.Hold;
        }

    }


    public void StartSpawningNotes()
    {
        StartCoroutine(SpawnNewNote());
    }


    IEnumerator SpawnNewNote()
    {
        currentNoteInOrder = 0;

        yield return new WaitForSeconds(1);

        musicBox.GetComponent<AudioSource>().Play();

        while (active)
        {
           
            if (noteCombination.Count != 0)
            {
                //if the list is NOT empty. We take one note out of the list and spawn it. 
                GameObject newNote = Instantiate(noteCombination.Dequeue(), startPoint);
                newNote.transform.localPosition = Vector3.zero;


                spawnNotes.Enqueue(newNote);

                if (currentActiveNote == null)
                {
                    currentActiveNote = spawnNotes.Dequeue();
                    currentActiveNoteType = currentActiveNote.GetComponent<NoteController>().currentType;
                }

                NoteController noteController = newNote.GetComponent<NoteController>();
                noteController.InitializeBar();
                noteController.speed = -LevelManager.instance.GetLevelInfo().beatTempo;
                newNote.name = noteController.currentType + " , " + currentNoteInOrder + "/" + totalNumOfNotes;

                currentNoteInOrder += 1;


                delay = -noteController.barLength / noteController.speed;
                delay += Random.Range(LevelManager.instance.GetLevelInfo().minDelay, LevelManager.instance.GetLevelInfo().maxDelay);


                yield return new WaitForSeconds(delay);


            }
            else
            {

                active = false;
            }
        }




    }






    public void SetMusic()
    {
        AudioSource audioPlayer = musicBox.GetComponent<AudioSource>();
        audioPlayer.clip = LevelManager.instance.GetMusic();
        //audioPlayer.Play();

    }


    public void DestroyCurrentNote()
    {
        Destroy(currentActiveNote);

        if (spawnNotes.Count != 0)
        {
            currentActiveNote = spawnNotes.Dequeue();

        }
        else
        {
            currentActiveNote = null;
        }

    }



    public void NoteHit()
    {
        currentHappeiness += happinessPerhit;
        UIManager.instance.UpdateHappiness();
        UIManager.instance.PawHitAnimation();

        PlayPositveSFX();

        UIManager.instance.StopHoldSFX();

        DestroyCurrentNote();


    }



    public void AddHoldingHappiness()
    {
        currentHappeiness += holdingHappiness*Time.deltaTime;
        UIManager.instance.UpdateHappiness();
       
    }

    public void ApplyPenality()
    {
        PlayNegativeSFX();

        currentHappeiness -= pentalityPerMiss;

        if (currentHappeiness < 0)
        {
            currentHappeiness = 0;
        }
        UIManager.instance.UpdateHappiness();
    }

    public void NoteMiss()
    {


        UIManager.instance.PawMissAnimation();
        ApplyPenality();

        DestroyCurrentNote();




    }




    public void CheckifNoteWithinHitZone()
    {
        if (currentActiveNote != null)
        {
            float distance = Vector3.Distance(hitZone.position, currentActiveNote.transform.position);

            //float checkRange = currentNote.transform.localScale.x;

            if (distance > notePixelSize * 75)
            {
                ignoreNote = true;
                return;
            }

            ignoreNote = false;

            if (distance <= notePixelSize * checkingRangeMod)
            {

                withinHitZone = true;
                //NoteHit();

            }
            else
            {
                withinHitZone = false;
            }

        }

    }





    public void PlayPositveSFX()
    {
        sfxPlayer.clip = positiveSFX;
        sfxPlayer.Play();
    }

    public void PlayNegativeSFX()
    {
        sfxPlayer.clip = negativeSFX;
        sfxPlayer.Play();
    }




}
