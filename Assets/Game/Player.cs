public class Player
{
    public void AddCoins( int value ) => coins += value;

    public string name { get; private set; }
    public int coins { get; private set; }

    public Player()
    {
        name = "Player";
        coins = 0;
    }

    public Player( Player player )
    {
        name = player.name;
        coins = player.coins;
    }
}