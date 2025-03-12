using System;
using DDA_Tool; // Assuming this is the namespace for your dynamic difficulty controller

namespace Survivor_of_the_Bulge
{
    public enum DifficultyLevel { Default, Easy, Medium, Hard }

    public class DifficultyManager
    {
        private static DifficultyManager _instance;
        public static DifficultyManager Instance => _instance ?? (_instance = new DifficultyManager());

        // The current difficulty level.
        public DifficultyLevel CurrentDifficulty { get; private set; } = DifficultyLevel.Default;

        // Parameters for enemies.
        public int BaseEnemyCount { get; private set; }
        public float EnemyHealthMultiplier { get; private set; }
        public float EnemySpeedMultiplier { get; private set; }
        public float EnemyDamageMultiplier { get; private set; }
        public float SpawnRateMultiplier { get; private set; }

        // Boss-specific multipliers.
        public float BossHealthMultiplier { get; private set; }
        public float BossAttackSpeedMultiplier { get; private set; }
        public float BossMovementSpeedMultiplier { get; private set; }
        public float BossDamageMultiplier { get; private set; }

        // Boss spawn threshold property.
        public int BossSpawnThreshold { get; private set; }

        // Instance of the DLL's dynamic difficulty controller.
        private DynamicDifficultyController dynamicController;

        // Private constructor.
        private DifficultyManager()
        {
            dynamicController = new DynamicDifficultyController();
            SetDifficulty(DifficultyLevel.Default);
        }

        /// <summary>
        /// Sets the difficulty level.
        /// For Default, preset values are used.
        /// For Easy, Medium, and Hard, values are retrieved from the DLL's DynamicDifficultyController.
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
                    BossHealthMultiplier = 0.8f;
                    BossAttackSpeedMultiplier = 0.8f;
                    BossMovementSpeedMultiplier = 0.8f;
                    BossDamageMultiplier = 0.8f;
                    BossSpawnThreshold = 10;
                    break;
                case DifficultyLevel.Easy:
                    dynamicController.SetDifficulty(DDA_Tool.DifficultyLevel.Easy);
                    BaseEnemyCount = dynamicController.BossSpawnThreshold; // Expected 8 for Easy.
                    EnemyHealthMultiplier = dynamicController.EnemyHealthMultiplier;
                    EnemySpeedMultiplier = dynamicController.EnemyMovementSpeedMultiplier;
                    EnemyDamageMultiplier = dynamicController.EnemyDamageMultiplier;
                    SpawnRateMultiplier = 1f;
                    BossHealthMultiplier = dynamicController.BossHealthMultiplier;
                    BossAttackSpeedMultiplier = dynamicController.BossAttackSpeedMultiplier;
                    BossMovementSpeedMultiplier = dynamicController.BossMovementSpeedMultiplier;
                    BossDamageMultiplier = dynamicController.BossDamageMultiplier;
                    BossSpawnThreshold = dynamicController.BossSpawnThreshold; // Expected 8.
                    break;
                case DifficultyLevel.Medium:
                    dynamicController.SetDifficulty(DDA_Tool.DifficultyLevel.Normal); // DLL's Normal now maps to Medium.
                    BaseEnemyCount = dynamicController.BossSpawnThreshold; // Expected 15 for Medium.
                    EnemyHealthMultiplier = dynamicController.EnemyHealthMultiplier;
                    EnemySpeedMultiplier = dynamicController.EnemyMovementSpeedMultiplier;
                    EnemyDamageMultiplier = dynamicController.EnemyDamageMultiplier;
                    SpawnRateMultiplier = 1f;
                    BossHealthMultiplier = dynamicController.BossHealthMultiplier;
                    BossAttackSpeedMultiplier = dynamicController.BossAttackSpeedMultiplier;
                    BossMovementSpeedMultiplier = dynamicController.BossMovementSpeedMultiplier;
                    BossDamageMultiplier = dynamicController.BossDamageMultiplier;
                    BossSpawnThreshold = dynamicController.BossSpawnThreshold; // Expected 15.
                    break;
                case DifficultyLevel.Hard:
                    dynamicController.SetDifficulty(DDA_Tool.DifficultyLevel.Hard);
                    BaseEnemyCount = dynamicController.BossSpawnThreshold; // Expected 25 for Hard.
                    EnemyHealthMultiplier = dynamicController.EnemyHealthMultiplier;
                    EnemySpeedMultiplier = dynamicController.EnemyMovementSpeedMultiplier;
                    EnemyDamageMultiplier = dynamicController.EnemyDamageMultiplier;
                    SpawnRateMultiplier = 1f;
                    BossHealthMultiplier = dynamicController.BossHealthMultiplier;
                    BossAttackSpeedMultiplier = dynamicController.BossAttackSpeedMultiplier;
                    BossMovementSpeedMultiplier = dynamicController.BossMovementSpeedMultiplier;
                    BossDamageMultiplier = dynamicController.BossDamageMultiplier;
                    BossSpawnThreshold = dynamicController.BossSpawnThreshold; // Expected 25.
                    break;
            }
        }

        /// <summary>
        /// Processes key input to set the difficulty level.
        /// '0' for Default, '1' for Easy, '2' for Medium, '3' for Hard.
        /// </summary>
        public void HandleKeyInput(char keyChar)
        {
            if (keyChar == '0')
                SetDifficulty(DifficultyLevel.Default);
            else if (keyChar == '1')
                SetDifficulty(DifficultyLevel.Easy);
            else if (keyChar == '2')
                SetDifficulty(DifficultyLevel.Medium);
            else if (keyChar == '3')
                SetDifficulty(DifficultyLevel.Hard);
        }
    }
}
