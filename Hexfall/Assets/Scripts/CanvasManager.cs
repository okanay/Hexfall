using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class CanvasManager : MonoBehaviour
{
    [HideInInspector] public int currentScore;
    [SerializeField] private Slider widthSlider, heightSlider, colorSlider;
    [SerializeField] private Text colorText,heightText,widthText;
    [SerializeField] private TMP_Text currentScoreText, highScoreText, hudHighScoreText, movesText;
    [SerializeField] private Transform gameOverMenu;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private Transform[] colorImages;
    
    public static CanvasManager Instance;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        InitializeHighestScore();
        ColorPotSync();
    }
    public void WidthSliderChange()
    {
        var sliderValue = widthSlider.value * 2 - 1;
        widthText.text = ($"Width ({sliderValue})");
        HexagonManager.Instance.width = (int) sliderValue;

        float mainCameraZ = Mathf.Clamp(sliderValue * 5, 35, 52);
        Camera.main.transform.position = Vector3.forward * -mainCameraZ;
    }
    public void HeightSliderChange()
    {
        var sliderValue = heightSlider.value;
        HexagonManager.Instance.height = (int) sliderValue; 
        heightText.text = ($"Height ({sliderValue})");
    }
    public void ColorSliderChange()
    {
        foreach (var color in colorImages) { color.gameObject.SetActive(false); }

        for (int i = 0; i < colorSlider.value; i++)
        {
            colorImages[i].gameObject.SetActive(true);
        }

        colorText.text = ($"Color ({colorSlider.value})");
        HexagonManager.Instance.colorVariableCount = (int) colorSlider.value;
    }
    public void ResetButton()
    {
        colorSlider.value = 5;
        heightSlider.value = 8;
        widthSlider.value = 5;
        
        ColorSliderChange();
        HeightSliderChange();
        WidthSliderChange();
    }
    public void CurrentScoreChange(int changeValue)
    {
        movesText.SetText(HexagonManager.Instance.Moves.ToString());
        currentScore += changeValue;
        currentScoreText.SetText(currentScore.ToString());

        if (currentScore >= PlayerPrefs.GetInt("HighScore"))
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
            PlayerPrefs.Save();
            InitializeHighestScore();
        }
    }
    public void InitializeHighestScore()
    {
        highScoreText.SetText(PlayerPrefs.GetInt("HighScore").ToString());
        hudHighScoreText.SetText("Highscore: " + PlayerPrefs.GetInt("HighScore").ToString());
    }
    private void ColorPotSync()
    {
        var colorList = HexagonManager.Instance.hexagonColors;
        List<Color> colors = new List<Color>();

        for (int i = 0; i < colorImages.Length; i++)
        {
            var newColor = colorList[i];
            colors.Add(newColor);
        }


        for (int i = 0; i < colors.Count; i++)
        {
            var image = colorImages[i].GetComponent<Image>();
            image.color = colors[i];
        }
    }
    public void ExitGame()
    {
        SceneManager.LoadScene(0);
    } 
    public void BombDropScoreSet(int score)
    {
        HexagonManager.Instance.BombRequiredSet(score);
    }
    public void SelectedSpriteSet(Image image)
    {
        image.sprite = sprites[1];
    }
    public void UnSelectedSpriteSet(Image image)
    {
        image.sprite = sprites[0];
    }
    public void GameOverMenu()
    {
        gameOverMenu.transform.gameObject.SetActive(true);
    }
}
