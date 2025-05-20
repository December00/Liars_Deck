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
        public string trump_card;
        public string current_cards;
        public bool isPlaying = false;
        public Dictionary<int, string> queue = new Dictionary<int, string>();
        public int currentPlayerIndex;      
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
                room.InitializeTurnInfo();
                room.server.StopAcceptingClients();
                List<char> deckLetters = new List<char>(remainingDeck);
                int i = 1;
                foreach (string player in allPlayers)
                {
                    
                    if (deckLetters.Count < 5) throw new InvalidOperationException("Недостаточно карт!");
                    players[player] = GetRandomCards(deckLetters, 5);
                    queue[i] = player;
                    i++;
                }

                if (players.TryGetValue(room.CurrentUsername, out string hostCards))
                {
                    room.CurrentDeck = hostCards;
                }
                
                trump_card = GetTrumpCard();
                room.currentTrump = GetTrumpCardName(trump_card);
                current_cards = "";
                currentPlayerIndex = 1;
                room.selectedCardIndices[queue[1]] = new List<int>();
                
                Application.Current.Dispatcher.Invoke(() => 
                {
                    room.UpdateTurnInfo();
                });

                _ = room.server.BroadcastPlayersCards(players);
                room.UpdateCardsForAllPlayers(players);
                room.server?.BroadcastTurnInfo(
                    queue[currentPlayerIndex], 
                    trump_card.ToString()
                );
                return true;
            }
            else
            {
                MessageBox.Show("Для начала игры в комнате должно быть от 2 до 4 игроков");
                return false;
            }
        }
        public async void Restart()
        {
            remainingDeck = "jjaaaaaakkkkkkqqqqqq";
            List<string> allPlayers = room.clientElements.Keys.ToList();
            List<char> deckLetters = new List<char>(remainingDeck);
            int i = 1;
            foreach (string player in allPlayers)
            {

                if (deckLetters.Count < 5) throw new InvalidOperationException("Недостаточно карт!");
                players[player] = GetRandomCards(deckLetters, 5);
                queue[i] = player;
                i++;
            }
            trump_card = GetTrumpCard();
            room.currentTrump = GetTrumpCardName(trump_card);
            current_cards = "";
            currentPlayerIndex = 1;
            room.selectedCardIndices[queue[1]] = new List<int>();
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                room.UpdateTurnInfo();
            });

            _ = room.server.BroadcastPlayersCards(players);
            room.UpdateCardsForAllPlayers(players);
            room.server?.BroadcastTurnInfo(
                queue[currentPlayerIndex],
                trump_card.ToString()
            );
            await room.server.BroadcastCardsToCenter("");
        }
        private string GetTrumpCardName(string trump)
        {
            return trump.ToLower() switch
            {
                "a" => "Ace",
                "k" => "King",
                "q" => "Queen",
                _ => "Unknown"
            };
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
        public string GetTrumpCard()
        {
            string cards = "akq";
            return cards[random.Next(0, cards.Length)].ToString();
        }
        public bool Check()
        {
            foreach (char c in current_cards)
            {
                if (c.ToString() != trump_card && c.ToString() != "j")
                {
                    return false;
                }
            }
            return true;
        }
        public async void Update(List<int> cardsIndexes)
        {
            if (!players.ContainsKey(queue[currentPlayerIndex])) return;

            string currentPlayer = queue[currentPlayerIndex];
            StringBuilder cardsToSend = new StringBuilder();

            var sortedIndexes = cardsIndexes.OrderByDescending(i => i).ToList();
            foreach (int index in sortedIndexes)
            {
                if (index < players[currentPlayer].Length)
                {
                    cardsToSend.Append(players[currentPlayer][index]);
                    players[currentPlayer] = players[currentPlayer].Remove(index, 1);
                }
            }
            
            if (currentPlayer == room.CurrentUsername)
            {
                room.CurrentDeck = players[currentPlayer];
            }

            if (room.server != null)
            {
                room.ShowCardsInCenter(cardsToSend.ToString());
                room.UpdateCardsForAllPlayers(players);
            }

            await room.server?.BroadcastCardsToCenter(cardsToSend.ToString());
            _ = room.server?.BroadcastPlayersCards(players);
            
            current_cards = cardsToSend.ToString();
            currentPlayerIndex = currentPlayerIndex == room.clientElements.Count ? 1 : currentPlayerIndex + 1;
            while (players[queue[currentPlayerIndex]] == "")
            {
                players.Remove(queue[currentPlayerIndex]);
                queue.Remove(currentPlayerIndex);
                var newQueue = new Dictionary<int, string>();
                int index = 1;
                foreach (var player in queue.Values)
                {
                    newQueue[index++] = player;
                }
                queue = newQueue;
                
            }
            
            room.currentTurn = queue[currentPlayerIndex]; 
            room.currentTrump = GetTrumpCardName(this.trump_card);

            Application.Current.Dispatcher.Invoke(() => 
            {
                room.UpdateTurnInfo();
            });

            if (room.server != null)
            {
                await room.server.BroadcastTurnInfo(room.currentTurn, trump_card);
            }   
        }
        public async void Update(List<int> cardsIndexes, string player = null)
        {
            string currentPlayer = player ?? queue[currentPlayerIndex];

            StringBuilder cardsToSend = new StringBuilder();
            var sortedIndexes = cardsIndexes.OrderByDescending(i => i).ToList();
            foreach (int index in sortedIndexes)
            {
                if (index < players[currentPlayer].Length)
                {
                    cardsToSend.Append(players[currentPlayer][index]);
                    players[currentPlayer] = players[currentPlayer].Remove(index, 1);
                }
            }
            
            current_cards = cardsToSend.ToString();
            if (currentPlayer == room.CurrentUsername)
            {
                room.CurrentDeck = players[currentPlayer];
            }

            if (room.server != null)
            {
                room.ShowCardsInCenter(cardsToSend.ToString());
                room.UpdateCardsForAllPlayers(players);
                await room.server.BroadcastCardsToCenter(cardsToSend.ToString());
                _ = room.server.BroadcastPlayersCards(players);
            }
            
            currentPlayerIndex = currentPlayerIndex == room.clientElements.Count ? 1 : currentPlayerIndex + 1;
            room.currentTurn = queue[currentPlayerIndex];
            await room.server?.BroadcastTurnInfo(room.currentTurn, trump_card);

            Application.Current.Dispatcher.Invoke(() => 
            {
                room.UpdateTurnInfo();
            });
        }
        public void HandleCheckResult(string liar)
        {
            if (liar != "no_liar")
            {
                players.Remove(liar);
                queue = queue.Where(p => p.Value != liar)
                    .ToDictionary(p => p.Key, p => p.Value);

                var newQueue = new Dictionary<int, string>();
                int index = 1;
                foreach (var player in queue.Values)
                {
                    newQueue[index++] = player;
                }
                queue = newQueue;

                currentPlayerIndex = currentPlayerIndex >= queue.Count ? 1 : currentPlayerIndex;
            }
        }
        public void HandlePlayerDisconnect(string username)
        {
            if (players.ContainsKey(username))
            {
                players.Remove(username);
                queue = queue.Where(p => p.Value != username)
                           .ToDictionary(p => p.Key, p => p.Value);

                var newQueue = new Dictionary<int, string>();
                int index = 1;
                foreach (var player in queue.Values)
                {
                    newQueue[index++] = player;
                }
                queue = newQueue;

                if (currentPlayerIndex > queue.Count)
                {
                    currentPlayerIndex = 1;
                }
            }
        }
    }
}