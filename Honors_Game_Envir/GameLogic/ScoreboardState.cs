using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Survivor_of_the_Bulge
{
    // This class now serves as a game state for the scoreboard.
    public class ScoreboardState
    {
        private SpriteFont font;
        private string promptText;
        private string currentInput;
        private int finalScore;
        private double timeSpent;
        private int bulletsFired;
        private int bulletsUsedEnemies;
        private int bulletsUsedBosses;
        private int livesLost;
        private int currentScore;
        private int currentLevel;
        private int currentLives;
        private int deaths;
        private GameData gameData;

        private bool finished;
        public bool Finished => finished;

        private KeyboardState previousKBState;

        // Public property to expose the player's entered name.
        public string PlayerName => currentInput;

        public ScoreboardState(SpriteFont font, double timeSpent, int bulletsFired, int bulletsUsedEnemies, int bulletsUsedBosses,
                               int livesLost, int currentScore, int currentLevel, int currentLives, int deaths, GameData gameData)
        {
            this.font = font;
            this.timeSpent = timeSpent;
            this.bulletsFired = bulletsFired;
            this.bulletsUsedEnemies = bulletsUsedEnemies;
            this.bulletsUsedBosses = bulletsUsedBosses;
            this.livesLost = livesLost;
            this.currentScore = currentScore;
            this.currentLevel = currentLevel;
            this.currentLives = currentLives;
            this.deaths = deaths;
            this.gameData = gameData;

            finalScore = (bulletsUsedEnemies * 10) +
                         (bulletsUsedBosses * 20) -
                         (livesLost * 50) +
                         (int)(timeSpent / 5) +
                         currentScore;

            promptText = "Please enter your name and press Enter to exit:";
            currentInput = "";
            finished = false;

            previousKBState = Keyboard.GetState();
        }

        // Update method processes keyboard input for entering the player's name.
        public void Update(GameTime gameTime)
        {
            KeyboardState currentKB = Keyboard.GetState();
            foreach (Keys key in currentKB.GetPressedKeys())
            {
                // Process key only if it was not pressed in the previous state.
                if (!previousKBState.IsKeyDown(key))
                {
                    if (key >= Keys.A && key <= Keys.Z)
                    {
                        char c = (char)('A' + (key - Keys.A));
                        if (!currentKB.IsKeyDown(Keys.LeftShift) && !currentKB.IsKeyDown(Keys.RightShift))
                            c = char.ToLower(c);
                        currentInput += c;
                    }
                    else if (key >= Keys.D0 && key <= Keys.D9)
                    {
                        char c = (char)('0' + (key - Keys.D0));
                        currentInput += c;
                    }
                    else if (key == Keys.Space)
                    {
                        currentInput += " ";
                    }
                    else if (key == Keys.Back && currentInput.Length > 0)
                    {
                        currentInput = currentInput.Substring(0, currentInput.Length - 1);
                    }
                    else if (key == Keys.Enter)
                    {
                        if (!string.IsNullOrWhiteSpace(currentInput))
                        {
                            SaveScore();
                            finished = true;
                        }
                    }
                }
            }
            previousKBState = currentKB;
        }

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            string displayText = $"{promptText}\n{currentInput}\n\nFinal Score: {finalScore}\nTime Spent: {timeSpent:F2} sec\n\nPrevious Scores:\n";
            foreach (ScoreboardEntry entry in gameData.Scoreboard)
            {
                displayText += $"{entry.PlayerName}: {entry.FinalScore} (Level {entry.LevelReached}, Lives Lost: {entry.LivesLost}, Time: {entry.TimeSpentSeconds:F0} sec)\n";
            }
            spriteBatch.DrawString(font, displayText, new Vector2(50, 50), Color.White);
            spriteBatch.End();
        }

        private void SaveScore()
        {
            ScoreboardEntry entry = new ScoreboardEntry
            {
                PlayerName = currentInput,
                LevelReached = currentLevel,
                BulletsFired = bulletsFired,
                BulletsUsedAgainstEnemies = bulletsUsedEnemies,
                BulletsUsedAgainstBosses = bulletsUsedBosses,
                LivesLost = livesLost,
                TimeSpentSeconds = timeSpent,
                FinalScore = finalScore
            };

            gameData.CumulativeStats.TotalBulletsFired += bulletsFired;
            gameData.CumulativeStats.TotalPlayerDeaths += deaths;

            SessionData session = new SessionData
            {
                SessionDate = DateTime.Now,
                LevelReached = currentLevel,
                LivesRemaining = currentLives,
                Score = currentScore
            };

            gameData.Sessions.Add(session);
            gameData.Scoreboard.Add(entry);
            SaveLoadManager.SaveGameData(gameData);

            // Create and append a debug log entry using the entered player's name.
            DebugLogEntry logEntry = new DebugLogEntry
            {
                PlayerName = currentInput,
                DifficultyLevel = DifficultyManager.Instance.CurrentDifficulty,
                EnemiesKilled = Game1.Instance.CurrentMap != null ? Game1.Instance.CurrentMap.KillCount : 0,
                BossesKilled = Game1.Instance.GetBossCount(),
                TimeTakenSeconds = (DateTime.Now - Game1.Instance.SessionStartTime).TotalSeconds,
                TotalDamageTaken = Game1.Instance.PlayerStatsInstance.TotalDamageTaken,
                BulletsFired = Game1.Instance.bulletsFiredThisSession,
                BulletsUsedAgainstEnemies = Game1.Instance.bulletsUsedAgainstEnemiesThisSession,
                BulletsUsedAgainstBosses = Game1.Instance.bulletsUsedAgainstBossesThisSession,
                LivesLost = Game1.Instance.livesLostThisSession,
                Deaths = Game1.Instance.deathsThisSession,
                EnemyHealthMultiplier = DifficultyManager.Instance.EnemyHealthMultiplier,
                EnemySpeedMultiplier = DifficultyManager.Instance.EnemySpeedMultiplier,
                EnemyDamageMultiplier = DifficultyManager.Instance.EnemyDamageMultiplier,
                BossHealthMultiplier = DifficultyManager.Instance.BossHealthMultiplier,
                BossAttackSpeedMultiplier = DifficultyManager.Instance.BossAttackSpeedMultiplier,
                BossMovementSpeedMultiplier = DifficultyManager.Instance.BossMovementSpeedMultiplier,
                BossDamageMultiplier = DifficultyManager.Instance.BossDamageMultiplier,
                LogDate = DateTime.Now
            };

            DebugLogger.AppendLogEntry(logEntry);
        }
    }
}
