using UnityEngine;

public class Star : Item
{
    public int m_Health;
    public override void Pick()
    {
        base.Pick();
        GameManager.GetGameManager().GetPlayer().AddHealth(m_Health);
    }
    public override bool CanPick()
    {
        if (GameManager.GetGameManager().GetPlayer().m_Life >= GameManager.GetGameManager().GetPlayer().m_maxLife)
        {
            return false;
        }
        return true;
    }
}
