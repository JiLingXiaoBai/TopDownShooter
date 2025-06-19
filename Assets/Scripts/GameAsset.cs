public class GameAsset
{
    private static GameAsset _instance;
    public static GameAsset Instance => _instance ??= new GameAsset();

    private GameAsset()
    {
    }

    public const int DEFAULT_LAYER = 0;
    
    public const float CHARACTER_HEIGHT_OFFSET = 1.5f;
    public float StickSpeed { get; private set; } = 10f;
}