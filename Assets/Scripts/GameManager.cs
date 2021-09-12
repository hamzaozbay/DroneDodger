using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    #region Singleton
    public static GameManager instance;

    private void Awake() {
        if (instance != this) {
            if (instance != null) {
                Destroy(instance.gameObject);
            }

            instance = this;
        }
    }
    #endregion

    public static Action OnStart;
    public static Action OnLevelFailed;
    public static Action OnLevelPassed;
    public static Action OnReset;

    public InputType inputType;
    public bool isGameFinished = false;
    [HideInInspector] public bool isLevelReady = false;
    [HideInInspector] public bool isOnStart = true;

    [SerializeField] private UIManager _uiManager;
    [SerializeField] private LevelManager _levelManager;
    [SerializeField] private PlayerMovement _playerMovement;
    [Space(20f)]
    [SerializeField] private bool _debug = false;


    private int _bestScore = 0;
    private int _currentLevel = 0;
    private int _levelCount = 0;
    private int _score = 0;
    private ParticleSystem[] _confettiParticles = new ParticleSystem[2];
    private bool _sceneTransitionFirstTime = true;




    private void Start() {
        SwipeDetector.OnSwipe += StartLevel;
        _score = PlayerPrefs.GetInt("CurrentScore", 0);

        ResetLevel();
        _levelCount = _levelManager.GetLevels().Count;
    }


    public void StartLevel(SwipeData swipeData) {
        if (!isOnStart) return;

        isOnStart = false;

        if (OnStart != null) {
            OnStart();
        }
    }


    public void LevelPassed() {
        if (_debug) {
            int nextLevel = _levelManager.currentLevel + 1;
            if (_levelManager.GetLevels().Count != nextLevel) {
                _levelManager.currentLevel++;
                _currentLevel = nextLevel;
            }
            else isGameFinished = true;
        }
        else {
            int nextLevel = PlayerPrefs.GetInt("CurrentLevel", 0) + 1;
            if (_levelManager.GetLevels().Count != nextLevel) {
                PlayerPrefs.SetInt("CurrentLevel", nextLevel);
                _currentLevel = nextLevel;
            }
            else isGameFinished = true;
        }

        PlayerPrefs.SetInt("CurrentScore", _score);

        foreach (ParticleSystem confetti in _confettiParticles) {
            confetti.Play();
        }

        SetNewBestScore();

        if (OnLevelPassed != null) {
            OnLevelPassed();
        }
    }


    public void LevelFailed() {
        SetNewBestScore();

        _score = 0;
        PlayerPrefs.SetInt("CurrentScore", _score);

        if (OnLevelFailed != null) {
            OnLevelFailed();
        }
    }


    public void ResetLevel() {
        if (_debug) {
            _currentLevel = _levelManager.currentLevel;
        }
        else {
            _currentLevel = PlayerPrefs.GetInt("CurrentLevel", 0);
        }

        _score = PlayerPrefs.GetInt("CurrentScore", 0);
        _bestScore = PlayerPrefs.GetInt("BestScore", 0);

        Time.timeScale = 1f;
        StartCoroutine(WaitForTransition());
    }

    private IEnumerator WaitForTransition() {
        if (!_sceneTransitionFirstTime) _uiManager.SceneTransition("Close");
        _sceneTransitionFirstTime = false;

        yield return new WaitForSeconds(0.5f);
        if (OnReset != null) {
            OnReset();
        }

        while (!isLevelReady) {
            yield return null;
        }
        _uiManager.SceneTransition("Open");
        isOnStart = true;
    }


    public void PauseGame() {
        Time.timeScale = 0f;
    }

    public void UnPauseGame() {
        Time.timeScale = 1f;
    }



    public void Score() {
        _score += _levelManager.currentLevel + 1;
        _uiManager.Score(_score);
        _uiManager.ScoreValueAnim(_levelManager.currentLevel + 1);
    }
    public void Score(int value) {
        _score += value;
        _uiManager.Score(_score);
        _uiManager.ScoreValueAnim(value);
    }

    public void SetNewBestScore() {
        if (_score > PlayerPrefs.GetInt("BestScore", 0)) {
            PlayerPrefs.SetInt("BestScore", _score);
        }
    }


    public void SetConfetti(GameObject confetti) {
        _confettiParticles = confetti.GetComponentsInChildren<ParticleSystem>();
    }


    public void ChangeInputType(InputType input) {
        if (input == InputType.JOYSTICK) {
            inputType = InputType.JOYSTICK;
            _playerMovement.InputTypeJoystick();
        }
        else {
            inputType = InputType.SWIPE;
            _playerMovement.InputTypeSwipe();
        }

        isOnStart = true;
    }



    public LevelManager LevelManager { get { return _levelManager; } }
    public int CurrentLevel { get { return _currentLevel; } }
    public int BestScore { get { return _bestScore; } }
    public int LevelCount { get { return _levelCount; } }
    public int CurrentScore { get { return _score; } }

}

public enum InputType {
    SWIPE,
    JOYSTICK
}
