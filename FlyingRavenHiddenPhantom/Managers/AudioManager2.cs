using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// there are obviously better ways to do this, but this is quick dirty and can be modified easily
public class AudioManager2 : Singleton<AudioManager2>
{
    [Header("Manual References - Music")]
    public List<AudioClip> MenuMusicPlaylist;
    public List<AudioClip> TeamPreviewMusicPlaylist;
    public List<AudioClip> DefeatMusicPlaylist;
    public List<AudioClip> CombatMusicPlaylist;
    public List<AudioClip> CombatAmbientPlaylist;

    [Header("Manual References - SFX UI")]
    public AudioClip BTN_EndTurn;
    public AudioClip BTN_BeginPlay;
    public AudioClip EVENT_TurnStart;
    public AudioClip HoverCharacter;
    public AudioClip HoverDemon;
    public AudioClip HoverUI;
    public AudioClip UIClickSuccess;
    public AudioClip UIClickFail;

    [Header("Manual References - Components")]
    public AudioSource AS_Music;
    public AudioSource AS_Ambient;
    public AudioSource AS_SFX;
    public AudioSource AS_UI;

    [Header("Menu")]
    public bool MenuRandomTrack = false;
    public bool MenuLoopTrack = false;

    [Header("TeamPreview")]
    public bool TeamPreviewRandomTrack = false;
    public bool TeamPreviewLoopTrack = false;

    [Header("Defeat")]
    public bool DefeatRandomTrack = false;
    public bool DefeatLoopTrack = false;

    [Header("Combat")]
    public bool CombatRandomTrack = false;
    public bool CombatLoopTrack = false;

    [Header("Combat - Ambient")]
    public bool CombatAmbientRandomTrack = false;
    public bool CombatAmbientLoopTrack = false;

    // These are always "playing"
    private List<AudioClip> CurrentMusicPlaylist;
    private bool CurrentMusicPlaylistRandomTrack = false;
    private bool CurrentMusicPlaylistLoopTrack = false;
    private int CurrentMusicPlaylistTrack = 0;
    private List<AudioClip> CurrentAmbientPlaylist;
    private bool CurrentAmbientPlaylistRandomTrack = false;
    private bool CurrentAmbientPlaylistLoopTrack = false;
    private int CurrentAmbientPlaylistTrack = 0;
    private bool NoAmbient = true;

    public void Start()
    {
        // Start with the MainMenu
        MainMenu();

        // start the coroutines
        StartCoroutine(PlayMusic());
        StartCoroutine(PlayAmbient());
    }

    public void MainMenu()
    {
        // switch playlist
        SwitchMusicPlaylist(MenuMusicPlaylist, MenuRandomTrack, MenuLoopTrack);
        // no ambient
        StopAmbientPlaylist();
    }

    public void TeamPreview()
    {
        // switch playlist
        SwitchMusicPlaylist(TeamPreviewMusicPlaylist, TeamPreviewRandomTrack, TeamPreviewLoopTrack);
        // no ambient
        StopAmbientPlaylist();
    }

    public void Combat()
    {
        // switch playlist
        SwitchMusicPlaylist(CombatMusicPlaylist, CombatRandomTrack, CombatLoopTrack);
        // no ambient
        SwitchAmbientPlaylist(CombatAmbientPlaylist, CombatAmbientRandomTrack, CombatAmbientLoopTrack);
    }

    public void Defeat()
    {
        // switch playlist
        SwitchMusicPlaylist(DefeatMusicPlaylist, DefeatRandomTrack, DefeatLoopTrack);
        // no ambient
        StopAmbientPlaylist();
    }

    public void Play_BTN_EndTurn() 
    {
        AS_UI.PlayOneShot(BTN_EndTurn);
    }

    public void Play_BTN_BeginPlay()
    {
        AS_UI.PlayOneShot(BTN_BeginPlay);
    }
    public void Play_EVENT_TurnStart()
    {
        AS_UI.PlayOneShot(EVENT_TurnStart);
    }
    public void Play_HoverCharacter()
    {
        AS_UI.PlayOneShot(HoverCharacter);
    }
    public void Play_HoverDemon()
    {
        AS_UI.PlayOneShot(HoverDemon);
    }
    public void Play_HoverUI()
    {
        AS_UI.PlayOneShot(HoverUI);
    }
    public void Play_UIClickSuccess()
    {
        AS_UI.PlayOneShot(UIClickSuccess);
    }
    public void Play_UIClickFail()
    {
        AS_UI.PlayOneShot(UIClickFail);
    }

    public void SwitchMusicPlaylist(List<AudioClip> pNewPlaylist, bool pNewPlaylistRandom, bool pNewPlaylistLoop)
    {
        // new references
        CurrentMusicPlaylist = pNewPlaylist;
        CurrentMusicPlaylistRandomTrack = pNewPlaylistRandom;
        CurrentMusicPlaylistLoopTrack = pNewPlaylistLoop;

        // stop the music
        AS_Music.Stop();

        // update the track based on booleans
        if (pNewPlaylistRandom)
            CurrentMusicPlaylistTrack = Random.Range(0, CurrentMusicPlaylist.Count);
        else
            CurrentMusicPlaylistTrack = 0;

        // no need to start playing since we have the coroutine doing that for us automatically
    }

    // again, very wet
    public void SwitchAmbientPlaylist(List<AudioClip> pNewPlaylist, bool pNewPlaylistRandom, bool pNewPlaylistLoop)
    {
        // new references
        CurrentAmbientPlaylist = pNewPlaylist;
        CurrentAmbientPlaylistRandomTrack = pNewPlaylistRandom;
        CurrentAmbientPlaylistLoopTrack = pNewPlaylistLoop;

        // stop the music
        AS_Ambient.Stop();

        // update the track based on booleans
        if (pNewPlaylistRandom)
            CurrentAmbientPlaylistTrack = Random.Range(0, CurrentAmbientPlaylist.Count);
        else
            CurrentAmbientPlaylistTrack = 0;

        if (NoAmbient)
        {
            NoAmbient = false;
        }

        // no need to start playing since we have the coroutine doing that for us automatically
    }

    public void StopAmbientPlaylist()
    {
        NoAmbient = true;
        AS_Ambient.Stop();
    }

    IEnumerator PlayMusic()
    {
        while (true)
        {
            // halt progress until the clip stops playing
            while (AS_Music.isPlaying)
            {
                yield return new WaitForSeconds(0.1f);
            }

            // when the music stops check the bools to know what to do next
            
            // if it's looping the same track
            if (CurrentMusicPlaylistLoopTrack)
            {
                // play the same clip
                AS_Music.PlayOneShot(CurrentMusicPlaylist[CurrentMusicPlaylistTrack]);
                //Debug.Log("Playing new track...");
            }
            else
            {
                // if it's using random tracks
                if (CurrentMusicPlaylistRandomTrack)
                {
                    CurrentMusicPlaylistTrack = Random.Range(0, CurrentMusicPlaylist.Count);
                }
                // otherwise just increment and/or loop back to the first track
                else
                {
                    CurrentMusicPlaylistTrack++;
                    if (CurrentMusicPlaylistTrack >= CurrentMusicPlaylist.Count)
                        CurrentMusicPlaylistTrack = 0;
                }

                // regardless, play the next track
                AS_Music.PlayOneShot(CurrentMusicPlaylist[CurrentMusicPlaylistTrack]);
                //Debug.Log("Playing new track...");
            }

            yield return null;
        }
    }

    // almost identical to play music, but has a NoAmbient Check
    // very wet, but oh well
    IEnumerator PlayAmbient()
    {
        while (true)
        {
            // halt progress until the clip stops playing
            while (AS_Ambient.isPlaying)
            {
                yield return new WaitForSeconds(0.1f);
            }

            // when the ambience stops check the bools to know what to do next

            if (NoAmbient == false)
            {
                // if it's looping the same track
                if (CurrentAmbientPlaylistLoopTrack)
                {
                    // play the same clip
                    AS_Ambient.PlayOneShot(CurrentAmbientPlaylist[CurrentAmbientPlaylistTrack]);
                    //Debug.Log("Playing ambient sound...");
                }
                else
                {
                    // if it's using random tracks
                    if (CurrentAmbientPlaylistRandomTrack)
                    {
                        CurrentAmbientPlaylistTrack = Random.Range(0, CurrentAmbientPlaylist.Count);
                    }
                    // otherwise just increment and/or loop back to the first track
                    else
                    {
                        CurrentAmbientPlaylistTrack++;
                        if (CurrentAmbientPlaylistTrack >= CurrentAmbientPlaylist.Count)
                            CurrentAmbientPlaylistTrack = 0;
                    }

                    // regardless, play the next track
                    AS_Ambient.PlayOneShot(CurrentAmbientPlaylist[CurrentAmbientPlaylistTrack]);

                    //Debug.Log("Playing ambient sound...");
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// helper function, just returns a random audioclip from the given list.
    /// </summary>
    /// <param name="pList"></param>
    /// <returns></returns>
    public AudioClip GetRandomClip(List<AudioClip> pList)
    {
        return pList[Random.Range(0, pList.Count)];
    }

    /// <summary>
    /// helper function, plays a random clip from a list of clips
    /// </summary>
    /// <param name="pList"></param>
    public void PlayRandomSFX(List<AudioClip> pList, AudioSource pAS = null)
    {
        if (pAS == null)
        {
            AS_SFX.PlayOneShot(pList[Random.Range(0, pList.Count)]);
        }
        else
        {
            pAS.PlayOneShot(pList[Random.Range(0, pList.Count)]);
        }
    }

    /// <summary>
    /// helper function, plays a random clip from a list of clips
    /// </summary>
    /// <param name="pList"></param>
    public void PlayRandomUISFX(List<AudioClip> pList, AudioSource pAS = null)
    {
        if (pAS == null)
        {
            AS_UI.PlayOneShot(pList[Random.Range(0, pList.Count)]);
        }
        else
        {
            pAS.PlayOneShot(pList[Random.Range(0, pList.Count)]);
        }
    }

    /// <summary>
    /// helper function, plays a clip from an audio source
    /// </summary>
    public void PlaySFX(AudioClip pAC, AudioSource pAS = null)
    {
        if (pAS == null)
        {
            AS_SFX.PlayOneShot(pAC);
        }
        else
        {
            pAS.PlayOneShot(pAC);
        }
    }

    /// <summary>
    /// helper function, plays a clip from an audio source
    /// </summary>
    public void PlayUI(AudioClip pAC, AudioSource pAS = null)
    {
        if (pAS == null)
        {
            AS_UI.PlayOneShot(pAC);
        }
        else
        {
            pAS.PlayOneShot(pAC);
        }
    }


}

