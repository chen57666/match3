using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StaffCanvasUI : MonoBehaviour
{
    void Start()
    {
        // 嘗試找出名為 X_Close 的按鈕
        Button closeBtn = GameObject.Find("X_Close")?.GetComponent<Button>();
        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("MainMenu");  // ? 替換為你的正確場景名稱
            });
        }
        else
        {
            Debug.LogWarning("找不到名為 X_Close 的按鈕！");
        }
    }
}
