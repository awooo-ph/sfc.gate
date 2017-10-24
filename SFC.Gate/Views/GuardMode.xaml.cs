﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SFC.Gate.Views
{
    /// <summary>
    /// Interaction logic for GuardMode.xaml
    /// </summary>
    public partial class GuardMode : UserControl
    {
        public GuardMode()
        {
            
            InitializeComponent();
            DateText.Text = DateTime.Now.ToString("yy-MMM-dd ddd");
        }
    }
}