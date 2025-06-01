using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class QuitButton : MonoBehaviour
{
    private Button btn;

    private void Quit()
    {
        Application.Quit();
    }

    private void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(Quit);
    }

    private void OnDestroy()
    {
        btn.onClick.RemoveAllListeners();
    }
}
