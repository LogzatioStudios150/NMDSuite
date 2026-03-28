using Newtonsoft.Json.Linq;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Numerics;

namespace NMDBase
{
    public static class NMDUtil
    {

        public static (List<BoneNode> RootBones, List<BoneNode> Bones,int Count) ParseNMD(string nmd_path, bool keep_eye, List<BoneNode> root_bones,List<BoneNode> bones)
        {
            //List<BoneNode> RootBones = root_bones;
           // List<BoneNode> Bones = bones;
            

            using (BinaryReader br = new(File.OpenRead(nmd_path)))
            {
                int file_header = 0;
                NMDType nmd_type = NMDType.Types["Other"];
                BoneType bone_type = BoneType.Types["none"];
                ushort bone_count = 0;
                ushort index = (ushort)((bones.Count > 0) ? (bones.Count - 1) : 0);
                ushort removed_bones = 0;
                ushort last_removed_index = 65535;
                List<string> eye_bones = new() { "EYE_L", "EYE_R", "DOUKO_L", "DOUKO_R" };

                long return_address = file_header;




                // Find 'NMD' header dynamically
                while (true)
                {
                    string buffer = Encoding.UTF8.GetString(br.ReadBytes(3));

                    if (buffer is "NMD")
                    {
                        file_header = (int)return_address;
                        break;
                    }

                    br.BaseStream.Position += 1;
                    return_address += BinaryPrimitives.ReadInt32BigEndian(br.ReadBytes(4));
                    br.BaseStream.Position = return_address;
                }



                // Get NMD layout type
                br.BaseStream.Position = file_header + 0x5;
                byte nmd_type_byte = br.ReadByte();
                nmd_type = nmd_type_byte switch
                {
                    3 => NMDType.Types["SCVI"],
                    _ => NMDType.Types["Other"],
                };


                // Get bone count
                br.BaseStream.Position = file_header + 0xA;
                bone_count = (nmd_type.FlipValues) ? BinaryPrimitives.ReadUInt16BigEndian(br.ReadBytes(2)) : br.ReadUInt16();
                

                //loop over bone data
                br.BaseStream.Position = file_header + nmd_type.BlockStartOffset;

                br.BaseStream.Position = (nmd_type.FlipValues) ? file_header + BinaryPrimitives.ReadInt32BigEndian(br.ReadBytes(4)) : br.ReadInt32();
                int block_start = (nmd_type.FlipValues) ? BinaryPrimitives.ReadInt32BigEndian(br.ReadBytes(4)) : br.ReadInt32();
                br.BaseStream.Position = file_header + block_start;



                for (int i = 0; i < bone_count; i++)
                {
                    return_address = br.BaseStream.Position;
                    BoneData data = new();
                   


                    br.BaseStream.Seek(nmd_type.BoneTypeOffset1, SeekOrigin.Current);
                    int type1 = br.ReadByte();

                    br.BaseStream.Position = return_address + nmd_type.BoneTypeOffset2;

                    int type2 = br.ReadByte();



                    (int, int) type_bytes = (type1, type2);
                    foreach (KeyValuePair<string, BoneType> type in BoneType.Types)
                    {
                        BoneType value = type.Value;
                        if (value.TypeValue == type_bytes)
                        {
                            data.Bonetype = type.Value;

                            break;
                        }
                    }

                    if (data.Bonetype.TypeValue != type_bytes)
                    {
                        Console.WriteLine($"Unknown Bone Type: {type_bytes}");
                        Trace.WriteLine($"Unknown Bone Type: {type_bytes}");
                        data.Bonetype = BoneType.Types["none"];
                    }

                    br.BaseStream.Position = return_address;


                    if (data.Bonetype.Category == BoneTypeCategory.SCIVJunk)
                    {
                        last_removed_index = index;
                        bone_count -= 9;
                        removed_bones += 9;
                        br.BaseStream.Position = return_address + nmd_type.BlockLength * 9;
                        continue;
                    }
                    for (int j = 0; j < 4; j++)
                    {
                        data.Header[j] = nmd_type.FlipValues ? BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) : br.ReadSingle();

                    }
                    Vector3 position = new(0, 0, 0)
                    {
                        X = nmd_type.FlipValues ? BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) : br.ReadSingle(),
                        Y = nmd_type.FlipValues ? BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) : br.ReadSingle(),
                        Z = nmd_type.FlipValues ? BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) : br.ReadSingle()
                    };
                    data.Position = position;

                    br.BaseStream.Position += 4;

                    Vector3 rotation = new(0, 0, 0)
                    {
                        X = nmd_type.FlipValues ? (float)BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) * 360 : (float)br.ReadSingle() * 360,
                        Y = nmd_type.FlipValues ? BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) * 360 : br.ReadSingle() * 360,
                        Z = nmd_type.FlipValues ? BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) * 360 : br.ReadSingle() * 360
                    };
                    data.Rotation = rotation;

                    br.BaseStream.Position = return_address;



                    if (data.Bonetype.Category == BoneTypeCategory.Swing)
                    {
                        br.BaseStream.Position = return_address + nmd_type.BonePhysicsOffset;
                        br.BaseStream.Position = nmd_type.FlipValues ? file_header + BinaryPrimitives.ReadUInt32BigEndian(br.ReadBytes(4)) : file_header + br.ReadUInt32();

                        data.Val1 = br.ReadByte();
                        data.Val2 = br.ReadByte();

                        data.Dampening = nmd_type.FlipValues ? BinaryPrimitives.ReadInt16BigEndian(br.ReadBytes(2)) : br.ReadInt16();

                        data.Val3 = nmd_type.FlipValues ? BinaryPrimitives.ReadInt16BigEndian(br.ReadBytes(2)) : br.ReadInt16();

                        data.Val4 = nmd_type.FlipValues ? BinaryPrimitives.ReadInt16BigEndian(br.ReadBytes(2)) : br.ReadInt16();

                        data.Val5 = nmd_type.FlipValues ? BinaryPrimitives.ReadInt16BigEndian(br.ReadBytes(2)) : br.ReadInt16();

                        br.BaseStream.Position += 6;

                        data.Val6 = nmd_type.FlipValues ? BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) : br.ReadSingle();

                        data.Val7 = nmd_type.FlipValues ? BinaryPrimitives.ReadInt16BigEndian(br.ReadBytes(2)) : br.ReadInt16();

                        br.BaseStream.Position += 1;

                        data.InfluenceX = br.ReadByte();
                        data.InfluenceY = br.ReadByte();
                        data.InfluenceZ = br.ReadByte();

                        br.BaseStream.Position = nmd_type.FlipValues ? return_address + nmd_type.BonePhysicsOffset + 4 : return_address + nmd_type.BonePhysicsOffset + 8;

                        data.Length = nmd_type.FlipValues ? BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) : br.ReadSingle();

                        (short X, short Y) gravity = new(0, 0)
                        {
                            X = nmd_type.FlipValues ? BinaryPrimitives.ReadInt16BigEndian(br.ReadBytes(2)) : br.ReadInt16(),
                            Y = nmd_type.FlipValues ? BinaryPrimitives.ReadInt16BigEndian(br.ReadBytes(2)) : br.ReadInt16()
                        };
                        data.Gravity = gravity;

                        for (int j = 0; j < 4; j++)
                        {
                            data.Constraints[j] = br.ReadSByte();
                        }

                        data.Rigidity = br.ReadSByte();

                        data.CollisionCount = br.ReadByte();


                        data.SwingCount = br.ReadByte();

                        data.Flag = br.ReadByte();

                        br.BaseStream.Position = return_address;


                        for (int l = 0; l < 4; l++)
                        {

                            ushort index_value = nmd_type.FlipValues ? BinaryPrimitives.ReadUInt16BigEndian(br.ReadBytes(2)) : br.ReadUInt16();

                            if (index_value > last_removed_index && last_removed_index != 65535)
                            {
                                index_value -= removed_bones;
                            }

                            data.CollisionList[l] = index_value;

                        }

                        //data.CollisionList = collision_list;

                        for (int n = 0; n < 4; n++)
                        {

                            ushort index_value = nmd_type.FlipValues ? BinaryPrimitives.ReadUInt16BigEndian(br.ReadBytes(2)) : br.ReadUInt16();

                            if (index_value > last_removed_index && last_removed_index != 65535)
                            {
                                index_value -= removed_bones;
                            }

                            data.SwingCollisionList[n] = index_value;

                        }

                        br.BaseStream.Position = return_address;


                    }

                    if (data.Bonetype.Category == BoneTypeCategory.RotationModifier)
                    {
                        br.BaseStream.Position += nmd_type.BonePhysicsOffset;
                        data.ModifierValue1 = nmd_type.FlipValues ? BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) : br.ReadSingle();

                        data.TargetID = nmd_type.FlipValues ? BinaryPrimitives.ReadUInt16BigEndian(br.ReadBytes(2)) : br.ReadUInt16();

                        data.ModifierAxis = br.ReadByte();

                        br.BaseStream.Position = return_address;
                    }

                    if (data.Bonetype.Category == BoneTypeCategory.ConstRotationModifier)
                    {
                        br.BaseStream.Position += nmd_type.BonePhysicsOffset;
                        data.ModifierValue1 = nmd_type.FlipValues ? BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) : br.ReadSingle();

                        data.TargetID = nmd_type.FlipValues ? BinaryPrimitives.ReadUInt16BigEndian(br.ReadBytes(2)) : br.ReadUInt16();

                        data.TargetIndex = nmd_type.FlipValues ? BinaryPrimitives.ReadUInt16BigEndian(br.ReadBytes(2)) : br.ReadUInt16();

                        br.BaseStream.Position = return_address;

                    }

                    if (data.Bonetype.Category == BoneTypeCategory.SlerpModifier)
                    {
                        br.BaseStream.Position += nmd_type.BonePhysicsOffset;

                        data.ModifierValue1 = nmd_type.FlipValues ? BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) : br.ReadSingle();
                        data.TargetID = nmd_type.FlipValues ? BinaryPrimitives.ReadUInt16BigEndian(br.ReadBytes(2)) : br.ReadUInt16();
                        data.TargetIndex = nmd_type.FlipValues ? BinaryPrimitives.ReadUInt16BigEndian(br.ReadBytes(2)) : br.ReadUInt16();
                        data.ModifierValue2 = nmd_type.FlipValues ? BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) : br.ReadSingle();
                        //data.ModifierIndex3 = nmd_type.FlipValues ? BinaryPrimitives.ReadUInt16BigEndian(br.ReadBytes(2)) : br.ReadUInt16();

                        br.BaseStream.Position = return_address;
                    }

                    if (data.Bonetype.Category == BoneTypeCategory.ConstSlerpModifier)
                    {
                        br.BaseStream.Position += nmd_type.BonePhysicsOffset;

                        data.TargetID = nmd_type.FlipValues ? BinaryPrimitives.ReadUInt16BigEndian(br.ReadBytes(2)) : br.ReadUInt16();
                        
                        br.BaseStream.Position = return_address;
                    }

                    if (data.Bonetype.Category == BoneTypeCategory.WeaponModifier1)
                    {
                        br.BaseStream.Position += nmd_type.BonePhysicsOffset;

                        data.ModifierValue1 = (float)(nmd_type.FlipValues ? Math.Round(BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) * 360, 2) : Math.Round(br.ReadSingle() * 360, 2));
                        data.ModifierValue2 = (float)(nmd_type.FlipValues ? Math.Round(BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) * 360, 2) : Math.Round(br.ReadSingle() * 360, 2));
                        
                        br.BaseStream.Position = return_address;
                    }

                    if (data.Bonetype.Category == BoneTypeCategory.WeaponModifier2)
                    {
                        br.BaseStream.Position += nmd_type.BonePhysicsOffset;

                        data.ModifierValue1 = (float)(nmd_type.FlipValues ? Math.Round(BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) * 360, 2) : Math.Round(br.ReadSingle() * 360, 2));
                        data.TargetID = nmd_type.FlipValues ? BinaryPrimitives.ReadUInt16BigEndian(br.ReadBytes(2)) : br.ReadUInt16();

                        br.BaseStream.Position = return_address;
                    }

                    if (data.Bonetype.Category == BoneTypeCategory.Scissor)
                    {
                        br.BaseStream.Position += nmd_type.BonePhysicsOffset;

                        data.ModifierValue1 = (float)(nmd_type.FlipValues ? Math.Round(BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) * 360, 2) : Math.Round(br.ReadSingle() * 360, 2));
                        data.ModifierValue2 = nmd_type.FlipValues ? BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) : br.ReadSingle();
                        data.ModifierAxis = br.ReadByte();

                        br.BaseStream.Position = return_address;
                    }

                    for (int k = 0; k < 4; k++)
                    {
                        data.Header[k] = nmd_type.FlipValues ? BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) : br.ReadSingle();
                    }
                    br.BaseStream.Position = return_address;
                    br.BaseStream.Seek(nmd_type.BoneNameOffset, SeekOrigin.Current);
                    br.BaseStream.Position = (nmd_type.FlipValues) ? file_header + BinaryPrimitives.ReadUInt32BigEndian(br.ReadBytes(4)) : br.ReadUInt32();
                    List<byte> name_byte_list = new();

                    while (true)
                    {
                        byte val = br.ReadByte();
                        if (val == 0x40)
                        {
                            break;
                        }
                        name_byte_list.Add(val);
                    }

                    if (nmd_type.DecodeNames)
                    {
                        for (int m = 0; m < name_byte_list.Count; m++)
                        {
                            name_byte_list[m] -= 0x40;
                        }
                    }
                    

                    data.Name = Encoding.UTF8.GetString(name_byte_list.ToArray());

                    if (keep_eye is false && eye_bones.Any(bone => bone.Equals(data.Name)))
                    {
                        last_removed_index = index;
                        removed_bones += 1;

                        br.BaseStream.Position = return_address + nmd_type.BlockLength;
                        continue;

                    }
                    br.BaseStream.Position = return_address + nmd_type.BonePhysicsOffset;
                    Vector3 scale = new(0, 0, 0)
                    {
                        X = nmd_type.FlipValues ? BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) : br.ReadSingle(),
                        Y = nmd_type.FlipValues ? BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) : br.ReadSingle(),
                        Z = nmd_type.FlipValues ? BinaryPrimitives.ReadSingleBigEndian(br.ReadBytes(4)) : br.ReadSingle()
                    };
                    data.Scale = scale;

                    br.BaseStream.Position = return_address + nmd_type.BoneParentIdOffset;
                    data.ParentId = nmd_type.FlipValues ? BinaryPrimitives.ReadUInt16BigEndian(br.ReadBytes(2)) : br.ReadUInt16();
                    if (data.ParentId > last_removed_index && data.ParentId != 65535)
                    {
                        data.ParentId -= removed_bones;
                    }
                    data.BoneId = nmd_type.FlipValues ? BinaryPrimitives.ReadUInt16BigEndian(br.ReadBytes(2)) : br.ReadUInt16();
                    if (data.BoneId > last_removed_index && data.ParentId != 65535)
                    {
                        data.BoneId -= removed_bones;
                    }

                    br.BaseStream.Position = return_address + nmd_type.BlockLength;
                    data.Index = index;
                    string axis = "";
                    axis = data.ModifierAxis switch
                    {
                        0 => "X",
                        1 => "Y",
                        2 => "Z",
                        _ => "X"
                    };
                    
                    
                    Console.WriteLine($"Name:{data.Name}");
                    Console.WriteLine($"Index: {data.Index}");
                    Console.WriteLine($"Parent Index: {data.ParentId}");
                    Console.WriteLine($"Position > X: {data.Position.X}, Y: {data.Position.Y}, Z: {data.Position.Z}");
                    Console.WriteLine($"Rotation > X: {data.Rotation.X}, Y: {data.Rotation.Y}, Z: {data.Rotation.Z}");
                    Console.WriteLine($"Scale > X:{data.Scale.X}, Y:{data.Scale.Y}, Z:{data.Scale.Z}");
                    Console.WriteLine($"Bone Type: {bone_type.Name}");
                    Console.WriteLine($"Bone Type Category: {bone_type.Category}");
                    if (data.Bonetype.Category == BoneTypeCategory.Swing) {
                        Console.WriteLine("- - - - swing - - - - ");
                        Console.WriteLine($"Val1: {data.Val1}");
                        Console.WriteLine($"Val2: {data.Val2}");
                        Console.WriteLine($"Dampening: {data.Dampening}");
                        Console.WriteLine($"Val3: {data.Val3}");
                        Console.WriteLine($"Val4: {data.Val4}");
                        Console.WriteLine($"Val5: {data.Val5}");
                        Console.WriteLine($"Val6: {data.Val6}");
                        Console.WriteLine($"Val7: {data.Val7}");
                        Console.WriteLine($"Influence: X: {data.InfluenceX}, Y: {data.InfluenceY}, Z: {data.InfluenceZ}");
                        Console.WriteLine($"Length: {data.Length}");
                        Console.WriteLine($"Gravity > X: {data.Gravity.X}, Y: {data.Gravity.Y}");
                        Console.WriteLine($"Constraints > +X: {data.Constraints[0]}, -X: {data.Constraints[1]}, +Y: {data.Constraints[2]}, -Y: {data.Constraints[3]}");
                        Console.WriteLine($"Rigidity: {data.Rigidity}");
                        Console.WriteLine($"Collision Count: {data.CollisionCount}");
                        Console.WriteLine($"Swing Collision Count: {data.SwingCount}");
                        if (data.CollisionCount > 0)
                        {
                            Console.WriteLine($"Collision Bones (Index):");
                            foreach (ushort val in data.CollisionList)
                            {
                                Console.WriteLine($"{val}");
                            }
                        }
                        if (data.SwingCount > 0)
                        {
                            Console.WriteLine($"Swing Collision Bones (Index):");
                            foreach (ushort val in data.SwingCollisionList)
                            {
                                Console.WriteLine($"{val}");
                            }
                        }
                        Console.WriteLine($"Flag: {data.Flag}");
                    }
                    if (data.Bonetype.Category.ToString().Contains("modifier") ) {
                        Console.WriteLine("- - - - modifier - - - - ");
                        Console.WriteLine($"Modifier Value >  1: {data.ModifierValue1}, 2: {data.ModifierValue2}");
                        Console.WriteLine($"Modifier Index >  1: {data.TargetID}, 2: {data.TargetIndex}, 3: {data.TargetIndex}");
                        if (data.Bonetype.Category.ToString().Contains("rotation"))
                        {
                            Console.WriteLine($"Modifier Axis: {axis}");
                        }
                        
                    }
           
                   

                    Console.WriteLine("______________________________\n");
                    BoneNode bone = new(data);

                    if (bone.Data.ParentId == 65535)
                    {
                        bones.Add(bone);
                    }
                    else
                    {
                        bones.Add(bone);
                        bones[bone.Data.ParentId].Items.Add(bone);
                    }
                    index += 1;
                    

                }
                for (int i = 0; i < bones.Count; i++)
                {
                    if (bones[i].Data.ParentId == 65535)
                    {
                        root_bones.Add(bones[i]);
                    }
                }

            }
            Console.WriteLine($"Number of bones: {bones.Count}");
       
            return (root_bones, bones, bones.Count);
        }

        public static (List<BoneNode> RootBones, List<BoneNode> Bones, int Count) ParseJson(string json_path, List<BoneNode> root_bones, List<BoneNode> bones)
        {
            if (!File.Exists(json_path)) return (root_bones, bones, 0);

            string jsonString = File.ReadAllText(json_path);
            JArray jArray = JArray.Parse(jsonString);

            foreach (JToken jToken in jArray)
            {
                JObject jObj = (JObject)jToken;
                BoneData data = new();

                if (jObj["name"] != null) data.Name = (string)jObj["name"];
                if (jObj["id"] != null) data.BoneId = (ushort)jObj["id"];
                if (jObj["index"] != null) data.Index = (int)jObj["index"];

                if (jObj["parent_index"] != null)
                {
                    int parent_index = (int)jObj["parent_index"];
                    data.ParentId = parent_index == -1 ? (ushort)65535 : (ushort)parent_index;
                }

                if (jObj["type"] != null)
                {
                    string typeToken = (string)jObj["type"];
                    if (!string.IsNullOrEmpty(typeToken) && BoneType.Types.ContainsKey(typeToken))
                    {
                        data.Bonetype = BoneType.Types[typeToken];
                    }
                }

                JArray headerArray = jObj["header"] as JArray;
                if (headerArray != null && headerArray.Count >= 4)
                {
                    data.Header[0] = (float)headerArray[0];
                    data.Header[1] = (float)headerArray[1];
                    data.Header[2] = (float)headerArray[2];
                    data.Header[3] = (float)headerArray[3];
                }

                JToken posToken = jObj["position"];
                if (posToken != null)
                    data.Position = new Vector3((float)posToken["x"], (float)posToken["y"], (float)posToken["z"]);

                JToken rotToken = jObj["rotation"];
                if (rotToken != null)
                    data.Rotation = new Vector3((float)rotToken["x"], (float)rotToken["y"], (float)rotToken["z"]);

                JToken scaleToken = jObj["scale"];
                if (scaleToken != null)
                    data.Scale = new Vector3((float)scaleToken["x"], (float)scaleToken["y"], (float)scaleToken["z"]);

                JToken modToken = jObj["modifier_params"];
                if (modToken != null && modToken.Type != JTokenType.Null)
                {
                    data.ModifierValue1 = (float)modToken["modifier_value1"];
                    data.ModifierValue2 = (float)modToken["modifier_value2"];
                    data.ModifierAxis = (byte)modToken["axis"];
                    data.TargetID = (ushort)modToken["target_id"];
                    data.TargetIndex = (ushort)modToken["target_index"];
                }

                JToken swingToken = jObj["swing_params"];
                if (swingToken != null && swingToken.Type != JTokenType.Null)
                {
                    JArray colData = swingToken["collision_data"] as JArray;
                    if (colData != null)
                        for (int i = 0; i < 4 && i < colData.Count; i++)
                            data.CollisionList[i] = (ushort)colData[i];

                    data.CollisionCount = (byte)swingToken["collision_count"];

                    JArray swingColData = swingToken["swing_collision_data"] as JArray;
                    if (swingColData != null)
                        for (int i = 0; i < 4 && i < swingColData.Count; i++)
                            data.SwingCollisionList[i] = (ushort)swingColData[i];

                    data.SwingCount = (byte)swingToken["swing_count"];
                    data.Length = (float)swingToken["length"];

                    JToken gravToken = swingToken["gravity"];
                    if (gravToken != null)
                        data.Gravity = ((short)gravToken["x"], (short)gravToken["y"]);

                    data.Dampening = (short)swingToken["dampening"];

                    JArray constraints = swingToken["constraints"] as JArray;
                    if (constraints != null)
                        for (int i = 0; i < 4 && i < constraints.Count; i++)
                            data.Constraints[i] = (sbyte)constraints[i];

                    data.Rigidity = (sbyte)swingToken["rigidity"];
                    data.Flag = (byte)swingToken["flag"];
                    data.Val1 = (byte)swingToken["val1"];
                    data.Val2 = (byte)swingToken["val2"];
                    data.Val3 = (short)swingToken["val3"];
                    data.Val4 = (short)swingToken["val4"];
                    data.Val5 = (short)swingToken["val5"];
                    data.Val6 = (float)swingToken["val6"];
                    data.Val7 = (short)swingToken["val7"];

                    JToken infToken = swingToken["influence"];
                    if (infToken != null)
                    {
                        data.InfluenceX = (byte)infToken["x"];
                        data.InfluenceY = (byte)infToken["y"];
                        data.InfluenceZ = (byte)infToken["z"];
                    }
                }

                BoneNode bone = new(data);

                if (bone.Data.ParentId == 65535)
                {
                    bones.Add(bone);
                }
                else
                {
                    bones.Add(bone);
                    if (bones.Count > bone.Data.ParentId)
                        bones[bone.Data.ParentId].Items.Add(bone);
                }
            }

            for (int i = 0; i < bones.Count; i++)
            {
                if (bones[i].Data.ParentId == 65535)
                {
                    root_bones.Add(bones[i]);
                }
            }

            Console.WriteLine($"Number of bones imported from JSON: {bones.Count}");

            return (root_bones, bones, bones.Count);
        }

        public static string ExportJson(List<BoneNode> bones, string export_path)
        {
            JArray jArray = new JArray();

            foreach (var node in bones)
            {
                var data = node.Data;

                JObject jObj = new JObject();
                jObj["name"] = data.Name;
                jObj["id"] = data.BoneId;
                jObj["index"] = data.Index;
                
                string typeKey = "none";
                foreach (var kvp in BoneType.Types)
                {
                    if (kvp.Value == data.Bonetype)
                    {
                        typeKey = kvp.Key;
                        break;
                    }
                }
                jObj["type"] = typeKey;

                string parentName = "Root";
                if (data.ParentId != 65535 && data.ParentId < bones.Count)
                    parentName = bones[data.ParentId].Data.Name;
                jObj["parent_name"] = parentName;
                jObj["parent_index"] = data.ParentId == 65535 ? -1 : (int)data.ParentId;

                jObj["header"] = new JArray(data.Header[0], data.Header[1], data.Header[2], data.Header[3]);

                JObject modObj = new JObject();
                modObj["modifier_value1"] = data.ModifierValue1;
                modObj["modifier_value2"] = data.ModifierValue2;
                modObj["modifier_value3"] = 0.0f;
                modObj["axis"] = data.ModifierAxis;
                modObj["target_id"] = data.TargetID;
                modObj["target_index"] = data.TargetIndex;
                jObj["modifier_params"] = modObj;

                JObject posObj = new JObject();
                posObj["x"] = data.Position.X;
                posObj["y"] = data.Position.Y;
                posObj["z"] = data.Position.Z;
                jObj["position"] = posObj;

                JObject rotObj = new JObject();
                rotObj["x"] = data.Rotation.X;
                rotObj["y"] = data.Rotation.Y;
                rotObj["z"] = data.Rotation.Z;
                jObj["rotation"] = rotObj;

                JObject scaleObj = new JObject();
                scaleObj["x"] = data.Scale.X;
                scaleObj["y"] = data.Scale.Y;
                scaleObj["z"] = data.Scale.Z;
                jObj["scale"] = scaleObj;

                if (data.Name.EndsWith("__swing") || data.Bonetype.Category == BoneTypeCategory.Swing)
                {
                    scaleObj["x"] = 1.0;
                    scaleObj["y"] = 1.0;
                    scaleObj["z"] = 1.0;
                    jObj["scale"] = scaleObj;

                    JObject swingObj = new JObject();
                    swingObj["collision_data"] = new JArray(data.CollisionList[0], data.CollisionList[1], data.CollisionList[2], data.CollisionList[3]);
                    swingObj["collision_count"] = data.CollisionCount;
                    swingObj["swing_collision_data"] = new JArray(data.SwingCollisionList[0], data.SwingCollisionList[1], data.SwingCollisionList[2], data.SwingCollisionList[3]);
                    swingObj["swing_count"] = data.SwingCount;
                    swingObj["length"] = data.Length;
                
                    JObject gravObj = new JObject();
                    gravObj["x"] = data.Gravity.X;
                    gravObj["y"] = data.Gravity.Y;
                    swingObj["gravity"] = gravObj;

                    swingObj["dampening"] = data.Dampening;
                    swingObj["constraints"] = new JArray(data.Constraints[0], data.Constraints[1], data.Constraints[2], data.Constraints[3]);
                    swingObj["rigidity"] = data.Rigidity;
                    swingObj["flag"] = data.Flag;
                    swingObj["val1"] = data.Val1;
                    swingObj["val2"] = data.Val2;
                    swingObj["val3"] = data.Val3;
                    swingObj["val4"] = data.Val4;
                    swingObj["val5"] = data.Val5;
                    swingObj["val6"] = data.Val6;
                    swingObj["val7"] = data.Val7;

                    JObject infObj = new JObject();
                    infObj["x"] = data.InfluenceX;
                    infObj["y"] = data.InfluenceY;
                    infObj["z"] = data.InfluenceZ;
                    swingObj["influence"] = infObj;

                    jObj["swing_params"] = swingObj;
                }

                jArray.Add(jObj);
            }

            File.WriteAllText(export_path, jArray.ToString(Newtonsoft.Json.Formatting.Indented));
            return $"Successfully saved {Path.GetFileName(export_path)}.";
        }

        


        public static string ExportNMD(List<BoneNode> bones, string export_path)
        {
            //export_path = "./hello.nmd";
            try
            {
                if (File.Exists(export_path))
                {
                    File.Delete(export_path);
                }
                NMDType nmd_type = NMDType.Types["SCVI"];
                using (BinaryWriter bw = new(File.OpenWrite(export_path)))
                {
                    long block_return = 0x20;
                    int name_list_offset = nmd_type.NameListOffset;
                    long physics_return = 0;
                    int physics_count = 0;
                    int name_list_start = 0;
                    long name_list_return = 0;
                    

                    bw.Write(new byte[10] { 0x4e, 0x4d, 0x44, 0x3, 0x4, 0x3, 0x2, 0x0, 0x0, 0x1 });
                    bw.Write((short)bones.Count);
                    bw.Write(new byte[16] { 0x18, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x20, 0x0, 0x0, 0x0 });
                    bw.Write((short)bones.Count);
                    bw.Write(new byte[2] { 0x0, 0x0 });


                    bw.Write(new byte[bones.Count * nmd_type.BlockLength]);
                    physics_return = bw.BaseStream.Position;

                    for (int i = 0; i < bones.Count; i++)
                    {
                        if (bones[i].Data.Bonetype.Category == BoneTypeCategory.Swing)
                        {
                            physics_count += 1;
                        }
                    };

                    bw.Write(new byte[physics_count * 0x20]);

                    name_list_return = bw.BaseStream.Position;
                    name_list_start = (int)bw.BaseStream.Position;
                    bw.BaseStream.Position = block_return;



                    for (int j = 0; j < bones.Count; j++)
                    {

                        if (bones[j].Data.Bonetype.Category == BoneTypeCategory.Swing)
                        {
                            bw.Write(bones[j].Data.CollisionList[0]);
                            bw.Write(bones[j].Data.CollisionList[1]);
                            bw.Write(bones[j].Data.CollisionList[2]);
                            bw.Write(bones[j].Data.CollisionList[3]);

                            bw.Write(bones[j].Data.SwingCollisionList[0]);
                            bw.Write(bones[j].Data.SwingCollisionList[1]);
                            bw.Write(bones[j].Data.SwingCollisionList[2]);
                            bw.Write(bones[j].Data.SwingCollisionList[3]);

                            bw.Write(bones[j].Data.Position.X);
                            bw.Write(bones[j].Data.Position.Y);
                            bw.Write(bones[j].Data.Position.Z);
                            bw.Write(1.0f);

                            bw.Write(bones[j].Data.Rotation.X / 360);
                            bw.Write(bones[j].Data.Rotation.Y / 360);
                            bw.Write(bones[j].Data.Rotation.Z / 360);
                            bw.Write(0.0f);

                            bw.Write(name_list_return);

                            bw.Write(physics_return);
                            bw.Write(bones[j].Data.Length);
                            bw.Write(bones[j].Data.Gravity.X);
                            bw.Write(bones[j].Data.Gravity.Y);
                            bw.Write(bones[j].Data.Constraints[0]);
                            bw.Write(bones[j].Data.Constraints[1]);
                            bw.Write(bones[j].Data.Constraints[2]);
                            bw.Write(bones[j].Data.Constraints[3]);
                            bw.Write(bones[j].Data.Rigidity);
                            bw.Write(bones[j].Data.CollisionCount);
                            bw.Write(bones[j].Data.SwingCount);
                            bw.Write(bones[j].Data.Flag);
                            bw.BaseStream.Position = physics_return;
                            bw.Write(bones[j].Data.Val1);
                            bw.Write(bones[j].Data.Val2);
                            bw.Write(bones[j].Data.Dampening);
                            bw.Write(bones[j].Data.Val3);
                            bw.Write(bones[j].Data.Val4);
                            bw.Write(bones[j].Data.Val5);
                            bw.BaseStream.Position += 6;
                            bw.Write(bones[j].Data.Val6);
                            bw.Write(bones[j].Data.Val7);
                            bw.BaseStream.Position += 1;
                            bw.Write(bones[j].Data.InfluenceX);
                            bw.Write(bones[j].Data.InfluenceY);
                            bw.Write(bones[j].Data.InfluenceZ);
                            physics_return += 0x20;
                            bw.BaseStream.Position = block_return;

                        }

                        if (bones[j].Data.Bonetype.Category == BoneTypeCategory.RotationModifier)
                        {
                            bw.Write(bones[j].Data.Header[0]);
                            bw.Write(bones[j].Data.Header[1]);
                            bw.Write(bones[j].Data.Header[2]);
                            bw.Write(bones[j].Data.Header[3]);

                            bw.Write(bones[j].Data.Position.X);
                            bw.Write(bones[j].Data.Position.Y);
                            bw.Write(bones[j].Data.Position.Z);
                            bw.Write(1.0f);

                            bw.Write(bones[j].Data.Rotation.X / 360);
                            bw.Write(bones[j].Data.Rotation.Y / 360);
                            bw.Write(bones[j].Data.Rotation.Z / 360);
                            bw.Write(0.0f);
                            bw.Write(name_list_return);

                            bw.Write(bones[j].Data.ModifierValue1);
                            bw.Write(bones[j].Data.TargetID);
                            bw.Write(bones[j].Data.ModifierAxis);

                        }

                        if (bones[j].Data.Bonetype.Category == BoneTypeCategory.ConstRotationModifier)
                        {
                            bw.Write(bones[j].Data.Header[0]);
                            bw.Write(bones[j].Data.Header[1]);
                            bw.Write(bones[j].Data.Header[2]);
                            bw.Write(bones[j].Data.Header[3]);

                            bw.Write(bones[j].Data.Position.X);
                            bw.Write(bones[j].Data.Position.Y);
                            bw.Write(bones[j].Data.Position.Z);
                            bw.Write(1.0f);

                            bw.Write(bones[j].Data.Rotation.X / 360);
                            bw.Write(bones[j].Data.Rotation.Y / 360);
                            bw.Write(bones[j].Data.Rotation.Z / 360);
                            bw.Write(0.0f);
                            bw.Write(name_list_return);

                            bw.Write(bones[j].Data.ModifierValue1);
                            bw.Write(bones[j].Data.TargetID);
                            bw.Write(bones[j].Data.TargetIndex);

                        }

                        if (bones[j].Data.Bonetype.Category == BoneTypeCategory.SlerpModifier)
                        {
                            bw.Write(bones[j].Data.Header[0]);
                            bw.Write(bones[j].Data.Header[1]);
                            bw.Write(bones[j].Data.Header[2]);
                            bw.Write(bones[j].Data.Header[3]);

                            bw.Write(bones[j].Data.Position.X);
                            bw.Write(bones[j].Data.Position.Y);
                            bw.Write(bones[j].Data.Position.Z);
                            bw.Write(1.0f);

                            bw.Write(bones[j].Data.Rotation.X / 360);
                            bw.Write(bones[j].Data.Rotation.Y / 360);
                            bw.Write(bones[j].Data.Rotation.Z / 360);
                            bw.Write(0.0f);
                            bw.Write(name_list_return);

                            bw.Write(bones[j].Data.ModifierValue1);
                            bw.Write(bones[j].Data.TargetID);
                            bw.Write(bones[j].Data.TargetIndex);
                            bw.Write(bones[j].Data.ModifierValue2);
                            bw.Write(bones[j].Data.TargetIndex);

                        }
                        if (bones[j].Data.Bonetype.Category == BoneTypeCategory.ConstRotationModifier)
                        {
                            bw.Write(bones[j].Data.Header[0]);
                            bw.Write(bones[j].Data.Header[1]);
                            bw.Write(bones[j].Data.Header[2]);
                            bw.Write(bones[j].Data.Header[3]);

                            bw.Write(bones[j].Data.Position.X);
                            bw.Write(bones[j].Data.Position.Y);
                            bw.Write(bones[j].Data.Position.Z);
                            bw.Write(1.0f);

                            bw.Write(bones[j].Data.Rotation.X / 360);
                            bw.Write(bones[j].Data.Rotation.Y / 360);
                            bw.Write(bones[j].Data.Rotation.Z / 360);
                            bw.Write(0.0f);
                            bw.Write(name_list_return);

                            bw.Write(bones[j].Data.TargetID);
                            

                        }

                        if (bones[j].Data.Bonetype.Category == BoneTypeCategory.ConstSlerpModifier)
                        {
                            bw.Write(bones[j].Data.Header[0]);
                            bw.Write(bones[j].Data.Header[1]);
                            bw.Write(bones[j].Data.Header[2]);
                            bw.Write(bones[j].Data.Header[3]);

                            bw.Write(bones[j].Data.Position.X);
                            bw.Write(bones[j].Data.Position.Y);
                            bw.Write(bones[j].Data.Position.Z);
                            bw.Write(1.0f);

                            bw.Write(bones[j].Data.Rotation.X / 360);
                            bw.Write(bones[j].Data.Rotation.Y / 360);
                            bw.Write(bones[j].Data.Rotation.Z / 360);
                            bw.Write(0.0f);
                            bw.Write(name_list_return);

                            bw.Write(bones[j].Data.TargetID);

                        }

                        if (bones[j].Data.Bonetype.Category == BoneTypeCategory.WeaponModifier1)
                        {
                            bw.Write(bones[j].Data.Header[0]);
                            bw.Write(bones[j].Data.Header[1]);
                            bw.Write(bones[j].Data.Header[2]);
                            bw.Write(bones[j].Data.Header[3]);

                            bw.Write(bones[j].Data.Position.X);
                            bw.Write(bones[j].Data.Position.Y);
                            bw.Write(bones[j].Data.Position.Z);
                            bw.Write(1.0f);

                            bw.Write(bones[j].Data.Rotation.X / 360);
                            bw.Write(bones[j].Data.Rotation.Y / 360);
                            bw.Write(bones[j].Data.Rotation.Z / 360);
                            bw.Write(0.0f);
                            bw.Write(name_list_return);

                            bw.Write(bones[j].Data.ModifierValue1 / 360);
                            bw.Write(bones[j].Data.ModifierValue2 / 360);

                        }

                        if (bones[j].Data.Bonetype.Category == BoneTypeCategory.WeaponModifier2)
                        {
                            bw.Write(bones[j].Data.Header[0]);
                            bw.Write(bones[j].Data.Header[1]);
                            bw.Write(bones[j].Data.Header[2]);
                            bw.Write(bones[j].Data.Header[3]);

                            bw.Write(bones[j].Data.Position.X);
                            bw.Write(bones[j].Data.Position.Y);
                            bw.Write(bones[j].Data.Position.Z);
                            bw.Write(1.0f);

                            bw.Write(bones[j].Data.Rotation.X / 360);
                            bw.Write(bones[j].Data.Rotation.Y / 360);
                            bw.Write(bones[j].Data.Rotation.Z / 360);
                            bw.Write(0.0f);
                            bw.Write(name_list_return);

                            bw.Write(bones[j].Data.ModifierValue1 / 360);
                            bw.Write(bones[j].Data.TargetID);

                        }

                        if (bones[j].Data.Bonetype.Category == BoneTypeCategory.Standard || bones[j].Data.Bonetype.Category == BoneTypeCategory.Collision || bones[j].Data.Bonetype.Category == BoneTypeCategory.Modifier)
                        {
                            bw.Write(bones[j].Data.Header[0]);
                            bw.Write(bones[j].Data.Header[1]);
                            bw.Write(bones[j].Data.Header[2]);
                            bw.Write(bones[j].Data.Header[3]);

                            bw.Write(bones[j].Data.Position.X);
                            bw.Write(bones[j].Data.Position.Y);
                            bw.Write(bones[j].Data.Position.Z);
                            bw.Write(1.0f);

                            bw.Write(bones[j].Data.Rotation.X / 360);
                            bw.Write(bones[j].Data.Rotation.Y / 360);
                            bw.Write(bones[j].Data.Rotation.Z / 360);
                            bw.Write(0.0f);
                            bw.Write(name_list_return);


                            bw.Write(bones[j].Data.Scale.X);
                            bw.Write(bones[j].Data.Scale.Y);
                            bw.Write(bones[j].Data.Scale.Z);
                        };

                        if (bones[j].Data.Bonetype.Category == BoneTypeCategory.Scissor)
                        {
                            bw.Write(bones[j].Data.Header[0]);
                            bw.Write(bones[j].Data.Header[1]);
                            bw.Write(bones[j].Data.Header[2]);
                            bw.Write(bones[j].Data.Header[3]);

                            bw.Write(bones[j].Data.Position.X);
                            bw.Write(bones[j].Data.Position.Y);
                            bw.Write(bones[j].Data.Position.Z);
                            bw.Write(1.0f);

                            bw.Write(bones[j].Data.Rotation.X / 360);
                            bw.Write(bones[j].Data.Rotation.Y / 360);
                            bw.Write(bones[j].Data.Rotation.Z / 360);
                            bw.Write(0.0f);
                            bw.Write(name_list_return);

                            bw.Write(bones[j].Data.ModifierValue1 / 360);
                            bw.Write(bones[j].Data.ModifierValue2);
                            bw.Write(bones[j].Data.ModifierAxis);

                        }
                      
                        bw.BaseStream.Position = block_return + nmd_type.BoneTypeOffset1;
                        bw.Write((byte)bones[j].Data.Bonetype.TypeValue.Item1);

                        bw.BaseStream.Position = block_return + nmd_type.BoneTypeOffset2;
                        bw.Write((byte)bones[j].Data.Bonetype.TypeValue.Item2);

                        bw.Write(bones[j].Data.ParentId);
                        bw.Write(bones[j].Data.BoneId);

                        bw.BaseStream.Position = name_list_return;
                        //List<byte> name_byte_list = new();
                        byte[] name_bytes = Encoding.UTF8.GetBytes(bones[j].Data.Name);

                        for (int m = 0; m < name_bytes.Length; m++)
                        {
                            name_bytes[m] += 0x40;
                            bw.Write(name_bytes[m]);
                        }
                        bw.Write((byte)0x40);
                        name_list_return = bw.BaseStream.Position;
                        block_return += (short)nmd_type.BlockLength;
                        bw.BaseStream.Position = block_return;

                    }
                    bw.BaseStream.Position = name_list_offset;
                    bw.Write(name_list_start);
                }
                return $"Successfully saved {Path.GetFileName(export_path)}.";

            }
            catch
            {
                //File.Delete(export_path);
                return $"An error occurred when attempting to save {Path.GetFileName(export_path)}.";

            }
        }
    }
}

