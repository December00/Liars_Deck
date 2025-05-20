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
    public TextBlock TurnInfoText { get; private set; }
    public string CurrentUsername { get; set; }
    public string CurrentDeck { get; set; } = "";
    public string currentTurn;
    public string currentTrump;
    public Dictionary<string, List<int>> selectedCardIndices = new Dictionary<string, List<int>>();

    public Room(Grid grid)
    {
        gameGrid = grid;
        InitializeGridStructure();
        InitializeTurnInfo();
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

    public void InitializeTurnInfo()
    {
        TurnInfoText = new TextBlock
        {
            Foreground = Brushes.White,
            FontSize = 20,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 10, 20, 0),
            FontWeight = FontWeights.Bold,
        };
        Grid.SetRow(TurnInfoText, 0);
        Grid.SetColumn(TurnInfoText, 3);
        gameGrid.Children.Add(TurnInfoText);
    }

    public void UpdateTurnInfo()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
        
            TurnInfoText.Text = $"Current: {currentTurn}\nTrump: {currentTrump}";
            TurnInfoText.Visibility = Visibility.Visible;
        });
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
        if (username.Contains(":")) return;
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
    public void RemovePlayerUI(string username)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (clientElements.TryGetValue(username, out var elements))
            {
                gameGrid.Children.Remove(elements.Item1);
                gameGrid.Children.Remove(elements.Item2);
                clientElements.Remove(username);

                var panels = gameGrid.Children.OfType<StackPanel>()
                    .Where(sp => Grid.GetRow(sp) == Grid.GetRow(elements.Item1) &&
                                Grid.GetColumn(sp) == Grid.GetColumn(elements.Item1))
                    .ToList();

                foreach (var panel in panels)
                {
                    gameGrid.Children.Remove(panel);
                }
            }
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
        if (playersCards.TryGetValue(CurrentUsername, out string myCards))
        {
            CurrentDeck = myCards;
        }
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

        if (server != null)
        {
            _ = server.BroadcastPlayersCards(playersCards);
        }
    }

    public void DrawCardsForPlayer(string username, string cards, bool isCheck = false)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (!clientElements.ContainsKey(username)) return;

            var (rectangle, label) = clientElements[username];
            bool isCurrentUser = username == CurrentUsername;

            // Удаляем предыдущие карты
            var panelsToRemove = gameGrid.Children
                .OfType<StackPanel>()
                .Where(sp => Grid.GetRow(sp) == Grid.GetRow(rectangle)
                          && Grid.GetColumn(sp) == Grid.GetColumn(rectangle))
                .ToList();

            foreach (var panel in panelsToRemove)
            {
                gameGrid.Children.Remove(panel);
            }

            StackPanel cardsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 0, 0, rectangle.Height + 10),
                Tag = "cardsPanel"
            };

            string cardsToShow = (username == CurrentUsername || isCheck) ? CurrentDeck : new string('x', cards.Length);

            for (int i = 0; i < Math.Min(5, cardsToShow.Length); i++)
            {
                string cardImagePath = GetCardImagePath(cardsToShow[i]);
                Image cardImage = new Image
                {
                    Source = new BitmapImage(new Uri(cardImagePath)),
                    Width = 80,
                    Height = 96,
                    Margin = new Thickness(0, 0, 0, 0),
                    Tag = i
                };
                if (!cardsToShow.Contains('x')){
                    cardImage.MouseDown += (sender, e) =>
                    {
                        int index = (int)((Image)sender).Tag;
                        ToggleCardSelection(username, index);
                    };
                }
                cardsPanel.Children.Add(cardImage);
            }

            Grid.SetRow(cardsPanel, Grid.GetRow(rectangle));
            Grid.SetColumn(cardsPanel, Grid.GetColumn(rectangle));
            Grid.SetColumnSpan(cardsPanel, Grid.GetColumnSpan(rectangle));
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
            _ => "D:\\sharpCodes\\Liars_deck\\Liars_deck\\resources\\Bluecard.png"
        };
    }
    private void ToggleCardSelection(string username, int index)
    {
        if (!selectedCardIndices.ContainsKey(username))
            selectedCardIndices[username] = new List<int>();

        var indices = selectedCardIndices[username];

        if (indices.Contains(index))
        {
            indices.Remove(index);
            UpdateCardVisual(username, index, false);
        }
        else if (indices.Count < 3)
        {
            indices.Add(index);
            UpdateCardVisual(username, index, true);
        }
    }

    private void UpdateCardVisual(string username, int index, bool selected)
    {
        var playerElement = clientElements[username].Item1;
        var panel = gameGrid.Children.OfType<StackPanel>()
            .FirstOrDefault(p =>
                Grid.GetRow(p) == Grid.GetRow(playerElement) &&
                Grid.GetColumn(p) == Grid.GetColumn(playerElement) &&
                Grid.GetColumnSpan(p) == Grid.GetColumnSpan(playerElement));

        if (panel != null && index < panel.Children.Count)
        {
            var image = (Image)panel.Children[index];
            image.Effect = selected ? new DropShadowEffect
            {
                Color = Colors.Yellow,
                BlurRadius = 20
            } : null;
            image.Margin = selected ? new Thickness(0, -10, 0, 0) : new Thickness(0, 0, 0, 0);
        }
    }
    public void ShowCardsInCenter(string cards, bool isCheck = false, string liarUsername = "", bool isHonest = false)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var centerPanel = gameGrid.Children
                .OfType<StackPanel>()
                .FirstOrDefault(sp => sp.Name == "CenterCards");

            if (centerPanel != null) gameGrid.Children.Remove(centerPanel);

            StackPanel newPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Name = "CenterCards"
            };

            // Панель с картами
            StackPanel cardsPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            foreach (char c in cards)
            {
                string cardImagePath = isCheck ? 
                    GetCardImagePath(c) : 
                    "D:\\sharpCodes\\Liars_deck\\Liars_deck\\resources\\Bluecard.png";
                
                Image card = new Image
                {
                    Source = new BitmapImage(new Uri(cardImagePath)),
                    Width = 80,
                    Height = 96,
                    Margin = new Thickness(5)
                };
                cardsPanel.Children.Add(card);
            }
            newPanel.Children.Add(cardsPanel);

            // Текст с результатом
            if (isCheck)
            {
                string resultText = isHonest ? 
                    $"Игрок {liarUsername} честен" : 
                    $"Игрок {liarUsername} лгал!";
                
                TextBlock resultBlock = new TextBlock
                {
                    Text = resultText,
                    Foreground = Brushes.White,
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                newPanel.Children.Add(resultBlock);
            }

            Grid.SetRow(newPanel, 1);
            Grid.SetColumn(newPanel, 1);
            Grid.SetColumnSpan(newPanel, 2);
            gameGrid.Children.Add(newPanel);
        });
    }
}