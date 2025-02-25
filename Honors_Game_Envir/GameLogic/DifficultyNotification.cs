using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Survivor_of_the_Bulge
{
    public class DifficultyNotification
    {
        private string message;
        private float duration;
        private float timer;
        private SpriteFont font;

        public bool IsActive => timer < duration;

        public DifficultyNotification(string message, float duration, SpriteFont font)
        {
            this.message = message;
            this.duration = duration;
            this.font = font;
            timer = 0f;
        }

        public void Update(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(SpriteBatch spriteBatch, Viewport viewport)
        {
            Vector2 textSize = font.MeasureString(message);
            Vector2 position = new Vector2((viewport.Width - textSize.X) / 2, 20);
            spriteBatch.DrawString(font, message, position, Color.Yellow);
        }
    }
}
