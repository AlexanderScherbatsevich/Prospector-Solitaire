using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager S;
    public AudioMixerGroup Music;
    public AudioMixerGroup Master;

    public AudioSource clickButton;
    public AudioSource backgroundMusic;
    public AudioSource soundWin;
    public AudioSource soundLose;
    public AudioSource cardFromDrawpile;
    public AudioSource cardFromTableau;

    private void Start()
    {
        S = this;
        Music.audioMixer.SetFloat("MusicVolume", 0);
    }

    public void SoundState(bool soundState, AudioMixerGroup mixerGroup)
    {
        if (mixerGroup == Music)
        {
            if (soundState) mixerGroup.audioMixer.SetFloat("MusicVolume", 0);
            else mixerGroup.audioMixer.SetFloat("MusicVolume", -80);
        }
        else if (mixerGroup == Master)
        {
            if (soundState) mixerGroup.audioMixer.SetFloat("MasterVolume", 0);
            else mixerGroup.audioMixer.SetFloat("MasterVolume", -80);
        }        
    }
}
