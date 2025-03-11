using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace Survivor_of_the_Bulge
{
    public class PauseMenu
    {
        private SpriteFont font;
        private Texture2D panelTexture;

        public PauseMenu(SpriteFont font, Texture2D panelTexture)
        {
            this.font = font;
            this.panelTexture = panelTexture;
        }

        public void Update(GameTime gameTime)
        {
            // No interactive elements to update.
        }

        public void Draw(SpriteBatch spriteBatch, PlayerStats stats)
        {
            // Draw a semi-transparent panel.
            Rectangle panelRect = new Rectangle(200, 150, 400, 300);
            spriteBatch.Draw(panelTexture, panelRect, Color.White * 0.8f);

            // Draw pause message.
            spriteBatch.DrawString(font, "Press Tab to Resume", new Vector2(panelRect.X + 50, panelRect.Y + 20), Color.Black);

            // Draw stat information.
            string statText = $"Health: {stats.Health}\nLives: {stats.Lives}\nAttack Damage: {stats.AttackDamage}\nAttack Speed: {stats.AttackSpeed}\nMovement Speed: {stats.MovementSpeed}\nExp: {stats.Experience}\nLevel: {stats.Level}";
            spriteBatch.DrawString(font, statText, new Vector2(panelRect.X + 20, panelRect.Y + 80), Color.Black);

            // Build debug overlay information.
            string debugInfo = $"DEBUG INFO:\n" +
                               $"Game State: {Game1.Instance.CurrentGameState}\n" +
                               $"Difficulty: {DifficultyManager.Instance.CurrentDifficulty}\n" +
                               $"Enemies: {Game1.Instance.GetCurrentEnemyCount()}\n" +
                               $"Bosses: {Game1.Instance.GetBossCount()}\n" +
                               $"FPS: {Game1.Instance.FPS}\n";

            // If there is at least one enemy, display its stats.
            Map currentMap = Game1.Instance.CurrentMap;
            if (currentMap != null && currentMap.Enemies.Count > 0)
            {
                Enemy sampleEnemy = currentMap.Enemies[0];
                debugInfo += "\nSample Enemy Stats:\n" +
                             $"Health: {sampleEnemy.Health}\n" +
                             $"Damage: {sampleEnemy.BulletDamage}\n" +
                             $"Movement Speed: {sampleEnemy.MovementSpeed}\n" +
                             $"Firing Interval: {sampleEnemy.FiringInterval}\n";
            }

            // If there is at least one boss, display its stats.
            Boss sampleBoss = currentMap != null ? currentMap.Enemies.FirstOrDefault(e => e is Boss) as Boss : null;
            if (sampleBoss != null)
            {
                debugInfo += "\nSample Boss Stats:\n" +
                             $"Health: {sampleBoss.Health}\n" +
                             $"Damage: {sampleBoss.BulletDamage}\n" +
                             $"Movement Speed: {sampleBoss.MovementSpeed}\n" +
                             $"Firing Interval: {sampleBoss.FiringInterval}\n";
            }

            Vector2 debugPos = new Vector2(panelRect.X, panelRect.Y + panelRect.Height + 10);
            spriteBatch.DrawString(font, debugInfo, debugPos, Color.Yellow);
        }
    }
}
