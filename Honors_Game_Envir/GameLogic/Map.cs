using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survivor_of_the_Bulge
{
    public class Map
    {
        // The map's background texture.
        public Texture2D Background { get; private set; }
        // List of enemies currently present on the map.
        public List<Enemy> Enemies { get; private set; }

        // Internal flag and counter for boss spawning and enemy kills.
        private bool bossSpawned = false;
        private int killCount = 0;

        // Respawn timer and interval (in seconds).
        private float respawnTimer = 0f;
        private float respawnInterval = 1f; // New enemy spawns 1 second after a kill (if needed).

        // Stored enemy spawn parameters for creating new enemies.
        private Texture2D enemyBack;
        private Texture2D enemyFront;
        private Texture2D enemyLeft;
        private Texture2D enemyBulletHorizontal;
        private Texture2D enemyBulletVertical;

        public Map(Texture2D background, List<Enemy> enemies)
        {
            Background = background;
            Enemies = enemies;
        }

        /// <summary>
        /// Adds an enemy to the map.
        /// </summary>
        public void AddEnemy(Enemy e)
        {
            Enemies.Add(e);
        }

        /// <summary>
        /// Marks that a boss has been spawned on this map.
        /// Once a boss is active, new enemies will not spawn.
        /// </summary>
        public void SetBossSpawned()
        {
            bossSpawned = true;
        }

        /// <summary>
        /// Gets whether a boss has been spawned on this map.
        /// </summary>
        public bool BossSpawned => bossSpawned;

        /// <summary>
        /// Increments the enemy kill count.
        /// </summary>
        public void IncrementKillCount()
        {
            killCount++;
        }

        /// <summary>
        /// Returns the current kill count.
        /// </summary>
        public int KillCount => killCount;

        /// <summary>
        /// Checks the respawn timer and spawns new enemies if:
        /// - No boss is present.
        /// - The current enemy count is below the difficulty cap (BaseEnemyCount).
        /// Newly spawned enemies use the current difficulty multipliers.
        /// </summary>
        public void UpdateRespawn(GameTime gameTime)
        {
            // Do not spawn new enemies if a boss is active.
            if (bossSpawned)
                return;

            respawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (Enemies.Count < DifficultyManager.Instance.BaseEnemyCount && respawnTimer >= respawnInterval)
            {
                if (enemyBack != null && enemyFront != null && enemyLeft != null &&
                    enemyBulletHorizontal != null && enemyBulletVertical != null)
                {
                    Random rng = new Random();
                    int x = rng.Next(0, Math.Max(1, Background.Width - enemyLeft.Width / 4));
                    int y = rng.Next(0, Math.Max(1, Background.Height - enemyLeft.Height));
                    Array dirs = Enum.GetValues(typeof(Enemy.Direction));
                    Enemy.Direction dir = (Enemy.Direction)dirs.GetValue(rng.Next(dirs.Length));

                    // Base stats for enemy spawn.
                    int baseHealth = 50;
                    int baseDamage = 5;
                    // Final stats based on current difficulty multipliers.
                    int finalHealth = (int)(baseHealth * DifficultyManager.Instance.EnemyHealthMultiplier);
                    int finalDamage = (int)(baseDamage * DifficultyManager.Instance.EnemyDamageMultiplier);

                    Enemy e = new Enemy(
                        enemyBack,
                        enemyFront,
                        enemyLeft,
                        enemyBulletHorizontal,
                        enemyBulletVertical,
                        new Vector2(x, y),
                        dir,
                        finalHealth,
                        finalDamage
                    );
                    // Store base stats so that future difficulty changes update this enemy.
                    e.SetBaseStats(baseHealth, baseDamage);
                    AddEnemy(e);
                }
                respawnTimer = 0f;
            }
        }

        /// <summary>
        /// Stores the enemy and bullet textures used for spawning new enemies.
        /// </summary>
        public void SetEnemySpawnParameters(Texture2D back, Texture2D front, Texture2D left, Texture2D bulletHorizontal, Texture2D bulletVertical)
        {
            enemyBack = back;
            enemyFront = front;
            enemyLeft = left;
            enemyBulletHorizontal = bulletHorizontal;
            enemyBulletVertical = bulletVertical;
        }
    }
}
