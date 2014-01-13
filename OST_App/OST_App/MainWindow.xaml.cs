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
            findSynsets(); // For initialization of GUI elements
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

        /// <summary>
        /// Finds synsets of word from text box and displays those synsets or appropriate message.
        /// </summary>
        private void findSynsets()
        {
            synsetsFound.Clear();
            synsetsFoundListBox.Visibility = System.Windows.Visibility.Hidden;
            synsetsFoundMsg.Visibility = System.Windows.Visibility.Visible;

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

                if (synsets.Count > 0)
                {
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

                    synsetsFoundMsg.Visibility = System.Windows.Visibility.Hidden;
                    synsetsFoundListBox.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    synsetsFoundMsg.Content = "No synsets found";
                }
            }
            else
            {
                synsetsFoundMsg.Content = "Type word in the field above to find its synsets";
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

        private void btnAddSynset_Click(object sender, RoutedEventArgs e)
        {
            SynSetListItem selectedSynset = ((SynSetListItem)synsetsFoundListBox.SelectedItem);
            if (selectedSynset != null)
                MessageBox.Show(selectedSynset.Synset.ID);
        }

        private void synsetsFoundListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnAddSynset.IsEnabled = synsetsFoundListBox.SelectedItem != null;
        }
    }
}
