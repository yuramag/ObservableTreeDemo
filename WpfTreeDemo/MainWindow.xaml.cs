using System.Windows;

namespace WpfTreeDemo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
        
        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ((MainWindowViewModel)DataContext).SelectedItem = e.NewValue;
        }
    }
}
