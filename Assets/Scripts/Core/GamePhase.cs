namespace CountdownAutoBattle.Core
{
    /// <summary>
    /// 單一關卡內的主要流程階段。
    /// </summary>
    public enum GamePhase
    {
        BeforeDraw = 0,
        Configuration = 1,
        Combat = 2,
        Result = 3
    }
}