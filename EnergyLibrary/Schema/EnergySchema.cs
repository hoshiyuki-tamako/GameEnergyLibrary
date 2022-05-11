using ProtoBuf;

namespace EnergyLibrary.Schema
{
    /// <summary>
    /// Schema for export data
    /// </summary>
    [ProtoContract]
    public class EnergyOptionSchema
    {
        [ProtoMember(1)]
        public bool AllowOverFlow { get; set; }

        [ProtoMember(2)]
        public long IntervalTick { get; set; }

        [ProtoMember(3)]
        public int MinAmount { get; set; }

        [ProtoMember(4)]
        public int MaxAmount { get; set; }
    }

    /// <summary>
    /// Schema for export data
    /// </summary>
    [ProtoContract]
    public class EnergySchema
    {
        [ProtoMember(1)]
        public EnergyOptionSchema Option { get; set; } = new();

        [ProtoMember(2)]
        public int Amount { get; set; }

        [ProtoMember(3)]
        public long LastReceived { get; set; }
    }
}
