using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeDetector : MonoBehaviour {

    public static event System.Action<SwipeData> OnSwipe = delegate { };
    public static event System.Action OnTap = delegate { };

    [SerializeField] private bool _detectSwipeOnlyAfterRelease = true;
    [SerializeField] private float _minDistanceForSwipe = 60f;

    private Vector2 _fingerDownPosition;
    private Vector2 _fingerUpPosition;

    private float[] _touchTimeBegan = new float[10];
    private bool[] _didTouchMove = new bool[10];
    private float _tapTimeThreshold = 0.25f;



    private void Update() {
        foreach (Touch touch in Input.touches) {
            TapDetection(touch);

            if (touch.phase == TouchPhase.Began) {
                _fingerUpPosition = touch.position;
                _fingerDownPosition = touch.position;
            }

            if (!_detectSwipeOnlyAfterRelease && touch.phase == TouchPhase.Moved) {
                _fingerDownPosition = touch.position;
                DetectSwipe();
            }

            if (touch.phase == TouchPhase.Ended) {
                _fingerDownPosition = touch.position;
                DetectSwipe();
            }
        }
    }


    private void DetectSwipe() {
        if (SwipeDistanceCheck()) {
            if (IsVerticalSwipe()) {
                SwipeDirection direction = _fingerDownPosition.y - _fingerUpPosition.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
                SendSwipe(direction);
            }
            else {
                SwipeDirection direction = _fingerDownPosition.x - _fingerUpPosition.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
                SendSwipe(direction);
            }
        }
    }


    private void SendSwipe(SwipeDirection direction) {
        SwipeData swipeData = new SwipeData();
        swipeData.startPosition = _fingerDownPosition;
        swipeData.endPosition = _fingerUpPosition;
        swipeData.direction = direction;

        OnSwipe(swipeData);
    }


    private void TapDetection(Touch touch) {
        int fingerIndex = touch.fingerId;

        if (touch.phase == TouchPhase.Began) {
            _touchTimeBegan[fingerIndex] = Time.time;
            _didTouchMove[fingerIndex] = false;
        }
        if (touch.phase == TouchPhase.Moved) {
            _didTouchMove[fingerIndex] = true;
        }
        if (touch.phase == TouchPhase.Ended) {
            float tapTime = Time.time - _touchTimeBegan[fingerIndex];

            if (tapTime <= _tapTimeThreshold && !_didTouchMove[fingerIndex]) {
                OnTap();
            }
        }
    }




    private bool IsVerticalSwipe() {
        return VerticalMovementDistance() > HorizontalMovementDistance();
    }


    private bool SwipeDistanceCheck() {
        return VerticalMovementDistance() > _minDistanceForSwipe || HorizontalMovementDistance() > _minDistanceForSwipe;
    }

    private float VerticalMovementDistance() {
        return Mathf.Abs(_fingerDownPosition.y - _fingerUpPosition.y);
    }

    private float HorizontalMovementDistance() {
        return Mathf.Abs(_fingerDownPosition.x - _fingerUpPosition.x);
    }

}

public struct SwipeData {
    public Vector2 startPosition;
    public Vector2 endPosition;
    public SwipeDirection direction;
}

public enum SwipeDirection {
    Up,
    Down,
    Left,
    Right
}