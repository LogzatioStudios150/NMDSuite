using System.Transactions;
using System.Windows.Controls;
using System.Windows.Media;

namespace NMDBase
{
    public class BoneNode : TreeViewItem
    {


        public BoneData Data { get; set; } = new();

        public bool Menu { get; set; } = false;


        public BoneNode()
        {
            Data.Name = "MUNE1";
            Header = $"{Data.Index}: {Data.Name}";
            Foreground = Brushes.White;
            ToolTip = Data.Name;
            this.MouseRightButtonUp += BoneNode_MouseRightButtonUp;
            
        }

        

        public BoneNode(BoneData data)
        {
            Data = data;
            Header = $"{Data.Index}: {Data.Name}";
            Foreground = Brushes.White;
            ToolTip = Data.Name;
            this.MouseRightButtonUp += BoneNode_MouseRightButtonUp;
        }

        public void BoneNode_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            BoneNode node = e.Source as BoneNode;
            if (node.Menu == false) 
            {
                node.Menu = true;
            }
            node.Menu = true;
            node.IsSelected = true;
        }

    }
}
