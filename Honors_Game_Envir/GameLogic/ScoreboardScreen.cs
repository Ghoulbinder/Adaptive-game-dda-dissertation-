using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Survivor_of_the_Bulge
{
    public class ScoreboardScreen
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

        public ScoreboardScreen(SpriteFont font, double timeSpent, int bulletsFired, int bulletsUsedEnemies, int bulletsUsedBosses, int livesLost, int currentScore, int currentLevel, int currentLives, int deaths, GameData gameData)
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
        }

        public void Update(GameTime gameTime)
        {
            KeyboardState keyboard = Keyboard.GetState();
            foreach (Keys key in keyboard.GetPressedKeys())
            {
                if (key >= Keys.A && key <= Keys.Z)
                {
                    char c = (char)('A' + (key - Keys.A));
                    if (!keyboard.IsKeyDown(Keys.LeftShift) && !keyboard.IsKeyDown(Keys.RightShift))
                        c = char.ToLower(c);
                    if (!currentInput.EndsWith(c.ToString()))
                        currentInput += c;
                }
                else if (key >= Keys.D0 && key <= Keys.D9)
                {
                    char c = (char)('0' + (key - Keys.D0));
                    if (!currentInput.EndsWith(c.ToString()))
                        currentInput += c;
                }
                else if (key == Keys.Space)
                {
                    if (!currentInput.EndsWith(" "))
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

        public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            string displayText = $"{promptText}\n{currentInput}\n\nFinal Score: {finalScore}\nTime Spent: {timeSpent:F2} sec\n\nPrevious Scores:\n";
            foreach (ScoreboardEntry entry in gameData.Scoreboard)
            {
                displayText += $"{entry.PlayerName}: {entry.FinalScore} (Level {entry.PlayerName}, Lives Lost: {entry.LivesLost}, Time: {entry.TimeSpentSeconds:F0} sec)\n";
                // (Adjust formatting as desired—here we use entry.PlayerName twice accidentally; fix next)
                // Corrected:
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
                BulletsFired = bulletsFired,
                BulletsUsedAgainstEnemies = bulletsUsedEnemies,
                BulletsUsedAgainstBosses = bulletsUsedBosses,
                LivesLost = livesLost,
                TimeSpentSeconds = timeSpent,
                FinalScore = finalScore,
                // Assuming SessionData doesn't include these details in ScoreboardEntry,
                // you may add more properties if needed.
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
        }
    }
}
