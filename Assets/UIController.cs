using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    // Method to load a scene by name
    public void StartGame(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Method to exit the application
    public void ExitGame()
    {
        #if UNITY_EDITOR
        // Exit Play Mode when running in the Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        // Quit the application
        Application.Quit();
        #endif
    }
    void Start()
    {
        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("SimpleNaturePack_Demo");
        }
    }
}
