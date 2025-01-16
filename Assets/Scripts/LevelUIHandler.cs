using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class LevelUIHandler : MonoBehaviour
{
    [Header("")]
    [Header("Game Over Screens")]
    public GameOverScreen EndlessGameOverScreen;
    public GameOverScreen LevelGameOverScreen;

    [Header("Texts")]
    public TMP_Text EndScoreLevel;
    public TMP_Text EndScoreEndless;
    public TMP_Text EndHighScore;
    public TMP_Text CurrentScore;
    public List<TMP_Text> StarRequirementTexts;
    public List<GameObject> Stars;
    public GameObject ObjectiveScreen;

    [Header("Animator")]
    public Animator Animator;

    [Header("Events")]
    public UnityEvent GotStarLevelEndEvent;
    public UnityEvent NoStarLevelEndEvent;
    public bool HasCompletedFirstPlay{ get; private set; }

    private int score;
    private int stars;

    void Awake()
    {
        HasCompletedFirstPlay = PlayerPrefs.GetInt("HasCompletedFirstPlay", 0) == 1;
        if(!HasCompletedFirstPlay)
        {
            SetScoreTextVisibility(false);
            // crane.EnableRopeBreak = false;
        }
    }

    public void SetScoreTextVisibility(bool visible)
    {
        CurrentScore.gameObject.SetActive(visible);
    }

    public void OnScoreUpdate(int score, int stars)
    {
        CurrentScore.text = $"{score}";
        this.score = score;
        this.stars = stars;
    }

    public void OnLevelGameOver()
    {
        CurrentScore.text = "";
        EndScoreLevel.text = $"{score}";
        // EndStars.text = $"{stars}";
        if(stars > 0)
        {
            GotStarLevelEndEvent.Invoke();
        }
        else
        {
            NoStarLevelEndEvent.Invoke();
        }

        if(stars == 1)
        {
            Stars[0].transform.localScale = new Vector2(2f, 2f);
            Stars[0].SetActive(true);
        }
        else if(stars == 2)
        {
            Stars[1].SetActive(true);
            Stars[2].SetActive(true);
        }
        else if(stars == 3)
        {
            Stars[0].SetActive(true);
            Stars[1].SetActive(true);
            Stars[2].SetActive(true);
        }

        Animator.SetTrigger("onGameOver");
        LevelGameOverScreen.Setup();
    }

    public void OnLevelStart(LevelStructure structure)
    {
        ObjectiveScreen.SetActive(true);
        StarRequirementTexts[0].text = $"{structure.FirstStarScoreRequirement}";
        StarRequirementTexts[1].text = $"{structure.SecondStarScoreRequirement}";
        StarRequirementTexts[2].text = $"{structure.ThirdStarScoreRequirement}";

        if (Input.GetMouseButtonDown(0))
        {
            ObjectiveScreen.SetActive(true);
        }
    }

    public void HideObjective()
    {
        ObjectiveScreen.SetActive(false);
    }

    public void OnEndlessGameOver()
    {
        SetScoreTextVisibility(false);
        EndScoreEndless.text = $"{score}";
        EndHighScore.text = $"{PlayerPrefs.GetInt("HighScore", 0)}";
        Animator.SetTrigger("onGameOver");
        EndlessGameOverScreen.Setup();
    }

}
