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
    public MainWindow()
    {
        InitializeComponent();
    }
    private void HyperLabel_MouseDown(object sender, MouseButtonEventArgs e)
    {
        
        MessageBox.Show("Сделать переход к регистрации");

    }
    private void HyperLabel_MouseEnter(object sender, MouseEventArgs e)
    {
        HyperLabel.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF55A94B"));
    }

    private void HyperLabel_MouseLeave(object sender, MouseEventArgs e)
    {
        HyperLabel.Foreground = Brushes.White;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Сделать аутентификацию и регистрацию");
    }
}