using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Text m_CoinsText;
    public Image m_LifeBar;

    [Header("Animations")]
    public Animation m_Animation;
    public AnimationClip m_InAnimationClip;
    public AnimationClip m_OutAnimationClip;
    public AnimationClip m_StayInAnimationClip;
    public AnimationClip m_StayOutAnimationClip;
    public float m_ShowUIWaitTime = 4.0f;

    public void Start()
    {
        SetCoins(0);
        SetLifeBar(1.0f);
        m_Animation.Play(m_StayOutAnimationClip.name);
        m_Animation.Sample();
        DependencyInjector.GetDependency<CoinsController>().m_OnCoinsChanged+=OnCoinsChanged;
        DependencyInjector.GetDependency<LifeController>().m_OnLifeChanged+=OnLifeChanged;
    }
    private void OnDestroy()
    {
        DependencyInjector.GetDependency<CoinsController>().m_OnCoinsChanged-=OnCoinsChanged;
        DependencyInjector.GetDependency<LifeController>().m_OnLifeChanged-=OnLifeChanged;       
    }
    public void SetCoins(int Coins)
    {
        m_CoinsText.text=Coins.ToString();
        
    }
    public void SetLifeBar(float m_LifeNormalized)
    {
        m_LifeBar.fillAmount=m_LifeNormalized;
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
    public void OnCoinsChanged(CoinsController _CoinsController)
    {
        SetCoins(_CoinsController.GetValue());
        ShowUI();
    }
    public void OnLifeChanged(LifeController _LifeController)
    {
        SetLifeBar(_LifeController.GetValue()/8.0f);
        ShowUI();
    }
}
