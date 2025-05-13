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
            throw new Exception($"Ошибка подключения: {ex.Message}");
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

                    OnCardsReceived?.Invoke(cardsData);
                }
                else
                {
                    OnPlayerConnected?.Invoke(message);
                }
            }
            catch
            {
                break;
            }
        }

    }
    private void Disconnect()
    {
        stream?.Close();
        client?.Close();
        MessageBox.Show("Комната заполнена");
    }
}