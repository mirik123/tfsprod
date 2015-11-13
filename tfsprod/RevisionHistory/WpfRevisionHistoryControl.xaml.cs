using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TFSExt.ShowRevHist;

namespace tfsprod
{
    /// <summary>
    /// Interaction logic for WpfRevisionHistoryControl.xaml
    /// </summary>
    public partial class WpfRevisionHistoryControl : UserControl
    {
        public WpfRevisionHistoryControl()
        {
            InitializeComponent();
        }
    }

    public class TreeViewLineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TreeViewItem item = (TreeViewItem)value;
            ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(item);
            return ic.ItemContainerGenerator.IndexFromContainer(item) == ic.Items.Count - 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
