using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public GameManager gm;
    public SoundManager sm;


    public GameObject start;
    public GameObject game;
    public GameObject options;
    public GameObject finish;

    public Image soundIM;
    public Image musicIM;

    public Image nextStage;
    public Image again;

    public Text finishText;
    

    public void ToggleOptions(bool active)
    {
        options.SetActive(active);
        start.SetActive(!active);
    }

    public void ToggleGameStart(bool goGame)
    {
        game.SetActive(goGame);
        start.SetActive(!goGame);
        SwicthPlayOrNoGame(goGame);

        if (!goGame)
        {
            gm.Restart();
        }
    }

    public void ToggleMusicOrSound(bool music)
    {

        if (music)
        {
            sm.music = !sm.music ;
            ToggleImage(musicIM, sm.music);
        }
        else
        {
            sm.sound = !sm.sound;
            ToggleImage(soundIM, sm.sound);
        }
    }

    void SwicthPlayOrNoGame(bool goGame)
    {
        if (goGame)
        {
            gm.gameMode = GameManager.GameMode.Play;
        }
        else
        {
            gm.gameMode = GameManager.GameMode.NoGame;
        }
    }


    void ToggleImage(Image ımage , bool onOff)
    {

        Color onColor = new Color(1, 1, 1, 1);
        Color offColor = new Color(1, 1, 1, 0.5f);

        ımage.color = onOff ? onColor : offColor;
    }

    public void OpenFinish(bool win,bool levels)
    {
        SwicthPlayOrNoGame(false);

        finish.SetActive(true);
        game.SetActive(false);

        finishText.text = win ? "WIN" : "Try Again";

        nextStage.enabled = levels;
        again.enabled = !levels;

    }

    public void FinishToStart()
    {
        finish.SetActive(false);
        start.SetActive(true);

        gm.Restart();
    }

    public void Again()
    {
        finish.SetActive(false);
        game.SetActive(true);

        gm.Restart();

        gm.gameMode = GameManager.GameMode.Play;
    }
}
