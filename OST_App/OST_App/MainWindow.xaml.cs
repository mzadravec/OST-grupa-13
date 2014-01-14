using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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
using System.Windows.Controls.Primitives;

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
        private BindingList<SynSetListItem> synsetsTagged = new BindingList<SynSetListItem>();

        private Popup tagPopup = null; // Tooltip that shows information about synset tag

        public MainWindow()
        {
            InitializeComponent();

            // create wordnet engine (use disk-based retrieval by default)
            _wordNetEngine = new WordNetEngine(@"..\..\..\dict\", false); // TODO: Set some other path, absolute (which one)?

            synsetsFoundListBox.ItemsSource = synsetsFound;
            synsetsTaggedListBox.ItemsSource = synsetsTagged;
            findSynsets(); // For initialization of GUI elements

            try
            {
                SQLiteDatabase db = new SQLiteDatabase();
                DataTable Picture;
                String query = "select id \"id\" from Picture;";
                Picture = db.GetDataTable(query);
                // looped through for some other reason
                foreach (DataRow r in Picture.Rows)
                {
                    Console.WriteLine(r["id"].ToString());
                }
            }
            catch (Exception fail) {
                Console.WriteLine(fail.Message.ToString());
            }
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
                        StringBuilder title = new StringBuilder();
                        bool prependComma = false;
                        foreach (string w in synset.Words)
                        {
                            title.Append((prependComma ? ", " : "") + w);
                            prependComma = true;
                        }
                        title.Append(" [" + synset.POS + "]");

                        synsetsFound.Add(new SynSetListItem { Synset = synset, Title = title.ToString(), Desc = synset.Gloss });
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
            public String Title { get; set; }
            public String Desc { get; set; }
        }

        private void word_TextChanged(object sender, TextChangedEventArgs e)
        {
            findSynsets();
        }

        private void btnAddSynset_Click(object sender, RoutedEventArgs e)
        {
            SynSetListItem selectedSynset = ((SynSetListItem)synsetsFoundListBox.SelectedItem);
            if (selectedSynset != null && !synsetsTagged.Contains(selectedSynset))
                synsetsTagged.Add(selectedSynset);
            synsetsFoundListBox.SelectedItem = null;
        }

        private void synsetsFoundListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnAddSynset.IsEnabled = synsetsFoundListBox.SelectedItem != null && !synsetsTagged.Contains(synsetsFoundListBox.SelectedItem);
        }

        private void labelDeleteTag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            synsetsTagged.Remove((SynSetListItem)((FrameworkElement)sender).Tag);
        }

        /// <summary>
        /// Show tooltip when mouse enters synset tag.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LabelTag_MouseEnter(object sender, MouseEventArgs e)
        {
            SynSetListItem synsetLI = (SynSetListItem)((FrameworkElement)sender).Tag;
            Popup popup = new Popup();
            popup.Child = new Border
            {
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Black,
                Child = new TextBlock
                {
                    Background = Brushes.White,
                    Text = synsetLI.Desc,
                    Padding = new Thickness(5),
                    TextWrapping = TextWrapping.Wrap,
                    FontWeight = FontWeights.Normal,
                    MaxWidth = 300
                }
            };
            popup.PlacementTarget = (UIElement)sender;
            popup.HorizontalOffset = popup.VerticalOffset = 5;
            
            popup.IsOpen = true;
            tagPopup = popup;
        }

        /// <summary>
        /// Remove tooltip when mouse leaves synset tag.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LabelTag_MouseLeave(object sender, MouseEventArgs e)
        {
            if (tagPopup != null)
            {
                tagPopup.IsOpen = false;
                tagPopup = null;
            }
        }
    }
}
