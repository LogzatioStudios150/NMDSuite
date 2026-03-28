using Aspose.Zip.Saving;
using Microsoft.Win32;
using NMDBase;
using NMDSuite.BoneViews;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace NMDSuite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Bones
        (List<BoneNode> RootBones, List<BoneNode> Bones, int Count) bones = new();
        BoneNode selected_node = new();
        BoneNode? previous_select = new();
        string base_title;
       



        //Views
        PositionView pos_panel = new();
        SwingView swing_panel = new();
        CollisionView collision_panel = new();
        RotationView rotation_panel = new();
        ConstRotationView const_rotation_panel = new();
        SlerpView slerp_panel = new();
        HeaderScaleView header_scale_panel = new();
        CollisionHeaderView collision_header_panel = new();
        HeaderView header_panel = new();
        RandEyeView rand_eye_panel = new();
        RandLidView rand_lid_panel = new();
        ConstView const_panel = new();
        ScissorView scissor_panel = new();

        //Vector transform_buffer;
        //Vector rotation_buffer;

        //Icon? icon = new("./icon.ico");





        //Animations
        DoubleAnimation side_panel_open = new DoubleAnimation(600, new Duration(TimeSpan.FromSeconds(0.2)));
        DoubleAnimation side_panel_close = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.2)));

        DoubleAnimation search_open = new DoubleAnimation(87, new Duration(TimeSpan.FromSeconds(0.2)));
        DoubleAnimation search_close = new DoubleAnimation(47, new Duration(TimeSpan.FromSeconds(0.2)));

        DoubleAnimation opacity_anim = new DoubleAnimation(1.0, new Duration(TimeSpan.FromSeconds(0.2)));
        DoubleAnimation opacity_anim2 = new DoubleAnimation(0, new Duration(TimeSpan.FromSeconds(0.2)));




        //Filtering Types
        public List<ComboBoxItem> search_filters = new List<ComboBoxItem>()
        {
            new ComboBoxItem() {Content="By Name"},
            new ComboBoxItem() {Content="By Index"},
            new ComboBoxItem() {Content="By Parent Index"},
            new ComboBoxItem() {Content="By Type"},
            new ComboBoxItem() {Content="By Type Category"}

        };



        //ComboBox Items
        static ComboBoxItem pos_param = new() { Content = "Position Parameters" };
        static ComboBoxItem scale_param = new() { Content = "Header & Scale Parameters" };
        static ComboBoxItem header_param = new() { Content = "Header Parameters" };
        static ComboBoxItem collision_header_param = new() { Content = "Rotation and Scale Parameters" };
        static ComboBoxItem swing_param = new() { Content = "Swing Parameters" };
        static ComboBoxItem collision_param = new() { Content = "Collision Parameters" };
        static ComboBoxItem rotation_param = new() { Content = "Modifier Parameters (Rotation)" };
        static ComboBoxItem const_rotation_param = new() { Content = "Modifier Parameters (Const Rotation)" };
        static ComboBoxItem slerp_param = new() { Content = "Modifier Parameters (Slerp)" };
        static ComboBoxItem rand_eye_param = new() { Content = "Weapon Modifier Parameters (Type 1)" };
        static ComboBoxItem rand_lid_param = new() { Content = "Weapon Modifier Parameters (Type 2)" };
        static ComboBoxItem const_param = new() { Content = "Const Slerp Value" };
        static ComboBoxItem scissor_param = new() { Content = "Scissor Parameters" };

        //Lists
        public List<ComboBoxItem> standard_category = new List<ComboBoxItem>()
        {
            pos_param,
            scale_param
        };

        public List<ComboBoxItem> swing_category = new List<ComboBoxItem>()
        {
           pos_param,
           swing_param,
           collision_param
        };

        public List<ComboBoxItem> rotation_category = new List<ComboBoxItem>()
        {
            pos_param,
            header_param,
            rotation_param

        };

        public List<ComboBoxItem> const_rotation_category = new List<ComboBoxItem>()
        {
            pos_param,
            header_param,
            const_rotation_param,
        };

        public List<ComboBoxItem> slerp_category = new List<ComboBoxItem>()
        {
            pos_param,
            header_param,
            slerp_param,
        };

        public List<ComboBoxItem> rand_eye_category = new List<ComboBoxItem>()
        {
            pos_param,
            header_param,
            rand_eye_param,
        };

        public List<ComboBoxItem> rand_lid_category = new List<ComboBoxItem>()
        {
            pos_param,
            header_param,
            rand_lid_param,
        };

        public List<ComboBoxItem> const_category = new List<ComboBoxItem>()
        {
            pos_param,
            header_param,
            const_param,
        };

        public List<ComboBoxItem> scissor_category = new List<ComboBoxItem>()
        {
            pos_param,
            header_param,
            scissor_param,
        };
        public List<ComboBoxItem> collision_category = new List<ComboBoxItem>()
        {
            pos_param,
            collision_header_param,
        };

        public MainWindow()
        {
            InitializeComponent();

            //New Project Panel events


            //Setup Search Filters
            BoneFilter.ItemsSource = search_filters;
            BoneFilter.SelectedIndex = 0;



            //Setup Position Panel events 
            pos_panel.x_Pos_Box.LostFocus += X_Pos_Box_LostFocus;
            pos_panel.y_Pos_Box.LostFocus += Y_Pos_Box_LostFocus;
            pos_panel.z_Pos_Box.LostFocus += Z_Pos_Box_LostFocus;
            pos_panel.x_Rot_Box.LostFocus += X_Rot_Box_LostFocus;
            pos_panel.y_Rot_Box.LostFocus += Y_Rot_Box_LostFocus;
            pos_panel.z_Rot_Box.LostFocus += Z_Rot_Box_LostFocus;

            //Setup Swing Panel events
            swing_panel.constraint_y_Box.LostFocus += Constraint_y_Box_LostFocus;
            swing_panel.constraint_Y_Box.LostFocus += Constraint_Y_Box_LostFocus;
            swing_panel.constraint_x_Box.LostFocus += Constraint_x_Box_LostFocus;
            swing_panel.constraint_X_Box.LostFocus += Constraint_X_Box_LostFocus;
            swing_panel.val7_Box.LostFocus += Val7_Box_LostFocus;
            swing_panel.val6_Box.LostFocus += Val6_Box_LostFocus;
            swing_panel.val5_Box.LostFocus += Val5_Box_LostFocus;
            swing_panel.val4_Box.LostFocus += Val4_Box_LostFocus;
            swing_panel.val3_Box.LostFocus += Val3_Box_LostFocus;
            swing_panel.val2_Box.LostFocus += Val2_Box_LostFocus;
            swing_panel.val1_Box.LostFocus += Val1_Box_LostFocus;
            swing_panel.dampening_Box.LostFocus += Dampening_Box_LostFocus;
            swing_panel.gravity_y_Box.LostFocus += Gravity_y_Box_LostFocus;
            swing_panel.gravity_x_Box.LostFocus += Gravity_x_Box_LostFocus;
            swing_panel.length_Box.LostFocus += Length_Box_LostFocus;
            swing_panel.rigidity_Box.LostFocus += Rigidity_Box_LostFocus;
            swing_panel.flag_Box.LostFocus += Flag_Box_LostFocus;
            swing_panel.influence_x_Box.LostFocus += Influence_x_Box_LostFocus;
            swing_panel.influence_y_Box.LostFocus += Influence_y_Box_LostFocus;
            swing_panel.influence_z_Box.LostFocus += Influence_z_Box_LostFocus;


            //Setup Rotation Panel events
            rotation_panel.Rotation_Box.LostFocus += ModifierValue1_LostFocus;
            rotation_panel.axis_Box.SelectionChanged += Axis_Box_SelectionChanged;
            rotation_panel.bone_index_Box.LostFocus += ModifierIndex1_LostFocus;
            rotation_panel.Rotation_Index1_Info.MouseEnter += Rotation_Index1_Info_MouseEnter;

            //Const Rotation Panel events
            const_rotation_panel.Rotation_Box.LostFocus += ModifierValue1_LostFocus;
            const_rotation_panel.bone_index1_Box.LostFocus += ModifierIndex1_LostFocus;
            const_rotation_panel.bone_index2_Box.LostFocus += Bone_index2_Box_LostFocus;
            const_rotation_panel.Const_Rotation_Index1_Info.MouseEnter += Const_Rotation_Index1_Info_MouseEnter;
            const_rotation_panel.Const_Rotation_Index2_Info.MouseEnter += Const_Rotation_Index2_Info_MouseEnter;

            //Slerp Panel events
            slerp_panel.slerp_Box.LostFocus += ModifierValue2_LostFocus;
            slerp_panel.bone_index1_Box.LostFocus += ModifierIndex1_LostFocus;
            slerp_panel.bone_index2_Box.LostFocus += Bone_index2_Box_LostFocus;
            slerp_panel.slerp2_Box.LostFocus += ModifierValue2_LostFocus;
            slerp_panel.bone_index1_Info.MouseEnter += Bone_index1_Info_MouseEnter;
            slerp_panel.bone_index2_Info.MouseEnter += Bone_index2_Info_MouseEnter;

            header_scale_panel.Scale_x_Box.LostFocus += Scale_x_Box_LostFocus;
            header_scale_panel.Scale_y_Box.LostFocus += Scale_y_Box_LostFocus;
            header_scale_panel.Scale_z_Box.LostFocus += Scale_z_Box_LostFocus;
            header_scale_panel.Header1_Box.LostFocus += Header1_Box_LostFocus;
            header_scale_panel.Header2_Box.LostFocus += Header2_Box_LostFocus;
            header_scale_panel.Header3_Box.LostFocus += Header3_Box_LostFocus;
            header_scale_panel.Header4_Box.LostFocus += Header4_Box_LostFocus;

            collision_header_panel.Scale_x_Box.LostFocus += Scale_x_Box_LostFocus;
            collision_header_panel.Scale_y_Box.LostFocus += Scale_y_Box_LostFocus;
            collision_header_panel.Scale_z_Box.LostFocus += Scale_z_Box_LostFocus;
            collision_header_panel.Header1_Box.LostFocus += Header1_Box_LostFocus;
            collision_header_panel.Header2_Box.LostFocus += Header2_Box_LostFocus;
            collision_header_panel.Header3_Box.LostFocus += Header3_Box_LostFocus;
            collision_header_panel.Header4_Box.LostFocus += Header4_Box_LostFocus;

            collision_header_panel.Scale_x_Box.LostFocus += Scale_x_Box_LostFocus;
            collision_header_panel.Scale_y_Box.LostFocus += Scale_y_Box_LostFocus;
            collision_header_panel.Scale_z_Box.LostFocus += Scale_z_Box_LostFocus;
            collision_header_panel.Header1_Box.LostFocus += Header1_Box_LostFocus;
            collision_header_panel.Header2_Box.LostFocus += Header2_Box_LostFocus;
            collision_header_panel.Header3_Box.LostFocus += Header3_Box_LostFocus;
            collision_header_panel.Header4_Box.LostFocus += Header4_Box_LostFocus;

            header_panel.Header1_Box.LostFocus += Header1_Box_LostFocus;
            header_panel.Header2_Box.LostFocus += Header2_Box_LostFocus;
            header_panel.Header3_Box.LostFocus += Header3_Box_LostFocus;
            header_panel.Header4_Box.LostFocus += Header4_Box_LostFocus;

            //Collision Panel events
            collision_panel.CollisionSlot1_Toggle.Unchecked += CollisionSlot1_Toggle_Unchecked;
            collision_panel.CollisionSlot2_Toggle.Unchecked += CollisionSlot2_Toggle_Unchecked;
            collision_panel.CollisionSlot3_Toggle.Unchecked += CollisionSlot3_Toggle_Unchecked;
            collision_panel.CollisionSlot4_Toggle.Unchecked += CollisionSlot4_Toggle_Unchecked;

            collision_panel.SwingCollisionSlot1_Toggle.Unchecked += SwingCollisionSlot1_Toggle_Unchecked;
            collision_panel.SwingCollisionSlot2_Toggle.Unchecked += SwingCollisionSlot2_Toggle_Unchecked;
            collision_panel.SwingCollisionSlot3_Toggle.Unchecked += SwingCollisionSlot3_Toggle_Unchecked;
            collision_panel.SwingCollisionSlot4_Toggle.Unchecked += SwingCollisionSlot4_Toggle_Unchecked;

            collision_panel.CollisionSlot1_Toggle.Checked += CollisionSlot1_Toggle_Checked;
            collision_panel.CollisionSlot2_Toggle.Checked += CollisionSlot2_Toggle_Checked;
            collision_panel.CollisionSlot3_Toggle.Checked += CollisionSlot3_Toggle_Checked;
            collision_panel.CollisionSlot4_Toggle.Checked += CollisionSlot4_Toggle_Checked;

            collision_panel.SwingCollisionSlot1_Toggle.Checked += SwingCollisionSlot1_Toggle_Checked;
            collision_panel.SwingCollisionSlot2_Toggle.Checked += SwingCollisionSlot2_Toggle_Checked;
            collision_panel.SwingCollisionSlot3_Toggle.Checked += SwingCollisionSlot3_Toggle_Checked;
            collision_panel.SwingCollisionSlot4_Toggle.Checked += SwingCollisionSlot4_Toggle_Checked;

            collision_panel.CollisionSlot1.LostFocus += CollisionSlot1_LostFocus;
            collision_panel.CollisionSlot2.LostFocus += CollisionSlot2_LostFocus;
            collision_panel.CollisionSlot3.LostFocus += CollisionSlot3_LostFocus;
            collision_panel.CollisionSlot4.LostFocus += CollisionSlot4_LostFocus;

            collision_panel.CollisionSlot1.TextChanged += CollisionSlot1_TextChanged;
            collision_panel.CollisionSlot2.TextChanged += CollisionSlot2_TextChanged;
            collision_panel.CollisionSlot3.TextChanged += CollisionSlot3_TextChanged;
            collision_panel.CollisionSlot4.TextChanged += CollisionSlot4_TextChanged;

            collision_panel.CollisonSlot1_Info.MouseEnter += CollisonSlot1_Info_MouseEnter;
            collision_panel.CollisonSlot2_Info.MouseEnter += CollisonSlot2_Info_MouseEnter;
            collision_panel.CollisonSlot3_Info.MouseEnter += CollisonSlot3_Info_MouseEnter;
            collision_panel.CollisonSlot4_Info.MouseEnter += CollisonSlot4_Info_MouseEnter;

            collision_panel.SwingCollisionSlot1.LostFocus += SwingCollisionSlot1_LostFocus;
            collision_panel.SwingCollisionSlot2.LostFocus += SwingCollisionSlot2_LostFocus;
            collision_panel.SwingCollisionSlot3.LostFocus += SwingCollisionSlot3_LostFocus;
            collision_panel.SwingCollisionSlot4.LostFocus += SwingCollisionSlot4_LostFocus;

            collision_panel.SwingCollisionSlot1.TextChanged += SwingCollisionSlot1_TextChanged;
            collision_panel.SwingCollisionSlot2.TextChanged += SwingCollisionSlot2_TextChanged;
            collision_panel.SwingCollisionSlot3.TextChanged += SwingCollisionSlot3_TextChanged;
            collision_panel.SwingCollisionSlot4.TextChanged += SwingCollisionSlot4_TextChanged;

            collision_panel.SwingCollisonSlot1_Info.MouseEnter += SwingCollisonSlot1_Info_MouseEnter;
            collision_panel.SwingCollisonSlot2_Info.MouseEnter += SwingCollisonSlot2_Info_MouseEnter;
            collision_panel.SwingCollisonSlot3_Info.MouseEnter += SwingCollisonSlot3_Info_MouseEnter;
            collision_panel.SwingCollisonSlot4_Info.MouseEnter += SwingCollisonSlot4_Info_MouseEnter;

            rand_eye_panel.Rotation_Box.LostFocus += ModifierValue1_LostFocus;
            rand_eye_panel.slerp2_Box.LostFocus += ModifierValue2_LostFocus;

            rand_lid_panel.Rotation_Box.LostFocus += ModifierValue1_LostFocus;
            rand_lid_panel.bone_index_Box.LostFocus += ModifierIndex1_LostFocus;

            const_panel.bone_index_Box.LostFocus += ModifierIndex1_LostFocus;
            const_panel.bone_index1_Info.MouseEnter += Const_index1_Info_MouseEnter;

            scissor_panel.Value1_Box.LostFocus += ModifierValue1_LostFocus;
            scissor_panel.Value2_Box.LostFocus += ModifierValue2_LostFocus;
            scissor_panel.Scissor_Axis_Box.LostFocus += ModifierAxis_LostFocus;


        }

        private void NMDBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog nmd_dialog = new();
            nmd_dialog.Filter = "NMD Files|*.nmd";
            nmd_dialog.Title = "Select NMD file";
            if ((bool)nmd_dialog.ShowDialog() == true)
            {
                NMDBox.Text = nmd_dialog.FileName;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //SidePanel.Navigate(new_panel_nmd);
            ShowInTaskbar = true;
            base_title = Title;
            if (Splash.fileToParse != null)
            {
                InitializeTree(Splash.fileToParse, Splash.keepeye);
            }

        }

        private void ViewSelect_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {

            ComboBoxItem selected_item = ViewSelect.SelectedItem as ComboBoxItem;

            if (selected_item != null)
            {
                if (selected_item.Content == pos_param.Content)
                {
                    BonePropView.Navigate(pos_panel);

                }

                if (selected_item.Content == scale_param.Content)
                {
                    BonePropView.Navigate(header_scale_panel);
                }

                if (selected_item.Content == header_param.Content)
                {
                    BonePropView.Navigate(header_panel);
                }

                if (selected_item.Content == swing_param.Content)
                {
                    BonePropView.Navigate(swing_panel);
                }

                if (selected_item.Content == collision_param.Content)
                {
                    BonePropView.Navigate(collision_panel);
                }

                if (selected_item.Content == rotation_param.Content)
                {
                    BoneNode? node = BoneView.SelectedItem as BoneNode;
                    rotation_panel.Bone_ID_text.Text = "Bone ID:";
                    if (node != null && node.Data.Bonetype == BoneType.Types["rot"])
                    {
                        rotation_panel.Bone_ID_text.Text = "Bone Index:";
                    }
                    BonePropView.Navigate(rotation_panel);
                }

                if (selected_item.Content == const_rotation_param.Content)
                {
                    BonePropView.Navigate(const_rotation_panel);
                }

                if (selected_item.Content == slerp_param.Content)
                {
                    BonePropView.Navigate(slerp_panel);
                }

                if (selected_item.Content == rand_eye_param.Content)
                {
                    BonePropView.Navigate(rand_eye_panel);
                }

                if (selected_item.Content == rand_lid_param.Content)
                {
                    BonePropView.Navigate(rand_lid_panel);
                }

                if (selected_item.Content == const_param.Content)
                {
                    BonePropView.Navigate(const_panel);
                }
                
                if (selected_item.Content == scissor_param.Content)
                {
                    BonePropView.Navigate(scissor_panel);
                }

                if (selected_item.Content == collision_header_param.Content)
                {
                    BonePropView.Navigate(collision_header_panel);
                }

            }






        }

        private void BoneView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //ViewSelect.SelectedIndex = 0;

            if (selected_node != null)
            {
                BoneTools_Panel.Visibility = Visibility.Visible;
                foreach (var bone in bones.Item2)
                {
                    TreeViewItem? selected = e.NewValue as TreeViewItem;

                    if (selected != null && selected.Header.ToString() == $"{bone.Data.Index}: {bone.Data.Name}")
                    {
                        selected_node = bone;
                        break;
                    }



                }
                /*if (selected_node.Menu == true)
                {
                    BoneView.ContextMenu = selected_node.Data.Bonetype.Category switch
                    {
                        "standard" => BoneView.Resources["StandardMenu"] as System.Windows.Controls.ContextMenu,
                        "swing" => BoneView.Resources["SwingMenu"] as System.Windows.Controls.ContextMenu,
                        "collision" => BoneView.Resources["CollisionMenu"] as System.Windows.Controls.ContextMenu,
                        "modifier"  => BoneView.Resources["ModifierMenu"] as System.Windows.Controls.ContextMenu,
                        _ => BoneView.Resources["StandardMenu"] as System.Windows.Controls.ContextMenu
                    };
                    
                }*/

                pos_panel.DataContext = selected_node;
                header_scale_panel.DataContext = selected_node;
                header_panel.DataContext = selected_node;
                swing_panel.DataContext = selected_node;
                collision_panel.DataContext = selected_node;
                rotation_panel.DataContext = selected_node;
                const_rotation_panel.DataContext = selected_node;
                slerp_panel.DataContext = selected_node;
                rand_eye_panel.DataContext = selected_node;
                rand_lid_panel.DataContext = selected_node;
                const_panel.DataContext = selected_node;
                scissor_panel.DataContext = selected_node;
                collision_header_panel.DataContext = selected_node;





                if (ViewSelect.SelectedIndex == -1)
                {
                    ViewSelect.SelectedIndex = 0;
                }


                if (selected_node != null && selected_node.Data.Bonetype.Category != previous_select.Data.Bonetype.Category)
                {
                    if (selected_node.Data.Bonetype.Category == BoneTypeCategory.RotationModifier)
                    {
                        ViewSelect.ItemsSource = rotation_category;
                        ViewSelect.SelectedIndex = 2;
                    }

                    if (selected_node.Data.Bonetype.Category == BoneTypeCategory.ConstRotationModifier)
                    {
                        ViewSelect.ItemsSource = const_rotation_category;
                        ViewSelect.SelectedIndex = 2;
                        Trace.Write("Const");                    
                    }

                    if (selected_node.Data.Bonetype.Category == BoneTypeCategory.SlerpModifier)
                    {
                        ViewSelect.ItemsSource = slerp_category;
                        ViewSelect.SelectedIndex = 2;
                    }

                    if(selected_node.Data.Bonetype.Category == BoneTypeCategory.Standard || selected_node.Data.Bonetype.Category == BoneTypeCategory.Modifier)
                    {
                        ViewSelect.ItemsSource = standard_category;

                        ViewSelect.SelectedIndex = 0;
                    }

                    if(selected_node.Data.Bonetype.Category == BoneTypeCategory.Collision)
                    {
                        ViewSelect.ItemsSource = collision_category;

                        ViewSelect.SelectedIndex = 1;
                    }

                    if (selected_node.Data.Bonetype.Category == BoneTypeCategory.Swing)
                    {
                        ViewSelect.ItemsSource = swing_category;
                        ViewSelect.SelectedIndex = 1;
                    }

                    if (selected_node.Data.Bonetype.Category == BoneTypeCategory.WeaponModifier1)
                    {
                        ViewSelect.ItemsSource = rand_eye_category;
                        ViewSelect.SelectedIndex = 2;
                    }

                    if (selected_node.Data.Bonetype.Category == BoneTypeCategory.WeaponModifier2)
                    {
                        ViewSelect.ItemsSource = rand_lid_category;
                        ViewSelect.SelectedIndex = 2;
                    }

                    if (selected_node.Data.Bonetype.Category == BoneTypeCategory.ConstSlerpModifier)
                    {
                        ViewSelect.ItemsSource = const_category;
                        ViewSelect.SelectedIndex = 2;
                    }

                    if (selected_node.Data.Bonetype.Category == BoneTypeCategory.Scissor)
                    {
                        ViewSelect.ItemsSource = scissor_category;
                        ViewSelect.SelectedIndex = 2;
                    }

                }


                if (selected_node != null)
                {
                    pos_panel.x_Pos_Box.Text = Convert.ToString(selected_node.Data.Position.X);
                    pos_panel.y_Pos_Box.Text = Convert.ToString(selected_node.Data.Position.Y);
                    pos_panel.z_Pos_Box.Text = Convert.ToString(selected_node.Data.Position.Z);
                    pos_panel.x_Rot_Box.Text = Convert.ToString(selected_node.Data.Rotation.X);
                    pos_panel.y_Rot_Box.Text = Convert.ToString(selected_node.Data.Rotation.Y);
                    pos_panel.z_Rot_Box.Text = Convert.ToString(selected_node.Data.Rotation.Z);

                    swing_panel.gravity_x_Box.Text = Convert.ToString(selected_node.Data.Gravity.X);
                    swing_panel.gravity_y_Box.Text = Convert.ToString(selected_node.Data.Gravity.Y);
                    swing_panel.length_Box.Text = Convert.ToString(selected_node.Data.Length);
                    swing_panel.dampening_Box.Text = Convert.ToString(selected_node.Data.Dampening);
                    swing_panel.val1_Box.Text = Convert.ToString(selected_node.Data.Val1);
                    swing_panel.val2_Box.Text = Convert.ToString(selected_node.Data.Val2);
                    swing_panel.val3_Box.Text = Convert.ToString(selected_node.Data.Val3);
                    swing_panel.val4_Box.Text = Convert.ToString(selected_node.Data.Val4);
                    swing_panel.val5_Box.Text = Convert.ToString(selected_node.Data.Val5);
                    swing_panel.val6_Box.Text = Convert.ToString(selected_node.Data.Val6);
                    swing_panel.val7_Box.Text = Convert.ToString(selected_node.Data.Val7);
                    swing_panel.constraint_X_Box.Text = Convert.ToString(selected_node.Data.Constraints[0]);
                    swing_panel.constraint_x_Box.Text = Convert.ToString(selected_node.Data.Constraints[1]);
                    swing_panel.constraint_Y_Box.Text = Convert.ToString(selected_node.Data.Constraints[2]);
                    swing_panel.constraint_y_Box.Text = Convert.ToString(selected_node.Data.Constraints[3]);
                    swing_panel.rigidity_Box.Text = Convert.ToString(selected_node.Data.Rigidity);
                    swing_panel.flag_Box.Text = Convert.ToString(selected_node.Data.Flag);
                    swing_panel.influence_x_Box.Text = Convert.ToString(selected_node.Data.InfluenceX);
                    swing_panel.influence_y_Box.Text = Convert.ToString(selected_node.Data.InfluenceY);
                    swing_panel.influence_z_Box.Text = Convert.ToString(selected_node.Data.InfluenceZ);

                    rotation_panel.Rotation_Box.Text = Convert.ToString(selected_node.Data.ModifierValue1);
                    rotation_panel.axis_Box.SelectedIndex = selected_node.Data.ModifierAxis;
                    rotation_panel.bone_index_Box.Text = Convert.ToString(selected_node.Data.TargetID);

                    const_rotation_panel.Rotation_Box.Text = Convert.ToString(selected_node.Data.ModifierValue1);
                    const_rotation_panel.bone_index1_Box.Text = Convert.ToString(selected_node.Data.TargetID);
                    const_rotation_panel.bone_index2_Box.Text = Convert.ToString(selected_node.Data.TargetIndex);

                    slerp_panel.slerp_Box.Text = Convert.ToString(selected_node.Data.ModifierValue1);
                    slerp_panel.bone_index1_Box.Text = Convert.ToString(selected_node.Data.TargetID);
                    slerp_panel.bone_index2_Box.Text = Convert.ToString(selected_node.Data.TargetIndex);
                    slerp_panel.slerp2_Box.Text = Convert.ToString(selected_node.Data.ModifierValue2);

                    header_scale_panel.Scale_x_Box.Text = Convert.ToString(selected_node.Data.Scale.X);
                    header_scale_panel.Scale_y_Box.Text = Convert.ToString(selected_node.Data.Scale.Y);
                    header_scale_panel.Scale_z_Box.Text = Convert.ToString(selected_node.Data.Scale.Z);
                    header_scale_panel.Header1_Box.Text = Convert.ToString(selected_node.Data.Header[0]);
                    header_scale_panel.Header2_Box.Text = Convert.ToString(selected_node.Data.Header[1]);
                    header_scale_panel.Header3_Box.Text = Convert.ToString(selected_node.Data.Header[2]);
                    header_scale_panel.Header4_Box.Text = Convert.ToString(selected_node.Data.Header[3]);

                    collision_header_panel.Scale_x_Box.Text = Convert.ToString(selected_node.Data.Scale.X);
                    collision_header_panel.Scale_y_Box.Text = Convert.ToString(selected_node.Data.Scale.Y);
                    collision_header_panel.Scale_z_Box.Text = Convert.ToString(selected_node.Data.Scale.Z);
                    collision_header_panel.Header1_Box.Text = Convert.ToString(selected_node.Data.Header[0]);
                    collision_header_panel.Header2_Box.Text = Convert.ToString(selected_node.Data.Header[1]);
                    collision_header_panel.Header3_Box.Text = Convert.ToString(selected_node.Data.Header[2]);
                    collision_header_panel.Header4_Box.Text = Convert.ToString(selected_node.Data.Header[3]);

                    header_panel.Header1_Box.Text = Convert.ToString(selected_node.Data.Header[0]);
                    header_panel.Header2_Box.Text = Convert.ToString(selected_node.Data.Header[1]);
                    header_panel.Header3_Box.Text = Convert.ToString(selected_node.Data.Header[2]);
                    header_panel.Header4_Box.Text = Convert.ToString(selected_node.Data.Header[3]);

                    rand_eye_panel.Rotation_Box.Text = Convert.ToString(selected_node.Data.ModifierValue1);
                    rand_eye_panel.slerp2_Box.Text = Convert.ToString(selected_node.Data.ModifierValue2);

                    rand_lid_panel.Rotation_Box.Text = Convert.ToString(selected_node.Data.ModifierValue1);
                    rand_lid_panel.bone_index_Box.Text = Convert.ToString(selected_node.Data.TargetID);

                    const_panel.bone_index_Box.Text = Convert.ToString(selected_node.Data.TargetID);

                    scissor_panel.Value1_Box.Text = Convert.ToString(selected_node.Data.ModifierValue1);
                    scissor_panel.Value2_Box.Text = Convert.ToString(selected_node.Data.ModifierValue2);
                    scissor_panel.Scissor_Axis_Box.Text = Convert.ToString(selected_node.Data.ModifierAxis);


                    collision_panel.CollisionSlot1.Text = String.Empty;
                    collision_panel.CollisionSlot2.Text = String.Empty;
                    collision_panel.CollisionSlot3.Text = String.Empty;
                    collision_panel.CollisionSlot4.Text = String.Empty;

                    collision_panel.SwingCollisionSlot1.Text = String.Empty;
                    collision_panel.SwingCollisionSlot2.Text = String.Empty;
                    collision_panel.SwingCollisionSlot3.Text = String.Empty;
                    collision_panel.SwingCollisionSlot4.Text = String.Empty;

                    if (selected_node.Data.CollisionCount == 1)
                    {
                        collision_panel.CollisionSlot1_Toggle.IsChecked = true;
                    }

                    if (selected_node.Data.CollisionCount == 2)
                    {
                        collision_panel.CollisionSlot1_Toggle.IsChecked = true;
                        collision_panel.CollisionSlot2_Toggle.IsChecked = true;
                    }

                    if (selected_node.Data.CollisionCount == 3)
                    {
                        collision_panel.CollisionSlot1_Toggle.IsChecked = true;
                        collision_panel.CollisionSlot2_Toggle.IsChecked = true;
                        collision_panel.CollisionSlot3_Toggle.IsChecked = true;
                    }

                    if (selected_node.Data.CollisionCount == 4)
                    {
                        collision_panel.CollisionSlot1_Toggle.IsChecked = true;
                        collision_panel.CollisionSlot2_Toggle.IsChecked = true;
                        collision_panel.CollisionSlot3_Toggle.IsChecked = true;
                        collision_panel.CollisionSlot4_Toggle.IsChecked = true;
                    }

                    if (selected_node.Data.SwingCount == 1)
                    {
                        collision_panel.SwingCollisionSlot1_Toggle.IsChecked = true;
                    }

                    if (selected_node.Data.SwingCount == 2)
                    {
                        collision_panel.SwingCollisionSlot1_Toggle.IsChecked = true;
                        collision_panel.SwingCollisionSlot2_Toggle.IsChecked = true;
                    }

                    if (selected_node.Data.SwingCount == 3)
                    {
                        collision_panel.SwingCollisionSlot1_Toggle.IsChecked = true;
                        collision_panel.SwingCollisionSlot2_Toggle.IsChecked = true;
                        collision_panel.SwingCollisionSlot3_Toggle.IsChecked = true;
                    }

                    if (selected_node.Data.SwingCount == 4)
                    {
                        collision_panel.SwingCollisionSlot1_Toggle.IsChecked = true;
                        collision_panel.SwingCollisionSlot2_Toggle.IsChecked = true;
                        collision_panel.SwingCollisionSlot3_Toggle.IsChecked = true;
                        collision_panel.SwingCollisionSlot4_Toggle.IsChecked = true;
                    }

                    collision_panel.CollisionSlot1.Text = Convert.ToString(selected_node.Data.CollisionList[0]);
                    collision_panel.CollisionSlot2.Text = Convert.ToString(selected_node.Data.CollisionList[1]);
                    collision_panel.CollisionSlot3.Text = Convert.ToString(selected_node.Data.CollisionList[2]);
                    collision_panel.CollisionSlot4.Text = Convert.ToString(selected_node.Data.CollisionList[3]);


                    collision_panel.SwingCollisionSlot1.Text = Convert.ToString(selected_node.Data.SwingCollisionList[0]);
                    collision_panel.SwingCollisionSlot2.Text = Convert.ToString(selected_node.Data.SwingCollisionList[1]);
                    collision_panel.SwingCollisionSlot3.Text = Convert.ToString(selected_node.Data.SwingCollisionList[2]);
                    collision_panel.SwingCollisionSlot4.Text = Convert.ToString(selected_node.Data.SwingCollisionList[3]);

                }
                previous_select = selected_node;
                BoneInfoLabel.Text = $"Bone Info -- ID: {selected_node.Data.BoneId}, Index: {selected_node.Data.Index}, Parent Index: {(short)selected_node.Data.ParentId}, Type: {selected_node.Data.Bonetype.Name}, Type Category: {selected_node.Data.Bonetype.Category}";            }
            //SidePanel.BeginAnimation(WidthProperty,side_panel_close);

        }
        #region BonePropView events
        // Pos text field events
        private void X_Pos_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            Vector3 position = selected_node.Data.Position;
            try
            {
                position.X = Convert.ToSingle(pos_panel.x_Pos_Box.Text);
            }
            catch
            {
                pos_panel.x_Pos_Box.Text = Convert.ToString(position.X);
            }
            selected_node.Data.Position = position;
        }
        private void Y_Pos_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            Vector3 position = selected_node.Data.Position;
            try
            {
                position.Y = Convert.ToSingle(pos_panel.y_Pos_Box.Text);
            }
            catch
            {
                pos_panel.y_Pos_Box.Text = Convert.ToString(position.Y);
            }
            selected_node.Data.Position = position;
        }
        private void Z_Pos_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            Vector3 position = selected_node.Data.Position;
            try
            {
                position.Z = Convert.ToSingle(pos_panel.z_Pos_Box.Text);
            }
            catch
            {
                pos_panel.z_Pos_Box.Text = Convert.ToString(position.Z);
            }
            selected_node.Data.Position = position;
        }
        private void X_Rot_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            Vector3 rotation = selected_node.Data.Rotation;
            try
            {
                rotation.X = Convert.ToSingle(pos_panel.x_Rot_Box.Text);
            }
            catch
            {
                pos_panel.x_Rot_Box.Text = Convert.ToString(rotation.X);
            }
            selected_node.Data.Rotation = rotation;
            
        }
        private void Y_Rot_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            Vector3 rotation = selected_node.Data.Rotation;
            try
            {
                rotation.Y = Convert.ToSingle(pos_panel.y_Rot_Box.Text);
            }
            catch
            {
                pos_panel.y_Rot_Box.Text = Convert.ToString(rotation.Y);
            }
            selected_node.Data.Rotation = rotation;

        }
        private void Z_Rot_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            Vector3 rotation = selected_node.Data.Rotation;
            try
            {
                rotation.Z = Convert.ToSingle(pos_panel.z_Rot_Box.Text);
            }
            catch
            {
                pos_panel.z_Rot_Box.Text = Convert.ToString(rotation.Z);
            }
            selected_node.Data.Rotation = rotation;
        }
        private void Scale_x_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            Vector3 scale = selected_node.Data.Scale;
            try
            {
                scale.X = Convert.ToSingle(header_scale_panel.Scale_x_Box.Text);
            }
            catch
            {
                header_scale_panel.Scale_x_Box.Text = Convert.ToString(scale.X);
            }
            selected_node.Data.Scale = scale;
            
        }
        private void Scale_y_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            Vector3 scale = selected_node.Data.Scale;
            try
            {
                scale.Y = Convert.ToSingle(header_scale_panel.Scale_y_Box.Text);
            }
            catch
            {
                header_scale_panel.Scale_y_Box.Text = Convert.ToString(scale.Y);
            }
            selected_node.Data.Scale = scale;
        }
        private void Scale_z_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            Vector3 scale = selected_node.Data.Scale;
            try
            {
                scale.Z = Convert.ToSingle(header_scale_panel.Scale_z_Box.Text);
            }
            catch
            {
                header_scale_panel.Scale_z_Box.Text = Convert.ToString(scale.Z);
            }
            selected_node.Data.Scale = scale;
        }
        private void Header1_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                selected_node.Data.Header[0] = Convert.ToSingle(header_scale_panel.Header1_Box.Text);
            }
            catch
            {
                header_scale_panel.Header1_Box.Text = Convert.ToString(selected_node.Data.Header[0]);
            }
        }
        private void Header2_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                selected_node.Data.Header[1] = Convert.ToSingle(header_scale_panel.Header2_Box.Text);
            }
            catch
            {
                header_scale_panel.Header2_Box.Text = Convert.ToString(selected_node.Data.Header[1]);
            }
        }
        private void Header3_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                selected_node.Data.Header[2] = Convert.ToSingle(header_scale_panel.Header3_Box.Text);
            }
            catch
            {
                header_scale_panel.Header3_Box.Text = Convert.ToString(selected_node.Data.Header[2]);
            }
            
        }
        private void Header4_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                selected_node.Data.Header[3] = Convert.ToSingle(header_scale_panel.Header4_Box.Text);
            }
            catch
            {
                header_scale_panel.Header4_Box.Text = Convert.ToString(selected_node.Data.Header[3]);
            }
        }

        //swing text field events
        private void Length_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            selected_node.Data.Length = Convert.ToSingle(swing_panel.length_Box.Text);
        }
        private void Gravity_x_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            (short X,short Y) gravity = selected_node.Data.Gravity;
            try
            {
                gravity.X = Convert.ToInt16(swing_panel.gravity_x_Box.Text);
            }
            catch
            {
                swing_panel.gravity_x_Box.Text = Convert.ToString(gravity.X);
            }
            selected_node.Data.Gravity = gravity;
        }
        private void Gravity_y_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            (short X, short Y) gravity = selected_node.Data.Gravity;
            try
            {
                gravity.Y = Convert.ToInt16(swing_panel.gravity_y_Box.Text);
            }
            catch
            {
                swing_panel.gravity_y_Box.Text = Convert.ToString(gravity.Y);
            }
            selected_node.Data.Gravity = gravity;
        }
        private void Dampening_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            selected_node.Data.Dampening = Convert.ToInt16(swing_panel.dampening_Box.Text);
        }
        private void Val1_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            selected_node.Data.Val1 = Convert.ToByte(swing_panel.val1_Box.Text);
        }
        private void Val2_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            selected_node.Data.Val2 = Convert.ToByte(swing_panel.val2_Box.Text);
        }
        private void Val3_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            selected_node.Data.Val3 = Convert.ToInt16(swing_panel.val3_Box.Text);
        }
        private void Val4_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            selected_node.Data.Val4 = Convert.ToInt16(swing_panel.val4_Box.Text);
        }
        private void Val5_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            selected_node.Data.Val5 = Convert.ToInt16(swing_panel.val5_Box.Text);
        }
        private void Val6_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            selected_node.Data.Val6 = Convert.ToSingle(swing_panel.val6_Box.Text);
        }
        private void Val7_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            selected_node.Data.Val7 = Convert.ToInt16(swing_panel.val7_Box.Text);
        }
        private void Constraint_X_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            selected_node.Data.Constraints[0] = Convert.ToSByte(swing_panel.constraint_X_Box.Text);
        }
        private void Constraint_x_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            selected_node.Data.Constraints[1] = Convert.ToSByte(swing_panel.constraint_x_Box.Text);
        }
        private void Constraint_Y_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            selected_node.Data.Constraints[2] = Convert.ToSByte(swing_panel.constraint_Y_Box.Text);
        }
        private void Constraint_y_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            selected_node.Data.Constraints[3] = Convert.ToSByte(swing_panel.constraint_y_Box.Text);
        }
        private void Rigidity_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            selected_node.Data.Rigidity = Convert.ToSByte(swing_panel.rigidity_Box.Text);
        }
        private void Flag_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            selected_node.Data.Flag = Convert.ToByte(swing_panel.flag_Box.Text);
        }
        private void Influence_x_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            selected_node.Data.InfluenceX = Convert.ToByte(swing_panel.influence_x_Box.Text);
        }
        private void Influence_y_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            selected_node.Data.InfluenceY = Convert.ToByte(swing_panel.influence_y_Box.Text);
        }
        private void Influence_z_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            selected_node.Data.InfluenceZ = Convert.ToByte(swing_panel.influence_z_Box.Text);
        }

        //rotation and slerp view events
        private void Axis_Box_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selected_node.Data.ModifierAxis = (byte)rotation_panel.axis_Box.SelectedIndex;
        }
        private void ModifierIndex1_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox val = e.Source as TextBox;
            try
            {
                selected_node.Data.TargetID = Convert.ToUInt16(val.Text);
            }
            catch 
            {
                val.Text = Convert.ToString(selected_node.Data.TargetID);
            }
            
        }
        private void Bone_index2_Box_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox val = e.Source as TextBox;
            try
            {
                selected_node.Data.TargetIndex = Convert.ToUInt16(val.Text);
            }
            catch
            {
                val.Text = Convert.ToString(selected_node.Data.TargetIndex);
            }
        }
        private void Bone_index2_Info_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                slerp_panel.bone_index2_Info.ToolTip = $"Value is currently set to: {BoneNameFromID(Convert.ToUInt16(slerp_panel.bone_index2_Box.Text))}";
            }
            catch
            {
                slerp_panel.bone_index2_Info.ToolTip = "";
            }

        }

        private void Bone_index1_Info_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                slerp_panel.bone_index1_Info.ToolTip = $"Value is currently set to: {BoneNameFromIndex(Convert.ToUInt16(slerp_panel.bone_index1_Box.Text))}";
            }
            catch
            {
                slerp_panel.bone_index1_Info.ToolTip = "";
            }
        }
        private void Const_index1_Info_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                const_panel.bone_index1_Info.ToolTip = $"Value is currently set to: {BoneNameFromIndex(Convert.ToUInt16(const_panel.bone_index_Box.Text))}";
            }
            catch
            {
                const_panel.bone_index1_Info.ToolTip = "";
            }

        }

        private void Const_Rotation_Index2_Info_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (const_rotation_panel.bone_index2_Box.Text == "0")
            {
                const_rotation_panel.Const_Rotation_Index2_Info.ToolTip = "";
            }
            else
            {
                const_rotation_panel.Const_Rotation_Index2_Info.ToolTip = $"Value is currently set to: {BoneNameFromIndex(Convert.ToUInt16(const_rotation_panel.bone_index2_Box.Text))}";
            }
        }

        private void Const_Rotation_Index1_Info_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (const_rotation_panel.bone_index1_Box.Text == "0")
            {
                const_rotation_panel.Const_Rotation_Index1_Info.ToolTip = "";
            }
            else
            {
                const_rotation_panel.Const_Rotation_Index1_Info.ToolTip = $"Value is currently set to: {BoneNameFromID(Convert.ToUInt16(const_rotation_panel.bone_index1_Box.Text))}";
            }
        }

        private void Rotation_Index1_Info_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (rotation_panel.bone_index_Box.Text == "0" || rotation_panel.bone_index_Box.Text == String.Empty)
            {
                rotation_panel.Rotation_Index1_Info.ToolTip = "";
            }
            else
            {
                BoneNode? node = BoneView.SelectedItem as BoneNode;
                rotation_panel.Rotation_Index1_Info.ToolTip = $"Value is currently set to: {BoneNameFromID(Convert.ToUInt16(rotation_panel.bone_index_Box.Text))}";

                if (node != null && node.Data.Bonetype == BoneType.Types["rot"])
                {
                    rotation_panel.Rotation_Index1_Info.ToolTip = $"Value is currently set to: {BoneNameFromIndex(Convert.ToUInt16(rotation_panel.bone_index_Box.Text))}";
                }
            }
        }
        private void ModifierAxis_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox val = e.Source as TextBox;
            try
            {
                selected_node.Data.ModifierAxis = Convert.ToByte(val.Text);
            }
            catch
            {
                val.Text = Convert.ToString(selected_node.Data.ModifierAxis);
            }

        }

        private void ModifierValue1_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox val = e.Source as TextBox;
            try
            {
                selected_node.Data.ModifierValue1 = Convert.ToSingle(val.Text);
            }
            catch
            {

                val.Text = Convert.ToString(selected_node.Data.ModifierValue1);
            }

        }

        private void ModifierValue2_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox val = e.Source as TextBox;
            try
            {
                selected_node.Data.ModifierValue2 = Convert.ToSingle(val.Text);
            }
            catch
            {
                val.Text = Convert.ToString(selected_node.Data.ModifierValue2);
            }

        }

        //collision view events
        private void CollisionSlot1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (collision_panel.CollisionSlot1.Text == "0")
            {
                collision_panel.CollisionSlot1_Toggle.IsChecked = false;
            }
        }

        private void CollisionSlot2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (collision_panel.CollisionSlot2.Text == "0")
            {
                collision_panel.CollisionSlot2_Toggle.IsChecked = false;
            }
        }

        private void CollisionSlot3_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (collision_panel.CollisionSlot3.Text == "0")
            {
                collision_panel.CollisionSlot3_Toggle.IsChecked = false;
            }
        }

        private void CollisionSlot4_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (collision_panel.CollisionSlot4.Text == "0")
            {
                collision_panel.CollisionSlot4_Toggle.IsChecked = false;
            }
        }

        private void SwingCollisionSlot1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (collision_panel.SwingCollisionSlot1.Text == "0")
            {
                collision_panel.SwingCollisionSlot1_Toggle.IsChecked = false;
            }
        }

        private void SwingCollisionSlot2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (collision_panel.SwingCollisionSlot2.Text == "0")
            {
                collision_panel.SwingCollisionSlot2_Toggle.IsChecked = false;
            }
        }

        private void SwingCollisionSlot3_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            if (collision_panel.SwingCollisionSlot3.Text == "0")
            {
                collision_panel.SwingCollisionSlot3_Toggle.IsChecked = false;
            }
        }

        private void SwingCollisionSlot4_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (collision_panel.SwingCollisionSlot4.Text == "0")
            {
                collision_panel.SwingCollisionSlot4_Toggle.IsChecked = false;
            }
        }

        private void SwingCollisonSlot1_Info_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (collision_panel.SwingCollisionSlot1.Text == "0" || collision_panel.SwingCollisionSlot1.Text == String.Empty)
            {
                collision_panel.SwingCollisonSlot1_Info.ToolTip = "";
            }
            else
            {
                try
                {
                    collision_panel.SwingCollisonSlot1_Info.ToolTip = $"Slot is currently set to: {BoneNameFromIndex(Convert.ToUInt16(collision_panel.SwingCollisionSlot1.Text))}";
                }
                catch
                {
                    collision_panel.SwingCollisonSlot1_Info.ToolTip = "ERROR";
                }
            }
        }

        private void SwingCollisonSlot2_Info_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (collision_panel.SwingCollisionSlot2.Text == "0" || collision_panel.SwingCollisionSlot2.Text == String.Empty)
            {
                collision_panel.SwingCollisonSlot2_Info.ToolTip = "";
            }
            else
            {
                try
                {
                    collision_panel.SwingCollisonSlot2_Info.ToolTip = $"Slot is currently set to: {BoneNameFromIndex(Convert.ToUInt16(collision_panel.SwingCollisionSlot2.Text))}";
                }
                catch
                {
                    collision_panel.SwingCollisonSlot2_Info.ToolTip = "ERROR";
                }
            }
        }

        private void SwingCollisonSlot3_Info_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (collision_panel.SwingCollisionSlot3.Text == "0" || collision_panel.SwingCollisionSlot3.Text == String.Empty)
            {
                collision_panel.SwingCollisonSlot3_Info.ToolTip = "";
            }
            else
            {
                try
                {
                    collision_panel.SwingCollisonSlot3_Info.ToolTip = $"Slot is currently set to: {BoneNameFromIndex(Convert.ToUInt16(collision_panel.SwingCollisionSlot3.Text))}";
                }
                catch
                {
                    collision_panel.SwingCollisonSlot3_Info.ToolTip = "ERROR";
                }
            }
        }

        private void SwingCollisonSlot4_Info_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (collision_panel.SwingCollisionSlot4.Text == "0" || collision_panel.SwingCollisionSlot4.Text == String.Empty)
            {
                collision_panel.SwingCollisonSlot4_Info.ToolTip = "";
            }
            else
            {
                try
                {
                    collision_panel.SwingCollisonSlot4_Info.ToolTip = $"Slot is currently set to: {BoneNameFromIndex(Convert.ToUInt16(collision_panel.SwingCollisionSlot4.Text))}";
                }
                catch
                {
                    collision_panel.SwingCollisonSlot4_Info.ToolTip = "ERROR";
                }
            }
        }

        private void CollisonSlot1_Info_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (collision_panel.CollisionSlot1.Text == "0" || collision_panel.CollisionSlot1.Text == String.Empty)
            {
                collision_panel.CollisonSlot1_Info.ToolTip = "";
            }
            else
            {
                try
                {
                    collision_panel.CollisonSlot1_Info.ToolTip = $"Slot is currently set to: {BoneNameFromIndex(Convert.ToUInt16(collision_panel.CollisionSlot1.Text))}";
                }
                catch
                {
                    collision_panel.CollisonSlot1_Info.ToolTip = "ERROR";
                }
            }


        }

        private void CollisonSlot2_Info_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (collision_panel.CollisionSlot2.Text == "0" || collision_panel.CollisionSlot2.Text == String.Empty)
            {
                collision_panel.CollisonSlot2_Info.ToolTip = "";
            }
            else
            {
                try
                {
                    collision_panel.CollisonSlot2_Info.ToolTip = $"Slot is currently set to: {BoneNameFromIndex(Convert.ToUInt16(collision_panel.CollisionSlot2.Text))}";
                }
                catch
                {
                    collision_panel.CollisonSlot2_Info.ToolTip = "ERROR";
                }
            }

        }

        private void CollisonSlot3_Info_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (collision_panel.CollisionSlot3.Text == "0" || collision_panel.CollisionSlot3.Text == String.Empty)
            {
                collision_panel.CollisonSlot3_Info.ToolTip = "";
            }
            else
            {
                try
                {
                    collision_panel.CollisonSlot3_Info.ToolTip = $"Slot is currently set to: {BoneNameFromIndex(Convert.ToUInt16(collision_panel.CollisionSlot3.Text))}";
                }
                catch
                {
                    collision_panel.CollisonSlot3_Info.ToolTip = "ERROR";
                }
            }

        }

        private void CollisonSlot4_Info_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (collision_panel.CollisionSlot4.Text == "0" || collision_panel.CollisionSlot4.Text == String.Empty)
            {
                collision_panel.CollisonSlot4_Info.ToolTip = "";
            }
            else
            {
                try
                {
                    collision_panel.CollisonSlot4_Info.ToolTip = $"Slot is currently set to: {BoneNameFromIndex(Convert.ToUInt16(collision_panel.CollisionSlot4.Text))}";
                }
                catch
                {
                    collision_panel.CollisonSlot4_Info.ToolTip = "ERROR";
                }
            }

        }

        private void SwingCollisionSlot1_Toggle_Checked(object sender, RoutedEventArgs e)
        {
            if (selected_node.Data.SwingCount < 4)
            {
                selected_node.Data.SwingCount += 1;
                Trace.WriteLine(selected_node.Data.SwingCount.ToString());
            }
            collision_panel.SwingCollisionSlot1_Toggle.ToolTip = "Disable Slot 1";
        }

        private void SwingCollisionSlot2_Toggle_Checked(object sender, RoutedEventArgs e)
        {
            if (selected_node.Data.SwingCount < 4)
            {
                selected_node.Data.SwingCount += 1;
                Trace.WriteLine(selected_node.Data.SwingCount.ToString());
            }
            collision_panel.SwingCollisionSlot2_Toggle.ToolTip = "Disable Slot 2";
        }

        private void SwingCollisionSlot3_Toggle_Checked(object sender, RoutedEventArgs e)
        {
            if (selected_node.Data.SwingCount < 4)
            {
                selected_node.Data.SwingCount += 1;
                Trace.WriteLine(selected_node.Data.SwingCount.ToString());
            }
            collision_panel.SwingCollisionSlot3_Toggle.ToolTip = "Disable Slot 3";
        }

        private void SwingCollisionSlot4_Toggle_Checked(object sender, RoutedEventArgs e)
        {
            if (selected_node.Data.SwingCount < 4)
            {
                selected_node.Data.SwingCount += 1;
                Trace.WriteLine(selected_node.Data.SwingCount.ToString());
            }
            collision_panel.SwingCollisionSlot4_Toggle.ToolTip = "Disable Slot 4";
        }

        private void SwingCollisionSlot1_Toggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (selected_node.Data.SwingCount > 0)
            {
                selected_node.Data.SwingCount -= 1;
                Trace.WriteLine(selected_node.Data.SwingCount.ToString());
            }
            collision_panel.SwingCollisionSlot1_Toggle.ToolTip = "Enable Slot 1";
            collision_panel.SwingCollisionSlot2_Toggle.IsChecked = false;
            collision_panel.SwingCollisionSlot3_Toggle.IsChecked = false;
            collision_panel.SwingCollisionSlot4_Toggle.IsChecked = false;
        }

        private void SwingCollisionSlot2_Toggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (selected_node.Data.SwingCount > 0)
            {
                selected_node.Data.SwingCount -= 1;
                Trace.WriteLine(selected_node.Data.SwingCount.ToString());
            }
            collision_panel.SwingCollisionSlot2_Toggle.ToolTip = "Enable Slot 2";
            collision_panel.SwingCollisionSlot3_Toggle.IsChecked = false;
            collision_panel.SwingCollisionSlot4_Toggle.IsChecked = false;
        }

        private void SwingCollisionSlot3_Toggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (selected_node.Data.SwingCount > 0)
            {
                selected_node.Data.SwingCount -= 1;
                Trace.WriteLine(selected_node.Data.SwingCount.ToString());
            }
            collision_panel.SwingCollisionSlot3_Toggle.ToolTip = "Enable Slot 3";
            collision_panel.SwingCollisionSlot4_Toggle.IsChecked = false;
        }

        private void SwingCollisionSlot4_Toggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (selected_node.Data.SwingCount > 0)
            {
                selected_node.Data.SwingCount -= 1;
                Trace.WriteLine(selected_node.Data.SwingCount.ToString());
            }
            collision_panel.SwingCollisionSlot4_Toggle.ToolTip = "Enable Slot 4";
        }

        private void SwingCollisionSlot1_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (collision_panel.SwingCollisionSlot1.Text == "")
                {
                    collision_panel.SwingCollisionSlot1_Toggle.IsChecked = false;
                }
                selected_node.Data.SwingCollisionList[0] = Convert.ToUInt16(collision_panel.SwingCollisionSlot1.Text);
            }
            catch
            {

                collision_panel.SwingCollisionSlot1.Text = Convert.ToString(selected_node.Data.SwingCollisionList[0]);
            }

        }

        private void SwingCollisionSlot2_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (collision_panel.SwingCollisionSlot2.Text == "")
                {
                    collision_panel.SwingCollisionSlot2_Toggle.IsChecked = false;
                }
                selected_node.Data.SwingCollisionList[1] = Convert.ToUInt16(collision_panel.SwingCollisionSlot2.Text);
            }
            catch
            {

                collision_panel.SwingCollisionSlot2.Text = Convert.ToString(selected_node.Data.SwingCollisionList[1]);
            }
        }

        private void SwingCollisionSlot3_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (collision_panel.SwingCollisionSlot3.Text == "")
                {
                    collision_panel.SwingCollisionSlot3_Toggle.IsChecked = false;
                }
                selected_node.Data.SwingCollisionList[2] = Convert.ToUInt16(collision_panel.SwingCollisionSlot3.Text);
            }
            catch
            {

                collision_panel.SwingCollisionSlot3.Text = Convert.ToString(selected_node.Data.SwingCollisionList[2]);
            }
        }

        private void SwingCollisionSlot4_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (collision_panel.SwingCollisionSlot4.Text == "")
                {
                    collision_panel.SwingCollisionSlot4_Toggle.IsChecked = false;
                }
                selected_node.Data.SwingCollisionList[3] = Convert.ToUInt16(collision_panel.SwingCollisionSlot4.Text);
            }
            catch
            {

                collision_panel.SwingCollisionSlot4.Text = Convert.ToString(selected_node.Data.SwingCollisionList[3]);
            }
        }

        private void CollisionSlot1_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (collision_panel.CollisionSlot1.Text == "")
                {
                    collision_panel.CollisionSlot1_Toggle.IsChecked = false;
                }
                selected_node.Data.CollisionList[0] = Convert.ToUInt16(collision_panel.CollisionSlot1.Text);
            }
            catch
            {

                collision_panel.CollisionSlot1.Text = Convert.ToString(selected_node.Data.CollisionList[0]);
            }
        }

        private void CollisionSlot2_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (collision_panel.CollisionSlot2.Text == "")
                {
                    collision_panel.CollisionSlot2_Toggle.IsChecked = false;
                }
                selected_node.Data.CollisionList[1] = Convert.ToUInt16(collision_panel.CollisionSlot2.Text);
            }
            catch
            {

                collision_panel.CollisionSlot2.Text = Convert.ToString(selected_node.Data.CollisionList[1]);
            }
        }

        private void CollisionSlot3_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (collision_panel.CollisionSlot3.Text == "")
                {
                    collision_panel.CollisionSlot3_Toggle.IsChecked = false;
                }
                selected_node.Data.CollisionList[2] = Convert.ToUInt16(collision_panel.CollisionSlot3.Text);
            }
            catch
            {

                collision_panel.CollisionSlot3.Text = Convert.ToString(selected_node.Data.CollisionList[2]);
            }
        }

        private void CollisionSlot4_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (collision_panel.CollisionSlot4.Text == "")
                {
                    collision_panel.CollisionSlot4_Toggle.IsChecked = false;
                }
                selected_node.Data.CollisionList[3] = Convert.ToUInt16(collision_panel.CollisionSlot4.Text);
            }
            catch
            {

                collision_panel.CollisionSlot4.Text = Convert.ToString(selected_node.Data.CollisionList[3]);
            }
        }

        private void CollisionSlot1_Toggle_Checked(object sender, RoutedEventArgs e)
        {
            if (selected_node.Data.CollisionCount < 4)
            {
                selected_node.Data.CollisionCount += 1;
                Trace.WriteLine(selected_node.Data.CollisionCount.ToString());
            }
            collision_panel.CollisionSlot1_Toggle.ToolTip = "Disable Slot 1";
        }

        private void CollisionSlot2_Toggle_Checked(object sender, RoutedEventArgs e)
        {
            if (selected_node.Data.CollisionCount < 4)
            {
                selected_node.Data.CollisionCount += 1;
                Trace.WriteLine(selected_node.Data.CollisionCount.ToString());
            }
            collision_panel.CollisionSlot2_Toggle.ToolTip = "Disable Slot 2";
        }

        private void CollisionSlot3_Toggle_Checked(object sender, RoutedEventArgs e)
        {
            if (selected_node.Data.CollisionCount < 4)
            {
                selected_node.Data.CollisionCount += 1;
                Trace.WriteLine(selected_node.Data.CollisionCount.ToString());
            }
            collision_panel.CollisionSlot3_Toggle.ToolTip = "Disable Slot 3";
        }

        private void CollisionSlot4_Toggle_Checked(object sender, RoutedEventArgs e)
        {
            if (selected_node.Data.CollisionCount < 4)
            {
                selected_node.Data.CollisionCount += 1;
                Trace.WriteLine(selected_node.Data.CollisionCount.ToString());
            }
            collision_panel.CollisionSlot4_Toggle.ToolTip = "Disable Slot 4";
        }

        private void CollisionSlot1_Toggle_Unchecked(object sender, RoutedEventArgs e)
        {
            collision_panel.CollisionSlot2_Toggle.IsChecked = false;
            if (selected_node.Data.CollisionCount > 0)
            {
                selected_node.Data.CollisionCount -= 1;
                Trace.WriteLine(selected_node.Data.CollisionCount.ToString());
            }
            collision_panel.CollisionSlot1_Toggle.ToolTip = "Enable Slot 1";
        }
        private void CollisionSlot2_Toggle_Unchecked(object sender, RoutedEventArgs e)
        {
            collision_panel.CollisionSlot3_Toggle.IsChecked = false;
            if (selected_node.Data.CollisionCount > 0)
            {
                selected_node.Data.CollisionCount -= 1;
                Trace.WriteLine(selected_node.Data.CollisionCount.ToString());
            }
            collision_panel.CollisionSlot2_Toggle.ToolTip = "Enable Slot 2";
        }
        private void CollisionSlot3_Toggle_Unchecked(object sender, RoutedEventArgs e)
        {
            collision_panel.CollisionSlot4_Toggle.IsChecked = false;
            if (selected_node.Data.CollisionCount > 0)
            {
                selected_node.Data.CollisionCount -= 1;
                Trace.WriteLine(selected_node.Data.CollisionCount.ToString());
            }
            collision_panel.CollisionSlot3_Toggle.ToolTip = "Enable Slot 3";
        }
        private void CollisionSlot4_Toggle_Unchecked(object sender, RoutedEventArgs e)
        {
            if (selected_node.Data.CollisionCount > 0)
            {
                selected_node.Data.CollisionCount -= 1;
                Trace.WriteLine(selected_node.Data.CollisionCount.ToString());
            }
            collision_panel.CollisionSlot4_Toggle.ToolTip = "Enable Slot 4";
        }
        #endregion
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {



        }



        private void CommandBindingNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBindingNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NewDialog.Visibility = Visibility.Visible;
            Trace.WriteLine("Message");
        }

        private void CommandBindingOpen_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBindingOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NewDialog.Visibility = Visibility.Visible;

        }
        private void LoopTree(BoneNode _parent, BoneNode node)
        {
            for (int i = 0; i < _parent.Items.Count; i++)
            {
                if (_parent.Data.Index == node.Data.ParentId)
                {
                    _parent.Items.Add(node);
                }
                else
                {
                    _parent.IsExpanded = true;
                    if (_parent.Items.Count > 0)
                    {
                        BoneNode? child = _parent.Items[i] as BoneNode;
                        string name = child.Data.Name;
                        LoopTree(child, node);
                    }
                }
            }

        }
        private void NewFromNMDBtn_Click(object sender, RoutedEventArgs e)
        {
            NewDialog.Visibility = Visibility.Visible;

        }

        private void NewFromTemplateBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RefreshSearch()
        {
            BoneView.Items.Clear();
            BoneTools_Panel.Visibility = Visibility.Collapsed;
            List<BoneNode> filtered = new();
            if (bones.RootBones != null || bones.Bones != null)
            {
                if (BoneSearch.Text == String.Empty)
                {

                    foreach (BoneNode bone in bones.Item1)
                    {
                        BoneView.Items.Add(bone);
                    }
                }
                else
                {

                    foreach (BoneNode bone in bones.Item2)
                    {
                        ComboBoxItem selected_item = BoneFilter.SelectedItem as ComboBoxItem;
                        if (selected_item.Content == "By Name")
                        {
                            if (bone.Data.Name.Contains(BoneSearch.Text, StringComparison.OrdinalIgnoreCase))
                            {
                                filtered.Add(new BoneNode(bone.Data));
                            }
                        }

                        if (selected_item.Content == "By Index")
                        {
                            String index = bone.Data.Index.ToString();
                            if (index == BoneSearch.Text)
                            {
                                filtered.Add(new BoneNode(bone.Data));
                            }
                        }

                        if (selected_item.Content == "By Parent Index")
                        {
                            String index = bone.Data.ParentId.ToString();
                            if (index == BoneSearch.Text)
                            {
                                filtered.Add(new BoneNode(bone.Data));
                            }
                        }

                        if (selected_item.Content == "By Type")
                        {
                            if (bone.Data.Bonetype.Name.Contains(BoneSearch.Text, StringComparison.OrdinalIgnoreCase))
                            {
                                filtered.Add(new BoneNode(bone.Data));
                            }
                        }

                        if (selected_item.Content == "By Type Category")
                        {
                            if (bone.Data.Bonetype.Category.ToString().Contains(BoneSearch.Text, StringComparison.OrdinalIgnoreCase))
                            {
                                filtered.Add(new BoneNode(bone.Data));
                            }
                        }




                    }
                    foreach (var bone in filtered)
                    {
                        //BoneNode item = new();
                        //item.Foreground = System.Windows.Media.Brushes.White;
                        //item.Header = bone;
                        BoneView.Items.Add(bone);
                    }

                }
                if (BoneView.Items.Count > 0)
                {

                }
            }


        }

        private string BoneNameFromIndex(ushort input_index)
        {
            string returned_name = "MUNE1";
            foreach (var bone in bones.Bones)
            {
                if (bone.Data.Index == input_index)
                {
                    returned_name = bone.Data.Name;
                }
            }
            return returned_name;
        }

        private string BoneNameFromID(ushort input_id)
        {
            string returned_name = "MUNE1";
            foreach (var bone in bones.Bones)
            {
                if (bone.Data.BoneId == input_id && bone.Data.Bonetype == BoneType.Types["standard"])
                {
                    returned_name = bone.Data.Name;
                }
            }
            return returned_name;
        }
        private void BoneSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshSearch();
        }

        private void BoneFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshSearch();
        }

        private void NewDialog_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {


            if (e.OldValue.Equals(false))
            {
                NewDialog.BeginAnimation(OpacityProperty, opacity_anim);
            }
            if (e.NewValue.Equals(false))
            {

                NewDialog.BeginAnimation(OpacityProperty, opacity_anim2);
            }


        }

        private void ProcessBtn_Click(object sender, RoutedEventArgs e)
        {
            InitializeTree(NMDBox.Text, (bool)KeepEyeBox.IsChecked);
        }
        private void InitializeTree(string nmd_path, bool keep_eye)
        {
            string filename = Path.GetFileName(nmd_path);
            bones = NMDUtil.ParseNMD(nmd_path, keep_eye,new(),new());
            if (BoneView.Items.Count > 0 )
            {
                BoneView.Items.Clear();
                BoneTools_Panel.Visibility = Visibility.Collapsed;
            }
            
            foreach (BoneNode bone in bones.RootBones)
            {
                BoneView.Items.Add(bone);
                //if (bone.Data.ParentId == 65535)
                //{

                //}

            }
            foreach (BoneNode bone in bones.Bones)
            {
                Trace.WriteLine(bone.Data.Name);

            }
            BoneSearch.IsEnabled = true;
            NewDialog.BeginAnimation(OpacityProperty, opacity_anim2);
            NewDialog.Visibility = Visibility.Hidden;
            if (BoneSearch.Text != String.Empty)
            {
                RefreshSearch();
            }
            Title = $"{filename} - {base_title}";
            //Title = $"{filename} [{nmd_path}] - {base_title}"
        }

        private void NMDBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (NMDBox.Text == String.Empty)
            {
                ProcessBtn.IsEnabled = false;
            }
            else
            {
                ProcessBtn.IsEnabled = true;
            }

        }

        private void CommandBindingNMDTemplate_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

        }

        private void CommandBindingNMDTemplate_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void NMDCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            NewDialog.Visibility = Visibility.Hidden;
        }

        private void CommandBindingSearch_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBindingSearch_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            BoneSearch.Focus();
        }

        private void CommandBindingExport_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (bones.Item2 != null)
            {
                e.CanExecute = true;
            }

        }

        private void CommandBindingExport_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ExportDialog.Visibility = Visibility.Visible;
        }

        private void ExportProcessBtn_Click(object sender, RoutedEventArgs e)
        {
            string export = NMDUtil.ExportNMD(bones.Item2, ExportBox.Text);
            if (export.Contains("success"))
            {
                ExportDialog.Visibility = Visibility.Hidden;
            }
            else
            {
                ErrorBox.Text = export;
            }
        }

        private void ExportCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            ExportDialog.Visibility = Visibility.Hidden;
            ErrorBox.Text = "";
        }

        private void ExportBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ExportBox.Text == String.Empty)
            {
                ExportProcessBtn.IsEnabled = false;
            }
            else
            {
                ExportProcessBtn.IsEnabled = true;
            }
        }

        private void ExportDialog_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue.Equals(false))
            {
                ExportDialog.BeginAnimation(OpacityProperty, opacity_anim);
            }
            if (e.NewValue.Equals(false))
            {

                ExportDialog.BeginAnimation(OpacityProperty, opacity_anim2);
            }
        }

        private void ExportNMDBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog nmd_dialog = new();
            nmd_dialog.Filter = "NMD Files|*.nmd";
            nmd_dialog.Title = "Select Export Path";
            if ((bool)nmd_dialog.ShowDialog() == true)
            {
                ExportBox.Text = nmd_dialog.FileName;
            }
        }

        private void NewFromJsonBtn_Click(object sender, RoutedEventArgs e)
        {
            NewJsonDialog.Visibility = Visibility.Visible;
        }

        private void ExportJsonBtn_Click(object sender, RoutedEventArgs e)
        {
            ExportJsonDialog.Visibility = Visibility.Visible;
        }

        private void NewJsonDialog_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue.Equals(false))
            {
                NewJsonDialog.BeginAnimation(OpacityProperty, opacity_anim);
            }
            if (e.NewValue.Equals(false))
            {
                NewJsonDialog.BeginAnimation(OpacityProperty, opacity_anim2);
            }
        }

        private void JsonImportProcessBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filename = Path.GetFileName(JsonImportBox.Text);
                bones = NMDUtil.ParseJson(JsonImportBox.Text,new(),new());
                if (BoneView.Items.Count > 0)
                {
                    BoneView.Items.Clear();
                    BoneTools_Panel.Visibility = Visibility.Collapsed;
                }
                
                foreach (BoneNode bone in bones.RootBones)
                {
                    BoneView.Items.Add(bone);
                }
                BoneSearch.IsEnabled = true;
                NewJsonDialog.BeginAnimation(OpacityProperty, opacity_anim2);
                NewJsonDialog.Visibility = Visibility.Hidden;
                if (BoneSearch.Text != String.Empty)
                {
                    RefreshSearch();
                }
                Title = $"{filename} - {base_title}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading JSON: " + ex.Message);
            }
        }

        private void JsonImportBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (JsonImportBox.Text == String.Empty)
            {
                JsonImportProcessBtn.IsEnabled = false;
            }
            else
            {
                JsonImportProcessBtn.IsEnabled = true;
            }
        }

        private void JsonImportCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            NewJsonDialog.Visibility = Visibility.Hidden;
        }

        private void JsonImportBrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new();
            dialog.Filter = "JSON Files|*.json";
            dialog.Title = "Select JSON file";
            if ((bool)dialog.ShowDialog() == true)
            {
                JsonImportBox.Text = dialog.FileName;
            }
        }

        private void ExportJsonDialog_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue.Equals(false))
            {
                ExportJsonDialog.BeginAnimation(OpacityProperty, opacity_anim);
            }
            if (e.NewValue.Equals(false))
            {
                ExportJsonDialog.BeginAnimation(OpacityProperty, opacity_anim2);
            }
        }

        private void ExportJsonProcessBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string export = NMDUtil.ExportJson(bones.Item2, ExportJsonBox.Text);
                if (export.Contains("SUCCESS"))
                {
                    ExportJsonDialog.Visibility = Visibility.Hidden;
                }
                else
                {
                    ExportJsonErrorBox.Text = export;
                }
            }
            catch (Exception ex)
            {
                ExportJsonErrorBox.Text = "Error: " + ex.Message;
            }
        }

        private void ExportJsonCancelBtn_Click(object sender, RoutedEventArgs e)
        {
            ExportJsonDialog.Visibility = Visibility.Hidden;
            ExportJsonErrorBox.Text = "";
        }

        private void ExportJsonBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ExportJsonBox.Text == String.Empty)
            {
                ExportJsonProcessBtn.IsEnabled = false;
            }
            else
            {
                ExportJsonProcessBtn.IsEnabled = true;
            }
        }

        private void ExportJsonBrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new();
            dialog.Filter = "JSON Files|*.json";
            dialog.Title = "Select Export Path";
            if ((bool)dialog.ShowDialog() == true)
            {
                ExportJsonBox.Text = dialog.FileName;
            }
        }

        private void BoneView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                if (filePaths != null && filePaths.Length > 0)
                {
                    string fileExtension = Path.GetExtension(filePaths[0]).ToLower();
                    if (fileExtension == ".nmd" || fileExtension == ".json")
                    {
                        DropIndicator.Visibility = Visibility.Visible;
                    }
                }
            }
            Trace.WriteLine(Keyboard.Modifiers);
        }

        private void BoneView_DragLeave(object sender, DragEventArgs e)
        {
            DropIndicator.BeginAnimation(OpacityProperty, opacity_anim2);
            DropIndicator.Visibility = Visibility.Hidden;
        }

        private void BoneView_Drop(object sender, DragEventArgs e)
        {
            string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (filePaths != null && filePaths.Length > 0)
            {
                string fileExtension = Path.GetExtension(filePaths[0]).ToLower();
                if (fileExtension == ".json")
                {
                    try
                    {
                        string filename = Path.GetFileName(filePaths[0]);
                        bones = NMDUtil.ParseJson(filePaths[0],new(), new());
                        if (BoneView.Items.Count > 0)
                        {
                            BoneView.Items.Clear();
                            BoneTools_Panel.Visibility = Visibility.Collapsed;
                        }
                        
                        foreach (BoneNode bone in bones.RootBones)
                        {
                            BoneView.Items.Add(bone);
                        }
                        BoneSearch.IsEnabled = true;
                        if (BoneSearch.Text != String.Empty)
                        {
                            RefreshSearch();
                        }
                        Title = $"{filename} (JSON) - {base_title}";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading JSON: " + ex.Message);
                    }
                }
                else if (fileExtension == ".nmd")
                {
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                        InitializeTree(filePaths[0], true);
                    else
                    {
                        InitializeTree(filePaths[0], false);
                    }
                }
            }
            DropIndicator.BeginAnimation(OpacityProperty, opacity_anim2);
            DropIndicator.Visibility = Visibility.Hidden;
        }

        private void DropIndicator_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue.Equals(false))
            {
               DropIndicator.BeginAnimation(OpacityProperty, opacity_anim);
            }
            if (e.OldValue.Equals(true))
            {

                DropIndicator.BeginAnimation(OpacityProperty, opacity_anim2);
            }
            
        }

        private void BoneView_ContextMenuClosing(object sender, ContextMenuEventArgs e)
        {
            selected_node.Menu = false;
        }

        

        private void RenameStandard_Click(object sender, RoutedEventArgs e)
        {
            //ContextMenu? menu;
            if (selected_node.Data.Bonetype.Category == BoneTypeCategory.Swing)
            {
                BoneOption_Btn.ContextMenu = BoneOption_Btn.Resources["SwingMenu"] as ContextMenu;
            }
            else
            {
                BoneOption_Btn.ContextMenu = BoneOption_Btn.Resources["StandardMenu"] as ContextMenu;
            }
            ContextMenu menu = BoneOption_Btn.ContextMenu;
            MenuItem? rename_item;
            StackPanel? rename_panel;
            TextBox? rename_text = null;
            

            if (menu != null)
            {
                rename_item = menu.Items.GetItemAt(0) as MenuItem;
                rename_panel = rename_item.Items.GetItemAt(0) as StackPanel;
                rename_text = rename_panel.Children[1] as TextBox;
                
                

            }
            
            selected_node.Data.Name = rename_text.Text;
            selected_node.ToolTip = rename_text.Text;
            selected_node.Header = $"{selected_node.Data.Index}: {selected_node.Data.Name}";
            if (BoneSearch.Text != string.Empty)
            {
                RefreshSearch();
            }
            menu.IsOpen = false;
            
        }

        private void BoneOption_Btn_Click(object sender, RoutedEventArgs e)
        {
            BoneNode node = BoneView.SelectedItem as BoneNode;
            if (selected_node.Data.Bonetype.Category == BoneTypeCategory.Swing)
            {
                BoneOption_Btn.ContextMenu = BoneOption_Btn.Resources["SwingMenu"] as ContextMenu;
            }
            else
            {
                BoneOption_Btn.ContextMenu = BoneOption_Btn.Resources["StandardMenu"] as ContextMenu;
            }
            ContextMenu? menu = BoneOption_Btn.ContextMenu;
            MenuItem? rename_item;
            StackPanel? rename_panel;
            TextBox? rename_text = null;
            MenuItem? mirror_item;

            if (menu != null)
            {
                rename_item = menu.Items.GetItemAt(0) as MenuItem;
                rename_panel = rename_item.Items.GetItemAt(0) as StackPanel;
                rename_text = rename_panel.Children[1] as TextBox;
                mirror_item = menu.Items.GetItemAt(1) as MenuItem;
                string[] mirrorValues = { "_L", "_l_", "_R", "_r_" };
                foreach (string val in mirrorValues)
                {
                    if (node.Data.Name.Contains(val))
                    {
                        mirror_item.IsEnabled = true;
                        break;
                    }
                    else
                    {
                        mirror_item.IsEnabled = false;
                    }
                }

            }
            
            rename_text.Text = node.Data.Name;
            BoneOption_Btn.ContextMenu.IsOpen = true;
            
            

            
            Trace.WriteLine(BoneOption_Btn.ContextMenu.Name);
            
            
           
            
        }

        private void BoneOption_Btn_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}

