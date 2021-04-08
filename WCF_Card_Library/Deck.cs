/*
 * Program:         CardsLibrary.dll
 * Module:          Deck.cs
 * Author:          Brian Hache, Danish Davis
 * Date:            March 25, 2021
 * Description:     Defines the Deck class which manages 52 playing cards.
 *                  
 *                  
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using static WCF_Card_Library.Card;

namespace WCF_Card_Library
{
    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void UpdateClient(int playerIndex, string cardDrawn, string counts);

        [OperationContract(IsOneWay = true)]
        void ClearData();
    }
    /*
    * Program:         CardsLibrary.dll
    * Module:          DeckInterface.cs
    * Author:          Brian Hache, Danish Davis
    * Date:            March 25, 2021
    * Description:     Defines the Deck Interface class.             
    */
    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface DeckInterface
    {
        int PlayerIndex { [OperationContract] get; [OperationContract(IsOneWay = true)] set; }
        int NumPlayers { [OperationContract] get; [OperationContract(IsOneWay = true)] set; }
        int ActivePlayerIndex { [OperationContract] get; [OperationContract(IsOneWay = true)] set; }

        [OperationContract(IsOneWay = true)]
        void RegisterForCallbacks();
        [OperationContract(IsOneWay = true)]
        void UnregisterFromCallbacks();

        [OperationContract]
        Card Draw(int playerIndex);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Deck : DeckInterface
    {

        private List<List<Card>> playersCardSet = null;
        private List<Card> cardsPlayed = null;
        private int numPlayers = 0;
        private int playerIndex = 0;
        private int activePlayerIndex = 0;
        private HashSet<ICallback> callbacks = new HashSet<ICallback>();
        static Random rnd = new Random();

        public Deck()
        {
            playersCardSet = new List<List<Card>>();
            cardsPlayed = new List<Card>();
        }

        public int PlayerIndex
        {
            get
            {
                return playerIndex;
            }
            set
            {
                playerIndex = value;
            }
        }

        public int ActivePlayerIndex
        {
            get
            {
                return activePlayerIndex;
            }
            set
            {
                activePlayerIndex = value;
            }
        }

        public int NumPlayers
        {
            get
            {
                return numPlayers;
            }
            set
            {
                numPlayers = value;
                populate();
            }
        }

        public Card Draw(int playerIndex)
        {

            List<Card> playerCardset = playersCardSet[playerIndex];

            //get random card
            int r = rnd.Next(playerCardset.Count) - 1;
            Card cardDrawn = playerCardset[r];
            Console.WriteLine(cardDrawn.Rank);
            if (cardsPlayed.Count > 0 && cardDrawn.Rank == cardsPlayed[cardsPlayed.Count - 1].Rank)
            {
                Console.WriteLine("PLAYER WON");
                foreach (Card c in cardsPlayed)
                {
                    playerCardset.Add(c);
                }
                cardsPlayed.Clear();

                updateClients("Player " + (playerIndex + 1) + " " + cardDrawn.ToString());

                //clear the window
                foreach (ICallback cb in callbacks)
                {
                    cb.ClearData();
                }
            }
            else
            {
                cardsPlayed.Add(cardDrawn);
                playerCardset.RemoveAt(r);
                //increment the active player index
                if (numPlayers == activePlayerIndex + 1)
                {
                    activePlayerIndex = 0;
                }
                else
                {
                    activePlayerIndex++;
                }

                updateClients("Player " + (playerIndex + 1) + " " + cardDrawn.ToString());
                return cardDrawn;
            }

            return null;
        }

        private void populate()
        {
            for (int d = 0; d < numPlayers; ++d)
            {
                List<Card> deck = new List<Card>();
                foreach (SuitID s in Enum.GetValues(typeof(SuitID)))
                    foreach (RankID r in Enum.GetValues(typeof(RankID)))
                        deck.Add(new Card(s, r));

                playersCardSet.Add(deck);
            }
        }

        private void updateClients(string cardDrawn)
        {
            string counts = "";

            int counter = 1;
            foreach (var l in playersCardSet)
            {
                counts += "Player " + counter + " has " + l.Count + " cards | ";
                counter++;
            }

            foreach (ICallback cb in callbacks)
            {
                //if (thisclient == null || thisclient != cb)
                cb.UpdateClient(activePlayerIndex, cardDrawn, counts);
            }
        }

        public void RegisterForCallbacks()
        {
            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();

            if (!callbacks.Contains(cb))
                callbacks.Add(cb);
        }

        public void UnregisterFromCallbacks()
        {
            ICallback cb = OperationContext.Current.GetCallbackChannel<ICallback>();

            if (callbacks.Contains(cb))
                callbacks.Remove(cb);
        }
    }
}
