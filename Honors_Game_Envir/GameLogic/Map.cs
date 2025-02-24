using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survivor_of_the_Bulge
{
    public class Map
    {
        public Texture2D Background { get; private set; }
        public List<Enemy> Enemies { get; private set; }

        private bool bossSpawned = false;
        private int killCount = 0;

        private float respawnTimer = 0f;
        public float EnemyRespawnInterval { get; set; } = 1f;
        public int BossSpawnThreshold { get; set; } = 2; // Changeable threshold

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

        public void AddEnemy(Enemy e)
        {
            Enemies.Add(e);
        }

        public void SetBossSpawned()
        {
            bossSpawned = true;
        }

        public bool BossSpawned => bossSpawned;

        public void IncrementKillCount()
        {
            killCount++;
        }

        public int KillCount => killCount;

        public void UpdateRespawn(GameTime gameTime)
        {
            respawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (Enemies.Count < 2 && respawnTimer >= EnemyRespawnInterval)
            {
                if (enemyBack != null && enemyFront != null && enemyLeft != null &&
                    enemyBulletHorizontal != null && enemyBulletVertical != null)
                {
                    Random rng = new Random();
                    int x = rng.Next(0, Math.Max(1, Background.Width - enemyLeft.Width));
                    int y = rng.Next(0, Math.Max(1, Background.Height - enemyLeft.Height));
                    Array dirs = Enum.GetValues(typeof(Enemy.Direction));
                    Enemy.Direction randomDirection = (Enemy.Direction)dirs.GetValue(rng.Next(dirs.Length));

                    int baseHealth = 50;
                    int baseDamage = 5;
                    int enemyHealth = (int)(baseHealth * DifficultyManager.Instance.EnemyHealthMultiplier);
                    int enemyDamage = (int)(baseDamage * DifficultyManager.Instance.EnemyDamageMultiplier);

                    Enemy e = new Enemy(
                        enemyBack,
                        enemyFront,
                        enemyLeft,
                        enemyBulletHorizontal,
                        enemyBulletVertical,
                        new Vector2(x, y),
                        randomDirection,
                        enemyHealth,
                        enemyDamage
                    );
                    AddEnemy(e);
                }
                respawnTimer = 0f;
            }
        }

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
