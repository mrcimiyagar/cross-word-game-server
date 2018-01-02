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

        private static SortedList<int, Dictionary<long, TourPlayer>> highscores;

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

            new SQLiteCommand("create table if not exists GameLevels (id integer primary key autoincrement, number integer" +
                ", prize integer, table_data text, question_data text, answer_data text);", dbConnection).ExecuteNonQuery();

            new SQLiteCommand("create table if not exists TourPlayers (passkey text" +
                ", name text, score integer);", dbConnection).ExecuteNonQuery();

            new SQLiteCommand("create table if not exists Messages (id integer primary key autoincrement, content text, " +
                "time bigint);", dbConnection).ExecuteNonQuery();

            new SQLiteCommand("create table if not exists Words (id integer primary key autoincrement, word text, meaning " +
                "text);", dbConnection).ExecuteNonQuery();

            new SQLiteCommand("create table if not exists Tours (total_days integer, start_millis bigint);", dbConnection)
                .ExecuteNonQuery();

            highscores = new SortedList<int, Dictionary<long, TourPlayer>>(new DescendedDateComparer());

            SQLiteDataReader reader0 = new SQLiteCommand("select rowid, * from TourPlayers", dbConnection).ExecuteReader();

            while (reader0.Read())
            {
                TourPlayer tourPlayer = new TourPlayer()
                {
                    id = Convert.ToInt64(reader0["rowid"].ToString()),
                    name = reader0["name"].ToString(),
                    passkey = reader0["passkey"].ToString(),
                    score = Convert.ToInt32(reader0["score"].ToString())
                };

                if (highscores.ContainsKey(tourPlayer.score)) {
                    highscores[tourPlayer.score].Add(tourPlayer.id, tourPlayer);
                }
                else
                {
                    Dictionary<long, TourPlayer> highscoreChunk = new Dictionary<long, TourPlayer>();
                    highscoreChunk.Add(tourPlayer.id, tourPlayer);
                    highscores.Add(tourPlayer.score, highscoreChunk);
                }
            }
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
            long currentMillis = Convert.ToInt64((DateTime.UtcNow - Jan1st1970).TotalMilliseconds);

            SQLiteCommand command1 = new SQLiteCommand("delete from TourPlayers", dbConnection);
            command1.ExecuteNonQuery();

            highscores.Clear();

            SQLiteCommand command0 = new SQLiteCommand("insert into Tours (total_days, start_millis) values (@param1, @param2);", dbConnection);
            command0.Parameters.AddWithValue("@param1", totalDays);
            command0.Parameters.AddWithValue("@param2", currentMillis);
            command0.ExecuteNonQuery();
        }

        public static Tournament GetTournamentData()
        {
            Tournament tournament = null;

            SQLiteCommand command0 = new SQLiteCommand("select rowid, * from Tours order by rowid desc limit 1", dbConnection);
            SQLiteDataReader reader0 = command0.ExecuteReader();

            if (reader0.Read())
            {
                long tourId = Convert.ToInt64(reader0["rowid"]);

                long currentMillis = Convert.ToInt64((DateTime.UtcNow - Jan1st1970).TotalMilliseconds);
                
                int totalDays = Convert.ToInt32(reader0["total_days"].ToString());
                long startMillis = Convert.ToInt64(reader0["start_millis"].ToString());
                int leftDays = totalDays - Convert.ToInt32((currentMillis - startMillis) / 86400000);
                
                if (leftDays > 0)
                {
                    SQLiteCommand command1 = new SQLiteCommand("select count(rowid) from TourPlayers", dbConnection);
                    int playersCount = Convert.ToInt32(command1.ExecuteScalar());

                    tournament = new Tournament()
                    {
                        id = tourId,
                        active = true,
                        playersCount = playersCount,
                        startMillis = startMillis,
                        totalDays = totalDays,
                        leftDays = leftDays
                    };
                }
                else
                {
                    tournament = new Tournament()
                    {
                        id = 0,
                        active = false,
                        playersCount = 0,
                        startMillis = 0,
                        totalDays = 0,
                        leftDays = 0
                    };
                }
            }
            else
            {
                tournament = new Tournament()
                {
                    active = false,
                    playersCount = 0,
                    totalDays = 0,
                    leftDays = 0
                };
            }

            reader0.Close();

            return tournament;
        }

        public static void EndTournament()
        {
            
        }

        public static long AddTourPlayer(string passkey, string name)
        {
            SQLiteCommand command0 = new SQLiteCommand("insert into TourPlayers (passkey, name, score) values (@param1, @param2, @param3); select last_insert_rowid();", dbConnection);
            command0.Parameters.AddWithValue("@param1", passkey);
            command0.Parameters.AddWithValue("@param2", name);
            command0.Parameters.AddWithValue("@param3", 0);
            long rowId = Convert.ToInt64(command0.ExecuteScalar());

            TourPlayer tourPlayer = new TourPlayer();
            tourPlayer.id = rowId;
            tourPlayer.name = name;
            tourPlayer.score = 0;

            lock (highscores)
            {
                if (highscores.ContainsKey(tourPlayer.score))
                {
                    highscores[tourPlayer.score].Add(tourPlayer.id, tourPlayer);
                }
                else
                {
                    Dictionary<long, TourPlayer> highscoreChunk = new Dictionary<long, TourPlayer>();
                    highscoreChunk.Add(tourPlayer.id, tourPlayer);
                    highscores.Add(tourPlayer.score, highscoreChunk);
                }
            }

            return rowId;
        }

        public static void DeleteTourPlayer(long id, string passkey)
        {
            SQLiteCommand command0 = new SQLiteCommand("delete from TourPlayers where rowid = @param1 and passkey = @param2", dbConnection);
            command0.Parameters.AddWithValue("@param1", id);
            command0.Parameters.AddWithValue("@param2", passkey);
            command0.ExecuteNonQuery();
        }

        public static List<TourPlayer> GetTourPlayers()
        {
            List<TourPlayer> result = new List<TourPlayer>();

            SQLiteCommand command0 = new SQLiteCommand("select rowid, * from TourPlayers order by levels_done_count desc", dbConnection);
            SQLiteDataReader reader0 = command0.ExecuteReader();

            while (reader0.Read())
            {
                TourPlayer tourPlayer = new TourPlayer()
                {
                    id = long.Parse(reader0["rowid"].ToString()),
                    passkey = (string)reader0["passkey"],
                    name = (string)reader0["name"],
                    score = int.Parse(reader0["score"].ToString())
                };

                result.Add(tourPlayer);
            }

            reader0.Close();

            return result;
        }

        public static List<TourPlayer> GetTopTourPlayers()
        {
            List<TourPlayer> result = new List<TourPlayer>();

            int playerCounter = 0;

            foreach (KeyValuePair<int, Dictionary<long, TourPlayer>> highscoreChunk in highscores)
            {
                if (playerCounter >= 20)
                {
                    break;
                }

                foreach (KeyValuePair<long, TourPlayer> tourPlayerPart in highscoreChunk.Value)
                {
                    if (playerCounter >= 20)
                    {
                        break;
                    }

                    result.Add(tourPlayerPart.Value);

                    playerCounter++;
                }
            }

            /*SQLiteCommand command0 = new SQLiteCommand("select rowid, * from TourPlayers order by score desc limit 20", dbConnection);
            SQLiteDataReader reader0 = command0.ExecuteReader();

            while (reader0.Read())
            {
                TourPlayer tourPlayer = new TourPlayer()
                {
                    id = long.Parse(reader0["rowid"].ToString()),
                    passkey = (string)reader0["passkey"],
                    name = (string)reader0["name"],
                    score = int.Parse(reader0["score"].ToString())
                };

                result.Add(tourPlayer);
            }

            reader0.Close();*/

            return result;
        }

        public static TourPlayer GetTourPlayerById(long id)
        {
            SQLiteCommand command0 = new SQLiteCommand("select rowid, * from TourPlayers where rowid = @param1", dbConnection);
            command0.Parameters.AddWithValue("@param1", id);
            SQLiteDataReader reader0 = command0.ExecuteReader();

            if (reader0.Read())
            {
                TourPlayer tourPlayer = new TourPlayer()
                {
                    id = long.Parse(reader0["rowid"].ToString()),
                    passkey = reader0["passkey"].ToString(),
                    name = reader0["name"].ToString(),
                    score = int.Parse(reader0["score"].ToString())
                };

                reader0.Close();

                int rank = 1;

                lock (highscores)
                {
                    foreach (KeyValuePair<int, Dictionary<long, TourPlayer>> chunk in highscores)
                    {
                        if (chunk.Key > tourPlayer.score)
                        {
                            rank += chunk.Value.Count;
                        }
                        else if (chunk.Key == tourPlayer.score)
                        {
                            foreach (int playerId in chunk.Value.Keys)
                            {
                                if (playerId == tourPlayer.id)
                                {
                                    break;
                                }

                                rank++;
                            }
                        }
                    }
                }

                tourPlayer.rank = rank;

                return tourPlayer;
            }
            else
            {
                reader0.Close();

                return null;
            }
        }

        public static void UpdateTourPlayerById(long id, string passkey, string name, int score)
        {
            lock (dbConnection)
            {
                SQLiteCommand command0 = new SQLiteCommand("select * from TourPlayers where rowid = @param1 and passkey = @param2", dbConnection);
                command0.Parameters.AddWithValue("@param1", id);
                command0.Parameters.AddWithValue("@param2", passkey);
                SQLiteDataReader reader0 = command0.ExecuteReader();

                if (reader0.Read())
                {
                    int oldScore = Convert.ToInt32(reader0["score"].ToString());

                    SQLiteCommand command1 = new SQLiteCommand("update TourPlayers set score = @param1, name = @param2 where rowid = @param3 and passkey = @param4;", dbConnection);
                    command1.Parameters.AddWithValue("@param1", score);
                    command1.Parameters.AddWithValue("@param2", name);
                    command1.Parameters.AddWithValue("@param3", id);
                    command1.Parameters.AddWithValue("@param4", passkey);
                    command1.ExecuteNonQuery();
                    
                    lock (highscores)
                    {
                        TourPlayer tourPlayer;

                        if (highscores.ContainsKey(oldScore))
                        {
                            tourPlayer = highscores[oldScore][id];

                            highscores[oldScore].Remove(id);
                        }
                        else
                        {
                            tourPlayer = new TourPlayer();
                            tourPlayer.id = id;
                        }

                        tourPlayer.name = name;
                        tourPlayer.score = score;

                        if (highscores.ContainsKey(score))
                        {
                            highscores[score].Add(id, tourPlayer);
                        }
                        else
                        {
                            Dictionary<long, TourPlayer> highscoreChunk = new Dictionary<long, TourPlayer>();

                            highscoreChunk.Add(id, tourPlayer);
                            highscores.Add(score, highscoreChunk);
                        }
                    }
                }

                reader0.Close();
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

        class DescendedDateComparer : IComparer<int>
        {
            public int Compare(int x, int y)
            {
                return y.CompareTo(x);
            }
        }

    }
}