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
            this.texture = texture;
            this.font = font;
            Text = text;
            Bounds = bounds;
        }

        public void Update(MouseState mouse)
        {
            if (Bounds.Contains(mouse.Position) && mouse.LeftButton == ButtonState.Pressed)
                IsClicked = true;
            else
                IsClicked = false;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Bounds, Color.White);
            Vector2 textSize = font.MeasureString(Text);
            Vector2 textPos = new Vector2(Bounds.X + (Bounds.Width - textSize.X) / 2, Bounds.Y + (Bounds.Height - textSize.Y) / 2);
            spriteBatch.DrawString(font, Text, textPos, Color.Black);
        }
    }
}
