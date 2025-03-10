using System;
using DDA_Tool;  // Import the DLL's namespace

namespace Survivor_of_the_Bulge
{
    // Local enum that includes Default along with the DLL's Easy/Normal/Hard.
    public enum DifficultyLevel { Default, Easy, Normal, Hard }

    // Singleton class that holds parameters used to adjust game difficulty dynamically.
    // It reads enemy count and multipliers from the DLL's DynamicDifficultyController.
    public class DifficultyManager
    {
        private static DifficultyManager _instance;
        public static DifficultyManager Instance => _instance ?? (_instance = new DifficultyManager());

        // The current difficulty level.
        public DifficultyLevel CurrentDifficulty { get; private set; } = DifficultyLevel.Default;

        // Properties for enemy parameters.
        public int BaseEnemyCount { get; private set; }
        public float EnemyHealthMultiplier { get; private set; }
        public float EnemySpeedMultiplier { get; private set; }
        public float EnemyDamageMultiplier { get; private set; }
        public float SpawnRateMultiplier { get; private set; }

        // Additional properties for boss parameters.
        public int BossSpawnThreshold { get; private set; }
        public float BossHealthMultiplier { get; private set; }
        public float BossDamageMultiplier { get; private set; }

        // Instance of the DLL's DynamicDifficultyController.
        private DynamicDifficultyController dynamicController;

        // Private constructor – initialize with Default difficulty.
        private DifficultyManager()
        {
            dynamicController = new DynamicDifficultyController();
            SetDifficulty(DifficultyLevel.Default);
        }

        /// <summary>
        /// Sets the difficulty level.
        /// For Default, preset values are used.
        /// For Easy, Normal, and Hard, the values come from the DLL.
        /// </summary>
        public void SetDifficulty(DifficultyLevel level)
        {
            CurrentDifficulty = level;
            switch (level)
            {
                case DifficultyLevel.Default:
                    BaseEnemyCount = 2;
                    EnemyHealthMultiplier = 0.8f;
                    EnemySpeedMultiplier = 0.8f;
                    EnemyDamageMultiplier = 0.8f;
                    SpawnRateMultiplier = 1f;
                    BossSpawnThreshold = 2; // preset for Default
                    BossHealthMultiplier = 0.8f;
                    BossDamageMultiplier = 0.8f;
                    break;
                case DifficultyLevel.Easy:
                    dynamicController.SetDifficulty(DDA_Tool.DifficultyLevel.Easy);
                    BaseEnemyCount = dynamicController.BossSpawnThreshold;  // Expected 8 for Easy
                    EnemyHealthMultiplier = dynamicController.EnemyHealthMultiplier;         // 1.0f
                    EnemySpeedMultiplier = dynamicController.EnemyMovementSpeedMultiplier;     // 1.0f
                    EnemyDamageMultiplier = dynamicController.EnemyDamageMultiplier;           // 1.0f
                    SpawnRateMultiplier = 1f;
                    BossSpawnThreshold = dynamicController.BossSpawnThreshold;                // Expected 8 for Easy
                    BossHealthMultiplier = dynamicController.BossHealthMultiplier;            // 1.0f
                    BossDamageMultiplier = dynamicController.BossDamageMultiplier;            // 1.0f
                    break;
                case DifficultyLevel.Normal:
                    dynamicController.SetDifficulty(DDA_Tool.DifficultyLevel.Normal);
                    BaseEnemyCount = dynamicController.BossSpawnThreshold;  // Expected 15 for Normal
                    EnemyHealthMultiplier = dynamicController.EnemyHealthMultiplier;         // 1.5f
                    EnemySpeedMultiplier = dynamicController.EnemyMovementSpeedMultiplier;     // 1.5f
                    EnemyDamageMultiplier = dynamicController.EnemyDamageMultiplier;           // 1.5f
                    SpawnRateMultiplier = 1f;
                    BossSpawnThreshold = dynamicController.BossSpawnThreshold;                // Expected 15 for Normal
                    BossHealthMultiplier = dynamicController.BossHealthMultiplier;            // 1.5f
                    BossDamageMultiplier = dynamicController.BossDamageMultiplier;            // 1.5f
                    break;
                case DifficultyLevel.Hard:
                    dynamicController.SetDifficulty(DDA_Tool.DifficultyLevel.Hard);
                    BaseEnemyCount = dynamicController.BossSpawnThreshold;  // Expected 25 for Hard
                    EnemyHealthMultiplier = dynamicController.EnemyHealthMultiplier;         // 2.0f
                    EnemySpeedMultiplier = dynamicController.EnemyMovementSpeedMultiplier;     // 2.0f
                    EnemyDamageMultiplier = dynamicController.EnemyDamageMultiplier;           // 2.0f
                    SpawnRateMultiplier = 1f;
                    BossSpawnThreshold = dynamicController.BossSpawnThreshold;                // Expected 25 for Hard
                    BossHealthMultiplier = dynamicController.BossHealthMultiplier;            // 2.0f
                    BossDamageMultiplier = dynamicController.BossDamageMultiplier;            // 2.0f
                    break;
            }
        }

        /// <summary>
        /// Processes key input to set the difficulty.
        /// '0' for Default, '1' for Easy, '2' for Normal, '3' for Hard.
        /// </summary>
        public void HandleKeyInput(char keyChar)
        {
            if (keyChar == '0')
                SetDifficulty(DifficultyLevel.Default);
            else if (keyChar == '1')
                SetDifficulty(DifficultyLevel.Easy);
            else if (keyChar == '2')
                SetDifficulty(DifficultyLevel.Normal);
            else if (keyChar == '3')
                SetDifficulty(DifficultyLevel.Hard);
        }
    }
}
