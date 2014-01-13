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
using System.ComponentModel;

using LAIR.ResourceAPIs.WordNet;
using LAIR.Collections.Generic;
using System.IO;

namespace OST_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private WordNetEngine _wordNetEngine;

        private BindingList<SynSetListItem> synsetsFound = new BindingList<SynSetListItem>();

        public MainWindow()
        {
            InitializeComponent();

            // create wordnet engine (use disk-based retrieval by default)
            string root = Directory.GetDirectoryRoot(".");
            _wordNetEngine = new WordNetEngine(root + @"\Users\Martin\Documents\GitHub\OST-grupa-13\OST_App\dict\", false);

            synsetsFoundListBox.ItemsSource = synsetsFound;
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

        private void findSynsets()
        {
            synsetsFound.Clear();

            if (word.Text != "")
            {
                synsetsFoundMsg.Content = "Searching...";

                // Find synsets
                Set<SynSet> synsets = null;
                try
                {
                    synsets = _wordNetEngine.GetSynSets(word.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:  " + ex); return;
                }

                // Populate the list of found synsets
                foreach (SynSet synset in synsets)
                {
                    StringBuilder words = new StringBuilder();
                    bool prependComma = false;
                    foreach (string w in synset.Words)
                    {
                        words.Append((prependComma ? ", " : "") + w);
                        prependComma = true;
                    }

                    synsetsFound.Add(new SynSetListItem { Synset = synset, Words = words.ToString(), Desc = synset.Gloss });
                }

                synsetsFoundMsg.Content = synsets.Count + " found";
            }
            else
            {
                synsetsFoundMsg.Content = "";
            }
        }

        /// <summary>
        /// Contains synset and information needed to represent it in ListBox.
        /// </summary>
        class SynSetListItem
        {
            public SynSet Synset { get; set; }
            public String Words { get; set; }
            public String Desc { get; set; }
        }

        private void word_TextChanged(object sender, TextChangedEventArgs e)
        {
            findSynsets();
        }
    }
}
