using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;


public class CinemaManager : MonoBehaviour
{
    public static CinemaManager instance;

    [System.Serializable]
    public struct DialogueSequence
    {
        public string ShotName;
        public PlayableDirector playableDirectior;

    }

    public DialogueSequence[] dialogueSequence;

    public bool playSequenceOnStart;
    
    //Don't touch this on run time. 
    //This is exposed so you know when you call the input needed function, it's actually happening. 
    public bool isInputNeeded;

    //Defeated state will be coded eventually but it's not relevent right now so I hid it. 
    //public bool isDefeated;
    //public GameObject defeatedMessage; 


    private int currentLineInArray;
    private int currentDialogueSequence = 0;

    //These arent built for this project specifically. I will get back to it. 
    [HideInInspector]
    public Text textUI;
    [HideInInspector]
    public GameObject[] textBox;


    private void Awake()
    {
        instance = this;

    }

    // Start is called before the first frame update
    void Start()
    {
        currentLineInArray = 0;
        currentDialogueSequence = 0;

        if (playSequenceOnStart)
        {
            PlayNewSequence();
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        //I will remap the button later, don't use PauseForInput function yet
        if (isInputNeeded)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isInputNeeded = false;
                currentDialogueSequence += 1;
                PlayNewSequence();

            }

        }

        //if (isDefeated)
        //{
        //    dialogueSequence[2].playableDirectior.playableGraph.GetRootPlayable(0).SetSpeed(0);
        //    defeatedMessage.SetActive(true);

        //    if (Input.GetKeyDown(KeyCode.Space))
        //    {
        //        isDefeated = false;
        //        dialogueSequence[2].playableDirectior.time = 0;
        //        dialogueSequence[2].playableDirectior.Stop();
        //        dialogueSequence[2].playableDirectior.Evaluate();

        //        dialogueSequence[2].playableDirectior.Play();
        //        defeatedMessage.SetActive(false);
        //    }
        //}

    }

    
    public void PlayNewSequence()
    {
        dialogueSequence[currentDialogueSequence].playableDirectior.Play();

    }





    //Don't use this yet because the button has not been remapped
    public void PauseForInput()
    {
        //Instead of using Pause, use playableGraph.GetRootPlayable(0).SetSpeed(0) which allows everything to hold their position
        //Set speed to 1 to resume
        dialogueSequence[currentDialogueSequence].playableDirectior.playableGraph.GetRootPlayable(0).SetSpeed(0);
        isInputNeeded = true;
    }






    //This is Not build for this project. I will rewrite it when we want diagloue comes. 
    public void NextLine()
    {
        if (currentLineInArray != 0)
        {

            textBox[currentLineInArray - 1].SetActive(false);
        }

        //string currentLine = line[currentLineInArray];
        //int charCount = currentLine.Length;


        textBox[currentLineInArray].SetActive(true);

        textUI = textBox[currentLineInArray].GetComponentInChildren<Text>();

        //textUI.text = line[currentLineInArray];
        currentLineInArray += 1;


    }



}
