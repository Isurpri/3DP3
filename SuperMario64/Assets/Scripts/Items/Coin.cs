using UnityEngine;

public class Coin : Item
{
    public int m_Coin;
    public override void Pick()
    {
        base.Pick();
        GameManager.GetGameManager().GetComponent<CoinsController>().AddCoins(1);
    }
    public override bool CanPick()
    {

        return true;
    }
}
