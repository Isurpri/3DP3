public class CoinsController
{
    public int m_Coins = 0;
    public delegate void OnCoinsChangedFn(CoinsController _CoinsController);
    public event OnCoinsChangedFn m_OnCoinsChanged;
    public CoinsController()
    {
        DependencyInjector.AddDependency<CoinsController>(this);
    }
    public void AddCoins(int Coins)
    {
        m_Coins += Coins;
        m_OnCoinsChanged.Invoke(this);
    }
    public int GetValue()
    {
        return m_Coins;
    }
}
