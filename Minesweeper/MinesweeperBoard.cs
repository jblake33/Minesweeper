using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper
{
    public class GameOverException : Exception
    {
        public bool Win;
        public GameOverException(bool win = false) : base() { Win = win; }
        public GameOverException(string message, bool win = false) : base() { Win = win; }
    }
    public class AlreadyGuessedException : Exception
    {
        public AlreadyGuessedException() { }
        public AlreadyGuessedException(string message) : base(message) { }
    }
    public class RNG
    {
        static Random _rng = new Random();
        /// <summary>
        /// Returns random float value from 0.0 to 1.0
        /// </summary>
        /// <returns></returns>
        public static float RandFloat()
        {
            return float.Parse("" + Math.Round(_rng.NextDouble(), 3));
        }
    }
    public enum MinesweeperDifficulty
    {
        EASY = 1,
        MEDIUM = 2,
        HARD = 3
    }
    public class Utils
    {
        public static string StringParse(MinesweeperDifficulty s)
        {
            foreach (var b in Enum.GetValues(typeof(MinesweeperDifficulty)))
            {
                if ((int)s == (int)b)
                {
                    return Enum.GetName(typeof(MinesweeperDifficulty), b);
                }
            }
            return "";
        }
    }
    public class MinesweeperCell
    {
        private bool _isMine;
        private bool _isFlagged;
        private bool _isRevealed;
        private int _neighborMines;

        public MinesweeperCell(bool isMine)
        {
            _isRevealed = false;
            _isMine = isMine;
        }
        public bool IsMine => _isMine;
        public bool IsFlagged => _isFlagged;
        public bool IsRevealed => _isRevealed;
        public int NeighborMines => _neighborMines;
        public void Reveal()
        {
            _isRevealed = true;
        }
        public void SetNeighborMines(int n)
        {
            if (0 <= n && n <= 8)
            {
                _neighborMines = n;
            }
        }
    }
    public class MinesweeperBoard
    {
        private static int MIN_SIZE = 4;
        public MinesweeperCell[,] Cells;

        private int _cellsRemaining;
        private int _mineCount;
        private bool _isGameOver;
        public bool IsGameOver => _isGameOver;
        public int MineCount => _mineCount;
        private MinesweeperDifficulty _diff;
        public MinesweeperDifficulty Difficulty => _diff;

        public MinesweeperBoard(int boardRows, int boardCols, MinesweeperDifficulty difficulty)
        {
            if (boardRows < MIN_SIZE || boardCols < MIN_SIZE)
            {
                throw new ArgumentOutOfRangeException($"Error creating MinesweeperBoard:\nNumber of rows {boardRows} and number of columns {boardCols} must both be at least {MIN_SIZE}.");
            }

            float mineChance = 0.0f;
            switch (difficulty)
            {
                case MinesweeperDifficulty.EASY:
                    mineChance = 0.1f;
                    break;
                case MinesweeperDifficulty.MEDIUM:
                    mineChance = 0.2f;
                    break;
                case MinesweeperDifficulty.HARD:
                    mineChance = 0.3f;
                    break;
            }

            _diff = difficulty;

            // Randomly place mines on the board.
            int mineCount = 0;
            // Loop ensures that a board with at least one mine is generated.
            while (mineCount == 0)
            {
                Cells = new MinesweeperCell[boardRows, boardCols];
                for (int i = 0; i < boardRows; i++)
                {
                    for (int j = 0; j < boardCols; j++)
                    {
                        if (RNG.RandFloat() < mineChance)
                        {
                            Cells[i, j] = new MinesweeperCell(true);
                            mineCount++;
                        }
                        else
                        {
                            Cells[i, j] = new MinesweeperCell(false);
                        }
                    }
                }
            }
            
            _mineCount = mineCount;
            _cellsRemaining = boardRows * boardCols - mineCount; // The number of non-mine cells.
            _isGameOver = false;

            // Assign number of mines around each cell to each cell's data
            for (int i = 0; i < boardRows; i++)
            {
                for (int j = 0; j < boardCols; j++)
                {
                    Cells[i, j].SetNeighborMines(GetMinesAroundCell(i, j));
                }
            }
        }
        public void Draw(bool debug = false)
        {
            if (debug)
            {
                Console.WriteLine(DebugToString());
            }
        }
        public string DebugToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Cells.GetLength(0); i++)
            {
                for (int c = 0; c < Cells.GetLength(1); c++)
                {
                    if (Cells[i, c].IsRevealed)
                    {
                        if (Cells[i, c].IsMine)
                        {
                            sb.Append("F");
                        }
                        else
                        {
                            sb.Append(Cells[i, c].NeighborMines + "");
                        }
                    }
                    else sb.Append("?");
                }
                sb.Append('\n');
            }
            return sb.ToString();
        }
        private int GetMinesAroundCell(int row, int col)
        {
            int mines = 0;
            for (int n = row - 1; n <= row + 1; n++)
            {
                /*  X--     Checks left of cell
                 *  X--
                 *  X--
                 */ 
                try
                {
                    if (Cells[n, col - 1].IsMine)
                    {
                        mines++;
                    }
                }
                catch (Exception) { }
                /*  --X     Checks right of cell
                 *  --X
                 *  --X
                 */
                try
                {
                    if (Cells[n, col + 1].IsMine)
                    {
                        mines++;
                    }
                }
                catch (Exception) { }
            }
            for (int n = row - 1; n <= row + 1; n += 2)
            {
                /*  -X-     Checks below and above cell
                 *  ---
                 *  -X-
                 */
                try
                {
                    if (Cells[n, col].IsMine)
                    {
                        mines++;
                    }
                }
                catch (Exception) { }
            }
            return mines;
        }
        /// <summary>
        /// Returns true if cell is not a mine, false if mine (game over).
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public bool GuessCell(int row, int col)
        {
            if (Cells[row, col].IsRevealed)
            {
                throw new AlreadyGuessedException("That cell is already revealed.");
            }
            if (Cells[row, col].IsMine)
            {
                // Game over!
                this.RevealAll();
                _isGameOver = true;
                throw new GameOverException(win: false);
            }
            this.RecursiveReveal(row, col, Cells[row, col].NeighborMines);
            CheckForWin();
            return true;
        }
        public void CheckForWin()
        {
            if (_cellsRemaining == 0)
            {
                this.RevealAll();
                _isGameOver = true;
                throw new GameOverException(win: true);
            }
        }
        public void RecursiveReveal(int r, int c, int mineCount)
        {
            if (Cells[r, c].IsMine) return;
            if (!Cells[r, c].IsRevealed)
            {
                Cells[r, c].Reveal();
                _cellsRemaining--;
            }
            
            // Left
            try
            {
                if (!Cells[r, c - 1].IsRevealed && Cells[r, c - 1].NeighborMines == mineCount)
                {
                    RecursiveReveal(r, c - 1, mineCount);
                }
            }
            catch { }

            // Up
            try
            {
                if (!Cells[r - 1, c].IsRevealed && Cells[r - 1, c].NeighborMines == mineCount)
                {
                    RecursiveReveal(r - 1, c, mineCount);
                }
            }
            catch { }

            // Down
            try
            {
                if (!Cells[r + 1, c].IsRevealed && Cells[r + 1, c].NeighborMines == mineCount)
                {
                    RecursiveReveal(r + 1, c, mineCount);
                }
            }
            catch { }

            // Right
            try
            {
                if (!Cells[r, c + 1].IsRevealed && Cells[r, c + 1].NeighborMines == mineCount)
                {
                    RecursiveReveal(r, c + 1, mineCount);
                }
            }
            catch { }
        }
        public void RevealAll()
        {
            foreach (MinesweeperCell cell in Cells)
            {
                cell.Reveal();
            }
        }
    }
}
