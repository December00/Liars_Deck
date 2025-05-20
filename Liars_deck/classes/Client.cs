    using Liars_deck.classes;
    using System.Net.Sockets;
    using System.Text;
    using System.Windows;

    public class Client
    {
        private TcpClient client;
        private NetworkStream stream;
        public User user;
        public event Action<string> OnPlayerConnected;
        public event Action<string> OnPlayerListReceived;
        public event Action<Dictionary<string, string>> OnCardsReceived;
        public event Action<string> OnCardsToCenter;
        public event Action<string, string> OnTurnChanged;
        public event Action<string, string, bool> OnCheckResultReceived;
        public event Action OnDisconnected;
        public string current_deck = "";

        public Client(User user)
        {
            this.user = user;
        }

        public async Task ConnectAsync(string ip)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(ip, 8000);
                stream = client.GetStream();

                byte[] data = Encoding.UTF8.GetBytes(user.login);
                await stream.WriteAsync(data, 0, data.Length);

                _ = Task.Run(ReceiveMessagesAsync);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения, возможно игра уже началась");
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                try
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    if (message.StartsWith("ERROR:"))
                    {
                        MessageBox.Show(message.Substring(6));
                        Disconnect();
                        return;
                    }
                    else if (message.StartsWith("PLAYERLIST:"))
                    {
                        OnPlayerListReceived?.Invoke(message.Substring(11));
                    }
                    else if (message.StartsWith("CARDS:"))
                    {
                        var cardsData = message.Substring(6)
                            .Split(';')
                            .Select(part => part.Split(':'))
                            .ToDictionary(parts => parts[0], parts => parts[1]);

                        if (cardsData.TryGetValue(user.login, out var myCards))
                        {
                            current_deck = myCards;
                        }

                        OnCardsReceived?.Invoke(cardsData);
                    }
                    else if (message.StartsWith("CARDS_TO_CENTER:"))
                    {
                        OnCardsToCenter?.Invoke(message.Substring(16));
                    }
                    else if (message.StartsWith("TURN:"))
                    {
                        var parts = message.Substring(5).Split(':');
                        OnTurnChanged?.Invoke(parts[0], parts[1]);
                    }
                    else if (message.StartsWith("CHECK_RESULT:"))
                    {
                        var parts = message.Split(':');
                        string liarUsername = parts[1];
                        string cards = parts[2];
                        bool isHonest = bool.Parse(parts[3]);
                        OnCheckResultReceived?.Invoke(liarUsername, cards, isHonest);
                    }
                    else
                    {
                        OnPlayerConnected?.Invoke(message);
                    }
                }
                catch
                {
                    OnDisconnected?.Invoke();
                    break;
                }
            }

        }

        public async void WriteAsync(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(data, 0, data.Length);
            
           
        }
        private void Disconnect()
        {
            stream?.Close();
            client?.Close(); 
            OnDisconnected?.Invoke();
    }
    }