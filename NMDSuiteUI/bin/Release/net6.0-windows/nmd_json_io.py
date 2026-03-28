bl_info = {
    "name": "NMD JSON Importer/Exporter",
    "blender": (3, 00, 0),
    "category": "Import-Export",
    "author": "Antigravity, LogzatioStudios",
    "version": (1, 2, 0),
    "description": "Imports/exports NMD JSON files for use in NMDSuite."
}

import bpy
import json
import math
import os
from mathutils import Matrix, Euler, Vector, Quaternion
from bpy_extras.io_utils import ExportHelper, ImportHelper

def get_nmd_rel_pos(self):
    obj = next((o for o in bpy.context.scene.objects if o.data == self.id_data), None)
    if not obj: return (0.0, 0.0, 0.0)
    
    is_edit_mode = (bpy.context.mode == 'EDIT_ARMATURE' and bpy.context.active_object == obj)
    if is_edit_mode:
        transform_bone = obj.data.edit_bones.get(self.name)
        if not transform_bone: return (0.0, 0.0, 0.0)
        matrix_current = transform_bone.matrix
        parent = transform_bone.parent
        matrix_parent = parent.matrix if parent else None
    else:
        transform_bone = self
        matrix_current = transform_bone.matrix_local
        parent = transform_bone.parent
        matrix_parent = parent.matrix_local if parent else None
        
    rel_matrix = matrix_parent.inverted() @ matrix_current if matrix_parent else matrix_current
    loc, _, _ = rel_matrix.decompose()
    return (loc.x / 10.0, loc.y / 10.0, loc.z / 10.0)

def set_nmd_rel_pos(self, value):
    obj = next((o for o in bpy.context.scene.objects if o.data == self.id_data), None)
    if not obj: return

    original_mode = bpy.context.mode
    original_active = bpy.context.active_object
    
    if original_mode != 'EDIT_ARMATURE' or original_active != obj:
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.mode_set(mode='EDIT')
        
    eb = obj.data.edit_bones.get(self.name)
    if eb:
        parent = eb.parent
        matrix_parent = parent.matrix if parent else None
        rel_matrix = matrix_parent.inverted() @ eb.matrix if matrix_parent else eb.matrix
        _, rot_quat, scale = rel_matrix.decompose()
        
        new_loc = Vector((value[0] * 10.0, value[1] * 10.0, value[2] * 10.0))
        new_rel_mat = Matrix.LocRotScale(new_loc, rot_quat, scale)
        abs_mat = matrix_parent @ new_rel_mat if matrix_parent else new_rel_mat
        
        length = eb.length
        eb.matrix = abs_mat
        eb.tail = eb.head + (abs_mat.to_3x3() @ Vector((0, 1, 0)) * length)

    if original_mode != 'EDIT_ARMATURE' or original_active != obj:
        if original_active:
            bpy.context.view_layer.objects.active = original_active
        bpy.ops.object.mode_set(mode=original_mode)

def get_nmd_rel_rot(self):
    obj = next((o for o in bpy.context.scene.objects if o.data == self.id_data), None)
    if not obj: return (0.0, 0.0, 0.0)
    
    is_edit_mode = (bpy.context.mode == 'EDIT_ARMATURE' and bpy.context.active_object == obj)
    if is_edit_mode:
        transform_bone = obj.data.edit_bones.get(self.name)
        if not transform_bone: return (0.0, 0.0, 0.0)
        matrix_current = transform_bone.matrix
        parent = transform_bone.parent
        matrix_parent = parent.matrix if parent else None
    else:
        transform_bone = self
        matrix_current = transform_bone.matrix_local
        parent = transform_bone.parent
        matrix_parent = parent.matrix_local if parent else None
        
    rel_matrix = matrix_parent.inverted() @ matrix_current if matrix_parent else matrix_current
    _, rot_quat, _ = rel_matrix.decompose()
    rot_euler = rot_quat.to_euler('XYZ')
    
    # if not parent or self.name in ["MUNE1", "KOSHI"]:
    #     rot_euler.z -= math.radians(90)
        
    return (math.degrees(rot_euler.x), math.degrees(rot_euler.y), math.degrees(rot_euler.z))

def set_nmd_rel_rot(self, value):
    obj = next((o for o in bpy.context.scene.objects if o.data == self.id_data), None)
    if not obj: return
    
    original_mode = bpy.context.mode
    original_active = bpy.context.active_object
    
    if original_mode != 'EDIT_ARMATURE' or original_active != obj:
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.mode_set(mode='EDIT')
        
    eb = obj.data.edit_bones.get(self.name)
    if eb:
        parent = eb.parent
        matrix_parent = parent.matrix if parent else None
        rel_matrix = matrix_parent.inverted() @ eb.matrix if matrix_parent else eb.matrix
        loc, _, scale = rel_matrix.decompose()
        
        rx = math.radians(value[0])
        ry = math.radians(value[1])
        rz = math.radians(value[2])
        if not parent or eb.name in ["MUNE1", "KOSHI"]:
            rz += math.radians(90)
            
        new_rot_euler = Euler((rx, ry, rz), 'XYZ')
        new_rel_mat = Matrix.LocRotScale(loc, new_rot_euler.to_quaternion(), scale)
        
        abs_mat = matrix_parent @ new_rel_mat if matrix_parent else new_rel_mat
        
        length = eb.length
        eb.matrix = abs_mat
        eb.tail = eb.head + (abs_mat.to_3x3() @ Vector((0, 1, 0)) * length)
        
    if original_mode != 'EDIT_ARMATURE' or original_active != obj:
        if original_active:
            bpy.context.view_layer.objects.active = original_active
        bpy.ops.object.mode_set(mode=original_mode)

def get_filtered_bones_search(self, context, edit_text):
    bones = []
    if self.armature and self.armature.type == 'ARMATURE':
        for bone in self.armature.data.bones:
            if bone.name.endswith('__group') or bone.name.endswith('_null') or bone.name.endswith('_end'):
                continue
            bones.append(bone.name)
    return bones

def update_import_preset(self, context):
    """Updates offsets based on the selected preset."""
    if self.import_preset == 'Equipment':
        self.r_offset = (0, 90, 0)
        self.l_offset = (0, 0, 11.5)
    elif self.import_preset == 'Weapon':
        self.r_offset = (90, 0, 0)
        self.l_offset = (0, 0, 0)

class BoneImportProperties(bpy.types.PropertyGroup):
    import_preset: bpy.props.EnumProperty(
        name="Preset",
        description="Quick-set offsets for different object types",
        items=[
            ('Equipment', "Equipment", "Apply standard character offsets (Y-90, Z-11.5)"),
            ('Weapon', "Weapon", "Apply weapon offsets")
        ],
        default='Equipment',
        update=update_import_preset
    )
    r_offset: bpy.props.FloatVectorProperty(name="", default=[0, 90, 0])
    l_offset: bpy.props.FloatVectorProperty(name="", default=[0, 0, 11.5])
    bone_length: bpy.props.FloatProperty(
        name="Bone Length Multiplier",
        description="Override default length scale applied to all imported bones",
        default=0.01,
        min=0.01,
        max=10
    )

class SwingGravityProperties(bpy.types.PropertyGroup):
    x: bpy.props.IntProperty(name="X", default=0)
    y: bpy.props.IntProperty(name="Y", default=0)

class SwingConstraintsProperties(bpy.types.PropertyGroup):
    pos_x: bpy.props.IntProperty(name="+X", default=0)
    neg_x: bpy.props.IntProperty(name="-X", default=0)
    pos_y: bpy.props.IntProperty(name="+Y", default=0)
    neg_y: bpy.props.IntProperty(name="-Y", default=0)

class SwingInfluenceProperties(bpy.types.PropertyGroup):
    x: bpy.props.IntProperty(name="X", default=100)
    y: bpy.props.IntProperty(name="Y", default=100)
    z: bpy.props.IntProperty(name="Z", default=100)

def get_all_bones_search_or_none(self, context, edit_text):
    bones = ["None"]
    armature = context.active_object or getattr(context, 'object', None)
    if armature and armature.type == 'ARMATURE':
        bones.extend(b.name for b in armature.data.bones)
    return bones

bone_type_items = [
    ("none", "Unknown", "", 0),
    ("standard", "Standard", "", 1),
    ("weapon", "Weapon", "", 2),
    ("finger", "Finger", "", 3),
    ("face", "Face", "", 4),
    ("rot", "Rotation", "", 5),
    ("prot", "Parent Rotation", "", 6),
    ("const_slerp", "Const Slerp", "", 7),
    ("const_rot", "Const Rotation", "", 8),
    ("slerp1", "Slerp", "", 9),
    ("slerp2", "Slerp", "", 10),
    ("swing", "Swing", "", 11),
    ("swing2", "Swing 2", "", 12),
    ("swing3", "Swing 3", "", 13),
    ("shit", "Sphere Hit", "", 14),
    ("phit", "P Hit", "", 15),
    ("sciv_junk", "SCIV Junk", "", 16),
    ("randeye", "Soul Edge Eye", "", 17),
    ("randlid", "Soul Edge Eyelid", "", 18),
    ("eyestare", "StareEye", "", 19),
    ("chit", "Cylinder Hit", "", 20),
    ("spin", "Spin", "", 21),
    ("scissor", "Scissor", "", 22),
    ("prog_auo", "Prog Auo", "", 23),
    ("prog", "Prog", "", 24),
    ("offset", "Offset", "", 25),
    ("rot_offset", "Rotation Offset", "", 26),
    ("prot_offset", "Parent Rotation Offset", "", 27),
    ("const_rot_offset", "Const Rotation Offset", "", 28),
    ("slerp_offset", "Slerp Offset", "", 29),
    ("breast", "Breast", "", 30)
]

axis_items = [
    ("0", "X", ""),
    ("1", "Y", ""),
    ("2", "Z", "")
]

def get_collision_bones_search(self, context, edit_text):
    bones = ["None"]
    armature = context.active_object or getattr(context, 'object', None)
    if armature and armature.type == 'ARMATURE':
        bones.extend(b.name for b in armature.data.bones if b.name.endswith(('_shit', '_chit', 'phit')))
    return bones

def get_swing_collision_bones_search(self, context, edit_text):
    bones = ["None"]
    armature = context.active_object or getattr(context, 'object', None)
    if armature and armature.type == 'ARMATURE':
        bones.extend(b.name for b in armature.data.bones if b.name.endswith('_swing'))
    return bones

class SwingParamsProperties(bpy.types.PropertyGroup):
    col_data_0: bpy.props.StringProperty(name="Col 0", default="None", search=get_collision_bones_search)
    col_data_1: bpy.props.StringProperty(name="Col 1", default="None", search=get_collision_bones_search)
    col_data_2: bpy.props.StringProperty(name="Col 2", default="None", search=get_collision_bones_search)
    col_data_3: bpy.props.StringProperty(name="Col 3", default="None", search=get_collision_bones_search)
    swing_col_data_0: bpy.props.StringProperty(name="Swing Col 0", default="None", search=get_swing_collision_bones_search)
    swing_col_data_1: bpy.props.StringProperty(name="Swing Col 1", default="None", search=get_swing_collision_bones_search)
    swing_col_data_2: bpy.props.StringProperty(name="Swing Col 2", default="None", search=get_swing_collision_bones_search)
    swing_col_data_3: bpy.props.StringProperty(name="Swing Col 3", default="None", search=get_swing_collision_bones_search)
    length: bpy.props.FloatProperty(name="Length", default=0.0)
    gravity: bpy.props.PointerProperty(type=SwingGravityProperties)
    dampening: bpy.props.IntProperty(name="Dampening", default=0)
    constraints: bpy.props.PointerProperty(type=SwingConstraintsProperties)
    rigidity: bpy.props.IntProperty(name="Rigidity", default=0)
    flag: bpy.props.IntProperty(name="Flag", default=0)
    val1: bpy.props.IntProperty(name="Val 1", default=0)
    val2: bpy.props.IntProperty(name="Val 2", default=0)
    val3: bpy.props.IntProperty(name="Val 3", default=0)
    val4: bpy.props.IntProperty(name="Val 4", default=0)
    val5: bpy.props.IntProperty(name="Val 5", default=0)
    val6: bpy.props.FloatProperty(name="Val 6", default=0.0)
    val7: bpy.props.IntProperty(name="Val 7", default=0)
    influence: bpy.props.PointerProperty(type=SwingInfluenceProperties)

class BoneExportProperties(bpy.types.PropertyGroup):
    armature: bpy.props.PointerProperty(
        name="Armature",
        type=bpy.types.Object,
        description="Select the armature to export",
        poll=lambda self, obj: obj.type == 'ARMATURE'
    )
    bone_name: bpy.props.StringProperty(
        name="Bone",
        description="Select a bone to preview",
        search=get_filtered_bones_search
    )

class GenerateCollisionShapes(bpy.types.Operator):
    """Generate Nurbs Spheres and Cylinders for collision bones based on panel selection"""
    bl_idname = "object.generate_collision_shapes"
    bl_label = "Generate Collision Shapes"
    bl_options = {'REGISTER', 'UNDO'}

    @classmethod
    def poll(cls, context):
        props = getattr(context.scene, "bone_export_props", None)
        return props and props.armature and props.armature.type == 'ARMATURE'

    def execute(self, context):
        arm_obj = context.scene.bone_export_props.armature
        arm_data = arm_obj.data

        original_mode = context.mode
        if original_mode != 'OBJECT':
            bpy.ops.object.mode_set(mode='OBJECT')

        def create_driver(obj, path, index, data_path, expression="val"):
            fcurve = obj.driver_add(path, index) if index >= 0 else obj.driver_add(path)
            drv = fcurve.driver
            drv.type = 'SCRIPTED'
            drv.expression = expression
            var = drv.variables.new()
            var.name = "val"
            var.type = 'SINGLE_PROP'
            var.targets[0].id_type = 'ARMATURE'
            var.targets[0].id = arm_data
            var.targets[0].data_path = data_path
            
            return drv

        shapes_created = 0
        
        #bpy.ops.outliner.item_activate(deselect_all=True)
        col_collection = bpy.data.collections.new("Collision Shapes")
        context.scene.collection.children.link(col_collection)
        bpy.context.view_layer.active_layer_collection = bpy.context.view_layer.layer_collection.children[-1]
        
        for bone in arm_data.bones:
            is_sphere = bone.name.endswith('_shit')
            is_cylinder = bone.name.endswith('_chit')
            
            if is_sphere or is_cylinder:
                bpy.ops.object.select_all(action='DESELECT')
                
                if is_sphere:
                    bpy.ops.surface.primitive_nurbs_surface_sphere_add(radius=1.0)
                else:
                    bpy.ops.surface.primitive_nurbs_surface_cylinder_add(radius=1.0)
                
                shape_obj = context.active_object
                shape_obj.name = f"{bone.name}"
                shape_obj.show_name = True
                shape_obj.display_type = 'BOUNDS'
                shape_obj.display_bounds_type = 'SPHERE' if is_sphere else 'CYLINDER'
                shape_obj.show_in_front = True
                
                #if is_sphere:
                    #shape_obj.scale[0] = bone.collision_extra1 * 10
                    #shape_obj.scale[1] = bone.collision_extra2 * 10
                    #shape_obj.scale[2] = bone.collision_extra3 * 10
                    #bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
                
                bpy.ops.object.origin_set(type='ORIGIN_GEOMETRY', center='MEDIAN')
                
                #if is_sphere:
                    #copy_trans = shape_obj.constraints.new(type='COPY_TRANSFORMS')
                #if is_cylinder:
                    
                copy_trans = shape_obj.constraints.new(type='COPY_LOCATION')
                    
                copy_trans.target = arm_obj
                copy_trans.subtarget = bone.name
                
                trans_const = shape_obj.constraints.new(type='TRANSFORM')
                trans_const.name = "Scale Transformation"
                trans_const.target = arm_obj
                trans_const.subtarget = bone.name
                trans_const.map_to = 'SCALE'
                
                rot_const = shape_obj.constraints.new(type='TRANSFORM')
                rot_const.name = "Rotation Transformation"
                rot_const.target = arm_obj
                rot_const.subtarget = bone.name
                rot_const.map_to = 'ROTATION'

                copy_rot = shape_obj.constraints.new(type='COPY_ROTATION')
                    
                copy_rot.target = arm_obj
                copy_rot.subtarget = bone.name
                copy_rot.mix_mode = 'BEFORE'
                
                #create_driver(shape_obj, 'constraints["Transformation"].to_min_x_scale', -1, f'bones["{bone.name}"].')
                #create_driver(shape_obj, 'constraints["Transformation"].to_min_y_scale', -1, f'bones["{bone.name}"].header_value2')
                #create_driver(shape_obj, 'constraints["Transformation"].to_min_z_scale', -1, f'bones["{bone.name}"].header_value3', "abs(val)")
                
                if is_sphere:        
                    create_driver(shape_obj, 'constraints["Scale Transformation"].to_min_x_scale', -1, f'bones["{bone.name}"].collision_extra1', "val * 10")
                    create_driver(shape_obj, 'constraints["Scale Transformation"].to_min_y_scale', -1, f'bones["{bone.name}"].collision_extra2', "val * 10")
                    create_driver(shape_obj, 'constraints["Scale Transformation"].to_min_z_scale', -1, f'bones["{bone.name}"].collision_extra3', "val * 10")
                    
                    create_driver(shape_obj, 'constraints["Rotation Transformation"].to_min_x_rot', -1, f'bones["{bone.name}"].header_value1', "(val*360)/57.2957795")
                    create_driver(shape_obj, 'constraints["Rotation Transformation"].to_min_y_rot', -1, f'bones["{bone.name}"].header_value2', "(val*360)/57.2957795")
                    create_driver(shape_obj, 'constraints["Rotation Transformation"].to_min_z_rot', -1, f'bones["{bone.name}"].header_value3', "(val*360)/57.2957795")
                elif is_cylinder:
                    create_driver(shape_obj, 'constraints["Scale Transformation"].to_min_x_scale', -1, f'bones["{bone.name}"].collision_extra1', "val * 2.5")
                    create_driver(shape_obj, 'constraints["Scale Transformation"].to_min_y_scale', -1, f'bones["{bone.name}"].collision_extra1', "val * 2.5")
                    create_driver(shape_obj, 'constraints["Scale Transformation"].to_min_z_scale', -1, f'bones["{bone.name}"].collision_extra2', "val * 10")

                shapes_created += 1

        bpy.ops.object.select_all(action='DESELECT')
        context.view_layer.objects.active = arm_obj
        arm_obj.select_set(True)
        if original_mode != 'OBJECT':
            bpy.ops.object.mode_set(mode=original_mode)
            
        self.report({'INFO'}, f"Generated {shapes_created} collision shapes for {arm_obj.name}")
        return {'FINISHED'}

class ExportBoneTransforms(bpy.types.Operator, ExportHelper):
    """Export Bone metadata to NMD JSON"""
    bl_idname = "export_scene.bone_relative_transforms"
    bl_label = "Export NMD JSON"
    filename_ext = ".json"
    
    filter_glob: bpy.props.StringProperty(
        default="*.json",
        options={'HIDDEN'},
        maxlen=255,
    )

    def execute(self, context):
        props = context.scene.bone_export_props
        obj = props.armature

        if not obj or obj.type != 'ARMATURE':
            self.report({'ERROR'}, "Please select an Armature in the Bone Export panel before exporting")
            return {'CANCELLED'}

        armature = obj.data
        bones_data = []

        bone_list = sorted(list(armature.bones), key=lambda b: b.bone_index)

        for index, bone in enumerate(bone_list):
            if bone.name.endswith('__group') or bone.name.endswith('_null') or bone.name.endswith('_end'):
                continue

            parent = bone.parent
            if bone.name in ["MUNE1", "KOSHI"] or "BUKI_" in bone.name:
                parent_name = "Root"
            elif parent:
                if parent.name == "HAND_L_SCALE__group":
                    parent_name = "TE_L"
                elif parent.name == "HAND_R_SCALE__group":
                    parent_name = "TE_R"
                elif parent.name == "FACE_SCALE__group":
                    parent_name = "ATAMA"
                else:
                    parent_name = parent.name
            else:
                parent_name = None

            # Calculate relative matrix
            if parent:
                rel_matrix = parent.matrix_local.inverted() @ bone.matrix_local
            else:
                rel_matrix = bone.matrix_local

            loc, rot_quat, scale = rel_matrix.decompose()
            rot_euler = rot_quat.to_euler() 

            if not parent or bone.name in ["MUNE1", "KOSHI"]:
                rot_euler.z -= math.radians(90)

            parent_idx = -1
            if not parent or bone.name in ["MUNE1", "KOSHI"] or "BUKI_" in bone.name:
                parent_idx = -1
            elif parent.name == "HAND_L_SCALE__group" and "TE_L" in armature.bones:
                parent_idx = armature.bones["TE_L"].bone_index
            elif parent.name == "HAND_R_SCALE__group" and "TE_R" in armature.bones:
                parent_idx = armature.bones["TE_R"].bone_index
            elif parent.name == "FACE_SCALE__group" and "ATAMA" in armature.bones:
                parent_idx = armature.bones["ATAMA"].bone_index
            elif "__group" in parent.name:
                parent_idx = -1
            else:
                parent_idx = parent.bone_index

            if bone.bone_type in ["shit", "chit", "phit"] or bone.name.endswith(('_shit', '_chit', '_phit')):
                export_scale = {
                    "x": bone.collision_extra1,
                    "y": bone.collision_extra2,
                    "z": bone.collision_extra3
                }
            else:
                export_scale = {
                    "x": 1.0,
                    "y": 1.0,
                    "z": 1.0
                }

            bone_dict = {
                "name": bone.name,
                "id": bone.bone_id,
                "index": bone.bone_index,
                "type": bone.bone_type,
                "parent_name": parent_name,
                "parent_index": parent_idx,
                "header": [bone.header_value1, bone.header_value2, bone.header_value3, bone.header_value4],
                "modifier_params": {
                    "modifier_value1": bone.modifier_value1,
                    "modifier_value2": bone.modifier_value2,
                    "modifier_value3": bone.modifier_value3,
                    "axis": int(bone.modifier_axis),
                    "target_id": bone.target_id,
                    "target_index": bone.target_index,
                    #"collision_extra1": bone.collision_extra1,
                    #"collision_extra2": bone.collision_extra2,
                    #"collision_extra3": bone.collision_extra3
                },
                "position": {
                    "x": round(loc.x / 10.0, 8),
                    "y": round(loc.y / 10.0, 8),
                    "z": round(loc.z / 10.0, 8)
                },
                "rotation": {
                    "x": math.degrees(rot_euler.x),
                    "y": math.degrees(rot_euler.y),
                    "z": math.degrees(rot_euler.z)
                },
                "scale": export_scale
            }
            if "_swing" in bone.name:
                sp = bone.swing_params
                
                def get_bone_idx(b_name):
                    if not b_name or b_name == "None":
                        return 0
                    if b_name in armature.bones:
                        return armature.bones[b_name].bone_index
                    return 0

                bone_dict["swing_params"] = {
                    "collision_data": [
                        get_bone_idx(sp.col_data_0),
                        get_bone_idx(sp.col_data_1),
                        get_bone_idx(sp.col_data_2),
                        get_bone_idx(sp.col_data_3)
                    ],
                    "collision_count": sum(1 for c in [sp.col_data_0, sp.col_data_1, sp.col_data_2, sp.col_data_3] if c and c != "None"),
                    "swing_collision_data": [
                        get_bone_idx(sp.swing_col_data_0),
                        get_bone_idx(sp.swing_col_data_1),
                        get_bone_idx(sp.swing_col_data_2),
                        get_bone_idx(sp.swing_col_data_3)
                    ],
                    "swing_count": sum(1 for c in [sp.swing_col_data_0, sp.swing_col_data_1, sp.swing_col_data_2, sp.swing_col_data_3] if c and c != "None"),
                    "length": sp.length,
                    "gravity": {
                        "x": sp.gravity.x,
                        "y": sp.gravity.y
                    },
                    "dampening": sp.dampening,
                    "constraints": [sp.constraints.pos_x, sp.constraints.neg_x, sp.constraints.pos_y, sp.constraints.neg_y],
                    "rigidity": sp.rigidity,
                    "flag": sp.flag,
                    "val1": sp.val1,
                    "val2": sp.val2,
                    "val3": sp.val3,
                    "val4": sp.val4,
                    "val5": sp.val5,
                    "val6": sp.val6,
                    "val7": sp.val7,
                    "influence":{
                        "x": sp.influence.x,
                        "y": sp.influence.y,
                        "z": sp.influence.z,
                    }
                }
            bones_data.append(bone_dict)

        with open(self.filepath, 'w') as f:
            json.dump(bones_data, f, indent=4)

        self.report({'INFO'}, f"Exported {len(bones_data)} bones to {self.filepath}")
        return {'FINISHED'}

def create_root_bones(context, arm_data):
    if context.scene.bone_import_props.import_preset == 'Equipment':
        mune1 = arm_data.edit_bones.get('MUNE1')
        koshi = arm_data.edit_bones.get('KOSHI')
        hito = arm_data.edit_bones.new("HITO__group")
        hito.head = (0.0,0.0,0.0)
        hito.tail = (0.0,0.0,1.0)
        hito.bone_id = -1
        hito.bone_index = -1
        body = arm_data.edit_bones.new("BODY_SCALE__group")
        body.head = hito.head
        body.tail = hito.tail
        body.matrix = hito.matrix
        body.parent = hito
        body.bone_id = -1
        body.bone_index = -1
        hara = arm_data.edit_bones.new("hara__null")
        hara.head = (0.0,0.0,11.5)
        hara.tail = (0.0,0.0,12.5)
        hara.roll = math.radians(90)
        hara.parent = body
        hara.bone_id = -1
        hara.bone_index = -1
        upper = arm_data.edit_bones.new("UPPER_SCALE__group")
        upper.head = hara.head
        upper.tail = hara.tail
        upper.matrix = hara.matrix
        upper.parent = hara
        upper.bone_id = -1
        upper.bone_index = -1
        lower = arm_data.edit_bones.new("LOWER_SCALE__group")
        lower.head = hara.head
        lower.tail = hara.tail
        lower.matrix = hara.matrix
        lower.parent = hara
        lower.bone_id = -1
        lower.bone_index = -1
        if mune1:
            mune1.parent = upper
        if koshi:
            koshi.parent = lower

        if 'TE_L' in arm_data.edit_bones:
            te_l = arm_data.edit_bones['TE_L']
            hand_l_scale = arm_data.edit_bones.new("HAND_L_SCALE__group")
            hand_l_scale.head = te_l.head
            hand_l_scale.tail = te_l.tail
            hand_l_scale.matrix = te_l.matrix
            hand_l_scale.parent = te_l
            hand_l_scale.bone_id = -1
            hand_l_scale.bone_index = -1
            te_l_list = ['OYA3_L', 'HITO3_L', 'NAKA3_L', 'KUSU3_L', 'KO3_L']
            for b_name in te_l_list:
                b = arm_data.edit_bones.get(b_name)
                if b:
                    b.parent = hand_l_scale

        if 'TE_R' in arm_data.edit_bones:
            te_r = arm_data.edit_bones['TE_R']
            hand_r_scale = arm_data.edit_bones.new("HAND_R_SCALE__group")
            hand_r_scale.head = te_r.head
            hand_r_scale.tail = te_r.tail
            hand_r_scale.matrix = te_r.matrix
            hand_r_scale.parent = te_r
            hand_r_scale.bone_id = -1
            hand_r_scale.bone_index = -1
            te_r_list = ['OYA3_R', 'HITO3_R', 'NAKA3_R', 'KUSU3_R', 'KO3_R']
            for b_name in te_r_list:
                b = arm_data.edit_bones.get(b_name)
                if b:
                    b.parent = hand_r_scale

        if 'ATAMA' in arm_data.edit_bones:
            atama = arm_data.edit_bones['ATAMA']
            face_scale = arm_data.edit_bones.new("FACE_SCALE__group")
            face_scale.head = atama.head
            face_scale.tail = atama.tail
            face_scale.matrix = atama.matrix
            face_scale.parent = atama
            face_scale.bone_id = -1
            face_scale.bone_index = -1
            atama_list = ['KUCHI_UP_L', 'KUCHI_UP_R', 'KUCHI_UP_M', 'AGO', 'CHOBO_R', 'CHOBO_L', 'BIKON_R', 'BIKON_L', 'HOHO_L', 'HOHO_R', 'MABUTA_UN_R', 'MABUTA_UN_L', 'MABUTA_UP_R', 'MABUTA_UP_L', 'MAYUJIRI_R', 'MAYU_R', 'MIKEN_R', 'MIKEN_L', 'MAYU_L', 'MAYUJIRI_L', 'EYE_R', 'EYE_L', 'SHIWA_R', 'SHIWA_L', 'MIMI_L', 'MIMI_R']
            for b_name in atama_list:
                b = arm_data.edit_bones.get(b_name)
                if b:
                    b.parent = face_scale
    elif context.scene.bone_import_props.import_preset == 'Weapon':
        if 'BUKI_1' in arm_data.edit_bones:
            buki1 = arm_data.edit_bones['BUKI_1']
            buki_root = arm_data.edit_bones.new("buki000__group")
            buki_root.head = (0.0,0.0,0.0)
            buki_root.tail = buki1.tail
            buki_root.bone_id = -1
            buki_root.bone_index = -1
            for bone in arm_data.edit_bones:
                if 'BUKI_' in bone.name:
                    bone.parent = buki_root

class ImportBoneTransforms(bpy.types.Operator, ImportHelper):
    """Import Bone Metadata from NMD JSON to create an Armature"""
    bl_idname = "import_scene.bone_relative_transforms"
    bl_label = "Create Armature from NMD JSON"
    filename_ext = ".json"
    
    filter_glob: bpy.props.StringProperty(
        default="*.json",
        options={'HIDDEN'},
        maxlen=255,
    )
    
    def draw(self, context):
        layout = self.layout
        arm_box = layout.box()
        props = context.scene.bone_import_props
        row1 = arm_box.row()
        row2 = arm_box.row()
        
        row1.label(text="Armature Type:", icon='ARMATURE_DATA')
        row2.prop(props, "import_preset", text="")
        
        arm_box.prop(props, "bone_length")
        
        layout.separator()
        
        t_box = layout.box()
        t_box.label(text="Position Offset:", icon='EMPTY_ARROWS')
        t_box.prop(props, "l_offset")
        t_box.label(text="Rotation Offset:", icon='ORIENTATION_GIMBAL')
        t_box.prop(props, "r_offset")
        
    def execute(self, context):
        with open(self.filepath, 'r') as f:
            bones_data = json.load(f)

        if not bones_data:
            self.report({'ERROR'}, "No data found in JSON")
            return {'CANCELLED'}

        arm_data = bpy.data.armatures.new(f"{os.path.splitext(os.path.basename(self.filepath))[0]}")
        obj = bpy.data.objects.new(f"{os.path.splitext(os.path.basename(self.filepath))[0]}", arm_data)
        context.collection.objects.link(obj)
        context.view_layer.objects.active = obj
        obj.select_set(True)

        bpy.ops.object.mode_set(mode='EDIT')

        edit_bones = arm_data.edit_bones
        bone_refs = {}

        for b_data in bones_data:
            name = b_data["name"]
            eb = edit_bones.new(name)
            bone_refs[name] = eb

        for b_data in bones_data:
            name = b_data["name"]
            eb = bone_refs[name]
            p_name = b_data.get("parent_name")
            if p_name and p_name in bone_refs:
                eb.parent = bone_refs[p_name]

        abs_matrices = {}
        
        bones_data_by_name = {bd["name"]: bd for bd in bones_data}

        def calc_matrix(b_name):
            if b_name in abs_matrices:
                return abs_matrices[b_name]
                
            b_data = bones_data_by_name.get(b_name)
            if not b_data:
                return Matrix.Identity(4)
                
            trans = b_data["position"]
            rot = b_data["rotation"]
            scale_data = b_data.get("scale", {"x": 1.0, "y": 1.0, "z": 1.0})
            
            rx = math.radians(rot["x"])
            ry = math.radians(rot["y"])
            rz = math.radians(rot["z"])

            p_name = b_data.get("parent_name")
            if not p_name or b_name in ["MUNE1", "KOSHI"]:
                rz += math.radians(90)
                
            euler = Euler((rx, ry, rz), 'XYZ')
            mat_rot = euler.to_matrix().to_4x4()
            mat_loc = Matrix.Translation((trans["x"] * 10.0, trans["y"] * 10.0, trans["z"] * 10.0))
            
            if b_data.get("type") in ["shit", "chit", "phit"] or b_name.endswith(('_shit', '_chit', '_phit')):
                mat_scale = Matrix.Diagonal((1.0, 1.0, 1.0, 1.0))
            else:
                mat_scale = Matrix.Diagonal((scale_data.get("x", 1.0), scale_data.get("y", 1.0), scale_data.get("z", 1.0), 1.0))
            
            rel_mat = mat_loc @ mat_rot @ mat_scale
            
            if p_name and p_name in bone_refs:
                parent_abs = calc_matrix(p_name)
                abs_mat = parent_abs @ rel_mat
            else:
                abs_mat = rel_mat
                
            abs_matrices[b_name] = abs_mat
            return abs_mat

        for name, eb in bone_refs.items():
            mat = calc_matrix(name)
            eb.matrix = mat
            
            base_len = context.scene.bone_import_props.bone_length
            eb.tail = eb.head + (mat.to_3x3() @ Vector((0, 1, 0)) * (base_len * 10.0))
        

        bpy.ops.object.mode_set(mode='OBJECT')
        
        import_props = context.scene.bone_import_props
        obj.rotation_euler = (math.radians(import_props.r_offset[0]), math.radians(import_props.r_offset[1]), math.radians(import_props.r_offset[2]))
        obj.location = (import_props.l_offset[0],import_props.l_offset[1],import_props.l_offset[2])
        
        bpy.ops.object.select_all(action='DESELECT')
        obj.select_set(True)
        context.view_layer.objects.active = obj
        bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
        
        bpy.ops.object.mode_set(mode='EDIT')
        create_root_bones(context, arm_data)
        bpy.ops.object.mode_set(mode='OBJECT')

        bones_data_by_index = {bd.get("index", -1): bd for bd in bones_data}

        for b_data in bones_data:
            name = b_data["name"]
            if name in arm_data.bones:
                b = arm_data.bones[name]
                if "id" in b_data: b.bone_id = b_data["id"]
                if "index" in b_data: b.bone_index = b_data["index"]
                if "type" in b_data: b.bone_type = b_data["type"]
                if "header" in b_data and len(b_data["header"]) >= 4:
                    b.header_value1 = b_data["header"][0]
                    b.header_value2 = b_data["header"][1]
                    b.header_value3 = b_data["header"][2]
                    b.header_value4 = b_data["header"][3]
                if "modifier_params" in b_data:
                    mp = b_data["modifier_params"]
                    b.modifier_value1 = mp.get("modifier_value1", 0.0)
                    b.modifier_value2 = mp.get("modifier_value2", 0.0)
                    b.modifier_value3 = mp.get("modifier_value3", 0.0)
                    axis_val = mp.get("axis", 0)
                    b.modifier_axis = str(axis_val) if str(axis_val) in ["0", "1", "2"] else "0"
                    b.target_id = mp.get("target_id", 0)
                    b.target_index = mp.get("target_index", 0)
                    #b.collision_extra1 = mp.get("collision_extra1", 0.0)
                    #b.collision_extra2 = mp.get("collision_extra2", 0.0)
                    #b.collision_extra3 = mp.get("collision_extra3", 0.0)

                if b.bone_type in ["shit", "chit", "phit"] or b.name.endswith(('_shit', '_chit', '_phit')):
                    if "scale" in b_data:
                        scale_data = b_data["scale"]
                        b.collision_extra1 = scale_data.get("x", 0.0)
                        b.collision_extra2 = scale_data.get("y", 0.0)
                        b.collision_extra3 = scale_data.get("z", 0.0)
                
                if "swing_params" in b_data and "_swing" in name:
                    sp_data = b_data["swing_params"]
                    sp = b.swing_params
                    def resolve_sp_bone(idx):
                        if idx == 0: return "None"
                        return bones_data_by_index[idx]["name"] if idx in bones_data_by_index else "None"
                    if "collision_data" in sp_data and len(sp_data["collision_data"]) >= 4:
                        sp.col_data_0 = resolve_sp_bone(sp_data["collision_data"][0])
                        sp.col_data_1 = resolve_sp_bone(sp_data["collision_data"][1])
                        sp.col_data_2 = resolve_sp_bone(sp_data["collision_data"][2])
                        sp.col_data_3 = resolve_sp_bone(sp_data["collision_data"][3])
                    if "swing_collision_data" in sp_data and len(sp_data["swing_collision_data"]) >= 4:
                        sp.swing_col_data_0 = resolve_sp_bone(sp_data["swing_collision_data"][0])
                        sp.swing_col_data_1 = resolve_sp_bone(sp_data["swing_collision_data"][1])
                        sp.swing_col_data_2 = resolve_sp_bone(sp_data["swing_collision_data"][2])
                        sp.swing_col_data_3 = resolve_sp_bone(sp_data["swing_collision_data"][3])
                    sp.length = sp_data.get("length", 0.0)
                    if "gravity" in sp_data:
                        sp.gravity.x = sp_data["gravity"].get("x", 0)
                        sp.gravity.y = sp_data["gravity"].get("y", 0)
                    sp.dampening = sp_data.get("dampening", 0)
                    if "constraints" in sp_data:
                        sp.constraints.pos_x = sp_data["constraints"][0]
                        sp.constraints.neg_x = sp_data["constraints"][1]
                        sp.constraints.pos_y = sp_data["constraints"][2]
                        sp.constraints.neg_y = sp_data["constraints"][3]
                    sp.rigidity = sp_data.get("rigidity", 0.0)
                    sp.flag = sp_data.get("flag", 0)
                    sp.val1 = sp_data.get("val1", 0)
                    sp.val2 = sp_data.get("val2", 0)
                    sp.val3 = sp_data.get("val3", 0)
                    sp.val4 = sp_data.get("val4", 0)
                    sp.val5 = sp_data.get("val5", 0)
                    sp.val6 = sp_data.get("val6", 0.0)
                    sp.val7 = sp_data.get("val7", 0)
                    if "influence" in sp_data:
                        sp.influence.x = sp_data["influence"].get("x", 100.0)
                        sp.influence.y = sp_data["influence"].get("y", 100.0)
                        sp.influence.z = sp_data["influence"].get("z", 100.0)

        self.report({'INFO'}, f"Imported {len(bones_data)} bones from JSON")
        return {'FINISHED'}

class GenerateRootBones(bpy.types.Operator):
    """Generate Root Bones explicitly for the active Armature based on the current Import Preset"""
    bl_idname = "object.generate_root_bones"
    bl_label = "Generate Root Bones"
    bl_options = {'REGISTER', 'UNDO'}

    @classmethod
    def poll(cls, context):
        return context.active_object and context.active_object.type == 'ARMATURE'

    def execute(self, context):
        arm_obj = context.active_object
        arm_data = arm_obj.data

        original_mode = context.mode
        if original_mode != 'EDIT':
            bpy.ops.object.mode_set(mode='EDIT')

        create_root_bones(context, arm_data)

        if original_mode != 'EDIT':
            bpy.ops.object.mode_set(mode=original_mode)
            
        self.report({'INFO'}, f"Generated Root bones for {arm_obj.name}")
        return {'FINISHED'}

class ImportBoneMetadata(bpy.types.Operator, ImportHelper):
    """Import Bone Metadata from NMD JSON to the selected Armature"""
    bl_idname = "import_scene.bone_metadata"
    bl_label = "Import Metadata from JSON"
    filename_ext = ".json"
    
    filter_glob: bpy.props.StringProperty(
        default="*.json",
        options={'HIDDEN'},
        maxlen=255,
    )

    def execute(self, context):
        props = context.scene.bone_export_props
        if not props.armature or props.armature.type != 'ARMATURE':
            self.report({'ERROR'}, "No valid Armature selected in Bone Export panel")
            return {'CANCELLED'}

        with open(self.filepath, 'r') as f:
            bones_data = json.load(f)

        if not bones_data:
            self.report({'ERROR'}, "No data found in JSON")
            return {'CANCELLED'}

        armature = props.armature.data

        for b_data in bones_data:
            name = b_data["name"]
            if name in armature.bones:
                b = armature.bones[name]
                if "id" in b_data: b.bone_id = b_data["id"]
                if "index" in b_data: b.bone_index = b_data["index"]
                if "type" in b_data: b.bone_type = b_data["type"]
                if "header" in b_data and len(b_data["header"]) >= 4:
                    b.header_value1 = b_data["header"][0]
                    b.header_value2 = b_data["header"][1]
                    b.header_value3 = b_data["header"][2]
                    b.header_value4 = b_data["header"][3]
                if "modifier_params" in b_data:
                    mp = b_data["modifier_params"]
                    b.modifier_value1 = mp.get("modifier_value1", 0.0)
                    b.modifier_value2 = mp.get("modifier_value2", 0.0)
                    b.modifier_value3 = mp.get("modifier_value3", 0.0)
                    axis_val = mp.get("axis", 0)
                    b.modifier_axis = str(axis_val) if str(axis_val) in ["0", "1", "2"] else "0"
                    b.target_id = mp.get("target_id", 0)
                    b.target_index = mp.get("target_index", 0)
                    #b.collision_extra1 = mp.get("collision_extra1", 0.0)
                    #b.collision_extra2 = mp.get("collision_extra2", 0.0)
                    #b.collision_extra3 = mp.get("collision_extra3", 0.0)
                
                if "swing_params" in b_data and "_swing" in name:
                    sp_data = b_data["swing_params"]
                    sp = b.swing_params
                    def resolve_sp_bone(idx):
                        if idx == 0: return "None"
                        for bd in bones_data:
                            if bd.get("index", -1) == idx: return bd["name"]
                        return "None"
                    if "collision_data" in sp_data and len(sp_data["collision_data"]) >= 4:
                        sp.col_data_0 = resolve_sp_bone(sp_data["collision_data"][0])
                        sp.col_data_1 = resolve_sp_bone(sp_data["collision_data"][1])
                        sp.col_data_2 = resolve_sp_bone(sp_data["collision_data"][2])
                        sp.col_data_3 = resolve_sp_bone(sp_data["collision_data"][3])
                    if "swing_collision_data" in sp_data and len(sp_data["swing_collision_data"]) >= 4:
                        sp.swing_col_data_0 = resolve_sp_bone(sp_data["swing_collision_data"][0])
                        sp.swing_col_data_1 = resolve_sp_bone(sp_data["swing_collision_data"][1])
                        sp.swing_col_data_2 = resolve_sp_bone(sp_data["swing_collision_data"][2])
                        sp.swing_col_data_3 = resolve_sp_bone(sp_data["swing_collision_data"][3])
                    sp.length = sp_data.get("length", 0.0)
                    if "gravity" in sp_data:
                        sp.gravity.x = sp_data["gravity"].get("x", 0)
                        sp.gravity.y = sp_data["gravity"].get("y", 0)
                    sp.dampening = sp_data.get("dampening", 0.0)
                    if "constraints" in sp_data:
                        sp.constraints.pos_x = sp_data["constraints"][0]
                        sp.constraints.neg_x = sp_data["constraints"][1]
                        sp.constraints.pos_y = sp_data["constraints"][2]
                        sp.constraints.neg_y = sp_data["constraints"][3]
                    sp.rigidity = sp_data.get("rigidity", 0)
                    sp.flag = sp_data.get("flag", 0)
                    sp.val1 = sp_data.get("val1", 0)
                    sp.val2 = sp_data.get("val2", 0)
                    sp.val3 = sp_data.get("val3", 0)
                    sp.val4 = sp_data.get("val4", 0)
                    sp.val5 = sp_data.get("val5", 0)
                    sp.val6 = sp_data.get("val6", 0.0)
                    sp.val7 = sp_data.get("val7", 0)
                    if "influence" in sp_data:
                        sp.influence.x = sp_data["influence"].get("x", 100)
                        sp.influence.y = sp_data["influence"].get("y", 100)
                        sp.influence.z = sp_data["influence"].get("z", 100)

        self.report({'INFO'}, f"Imported metadata to {len(bones_data)} bones from JSON")
        return {'FINISHED'}


class VIEW3D_PT_bone_transforms(bpy.types.Panel):
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "NMD JSON Import/Export"
    bl_label = "NMD JSON Export"

    def draw(self, context):
        layout = self.layout
        props = context.scene.bone_export_props

        layout.prop(props, "armature", icon='ARMATURE_DATA')
        
        if props.armature and props.armature.type == 'ARMATURE':
            layout.prop(props, "bone_name", text="Bone", icon='BONE_DATA')
            
            if props.bone_name and props.bone_name != "NONE" and props.bone_name in props.armature.data.bones:
                bone = props.armature.data.bones[props.bone_name]
                bone_id = bone.bone_id
                bone_index = bone.bone_index
                parent = bone.parent
                parent_index = bone.parent.bone_index if parent else -1
                
                if parent:
                    rel_matrix = parent.matrix_local.inverted() @ bone.matrix_local
                else:
                    rel_matrix = bone.matrix_local
                
                loc, rot_quat, _ = rel_matrix.decompose()
                rot_euler = rot_quat.to_euler()
                
                if not parent or bone.name in ["MUNE1", "KOSHI"]:
                    rot_euler.z -= math.radians(90)
                
                parent_display = "Root" if bone.name in ["MUNE1", "KOSHI"] else (parent.name if parent else "None")
                
                box = layout.box()
                box.label(text="Data Preview:")
                box.label(text=f"Bone ID: {bone_id}", icon='BONE_DATA')
                box.label(text=f"Bone Index: {bone_index}", icon='MOD_ARRAY')
                box.label(text=f"Parent: {parent_display}",  icon='CONSTRAINT_BONE')
                box.label(text=f"Parent Index: {parent_index}", icon='MOD_ARRAY')
                
                col = box.column()
                col.prop(bone, "nmd_rel_pos", text="Relative Position")
                col.prop(bone, "nmd_rel_rot", text="Relative Rotation")
                
        layout.separator()
        layout.operator(ExportBoneTransforms.bl_idname, text="Export NMD JSON", icon='EXPORT')
        if props.armature and props.armature.type == 'ARMATURE':
            layout.operator(ImportBoneMetadata.bl_idname, text="Import Metadata from NMD JSON", icon='OPTIONS')
            layout.separator()
            layout.operator(GenerateCollisionShapes.bl_idname, text="Generate Collision Shapes", icon='MESH_ICOSPHERE')

class VIEW3D_PT_bone_transforms_import(bpy.types.Panel):
    bl_space_type = 'VIEW_3D'
    bl_region_type = 'UI'
    bl_category = "NMD JSON Import/Export"
    bl_label = "NMD JSON Import"

    def draw(self, context):
        layout = self.layout
        layout.operator(ImportBoneTransforms.bl_idname, text="Create Armature from NMD JSON", icon='ARMATURE_DATA')
        layout.separator()
        layout.operator(GenerateRootBones.bl_idname, text="Generate Root Bones (Post-Import)", icon='BONE_DATA')

class BONE_PT_swing_properties(bpy.types.Panel):
    bl_space_type = 'PROPERTIES'
    bl_region_type = 'WINDOW'
    bl_context = "bone"
    bl_label = "Bone Metadata"

    @classmethod
    def poll(cls, context):
        return context.active_bone is not None or context.bone is not None

    def draw(self, context):
        layout = self.layout
        is_edit_mode = (context.mode == 'EDIT_ARMATURE')
        
        if is_edit_mode:
            transform_bone = context.active_bone
            if not transform_bone or not context.active_object: return
            prop_bone = context.active_object.data.bones.get(transform_bone.name)
            if not prop_bone:
                layout.label(text="Toggle Edit Mode once to edit properties for this new bone.", icon='INFO')
                return
        else:
            transform_bone = context.bone
            prop_bone = context.bone
            
        if not transform_bone or not prop_bone: return
            
        layout.prop(prop_bone, "bone_id")
        layout.prop(prop_bone, "bone_index")
        
        box = layout.box()
        box.label(text="Transform Preview:")
        col = box.column()
        col.prop(prop_bone, "nmd_rel_pos", text="Relative Position")
        col.prop(prop_bone, "nmd_rel_rot", text="Relative Rotation")
        
        layout.prop(prop_bone, "bone_type", text="Type")

        box = layout.box()
        box.label(text="Header Floats:")
        row = box.row()
        row.prop(prop_bone, "header_value1", text="H1")
        row.prop(prop_bone, "header_value2", text="H2")
        row.prop(prop_bone, "header_value3", text="H3")
        row.prop(prop_bone, "header_value4", text="H4")

        btype = prop_bone.bone_type
        if btype in ["rot","rot_offset"]:
            box = layout.box()
            box.prop(prop_bone, "modifier_value1", text="Modifier Value")
            box.prop(prop_bone, "modifier_axis", text="Axis")
            box.prop(prop_bone, "target_id", text="Target Bone ID")
        elif btype in ["slerp1", "slerp2", "slerp_offset"]:
            box = layout.box()
            box.prop(prop_bone, "modifier_value1", text="Modifier Value 1")
            box.prop(prop_bone, "target_index", text="Target Bone Index")
            box.prop(prop_bone, "target_id", text="Target Bone ID")
            box.prop(prop_bone, "modifier_value2", text="Modifier Value 2")
        elif btype in ["const_slerp"]:
            box = layout.box()
            box.prop(prop_bone, "target_index", text="Bone Index")
        elif btype in ["const_rot","const_rot_offset"]:
            box = layout.box()
            box.prop(prop_bone, "modifier_value1", text="Modifier Value")
            box.prop(prop_bone, "target_id", text="Target Bone ID")
            box.prop(prop_bone, "target_index", text="Target Bone Index")
        elif btype == "scissor":
            box = layout.box()
            box.prop(prop_bone, "modifier_value1", text="Value")
            box.prop(prop_bone, "modifier_value2", text="Value 2")
            box.prop(prop_bone, "modifier_value3", text="Value 3")
        elif btype == "randlid":
            box = layout.box()
            box.prop(prop_bone, "modifier_value1", text="Value")
            box.prop(prop_bone, "target_index", text="Target Bone Index")
        elif btype in ["randeye", "eyestare"]:
            box = layout.box()
            box.prop(prop_bone, "modifier_value1", text="Value 1")
            box.prop(prop_bone, "modifier_value2", text="Value 2")
        elif btype in ["shit", "chit", "phit"]:
            box = layout.box()
            box.prop(prop_bone, "collision_extra1", text="Extra Value 1")
            box.prop(prop_bone, "collision_extra2", text="Extra Value 2")
            box.prop(prop_bone, "collision_extra3", text="Extra Value 3")
        elif btype in ["swing", "swing2", "swing3", "breast"]:
            sp = prop_bone.swing_params
            box = layout.box(); box.label(text="Collision Data:")
            row = box.row(); row.prop(sp, "col_data_0", text="")
            sub = row.row(); sub.enabled = sp.col_data_0 != "None"
            sub.prop(sp, "col_data_1", text=""); sub.prop(sp, "col_data_2", text=""); sub.prop(sp, "col_data_3", text="")
            box = layout.box(); box.label(text="Swing Collision Data:")
            row = box.row(); row.prop(sp, "swing_col_data_0", text="")
            sub2 = row.row(); sub2.enabled = sp.swing_col_data_0 != "None"
            sub2.prop(sp, "swing_col_data_1", text=""); sub2.prop(sp, "swing_col_data_2", text=""); sub2.prop(sp, "swing_col_data_3", text="")
            layout.prop(sp, "length")
            box = layout.box(); box.label(text="Gravity:"); row = box.row(); row.prop(sp.gravity, "x"); row.prop(sp.gravity, "y")
            layout.prop(sp, "dampening")
            box = layout.box(); box.label(text="Constraints:"); row1 = box.row(); row1.prop(sp.constraints, "pos_x"); row1.prop(sp.constraints, "neg_x"); row2 = box.row(); row2.prop(sp.constraints, "pos_y"); row2.prop(sp.constraints, "neg_y")
            layout.prop(sp, "rigidity"); layout.prop(sp, "flag")
            box = layout.box(); box.label(text="Values:"); row1 = box.row(); row1.prop(sp, "val1"); row1.prop(sp, "val2"); row1.prop(sp, "val3"); row2 = box.row(); row2.prop(sp, "val4"); row2.prop(sp, "val5"); row3 = box.row(); row3.prop(sp, "val6"); row4 = box.row(); row4.prop(sp, "val7")
            box = layout.box(); box.label(text="Influence:"); row = box.row(); row.prop(sp.influence, "x"); row.prop(sp.influence, "y"); row.prop(sp.influence, "z")

classes = (
    SwingGravityProperties,
    SwingConstraintsProperties,
    SwingInfluenceProperties,
    SwingParamsProperties,
    BoneImportProperties,
    BoneExportProperties,
    GenerateCollisionShapes,
    GenerateRootBones,
    ExportBoneTransforms,
    ImportBoneTransforms,
    ImportBoneMetadata,
    VIEW3D_PT_bone_transforms,
    VIEW3D_PT_bone_transforms_import,
    BONE_PT_swing_properties,
)

def update_drivers(self, context):
    try:
        self.id_data.update_tag({'OBJECT', 'DATA'})
    except Exception:
        pass
    for obj in bpy.data.objects:
        if obj.animation_data and obj.animation_data.drivers:
            for d in obj.animation_data.drivers:
                d.driver.expression = d.driver.expression
    context.view_layer.update()

def register():
    for cls in classes:
        bpy.utils.register_class(cls)
    bpy.types.Scene.bone_import_props = bpy.props.PointerProperty(type=BoneImportProperties)    
    bpy.types.Scene.bone_export_props = bpy.props.PointerProperty(type=BoneExportProperties)
    bpy.types.Bone.bone_id = bpy.props.IntProperty(name="Bone ID", default=0)
    bpy.types.Bone.bone_index = bpy.props.IntProperty(name="Bone Index", default=0)
    bpy.types.Bone.bone_type = bpy.props.EnumProperty(items=bone_type_items, name="Bone Type", default="none")
    bpy.types.Bone.modifier_value1 = bpy.props.FloatProperty(name="Modifier Value 1", default=0.0)
    bpy.types.Bone.modifier_value2 = bpy.props.FloatProperty(name="Modifier Value 2", default=0.0)
    bpy.types.Bone.modifier_value3 = bpy.props.FloatProperty(name="Modifier Value 3", default=0.0)
    bpy.types.Bone.modifier_axis = bpy.props.EnumProperty(items=axis_items, name="Modifier Axis", default="0")
    bpy.types.Bone.target_id = bpy.props.IntProperty(name="Affected Bone ID", default=0)
    bpy.types.Bone.target_index = bpy.props.IntProperty(name="Affected Bone Index", default=0)
    bpy.types.Bone.collision_extra1 = bpy.props.FloatProperty(name="Collision Extra 1", default=0.0, update=update_drivers)
    bpy.types.Bone.collision_extra2 = bpy.props.FloatProperty(name="Collision Extra 2", default=0.0, update=update_drivers)
    bpy.types.Bone.collision_extra3 = bpy.props.FloatProperty(name="Collision Extra 3", default=0.0, update=update_drivers)
    bpy.types.Bone.header_value1 = bpy.props.FloatProperty(name="H1", default=0.0, update=update_drivers)
    bpy.types.Bone.header_value2 = bpy.props.FloatProperty(name="H2", default=0.0, update=update_drivers)
    bpy.types.Bone.header_value3 = bpy.props.FloatProperty(name="H3", default=0.0, update=update_drivers)
    bpy.types.Bone.header_value4 = bpy.props.FloatProperty(name="H4", default=1.0, update=update_drivers)
    bpy.types.Bone.swing_params = bpy.props.PointerProperty(type=SwingParamsProperties)
    bpy.types.Bone.nmd_rel_pos = bpy.props.FloatVectorProperty(name="Relative Position", get=get_nmd_rel_pos, set=set_nmd_rel_pos, size=3)
    bpy.types.Bone.nmd_rel_rot = bpy.props.FloatVectorProperty(name="Relative Rotation", get=get_nmd_rel_rot, set=set_nmd_rel_rot, size=3)

    bpy.types.EditBone.bone_id = bpy.props.IntProperty(name="Bone ID", default=0)
    bpy.types.EditBone.bone_index = bpy.props.IntProperty(name="Bone Index", default=0)
    bpy.types.EditBone.bone_type = bpy.props.EnumProperty(items=bone_type_items, name="Bone Type", default="none")
    bpy.types.EditBone.modifier_value1 = bpy.props.FloatProperty(name="Modifier Value 1", default=0.0)
    bpy.types.EditBone.modifier_value2 = bpy.props.FloatProperty(name="Modifier Value 2", default=0.0)
    bpy.types.EditBone.modifier_value3 = bpy.props.FloatProperty(name="Modifier Value 3", default=0.0)
    bpy.types.EditBone.modifier_axis = bpy.props.EnumProperty(items=axis_items, name="Modifier Axis", default="0")
    bpy.types.EditBone.target_id = bpy.props.IntProperty(name="Affected Bone ID", default=0)
    bpy.types.EditBone.target_index = bpy.props.IntProperty(name="Affected Bone Index", default=0)
    bpy.types.EditBone.collision_extra1 = bpy.props.FloatProperty(name="Collision Extra 1", default=0.0, update=update_drivers)
    bpy.types.EditBone.collision_extra2 = bpy.props.FloatProperty(name="Collision Extra 2", default=0.0, update=update_drivers)
    bpy.types.EditBone.collision_extra3 = bpy.props.FloatProperty(name="Collision Extra 3", default=0.0, update=update_drivers)
    # The header_value properties are already registered in the registration sequence

def unregister():
    for cls in reversed(classes):
        bpy.utils.unregister_class(cls)
    del bpy.types.Scene.bone_export_props
    del bpy.types.Bone.bone_id
    del bpy.types.Bone.bone_index
    del bpy.types.Bone.bone_type
    del bpy.types.Bone.modifier_value1
    del bpy.types.Bone.modifier_value2
    del bpy.types.Bone.modifier_value3
    del bpy.types.Bone.modifier_axis
    del bpy.types.Bone.target_id
    del bpy.types.Bone.target_index
    del bpy.types.Bone.collision_extra1
    del bpy.types.Bone.collision_extra2
    del bpy.types.Bone.collision_extra3
    del bpy.types.Bone.header_value1
    del bpy.types.Bone.header_value2
    del bpy.types.Bone.header_value3
    del bpy.types.Bone.header_value4
    del bpy.types.Bone.swing_params
    del bpy.types.Bone.nmd_rel_pos
    del bpy.types.Bone.nmd_rel_rot

if __name__ == "__main__":
    register()