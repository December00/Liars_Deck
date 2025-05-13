using Liars_deck.classes;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Windows.Input;

public class Room
{
    public Server server;
    private Grid gameGrid;
    public Dictionary<string, Tuple<Rectangle, Label>> clientElements = new Dictionary<string, Tuple<Rectangle, Label>>();
    public Button StartButton { get; private set; }
    public Button CheckButton { get; private set; }
    public Button NextButton { get; private set; }
    public Room(Grid grid)
    {
        gameGrid = grid;
        InitializeGridStructure();
        server = new Server();
        server.OnClientConnected += AddClientUI;
    }

    private void InitializeGridStructure()
    {
        gameGrid.RowDefinitions.Clear();
        gameGrid.ColumnDefinitions.Clear();

        for (int i = 0; i < 3; i++)
        {
            gameGrid.RowDefinitions.Add(new RowDefinition());
        }

        for (int i = 0; i < 4; i++)
        {
            gameGrid.ColumnDefinitions.Add(new ColumnDefinition());
        }
    }
    public void InitializeButtons()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            StartButton = CreateButton("Начать", HorizontalAlignment.Left,
                new Thickness(20, 0, 0, 20), "StartButton");
            Grid.SetRow(StartButton, 2);
            Grid.SetColumn(StartButton, 0);

            CheckButton = CreateButton("Проверить", HorizontalAlignment.Right,
                new Thickness(0, 0, 20, 120), "CheckButton");
            Grid.SetRow(CheckButton, 2);
            Grid.SetColumn(CheckButton, 3);

            NextButton = CreateButton("Завершить ход", HorizontalAlignment.Right,
                new Thickness(0, 0, 20, 20), "NextButton");
            Grid.SetRow(NextButton, 2);
            Grid.SetColumn(NextButton, 3);

            gameGrid.Children.Add(StartButton);
            gameGrid.Children.Add(CheckButton);
            gameGrid.Children.Add(NextButton);
        });
    }
    private Button CreateButton(string content, HorizontalAlignment alignment,
                              Thickness margin, string name)
    {
        Button button = new Button
        {
            Content = content,
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = alignment,
            Margin = margin,
            Width = 320,
            Height = 60,
            Foreground = Brushes.White,
            FontFamily = new FontFamily("Arial Black"),
            FontSize = 28,
            FontWeight = FontWeights.Bold,
            Cursor = Cursors.Hand,
            Name = name
        };

        ControlTemplate template = new ControlTemplate(typeof(Button));
        FrameworkElementFactory grid = new FrameworkElementFactory(typeof(Grid));

        FrameworkElementFactory rect = new FrameworkElementFactory(typeof(Rectangle));
        rect.SetValue(Rectangle.FillProperty, new SolidColorBrush(Color.FromRgb(0x42, 0x42, 0x42)));
        rect.SetValue(Rectangle.RadiusXProperty, 10.0);
        rect.SetValue(Rectangle.RadiusYProperty, 10.0);
        rect.SetValue(Rectangle.EffectProperty, new DropShadowEffect
        {
        });

        FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
        contentPresenter.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        contentPresenter.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);

        grid.AppendChild(rect);
        grid.AppendChild(contentPresenter);

        template.VisualTree = grid;
        button.Template = template;

        return button;
    }


    public void AddHostPlayer(string username)
    {
        AddClientUI(username);
    }

    public void AddClientUI(string username)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (clientElements.Count >= 4) 
            {
                MessageBox.Show("Комната заполнена");
                return;
            }
            if (clientElements.ContainsKey(username)) return;

            Rectangle rectangle = CreateClientRectangle();

            Label label = CreateClientLabel(username);

            int position = clientElements.Count + 1;

            gameGrid.Children.Add(rectangle);
            gameGrid.Children.Add(label);

            PositionUIElement(rectangle, label, position);

            clientElements[username] = Tuple.Create(rectangle, label);
        });
    }

    private Rectangle CreateClientRectangle()
    {
        return new Rectangle
        {
            Fill = new SolidColorBrush(Color.FromRgb(0x42, 0x42, 0x42)),
            RadiusX = 10,
            RadiusY = 10,
            Width = 360,
            Height = 60,
            Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                Direction = 320,
                ShadowDepth = 10,
                BlurRadius = 20,
                Opacity = 0.7
            }
        };
    }

    private Label CreateClientLabel(string username)
    {
        return new Label
        {
            Content = username,
            Foreground = Brushes.White,
            FontSize = 24,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontWeight = FontWeights.Bold
        };
    }

    private void PositionUIElement(Rectangle rectangle, Label label, int position)
    {
        switch (position)
        {
            case 1: 
                SetPosition(rectangle, label, 2, 1, 2,
                    HorizontalAlignment.Center, VerticalAlignment.Bottom,
                    new Thickness(0, 0, 0, 20));
                break;

            case 2: 
                SetPosition(rectangle, label, 1, 0, 1,
                    HorizontalAlignment.Center, VerticalAlignment.Bottom,
                    new Thickness(20, 0, 0, 0));
                break;

            case 3: 
                SetPosition(rectangle, label, 0, 1, 2,
                    HorizontalAlignment.Center, VerticalAlignment.Bottom,
                    new Thickness(0, 20, 0, 0));
                break;

            case 4:
                SetPosition(rectangle, label, 1, 3, 1,
                    HorizontalAlignment.Center, VerticalAlignment.Bottom,
                    new Thickness(0, 0, 20, 0));
                break;
        }
    }

    private void SetPosition(FrameworkElement element, Label label,
        int row, int column, int columnSpan,
        HorizontalAlignment hAlign, VerticalAlignment vAlign, Thickness margin)
    {
        Grid.SetRow(element, row);
        Grid.SetColumn(element, column);
        Grid.SetColumnSpan(element, columnSpan);
        element.HorizontalAlignment = hAlign;
        element.VerticalAlignment = vAlign;
        element.Margin = margin;

        Grid.SetRow(label, row);
        Grid.SetColumn(label, column);
        Grid.SetColumnSpan(label, columnSpan);
        label.HorizontalAlignment = hAlign;
        label.VerticalAlignment = vAlign;
        label.Margin = margin;
    }
    public void UpdateCardsForAllPlayers(Dictionary<string, string> playersCards)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var cardsPanels = gameGrid.Children
                .OfType<StackPanel>()
                .Where(sp => sp.Tag?.ToString() == "cardsPanel")
                .ToList();

            foreach (var panel in cardsPanels)
            {
                gameGrid.Children.Remove(panel);
            }

            foreach (var player in playersCards)
            {
                DrawCardsForPlayer(player.Key, player.Value);
            }
        });
    }
    public void UpdateAllPlayersCards(Dictionary<string, string> playersCards)
    {
        // Обновляем локально
        UpdateCardsForAllPlayers(playersCards);

        // Рассылаем клиентам только если мы хост
        if (server != null)
        {
            _ = server.BroadcastPlayersCards(playersCards);
        }
    }

    public void DrawCardsForPlayer(string username, string cards)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (!clientElements.ContainsKey(username)) return;

            var (rectangle, label) = clientElements[username];
            int row = Grid.GetRow(rectangle);
            int column = Grid.GetColumn(rectangle);
            int columnSpan = Grid.GetColumnSpan(rectangle);

            StackPanel cardsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, rectangle.Height + 10),
                Tag = "cardsPanel" 
            };

            for (int i = 0; i < Math.Min(5, cards.Length); i++)
            {
                string cardImagePath = GetCardImagePath(cards[i]);
                Image cardImage = new Image
                {
                    Source = new BitmapImage(new Uri(cardImagePath)),
                    Width = 80,
                    Height = 96,
                    Margin = new Thickness(0, 0, 0, 0),

                };

                cardsPanel.Children.Add(cardImage);
            }

            Grid.SetRow(cardsPanel, row);
            Grid.SetColumn(cardsPanel, column);
            Grid.SetColumnSpan(cardsPanel, columnSpan);
            gameGrid.Children.Add(cardsPanel);
        });
    }
    private string GetCardImagePath(char card)
    {
        return card switch
        {
            'a' => "D:\\sharpCodes\\Liars_deck\\Liars_deck\\resources\\ace.png",
            'k' => "D:\\sharpCodes\\Liars_deck\\Liars_deck\\resources\\king.png",
            'q' => "D:\\sharpCodes\\Liars_deck\\Liars_deck\\resources\\queen.png",
            'j' => "D:\\sharpCodes\\Liars_deck\\Liars_deck\\resources\\joker.png",
            _ => "pack://application:,,,/resources/Bluecard.png"
        };
    }
}