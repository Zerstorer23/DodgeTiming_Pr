using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] audioLists;
    AudioSource audioPlayer;

    private static AudioManager prAudioManager;
    public static AudioManager instance
    {
        get
        {
            if (!prAudioManager)
            {
                prAudioManager = FindObjectOfType<AudioManager>();
                if (!prAudioManager)
                {
                }
                else
                {
                }
            }

            return prAudioManager;
        }
    }
    private void Awake()
    {
        EventManager.StartListening(MyEvents.EVENT_SHOW_PANEL, OnSceneChanged);
        audioPlayer = GetComponent<AudioSource>();
    }
    public static bool GetMuted() {
        return instance.audioPlayer.mute;
    }
    public static void SetMute(bool enable)
    {
        instance.audioPlayer.mute = enable;
    }

    private void OnDestroy()
    {
        EventManager.StopListening(MyEvents.EVENT_SHOW_PANEL, OnSceneChanged);
    }

    private void OnSceneChanged(EventObject arg0)
    {
        ScreenType sceneIdx = (ScreenType)arg0.objData;
        switch (sceneIdx)
        {
            case ScreenType.PreGame:
                PlayAudio(audioLists[0]);
                break;
            case ScreenType.InGame:
                PlayAudio(audioLists[UnityEngine.Random.Range(1,audioLists.Length)]);
                break;
            case ScreenType.GameOver:
                break;
        }

    }
    public static void PlayAudio(AudioClip audio) {
       instance.audioPlayer.clip = audio;
       instance.audioPlayer.Play();
    }
    public static void PlayAudioOneShot(AudioClip audio)
    {
        if (GetMuted()) return;
        instance.audioPlayer.PlayOneShot(audio);
    }
}
