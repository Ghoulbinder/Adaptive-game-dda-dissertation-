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

        /// <summary>
        /// PSEUDOCODE:
        /// 1. If a debug log file exists, deserialize its contents into a list.
        /// 2. If an error occurs or the file doesn't exist, start with an empty list.
        /// 3. Append the new entry to the list.
        /// 4. Serialize the updated list back to the same XML file.
        /// </summary>
        public static void AppendLogEntry(DebugLogEntry entry)
        {
            List<DebugLogEntry> entries = new List<DebugLogEntry>();

            // PSEUDOCODE: Try to read existing log entries from disk
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
                    // PSEUDOCODE: On failure, reset to an empty list
                    entries = new List<DebugLogEntry>();
                }
            }

            // PSEUDOCODE: Add the new log entry to the list
            entries.Add(entry);

            // PSEUDOCODE: Write the updated list back to disk
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

        /// <summary>
        /// PSEUDOCODE:
        /// Return the full path where the debug log is stored.
        /// </summary>
        public static string GetLogPath()
        {
            // PSEUDOCODE: Return the configured log file path
            return logPath;
        }

        /// <summary>
        /// PSEUDOCODE:
        /// If the debug log file exists, delete it to start fresh.
        /// </summary>
        public static void ClearLog()
        {
            // PSEUDOCODE: Remove the log file if it exists
            if (File.Exists(logPath))
            {
                File.Delete(logPath);
            }
        }
    }
}
