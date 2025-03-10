using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Survivor_of_the_Bulge
{
    // A singleton class that holds parameters used to adjust game difficulty dynamically.
    public class DifficultyManager
    {
        // Singleton instance.
        private static DifficultyManager _instance;
        public static DifficultyManager Instance => _instance ?? (_instance = new DifficultyManager());

        // Current difficulty level (e.g., 1 for easy, 2 for medium, etc.).
        public int Level { get; private set; } = 1;

        // Base parameters that are adjusted as difficulty increases.
        public int BaseEnemyCount { get; private set; } = 2;
        public float EnemyHealthMultiplier { get; private set; } = 1f;
        public float EnemySpeedMultiplier { get; private set; } = 1f;
        public float EnemyDamageMultiplier { get; private set; } = 1f;
        public float SpawnRateMultiplier { get; private set; } = 1f;

        // Private constructor so the class can only be instantiated via Instance.
        private DifficultyManager() { }

        // Increase difficulty: call this method to raise difficulty (e.g., new wave).
        public void IncreaseDifficulty()
        {
            Level++;
            BaseEnemyCount += 2;              // Increase the number of enemies per map.
            EnemyHealthMultiplier += 0.2f;      // Increase enemy health.
            EnemySpeedMultiplier += 0.1f;       // Increase enemy speed.
            EnemyDamageMultiplier += 0.1f;      // Increase enemy bullet damage.
            SpawnRateMultiplier += 0.1f;        // Optionally, make enemies spawn faster.
        }

        // Reset all difficulty parameters to their easy defaults.
        public void ResetToEasy()
        {
            Level = 1;
            BaseEnemyCount = 2;
            EnemyHealthMultiplier = 1f;
            EnemySpeedMultiplier = 1f;
            EnemyDamageMultiplier = 1f;
            SpawnRateMultiplier = 1f;
        }
    }
}
