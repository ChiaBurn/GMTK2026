namespace CountdownAutoBattle.Gameplay
{
    /// <summary>
    /// 戰鬥效果類型。
    ///
    /// 實際解析順序由 CombatResolver 明確定義，
    /// 不直接依賴 enum 數值。
    /// </summary>
    public enum CombatEffectType
    {
        Shield = 0,
        Attack = 1,
        Heal = 2
    }
}