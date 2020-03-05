using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MuteButton : MonoBehaviour
{
    Image image;
    public Sprite mute_on;
    public Sprite mute_off;
    public AudioMixer mixer;

    bool muted = false;

    private void Awake()
    {
        image = transform.GetChild(0).GetComponent<Image>();
    }

    public void OnClick()
    {

        if (!muted)
        {
            muted = true;
            mixer.SetFloat("masterVolume", -100.0f);
            image.sprite = mute_off;
        }
        else
        {
            muted = false;
            mixer.SetFloat("masterVolume", 0.0f);
            image.sprite = mute_on;
        }
    }
}
