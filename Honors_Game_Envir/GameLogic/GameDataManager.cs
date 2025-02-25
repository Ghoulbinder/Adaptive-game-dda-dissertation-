using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Survivor_of_the_Bulge
{
    [Serializable]
    public class PlayerStatistics
    {
        public int TotalBulletsFired { get; set; }
        public int TotalHits { get; set; }
        public int TotalPlayerDeaths { get; set; }
    }

    [Serializable]
    public class SessionData
    {
        public DateTime SessionDate { get; set; }
        public int LevelReached { get; set; }
        public int LivesRemaining { get; set; }
        public int Score { get; set; }
    }

    [Serializable]
    public class ScoreboardEntry
    {
        public string PlayerName { get; set; }
        public int LevelReached { get; set; }  // Added LevelReached property
        public int BulletsFired { get; set; }
        public int BulletsUsedAgainstEnemies { get; set; }
        public int BulletsUsedAgainstBosses { get; set; }
        public int LivesLost { get; set; }
        public double TimeSpentSeconds { get; set; }
        public int FinalScore { get; set; }
    }

    [Serializable]
    public class GameData
    {
        public PlayerStatistics CumulativeStats { get; set; } = new PlayerStatistics();
        public List<SessionData> Sessions { get; set; } = new List<SessionData>();
        public List<ScoreboardEntry> Scoreboard { get; set; } = new List<ScoreboardEntry>();
    }

    public static class SaveLoadManager
    {
        private static string savePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SurvivorOfTheBulge_GameData.xml");

        public static void SaveGameData(GameData data)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(GameData));
                using (StreamWriter writer = new StreamWriter(savePath))
                {
                    serializer.Serialize(writer, data);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error saving game data: " + ex.Message);
            }
        }

        public static GameData LoadGameData()
        {
            try
            {
                if (File.Exists(savePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(GameData));
                    using (StreamReader reader = new StreamReader(savePath))
                    {
                        return (GameData)serializer.Deserialize(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading game data: " + ex.Message);
            }
            return new GameData();
        }
    }
}
