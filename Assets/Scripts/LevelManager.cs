using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using System.IO;
using DG.Tweening;
using System.Linq;

public class LevelManager : MonoBehaviour {

    public float speed = 8.5f;
    [HideInInspector] public int currentLevel = 5;
    [HideInInspector] public int randomFloorLength = 1;
    public bool canMove = false;

    [SerializeField] private GameObject _floorPrefab;
    [SerializeField] private GameObject _floorFinishPrefab;
    [SerializeField] private GameObject _diamondPrefab;
    [SerializeField] private GameObject _confettiPrefab;
    [SerializeField] private GameObject _scoreCollider;
    [SerializeField] private Transform _floorsParent;
    [SerializeField] private Transform _obstaclesParent;
    [SerializeField] private Transform _diamondsParent;
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private float _obstacleGap = 18f;

    private float _tempSpeed = 0f;



    private void Start() {
        GameManager.OnStart += OnStart;
        GameManager.OnLevelPassed += OnLevelPassed;
        GameManager.OnLevelFailed += OnLevelFailed;
        GameManager.OnReset += OnReset;

        speed += (int)(PlayerPrefs.GetInt("CurrentLevel", 0) / 5) * 0.5f;
        _tempSpeed = speed;
    }


    private void OnStart() {
        _tempSpeed = speed;
        canMove = true;
    }
    private void OnLevelFailed() {
        canMove = false;
    }

    private void OnLevelPassed() {
        Tween t = DOTween.To(() => speed, x => speed = x, 0f, (8.5f / speed) * 1.5f).SetEase(Ease.OutCirc).OnComplete(() => {
            canMove = false;
        });

        if (GameManager.instance.CurrentLevel % 5 == 0) IncreaseSpeed();
    }

    public void OnReset() {
        speed = _tempSpeed;
        this.transform.position = Vector3.zero;

        CreateLevel(GameManager.instance.CurrentLevel);
    }



    private void Update() {
        if (!canMove) return;

        transform.position += -transform.forward * speed * Time.deltaTime;
    }


    public void IncreaseSpeed() {
        speed += 0.5f;
    }


    public void CreateLevel(int levelIndex) {
        if (Application.isPlaying) GameManager.instance.isLevelReady = false;

        List<Level> levels = GetLevels();
        if (levelIndex < 0 || levelIndex > levels.Count) {
            Debug.Log("Invalid index: " + levelIndex);
            return;
        }

        ClearLevel();

        Level level = levels[levelIndex];
        currentLevel = levelIndex;
        AddFloor(level.floorLength);

        for (int i = 0; i < level.obstacleNames.Length; i++) {
            GameObject emptyParent = new GameObject();
            emptyParent.name = "Obstacle" + (i + 1).ToString();
            emptyParent.transform.SetParent(this.transform.GetChild(1));
            emptyParent.transform.position = new Vector3(0f, 0f, (i * _obstacleGap) + 29f);

            GameObject obstacle = Instantiate(GetObstacle(level.obstacleNames[i])) as GameObject;
            obstacle.name = obstacle.name.Replace("(Clone)", "");
            obstacle.transform.SetParent(emptyParent.transform);
            obstacle.transform.localPosition = obstacle.transform.position;

            GameObject scoreCollider = Instantiate(_scoreCollider, emptyParent.transform);
            scoreCollider.name = scoreCollider.name.Replace("(Clone)", "");
            scoreCollider.transform.localPosition = Vector3.zero;
        }

        CreateDiamonds(level);

        if (Application.isPlaying) GameManager.instance.isLevelReady = true;
    }

    private GameObject GetObstacle(string name) {
        GameObject obstacle;
        int length = obstaclePrefabs.Length;

        for (int i = 0; i < length; i++) {
            obstacle = obstaclePrefabs[i];

            if (obstacle.name.Equals(name)) {
                return obstacle;
            }
        }
        return null;
    }


    public void AddFloor(int floorLength) {
        for (int i = 0; i < floorLength; i++) {
            if (i == floorLength - 1) {
                GameObject floorFinish = Instantiate(_floorFinishPrefab);
                floorFinish.name = floorFinish.name.Replace("(Clone)", "");
                floorFinish.transform.position = new Vector3(0f, 0f, (i * 12.5f) + 10f);
                floorFinish.transform.SetParent(transform.GetChild(0));

                GameObject confetti = Instantiate(_confettiPrefab, floorFinish.transform);
                confetti.name = confetti.name.Replace("(Clone)", "");
                if (Application.isPlaying) GameManager.instance.SetConfetti(confetti);
                continue;
            }

            GameObject floor = Instantiate(_floorPrefab);
            floor.name = floor.name.Replace("(Clone)", "");
            floor.transform.position = new Vector3(0f, 0f, (i * 12.5f) + 10f);
            floor.transform.SetParent(transform.GetChild(0));
        }
    }

    private void CreateDiamonds(Level level) {
        if (level.diamondPositions == null) return;

        for (int i = 0; i < level.diamondPositions.GetLength(0); i++) {
            GameObject diamond = Instantiate(_diamondPrefab, _diamondsParent);
            diamond.name = diamond.name.Replace("(Clone)", "");
            diamond.transform.position = new Vector3(level.diamondPositions[i, 0], level.diamondPositions[i, 1], level.diamondPositions[i, 2]);
        }
    }


    private void ClearLevel() {
        for (int i = _obstaclesParent.childCount - 1; i >= 0; i--) {
            DestroyImmediate(_obstaclesParent.GetChild(i).gameObject);
        }

        for (int i = _floorsParent.childCount - 1; i >= 0; i--) {
            DestroyImmediate(_floorsParent.GetChild(i).gameObject);
        }

        for (int i = _diamondsParent.childCount - 1; i >= 0; i--) {
            DestroyImmediate(_diamondsParent.GetChild(i).gameObject);
        }
    }

    public List<Level> GetLevels() {
#if UNITY_EDITOR
        string path = "Assets/Resources/levels.json";
        if (!File.Exists(path)) return null;
        string jsonLoad = File.ReadAllText(path);
#else
        TextAsset textFile = Resources.Load<TextAsset>("levels");
        if(textFile == null) return null;
        string jsonLoad = textFile.text;
#endif

        return JsonConvert.DeserializeObject<List<Level>>(jsonLoad);
    }

    public string[] GetLevelsName() {
        string[] names = new string[GetLevels().Count];

        for (int i = 0; i < names.Length; i++) {
            names[i] = (i + 1).ToString();
        }

        return names;
    }


    #region EditorScripts
#if UNITY_EDITOR
    public void ShowLevel(int levelIndex) {
        if (levelIndex < 0 || levelIndex >= GetLevels().Count) {
            Debug.Log("Invalid index: " + levelIndex);
            return;
        }

        CreateLevel(levelIndex);
    }


    public void CreateNewLevel() {
        Level level = new Level();
        level.levelName = GetLevels().Count + 1;
        level.floorLength = _floorsParent.childCount;
        level.obstacleNames = new string[_obstaclesParent.childCount];
        for (int i = 0; i < _obstaclesParent.childCount; i++) {
            level.obstacleNames[i] = _obstaclesParent.GetChild(i).GetChild(0).name;
        }

        Transform[] diamonds = _diamondsParent.GetComponentsInChildren<Transform>().Where(t => t.tag == "Diamond").ToArray();
        level.ConvertPosition(diamonds);

        AddLevel(level);
        currentLevel = level.levelName - 1;

        Debug.Log("Level: " + level.levelName + " added.");
    }


    public void CreateRandomLevel() {
        ClearLevel();
        AddFloor(randomFloorLength);
        AddRandomObstacles(randomFloorLength);
    }

    private void AddRandomObstacles(int floorLength) {
        int obstacleCount = Mathf.RoundToInt((floorLength * 11f) / _obstacleGap) - 2;

        for (int i = 0; i < obstacleCount + 1; i++) {
            GameObject emptyParent = new GameObject();
            emptyParent.name = "Obstacle" + (i + 1).ToString();
            emptyParent.transform.SetParent(this.transform.GetChild(1));
            emptyParent.transform.position = new Vector3(0f, 0f, (i * _obstacleGap) + 29f);

            GameObject obstacle = Instantiate(obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)]);
            obstacle.name = obstacle.name.Replace("(Clone)", "");
            obstacle.transform.SetParent(emptyParent.transform);
            obstacle.transform.localPosition = obstacle.transform.position;

            GameObject scoreCollider = Instantiate(_scoreCollider, emptyParent.transform);
            scoreCollider.transform.localPosition = Vector3.zero;
        }
    }

    private void AddLevel(Level level) {
        List<Level> levels = GetLevels();
        levels.Add(level);
        WriteLevels(levels);
    }


    public void UpdateLevel(int levelIndex) {
        List<Level> levels = GetLevels();
        if (levels[levelIndex] == null) return;

        Level level = new Level();
        level.levelName = levelIndex + 1;
        level.floorLength = _floorsParent.childCount;
        level.obstacleNames = new string[_obstaclesParent.childCount];
        for (int i = 0; i < _obstaclesParent.childCount; i++) {
            level.obstacleNames[i] = _obstaclesParent.GetChild(i).GetChild(0).name;
        }

        Transform[] diamonds = _diamondsParent.GetComponentsInChildren<Transform>().Where(t => t.tag == "Diamond").ToArray();
        level.ConvertPosition(diamonds);

        levels[levelIndex] = level;

        WriteLevels(levels);
        Debug.Log("Level: " + level.levelName + " updated.");
    }

    public void DeleteLevel(int levelIndex) {
        List<Level> levels = GetLevels();
        if (levelIndex < 0 || levelIndex > levels.Count) {
            Debug.Log("Invalid index: " + levelIndex);
            return;
        }
        levels.RemoveAt(levelIndex);
        WriteLevels(levels);
        CreateLevel(levelIndex - 1);

        Debug.Log("Level: " + (levelIndex - 1) + " deleted.");
    }


    private void WriteLevels(List<Level> levels) {
        string path = "Assets/Resources/levels.json";
        string json = JsonConvert.SerializeObject(levels, Formatting.Indented);

        using (FileStream fs = new FileStream(path, FileMode.Create)) {
            using (StreamWriter writer = new StreamWriter(fs)) {
                writer.Write(json);
            }
        }
    }
#endif
    #endregion


}
