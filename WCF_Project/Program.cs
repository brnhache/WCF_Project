/*
    * Program:         WCF_Project
    * Module:          Program.cs
    * Author:          Brian Hache, Danish Davis
    * Date:            April 04, 2021
    * Description:     Game Client
    *                  
    *                  
    */

using System;
using WCF_Card_Library;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;  // WCF  types
using System.Windows.Forms;
using System.Drawing;

namespace WCF_Project
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    class Program
    {
        [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
        public class Connect : Form, ICallback
        {
            public DeckInterface deck = null;
            public int playerIndex = 0;

            Label card = new Label();
            Button button1 = new Button();

            public Connect()
            {
                initialize();
            }

            private void initialize()
            {
                try
                {
                    // Connect to the WCF service endpoint
                    DuplexChannelFactory<DeckInterface> channel = new DuplexChannelFactory<DeckInterface>(this, "DeckEndPoint");
                    deck = channel.CreateChannel();

                    // Register for the callback service
                    deck.RegisterForCallbacks();

                    playerIndex = deck.PlayerIndex;
                    deck.PlayerIndex++;

                    Console.WriteLine("player index " + playerIndex);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            public string DrawCard()
            {
                Card cardDrawn = deck.Draw(playerIndex);
                return cardDrawn.ToString();
            }

            private delegate void ClientUpdateDelegate(int activePlayerIndex);
            public void UpdateClient(int activePlayerIndex)
            {
                Console.WriteLine("Triggered " + activePlayerIndex + "," + playerIndex);


                //if (System.Threading.Thread.CurrentThread == this.Dispatcher.Thread)
                //{
                //    // Update the GUI
                //    if (activePlayerIndex == playerIndex)
                //    {
                //        button1.Enabled = true;
                //    }
                //}
                //else
                //{
                //    // Not the dispatcher thread that's calling this method!
                //    this.BeginInvoke(new ClientUpdateDelegate(UpdateClient), activePlayerIndex);
                //}
            }

            public void initializeForm()
            {
                // Create a new instance of the form.
                Form form1 = new Form();
                // Create two buttons to use as the accept and cancel buttons.
                button1 = new Button();
                Button button2 = new Button();

                // Set the text of button1 to "OK".
                button1.Text = "Draw Card";
                button1.Enabled = false;

                // Set the position of the button on the form.
                button1.Location = new Point(10, 10);
                // Set the text of button2 to "Cancel".
                button2.Text = "Cancel";
                // Set the position of the button based on the location of button1.
                button2.Location = new Point(button1.Left, button1.Height + button1.Top + 10);
                // Set the caption bar text of the form.   
                form1.Text = "My Dialog Box";
                // Display a help button on the form.
                form1.HelpButton = true;

                // Define the border style of the form to a dialog box.
                form1.FormBorderStyle = FormBorderStyle.FixedDialog;
                // Set the MaximizeBox to false to remove the maximize box.
                form1.MaximizeBox = false;
                // Set the MinimizeBox to false to remove the minimize box.
                form1.MinimizeBox = false;
                // Set the accept button of the form to button1.
                form1.AcceptButton = button1;
                // Set the cancel button of the form to button2.
                form1.CancelButton = button2;
                // Set the start position of the form to the center of the screen.
                form1.StartPosition = FormStartPosition.CenterScreen;

                //label
                card = new Label();
                card.Text = "Something";
                card.Location = new Point(110, 90);

                //button control
                button1.Click += new EventHandler(DrawButtonClick);



                // Add button1 to the form.
                form1.Controls.Add(button1);
                // Add button2 to the form.
                form1.Controls.Add(button2);
                form1.Controls.Add(card);

                // Display the form as a modal dialog box.
                form1.ShowDialog();
            }

            public void DrawButtonClick(object sender, EventArgs e)
            {
                string cardDrawn = DrawCard();
                card.Text = cardDrawn;
                button1.Enabled = false;
            }
        }

        static void Main(string[] args)
        {
            Connect c = new Connect();

            if (c.playerIndex == 0)
            {
                Console.WriteLine("Enter the number of the players: ");
                int numPlayers;
                while (!Int32.TryParse(Console.ReadLine(), out numPlayers))
                {
                    Console.WriteLine("Wrong input! please try again:");
                }
                c.deck.NumPlayers = numPlayers;
            }

            c.initializeForm();

        }
    }
}