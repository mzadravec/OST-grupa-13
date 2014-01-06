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

namespace OST_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            Nullable<bool> result = ofd.ShowDialog();
            if (result == true)
            {
                txtBrowseFile.Text = ofd.FileName;
            }
        }

        /// <summary>
        /// On the click event of Upload button we store the image in Image control.
        /// Here for providing Source of the image control you need to create an
        /// object of the BitmapImage class and provide all the functionality.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            if (txtBrowseFile.Text.Trim().Length != 0)
            {
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(txtBrowseFile.Text.Trim(), UriKind.Relative);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                image1.Source = src;
            }
        }
 
    }
}
