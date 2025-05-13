using Liars_deck.classes;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Liars_deck;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private bool isRegMenu = false;
    public MainWindow()
    {
        InitializeComponent();
        this.KeyDown += MainWindow_KeyDown;
    }
    private void ChangeMenu()
    {
        if (isRegMenu)
        {
            this.GlateLabel.Content = "Мы рады видеть вас снова!";
            this.WelcomeLabel.Content = "С возвращением!";
            this.HyperLabel.Content = "Зарегистрироваться";
            this.Button.Content = "Вход";
            isRegMenu = !isRegMenu;
        }
        else
        {
            this.GlateLabel.Content = "Создайте себе новый аккаунт.";
            this.WelcomeLabel.Content = "Добро пожаловать!";
            this.HyperLabel.Content = "Войти в аккаунт";
            this.Button.Content = "Регистрация";
            isRegMenu = !isRegMenu;
        }
    }
    private void AuthRegOperation()
    {
        User user = new User(this.LoginTextBox.Text, this.PasTextBox.Text);
        if (!isRegMenu)
        {

            if (user.Authorization())
            {
                this.Hide();
                MenuWindow menuWindow = new MenuWindow(user);
                menuWindow.Show();
            }
        }
        else
        {
            if (user.Registration())
            {
                MessageBox.Show("Успешная регистрация");
                this.PasTextBox.Text = "";
                ChangeMenu();
            }
        }
    }
    private void MainWindow_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            this.Close();
        }
        if (e.Key == Key.Enter)
        {
            AuthRegOperation();
        }
    }
    private void HyperLabel_MouseDown(object sender, MouseButtonEventArgs e)
    {
        ChangeMenu();
    }
    private void HyperLabel_MouseEnter(object sender, MouseEventArgs e)
    {
        HyperLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF55A94B"));
    }

    private void HyperLabel_MouseLeave(object sender, MouseEventArgs e)
    {
        HyperLabel.Foreground = Brushes.White;
    }
    private void Button_MouseEnter(object sender, MouseEventArgs e)
    {
        Button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#419937"));
        //rgb = 65; 153; 55

    }
    private void Button_MouseLeave(object sender, MouseEventArgs e)
    {
        Button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF55A94B"));
        //rgb = 85; 169; 75
    }
    private void Exit_MouseDown(object sender, MouseButtonEventArgs e)
    {
        this.Close();
    }
    private void Button_Click(object sender, RoutedEventArgs e)
    {
        AuthRegOperation();
        //this.Hide();
        //MenuWindow menuWindow = new MenuWindow(new User("December",""));
        //menuWindow.Show();
    }
}