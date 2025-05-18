using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Shapes;

namespace Liars_deck.classes
{
    public class Server
    {

        private TcpListener? listener;
        public List<TcpClient> clients = new List<TcpClient>();
        public Dictionary<TcpClient, string> clientUsers = new Dictionary<TcpClient, string>();
        public IPAddress ip = IPAddress.Any;
        public bool isAcceptingClients = true;
        public event Action<string>? OnClientConnected;
        public event Action<string, List<int>> OnClientAction;
        public event Action<string> OnCheckRequest;
        public string hostname;
        private const int MAX_PLAYERS = 4;
        public void Start(int port, string name)
        {
            try
            {
                listener = new TcpListener(ip, port);
                listener.Start();
                this.hostname = name;
                Task.Run(() => AcceptClients());
            }
            catch
            {
                MessageBox.Show("Порт занят");
            }
        }
        public void StopAcceptingClients()
        {
            isAcceptingClients = false;
            listener.Stop(); 
        }
        private async Task AcceptClients()
        {
            while (isAcceptingClients)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                if (clients.Count >= MAX_PLAYERS)
                {
                    SendRejectionMessage(client, "Комната заполнена");
                    continue;
                }
                clients.Add(client);
                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
        }

        private void HandleClient(TcpClient newClient)
        {
            NetworkStream stream = newClient.GetStream();
            byte[] buffer = new byte[1024];

            while (true)
            {
                try
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    if (!clientUsers.ContainsKey(newClient))
                    {
                        clientUsers[newClient] = message;
                        OnClientConnected?.Invoke(message);

                        // Отправляем новому клиенту список всех игроков
                        SendPlayerList(newClient);

                        // Уведомляем остальных о новом игроке
                        NotifyOthersAboutNewPlayer(newClient, message);
                    }
                    else if (message.StartsWith("PLAYER_TURN:"))
                    {
                        var parts = message.Split(':');
                        string username = parts[1];
                        List<int> cards = parts[2].Split(',').Select(int.Parse).ToList();

                        var hostClient = clients.First();
                        string modifiedMessage = $"PLAYER_TURN:{username}:{string.Join(",", cards)}";
                        byte[] data = Encoding.UTF8.GetBytes(modifiedMessage);
                        hostClient.GetStream().Write(data, 0, data.Length);
                        OnClientAction?.Invoke(username, cards);
                    }
                    else if (message.StartsWith("CARDS_ACTION:"))
                    {
                        string username = clientUsers[newClient];
                        string modifiedMessage = $"CARDS_ACTION:{username}:{message.Substring(13)}";
                        var hostClient = clients.First();
                        byte[] data = Encoding.UTF8.GetBytes(modifiedMessage);
                        hostClient.GetStream().Write(data, 0, data.Length);
                    }
                    else if (message.StartsWith("CHECK_REQUEST:"))
                    {
                        string username = message.Split(':')[1];
                        OnCheckRequest?.Invoke(username);
                    }
                }
                catch
                {
                    return;
                }
            }
        }

        private void SendPlayerList(TcpClient client)
        {
            var stream = client.GetStream();
            string playerList = string.Join(",", clientUsers.Values);
            byte[] data = Encoding.UTF8.GetBytes($"PLAYERLIST:{playerList}");
            stream.Write(data, 0, data.Length);
        }

        private void NotifyOthersAboutNewPlayer(TcpClient newClient, string newPlayerName)
        {
            foreach (var client in clients)
            {
                if (client != newClient && client.Connected)
                {
                    var stream = client.GetStream();
                    byte[] data = Encoding.UTF8.GetBytes(newPlayerName);
                    stream.Write(data, 0, data.Length);
                }
            }
        }
        public async Task BroadcastPlayersCards(Dictionary<string, string> players)
        {
            try
            {
                string message = "CARDS:" + string.Join(";", players.Select(p => $"{p.Key}:{p.Value}"));
                
                byte[] data = Encoding.UTF8.GetBytes(message);
                
                foreach (var client in clients)
                {
                    if (client.Connected)
                    {
                        await client.GetStream().WriteAsync(data, 0, data.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка рассылки карт: {ex.Message}");
            }
        }
        public async Task BroadcastCardsToCenter(string cards)
        {
            try
            {
                string message = $"CARDS_TO_CENTER:{cards}";
                
                
                await Task.Delay(500); 
                byte[] data = Encoding.UTF8.GetBytes(message);
                
                foreach (var client in clients)
                {
                    if (client.Connected)
                    {
                        await client.GetStream().WriteAsync(data, 0, data.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка рассылки карт: {ex.Message}");
            }
        }
        public async Task BroadcastTurnInfo(string currentPlayer, string trumpCard)
        {
            try
            {
                string message = $"TURN:{currentPlayer}:{trumpCard}";
                await Task.Delay(300); 

                //MessageBox.Show(message);
                byte[] data = Encoding.UTF8.GetBytes(message);

                foreach (var client in clients)
                {
                    if (client.Connected)
                    {
                        await client.GetStream().WriteAsync(data, 0, data.Length);
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка рассылки хода: {ex.Message}");
            }
        }
        public async Task BroadcastCheckResult(string liarUsername, string cards, bool isHonest)
        {
            try
            {
                string message = $"CHECK_RESULT:{liarUsername}:{cards}:{isHonest}";
                byte[] data = Encoding.UTF8.GetBytes(message);
        
                foreach (var client in clients)
                {
                    if (client.Connected)
                    {
                        await client.GetStream().WriteAsync(data, 0, data.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка рассылки результата проверки: {ex.Message}");
            }
        }
        private void SendRejectionMessage(TcpClient client, string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes($"ERROR:{message}");
                client.GetStream().Write(data, 0, data.Length);
                client.Close();
            }
            catch { }
        }
    }
}