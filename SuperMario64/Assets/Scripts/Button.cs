using UnityEngine;
using UnityEngine.SceneManagement;

public class Button : MonoBehaviour
{
    public void ButtonRestart()
    {
        SceneManager.LoadScene("Game");
    }
}
