using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        }
    }
}
