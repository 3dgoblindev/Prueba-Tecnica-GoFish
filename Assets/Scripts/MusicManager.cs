using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    // This script is just an example of how to use the AudioManager to play music.
    // The ideal would be to have this be done through a UI that allows you to change the music in the game
    [SerializeField] private AudioClip myMusicClip;
    // Start is called before the first frame update
    void Start()
    {
        AudioManager.Instance.PlayMusic(myMusicClip);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
