using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Survivor_of_the_Bulge
{
    public class MenuState
    {
        private SpriteFont font;
        private Texture2D backgroundImage;
        private bool startGame;

        public MenuState(SpriteFont font, Texture2D backgroundImage)
        {
            this.font = font;
            this.backgroundImage = backgroundImage;
            this.startGame = false;
        }

        public bool Update(GameTime gameTime)
        {
            var keyboardState = Keyboard.GetState();

            // Start the game when Enter is pressed
            if (keyboardState.IsKeyDown(Keys.Enter))
            {
                startGame = true;
            }

            return startGame;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(backgroundImage, Vector2.Zero, Color.White);
            spriteBatch.DrawString(font, "Press Enter to Start", new Vector2(200, 200), Color.Black);
        }
    }
}
