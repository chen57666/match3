using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UI_Controller : MonoBehaviour
{
    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        Button startButton = root.Q<Button>("But_Start");
        Button mapButton = root.Q<Button>("But_Map");
        Button staffButton = root.Q<Button>("But_Staff"); // �� �s�W

        if (LevelManager.Instance.Level == 0)
        {
            LevelManager.Instance.Level = 1;
        }

        startButton.clicked += () => { LevelManager.Instance.LoadLevel(); };
        mapButton.clicked += () => { LevelManager.Instance.LoadMap(); };
        staffButton.clicked += () => { SceneManager.LoadScene("StaffScene"); }; // �� �s�W
    }
}

