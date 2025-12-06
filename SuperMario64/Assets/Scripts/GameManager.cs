using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager m_GameManager;
    public List<IRestartGameElement> m_RestartGameElements=new List<IRestartGameElement>();
    public GameUI m_GameUI;
    public PlayerController m_Player;
    void Awake()
    {
        if(m_GameManager!=null)
        {
            GameObject.Destroy(gameObject);
            return;
        }
        m_GameManager=this;
        DontDestroyOnLoad(gameObject);
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
        //if (Input.GetKeyDown(KeyCode.H))
           // m_Player.Hit();Debug.Log("Pulsado");
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
        foreach(IRestartGameElement l_RestartGameElement in m_RestartGameElements)
            l_RestartGameElement.RestartGame();
    }
}
