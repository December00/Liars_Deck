        using Liars_deck.classes;
        using System;
        using System.Media;
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
                private List<string> allPlayers;
                private string hostName;
                SoundPlayer player = new SoundPlayer("D:\\sharpCodes\\Liars_deck\\Liars_deck\\resources\\cardsound.wav");
                public GameWindow(Server server, User user)
                {
                    InitializeComponent();
                    this.user = user;
                    this.client = new Client(user);
                    this.isHost = true;
                    this.room = new Room(MainGrid) { CurrentUsername = user.login };
                    this.room.server = server;
                    this.room.server.Start(8000);
                    IPTextBox.Text = "127.0.0.1";
                    //IPTextBox.Text = room.server.ip.ToString();
                    this.room.AddHostPlayer(user.login);
                    room.InitializeButtons();
                    this.client.OnPlayerConnected += OnPlayerConnectedHandler;
                    this.client.OnPlayerListReceived += OnPlayerListReceivedHandler;
                    this.client.OnCardsReceived += OnCardsReceivedHandler;
                    this.client.OnCardsToCenter += OnCardsToCenterHandler;
                    this.client.OnTurnChanged += OnTurnChangedHandler;
                    this.client.OnCheckResultReceived += OnCheckResultReceivedHandler;
                    this.client.OnDisconnected += HandleDisconnection;
                    this.room.server.OnClientAction += HandlePlayerAction;
                    this.room.server.OnCheckRequest += HandleCheckRequest;
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
                    this.client.OnCheckResultReceived += OnCheckResultReceivedHandler;
                    this.client.OnDisconnected += HandleDisconnection;
                    this.game = new Game(room);
                    room.NextButton.Click += NextButton_Click;
                    room.CheckButton.Click += CheckButton_Click;
                    room.NextButton.IsEnabled = false;
                    room.CheckButton.IsEnabled = false;
                    room.CheckButton.Foreground = Brushes.Gray;
                    room.NextButton.Foreground = Brushes.Gray;
                }
                
                
                private void OnPlayerConnectedHandler(string message)
                {
                    if (message.StartsWith("PLAYER_DISCONNECTED:"))
                    {
                        string username = message.Substring(20);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            room.RemovePlayerUI(username);
                            game.HandlePlayerDisconnect(username);
                            if (hostName != null)
                            {
                                if (username == hostName)
                                {
                                    MessageBox.Show("Соединение с сервером потеряно, ваш сеанс будет окончен");
                                    Application.Current.Shutdown(0);
                                }
                            }
                            else
                            {
                                List<string> Players = room.clientElements.Keys.ToList();
                                if (username == Players[0])
                                {

                                }
                            }
                            if (isHost)
                            {
                                if (game.isPlaying)
                                {
                                    if (room.clientElements.Count > 1)
                                    {
                                        game.Restart();
                                    }
                                    else
                                    {
                                        MessageBox.Show("Все игроки покинули комнату, ваш сеанс будет окончен");
                                        Application.Current.Shutdown(0);
                                    }
                                }
                            }
                            if (game.trump_card != null)    
                            {
                                TextWindow textWindow =
                                    new TextWindow("Игра была начата заново, так как игрок " + username + " отключился");
                                textWindow.Show();
                            }
                        });
                        return;
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (!room.clientElements.ContainsKey(message))
                        {
                            room.AddClientUI(message);
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
                        player.Play();
                        
                    });
                }
                private void OnCardsToCenterHandler(string cards)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        room.ShowCardsInCenter(cards);
                        game.current_cards = cards;
                        if (room.selectedCardIndices.ContainsKey(user.login))
                            room.selectedCardIndices[user.login].Clear();
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
                        room.TurnInfoText.Text = room.currentTrump;
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
                        allPlayers = room.clientElements.Keys.ToList();
                        foreach (var player in allPlayers)
                        {
                            if (currentTurn == player)
                            {
                                room.clientElements[player].Item1.Fill =  new SolidColorBrush(Color.FromRgb(0x16, 0x16, 0x16 ));
                            }
                            else
                            {
                                room.clientElements[player].Item1.Fill =  new SolidColorBrush(Color.FromRgb(0x42, 0x42, 0x42));
                            }
                        }

                        if (hostName == null)
                        {
                            hostName = currentTurn;
                        }
                    });
                }
                private void OnCheckResultReceivedHandler(string liarUsername, string cards, bool isHonest)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {

                        room.ShowCardsInCenter(cards, true, liarUsername, isHonest);
                        game.HandleCheckResult(liarUsername);
                        room.NextButton.IsEnabled = false;
                        room.CheckButton.IsEnabled = false;
                        room.CheckButton.Foreground = Brushes.Gray;
                        room.NextButton.Foreground = Brushes.Gray;
                        if (isHost)
                        {
                            
                            room.StartButton.Foreground = Brushes.White;
                            room.StartButton.IsEnabled = true;
                            
                        }
                    });
                }
                private void HandleDisconnection()
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TextWindow textWindow = new TextWindow("Соединение с сервером потеряно");
                        textWindow.Show();
                        Close();
                    });
                }
            private void HandlePlayerAction(string username, List<int> cards)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (game.queue[game.currentPlayerIndex] == username)
                        {
                            game.Update(cards, username);
                        }
                    });
                }
                private void HandleCheckRequest(string username)
                {
                    ProcessCheck();
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
                    Enter.IsEnabled = true;
                    IPTextBox.IsEnabled = true;
                    }
                }


                private void Enter_MouseDown(object sender, MouseButtonEventArgs e)
                {
                    Connect();
                    
                }

                

                private async void Exit_MouseDown(object sender, MouseButtonEventArgs e)
                {
                    if (isHost)
                    {
                        await room.server.BroadcastPlayerDisconnected(user.login);
                    }
                    Application.Current.Shutdown(0);
                }
            private void StartButton_Click(object sender, RoutedEventArgs e)
            {
                
                if (game.isPlaying)
                {
                    game.Restart();
                    room.CurrentDeck = game.GetPlayersHands()[user.login];
                    room.StartButton.IsEnabled = false;
                    room.StartButton.Foreground = Brushes.Gray;
                    room.UpdateAllPlayersCards(game.GetPlayersHands());
                    room.NextButton.IsEnabled = true;
                    room.NextButton.Foreground = Brushes.White;
                    room.CheckButton.IsEnabled = true;
                    room.CheckButton.Foreground = Brushes.White;
                }
                else
                {
                    if (game.Start())
                    {
                        room.CurrentDeck = game.GetPlayersHands()[user.login];
                        room.StartButton.IsEnabled = false;
                        room.StartButton.Foreground = Brushes.Gray;
                        room.UpdateAllPlayersCards(game.GetPlayersHands());
                        room.NextButton.IsEnabled = true;
                        room.NextButton.Foreground = Brushes.White;
                        room.CheckButton.IsEnabled = true;
                        room.CheckButton.Foreground = Brushes.White;
                    }
                }
            }
                private void CheckButton_Click(object sender, RoutedEventArgs e)
                {
                    if (game.current_cards == null || game.current_cards.Length == 0)
                    {
                        TextWindow textWindow = new TextWindow("Нельзя проверять карты, если их ещё не положили");
                        textWindow.Show();
                        return;
                    }

                    if (isHost)
                    {
                        ProcessCheck();
                    }
                    else
                    {
                        client.WriteAsync($"CHECK_REQUEST:{user.login}");
                    }
                }
                private void ProcessCheck()
                {
                    bool checkResult = game.Check();
                    int index = game.currentPlayerIndex - 1 == 0 ? game.queue.Count : game.currentPlayerIndex - 1;
                    string liarUsername = game.queue[index];
        
                    room.ShowCardsInCenter(game.current_cards, true, liarUsername, checkResult);
                    game.HandleCheckResult(checkResult ? "no_liar" : liarUsername);

                    if (isHost)
                    {
                        room.server.BroadcastCheckResult(liarUsername, game.current_cards, checkResult);
                    }
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
                    else
                    {
                        TextWindow textWindow = new TextWindow("Для завершения хода нужно либо выбрать от 1 до 3 карт, либо проверить карты предыдущего игрока");
                        textWindow.Show();
                    }

                    
                    
                }
            }
        }