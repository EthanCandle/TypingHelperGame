using UnityEngine.Audio;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
public class AudioManager : MonoBehaviour
{
    public int currentVolume = 100;
    public bool isMuted;
    public Sound[] mainMenuMusic;
    public Sound[] gamePlayMusic;
    public Sound[] sounds;

    public static AudioManager instance;
    public AudioSource songCurrentlyPlaying;

    public int amountOfSoundsSoFar = 0;

    public bool shouldFadeIn = false, shouldFadeOut = false;
    public float fadeInOutTime = 1.0f;
    public float fadeInOutSpeed = 0.5f;
    public float songVolumeMax; // this is the music's current volume before transitioning

    public AudioMixerGroup audioMixerGroupMusic, audioMixerGroupSFX;

    // Start is called before the first frame update
    void Awake()
    {
        // create self if not already in
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);


        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();

            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

        }

        foreach (Sound s in mainMenuMusic)
        {
            s.source = gameObject.AddComponent<AudioSource>();

            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

        }
        foreach (Sound s in gamePlayMusic)
        {
            s.source = gameObject.AddComponent<AudioSource>();

            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = audioMixerGroupMusic;
        }

        AdjustVolume(currentVolume); 

    }

    public Sound FindSound(string name)
    {
        // goes through the sounds array
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return null;
        }
        return s;
    }


    public void PlaySoundInstantiate(Sound sourceSound)
    {
        if(sourceSound == null || isMuted)
        {
            return;
        }
       // print(sourceSound.name);
        // plays a sound by creating an empty object with the sound attached and deletes it when it ends
        amountOfSoundsSoFar++;

        // FindObjectOfType<AudioManager>().PlaySoundInstantiate(deathSFX);
        // use the var:     public Sound deathSFX;

        // creates the object
        GameObject newGameObject = new GameObject($"Sound Holder ({amountOfSoundsSoFar})", typeof(AudioSource));

        // addes the audiosource and sets it
        sourceSound.source = newGameObject.GetComponent<AudioSource>();

        sourceSound.source.clip = sourceSound.clip;

        sourceSound.source.volume = currentVolume / 100.0f * sourceSound.volumeOriginal;
        sourceSound.source.pitch = sourceSound.pitch;
        sourceSound.source.loop = sourceSound.loop;
        sourceSound.source.outputAudioMixerGroup = audioMixerGroupSFX;
        // plays teh sound
        sourceSound.source.Play();

        // deletes objects after finishing (might mess up if pitch is different, maybe multiply or divide by pitch?)
        Destroy(newGameObject, sourceSound.clip.length / sourceSound.pitch);
    }

    public void PlaySoundInstantiate(string nameOfSound)
    {
        // plays a sound by creating an empty object with the sound attached and deletes it when it ends
        Sound sourceSound = FindSound(nameOfSound);

        // call using this
        // FindObjectOfType<AudioManager>().PlaySoundInstantiate(deathSFX);
  

        // creates empty object with an audioSource on it
        GameObject newGameObject = new GameObject($"Sound Holder (1)", typeof(AudioSource));
        //AddComponent
        sourceSound.source = newGameObject.GetComponent<AudioSource>();

        sourceSound.source.clip = sourceSound.clip;

        sourceSound.source.volume = sourceSound.volume;
        sourceSound.source.pitch = sourceSound.pitch;
        sourceSound.source.loop = sourceSound.loop;

        // plays the sound
        sourceSound.source.Play();

        // deletes objects after finishing (might mess up if pitch is different, maybe multiply or divide by pitch?)
        Destroy(newGameObject, sourceSound.clip.length / sourceSound.pitch);
    }




    private void Start()
    {

       // PlayRandomGamePlayMusic();
        /*
         * Lo-Fi
         * Ambient, Good at 1.3 pitch. soft and child
         * 
         * BossaNova
         * Moonlit I like, feels like main menu music, cafe loading screen
        */
    }

    private void Update()
    {
        if (shouldFadeOut)
        {
            songCurrentlyPlaying.volume -= fadeInOutSpeed * Time.unscaledDeltaTime;
            if (songCurrentlyPlaying.volume <= 0)
            {
                shouldFadeOut = false;
            }
        }

        if (shouldFadeIn)
        {
            songCurrentlyPlaying.volume += fadeInOutSpeed / 5 * Time.unscaledDeltaTime;
            if (songCurrentlyPlaying.volume >= songVolumeMax)
            {

                shouldFadeIn = false;
                songCurrentlyPlaying.volume = songVolumeMax;
            }
        }

    }

    public void GetTransitionSpeed()
    {
        print(songVolumeMax);
        songVolumeMax = songCurrentlyPlaying.volume;
        fadeInOutSpeed = songCurrentlyPlaying.volume / fadeInOutTime;
        print(songVolumeMax);
    }

    public void FadeOut()
    {
        // this should only be called by the transition manager
        shouldFadeOut = true;
        GetTransitionSpeed();

    }
    public void FadeIn()
    {
        // this should only be called by the transition manager
        // sets the fade in to true, gets teh new transition speed based on the volume of the song, mutes teh song so it comes back in teh fade
        PlayLevelMusic();
        shouldFadeIn = true;
        GetTransitionSpeed();
        songCurrentlyPlaying.volume = 0;
    }
    // FindObjectOfType<AudioManager>().Play("BombDeport");

    public void PlaySFX(string name)
    {
        //  FindObjectOfType<AudioManager>().PlaySFX("BombDeport");

        // goes through the sounds array
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();

    }

    public void PlayMainMenuTheme(string name)
    {
        //  FindObjectOfType<AudioManager>().PlayMainMenuTheme("BombDeport");

        StopCurrentSong();

        // goes through the music array
        Sound s = Array.Find(mainMenuMusic, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
        songCurrentlyPlaying = s.source;

    }

    public void PlayLevelMusic()
    {
        Sound s = gamePlayMusic[SceneManager.GetActiveScene().buildIndex];

        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.source.Play();
        songCurrentlyPlaying = s.source;
        print(songCurrentlyPlaying.volume);
    }

    public void StopCurrentSong()
    {
        if (songCurrentlyPlaying == null)
        {
            return;
        }

        songCurrentlyPlaying.Stop();
    }

    public void PauseCurrentSong()
    {
        if (songCurrentlyPlaying == null)
        {
            return;
        }

        songCurrentlyPlaying.Pause();
    }

    public void UnPauseCurrentSong()
    {
        if (songCurrentlyPlaying == null)
        {
            return;
        }

        songCurrentlyPlaying.UnPause();
    }

    public void PlayGamePlayMusic(string name)
    {
        //  FindObjectOfType<AudioManager>().PlayGamePlayMusic("Ambient");

        StopCurrentSong();
        // goes through the music array
        Sound s = Array.Find(gamePlayMusic, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        s.source.Play();
        songCurrentlyPlaying = s.source;
    }


    public void PlayRandomGamePlayMusic()
    {
        //   FindObjectOfType<AudioManager>().PlayRandomGamePlayMusic();

        // plays a random song from the music playlist
        // called when a song ends (need to implement)

        StopCurrentSong();

        Sound s = gamePlayMusic[UnityEngine.Random.Range(0, gamePlayMusic.Length)];

        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        s.source.Play();
        songCurrentlyPlaying = s.source;
    }

    // these called by some sort of game manager
    public void IncreaseVolume(int amountToChange)
    {
        currentVolume += amountToChange;
        if (currentVolume > 100)
        {
            currentVolume = 100;
        }
        AdjustVolume(currentVolume);
    }

    public void DecreaseVolume(int amountToChange)
    {
        currentVolume -= amountToChange;
        if (currentVolume < 0)
        {
            currentVolume = 0;
        }
        AdjustVolume(currentVolume);
    }

    public void SetVolume(int amountToChange)
    {
        currentVolume = amountToChange;
        AdjustVolume(currentVolume);
        isMuted = false;
    }

    public void MuteVolume()
    {
        AdjustVolume(0);
        isMuted = true;
    }

    public void UnMuteVolume()
    {
        AdjustVolume(currentVolume);
        isMuted = false;
    }

    public void AdjustVolume(int volume = 100)
    {
        print("Adjust volume start");
        // volume range is between 0-100
        foreach (Sound s in sounds)
        {
            s.source.volume = volume / 100.0f * s.volumeOriginal;
        }

        foreach (Sound s in mainMenuMusic)
        {
            s.source.volume = volume / 100.0f * s.volumeOriginal;

        }
        foreach (Sound s in gamePlayMusic)
        {
            s.source.volume = volume / 100.0f * s.volumeOriginal;

        }
        print("Adjust volume end");
    }


}
