using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerMovement : MonoBehaviour {

    public FloatingJoystick joystick;
    public float movementSpeed = 5f;

    [SerializeField] private float _movementTime = 0.5f;
    [SerializeField] private Ease _easeType;
    [SerializeField] private ParticleSystem _explosion;
    [SerializeField] private ParticleSystem _scoreEffect;
    [SerializeField] private bool _godMode = false;

    private int _gridWidth = 3;
    private int _gridHeight = 2;
    private float _gridSize = 1.8f;
    private Vector3[,] _grid;
    private int _horizontalIndex = 1;
    private int _verticalIndex = 0;
    private Vector3 _rotation;
    private bool _canMove = false;
    private Animator _anim;
    private Animator _parentAnim;
    private Queue<Tween> _movementQueue = new Queue<Tween>();
    private Queue<Tween> _rotateQueue = new Queue<Tween>();
    private float _rotationTime = 0f;




    private void Awake() {
        SwipeDetector.OnSwipe += OnSwipe;

        GameManager.OnStart += OnStart;
        GameManager.OnLevelFailed += OnLevelFailed;
        GameManager.OnLevelPassed += OnLevelPassed;
        GameManager.OnReset += OnReset;

        _anim = transform.GetChild(0).GetComponent<Animator>();
        _parentAnim = GetComponent<Animator>();
        _scoreEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        InitializeGrid();

        _rotationTime = _movementTime / 2f;
    }

    private void OnStart() {
        if (GameManager.instance.inputType == InputType.JOYSTICK) {
            joystick.gameObject.SetActive(true);
            joystick.Input = Vector2.zero;
            joystick.transform.GetChild(0).gameObject.SetActive(false);
        }

        _canMove = true;
    }

    private void OnLevelFailed() {
        if (GameManager.instance.inputType == InputType.JOYSTICK) joystick.gameObject.SetActive(false);

        _canMove = false;
    }

    private void OnLevelPassed() {
        if (GameManager.instance.inputType == InputType.JOYSTICK) joystick.gameObject.SetActive(false);

        _canMove = false;
        Tween t = DOTween.To(() => _anim.speed, x => _anim.speed = x, 0f, 1.5f);
    }

    public void OnReset() {
        _verticalIndex = 0;
        _horizontalIndex = 1;
        transform.position = new Vector3(0f, 1f, 11f);
        transform.GetChild(0).gameObject.SetActive(true);

        joystick.gameObject.SetActive(false);
        _parentAnim.SetFloat("Horizontal", 0f);
        _parentAnim.SetFloat("Vertical", 0f);

        _explosion.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        _anim.speed = 1f;
    }


    private void Update() {
        if (!_canMove) return;
        if (GameManager.instance.inputType == InputType.SWIPE) return;

        _parentAnim.SetFloat("Horizontal", joystick.Horizontal, 0.1f, Time.deltaTime);
        _parentAnim.SetFloat("Vertical", joystick.Vertical, 0.1f, Time.deltaTime);

        transform.position += new Vector3(joystick.Horizontal, joystick.Vertical) * movementSpeed * Time.deltaTime;
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -1.8f, 1.8f),
        Mathf.Clamp(transform.position.y, 1f, 2.8f), 11f);
    }



    private void OnSwipe(SwipeData swipeData) {
        if (!_canMove) return;
        if (GameManager.instance.inputType == InputType.JOYSTICK) return;

        Move(swipeData);
    }


    private void Move(SwipeData swipeData) {
        if (_movementQueue.Count > 1) return;

        if (swipeData.direction == SwipeDirection.Up && _verticalIndex != _gridHeight - 1) {
            _verticalIndex++;
            _rotation = new Vector3(-30f, 0f, 0f);

            Move();
            Rotate();
        }
        else if (swipeData.direction == SwipeDirection.Down && _verticalIndex != 0) {
            _verticalIndex--;
            _rotation = new Vector3(60f, 0f, 0f);

            Move();
            Rotate();
        }
        else if (swipeData.direction == SwipeDirection.Left && _horizontalIndex != 0) {
            _horizontalIndex--;
            _rotation = new Vector3(25f, 0f, 35f);

            Move();
            Rotate();
        }
        else if (swipeData.direction == SwipeDirection.Right && _horizontalIndex != _gridWidth - 1) {
            _horizontalIndex++;
            _rotation = new Vector3(25f, 0f, -35f);

            Move();
            Rotate();
        }
    }

    private void Move() {
        Tween tween = transform.DOMove(_grid[_horizontalIndex, _verticalIndex], _movementTime).SetEase(_easeType);
        tween.Pause();
        _movementQueue.Enqueue(tween);
        if (_movementQueue.Count == 1) _movementQueue.Peek().Play();
        tween.OnComplete(MovementOnComplete);
    }

    private void MovementOnComplete() {
        _movementQueue.Dequeue();

        if (_movementQueue.Count > 0) _movementQueue.Peek().Play();
    }

    private void Rotate() {
        Tween tween = transform.DORotate(_rotation, _rotationTime).SetEase(_easeType).OnComplete(() => {
            transform.DORotate(new Vector3(25f, 0f, 0f), _rotationTime).SetEase(_easeType).OnComplete(RotateOnComplete);
        });

        tween.Pause();
        _rotateQueue.Enqueue(tween);
        if (_rotateQueue.Count == 1) _rotateQueue.Peek().Play();
    }

    private void RotateOnComplete() {
        _rotateQueue.Dequeue();

        if (_rotateQueue.Count > 0) _rotateQueue.Peek().Play();
    }



    private void InitializeGrid() {
        _grid = new Vector3[_gridWidth, _gridHeight];

        for (int i = 0; i < _gridWidth; i++) {
            for (int j = 0; j < _gridHeight; j++) {
                Vector3 pos = new Vector3((i * _gridSize) + -1.8f, (j * _gridSize) + 1f, transform.position.z);
                _grid[i, j] = pos;
            }
        }
    }



    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("Obstacle") && !_godMode) {
            _canMove = false;
            _explosion.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 11f);
            _explosion.Play();
            GameManager.instance.LevelFailed();
            transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.CompareTag("Finish")) {
            GameManager.instance.LevelPassed();
        }
        else if (other.gameObject.CompareTag("ScoreTrigger")) {
            _scoreEffect.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 11f);
            _scoreEffect.Play();
            GameManager.instance.Score();
        }
    }


    public void InputTypeSwipe() {
        _parentAnim.enabled = false;
        joystick.gameObject.SetActive(false);
    }
    public void InputTypeJoystick() {
        _parentAnim.enabled = true;
    }


}
