using UnityEngine;

public class LifeController
{
    public int m_Life = 8;
    public delegate void OnLifeChangedFn(LifeController _LifeController);
    public event OnLifeChangedFn m_OnLifeChanged;
    
    public LifeController()
    {
        DependencyInjector.AddDependency<LifeController>(this);
    }

    public void AddLife(int Life)
    {
        if (m_Life > 0)
        {
            m_Life += Life;
            m_OnLifeChanged.Invoke(this);
        }
        else if (m_Life >= 0)
        {
            m_Life = 0; ;
        }
       
    }
    public int GetValue()
    {
        return m_Life;
    }    
}
