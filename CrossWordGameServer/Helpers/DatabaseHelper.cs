using CrossWordGameServer.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Web;

namespace CrossWordGameServer.Helpers
{
    public class DatabaseHelper
    {
        private static string dbPath = "";

        private static string sqlDbName = @"\GameLevels.sqlite";
        private static string fileDbName = @"\StateFile.txt";

        private static object fileLockObj = new object();

        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


        private static SQLiteConnection dbConnection;

        private static string mapPath(string relativePath)
        {
            return System.Web.HttpContext.Current.Server.MapPath(relativePath);
        }

        public static void SetupDatabase(string dbPath)
        {
            DatabaseHelper.dbPath = dbPath;

            if (!Directory.Exists(mapPath(dbPath)))
            {
                Directory.CreateDirectory(mapPath(dbPath));
            }

            dbConnection = new SQLiteConnection(@"Data Source=" + System.Web.HttpContext.Current.Server.MapPath(dbPath + sqlDbName) + ";Version=3;");
            dbConnection.Open();

            if (!File.Exists(mapPath(dbPath + fileDbName)))
            {
                File.WriteAllLines(mapPath(dbPath + fileDbName), new string[] { "finished", "0", "0" });
            }

            new SQLiteCommand("create table if not exists GameLevels (id integer primary key autoincrement, number integer" +
                ", prize integer, table_data text, question_data text, answer_data text);", dbConnection).ExecuteNonQuery();

            new SQLiteCommand("create table if not exists TourPlayers (id integer primary key autoincrement, passkey text" +
                ", name text, levels_done text, score integer);", dbConnection).ExecuteNonQuery();

            new SQLiteCommand("create table if not exists Messages (id integer primary key autoincrement, content text, " +
                "time bigint);", dbConnection).ExecuteNonQuery();

            new SQLiteCommand("create table if not exists Words (id integer primary key autoincrement, word text, meaning " +
                "text);", dbConnection).ExecuteNonQuery();
        }

        public static void AddGameLevel(int number, int prize, string tableData, string questionData, string answerData)
        {
            SQLiteCommand command0 = new SQLiteCommand("insert into GameLevels (number, prize, table_data, question_data, answer_data) values (@param1, @param2, @param3, @param4, @param5);", dbConnection);
            command0.CommandType = System.Data.CommandType.Text;
            command0.Parameters.AddWithValue("@param1", number);
            command0.Parameters.AddWithValue("@param2", prize);
            command0.Parameters.AddWithValue("@param3", tableData);
            command0.Parameters.AddWithValue("@param4", questionData);
            command0.Parameters.AddWithValue("@param5", answerData);
            command0.ExecuteNonQuery();
        }

        public static void EditGameLevel(int id, int number, int prize, string tableData, string questionData, string answerData)
        {
            SQLiteCommand command0 = new SQLiteCommand("update GameLevels set number = @param1, prize = @param2, table_data = @param3, question_data = @param4, answer_data = @param5 where Id = @param6", dbConnection);
            command0.CommandType = System.Data.CommandType.Text;
            command0.Parameters.AddWithValue("@param1", number);
            command0.Parameters.AddWithValue("@param2", prize);
            command0.Parameters.AddWithValue("@param3", tableData);
            command0.Parameters.AddWithValue("@param4", questionData);
            command0.Parameters.AddWithValue("@param5", answerData);
            command0.Parameters.AddWithValue("@param6", id);
            command0.ExecuteNonQuery();
        }

        public static List<GameLevel> GetGameLevels()
        {
            List<GameLevel> result = new List<GameLevel>();

            SQLiteCommand command0 = new SQLiteCommand("select * from GameLevels", dbConnection);
            SQLiteDataReader reader0 = command0.ExecuteReader();

            while (reader0.Read())
            {
                result.Add(new GameLevel()
                {
                    id = int.Parse(reader0["id"].ToString()),
                    number = int.Parse(reader0["number"].ToString()),
                    prize = int.Parse(reader0["prize"].ToString()),
                    tableData = reader0["table_data"].ToString(),
                    questionData = reader0["question_data"].ToString(),
                    answerData = reader0["answer_data"].ToString()
                });
            }

            reader0.Close();

            return result;
        }

        public static List<int> GetGameLevelIds()
        {
            List<int> result = new List<int>();

            SQLiteCommand command0 = new SQLiteCommand("select * from GameLevels", dbConnection);
            SQLiteDataReader reader0 = command0.ExecuteReader();

            while (reader0.Read())
            {
                result.Add(int.Parse(reader0["id"].ToString()));
            }

            reader0.Close();

            return result;
        }

        public static int GetGameLevelsCount()
        {
            SQLiteCommand command0 = new SQLiteCommand("select count(id) from GameLevels", dbConnection);
            command0.CommandType = System.Data.CommandType.Text;
            return Convert.ToInt32(command0.ExecuteScalar());
        }

        public static void DeleteGameLevel(int id)
        {
            SQLiteCommand command0 = new SQLiteCommand("delete from GameLevels where Id = @param1", dbConnection);
            command0.Parameters.AddWithValue("@param1", id);
            command0.ExecuteNonQuery();
        }

        public static void StartTournament(int totalDays)
        {
            lock (fileLockObj)
            {
                string[] tourData = File.ReadAllLines(mapPath(dbPath + fileDbName));

                if (tourData[0] == "finished")
                {
                    tourData[0] = "started";
                    tourData[1] = totalDays.ToString();
                    tourData[2] = (DateTime.Now - DateTime.MinValue).TotalMilliseconds.ToString();

                    File.WriteAllLines(mapPath(dbPath + fileDbName), tourData);
                }
            }
        }

        public static Tournament GetTournamentData()
        {
            string[] tourData = File.ReadAllLines(mapPath(dbPath + fileDbName));
            
            int totalDays = Convert.ToInt32(tourData[1]);
            double startMillis = Convert.ToDouble(tourData[2]);

            double currentMillis = (DateTime.Now - DateTime.MinValue).TotalMilliseconds;
            int leftDays = totalDays - Convert.ToInt32((currentMillis - startMillis) / 86400000);

            if (leftDays <= 0)
            {
                tourData[0] = "finished";
                File.WriteAllLines(mapPath(dbPath + fileDbName), tourData);
            }

            bool isActive = (tourData[0] == "started");
            
            SQLiteCommand command0 = new SQLiteCommand("select count(id) from TourPlayers", dbConnection);
            int playersCount = Convert.ToInt32(command0.ExecuteScalar());

            return new Tournament()
            {
                active = isActive,
                totalDays = totalDays,
                leftDays = leftDays,
                playersCount = playersCount
            };
        }

        public static void EndTournament()
        {
            lock (fileLockObj)
            {
                string gameState = File.ReadAllText(mapPath(dbPath + fileDbName));

                if (gameState == "started")
                {
                    gameState = "finished";

                    File.WriteAllText(mapPath(dbPath + fileDbName), gameState);
                }
            }
        }

        public static void AddTourPlayer(string passkey, string name)
        {
            SQLiteCommand command0 = new SQLiteCommand("insert into TourPlayers (passkey, name, levels_done, levels_done_count) values (@param1, @param2, @param3, @param4);", dbConnection);
            command0.Parameters.AddWithValue("@param1", passkey);
            command0.Parameters.AddWithValue("@param2", name);
            command0.Parameters.AddWithValue("@param3", "");
            command0.Parameters.AddWithValue("@param4", 0);
            command0.ExecuteNonQuery();
        }

        public static void DeleteTourPlayer(int id)
        {
            SQLiteCommand command0 = new SQLiteCommand("delete from TourPlayers where id = @param1", dbConnection);
            command0.Parameters.AddWithValue("@param1", id);
            command0.ExecuteNonQuery();
        }

        public static List<TourPlayer> GetTourPlayers()
        {
            List<TourPlayer> result = new List<TourPlayer>();

            SQLiteCommand command0 = new SQLiteCommand("select * from TourPlayers order by levels_done_count desc", dbConnection);
            SQLiteDataReader reader0 = command0.ExecuteReader();

            while (reader0.Read())
            {
                TourPlayer tourPlayer = new TourPlayer()
                {
                    id = int.Parse(reader0["id"].ToString()),
                    passkey = (string)reader0["passkey"],
                    name = (string)reader0["name"],
                    levelsDone = (string)reader0["levels_done"],
                    score = int.Parse(reader0["levels_done_count"].ToString())
                };

                result.Add(tourPlayer);
            }

            reader0.Close();

            return result;
        }

        public static List<TourPlayer> GetTopTourPlayers()
        {
            List<TourPlayer> result = new List<TourPlayer>();

            SQLiteCommand command0 = new SQLiteCommand("select * from TourPlayers order by levels_done_count desc limit 20", dbConnection);
            SQLiteDataReader reader0 = command0.ExecuteReader();

            while (reader0.Read())
            {
                TourPlayer tourPlayer = new TourPlayer()
                {
                    id = int.Parse(reader0["id"].ToString()),
                    passkey = (string)reader0["passkey"],
                    name = (string)reader0["name"],
                    levelsDone = (string)reader0["levels_done"],
                    score = int.Parse(reader0["levels_done_count"].ToString())
                };

                result.Add(tourPlayer);
            }

            reader0.Close();

            return result;
        }

        public static TourPlayer GetTourPlayerById(int id)
        {
            SQLiteCommand command0 = new SQLiteCommand("select * from TourPlayers where id = @param1", dbConnection);
            command0.Parameters.AddWithValue("@param1", id);
            SQLiteDataReader reader0 = command0.ExecuteReader();

            if (reader0.Read())
            {
                TourPlayer tourPlayer = new TourPlayer()
                {
                    id = (int)reader0["id"],
                    passkey = (string)reader0["passkey"],
                    name = (string)reader0["name"],
                    levelsDone = (string)reader0["levels_done"],
                    score = (int)reader0["levels_done_count"]
                };

                return tourPlayer;
            }
            else
            {
                return null;
            }
        }

        public static void AddMessage(string content)
        {
            SQLiteCommand command0 = new SQLiteCommand("insert into Messages (content, time) values (@param1, @param2);", dbConnection);
            command0.Parameters.AddWithValue("@param1", content);
            command0.Parameters.AddWithValue("@param2", Convert.ToInt64((DateTime.UtcNow - Jan1st1970).TotalMilliseconds));
            command0.ExecuteNonQuery();
        }

        public static void DeleteMessage(int id)
        {
            SQLiteCommand command0 = new SQLiteCommand("delete from Messages where id = @param1;", dbConnection);
            command0.Parameters.AddWithValue("@param1", id);
            command0.ExecuteNonQuery();
        }

        public static List<Message> GetMessages()
        {
            List<Message> result = new List<Message>();

            SQLiteCommand command0 = new SQLiteCommand("select * from Messages", dbConnection);
            SQLiteDataReader reader0 = command0.ExecuteReader();

            while (reader0.Read())
            {
                result.Add(new Message()
                {
                    id = int.Parse(reader0["id"].ToString()),
                    content = reader0["content"].ToString(),
                    time = long.Parse(reader0["time"].ToString())
                });
            }

            reader0.Close();

            return result;
        }

        public static List<int> GetMessageIds()
        {
            List<int> result = new List<int>();

            SQLiteCommand command0 = new SQLiteCommand("select * from Messages", dbConnection);
            SQLiteDataReader reader0 = command0.ExecuteReader();

            while (reader0.Read())
            {
                result.Add(int.Parse(reader0["id"].ToString()));
            }

            reader0.Close();

            return result;
        }

        public static int GetMessagesCount()
        {
            SQLiteCommand command0 = new SQLiteCommand("select count(id) from Messages", dbConnection);
            command0.CommandType = System.Data.CommandType.Text;
            return Convert.ToInt32(command0.ExecuteScalar());
        }

        public static List<Word> GetWords()
        {
            List<Word> result = new List<Word>();

            SQLiteCommand command0 = new SQLiteCommand("select * from Words", dbConnection);
            SQLiteDataReader reader0 = command0.ExecuteReader();

            while (reader0.Read())
            {
                result.Add(new Word()
                {
                    id = int.Parse(reader0["id"].ToString()),
                    word = reader0["word"].ToString(),
                    meaning = reader0["meaning"].ToString()
                });
            }

            reader0.Close();

            return result;
        }

        public static void AddWord(string word, string meaning)
        {
            SQLiteCommand command0 = new SQLiteCommand("insert into Words (word, meaning) values (@param1, @param2);", dbConnection);
            command0.Parameters.AddWithValue("@param1", word);
            command0.Parameters.AddWithValue("@param2", meaning);
            command0.ExecuteNonQuery();
        }

        public static void DeleteWord(int id)
        {
            SQLiteCommand command0 = new SQLiteCommand("delete from Words where id = @param1;", dbConnection);
            command0.Parameters.AddWithValue("@param1", id);
            command0.ExecuteNonQuery();
        }

        public static List<int> GetWordIds()
        {
            List<int> result = new List<int>();

            SQLiteCommand command0 = new SQLiteCommand("select * from Words", dbConnection);
            SQLiteDataReader reader0 = command0.ExecuteReader();

            while (reader0.Read())
            {
                result.Add(int.Parse(reader0["id"].ToString()));
            }

            reader0.Close();

            return result;
        }

        public static int GetWordsCount()
        {
            SQLiteCommand command0 = new SQLiteCommand("select count(id) from Words", dbConnection);
            command0.CommandType = System.Data.CommandType.Text;
            return Convert.ToInt32(command0.ExecuteScalar());
        }

        public static void ShutdownDatabase()
        {
            dbConnection.Close();
        }
    }
}