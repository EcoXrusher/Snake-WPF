using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Snake
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Snake Public Data
        bool IsGameInProgress = false;

        public const int SnakeSquareSize = 20;

        private SolidColorBrush snakeBodyBrush = Brushes.Green;
        private SolidColorBrush snakeHeadBrush = Brushes.YellowGreen;
        private List<SnakePart> snakeParts = new List<SnakePart>();
        public enum SnakeDirection { Left, Right, Up, Down };
        private SnakeDirection snakeDirection = SnakeDirection.Right;
        

        const int SnakeStartLength = 3;
        const int SnakeStartSpeed = 400;
        const int SnakeSpeedThreshold = 100;

        private int snakeLength;
        private int currentScore = 0;

        //food elements
        private UIElement snakeFood = null;
        private SolidColorBrush foodBrush = Brushes.Red;

        private System.Windows.Threading.DispatcherTimer gameTickTimer = new System.Windows.Threading.DispatcherTimer();
        private Random rnd=new Random();
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            gameTickTimer.Tick += GameTickTimer_Tick;
            
        }

        
        #endregion

        /// <summary>
        /// Draws Blank Playground
        /// </summary>
        private void DrawPlayground()
        {
            int NextX = 0, NextY = 0;
            int NextRow = 0;
            bool DoneDrawing = false;
            bool nextIsOdd = false;

            while (!DoneDrawing)
            {
                Rectangle rect = new Rectangle
                {
                    Width = SnakeSquareSize,
                    Height = SnakeSquareSize,
                    Fill = nextIsOdd ? Brushes.White : Brushes.Black
                };
                Playground.Children.Add(rect);
                Canvas.SetLeft(rect, NextX);
                Canvas.SetTop(rect, NextY);

                nextIsOdd ^= true;
                NextX += SnakeSquareSize;

                if (NextX >= Playground.ActualWidth)
                {
                    NextX = 0;
                    NextRow++;
                    NextY += SnakeSquareSize;
                    nextIsOdd = (NextRow % 2 != 0);
                }

                if(NextY >= Playground.ActualHeight)
                {
                    DoneDrawing = true;
                }

            }
        }
        /// <summary>
        /// Draws snake on the screen
        /// </summary>
        private void DrawSnake()
        {
            foreach(SnakePart snakePart in snakeParts)
            {
                if (snakePart.UiElement == null)
                {
                    snakePart.UiElement = new Rectangle() {
                        Width = SnakeSquareSize,
                        Height=SnakeSquareSize,
                        Fill=(snakePart.IsHead ? snakeHeadBrush : snakeBodyBrush)
                    };
                    Playground.Children.Add(snakePart.UiElement);
                    Canvas.SetLeft(snakePart.UiElement, snakePart.Position.X);
                    Canvas.SetTop(snakePart.UiElement, snakePart.Position.Y);
                }
            }
        }
        /// <summary>
        /// Allows user to move snake
        /// </summary>
        private void MoveSnake()
        {
            while (snakeParts.Count >= snakeLength)
            {
                Playground.Children.Remove(snakeParts[0].UiElement);
                snakeParts.RemoveAt(0);
            }
            foreach(SnakePart snakePart in snakeParts)
            {
                (snakePart.UiElement as Rectangle).Fill = snakeBodyBrush;
                snakePart.IsHead = false;
            }

            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;

            switch (snakeDirection)
            {
                case SnakeDirection.Left:
                    nextX -= SnakeSquareSize;
                    break;
                case SnakeDirection.Right:
                    nextX += SnakeSquareSize;
                    break;
                case SnakeDirection.Up:
                    nextY -= SnakeSquareSize;
                    break;
                case SnakeDirection.Down:
                    nextY += SnakeSquareSize;
                    break;
            }
            snakeParts.Add(new SnakePart()
            {
                Position = new Point(nextX, nextY),
                IsHead = true
            });

            DrawSnake();
            CheckColision();
        }

        /// <summary>
        /// Keeps snake moving
        /// </summary>
        /// <param name="sender">Tick Timer</param>
        /// <param name="e"> Time pases </param>
        private void GameTickTimer_Tick(object sender, EventArgs e)
        {
            MoveSnake();
        }
        /// <summary>
        /// Starts new game with Start values
        /// </summary>
        private void StartNewGame()
        {
            foreach (SnakePart snakeBodyPart in snakeParts)
            {
                if (snakeBodyPart.UiElement != null)
                    Playground.Children.Remove(snakeBodyPart.UiElement);
            }
            snakeParts.Clear();
            if (snakeFood != null)
                Playground.Children.Remove(snakeFood);

            // Reset stuff
            currentScore = 0;
            snakeLength = SnakeStartLength;
            snakeDirection = SnakeDirection.Right;
            snakeParts.Add(new SnakePart() { Position = new Point(SnakeSquareSize * 5, SnakeSquareSize * 5) });
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(SnakeStartSpeed);

            // Draw the snake again and some new food...
            DrawSnake();
            DrawFood();

            // Update status
            UpdateGameStatus();

            // Go!        
            gameTickTimer.IsEnabled = true;
        }
        #region food elements handling
        /// <summary>
        /// Generates food for snake not in snake current position
        /// </summary>
        /// <returns>new food position</returns>
        private Point Food()
        {
            int maxX = (int)(Playground.ActualWidth / SnakeSquareSize);
            int maxY = (int)(Playground.ActualHeight / SnakeSquareSize);
            int foodX = rnd.Next(0, maxX) * SnakeSquareSize;
            int foodY = rnd.Next(0, maxY) * SnakeSquareSize;

            foreach(SnakePart snakePart in snakeParts)
            {
                if(snakePart.Position.X==foodX && snakePart.Position.Y == foodY)
                {
                    return Food();
                }           
            }
            return new Point(foodX, foodY);
        }
        
        /// <summary>
        /// Draws food on current food position
        /// </summary>
        private void DrawFood()
        {
            Point foodposition = Food();
            snakeFood = new Ellipse() {
            Width=SnakeSquareSize,
            Height=SnakeSquareSize,
            Fill=foodBrush
            };
            Playground.Children.Add(snakeFood);
            Canvas.SetLeft(snakeFood, foodposition.X);
            Canvas.SetTop(snakeFood, foodposition.Y);
        }

        #endregion

        #region Gameplay Order
        /// <summary>
        /// Event starting while Window rendered
        /// </summary>
        /// <param name="sender"> Window rendered</param>
        /// <param name="e">Window rendered</param>
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            
            DrawPlayground();
            
            
        }
        #endregion

        /// <summary>
        /// Changes the snake direction
        /// </summary>
        /// <param name="sender">User Presses a key</param>
        /// <param name="e"> key which is pressed</param>
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            SnakeDirection originalSnakeDirection = snakeDirection;
            switch (e.Key)
            {
                case Key.Up:
                    if (snakeDirection != SnakeDirection.Up && snakeDirection!=SnakeDirection.Down)
                    {
                        snakeDirection = SnakeDirection.Up;
                    }
                    break;
                case Key.Down:
                    if (snakeDirection != SnakeDirection.Down && snakeDirection != SnakeDirection.Up)
                    {
                        snakeDirection = SnakeDirection.Down;
                    }
                    break;
                case Key.Left:
                    if (snakeDirection != SnakeDirection.Left && snakeDirection != SnakeDirection.Right)
                    {
                        snakeDirection = SnakeDirection.Left;
                    }
                    break;
                case Key.Right:
                    if (snakeDirection != SnakeDirection.Right && snakeDirection != SnakeDirection.Left)
                    {
                        snakeDirection = SnakeDirection.Right;
                    }
                    break;
                case Key.Space:
                    if (!IsGameInProgress)
                    {
                        IsGameInProgress = true;
                        StartNewGame();
                    }
                    break;
            }
            if (snakeDirection != originalSnakeDirection)
            {
                MoveSnake();
            }
        }

        private void CheckColision()
        {
            SnakePart snakeHead = snakeParts[snakeParts.Count - 1];
            if((snakeHead.Position.X==Canvas.GetLeft(snakeFood))&&( snakeHead.Position.Y == Canvas.GetTop(snakeFood))){
                SnakeEatsFood();
                return;
            }
            if(snakeHead.Position.Y<0 || snakeHead.Position.Y > Playground.ActualHeight ||snakeHead.Position.X<0 || snakeHead.Position.X>Playground.ActualWidth)
            {
                EndGame();
            }
            foreach(SnakePart snakePart in snakeParts.Take(snakeParts.Count-1))
            {
                if((snakeHead.Position.X==snakePart.Position.X)&& (snakeHead.Position.Y == snakePart.Position.Y)) { EndGame(); }
            }
        }
        private void SnakeEatsFood()
        {
            snakeLength++;
            currentScore++;
            int timeInterval = Math.Max(SnakeSpeedThreshold, (int)gameTickTimer.Interval.TotalMilliseconds - (currentScore * 2));
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(timeInterval);
            Playground.Children.Remove(snakeFood);
            DrawFood();
            UpdateGameStatus();
        }

        /// <summary>
        /// Updfates window title to show scores and speed
        /// </summary>
        private void UpdateGameStatus()
        {
            this.Title= "Snake | Score : " + currentScore + "  Game speed : " + gameTickTimer.Interval.TotalMilliseconds;
        }

        private void EndGame()
        {
            gameTickTimer.IsEnabled = false;
            IsGameInProgress = false;
            MessageBox.Show($"Oops, You Died!\nYour Score was {currentScore}\nPress spacebar to play again");
        }


    }
}
