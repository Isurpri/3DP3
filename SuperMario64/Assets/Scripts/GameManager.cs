using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    static GameManager m_GameManager;
    public List<IRestartGameElement> m_RestartGameElements=new List<IRestartGameElement>();
    public GameUI m_GameUI;
    public GameObject m_GameOver;
    public PlayerController m_Player;
    public Fade m_fade;

    void Awake()
    {
        m_GameOver.SetActive(false);
        if(m_GameManager!=null)
        {
            GameObject.Destroy(gameObject);
            return;
        }
        m_GameManager=this;
        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    public static GameManager GetGameManager()
    {
        return m_GameManager;   
    }
    public void AddRestartGameElement(IRestartGameElement RestartGameElement)
    {
        m_RestartGameElements.Add(RestartGameElement);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
            RestartGame();
        if (Input.GetKeyDown(KeyCode.H))
            GameOver();
            //m_Player.Hit();
       // if (Input.GetKeyDown(KeyCode.C))
          //  m_Player.AddCoin();
    }
    public PlayerController GetPlayer()
    {
        return m_Player;
    }
    public void SetPlayer(PlayerController Player)
    {
        m_Player = Player;
    }
    public void RestartGame()
    {
        m_GameOver.SetActive(false);

        foreach (IRestartGameElement l_RestartGameElement in m_RestartGameElements)
            l_RestartGameElement.RestartGame();

        m_fade.FadeOut(() =>
        {
            m_fade.gameObject.SetActive(false);
        });
    }
    public void GameOver()
    {
        m_GameOver.SetActive(true);
        Cursor.lockState = CursorLockMode.None;

        //SceneManager.LoadScene("GameOver");
        m_fade.FadeOut(() =>
        {
            m_fade.gameObject.SetActive(false);
        });
    }
    public void ButtonRestart()
    {
        Cursor.lockState = CursorLockMode.Locked;
        RestartGame();
    }
}
