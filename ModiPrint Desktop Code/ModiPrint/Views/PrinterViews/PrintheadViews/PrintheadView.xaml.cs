using System;
using System.Collections.Generic;
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

namespace ModiPrint.Views.PrinterViews.PrintheadViews
{
    /// <summary>
    /// Interaction logic for PrintheadView.xaml
    /// </summary>
    public partial class PrintheadView : UserControl
    {
        public PrintheadView()
        {
            InitializeComponent();
        }

        private void PrintheadTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PrintheadTabControl.Focus();
        }
    }
}
