using System;
using Telegram.Bot;
using Telegram.Bot.Args;
using System.Data.SqlClient;





namespace BotTest
{
    class Program
    {
        private static string token { get; set; } = "2072510364:AAGqJsvmQDpPWL4sQoUlrC0_2dmiucFugXc";
        private static TelegramBotClient client;
        static void Main(string[] args)
        {




            //часть работы с телеграмом 
            client = new TelegramBotClient(token);
            client.StartReceiving();
            client.OnMessage += omh;
            Console.ReadLine();
            client.StartReceiving();


        }

        public static async void omh(object sender, MessageEventArgs e)
        {
            double bonusov = 0,
                   tovar = 0,
                   chekov = 0,
                   time = 0;
            string timeword;

            string connStr = "Data Source=DEL_E7440\\MSSQLSERVER01;Initial Catalog=TestDB;Integrated Security=True";


            var msg = e.Message;
            string numword = Convert.ToString(msg.Text);
            // проверка введенного текста
            if (numword.EndsWith("M"))
            {
                time = -30;
                numword = numword.Remove(numword.Length - 1);
                timeword = "месяц";

            }
            else if (numword.EndsWith("W"))
            {
                time = -7;
                numword = numword.Remove(numword.Length - 1);
                timeword = "неделю";
            }
            else
            {

                numword = "0";
                timeword = "?";
            }

            Console.WriteLine($"Сообщение: { numword}");

            string sqlexpression = sqlcd(time, "AddBonusesDate", "UserBonuses", "tblCheckBonusesUser", numword);
            string sqlexpression2 = sqlcd(time, "check_date", "check_count_sku", "tblTransactions", numword);

            SqlConnection connection = null;
            // подключение к бд
            connection = new SqlConnection(connStr);

            //перебор таблицы бонусов
            SqlCommand command = new SqlCommand(sqlexpression, connection);
            try
            {

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    bonusov += Convert.ToDouble(reader["UserBonuses"].ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Таблица бонусов:");
                Console.WriteLine(ex.Message);

            }
            finally
            {
                connection.Close();
            }
            //перебор таблицы транзакций 
            SqlCommand command2 = new SqlCommand(sqlexpression2, connection);
            try
            {
                connection.Open();
                SqlDataReader reader2 = command2.ExecuteReader();

                while (reader2.Read())
                {
                    tovar += Convert.ToDouble(reader2["check_count_sku"].ToString());
                    chekov += 1;
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("Таблица транзакций:");
                Console.WriteLine(ex.Message);

            }
            finally
            {

                connection.Close();
            }

            //вывод сообщения пользователю
            if (numword != "0")
            {
                var itog =
                     $"Клиент: " + numword + "\n" +
                     $"Отправлено чеков за {timeword}: " + Convert.ToString(chekov) + "\n" +
                     $"Куплено товаров за {timeword}: " + Convert.ToString(tovar) + "\n" +
                     $"Заработано за {timeword} БОНУСОВ: " + Convert.ToString(bonusov);

                await client.SendTextMessageAsync(msg.Chat.Id, itog);
            }
            else
            {
                await client.SendTextMessageAsync(msg.Chat.Id, "вы забыли символ, или ввели некорректный номер", replyToMessageId: msg.MessageId);
            }
            //функция вывода sql 
            string sqlcd(double wt, string ft, string st, string tabl, string phn)
            {

                string sqlp = $"declare @Lastday as datetime set @Lastday = (select max({ft}) " +
                              $"from {tabl} where UserPhoneNumber = {Convert.ToDouble(phn)}); " +
                              $"select {ft}, {st} from {tabl} " +
                              $"where UserPhoneNumber = {Convert.ToDouble(phn)} and {ft}> " +
                              $"DATEADD(day,{wt}, @Lastday) and {st}!=0";
                return sqlp;

            }
        }

    }
}
