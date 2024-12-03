using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public const string HAPPY = "pack://application:,,,/Minesweeper;component/Resources/happy.png";
        public const string DEAD = "pack://application:,,,/Minesweeper;component/Resources/dead.png";
        public const string WINNER = "pack://application:,,,/Minesweeper;component/Resources/winner.png";
        internal MinesweeperBoard board;
        public MainWindow()
        {
            InitializeComponent();
        }

        public void RefreshBoard()
        {
            string b = board.DebugToString();
            //BoardLabel.Content = b;
            Draw();
        }

        public void Draw()
        {
            Board.Children.Clear();
            StackPanel rows = new StackPanel();
            int boardRows = board.Cells.GetLength(0), boardCols = board.Cells.GetLength(1);
            rows.Name = "Rows";
            rows.Orientation = Orientation.Vertical;
            for (int i = 0; i < boardRows; i++)
            {
                // Make a stackpanel for each row
                StackPanel curr_row = new StackPanel();
                curr_row.Orientation = Orientation.Horizontal;
                curr_row.Name = "R" + i.ToString();

                for (int j = 0; j < boardCols; j++)
                {
                    Button btn = new Button();
                    BitmapImage tmp_bimg = new BitmapImage();
                    tmp_bimg.BeginInit();
                    tmp_bimg.UriSource = GetSrc(board.Cells[i, j]);
                    tmp_bimg.EndInit();
                    Image tmp_img = new Image();
                    tmp_img.Source = tmp_bimg;
                    btn.Content = tmp_img;
                    btn.Name = $"x{i}x{j}";
                    btn.Click += Cell_Click;
                    curr_row.Children.Add(btn);
                }
                rows.Children.Add(curr_row);
            }
            Board.Children.Add(rows);
        }

        public void Cell_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button)
                {
                    string cellname = button.Name;
                    cellname = cellname.Substring(1);
                    var p = cellname.Split('x');
                    int r = int.Parse(p[0]);
                    int c = int.Parse(p[1]);

                    board.GuessCell(r, c);

                    Draw();
                }
            }
            catch (GameOverException goe)
            {
                board.RevealAll();
                Draw();
                if (goe.Win)
                {
                    SetStatus(PlayerStatus.Winner);
                    StatusLabel.Content = "You won!";
                }
                else
                {
                    SetStatus(PlayerStatus.Dead);
                    StatusLabel.Content = "You exploded! Sorry.";
                }
            }
            catch (AlreadyGuessedException) { }
            catch (Exception c)
            {
                MessageBox.Show(c.Message);
            }
        }
        public Uri GetSrc(Minesweeper.MinesweeperCell c)
        {
            if (!c.IsRevealed)
            {
                return new Uri("pack://application:,,,/Resources/hidden.png");
            }
            if (c.IsMine)
            {
                return new Uri("pack://application:,,,/Resources/mine.png");
            }
            switch (c.NeighborMines)
            {
                case 0:
                    return new Uri("pack://application:,,,/Resources/0.png");
                case 1:
                    return new Uri("pack://application:,,,/Resources/1.png");
                case 2:
                    return new Uri("pack://application:,,,/Resources/2.png");
                case 3:
                    return new Uri("pack://application:,,,/Resources/3.png");
                case 4:
                    return new Uri("pack://application:,,,/Resources/4.png");
                case 5:
                    return new Uri("pack://application:,,,/Resources/5.png");
                case 6:
                    return new Uri("pack://application:,,,/Resources/6.png");
                case 7:
                    return new Uri("pack://application:,,,/Resources/7.png");
                case 8:
                    return new Uri("pack://application:,,,/Resources/8.png");
            }


            // Should not happen
            throw new Exception("Image to produce for cell undetermined");
        }
        public enum PlayerStatus
        {
            Alive = 0,
            Dead = 1,
            Winner = 2
        }
        public void SetStatus(PlayerStatus status)
        {
            switch (status)
            {
                case PlayerStatus.Alive:
                    AliveImg.Visibility = Visibility.Visible;
                    DeadImg.Visibility = Visibility.Collapsed;
                    WinnerImg.Visibility = Visibility.Collapsed;
                    break;
                case PlayerStatus.Dead:
                    AliveImg.Visibility = Visibility.Collapsed;
                    DeadImg.Visibility = Visibility.Visible;
                    WinnerImg.Visibility = Visibility.Collapsed;
                    break;
                case PlayerStatus.Winner:
                    AliveImg.Visibility = Visibility.Collapsed;
                    DeadImg.Visibility = Visibility.Collapsed;
                    WinnerImg.Visibility = Visibility.Visible;
                    break;
            }
        }

        /// <summary>
        /// Opens New Game Selection Box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            if (NewGameBox.Visibility == Visibility.Visible)
                NewGameBox.Visibility = Visibility.Collapsed;
            else
                NewGameBox.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// User submits choices for starting new game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewGameSubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            // Get difficulty
            var choice = DifficultyRadioBtns.Children.OfType<RadioButton>().FirstOrDefault(r => r.IsChecked.HasValue && r.IsChecked.Value);
            if (choice == null)
            {
                MessageBox.Show("You must select a difficulty.");
                return;
            }
            MinesweeperDifficulty d;
            switch (choice.Content.ToString())
            {
                case "Easy":
                    d = MinesweeperDifficulty.EASY;
                    break;
                case "Medium":
                    d = MinesweeperDifficulty.MEDIUM;
                    break;
                case "Hard":
                    d = MinesweeperDifficulty.HARD;
                    break;
                default:
                    MessageBox.Show($"Unknown difficulty selection: {choice.Content.ToString()}");
                    return;
            }

            // Get Board Size
            string rowText = NewBoardRowInput.Text;
            string colText = NewBoardColumnInput.Text;
            if (!int.TryParse(rowText, out int rows))
            {
                MessageBox.Show($"Invalid row amount: \"{rowText}\"\nMust be a positive number.");
                return;
            }
            if (!int.TryParse(colText, out int cols))
            {
                MessageBox.Show($"Invalid column amount: \"{colText}\"\nMust be a positive number.");
                return;
            }
            

            try
            {
                board = new MinesweeperBoard(rows, cols, d);
                // Successfully creating board should close the new game box
                NewGameBox.Visibility = Visibility.Collapsed;
                MinesLabel.Content = $"Mines: {board.MineCount}";
                DifficultyLabel.Content = $"Difficulty: {Minesweeper.Utils.StringParse(board.Difficulty)}";
                SetStatus(PlayerStatus.Alive);
                StatusLabel.Content = "";
                RefreshBoard();
            }
            catch (Exception g)
            {
                MessageBox.Show(g.Message);
            }
        }

        /// <summary>
        /// Debug control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /*
        private void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            if (board is null) return;
            if (board.IsGameOver) return;

            string rowText = RowField.Text;
            string colText = ColumnField.Text;
            if (!int.TryParse(rowText, out int row))
            {
                MessageBox.Show($"Invalid row amount: \"{rowText}\"\nMust be a positive number.");
                return;
            }
            if (!int.TryParse(colText, out int col))
            {
                MessageBox.Show($"Invalid column amount: \"{colText}\"\nMust be a positive number.");
                return;
            }

            try
            {
                board.GuessCell(row, col);
                // A safe cell was guessed if no exception thrown

            }
            catch (Minesweeper.GameOverException g)
            {
                if (g.Win)
                {
                    SetStatus(PlayerStatus.Winner);
                    MessageBox.Show("You Won!");
                }
                else
                {
                    SetStatus(PlayerStatus.Dead);
                    MessageBox.Show("You Lost!");
                }
            }
            catch (Exception g)
            {
                MessageBox.Show(g.Message);
            }
            RefreshBoard();
        }
        //*/
    }
}