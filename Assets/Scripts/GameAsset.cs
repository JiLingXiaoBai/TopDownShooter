public class GameAsset
{
    private static GameAsset _instance;
    public static GameAsset Instance => _instance ??= new GameAsset();

    private GameAsset()
    {
    }

    public const int DEFAULT_LAYER = 0;
    public float StickSpeed { get; private set; } = 10f;
}