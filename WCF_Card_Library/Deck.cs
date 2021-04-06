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

namespace WCF_Card_Library
{
    //[ServiceContract]
    public interface ICallback
    {
        [OperationContract(IsOneWay = true)]
        void UpdateClient(CallbackInfo info);
    }
    /*
    * Program:         CardsLibrary.dll
    * Module:          DeckInterface.cs
    * Author:          Brian Hache, Danish Davis
    * Date:            March 25, 2021
    * Description:     Defines the Deck Interface class.
    *                  
    *                  
    */
    [ServiceContract(CallbackContract = typeof(ICallback))]
    public interface DeckInterface
    {
        void Shuffle();
        Card Draw();
        int NumDecks { get; set; }
        int NumCards { get; }
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class Deck : DeckInterface
    {
        /*------------------------ Member Variables ------------------------*/

        private List<Card> cards = null;    // collection of cards
        private int cardIdx;                // index of the next card to be dealt
        private int numDecks;               // number of decks in the shoe - for our game we just need one.

        private HashSet<ICallback> callbacks = new HashSet<ICallback>();

        /*-------------------------- Constructors --------------------------*/

        public Deck()
        {
            cards = new List<Card>();
            numDecks = 1;
            populate();
        }

        /*------------------ Public properties and methods -----------------*/

        // Randomizes the sequence of the Cards in the cards collection
        public void Shuffle()
        {
            // Randomize the cards collection
            Random rng = new Random();
            cards = cards.OrderBy(card => rng.Next()).ToList();

            // Reset the cards index
            cardIdx = 0;
        }

        // Returns a copy of the next Card in the cards collection
        public Card Draw()
        {
            if (cardIdx >= cards.Count)
                throw new ArgumentException("The shoe is empty.");

            return cards[cardIdx++];
        }

        // Lets the client read or modify the number of decks in the shoe
        public int NumDecks
        {
            get
            {
                return numDecks;
            }
            set
            {
                if (numDecks != value)
                {
                    numDecks = value;
                    populate();
                }
            }
        }

        // Lets the client read the number of cards remaining in the shoe
        public int NumCards
        {
            get
            {
                return cards.Count - cardIdx;
            }
        }

        /*------------------------- Helper methods -------------------------*/

        // Populates the cards attribute with Card objects and then shuffles it 
        private void populate()
        {
            // Clear-out all the "old' cards
            cards.Clear();

            // For each deck in numDecks...
            for (int d = 0; d < numDecks; ++d)
                // For each suit..
                foreach (Card.SuitID s in Enum.GetValues(typeof(Card.SuitID)))
                    // For each rank..
                    foreach (Card.RankID r in Enum.GetValues(typeof(Card.RankID)))
                        cards.Add(new Card(s, r));

            Shuffle();
        }

        // Uses the client callback objects to send current Shoe information 
        // to clients. If the change in teh Shoe state was triggered by a method call 
        // from a specific client, then that particular client will be excluded from
        // the update since it will already be updated directly by the call.
        private void updateClients(bool emptyHand)
        {
            // Identify which client just changed the Shoe object's state
            ICallback thisClient = null;
            if (OperationContext.Current != null)
                thisClient = OperationContext.Current.GetCallbackChannel<ICallback>();

            // Prepare the CallbackInfo parameter
            CallbackInfo info = new CallbackInfo(cards.Count - cardIdx, numDecks, emptyHand);

            // Update all clients except thisClient
            foreach (ICallback cb in callbacks)
                if (thisClient == null || thisClient != cb)
                    cb.UpdateClient(info);
        }

    } // end Shoe class
}
