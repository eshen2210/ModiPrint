﻿using System;
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

namespace ModiPrint.Views.PrintViews.MaterialViews
{
    /// <summary>
    /// Interaction logic for MaterialView.xaml
    /// </summary>
    public partial class MaterialView : UserControl
    {
        public MaterialView()
        {
            InitializeComponent();
        }

        private void MaterialTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MaterialTabControl.Focus();
        }
    }
}
