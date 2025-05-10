using Liars_deck.classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Liars_deck
{
    /// <summary>
    /// Логика взаимодействия для MenuWindow.xaml
    /// </summary>
    public partial class MenuWindow : Window
    {
        public MenuWindow(User user)
        {
            InitializeComponent();
            this.Username.Content = user.login;
            this.RatingLabel.Content = user.rating;
        }
        private void Exit_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            Server room = new Server();
            room.Start(8000);
        }
    }
}
