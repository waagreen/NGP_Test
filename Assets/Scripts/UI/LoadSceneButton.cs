using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class LoadSceneButton : MonoBehaviour
{
    [SerializeField] private string sceneName;

    private Button btn;

    private void LoadScene()
    {
        SaveDataManager.Instance.SaveGame();
        SceneManager.LoadScene(sceneName);
    }

    private void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(LoadScene);
    }

    private void OnDestroy()
    {
        btn.onClick.RemoveAllListeners();
    }
}
