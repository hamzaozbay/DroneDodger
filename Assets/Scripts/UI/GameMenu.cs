using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI _currentLevel, _nextLevel, _score;
    [SerializeField] private Image _levelProgressFill;

    private float _levelProgressFillSpeed;
    private bool _canLevelProgress = false;




    private void Update() {
        if (!_canLevelProgress) return;
        if (_levelProgressFill.fillAmount == 1f) return;

        _levelProgressFill.fillAmount += _levelProgressFillSpeed * Time.deltaTime;
    }


    public void SetUI() {
        Reset();
        Score(GameManager.instance.CurrentScore);
        int currentLevel = GameManager.instance.CurrentLevel;
        _currentLevel.text = (currentLevel + 1).ToString();

        if (currentLevel + 2 > GameManager.instance.LevelCount) {
            _nextLevel.text = "-";
            return;
        }

        _nextLevel.text = (currentLevel + 2).ToString();
    }


    private void Reset() {
        _canLevelProgress = false;
        _levelProgressFill.fillAmount = 0f;
    }

    public void OnStart() {
        SetLevelProgressSpeed(GameManager.instance.LevelManager.speed,
        GameManager.instance.LevelManager.GetLevels()[GameManager.instance.CurrentLevel].floorLength);
        _canLevelProgress = true;
    }

    private void SetLevelProgressSpeed(float speed, int floorLength) {
        _levelProgressFillSpeed = (speed / 11f) / floorLength;
    }

    public void Score(int score) {
        _score.text = score.ToString();
    }

}
