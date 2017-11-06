using CrossWordGameServerProject.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Web;

namespace CrossWordGameServerProject.Helpers
{
    public class DatabaseHelper
    {
        private static string dbPath = "";

        private static string sqlDbName = @"\GameLevels.sqlite";
        private static string fileDbName = @"\StateFile.txt";

        private static object fileLockObj = new object();

        private static SQLiteConnection dbConnection;

        public static void SetupDatabase(string dbPath)
        {
            DatabaseHelper.dbPath = dbPath;

            if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath(dbPath)))
            {
                Directory.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath(dbPath));
            }

            dbConnection = new SQLiteConnection(@"Data Source=" + System.Web.HttpContext.Current.Server.MapPath(dbPath + sqlDbName) + ";Version=3;");
            dbConnection.Open();

            if (!File.Exists(System.Web.HttpContext.Current.Server.MapPath(dbPath + fileDbName)))
            {
                File.Create(System.Web.HttpContext.Current.Server.MapPath(dbPath + fileDbName)).Close();
            }

            new SQLiteCommand("create table if not exists GameLevels (id integer primary key autoincrement, number integer" +
                ", prize integer, table_data text, question_data text, answer_data text);", dbConnection).ExecuteNonQuery();

            new SQLiteCommand("create table if not exists TourPlayers (id integer primary key autoincrement, passkey text" +
                ", name text, levels_done text, levels_done_count integer);", dbConnection).ExecuteNonQuery();
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

        public static void DeleteGameLevel(int id)
        {
            SQLiteCommand command0 = new SQLiteCommand("delete from GameLevels where Id = @param1", dbConnection);
            command0.Parameters.AddWithValue("@param1", id);
            command0.ExecuteNonQuery();
        }

        public static void StartTournament()
        {
            lock (fileLockObj)
            {
                string gameState = File.ReadAllText(dbPath + fileDbName);

                if (gameState == "tournament = started")
                {
                    gameState = "tournament = finished";

                    File.WriteAllText(dbPath + fileDbName, gameState);
                }
            }
        }

        public static void EndTournament()
        {
            lock (fileLockObj)
            {
                string gameState = File.ReadAllText(dbPath + fileDbName);

                if (gameState == "tournament = finished")
                {
                    gameState = "tournament = started";

                    File.WriteAllText(dbPath + fileDbName, gameState);
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
                    levelsDoneCount = int.Parse(reader0["levels_done_count"].ToString())
                };

                result.Add(tourPlayer);
            }

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
                    levelsDoneCount = int.Parse(reader0["levels_done_count"].ToString())
                };

                result.Add(tourPlayer);
            }

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
                    levelsDoneCount = (int)reader0["levels_done_count"]
                };

                return tourPlayer;
            }
            else
            {
                return null;
            }
        }

        public static void ShutdownDatabase()
        {
            dbConnection.Close();
        }
    }
}