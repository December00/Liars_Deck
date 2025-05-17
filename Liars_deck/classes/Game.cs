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
        private string current_cards;
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
                room.StartButton.Foreground = Brushes.Gray;
                room.InitializeTurnInfo();
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

            // Явное обновление CurrentDeck для хоста
            if (currentPlayer == room.CurrentUsername)
            {
                room.CurrentDeck = players[currentPlayer];
            }

            // Локальное обновление для хоста
            if (room.server != null)
            {
                room.ShowCardsInCenter(cardsToSend.ToString());
                room.UpdateCardsForAllPlayers(players); // Важное изменение!
            }

            // Рассылка для клиентов
            room.server?.BroadcastCardsToCenter(cardsToSend.ToString());
            _ = room.server?.BroadcastPlayersCards(players);
            
            current_cards = cardsToSend.ToString();
            currentPlayerIndex = currentPlayerIndex == room.clientElements.Count ? 1 : currentPlayerIndex + 1;
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
        public void Update(List<int> cardsIndexes, string player = null)
        {
            // Определяем, чей ход обрабатываем
            string currentPlayer = player ?? queue[currentPlayerIndex];

            // Удаляем карты из колоды игрока
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

            // Обновляем UI хоста (если это его карты)
            if (currentPlayer == room.CurrentUsername)
            {
                room.CurrentDeck = players[currentPlayer];
            }

            // Рассылаем изменения всем игрокам
            if (room.server != null)
            {
                room.ShowCardsInCenter(cardsToSend.ToString());
                room.UpdateCardsForAllPlayers(players);
                room.server.BroadcastCardsToCenter(cardsToSend.ToString());
                _ = room.server.BroadcastPlayersCards(players);
            }

            // Переключаем очередь на следующего игрока ВНЕ ЗАВИСИМОСТИ ОТ ТОГО, ХОДИЛ ХОСТ ИЛИ КЛИЕНТ
            currentPlayerIndex = (currentPlayerIndex + 1) % queue.Count;
            room.currentTurn = queue[currentPlayerIndex];
            room.server?.BroadcastTurnInfo(room.currentTurn, trump_card);

            Application.Current.Dispatcher.Invoke(() => 
            {
                room.UpdateTurnInfo();
            });
        }
    }
}