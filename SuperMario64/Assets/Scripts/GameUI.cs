using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Text m_CoinsText;
    public Image m_LifeBar;

    [Header("Animations")]
    public Animation m_Animation;
    public Animation m_InAnimationClip;
    public Animation m_OutAnimationClip;
    public Animation m_StayInAnimationClip;
    public Animation m_StayOutAnimationClip;
    public float m_ShowUIWaitTime = 4.0f;

    public void Start()
    {
        SetCoins(0);
        SetLifeBar(1.0f);
        m_Animation.Play(m_StayOutAnimationClip.name);
        m_Animation.Sample();
    }
    public void SetCoins(int Coins)
    {
        m_CoinsText.text = Coins.ToString();
    }
    public void SetLifeBar(float LifeNormalized)
    {
        m_LifeBar.fillAmount = LifeNormalized;
    }
    public void ShowUI()
    {
        m_Animation.Play(m_InAnimationClip.name);
        m_Animation.PlayQueued(m_StayInAnimationClip.name);
        m_Animation.Sample();
        StartCoroutine(HideUICourutine());

    }
    IEnumerator HideUICourutine()
    {
        yield return new WaitForSeconds(m_ShowUIWaitTime);
        HideUI();
    }
    public void HideUI()
    {
        m_Animation.Play(m_OutAnimationClip.name);
        m_Animation.PlayQueued(m_StayOutAnimationClip.name);
        m_Animation.Sample();

    }
}
