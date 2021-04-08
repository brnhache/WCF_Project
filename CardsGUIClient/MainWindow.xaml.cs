/*
 * Program:         CardsGuiClient.exe
 * Module:          MainWindow.xaml.cs
 * Author:          T. Haworth
 * Date:            March 9, 2021
 * Description:     A Windows WPF client that uses CardsLibrary.dll via a WCF service.
 * 
 *                  Note that we had to add a reference to the .NET Framework 
 *                  assembly System.ServiceModel.dll.
 *                  
 * Modifications:   Mar 16, 2021
 *                  The client now receives a Card object from the Shoe's Draw() method 
 *                  now returns a  instead of a string because Card is now a data 
 *                  contract. Now uses an administrative endpoint which is configured
 *                  in the project's App.config file.
 *                  
 *                  Mar 23, 2021
 *                  Implements the ICallback contract and registers (and unregisters) for 
 *                  the callbacks service so that the client will reflect real time updates 
 *                  about the state of the Shoe.
 */

using System;
using System.Windows;
using System.ServiceModel;  // WCF  types
using WCF_Card_Library;
using System.Threading;
using Microsoft.VisualBasic;

namespace CardsGUIClient
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public partial class MainWindow : Window, ICallback
    {
        // Private member variables
        private DeckInterface deck = null;
        private int playerIndex = 0;

        public MainWindow()
        {
            InitializeComponent();

            DuplexChannelFactory<DeckInterface> channel = new DuplexChannelFactory<DeckInterface>(this, "DeckEndPoint");
            deck = channel.CreateChannel();

            playerIndex = deck.PlayerIndex;
            playerLabel.Content = "Player " + (playerIndex + 1);

            if (playerIndex != 0)
                btnDraw.IsEnabled = false;

            if (playerIndex == 0)
            {
                string numPlayers = Interaction.InputBox("Enter Number of Players", "Enter Number of Players", "0", -1, -1);
                deck.NumPlayers = Int32.Parse(numPlayers); ;
            }

            deck.PlayerIndex++;

            // Register for the callback service
            deck.RegisterForCallbacks();

            // Initialize the GUI
            //updateCardCounts(false);

        } // end default C'tor


        // Helper methods

        private void updateCardCounts(bool emptyHand)
        {
            //if (emptyHand)
            //    // Only "throw out" drawn cards if the Shoe was shuffled 
            //    // or the number of decks was changed
            //    lstCards.Items.Clear();

            //txtHandCount.Text = lstCards.Items.Count.ToString();
            //txtShoeCount.Text = shoe.NumCards.ToString();
        } // end updateCardCounts()

        // Event handlers

        private void btnDraw_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Card cardDrawn = deck.Draw(playerIndex);
                //lstCards.Items.Insert(0, "sfdjfdhfd");
                //txtDeckCount.Text = cardDrawn.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnShuffle_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    shoe.Shuffle();

            //    // Update the GUI
            //    updateCardCounts(true);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        } // end btnShuffle_Click()

        private void sliderDecks_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //try
            //{
            //    if (shoe != null)
            //    {
            //        // Reset the number of decks
            //        shoe.NumDecks = (int)sliderDecks.Value;

            //        // Update the GUI
            //        updateCardCounts(true);
            //        int n = shoe.NumDecks;
            //        txtDeckCount.Text = n + " deck" + (n == 1 ? "" : "s");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
        } // end sliderDecks_ValueChanged()

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private delegate void ClientUpdateDelegate(int playerIndex, string cardDrawn, string count);

        public void UpdateClient(int activePlayerIndex, string cardDrawn, string count)
        {
            if (Thread.CurrentThread == this.Dispatcher.Thread)
            {
                lstCards.Items.Insert(0, cardDrawn);
                cardCount.Text = count;
                //Update the GUI
                if (activePlayerIndex == playerIndex)
                {
                    btnDraw.IsEnabled = true;
                    cardCount.Text = count;
                }
                else
                {
                    btnDraw.IsEnabled = false;
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new ClientUpdateDelegate(UpdateClient), activePlayerIndex, cardDrawn, count);
            }
        }


        private delegate void ClientClearDelegate();

        public void ClearData()
        {
            if (System.Threading.Thread.CurrentThread == this.Dispatcher.Thread)
            {
                lstCards.Items.Clear();
            }
            else
            {
                this.Dispatcher.BeginInvoke(new ClientClearDelegate(ClearData));
            }
        }

        public void updateButton(int activePlayerIndex)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //shoe?.UnregisterFromCallbacks();
        }

    } // end MainWindow class
}
