using System.Windows.Input;

namespace NMDSuite.Commands
{
    public static class Commands
    {
        public static readonly RoutedUICommand NMDTemplate = new RoutedUICommand
            (
                "NMDTemplate",
                "NMDTemplate",
                typeof(Commands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.N,ModifierKeys.Control | ModifierKeys.Shift)
                }
            );

        public static readonly RoutedUICommand Search = new RoutedUICommand
            (
                "Search",
                "Search",
                typeof(Commands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.F,ModifierKeys.Control)
                }
            );

        public static readonly RoutedUICommand Export = new RoutedUICommand
            (
                "Export",
                "Export",
                typeof(Commands),
                new InputGestureCollection()
                {
                    new KeyGesture(Key.E,ModifierKeys.Control)
                    
                }
            );
    }
}
