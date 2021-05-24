using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance { get; private set; }

    [SerializeField] private GameObject audioSourcePrefab;
    [SerializeField] private Transform audioParent;

    private List<AudioSource> nonLoopingSounds;
    private Dictionary<AudioClip, GameObject> loopingSounds;

    private void Start() {
        Instance = this;

        nonLoopingSounds = new List<AudioSource>();
        loopingSounds = new Dictionary<AudioClip, GameObject>();
    }

    public void PlaySound(AudioClip sound, float time, float delay = 0f, float volume = 1f, Action<AudioClip> cbOnFinishedPlaying = null) {
        StartCoroutine(PlaySoundActual(sound, time, delay, volume, cbOnFinishedPlaying));
    }

    private IEnumerator PlaySoundActual(AudioClip sound, float time, float delay, float volume, Action<AudioClip> cbOnFinishedPlaying) {
        yield return new WaitForSeconds(delay);

        GameObject obj = GameObject.Instantiate(audioSourcePrefab, audioParent);
        AudioSource src = obj.GetComponent<AudioSource>();
        src.clip = sound;
        src.volume = volume;
        src.Play();
        nonLoopingSounds.Add(src);

        yield return new WaitForSeconds(time);

        nonLoopingSounds.Remove(src);
        Destroy(obj);
        if (cbOnFinishedPlaying != null) {
            cbOnFinishedPlaying(sound);
        }
    }

    public void SetVolume(AudioClip sound, float volume) {
        for (int i = 0; i < nonLoopingSounds.Count; i++) {
            if (nonLoopingSounds[i].clip == sound) {
                nonLoopingSounds[i].volume = volume;
            }
        }
    }

    public void PlayLoopingSound(AudioClip sound, float volume = 1f) {
        if (!loopingSounds.ContainsKey(sound)) {
            GameObject obj = GameObject.Instantiate(audioSourcePrefab, audioParent);
            AudioSource src = obj.GetComponent<AudioSource>();
            src.clip = sound;
            src.volume = volume;
            src.loop = true;
            src.Play();
            loopingSounds.Add(sound, obj);
        } else {
            if (volume == 0) {
                StopLoopingSound(sound);
            } else {
                loopingSounds[sound].GetComponent<AudioSource>().volume = volume;
            }
        }
    }

    public void StopLoopingSound(AudioClip sound) {
        if (loopingSounds.ContainsKey(sound)) {
            Destroy(loopingSounds[sound]);
        }
    }
}
