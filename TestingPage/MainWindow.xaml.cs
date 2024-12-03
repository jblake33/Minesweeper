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



namespace TestingPage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Minesweeper.MinesweeperBoard d;
        public MainWindow()
        {
            InitializeComponent();
            d = new Minesweeper.MinesweeperBoard(5, 5, Minesweeper.MinesweeperDifficulty.EASY);
            Draw();
            
        }
        public void Cell_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string cellname = button.Name;
                cellname = cellname.Substring(1);
                var p = cellname.Split('x');
                int r = int.Parse(p[0]);
                int c = int.Parse(p[1]);
                d.GuessCell(r, c);

                Draw(); // Expensive, yes i know
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
        public void Draw()
        {
            Box.Children.Clear();
            StackPanel rows = new StackPanel();
            rows.Name = "Rows";
            rows.Orientation = Orientation.Vertical;
            for (int i = 0; i < 5; i++)
            {
                // Make a stackpanel for each row
                StackPanel curr_row = new StackPanel();
                curr_row.Orientation = Orientation.Horizontal;
                curr_row.Name = "R" + i.ToString();

                for (int j = 0; j < 5; j++)
                {
                    Button btn = new Button();
                    BitmapImage tmp_bimg = new BitmapImage();
                    tmp_bimg.BeginInit();
                    tmp_bimg.UriSource = GetSrc(d.Cells[i, j]);
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
            Box.Children.Add(rows);
        }
    }
}