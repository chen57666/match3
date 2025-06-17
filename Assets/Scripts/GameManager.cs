using UnityEngine;
using TMPro;
using Match3Game;
using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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

    [System.Serializable]
    public struct LevelData
    {
        public int targetPrefabIndex;
        public int targetCount;
        public int roundCount;
        public int[] dropGemIndexes; // 新增！這關實際會掉落哪些寶石 index
    }

    [SerializeField] public LevelData[] levelDatas;

    #endregion

    private void Start()
    {
        InitializeComponents();
        InitializeLevelDatas();
        UpdateUIElements();

        if (SceneManager.GetActiveScene().name == "Level")
        {
            setGameLevelTarget();
        }
    }

    private void InitializeComponents()
    {
        instance = this;
        board = Board.instance;
        specialGemActivator = new SpecialGemActivator(board);
        currentLevel = LevelManager.Instance.Level;

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

    private void setGameLevelTarget()
    {
        if (targetPrefab.Length <= 0 || levelDatas == null || levelDatas.Length == 0) return;

        int currentLevel = LevelManager.Instance.Level;

        if (currentLevel < 1 || currentLevel > levelDatas.Length)
        {
            Debug.LogWarning($"Level {currentLevel} is out of range! Using default setting.");
            currentLevel = 1;
        }

        if (targetTop == null)
        {
            Debug.LogError("❌ TargetTop 尚未設定，請在 Inspector 指定！");
            return;
        }

        LevelData levelData = levelDatas[currentLevel - 1];

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

    private void InitializeLevelDatas()
    {
        if (targetPrefab == null || targetPrefab.Length == 0)
        {
            Debug.LogError("❌ 無法初始化 LevelDatas：targetPrefab 尚未指定或為空！");
            return;
        }

        int totalLevels = 20;
        int maxGemTypes = targetPrefab.Length;
        levelDatas = new LevelData[totalLevels];

        for (int i = 0; i < totalLevels; i++)
        {
            int count = Mathf.Min(3 + i / 2, maxGemTypes);
            int[] drops = Enumerable.Range(0, count).ToArray();
            levelDatas[i] = new LevelData
            {
                targetPrefabIndex = i % maxGemTypes,
                targetCount = 3 + i % 5,
                roundCount = 30 + i * 5,
                dropGemIndexes = drops
            };
        }
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
