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
using Microsoft.Win32;

namespace GrindToGCode
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Slicer slicer { get; set; }
        public MainWindow()
        {
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;

            slicer = new Slicer();            
            InitializeComponent();
            this.DataContext = this;
        }

        private void GenerateGCode_Click(object sender, RoutedEventArgs e)
        {
            GCode.Text = slicer.toGCode();
        }

        private void SaveGCode_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "G-Code (*.gcode)|*.gcode";
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FileName = "grind.gcode";
            if (saveFileDialog.ShowDialog() == true)
                System.IO.File.WriteAllText(saveFileDialog.FileName, GCode.Text);
        }
    }
}
