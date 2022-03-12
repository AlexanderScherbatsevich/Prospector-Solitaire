using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    private bool _musicState = true;
    private bool _soundState = true;
    public static bool MenuIsOpened = false;

    public Toggle soundToggle;
    public Toggle musicToggle;

    public Transform menuPanel;
    public Transform soundOff;
    public Transform musicOff;

    private void Start()
    {

        //if (PlayerPrefs.HasKey("MusicState"))
        //{
        //    _musicState = (PlayerPrefs.GetInt("MusicState") != 0);
        //}
        //PlayerPrefs.SetFloat("MusicState", (_musicState ? 1 : 0));

        //if (PlayerPrefs.HasKey("SoundState"))
        //{
        //    _soundState = (PlayerPrefs.GetInt("SoundState") != 0);
        //}
        //PlayerPrefs.SetFloat("SoundState", (_soundState ? 1 : 0));

        //musicToggle.isOn = true;
        //soundToggle.isOn = true;


        //musicTurn();
        //soundTurn();
        MenuIsOpened = false;
        Time.timeScale = 1f;
    }

    public void NewGame()
    {
        AudioManager.S.clickButton.Play();
        SceneManager.LoadScene("__Prospector_Scene_0");
    }

    public void ExitGame()
    {
        AudioManager.S.clickButton.Play();
        Debug.Log("exitGame");
        Application.Quit();
    }

    public void CloseMenu()
    {
        AudioManager.S.clickButton.Play();       
        menuPanel.gameObject.SetActive(false);
        Time.timeScale = 1f;
        MenuIsOpened = false;
    }

    public void OpenMenu()
    {
        AudioManager.S.clickButton.Play();        
        menuPanel.gameObject.SetActive(true);
        menuPanel.SetAsLastSibling();
        Time.timeScale = 0f;
        MenuIsOpened = true;

    }

    public void soundTurn()
    {
        //if (SoundState)
        //{
        //    AudioManager.S.Master.audioMixer.SetFloat("MasterVolume", -80);
        //    soundOff.gameObject.SetActive(true);
        //    SoundState = false;
        //}
        //else
        //{
        //    AudioManager.S.Master.audioMixer.SetFloat("MasterVolume", 0);
        //    soundOff.gameObject.SetActive(false);
        //    SoundState = true;
        //} 

        _soundState = !_soundState;
        //soundOff.gameObject.SetActive(SoundState);
        AudioManager.S.SoundState(_soundState, AudioManager.S.Master);
        AudioManager.S.clickButton.Play();

        //PlayerPrefs.SetFloat("SoundState", (_soundState ? 1 : 0));

    }

    public void musicTurn()
    {
        //if (MusicState)
        //{
        //    AudioManager.S.Music.audioMixer.SetFloat("MusicVolume", -80);
        //    musicOff.gameObject.SetActive(true);
        //    MusicState = false;
        //}
        //else
        //{
        //    AudioManager.S.Music.audioMixer.SetFloat("MusicVolume", 0);
        //    musicOff.gameObject.SetActive(false);
        //    MusicState = true;
        //}

        _musicState = !_musicState;
        //musicOff.gameObject.SetActive(MusicState);
        AudioManager.S.SoundState(_musicState, AudioManager.S.Music);
        AudioManager.S.clickButton.Play();
        //PlayerPrefs.SetFloat("MusicState", (_musicState ? 1 : 0));
    }
}
