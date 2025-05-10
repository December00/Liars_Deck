using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Liars_deck.classes
{
    public class User
    {
        public string login;
        private readonly string? password;
        public int? rating;
        public User(string login, string pas)
        {
            this.login = login;
            this.password = pas;
            this.rating = 1000;
        }
        public bool Authorization()
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Логин и пароль должны быть заполнены");
                return false;
            }

            DB db = new DB();
            DataTable table = new DataTable();

            try
            {
                using (MySqlCommand command = new MySqlCommand(
                    "SELECT `login`, `rating` FROM `users` WHERE `login` = @uL AND `pas` = @uP",
                    db.getConnection()))
                {
                    command.Parameters.Add("@uL", MySqlDbType.VarChar).Value = login;
                    command.Parameters.Add("@uP", MySqlDbType.VarChar).Value = password;

                    db.openConnection();

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            this.rating = reader.GetInt32("rating");
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Неверный логин или пароль");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка авторизации: {ex.Message}");
                return false;
            }
            finally
            {
                db.closeConnection();
            }
        }


        private bool IsEnterValid()
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Поля должны быть заполнены");
                return false;
            }
            if (login.Contains(' ') || password.Contains(' '))
            {
                MessageBox.Show("Поля не должны содержать пробел");
                return false;
            }
            if (password.Length < 8 || password.Length >= 32)
            {
                MessageBox.Show("Пароль должен иметь хотя бы 8 символов и не более 32");
                return false;
            }
            if (login.Length >= 16)
            {
                MessageBox.Show("Логин не должен содержать более 16 символов");
                return false;
            }
            DB db = new DB();
            DataTable table = new DataTable();
            MySqlDataAdapter adapter = new MySqlDataAdapter();

            MySqlCommand command = new MySqlCommand("SELECT * FROM `users` WHERE `login` = @uL", db.getConnection());
            command.Parameters.Add("@uL", MySqlDbType.VarChar).Value = login;

            adapter.SelectCommand = command;
            adapter.Fill(table);

            if (table.Rows.Count > 0)
            {
                MessageBox.Show("Пользователь с таким логином уже существует");
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool Registration()
        {
            DB db = new DB();
            MySqlCommand command = new MySqlCommand("INSERT INTO `users` (`login`, `pas`, `rating`) VALUES (@login, @pas, @rating)", db.getConnection());
            command.Parameters.Add("@login", MySqlDbType.VarChar).Value = login;
            command.Parameters.Add("@pas", MySqlDbType.VarChar).Value = password;
            command.Parameters.Add("@rating", MySqlDbType.Int32).Value = 1000;

            db.openConnection();

            if (IsEnterValid())
            {
                if (command.ExecuteNonQuery() == 1)
                    return true;
                else
                   return false;
            }
            db.closeConnection();
            return false;
        }
    }
}
