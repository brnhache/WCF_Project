/*
 * Program:         PlayingCardsLibrary.dll
 * Module:          Card.cs
 * Author:          Brian Hache, Danish Davis
 * Date:            March 25, 2021
 * Description:     Defines the Playing Card class
 *                  
 *                  
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WCF_Card_Library
{
    [DataContract]
    public class Card
    {
        /*--------------------- Public type definitions --------------------*/

        public enum SuitID { Clubs, Diamonds, Hearts, Spades };
        public enum RankID { Ace, King, Queen, Jack, Ten, Nine, Eight, Seven, Six, Five, Four, Three, Two };

        /*-------------------------- Constructors --------------------------*/

        internal Card(SuitID s, RankID r)
        {
            Suit = s;
            Rank = r;
        }

        /*------------------ Public properties and methods -----------------*/
        [DataMember]
        public SuitID Suit { get; private set; }
        [DataMember]
        public RankID Rank { get; private set; }

        public override string ToString()
        {
            return Rank.ToString() + " of " + Suit.ToString();
        }

    } // end Card class
}
