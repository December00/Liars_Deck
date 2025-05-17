    using Liars_deck.classes;
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    namespace Liars_deck
    {
        public partial class GameWindow : Window
        {
            private Game game;
            private Room room;
            private Client client;
            private User user;
            private bool isHost;
            public GameWindow(Server server, User user)
            {
                InitializeComponent();
                this.user = user;
                this.client = new Client(user);
                this.isHost = true;
                this.room = new Room(MainGrid) { CurrentUsername = user.login };
                this.room.server = server;
                this.room.server.Start(8000, user.login);
                this.room.AddHostPlayer(user.login);
                room.InitializeButtons();
                this.client.OnPlayerConnected += OnPlayerConnectedHandler;
                this.client.OnPlayerListReceived += OnPlayerListReceivedHandler;
                this.client.OnCardsReceived += OnCardsReceivedHandler;
                this.client.OnCardsToCenter += OnCardsToCenterHandler;
                this.client.OnTurnChanged += OnTurnChangedHandler;
                this.room.server.OnClientAction += HandlePlayerAction;
                IPTextBox.Text = "127.0.0.1";
                IPTextBox.IsEnabled = false;
                Connect();
                this.game = new Game(room);
                room.StartButton.Click += StartButton_Click;
                room.NextButton.Click += NextButton_Click;
                room.CheckButton.Click += CheckButton_Click;
                room.NextButton.IsEnabled = false;
                room.CheckButton.IsEnabled = false;
                room.CheckButton.Foreground = Brushes.Gray;
                room.NextButton.Foreground = Brushes.Gray;
            }
            
            public GameWindow(User user)
            {
                InitializeComponent();
                this.isHost = false;
                this.user = user;
                this.client = new Client(user);
                IPTextBox.Text = "127.0.0.1";
                this.room = new Room(MainGrid) { CurrentUsername = user.login };
                room.InitializeButtons();
                room.StartButton.Visibility = Visibility.Collapsed;
                this.client.OnPlayerConnected += OnPlayerConnectedHandler;
                this.client.OnPlayerListReceived += OnPlayerListReceivedHandler;
                this.client.OnCardsReceived += OnCardsReceivedHandler;
                this.client.OnCardsToCenter += OnCardsToCenterHandler;
                this.client.OnTurnChanged += OnTurnChangedHandler;
                this.game = new Game(room);
                room.NextButton.Click += NextButton_Click;
                room.CheckButton.Click += CheckButton_Click;
                room.NextButton.IsEnabled = false;
                room.CheckButton.IsEnabled = false;
                room.CheckButton.Foreground = Brushes.Gray;
                room.NextButton.Foreground = Brushes.Gray;
            }

            
            private void OnPlayerConnectedHandler(string username)
            {
                if (username.StartsWith("PLAYER_TURN:")) return;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!room.clientElements.ContainsKey(username))
                    {
                        room.AddClientUI(username);
                    }
                });
            }

            private void OnPlayerListReceivedHandler(string playerList)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var username in playerList.Split(','))
                    {
                        if (!string.IsNullOrEmpty(username) && username != user.login)
                        {
                            room.AddClientUI(username);
                        }
                    }

                    if (!isHost)
                    {
                        room.AddClientUI(user.login);
                    }
                });
            }
            private void OnCardsReceivedHandler(Dictionary<string, string> playersCards)
            {
                if (playersCards.TryGetValue(user.login, out string myCards))
                {
                    client.current_deck = myCards;
                    room.CurrentDeck = myCards;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    room.UpdateCardsForAllPlayers(playersCards);
                });
            }
            private void OnCardsToCenterHandler(string cards)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    room.ShowCardsInCenter(cards);
                    game.current_cards = cards;
                });
            }
            private void OnTurnChangedHandler(string currentTurn, string trump)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    room.currentTurn = currentTurn;
                    room.currentTrump = GetTrumpCardName(trump);
                    game.trump_card = trump;
                    room.UpdateTurnInfo();
                    room.TurnInfoText.Text = $"Current: {currentTurn}\nTrump: {room.currentTrump}";
                    if (currentTurn != user.login)
                    {
                        room.NextButton.IsEnabled = false;
                        room.CheckButton.IsEnabled = false;
                        room.CheckButton.Foreground = Brushes.Gray;
                        room.NextButton.Foreground = Brushes.Gray;
                    }
                    else
                    {
                        room.NextButton.IsEnabled = true;
                        room.CheckButton.IsEnabled = true;
                        room.CheckButton.Foreground = Brushes.White;
                        room.NextButton.Foreground = Brushes.White;
                    }
                });
                
            }
            private void HandlePlayerAction(string username, List<int> cards)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Проверяем, что ход текущего игрока совпадает с отправителем
                    if (game.queue[game.currentPlayerIndex] == username)
                    {
                        // Обновляем карты игрока
                        game.Update(cards, username);
                    }
                });
            }
            private string GetTrumpCardName(string trump)
            {
                return trump switch
                {
                    "a" => "Ace",
                    "k" => "King",
                    "q" => "Queen",
                    _ => "Unknown"
                };
            }
            private async void Connect()
            {
                try
                {
                    await client.ConnectAsync(IPTextBox.Text);
                    Enter.IsEnabled = false;
                    IPTextBox.IsEnabled = false;
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }


            private void Enter_MouseDown(object sender, MouseButtonEventArgs e)
            {
                Connect();
                
            }

            

            private void Exit_MouseDown(object sender, MouseButtonEventArgs e)
            {
                Application.Current.Shutdown();
            }
            private void StartButton_Click(object sender, RoutedEventArgs e)
            {
                if (game != null && game.isPlaying) return;

                if (game.Start())
                {
                    room.CurrentDeck = game.GetPlayersHands()[user.login];
                    room.StartButton.IsEnabled = false;
                    room.UpdateAllPlayersCards(game.GetPlayersHands());
                    room.NextButton.IsEnabled = true;
                    room.NextButton.Foreground = Brushes.White;
                    room.CheckButton.IsEnabled = true;
                    room.CheckButton.Foreground = Brushes.White;
                }
            }
            private void CheckButton_Click(object sender, RoutedEventArgs e)
            {
                if (game.current_cards == null || game.current_cards.Length == 0)
                {
                    MessageBox.Show("Нельзя проверять карты, если их ещё не положили");
                    return;
                }

                if (game.Check())
                {
                    MessageBox.Show("Предыдущий игрок не врал!");
                }
                else
                {
                    MessageBox.Show("Предыдущий игрок пытался вас обмануть!");
                }
                room.ShowCardsInCenter(game.current_cards, true);
            }
            private async void NextButton_Click(object sender, RoutedEventArgs e)
            {
                if (room.selectedCardIndices[user.login].Count != 0)
                {
                    if (isHost)
                    {
                        
                        game.Update(room.selectedCardIndices[user.login]);
                    }
                    else
                    {
                        var cards = string.Join(",", room.selectedCardIndices[user.login]);
                        string message = $"PLAYER_TURN:{user.login}:{cards}";
                        client.WriteAsync(message);
                    }
                    room.selectedCardIndices[user.login].Clear();

                }
                else MessageBox.Show("Для завершения хода нужно либо выбрать от 1 до 3 карт, либо проверить карты предыдущего игрока");
                
                
            }
        }
    }