using System;
using System.Collections.Generic;
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

        private const int TileSize = 25;
        private Rectangle mapBounds;

        private Dictionary<Point, (GameState, Point)> transitions;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1600;
            _graphics.PreferredBackBufferHeight = 980;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            currentState = GameState.MainMenu;
            mapBounds = new Rectangle(0, 0, 1600, 1600);
            InitializeTransitions();
            base.Initialize();
        }

        private void InitializeTransitions()
        {
            transitions = new Dictionary<Point, (GameState, Point)>();

            // GreenForestCentre transitions
            transitions[new Point(0, 29)] = (GameState.ForestTop, new Point(38, 29));
            transitions[new Point(0, 30)] = (GameState.ForestTop, new Point(38, 30));
            transitions[new Point(0, 31)] = (GameState.ForestTop, new Point(38, 31));

            transitions[new Point(38, 29)] = (GameState.ForestButtom, new Point(0, 29));
            transitions[new Point(38, 30)] = (GameState.ForestButtom, new Point(0, 30));
            transitions[new Point(38, 31)] = (GameState.ForestButtom, new Point(0, 31));

            transitions[new Point(17, 0)] = (GameState.ForestLeft, new Point(17, 63));
            transitions[new Point(18, 0)] = (GameState.ForestLeft, new Point(18, 63));

            transitions[new Point(17, 63)] = (GameState.ForestRight, new Point(17, 0));
            transitions[new Point(18, 63)] = (GameState.ForestRight, new Point(18, 0));

            // ForestTop transitions
            transitions[new Point(38, 29)] = (GameState.GreenForestCentre, new Point(0, 29));
            transitions[new Point(38, 30)] = (GameState.GreenForestCentre, new Point(0, 30));
            transitions[new Point(38, 31)] = (GameState.GreenForestCentre, new Point(0, 31));

            // ForestButtom transitions
            transitions[new Point(0, 29)] = (GameState.GreenForestCentre, new Point(38, 29));
            transitions[new Point(0, 30)] = (GameState.GreenForestCentre, new Point(38, 30));
            transitions[new Point(0, 31)] = (GameState.GreenForestCentre, new Point(38, 31));

            // ForestLeft transitions
            transitions[new Point(17, 63)] = (GameState.GreenForestCentre, new Point(17, 0));
            transitions[new Point(18, 63)] = (GameState.GreenForestCentre, new Point(18, 0));

            // ForestRight transitions
            transitions[new Point(17, 0)] = (GameState.GreenForestCentre, new Point(17, 63));
            transitions[new Point(18, 0)] = (GameState.GreenForestCentre, new Point(18, 63));
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            mainMenuBackground = Content.Load<Texture2D>("Images/Maps/mmBackground");
            greenForestBackground = Content.Load<Texture2D>("Images/Maps/greenForestCentre");
            forestTopBackground = Content.Load<Texture2D>("Images/Maps/snowForestTop");
            forestButtomBackground = Content.Load<Texture2D>("Images/Maps/snowForestButtom");
            forestLeftBackground = Content.Load<Texture2D>("Images/Maps/snowForestLeft");
            forestRightBackground = Content.Load<Texture2D>("Images/Maps/snowForestRight");

            gridLineTexture = new Texture2D(GraphicsDevice, 1, 1);
            gridLineTexture.SetData(new[] { Color.White });

            gameFont = Content.Load<SpriteFont>("Fonts/jungleFont");

            player = new Player(
                Content.Load<Texture2D>("Images/Soldier/backWalking"),
                Content.Load<Texture2D>("Images/Soldier/frontWalking"),
                Content.Load<Texture2D>("Images/Soldier/leftWalking"),
                new Vector2(100, 100)
            );

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
                Vector2 previousPosition = player.Position;
                player.Update(gameTime, _graphics.GraphicsDevice.Viewport);

                Point gridPosition = new Point((int)(player.Position.Y / TileSize), (int)(player.Position.X / TileSize));

                if (transitions.ContainsKey(gridPosition))
                {
                    var (nextState, newGridPosition) = transitions[gridPosition];
                    currentState = nextState;
                    player.Position = new Vector2(newGridPosition.Y * TileSize, newGridPosition.X * TileSize);
                }
            }

            base.Update(gameTime);
        }

        private void DrawGridOverlay(int mapWidth, int mapHeight, float scaleX, float scaleY)
        {
            int scaledTileSize = (int)(TileSize * scaleX);

            for (int y = 0; y <= mapHeight; y += scaledTileSize)
            {
                for (int x = 0; x <= mapWidth; x += scaledTileSize)
                {
                    _spriteBatch.Draw(gridLineTexture, new Rectangle(0, y, mapWidth, 1), Color.White * 0.3f);
                    _spriteBatch.Draw(gridLineTexture, new Rectangle(x, 0, 1, mapHeight), Color.White * 0.3f);
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
