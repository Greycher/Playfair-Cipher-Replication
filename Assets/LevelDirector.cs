using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelDirector : MonoBehaviour
{
    public Button encryptBtn;
    public Button decryptBtn;
    public Button menuBtn;
    public Button reloadBtn;
    
    private void Awake()
    {
        encryptBtn?.onClick.AddListener(() => SceneManager.LoadScene(1));
        decryptBtn?.onClick.AddListener(() => SceneManager.LoadScene(2));
        menuBtn?.onClick.AddListener(() =>
        {
            Debug.Log("Menu button clicked.");
            SceneManager.LoadScene(0);
        });
        reloadBtn?.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
    }
}
