using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types.InputFiles;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            TelegramService.GetInstance();
            Console.ReadLine();
        }

        public class TelegramService
        {

            private static readonly TelegramBotClient Bot =
                new TelegramBotClient("525602288:AAFdNiHmmZOB6wK13tQaRvTBQpf7YD6glPc");

            private static TelegramService telegram;
            private const string URL = "http://rating.chgk.info/api/";

            #region Private Methods

            private TelegramService()
            {
                Bot.OnMessage += On_Message;
                Bot.StartReceiving();
            }

            private static async void GetPlayerRating(int id, long chatId)
            {
                var urlParams = $"players/{id}/rating/last";
                var deserializedJson = SendApiRequest(urlParams);
                if (deserializedJson.ToString() != string.Empty)
                {
                    var hoursPlayed = deserializedJson.tournament_count_total * 3;
                    await Bot.SendTextMessageAsync(chatId,
                        $"Зырь, вот твой рейтинг - {deserializedJson.rating}\n" +
                        $"А вот это твое место - {deserializedJson.rating_position}\n" +
                        $"Вот столько игр ты сыграл в этом году - {deserializedJson.tournaments_in_year}\n" +
                        $"А вот столько за всю жизнь - {deserializedJson.tournament_count_total}\n" +
                        $"Вот столько часов жизни ты вложил в ЧГК - {hoursPlayed}\n");
                    if (hoursPlayed > 24)
                    {
                        await Bot.SendTextMessageAsync(chatId,
                            $"А вот столько дней ты жалел, что не прочел Шекспира - {hoursPlayed / 24}");
                    }
                }
                else
                    await Bot.SendTextMessageAsync(chatId, $"Игрок с таким Id не найден");
            }

            private static async void GetPlayerInfo(int id, long chatId)
            {
                var urlParams = $"players/{id}";
                var deserializedJson = SendApiRequest(urlParams);
                if (deserializedJson.ToString() != string.Empty)
                {
                    await Bot.SendTextMessageAsync(chatId,
                        $"Id - {deserializedJson[0].idplayer}\n" +
                        $"Фамилия - {deserializedJson[0].surname}\n" +
                        $"Имя - {deserializedJson[0].name}\n" +
                        $"Отчество - {deserializedJson[0].patronymic}\n" +
                        $"Комментарий - {deserializedJson[0].comment}\n");
                }
                else
                    await Bot.SendTextMessageAsync(chatId, $"Игрок с таким Id не найден");
            }

            private static async void GetTeamInfo(int id, long chatId)
            {
                var urlParams = $"teams/{id}";
                var deserializedJson = SendApiRequest(urlParams);
                if (deserializedJson.ToString() != string.Empty)
                {
                    await Bot.SendTextMessageAsync(chatId,
                        $"Id - {deserializedJson[0].idteam}\n" +
                        $"Название - {deserializedJson[0].name}\n" +
                        $"Город - {deserializedJson[0].town}\n" +
                        $"Комментарий - {deserializedJson[0].comment}\n");
                }
                else
                    await Bot.SendTextMessageAsync(chatId, $"Команда с таким Id не найдена");
            }

            private static async void GetLastTeamRating(int id, long chatId)
            {
                var urlParams = $"teams/{id}/rating/1334";
                var deserializedJson = SendApiRequest(urlParams);
                if (deserializedJson.ToString() != string.Empty)
                {
                    await Bot.SendTextMessageAsync(chatId,
                        $"Id команды - {deserializedJson.idteam}\n" +
                        $"Id релиза - {deserializedJson.idrelease}\n" +
                        $"Рейтинг - {deserializedJson.rating}\n" +
                        $"Место - {deserializedJson.rating_position}\n" +
                        $"Дата - {deserializedJson.date}\n" +
                        $"Формула - {deserializedJson.formula}\n");
                }
                else
                    await Bot.SendTextMessageAsync(chatId, $"Команда с таким Id не найдена");
            }

            private static dynamic SendApiRequest(string urlParams)
            {
                HttpClient client = new HttpClient {BaseAddress = new Uri(URL)};

                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync(urlParams).Result;
                if (response.IsSuccessStatusCode)
                {
                    var dataObjects = response.Content.ReadAsStringAsync().Result;
                    dynamic deserializedJson = JsonConvert.DeserializeObject(dataObjects);
                    return deserializedJson;
                }
                else
                {
                    Console.WriteLine("{0} ({1})", (int) response.StatusCode, response.ReasonPhrase);
                }

                return response.StatusCode;

            }

            #endregion


            #region Public Methods

            public static TelegramService GetInstance()
            {
                if (telegram == null)
                {
                    lock (typeof(TelegramService))
                    {
                        if (telegram == null)
                        {
                            telegram = new TelegramService();
                        }
                    }
                }

                return telegram;
            }

            public void SendMessage(string message, string recipients)
            {
                foreach (var recipient in recipients)
                {
                    try
                    {
                        var t = Bot.SendTextMessageAsync(recipient, message);
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                }
            }

            private static async void On_Message(object sender, Telegram.Bot.Args.MessageEventArgs e)
            {
                Console.WriteLine(e.Message.Chat.Username);
                Console.WriteLine(e.Message.Text);
                Console.WriteLine(e.Message.Chat.Id);

                if (e.Message.Text.Equals("/start"))
                {
                    var FileUrl = @"https://sun1-9.userapi.com/c830609/v830609506/b5295/ZX6_23cOaHs.jpg";
                    await Bot.SendPhotoAsync(e.Message.Chat.Id, FileUrl);
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id, "/help - доступные команды");
                }

                if (e.Message.Text.Equals("/help"))
                {
                    await Bot.SendTextMessageAsync(e.Message.Chat.Id,

                        $"ID Комочков - 41047\n" +
                        $"ID игрока: \n" +
                        $"40942 Бароньянц Александр Сергеевич \n" +
                        $"76258 Черных Михаил Владимирович \n" +
                        $"40991 Дмитриев Владислав Аркадьевич \n" +
                        $"60779 Мамедова Полина Руслановна \n" +
                        $"40944 Мышко Анастасия Сергеевна \n" +
                        $"137818 Горошко Аксинья Игоревна \n" +
                        $"Команды: \n" +
                        $"player id_игрока - информация об игроке\n" +
                        $"prating id_игрока - рейтинг игрока\n" +
                        $"team id_команды - информация о команде\n" +
                        $"trating id_команды - рейтинг команды на данный момент\n");
                }

                var str = e.Message.Text.Split(' ');
                if (str.Length == 2)
                {
                    if (str[0].Equals("prating") && IsDigitsOnly(str[1]))
                    {
                        GetPlayerRating(Convert.ToInt32(str[1]), e.Message.Chat.Id);
                    }

                    if (str[0].Equals("player") && IsDigitsOnly(str[1]))
                    {
                        GetPlayerInfo(Convert.ToInt32(str[1]), e.Message.Chat.Id);
                    }

                    if (str[0].Equals("team") && IsDigitsOnly(str[1]))
                    {
                        GetTeamInfo(Convert.ToInt32(str[1]), e.Message.Chat.Id);
                    }

                    if (str[0].Equals("trating") && IsDigitsOnly(str[1]))
                    {
                        GetLastTeamRating(Convert.ToInt32(str[1]), e.Message.Chat.Id);
                    }
                }
            }
        }

        private static bool IsDigitsOnly(string str)
        {
            foreach (var c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        #endregion
    }
}

