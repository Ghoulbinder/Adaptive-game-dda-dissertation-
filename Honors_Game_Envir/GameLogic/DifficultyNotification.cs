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

        // PSEUDOCODE: Determine if the notification should still be shown.
        public bool IsActive => timer < duration;

        // PSEUDOCODE: Initialize a new notification with text, how long to show it, and which font to use.
        public DifficultyNotification(string message, float duration, SpriteFont font)
        {
            this.message = message;
            this.duration = duration;
            this.font = font;
            timer = 0f; // PSEUDOCODE: Start the display timer at zero.
        }

        // PSEUDOCODE: Advance the timer by the elapsed time; once timer exceeds duration, IsActive becomes false.
        public void Update(GameTime gameTime)
        {
            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        // PSEUDOCODE: If still active, draw the message centered at the top of the screen.
        public void Draw(SpriteBatch spriteBatch, Viewport viewport)
        {
            Vector2 textSize = font.MeasureString(message);
            Vector2 position = new Vector2((viewport.Width - textSize.X) / 2, 20);
            spriteBatch.DrawString(font, message, position, Color.Yellow);
        }
    }
}
