using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mikuBGM : MonoBehaviour
{
    public AudioSource bgm;
    public AudioSource mikubgm;

    private bool mikuBGM_playing = false;

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!mikuBGM_playing)
            {
                bgm.Pause();
                mikubgm.Play();
                mikuBGM_playing = true;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (mikuBGM_playing)
            {
                mikubgm.Pause();
                bgm.UnPause();
                mikuBGM_playing = false;
            }
        }
    }
}