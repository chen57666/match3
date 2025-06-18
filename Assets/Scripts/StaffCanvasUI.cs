using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StaffCanvasUI : MonoBehaviour
{
    void Start()
    {
        // ���է�X�W�� X_Close �����s
        Button closeBtn = GameObject.Find("X_Close")?.GetComponent<Button>();
        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("MainMenu");  // ? �������A�����T�����W��
            });
        }
        else
        {
            Debug.LogWarning("�䤣��W�� X_Close �����s�I");
        }
    }
}
