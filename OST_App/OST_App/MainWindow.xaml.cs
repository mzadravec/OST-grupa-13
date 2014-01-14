﻿using System;
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

        private Picture currentPicture;

        public MainWindow()
        {
            InitializeComponent();

            // create wordnet engine (use disk-based retrieval by default)
            _wordNetEngine = new WordNetEngine(@"..\..\..\dict\", false); // TODO: Set some other path, absolute (which one)?

            synsetsFoundListBox.ItemsSource = synsetsFound;
            synsetsTaggedListBox.ItemsSource = synsetsTagged;
            findSynsets(); // For initialization of GUI elements

            currentPicture = Picture.GetFirstPicture();
            showPicture(currentPicture);
            
        }

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
            synsetsTagged.Clear();
            List<String> synsetIDs = picture.getSynsets();
            foreach (var ID in synsetIDs)
                synsetsTagged.Add(new SynSetListItem(_wordNetEngine.GetSynSet(ID)));
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
                        synsetsFound.Add(new SynSetListItem (synset));

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
                if (!listContainsSynset(synsetsTagged, selectedSynset))
                    synsetsTagged.Add(selectedSynset);
            }
            synsetsFoundListBox.SelectedItem = null;
            currentPicture.addSynset(selectedSynset.Synset.ID);
        }

        /// <summary>
        /// Removes clicked synset from tagged synsets and updates database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void labelDeleteTag_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SynSetListItem synsetItem = (SynSetListItem)((FrameworkElement)sender).Tag;
            synsetsTagged.Remove(synsetItem);
            currentPicture.removeSynset(synsetItem.Synset.ID);
        }

        private void synsetsFoundListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnAddSynset.IsEnabled = synsetsFoundListBox.SelectedItem != null && !listContainsSynset(synsetsTagged, (SynSetListItem)synsetsFoundListBox.SelectedItem);
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

        private void btnFirstPicture_Click(object sender, RoutedEventArgs e)
        {
            currentPicture = Picture.GetFirstPicture();
            showPicture(currentPicture);
        }

        private void btnLastPicture_Click(object sender, RoutedEventArgs e)
        {
            currentPicture = Picture.GetLastPicture();
            showPicture(currentPicture);
        }

        private void btnNextPicture_Click(object sender, RoutedEventArgs e)
        {
            currentPicture = currentPicture.GetNextPicture();
            showPicture(currentPicture);
        }

        private void btnPrevPicture_Click(object sender, RoutedEventArgs e)
        {
            currentPicture = currentPicture.GetPreviousPicture();
            showPicture(currentPicture);
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

        private void btnPictureSearch_Click(object sender, RoutedEventArgs e)
        {
            Picture searchResult = Picture.searchPicture(tbPictureName.Text);
            if (searchResult != null)
            {
                currentPicture = searchResult;
                showPicture(currentPicture);
            } else {
                MessageBox.Show("Picture with name \"" + tbPictureName.Text + "\" not found.");
            }
        }
    }
}
