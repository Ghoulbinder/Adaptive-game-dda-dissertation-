using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Survivor_of_the_Bulge
{
    public class Button
    {
        private Texture2D texture;
        private SpriteFont font;
        public string Text { get; private set; }
        public Rectangle Bounds { get; private set; }
        public bool IsClicked { get; private set; }

        public Button(Texture2D texture, SpriteFont font, string text, Rectangle bounds)
        {
            // PSEUDOCODE: Store the button's texture, font, display text, and clickable area
            this.texture = texture;
            this.font = font;
            Text = text;
            Bounds = bounds;
        }

        public void Update(MouseState mouse)
        {
            // PSEUDOCODE: Check if mouse cursor is within the button's bounds and the left button is pressed
            if (Bounds.Contains(mouse.Position) && mouse.LeftButton == ButtonState.Pressed)
                IsClicked = true;   // PSEUDOCODE: Mark the button as clicked
            else
                IsClicked = false;  // PSEUDOCODE: Otherwise, ensure button is not clicked
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // PSEUDOCODE: Draw the button background texture at its defined bounds
            spriteBatch.Draw(texture, Bounds, Color.White);

            // PSEUDOCODE: Measure the size of the text to center it on the button
            Vector2 textSize = font.MeasureString(Text);
            Vector2 textPos = new Vector2(
                Bounds.X + (Bounds.Width - textSize.X) / 2,
                Bounds.Y + (Bounds.Height - textSize.Y) / 2
            );

            // PSEUDOCODE: Draw the button text centered within the button's bounds
            spriteBatch.DrawString(font, Text, textPos, Color.Black);
        }
    }
}
