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
        public event Action<string>? OnClientConnected;
        public string hostname;
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

        private async Task AcceptClients()
        {
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
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
            string playerList = hostname + "," + string.Join(",", clientUsers.Values);
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
    }
}