using UnityEngine;

public class Star : Item
{
    public int m_Health;
    public override void Pick()
    {
        base.Pick();
        GameManager.GetGameManager().GetComponent<LifeController>().AddLife(1);
    }
    public override bool CanPick()
    {
        if (GameManager.GetGameManager().GetComponent<LifeController>().GetValue() >= GameManager.GetGameManager().GetPlayer().m_maxLife)
        {
            return false;
        }
        return true;
    }
}
