using Liars_deck.classes;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Liars_deck
{
    public partial class GameWindow : Window
    {
        private Game game;
        private readonly Room room;
        private readonly User user;
        private readonly bool isHost;
        private Client gameClient;

        public GameWindow(Server server, User user)
        {
            InitializeComponent();
            InitializeMainGrid();

            this.user = user;
            this.isHost = true;
            this.room = new Room(MainGrid);
            this.room.server = server;
            this.room.server.OnClientConnected += room.AddClientUI;
            this.Enter.Visibility = Visibility.Collapsed;
            this.room.server.Start(8000, user.login);
            this.room.AddHostPlayer(user.login);

            IPTextBox.Text = server.ip.ToString();
            IPTextBox.IsEnabled = false;
            Enter.IsEnabled = false;

            room.InitializeButtons();
            room.StartButton.Click += StartButton_Click;
        }
        public GameWindow(User user)
        {
            InitializeComponent();
            InitializeMainGrid();

            this.user = user;
            this.isHost = false;
            this.room = new Room(MainGrid);
            this.gameClient = new Client(user);

            this.gameClient.OnPlayerConnected += OnPlayerConnectedHandler;
            this.gameClient.OnPlayerListReceived += OnPlayerListReceivedHandler;
            this.gameClient.OnCardsReceived += OnCardsReceivedHandler;
            IPTextBox.Text = "127.0.0.1";

            room.InitializeButtons();
            room.StartButton.Visibility = Visibility.Collapsed;
        }

        private void InitializeMainGrid()
        {
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();

            for (int i = 0; i < 3; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < 4; i++)
            {
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
        }

        private async void Enter_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!isHost)
                {
                    await gameClient.ConnectAsync(IPTextBox.Text);
                }

                Enter.IsEnabled = false;
                IPTextBox.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnPlayerConnectedHandler(string username)
        {
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
            Application.Current.Dispatcher.Invoke(() =>
            {
                room.UpdateCardsForAllPlayers(playersCards);
            });
        }

        private void Exit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (!isHost && gameClient != null)
            {
                gameClient.OnPlayerConnected -= OnPlayerConnectedHandler;
                gameClient.OnPlayerListReceived -= OnPlayerListReceivedHandler;
            }

            if (isHost && room?.server != null)
            {
                room.server.OnClientConnected -= room.AddClientUI;
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            game = new Game(room);
            game.Start();

            room.UpdateAllPlayersCards(game.GetPlayersHands());
        }
    }
}