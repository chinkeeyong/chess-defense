using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAgainButton : MonoBehaviour
{
    [HideInInspector] public GameController gameController;

    private void Awake()
    {
        gameController = GameObject.Find("/GameController").GetComponent<GameController>();
    }

    public void OnClick()
    {
        gameController.gamePhase = GameController.GamePhase.FADING_OUT_TO_RESET;
    }
}
