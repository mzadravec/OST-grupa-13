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

        private BindingList<SynSetListItem> _synsetsFound = new BindingList<SynSetListItem>(); // Used for list box
        private BindingList<SynSetListItem> _synsetsTagged = new BindingList<SynSetListItem>(); // Used for list box

        private Popup _tagPopup = null; // Tooltip that shows information about synset tag

        private Picture _currentPicture;

        public MainWindow()
        {
            InitializeComponent();

            // create wordnet engine (use disk-based retrieval by default)
            _wordNetEngine = new WordNetEngine(@"..\..\..\dict\", false); // TODO: Set some other path, absolute (which one)?

            // bind list box with their lists
            synsetsFoundListBox.ItemsSource = _synsetsFound;
            synsetsTaggedListBox.ItemsSource = _synsetsTagged;

            findSynsets(); // For initialization of GUI elements

            // show first picture
            _currentPicture = Picture.GetFirstPicture();
            showPicture(_currentPicture);
        }

        /// <summary>
        /// Show picture in picture container and also shows tagged synsets.
        /// </summary>
        /// <param name="picture"></param>
        private void showPicture(Picture picture)
        {
            // Display picture in picture container
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.UriSource = new Uri(picture.path, UriKind.Relative);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            pictureContainer.Source = src;

            // Show tagged synsets
            _synsetsTagged.Clear();
            List<String> synsetIDs = picture.getSynsets();
            foreach (var ID in synsetIDs)
                _synsetsTagged.Add(new SynSetListItem(_wordNetEngine.GetSynSet(ID)));
        }

        /// <summary>
        /// Finds synsets of word from text box and displays those synsets or appropriate message.
        /// </summary>
        private void findSynsets()
        {
            _synsetsFound.Clear();
            synsetsFoundListBox.Visibility = System.Windows.Visibility.Hidden;
            synsetsFoundMsg.Visibility = System.Windows.Visibility.Visible;

            if (word.Text != "")
            {
                synsetsFoundMsg.Content = "Searching...";

                // Find synsets
                Set<SynSet> synsets = null;
                try
                {
                    synsets = _wordNetEngine.GetSynSets(word.Text.Trim());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error:  " + ex); return;
                }

                if (synsets.Count > 0)
                {
                    // Populate the list of found synsets
                    foreach (SynSet synset in synsets)
                        _synsetsFound.Add(new SynSetListItem (synset));

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

        private void word_TextChanged(object sender, TextChangedEventArgs e)
        {
            findSynsets();
        }

        /// <summary>
        /// Adds selected synset to tagged synsets and updates database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddSynset_Click(object sender, RoutedEventArgs e)
        {
            SynSetListItem selectedSynset = ((SynSetListItem)synsetsFoundListBox.SelectedItem);
            if (selectedSynset != null)
            {
                if (!listContainsSynset(_synsetsTagged, selectedSynset))
                    _synsetsTagged.Add(selectedSynset);
            }
            synsetsFoundListBox.SelectedItem = null;
            _currentPicture.addSynset(selectedSynset.Synset.ID);
        }

        /// <summary>
        /// Removes clicked synset from tagged synsets and updates database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void labelDeleteTag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SynSetListItem synsetItem = (SynSetListItem)((FrameworkElement)sender).Tag;
            _synsetsTagged.Remove(synsetItem);
            _currentPicture.removeSynset(synsetItem.Synset.ID);
        }

        private void synsetsFoundListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnAddSynset.IsEnabled = synsetsFoundListBox.SelectedItem != null && !listContainsSynset(_synsetsTagged, (SynSetListItem)synsetsFoundListBox.SelectedItem);
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
            _tagPopup = popup;
        }

        /// <summary>
        /// Remove tooltip when mouse leaves synset tag.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LabelTag_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_tagPopup != null)
            {
                _tagPopup.IsOpen = false;
                _tagPopup = null;
            }
        }

        private void btnFirstPicture_Click(object sender, RoutedEventArgs e)
        {
            _currentPicture = Picture.GetFirstPicture();
            showPicture(_currentPicture);
        }

        private void btnLastPicture_Click(object sender, RoutedEventArgs e)
        {
            _currentPicture = Picture.GetLastPicture();
            showPicture(_currentPicture);
        }

        private void btnNextPicture_Click(object sender, RoutedEventArgs e)
        {
            _currentPicture = _currentPicture.GetNextPicture();
            showPicture(_currentPicture);
        }

        private void btnPrevPicture_Click(object sender, RoutedEventArgs e)
        {
            _currentPicture = _currentPicture.GetPreviousPicture();
            showPicture(_currentPicture);
        }

        private void btnPictureSearch_Click(object sender, RoutedEventArgs e)
        {
            Picture searchResult = Picture.searchPicture(tbPictureName.Text);
            if (searchResult != null)
            {
                _currentPicture = searchResult;
                showPicture(_currentPicture);
            } else {
                MessageBox.Show("Picture with name \"" + tbPictureName.Text + "\" not found.");
            }
        }

        /// <summary>
        /// Checks if list of SynSetListItems contains given SynsetListItem. Compares synsets by ID.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="synset"></param>
        /// <returns></returns>
        static private bool listContainsSynset(BindingList<SynSetListItem> list, SynSetListItem synset)
        {
            foreach (var synset_ in list)
                if (synset_.Synset.ID == synset.Synset.ID)
                    return true;
            return false;
        }

        /// <summary>
        /// Contains synset and extra information needed to represent it in ListBox.
        /// </summary>
        class SynSetListItem
        {
            public SynSet Synset { get; set; }
            public String Title { get; set; }
            public String Desc { get; set; }

            public SynSetListItem(SynSet synset)
            {
                StringBuilder title = new StringBuilder();
                bool prependComma = false;
                foreach (string w in synset.Words)
                {
                    title.Append((prependComma ? ", " : "") + w);
                    prependComma = true;
                }
                title.Append(" [" + synset.POS + "]");
                this.Title = title.ToString();
                this.Synset = synset;
                this.Desc = synset.Gloss;
            }
        }
    }
}
