using System.Numerics;
using System.Text.Json.Serialization;

namespace NMDBase
{

    public class BoneData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "MUNE1";

        [JsonPropertyName("id")]
        public ushort BoneId { get; set; } = 0;

        [JsonPropertyName("parent_index")]
        public ushort ParentId { get; set; } = 65535;

        [JsonPropertyName("header")]
        public float[] Header { get; set; } = new float[4] { 0, 0, 0, 0 };

        [JsonPropertyName("position")] 
        public Vector3 Position {  get; set; } = new Vector3(0,0,0);

        [JsonPropertyName("rotation")]
        public Vector3 Rotation { get; set; } = new Vector3(0, 0, 0);

        [JsonPropertyName("scale")]
        public Vector3 Scale { get; set; } = new Vector3(0,0,0);
        
        [JsonPropertyName("gravity")]
        public (short X, short Y) Gravity { get; set; } = (0,0);
        
        [JsonPropertyName("dampening")]
        public short Dampening { get; set; } = 0;
        
        [JsonPropertyName("length")]
        public float Length { get; set; } = 0.0f;

        [JsonPropertyName("val1")]
        public byte Val1 { get; set; } = 0;

        [JsonPropertyName("val2")]
        public byte Val2 { get; set; } = 0;

        [JsonPropertyName("val3")]
        public short Val3 { get; set; } = 0;

        [JsonPropertyName("val4")]
        public short Val4 { get; set; } = 0;

        [JsonPropertyName("val5")]
        public short Val5 { get; set; } = 0;

        [JsonPropertyName("val6")] 
        public float Val6 { get; set; } = 0;

        [JsonPropertyName("val7")]
        public short Val7 { get; set; } = 0;

        public byte InfluenceX { get; set; } = 100;
        public byte InfluenceY { get; set; } = 100;
        public byte InfluenceZ { get; set; } = 100;
        public sbyte[] Constraints { get; set; } = new sbyte[4] { 0, 0, 0, 0 };
        public sbyte Rigidity { get; set; } = 4;

        [JsonPropertyName("collision_count")]
        public byte CollisionCount { get; set; }

        [JsonPropertyName("collision_data")]
        public ushort[] CollisionList { get; set; } = new ushort[4] { 0, 0, 0, 0 };

        [JsonPropertyName("swing_count")]
        public byte SwingCount { get; set; } = 0;

        [JsonPropertyName("swing_collision_data")]
        public ushort[] SwingCollisionList { get; set; } = new ushort[4] { 0, 0, 0, 0 };

        [JsonPropertyName("flag")]
        public byte Flag { get; set; } = 0;

        [JsonPropertyName("type")]
        public BoneType Bonetype { get; set; } = BoneType.Types["none"];

        [JsonPropertyName("index")]
        public int Index { get; set; } = 0;

        [JsonPropertyName("target_id")]
        public ushort TargetID { get; set; } = 0;

        [JsonPropertyName("target_index")]
        public ushort TargetIndex { get; set; } = 0;

        [JsonPropertyName("Modifier_value1")]
        public float ModifierValue1 { get; set; } = 0;

        [JsonPropertyName("Modifier_value2")]
        public float ModifierValue2 { get; set; } = 0;

        [JsonPropertyName("axis")]
        public byte ModifierAxis { get; set; } = 0;
    }

}
