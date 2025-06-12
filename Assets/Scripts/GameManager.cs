using UnityEngine;
using TMPro;
using Match3Game;
using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    #region UI References
    [SerializeField] public TextMeshPro resultText;
    [SerializeField] public TextMeshPro targetText;
    [SerializeField] public TextMeshPro roundText;
    [SerializeField] public TextMeshPro statusText;
    [SerializeField] public TextMeshPro gemsText;
    [SerializeField] public TextMeshPro resolutionText;
    [SerializeField] public TextMeshPro levelText;
    [SerializeField] public TextMeshPro buttonText;
    [SerializeField] public GameObject resultPanel;
    #endregion

    #region Game State
    [SerializeField] private int currentLevel = 1;
    [SerializeField] public int target = 5;
    [SerializeField] public int round = 30;
    [SerializeField] private GameObject[] targetPrefab;
    [SerializeField] private GameObject targetTop;
    [SerializeField] public int targetID = 103;

    private Board board;
    private SpecialGemActivator specialGemActivator;

    // 新增 LevelData 結構 + levelDatas 陣列 ✅
    [System.Serializable]
    public struct LevelData
    {
        public int targetPrefabIndex; // targetPrefab 的 index
        public int targetCount;       // 通關要幾個
        public int roundCount;        // 回合數
    }

    [SerializeField] public LevelData[] levelDatas;

    #endregion

    private void Start()
    {
        InitializeComponents();
        InitializeLevelDatas(); // 新增呼叫 ✅
        UpdateUIElements();
        setGameLevelTarget();
    }

    private void InitializeComponents()
    {
        instance = this;
        board = Board.instance;
        specialGemActivator = new SpecialGemActivator(board);
        currentLevel = LevelManager.Instance.Level;

        // 初始化解析度顯示
        if (resolutionText != null)
        {
            int width = Screen.width;
            int height = Screen.height;
            resolutionText.text = $"{width}x{height}";
        }
        if (levelText != null)
        {
            levelText.text = "Lv." + currentLevel;
        }
    }

    #region UI Update Methods
    public void UpdateGameState(GameState state)
    {
        if (statusText == null) return;

        statusText.text = state switch
        {
            GameState.Ready => "可以開始交換寶石",
            GameState.Swapping => "交換中：寶石正在移動",
            GameState.Processing => "處理中：消除匹配的寶石",
            GameState.Filling => "填充中：補充新的寶石",
            GameState.Resetting => "重置中：遊戲板恢復初始狀態",
            GameState.Completed => "遊戲結束",
            _ => "遊戲進行中"
        };
    }

    public void UpdateGemCount(int count)
    {
        if (gemsText != null)
        {
            gemsText.text = $"寶石數量：{count}";
        }
    }

    private void UpdateUIElements()
    {
        if (roundText != null) roundText.text = $"Round: {round}";
        if (targetText != null) targetText.text = $"x {target}";
        if (resultText != null)
        {
            resultText.text = "";
        }

        if (resultPanel != null) resultPanel.SetActive(false);
    }

    // ✅ 改成資料驅動版 setGameLevelTarget()
    private void setGameLevelTarget()
    {
        if (targetPrefab.Length <= 0 || levelDatas == null || levelDatas.Length == 0) return;

        int currentLevel = LevelManager.Instance.Level; // 用 LevelManager 拿目前關卡

        if (currentLevel < 1 || currentLevel > levelDatas.Length)
        {
            Debug.LogWarning($"Level {currentLevel} is out of range! Using default setting.");
            currentLevel = 1;
        }

        targetTop = GameObject.Find("/TargetTop");

        LevelData levelData = levelDatas[currentLevel - 1]; // currentLevel 是 1-based，陣列 0-based

        targetTop.GetComponent<SpriteRenderer>().sprite = targetPrefab[levelData.targetPrefabIndex].GetComponent<SpriteRenderer>().sprite;
        targetTop.transform.localScale = Vector3.one;

        targetID = int.Parse(targetPrefab[levelData.targetPrefabIndex].name);
        target = levelData.targetCount;
        targetText.text = $"x {target}";

        round = levelData.roundCount;
        roundText.text = "Round: " + round.ToString();
    }

    public void ShowMessage(string message, float duration = 2f)
    {
        if (statusText != null)
        {
            StartCoroutine(ShowTemporaryMessage(message, duration));
        }
    }

    private System.Collections.IEnumerator ShowTemporaryMessage(string message, float duration)
    {
        if (statusText == null) yield break;

        string originalText = statusText.text;
        statusText.text = message;
        yield return new WaitForSeconds(duration);
        statusText.text = originalText;
    }
    #endregion

    #region Game Progress Methods
    public void UpdateLevel()
    {
        LevelManager.Instance.Level++;
        currentLevel = LevelManager.Instance.Level;
        if (resultText != null)
        {
            resultText.text = $"Level: {currentLevel}";
        }
    }

    public void UpdateTarget()
    {
        target--;
        if (target < 0) target = 0;

        if (targetText != null)
        {
            targetText.text = $"x {target}";
        }

        if (target == 0)
        {
            board.changeGameState(GameState.Completed);
            resultPanel.SetActive(true);

            if (resultText != null)
            {
                resultText.text = $"Level: {currentLevel} Completed!";
                buttonText.text = "Next";
                LevelManager.Instance.Level = currentLevel;
            }
        }
    }

    public void updateRound()
    {
        if (board.currentState == GameState.Completed) return;
        round--;
        if (roundText != null)
        {
            roundText.text = $"Round: {round}";
        }

        if (round == 0)
        {
            board.changeGameState(GameState.Completed);
            resultPanel.SetActive(true);
            resultText.text = $"Level: {currentLevel} Failed!";
            buttonText.text = "Back";
        }
    }
    #endregion

    // ✅ InitializeLevelDatas() → 你可以直接改數值 → 設 20 關、50 關
    private void InitializeLevelDatas()
    {
        levelDatas = new LevelData[20];

        levelDatas[0] = new LevelData { targetPrefabIndex = 0, targetCount = 1, roundCount = 30 };
        levelDatas[1] = new LevelData { targetPrefabIndex = 0, targetCount = 3, roundCount = 30 };
        levelDatas[2] = new LevelData { targetPrefabIndex = 1, targetCount = 3, roundCount = 50 };
        levelDatas[3] = new LevelData { targetPrefabIndex = 1, targetCount = 3, roundCount = 70 };
        levelDatas[4] = new LevelData { targetPrefabIndex = 2, targetCount = 1, roundCount = 100 };
        levelDatas[5] = new LevelData { targetPrefabIndex = 2, targetCount = 2, roundCount = 120 };
        levelDatas[6] = new LevelData { targetPrefabIndex = 3, targetCount = 2, roundCount = 150 };

        // 自行補齊 7~19 關（範例先放幾個）
        levelDatas[7] = new LevelData { targetPrefabIndex = 0, targetCount = 4, roundCount = 50 };
        levelDatas[8] = new LevelData { targetPrefabIndex = 1, targetCount = 5, roundCount = 60 };
        levelDatas[9] = new LevelData { targetPrefabIndex = 2, targetCount = 6, roundCount = 70 };
        levelDatas[10] = new LevelData { targetPrefabIndex = 3, targetCount = 7, roundCount = 80 };
        levelDatas[11] = new LevelData { targetPrefabIndex = 0, targetCount = 5, roundCount = 90 };
        levelDatas[12] = new LevelData { targetPrefabIndex = 1, targetCount = 4, roundCount = 100 };
        levelDatas[13] = new LevelData { targetPrefabIndex = 2, targetCount = 6, roundCount = 110 };
        levelDatas[14] = new LevelData { targetPrefabIndex = 3, targetCount = 7, roundCount = 120 };
        levelDatas[15] = new LevelData { targetPrefabIndex = 0, targetCount = 8, roundCount = 130 };
        levelDatas[16] = new LevelData { targetPrefabIndex = 1, targetCount = 9, roundCount = 140 };
        levelDatas[17] = new LevelData { targetPrefabIndex = 2, targetCount = 10, roundCount = 150 };
        levelDatas[18] = new LevelData { targetPrefabIndex = 3, targetCount = 5, roundCount = 160 };
        levelDatas[19] = new LevelData { targetPrefabIndex = 0, targetCount = 6, roundCount = 170 };
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Debug.Log("Reset Board");
            Gem gem = board.gems[0, 0];
            gem.id = 106;
            specialGemActivator.啟動特殊寶石(gem);
        }
    }
}

