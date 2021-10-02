using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour {

    float musicVol;
    float sfxVol;

    void Start() {
        InvokeRepeating("Check", 0.1f, 0.1f);
        musicVol = GetMusic().volume;
        sfxVol = GetSFX()[0].volume;
    }

    AudioSource GetMusic() {
        return GameObject.Find("Music").GetComponent<AudioSource>();
    }

    List<AudioSource> GetSFX() {
        var all = new List<AudioSource>(FindObjectsOfType<AudioSource>());
        all.Remove(GetMusic());
        return all;
    }

    void Update() {
        if (Input.GetKeyDown(",")) sfxVol -= 0.1f;
        else if (Input.GetKeyDown(".")) sfxVol += 0.1f;
        if (Input.GetKeyDown("[")) musicVol -= 0.1f;
        else if (Input.GetKeyDown("]")) musicVol += 0.1f;
    }

    void Check() {
        var msg = string.Format(
            "Settings:\nmusic   {0}/10\nsfx:    {1}/10"
            + "\n\nchange music:\nuse [ and ]"
            + "\n\nchange sfx:\nuse < and >",
            (int) (musicVol * 10), (int) (sfxVol * 10));
        
        GetComponentInChildren<Text>().text = msg;

        GetMusic().volume = musicVol;
        foreach (var a in GetSFX()) {
            a.volume = sfxVol;
        }
    }
}
