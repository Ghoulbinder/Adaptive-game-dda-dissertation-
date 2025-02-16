using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Survivor_of_the_Bulge
{
    public class Map
    {
        public Texture2D Background { get; }
        public List<Enemy> Enemies { get; }
        public int KillCount { get; private set; } = 0;
        public bool BossSpawned { get; private set; } = false;

        // Spawn parameters for respawning new enemies.
        private Texture2D enemyBackTexture;
        private Texture2D enemyFrontTexture;
        private Texture2D enemyLeftTexture;
        private Texture2D enemyBulletHorizontal;
        private Texture2D enemyBulletVertical;

        // Timer for enemy respawn.
        private float respawnTimer = 0f;

        public Map(Texture2D background, List<Enemy> enemies)
        {
            Background = background;
            Enemies = enemies;
        }

        public void AddEnemy(Enemy enemy)
        {
            Enemies.Add(enemy);
        }

        public void SetEnemySpawnParameters(Texture2D back, Texture2D front, Texture2D left, Texture2D bulletH, Texture2D bulletV)
        {
            enemyBackTexture = back;
            enemyFrontTexture = front;
            enemyLeftTexture = left;
            enemyBulletHorizontal = bulletH;
            enemyBulletVertical = bulletV;
        }

        public void IncrementKillCount()
        {
            KillCount++;
        }

        public void SetBossSpawned()
        {
            BossSpawned = true;
        }

        /// <summary>
        /// Call this each update. If there are fewer than 2 enemies and no boss is spawned,
        /// wait 1 second and then spawn a new enemy using preset spawn parameters.
        /// </summary>
        public void UpdateRespawn(GameTime gameTime)
        {
            if (!BossSpawned && Enemies.Count < 2 && enemyBackTexture != null)
            {
                respawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (respawnTimer >= 1f)
                {
                    Random rng = new Random();
                    int enemyWidth = enemyLeftTexture.Width / 4; // approximate frame width
                    int enemyHeight = enemyLeftTexture.Height;
                    int x = rng.Next(0, Math.Max(1, Background.Width - enemyWidth));
                    int y = rng.Next(0, Math.Max(1, Background.Height - enemyHeight));
                    Array directions = Enum.GetValues(typeof(Enemy.Direction));
                    Enemy.Direction dir = (Enemy.Direction)directions.GetValue(rng.Next(directions.Length));
                    int baseHealth = 50;
                    int baseDamage = 5;
                    int enemyHealth = (int)(baseHealth * DifficultyManager.Instance.EnemyHealthMultiplier);
                    int enemyDamage = (int)(baseDamage * DifficultyManager.Instance.EnemyDamageMultiplier);

                    Enemy newEnemy = new Enemy(
                        enemyBackTexture,
                        enemyFrontTexture,
                        enemyLeftTexture,
                        enemyBulletHorizontal,
                        enemyBulletVertical,
                        new Vector2(x, y),
                        dir,
                        enemyHealth,
                        enemyDamage
                    );
                    Enemies.Add(newEnemy);
                    respawnTimer = 0f;
                }
            }
            else
            {
                respawnTimer = 0f;
            }
        }
    }
}
