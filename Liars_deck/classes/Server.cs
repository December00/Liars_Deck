using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace Liars_deck.classes
{
    public class Server
    {
        private TcpListener? listener;
        private List<TcpClient> clients = new List<TcpClient>();
        public void Start(int port)
        {
            try
            {
                IPAddress anyIP = IPAddress.Any;
                listener = new TcpListener(anyIP, port);
                listener.Start();
                MessageBox.Show("Сервер запущен на IP: " + anyIP + " порт: " + port);
                return;
            }
            catch
            {
                MessageBox.Show("Порт занят");
            }

        }
        private async void AcceptClients()
        {
            while (true)
            {
                
                 TcpClient client = await listener.AcceptTcpClientAsync();
                 clients.Add(client);
                 await Task.Run(() => HandleClient(client));

            }
            

        }
        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // Рассылка сообщения всем игрокам
                //Broadcast(message);
            }

        }
    }
}
