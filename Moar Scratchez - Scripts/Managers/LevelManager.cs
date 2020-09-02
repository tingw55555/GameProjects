using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NoteType
{
    Tap = 0,
    Hold = 1,
    DoubleTap = 2,

};


public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;



    [System.Serializable]
    public struct Level
    {
        public string levelName;

        public int numOf_Tap;
        public int numOf_Hold;


        public float beatTempo;
        public float minDelay;
        public float maxDelay;

        public float happinessPerNoteHit;
        public float happinessWhileHold;
        public float penaltyPerNoteMiss;

        public AudioClip backgroundMusic;


    }

    public Level[] levels;

    //[[Inspector only]] Do not change these through code. 
    public int currentLevel;

    private void Awake()
    {
        instance = this;
    }



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    /// <summary>
    /// Get Current Level Info. This entails number of Tap & Hold notes. Speed and length
    /// </summary>
    /// <returns></returns>
    public Level GetLevelInfo()
    {

        return levels[currentLevel];
    }

    //
    public int GetTotalNumOfNotes()
    {
        return levels[currentLevel].numOf_Tap + levels[currentLevel].numOf_Hold;
    }

    public AudioClip GetMusic()
    {
        return levels[currentLevel].backgroundMusic;
    }

    //Currently there is no meaningful implementation of beatTempo and BPM so ignore it for now 
    //public float GetFrequency()
    //{
    //    return levels[currentLevel].beatTempo / levels[currentLevel].beatsPerMin;
    //}


    /// <summary>
    /// Update current level if there are more levels left. If not, returns false;
    /// </summary>
    /// <returns></returns>
    public bool CheckAndUpdateCurrentLevel()
    {
        currentLevel += 1;
        
        if (levels.Length > currentLevel)
        {

            return true;

        }
        else
        {


            return false;
        }

    }



}
