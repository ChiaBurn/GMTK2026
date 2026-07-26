using System;

namespace CountdownAutoBattle.Gameplay
{
    /// <summary>
    /// 單張點數卡的執行期資料。
    /// 相同數字的卡片仍具有不同 InstanceId。
    /// </summary>
    [Serializable]
    public sealed class PointCardInstance
    {
        public PointCardInstance(int instanceId, int value)
        {
            InstanceId = instanceId;
            Value = value;
        }

        public int InstanceId { get; }

        public int Value { get; }

        public override string ToString()
        {
            return $"Card #{InstanceId}, Value: {Value}";
        }
    }
}