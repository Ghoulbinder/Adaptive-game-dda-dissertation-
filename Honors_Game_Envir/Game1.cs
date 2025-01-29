using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Survivor_of_the_Bulge
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Player player;

        private enum GameState { MainMenu, GreenForestCentre, ForestTop, ForestButtom, ForestLeft, ForestRight }
        private GameState currentState;

        private Texture2D mainMenuBackground;
        private Texture2D greenForestBackground;
        private Texture2D forestTopBackground;
        private Texture2D forestButtomBackground;
        private Texture2D forestLeftBackground;
        private Texture2D forestRightBackground;
        private Texture2D gridLineTexture;
        private SpriteFont gameFont;

        private MenuState menuState;

        private const int TileSize = 25; // Each tile fits 4 smaller cells in the original size
        private Rectangle mapBounds; // Defines the map boundaries

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Set window size
            _graphics.PreferredBackBufferWidth = 1600; // Window width
            _graphics.PreferredBackBufferHeight = 980; // Window height
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            currentState = GameState.MainMenu;

            // Define map bounds to match the original map size
            mapBounds = new Rectangle(0, 0, 1600, 1600);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load background images
            mainMenuBackground = Content.Load<Texture2D>("Images/Maps/mmBackground");
            greenForestBackground = Content.Load<Texture2D>("Images/Maps/greenForestCentre");
            forestTopBackground = Content.Load<Texture2D>("Images/Maps/snowForestTop");
            forestButtomBackground = Content.Load<Texture2D>("Images/Maps/snowForestButtom");
            forestLeftBackground = Content.Load<Texture2D>("Images/Maps/snowForestLeft");
            forestRightBackground = Content.Load<Texture2D>("Images/Maps/snowForestRight");

            // Create a white texture for grid lines
            gridLineTexture = new Texture2D(GraphicsDevice, 1, 1);
            gridLineTexture.SetData(new[] { Color.White });

            gameFont = Content.Load<SpriteFont>("Fonts/jungleFont");

            // Initialize player
            player = new Player(
                Content.Load<Texture2D>("Images/Soldier/backWalking"),
                Content.Load<Texture2D>("Images/Soldier/frontWalking"),
                Content.Load<Texture2D>("Images/Soldier/leftWalking"),
                new Vector2(100, 100)
            );

            // Initialize menu
            menuState = new MenuState(gameFont, mainMenuBackground);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Enter) && currentState == GameState.MainMenu)
            {
                currentState = GameState.GreenForestCentre;
            }

            if (currentState != GameState.MainMenu)
            {
                player.Update(gameTime, _graphics.GraphicsDevice.Viewport);
            }

            base.Update(gameTime);
        }

        private void DrawGridOverlay(int mapWidth, int mapHeight, float scaleX, float scaleY)
        {
            // Adjust for scaled tile size
            int scaledTileSize = (int)(TileSize * scaleX);

            for (int y = 0; y <= mapHeight; y += scaledTileSize)
            {
                for (int x = 0; x <= mapWidth; x += scaledTileSize)
                {
                    // Draw horizontal lines
                    _spriteBatch.Draw(gridLineTexture, new Rectangle(0, y, mapWidth, 1), Color.White * 0.3f);

                    // Draw vertical lines
                    _spriteBatch.Draw(gridLineTexture, new Rectangle(x, 0, 1, mapHeight), Color.White * 0.3f);

                    // Draw cell numbers (row.column) only for the top-left corner of the tile
                    int row = y / scaledTileSize;
                    int column = x / scaledTileSize;

                    // Avoid overlapping numbers outside the map boundaries
                    if (x < mapWidth && y < mapHeight)
                    {
                        _spriteBatch.DrawString(
                            gameFont,
                            $"{row},{column}",
                            new Vector2(x + 5, y + 5), // Offset the numbers slightly
                            Color.Black,
                            0f,
                            Vector2.Zero,
                            0.5f, // Scale down the font size
                            SpriteEffects.None,
                            0f
                        );
                    }
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            float scaleX = (float)_graphics.PreferredBackBufferWidth / mapBounds.Width;
            float scaleY = (float)_graphics.PreferredBackBufferHeight / mapBounds.Height;

            switch (currentState)
            {
                case GameState.MainMenu:
                    _spriteBatch.Draw(mainMenuBackground, Vector2.Zero, Color.White);
                    menuState.Draw(_spriteBatch);
                    break;

                case GameState.GreenForestCentre:
                    _spriteBatch.Draw(greenForestBackground, Vector2.Zero, null, Color.White, 0f, Vector2.Zero,
                        new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    DrawGridOverlay(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight, scaleX, scaleY);
                    player.Draw(_spriteBatch);
                    break;

                case GameState.ForestTop:
                    _spriteBatch.Draw(forestTopBackground, Vector2.Zero, null, Color.White, 0f, Vector2.Zero,
                        new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    DrawGridOverlay(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight, scaleX, scaleY);
                    player.Draw(_spriteBatch);
                    break;

                case GameState.ForestButtom:
                    _spriteBatch.Draw(forestButtomBackground, Vector2.Zero, null, Color.White, 0f, Vector2.Zero,
                        new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    DrawGridOverlay(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight, scaleX, scaleY);
                    player.Draw(_spriteBatch);
                    break;

                case GameState.ForestLeft:
                    _spriteBatch.Draw(forestLeftBackground, Vector2.Zero, null, Color.White, 0f, Vector2.Zero,
                        new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    DrawGridOverlay(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight, scaleX, scaleY);
                    player.Draw(_spriteBatch);
                    break;

                case GameState.ForestRight:
                    _spriteBatch.Draw(forestRightBackground, Vector2.Zero, null, Color.White, 0f, Vector2.Zero,
                        new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
                    DrawGridOverlay(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight, scaleX, scaleY);
                    player.Draw(_spriteBatch);
                    break;
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
