using System.Collections.Generic;

namespace NMDBase
{
    public class NMDType
    {
        public string Name { get; set; }
        public int BlockStartOffset { get; set; }
        public int NameListOffset { get; set; }
        public int BlockLength { get; set; }
        public int BoneNameOffset { get; set; }
        public int BonePhysicsOffset { get; set; }
        public int BoneLengthOffset { get; set; }
        public int BoneTypeOffset1 { get; set; }
        public int BoneTypeOffset2 { get; set; }
        public int BoneParentIdOffset { get; set; }
        public int BoneIdOffset { get; set; }
        public bool FlipValues { get; set; }
        public bool DecodeNames {  get; set; }





        public static Dictionary<string, NMDType> Types = new()
        {
            {"Other",new NMDType()
               {
                Name = "SCIV / SCV / SCLS",
                BlockStartOffset = 0x14,
                NameListOffset = 0x1c,
                BlockLength = 0x50,
                BoneNameOffset = 0x2c,
                BonePhysicsOffset = 0x30,
                BoneLengthOffset = 0x34,
                BoneTypeOffset1 = 0x44,
                BoneTypeOffset2 = 0x4b,
                BoneParentIdOffset = 0x4c,
                BoneIdOffset = 0x4e,
                FlipValues = true,
                DecodeNames = true
               }
             },

            {"SCVI",new NMDType()
               {
                Name = "SCVI",
                BlockStartOffset = 0x0c,
                NameListOffset = 0x14,
                BlockLength = 0x70,
                BoneNameOffset = 0x30,
                BonePhysicsOffset = 0x38,
                BoneLengthOffset = 0x40,
                BoneTypeOffset1 = 0x50,
                BoneTypeOffset2 = 0x5f,
                BoneParentIdOffset = 0x60,
                BoneIdOffset = 0x62,
                FlipValues = false,
                DecodeNames = true
               }
            },
        };







    }
}
