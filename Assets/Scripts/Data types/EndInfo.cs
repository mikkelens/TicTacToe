public enum EndState
{
    Playing,
    Won,
    Draw,
    WonPermanently,
    DrawPermanently
}

public class EndData
{
    public EndState State;
    public SpaceData TargetSpace;
}
