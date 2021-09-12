using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMenu : MonoBehaviour {

    [SerializeField] private TextMeshProUGUI _currentLevel;
    [SerializeField] private TextMeshProUGUI _bestScore;



    public void SetUI() {
        _currentLevel.text = "Level : " + (GameManager.instance.CurrentLevel + 1);
        _bestScore.text = "Best : " + GameManager.instance.BestScore;
    }

}
