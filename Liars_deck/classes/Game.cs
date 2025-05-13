using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Liars_deck.classes
{
    public class Game
    {
        private Room room;
        private Dictionary<string, string> players = new Dictionary<string, string>();
        private Dictionary<string, int> lives = new Dictionary<string, int>();
        private Random random = new Random();
        private string remainingDeck;
        public char trump_card;
        private string current_cards;
        private string last_player;
        public bool isPlaying = false;
        public Game(Room room)
        {
            this.room = room;
        }

        public bool Start()
        {
            remainingDeck = "jjaaaaaakkkkkkqqqqqq";
            List<string> allPlayers = room.clientElements.Keys.ToList();
            if (allPlayers.Count > 1)
            {
                isPlaying = true;
                room.StartButton.Foreground = Brushes.Gray;
                List<char> deckLetters = new List<char>(remainingDeck);

                foreach (string player in allPlayers)
                {
                    if (deckLetters.Count < 5) throw new InvalidOperationException("Недостаточно карт!");
                    players[player] = GetRandomCards(deckLetters, 5);
                }

                // Добавлено: сохраняем карты хоста в room.CurrentDeck
                if (players.TryGetValue(room.CurrentUsername, out string hostCards))
                {
                    room.CurrentDeck = hostCards;
                }

                trump_card = GetTrumpCard();
                current_cards = "";

                _ = room.server.BroadcastPlayersCards(players);
                room.UpdateCardsForAllPlayers(players);

                return true;
            }
            else
            {
                MessageBox.Show("Для начала игры в комнате должно быть от 2 до 4 игроков");
                return false;
            }
        }

        private string GetRandomCards(List<char> deck, int count)
        {
            StringBuilder cards = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                int randomIndex = random.Next(deck.Count);
                cards.Append(deck[randomIndex]);
                deck.RemoveAt(randomIndex);
            }
            return cards.ToString();
        }

        public Dictionary<string, string> GetPlayersHands()
        {
            return new Dictionary<string, string>(players);
        }

        public string GetRemainingDeck()
        {
            return remainingDeck;
        }
        public char GetTrumpCard()
        {
            string cards = "akq";
            return cards[random.Next(0, cards.Length)];
        }
        public bool Check()
        {
            foreach (char c in current_cards)
            {
                if (c != trump_card)
                {
                    return false;
                }
            }
            return true;
        }
        public void Update(string cards)
        {
            current_cards = cards;

        }
        public void Draw()
        {
            foreach (var player in players)
            {
                if (!string.IsNullOrEmpty(player.Value))
                {
                    room.DrawCardsForPlayer(player.Key, player.Value);
                }
            }
        }
        public void ChangeLives()
        {

        }
    }
}