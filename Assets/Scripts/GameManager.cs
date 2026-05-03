using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public TileBoard Board;
    public CanvasGroup gameOver;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    private int score;

    private void Start()
    {
        NewGame();

    }

    public void NewGame()
    {
        SetScore(0);
        highScoreText.text = LoadHighScore().ToString();
        gameOver.alpha = 0f;
        gameOver.interactable = false;

        Board.ClearBoard();
        Board.CreateTile();
        Board.CreateTile();
        Board.enabled = true;
    }

    public void GameOver()
    {
        Board.enabled = false;
        gameOver.interactable = true;

        StartCoroutine(Fade(gameOver, 1f, 1f));
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float to, float delay)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        float duration = 0.5f;
        float from = canvasGroup.alpha;

        while(elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = to;

    }

    public void IncreaseScore(int points)
    {
        SetScore(score + points);
    }

    private void SetScore(int score)
    {
        this.score = score;

        scoreText.text = score.ToString();

        SaveHighScore();
    }
    private void SaveHighScore()
    {
        int highScore = LoadHighScore();
        if(score > highScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
        }
    }
    private int LoadHighScore()
    {
        return PlayerPrefs.GetInt("HighScore", 0);
    }
}
