using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public Image fadeCurtain;
    public string nextSceneName;

    [HideInInspector] public float animationTimer = 0f;

    [HideInInspector] public bool fadingOut = false;
    [HideInInspector] public float fadeTimer = 0f;
    [HideInInspector] public float fadingInAnimationTime = 1f; // Doesn't seem to work? Fuck Unity
    [HideInInspector] public float fadingOutAnimationTime = 1f;

    [HideInInspector] public bool gameStarted = false;

    private void Start()
    {
        fadeCurtain.color = new Color(0f, 0f, 0f, 1f);
        fadeTimer = fadingInAnimationTime;
        animationTimer = fadingInAnimationTime;
    }

    private void Update()
    {
        if (fadeTimer > -1f)
        {
            Color _newColor = fadeCurtain.color;
            if (fadingOut)
            {
                _newColor.a = Mathf.Lerp(1f, 0f, fadeTimer / fadingOutAnimationTime);
            }
            else
            {
                _newColor.a = Mathf.Lerp(0f, 1f, fadeTimer / fadingOutAnimationTime);
            }
            fadeCurtain.color = _newColor;
            fadeTimer -= Time.deltaTime;
        }

        if (animationTimer > 0f)
        {
            animationTimer -= Time.deltaTime;
            return;
        }

        if (gameStarted)
        {
            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
    }

    public void OnClick()
    {
        fadingOut = true;
        fadeTimer = fadingOutAnimationTime;
        animationTimer = fadingOutAnimationTime;
        gameStarted = true;
    }
}
