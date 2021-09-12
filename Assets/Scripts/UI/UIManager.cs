using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class UIManager : MonoBehaviour {

    [SerializeField] private MainMenu _mainMenu;
    [SerializeField] private GameMenu _gameMenu;
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _endLevelMenu;
    [SerializeField] private GameObject _gameFinishedMenu;
    [SerializeField] private GameObject _inputSelectionMenu;

    [Space(15f)]
    [SerializeField] private TextMeshProUGUI _endLevelTitle;
    [SerializeField] private GameObject _continueButton;
    [SerializeField] private TextMeshProUGUI _tipText;
    [SerializeField] private GameObject _scoreValuePrefab;

    [Space(20f)]
    [SerializeField] private TextMeshProUGUI _gameFinishedTitle;
    [SerializeField] private TextMeshProUGUI _gameFinishedBestScore;
    [SerializeField] private GameObject _gameFinishedButton;

    [Space(20f)]
    [SerializeField] private FloatingJoystick _joystick;

    [SerializeField] private Animator _sceneTransition;

    private List<GameObject> _menus;
    private LevelManager _levelManager;




    private void Awake() {
        GameManager.OnStart += OnStart;
        GameManager.OnLevelFailed += OnLevelFailed;
        GameManager.OnLevelPassed += OnLevelPassed;
        GameManager.OnReset += OnReset;

        _menus = new List<GameObject>() { _mainMenu.gameObject, _gameMenu.gameObject, _endLevelMenu, _gameFinishedMenu, _pauseMenu };
        _levelManager = GameManager.instance.LevelManager;
    }



    private void Start() {
        OnReset();
    }


    private void OnStart() {
        _tipText.gameObject.SetActive(false);
        ActivateMenu(_gameMenu.gameObject);
        _gameMenu.OnStart();
    }


    private void OnLevelFailed() {
        ActivateMenu(_endLevelMenu);

        _endLevelTitle.text = "level failed !";
        _endLevelTitle.transform.localScale = new Vector3(5f, 5f, 1f);
        _endLevelTitle.transform.DOScale(Vector3.one, 0.75f).OnComplete(() => {
            StartCoroutine(WaitForLevelManagerStop());
        }).SetEase(Ease.OutQuart);

        TipTextAnimation("tap to continue");

        ResetScore();
    }

    private void OnLevelPassed() {
        if (GameManager.instance.isGameFinished) {
            GameFinished();
            return;
        }
        ActivateMenu(_endLevelMenu);

        _endLevelTitle.text = "level passed !";
        _endLevelTitle.transform.localScale = new Vector3(5f, 5f, 1f);
        _endLevelTitle.transform.DOScale(Vector3.one, 0.75f).OnComplete(() => {
            StartCoroutine(WaitForLevelManagerStop());
        }).SetEase(Ease.OutQuart);

        TipTextAnimation("tap to continue");
    }
    private IEnumerator WaitForLevelManagerStop() {
        yield return new WaitForSeconds(1f);
        _continueButton.SetActive(true);
    }


    public void PauseGame() {
        ActivateMenu(_pauseMenu);
        TipTextAnimation("tap to continue");

        if (GameManager.instance.inputType == InputType.JOYSTICK) _joystick.gameObject.SetActive(false);
        GameManager.instance.PauseGame();
    }

    public void UnPauseGame() {
        ActivateMenu(_gameMenu.gameObject);
        TipTextReset();
        _tipText.gameObject.SetActive(false);

        if (GameManager.instance.inputType == InputType.JOYSTICK) {
            _joystick.Input = Vector2.zero;
            _joystick.gameObject.SetActive(true);
            _joystick.transform.GetChild(0).gameObject.SetActive(false);
        }


        GameManager.instance.UnPauseGame();
    }


    private void GameFinished() {
        ActivateMenu(_gameFinishedMenu);

        _gameFinishedTitle.text = "game finished!";
        _gameFinishedTitle.transform.localScale = new Vector3(5f, 5f, 1f);
        _gameFinishedTitle.transform.DOScale(Vector3.one, 0.75f).OnComplete(() => {
            _gameFinishedButton.SetActive(true);
        }).SetEase(Ease.OutQuart);
        _gameFinishedBestScore.text = "best: " + PlayerPrefs.GetInt("BestScore", 0);

        TipTextAnimation("tap to continue");
    }

    private void OnReset() {
        _gameMenu.SetUI();
        TipTextReset();
        ActivateMenu(_mainMenu.gameObject);
        TipTextAnimation("swipe to move");
        _mainMenu.SetUI();
    }





    public void Score(int score) {
        _gameMenu.Score(score);
    }

    public void ScoreValueAnim(int value) {
        GameObject scoreValue = Instantiate(_scoreValuePrefab, _gameMenu.transform);
        scoreValue.GetComponent<TextMeshProUGUI>().text = "+" + value;

        scoreValue.GetComponent<RectTransform>().DOAnchorPosY(-250f, 0.5f).OnComplete(() => {
            Destroy(scoreValue.gameObject);
        });
    }

    private void ResetScore() {
        _gameMenu.Score(0);
    }


    public void InputSelection() {
        GameManager.instance.isOnStart = false;
        _inputSelectionMenu.SetActive(true);
        TipTextReset();
        _tipText.gameObject.SetActive(false);
    }

    public void SelectInputType(string type) {
        if (type.ToLower() == "swipe") {
            GameManager.instance.ChangeInputType(InputType.SWIPE);
        }
        else if (type.ToLower() == "joystick") {
            GameManager.instance.ChangeInputType(InputType.JOYSTICK);
        }

        _inputSelectionMenu.SetActive(false);

        TipTextAnimation("swipe to move");
        _tipText.gameObject.SetActive(true);
    }




    private void ActivateMenu(GameObject menu) {
        foreach (GameObject m in _menus) {
            if (m == menu) {
                m.SetActive(true);
            }
            else {
                m.SetActive(false);
            }
        }
    }


    private void TipTextAnimation(string text) {
        _tipText.text = text;
        Color color = _tipText.color;
        _tipText.gameObject.SetActive(true);

        TipTextReset();
        _tipText.DOColor(new Color(color.r, color.g, color.b, 0.5f), 0.75f).SetLoops(-1, LoopType.Yoyo).SetId(1).SetUpdate(true);
        _tipText.transform.DOScale(0.85f, 0.75f).SetLoops(-1, LoopType.Yoyo).SetId(1).SetUpdate(true);
    }

    private void TipTextReset() {
        DOTween.Kill(1);
        Color color = _tipText.color;
        _tipText.color = new Color(color.r, color.g, color.b, 1f);
        _tipText.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    public void SceneTransition(string animation) {
        _sceneTransition.Play(animation, 0, 0f);
    }



    public void ResetEverything() {
        _gameFinishedButton.SetActive(false);
        PlayerPrefs.DeleteKey("CurrentScore");
        PlayerPrefs.DeleteKey("CurrentLevel");
        GameManager.instance.isGameFinished = false;
        GameManager.instance.ResetLevel();
    }



}
