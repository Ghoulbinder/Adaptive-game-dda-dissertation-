using System;
using System.Collections.Generic;
using System.Diagnostics;  // For Debug.WriteLine
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using DDA_Tool; // For the DLL types

namespace Survivor_of_the_Bulge
{
    public enum GameState { MainMenu, GreenForestCentre, ForestTop, ForestLeft, ForestButtom, ForestRight, Scoreboard }

    public class Game1 : Game
    {
        // PSEUDOCODE: Declare graphics and sprite-batch managers
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // PSEUDOCODE: Declare player, stats, UI, and world state
        private Player player;
        private PlayerStats playerStats;
        private GameState currentState;
        private Texture2D mainMenuBackground;
        private SpriteFont gameFont;
        private MenuState mainMenu;
        private PauseMenu pauseMenu;
        private const int TileSize = 25;
        private KeyboardState previousKeyboardState;
        private List<Transition> transitions;
        private Dictionary<GameState, Map> maps;
        private List<FallingLeaf> fallingLeaves;
        private List<SnowFlake> snowFlakes;
        private Random random = new Random();
        private bool isPaused = false;

        // PSEUDOCODE: Autosave and scoreboard fields
        private GameData gameData;
        public int currentLevel = 1;
        public int currentLives = 3;
        public int currentScore = 0;
        public int bulletsFiredThisSession = 0;
        public int bulletsUsedAgainstEnemiesThisSession = 0;
        public int bulletsUsedAgainstBossesThisSession = 0;
        public int livesLostThisSession = 0;
        public int deathsThisSession = 0;
        private DateTime sessionStartTime;

        // PSEUDOCODE: Track kills for DDA thresholds
        public int totalEnemiesKilled = 0;
        public int totalBossesKilled = 0;
        public PlayerStats PlayerStatsInstance => playerStats;
        public DateTime SessionStartTime => sessionStartTime;

        // PSEUDOCODE: Difficulty change notification
        private DifficultyLevel previousDifficulty;
        private DifficultyNotification difficultyNotification;

        // PSEUDOCODE: Scoreboard state
        private ScoreboardState scoreboardState;
        private bool gameOverTriggered = false;

        // PSEUDOCODE: FPS calculation fields for debug overlay
        private double totalTime = 0;
        private int frameCount = 0;
        private int fps = 0;
        public int FPS => fps;

        public GameState CurrentGameState => currentState;
        public Map CurrentMap => (currentState != GameState.MainMenu && maps.ContainsKey(currentState)) ? maps[currentState] : null;

        // PSEUDOCODE: Helpers for tests
        public int GetCurrentEnemyCount()
        {
            if (currentState != GameState.MainMenu && maps.ContainsKey(currentState))
                return maps[currentState].Enemies.Count;
            return 0;
        }
        public int GetBossCount() => totalBossesKilled;

        // PSEUDOCODE: Singleton reference
        public static Game1 Instance { get; private set; }

        public Game1()
        {
            // PSEUDOCODE: Set up singleton and graphics device
            Instance = this;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            _graphics.PreferredBackBufferWidth = 1600;
            _graphics.PreferredBackBufferHeight = 980;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            // PSEUDOCODE: Set initial game state and input
            currentState = GameState.MainMenu;
            InitializeTransitions();
            previousKeyboardState = Keyboard.GetState();

            // PSEUDOCODE: Initialize DDA difficulty
            DifficultyManager.Instance.SetDifficulty(DifficultyLevel.Default);
            previousDifficulty = DifficultyManager.Instance.CurrentDifficulty;

            // PSEUDOCODE: Load persistent data and reset counters
            gameData = SaveLoadManager.LoadGameData();
            sessionStartTime = DateTime.Now;
            totalEnemiesKilled = totalBossesKilled = 0;

            Debug.WriteLine("Game initialized in state: " + currentState);

            base.Initialize();
        }

        private void InitializeTransitions()
        {
            // PSEUDOCODE: Define teleport zones between maps
            transitions = new List<Transition>
            {
                new Transition(GameState.GreenForestCentre, GameState.ForestTop, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, TileSize * 2)),
                // ... other transitions omitted for brevity
            };
        }

        protected override void LoadContent()
        {
            // PSEUDOCODE: Load textures, fonts, and create player instance
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            mainMenuBackground = Content.Load<Texture2D>("Images/Maps/mmBackground2");
            gameFont = Content.Load<SpriteFont>("Fonts/jungleFont");
            // ... load bullets, player textures, stats, maps, weather effects
        }

        protected override void Update(GameTime gameTime)
        {
            // PSEUDOCODE: Update FPS counter
            totalTime += gameTime.ElapsedGameTime.TotalSeconds;
            frameCount++;
            if (totalTime >= 1)
            {
                fps = frameCount;
                frameCount = 0;
                totalTime -= 1;
            }

            var currentKeyboardState = Keyboard.GetState();

            // PSEUDOCODE: Handle pause toggle
            if (currentKeyboardState.IsKeyDown(Keys.Tab) && previousKeyboardState.IsKeyUp(Keys.Tab))
            {
                isPaused = !isPaused;
                IsMouseVisible = isPaused;
                Debug.WriteLine("Pause toggled: " + isPaused);
            }

            // PSEUDOCODE: Difficulty hotkeys
            if (currentKeyboardState.IsKeyDown(Keys.D0)) DifficultyManager.Instance.HandleKeyInput('0');
            if (currentKeyboardState.IsKeyDown(Keys.D1)) DifficultyManager.Instance.HandleKeyInput('1');
            if (currentKeyboardState.IsKeyDown(Keys.D2)) DifficultyManager.Instance.HandleKeyInput('2');
            if (currentKeyboardState.IsKeyDown(Keys.D3)) DifficultyManager.Instance.HandleKeyInput('3');

            // PSEUDOCODE: Show notification on difficulty change
            if (DifficultyManager.Instance.CurrentDifficulty != previousDifficulty)
            {
                difficultyNotification = new DifficultyNotification(
                    "Difficulty changed to " + DifficultyManager.Instance.CurrentDifficulty, 5f, gameFont);
                previousDifficulty = DifficultyManager.Instance.CurrentDifficulty;
            }

            // PSEUDOCODE: Trigger GameOver on Escape or zero lives
            if ((!gameOverTriggered && currentKeyboardState.IsKeyDown(Keys.Escape) && previousKeyboardState.IsKeyUp(Keys.Escape))
                || (!gameOverTriggered && playerStats.Lives <= 0))
            {
                GameOver();
                gameOverTriggered = true;
                return;
            }

            // PSEUDOCODE: Main menu start
            if (currentState == GameState.MainMenu && currentKeyboardState.IsKeyDown(Keys.Enter))
            {
                currentState = GameState.GreenForestCentre;
            }

            // PSEUDOCODE: In-game updates (player, enemies, spawns, transitions)
            if (!isPaused && currentState != GameState.MainMenu && currentState != GameState.Scoreboard)
            {
                var currentMap = maps[currentState];
                player.Update(gameTime, _graphics.GraphicsDevice.Viewport, currentMap.Enemies);
                playerStats.UpdateHealth(player.Health);

                // PSEUDOCODE: Remove dead enemies and count kills
                for (int i = currentMap.Enemies.Count - 1; i >= 0; i--)
                {
                    if (currentMap.Enemies[i].IsDead)
                    {
                        if (currentMap.Enemies[i] is Boss) totalBossesKilled++;
                        else totalEnemiesKilled++;
                        currentMap.IncrementKillCount();
                        currentMap.Enemies.RemoveAt(i);
                    }
                }

                // PSEUDOCODE: Spawn boss when threshold reached
                if (currentMap.KillCount >= DifficultyManager.Instance.BossSpawnThreshold && !currentMap.BossSpawned)
                {
                    // ... instantiate and add boss based on current State
                    currentMap.SetBossSpawned();
                }

                // PSEUDOCODE: Update all enemies, environment, and handle transitions
                foreach (var enemy in currentMap.Enemies) enemy.Update(gameTime, _graphics.GraphicsDevice.Viewport, player.Position, player);
                currentMap.UpdateRespawn(gameTime);
                // ... update weather and transitions
            }

            // PSEUDOCODE: Scoreboard update
            if (currentState == GameState.Scoreboard)
            {
                scoreboardState.Update(gameTime);
                if (scoreboardState.Finished) Environment.Exit(0);
            }

            previousKeyboardState = currentKeyboardState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // PSEUDOCODE: Clear screen and begin sprite batch
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();

            if (currentState == GameState.Scoreboard)
            {
                scoreboardState.Draw(_spriteBatch, GraphicsDevice);
            }
            else if (currentState == GameState.MainMenu)
            {
                // PSEUDOCODE: Draw main menu and intro text
                _spriteBatch.Draw(mainMenuBackground, Vector2.Zero, Color.White);
                mainMenu.Draw(_spriteBatch);
                // ... draw intro story text
            }
            else
            {
                // PSEUDOCODE: Draw game world, player, enemies, UI
                var currentMap = maps[currentState];
                _spriteBatch.Draw(currentMap.Background, Vector2.Zero, Color.White);
                // ... draw weather, player, enemies, stats
            }

            // PSEUDOCODE: Draw pause menu or difficulty notification if active
            if (isPaused) pauseMenu.Draw(_spriteBatch, playerStats);
            if (difficultyNotification != null)
            {
                difficultyNotification.Update(gameTime);
                if (difficultyNotification.IsActive)
                    difficultyNotification.Draw(_spriteBatch, GraphicsDevice.Viewport);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        public void GameOver()
        {
            // PSEUDOCODE: Reset difficulty, switch to scoreboard, pass session data
            var endingDifficulty = DifficultyManager.Instance.CurrentDifficulty;
            DifficultyManager.Instance.SetDifficulty(DifficultyLevel.Medium);
            currentState = GameState.Scoreboard;
            double timeSpent = (DateTime.Now - sessionStartTime).TotalSeconds;
            scoreboardState = new ScoreboardState(
                gameFont, timeSpent, bulletsFiredThisSession,
                bulletsUsedAgainstEnemiesThisSession, bulletsUsedAgainstBossesThisSession,
                livesLostThisSession, currentScore, currentLevel, currentLives,
                deathsThisSession, gameData, endingDifficulty
            );
        }

        public void ResetGameForTesting()
        {
            // PSEUDOCODE: Reset all session variables and flags
            currentState = GameState.MainMenu;
            gameOverTriggered = false;
            currentLevel = 1;
            currentLives = 3;
            currentScore = 0;
            bulletsFiredThisSession = bulletsUsedAgainstEnemiesThisSession =
            bulletsUsedAgainstBossesThisSession = livesLostThisSession = deathsThisSession = 0;
            sessionStartTime = DateTime.Now;
            totalEnemiesKilled = totalBossesKilled = 0;
            Debug.WriteLine("Game state reset for testing.");
        }
    }
}
