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
                deck.PlayerIndex++;
                string numPlayers = Interaction.InputBox("Enter Number of Players", "Enter Number of Players", "0", -1, -1);
                deck.NumPlayers = Int32.Parse(numPlayers); ;
            }
            else
            {
                deck.PlayerIndex++;
            }

            deck.RegisterForCallbacks();

        }

        private void btnDraw_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Card cardDrawn = deck.Draw(playerIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

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

        private delegate void GenericMessageDelegate(string message);

        public void GenericMessage(string message)
        {
            if (Thread.CurrentThread == this.Dispatcher.Thread)
            {
                if (message.Contains("won"))
                {
                    MessageBox.Show(message);
                    this.Close();
                }
                else
                {
                    lstCards.Items.Insert(0, message);
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new GenericMessageDelegate(GenericMessage), message);
            }
        }

        private delegate void KnockOutDelegate(int activePlayerIndex);

        public void KnockOut(int activePlayerIndex)
        {
            if (Thread.CurrentThread == this.Dispatcher.Thread)
            {
                if (playerIndex == activePlayerIndex)
                {
                    MessageBox.Show("Player " + (playerIndex + 1) + " lost the game");
                    this.Close();
                }
            }
            else
            {
                this.Dispatcher.BeginInvoke(new KnockOutDelegate(KnockOut), activePlayerIndex);
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            deck?.UnregisterFromCallbacks();
        }

    }
}
