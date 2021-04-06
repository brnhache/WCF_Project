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

namespace WCF_Project
{
    class Program
    {
        static void Main(string[] args)
        {
            // Welcome players to the game and get number of players
            Console.WriteLine("Welcome to Match_War! How many players?");
            string num_players_input = Console.ReadLine();
            int num_players;

            while (!Int32.TryParse(num_players_input, out num_players))
            {
                Console.WriteLine("Please enter a whole number to represent the number of players.");
                num_players_input = Console.ReadLine();
            }
            Console.WriteLine($"{num_players} players have been added to the game.");

            // Create the deck object and deal out hands to the players
            Deck deck = new Deck();
            List<List<Card>> playerHands = new List<List<Card>>();
            for (int i = 0; i < num_players; i++)
            {
                playerHands.Add(new List<Card>());
            }
            // deal until there are less cards in the deck than players
            while (deck.NumCards >= num_players)
            {
                playerHands.ForEach(hand =>
                {
                    hand.Add(deck.Draw());
                });
            }

            // Print out the player hands
            int player = 1;
            playerHands.ForEach(item =>
            {
                Console.WriteLine($"\nPlayer {player}'s hand");
                Console.WriteLine("-----------------------------");
                item.ForEach(card =>
                {
                    Console.WriteLine(card);
                });
                player++;
            });

            // Create the middle pile and start playing
            List<Card> pile = new List<Card>();
            bool game_over = false;
            Card lastPlayed = null;
            while (!game_over)
            {
                int player_turn = 1;
                foreach (List<Card> hand in playerHands)
                {
                    if (game_over) { break; }
                    Card played = null;
                    Console.WriteLine($"\n\nPlayer {player_turn}, it is your turn.");
                    if (pile.Count == 0)
                    {
                        Console.WriteLine($"There are no cards in the pile. Enter the number beside the card you would like to play:\n");
                    }
                    else
                    {
                        Console.WriteLine($"There are {pile.Count} cards in the pile. The last card played was {lastPlayed}");
                        Console.WriteLine("Enter the number beside the card you would like to play:\n");
                    }

                    // List out the player cards and get the player's choice (this will only happen in the individual client's window)
                    int card_num = 1;
                    hand.ForEach(card =>
                    {
                        Console.WriteLine($"{card_num}. {card}");
                        card_num++;
                    });
                    string card_choice_input = Console.ReadLine();
                    int card_choice;
                    while (!Int32.TryParse(card_choice_input, out card_choice))
                    {
                        Console.WriteLine("That is not a valid selection. Try again.");
                        card_choice_input = Console.ReadLine();
                    }

                    // Add the card to the pile and decide if they won the round
                    played = hand[card_choice - 1];
                    pile.Add(played);
                    hand.Remove(played);
                    Console.WriteLine($"Player {player_turn} has put down the {played}");

                    if (lastPlayed != null && played.Rank == lastPlayed.Rank)
                    {
                        Console.WriteLine($"Congratulations Player {player_turn}! You have won the pile.");
                        pile.ForEach(card =>
                        {
                            hand.Add(card);
                        });
                        pile.Clear();
                        lastPlayed = null;
                    }
                    else
                    {
                        lastPlayed = played;
                    }

                    // Check to see if the game is over
                    if (hand.Count == 0)
                    {
                        Console.WriteLine($"\n\nGAME OVER\n\n");
                        game_over = true;
                    }
                    else
                    {
                        Console.WriteLine("Here are your remaining cards:");
                        hand.ForEach(card =>
                        {
                            Console.WriteLine(card);
                        });
                        player_turn++;
                    }

                };
            }

            // List the score and find the winner
            Console.WriteLine($"Here is the score of the game:");
            int highscore = 0;
            player = 1;
            int winner = 0;
            playerHands.ForEach(hand =>
            {
                Console.WriteLine($"Player {player}: {hand.Count} Cards.");
                if (hand.Count > highscore)
                {
                    highscore = hand.Count;
                    winner = player;
                }
            });

            Console.WriteLine($"Player {winner} Won!!");

        }
    }
}
