using System;
using System.Collections.Generic;

namespace NMDBase
{
    [Flags]
    public enum BoneTypeCategory
    {
        None = 0,
        Standard = 1,
        Modifier = 2,
        RotationModifier = 4,
        SlerpModifier = 8,
        ConstRotationModifier = 16,
        ConstSlerpModifier = 32,
        WeaponModifier1 = 64,
        WeaponModifier2 = 128,
        Swing = 256,
        Collision = 512,
        SCIVJunk = 1024,
        Scissor = 2048,
    }



    public class BoneType
    {
        //Category
        
        
        private BoneTypeCategory category;
        private string name = "standard";

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public BoneTypeCategory Category
        {
            get { return category; }
            set { category = value; }
        }

        //Data
        private (int, int) data = (0, 0);

        public (int, int) TypeValue
        {
            get { return data; }
            set { data = value; }
        }


        public BoneType(string _name, BoneTypeCategory _category, (int, int) _data)
        {
            Name = _name;
            Category = _category;
            TypeValue = _data;


        }
        public static Dictionary<string, BoneType> Types = new(){
            //MISSING: 0x0A (10), 0x16 (22), 0x1C (28)
            //Key                 |Name|Category|Value|
            {"none",new BoneType("Unknown",BoneTypeCategory.None,(0,-1))},
            {"standard",new BoneType("Standard",BoneTypeCategory.Standard,(0,0))},
            {"weapon",new BoneType("Weapon",BoneTypeCategory.Standard,(0,1))},
            {"finger",new BoneType("Finger",BoneTypeCategory.Standard,(0,2))},
            {"face",new BoneType("Face",BoneTypeCategory.Standard,(0,3))},
            {"rot",new BoneType("Rotation",BoneTypeCategory.RotationModifier,(0,4))},
            {"prot",new BoneType("Parent Rotation",BoneTypeCategory.Standard,(0,5))},
            {"const_slerp",new BoneType("Const Slerp",BoneTypeCategory.ConstSlerpModifier,(0,6))},
            {"const_rot",new BoneType("Const Rotation",BoneTypeCategory.ConstRotationModifier,(0,7))},
            {"slerp1",new BoneType("Slerp",BoneTypeCategory.SlerpModifier,(0,8))},
            {"slerp2",new BoneType("Slerp",BoneTypeCategory.SlerpModifier,(0,9))},
            {"swing",new BoneType("Swing",BoneTypeCategory.Swing,(4,11))},
            {"swing2",new BoneType("Swing 2",BoneTypeCategory.Swing,(5,11))},
            {"swing3",new BoneType("Swing 3",BoneTypeCategory.Swing,(6,11))},
            {"shit",new BoneType("Sphere Hit",BoneTypeCategory.Collision,(0,12))},
            {"phit",new BoneType("P Hit",BoneTypeCategory.Collision,(0,13))},
            {"sciv_junk",new BoneType("SCIV Junk",BoneTypeCategory.SCIVJunk,(0,14))},
            {"randeye",new BoneType("Soul Edge Eye",BoneTypeCategory.WeaponModifier1,(0,15))},
            {"randlid",new BoneType("Soul Edge Eyelid",BoneTypeCategory.WeaponModifier2,(0,16))},
            {"eyestare",new BoneType("StareEye",BoneTypeCategory.WeaponModifier1,(0,17))},
            {"chit",new BoneType("Cylinder Hit",BoneTypeCategory.Collision,(0,18))},
            {"spin",new BoneType("Spin",BoneTypeCategory.Standard,(0,19))},
            {"scissor",new BoneType("Scissor",BoneTypeCategory.Scissor,(0,20))},
            {"prog_auo",new BoneType("Prog Auo",BoneTypeCategory.Standard,(0,21))},
            {"prog",new BoneType("Prog",BoneTypeCategory.Standard,(0,23))},
            {"offset",new BoneType("Offset",BoneTypeCategory.Modifier,(0,24))},
            {"rot_offset",new BoneType("Rotation Offset",BoneTypeCategory.RotationModifier,(0,25))},
            {"prot_offset",new BoneType("Parent Rotation Offset",BoneTypeCategory.Standard,(0,26))},
            {"const_rot_offset",new BoneType("Const Rotation Offset",BoneTypeCategory.ConstRotationModifier,(0,27))},
            {"slerp_offset",new BoneType("Slerp Offset",BoneTypeCategory.SlerpModifier,(0,29))},
            {"breast",new BoneType("Breast",BoneTypeCategory.Swing,(4,30))}
        };
    }
}
