using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;

namespace Survivor_of_the_Bulge
{
    [Serializable]
    public class DebugLogEntry
    {
        public string PlayerName { get; set; }
        public DifficultyLevel DifficultyLevel { get; set; }
        public int EnemiesKilled { get; set; }
        public int BossesKilled { get; set; }
        public double TimeTakenSeconds { get; set; }
        public int TotalDamageTaken { get; set; }
        public int BulletsFired { get; set; }
        public int BulletsUsedAgainstEnemies { get; set; }
        public int BulletsUsedAgainstBosses { get; set; }
        public int LivesLost { get; set; }
        public int Deaths { get; set; }
        // Log the current multipliers.
        public float EnemyHealthMultiplier { get; set; }
        public float EnemySpeedMultiplier { get; set; }
        public float EnemyDamageMultiplier { get; set; }
        public float BossHealthMultiplier { get; set; }
        public float BossAttackSpeedMultiplier { get; set; }
        public float BossMovementSpeedMultiplier { get; set; }
        public float BossDamageMultiplier { get; set; }
        public DateTime LogDate { get; set; }
    }

    public static class DebugLogger
    {
        private static string logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SurvivorOfTheBulge_DebugLog.xml");

        // Reads existing entries (if any), appends the new entry, and saves all.
        public static void AppendLogEntry(DebugLogEntry entry)
        {
            List<DebugLogEntry> entries = new List<DebugLogEntry>();
            if (File.Exists(logPath))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<DebugLogEntry>));
                    using (StreamReader reader = new StreamReader(logPath))
                    {
                        entries = (List<DebugLogEntry>)serializer.Deserialize(reader);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error reading existing debug log: " + ex.Message);
                    // In case of an error, start a new list.
                    entries = new List<DebugLogEntry>();
                }
            }
            entries.Add(entry);

            try
            {
                XmlSerializer serializer2 = new XmlSerializer(typeof(List<DebugLogEntry>));
                using (StreamWriter writer = new StreamWriter(logPath))
                {
                    serializer2.Serialize(writer, entries);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error saving debug log: " + ex.Message);
            }
        }

        public static string GetLogPath()
        {
            return logPath;
        }

        public static void ClearLog()
        {
            if (File.Exists(logPath))
            {
                File.Delete(logPath);
            }
        }
    }
}
