using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SSSnake
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 

    using System.Windows.Threading;

    public partial class MainWindow : Window
    {
        // Этот список описывает Бонусные красные куски еды на объекте Canvas
        private List<Point> bonusPoints = new List<Point>();

        // Этот список описывает тело змеи на объекте Canvas
        private List<Point> snakePoints = new List<Point>();



        private Brush snakeColor = Brushes.Green;
        private enum SIZE
        {
            THIN = 4,
            NORMAL = 6,
            THICK = 8
        };

        private enum MOVINGDIRECTION
        {
            UPWARDS = 8,
            DOWNWARDS = 2,
            TOLEFT = 4,
            TORIGHT = 6
        };

        private TimeSpan FAST = new TimeSpan(1);
        private TimeSpan MODERATE = new TimeSpan(10000);
        private TimeSpan SLOW = new TimeSpan(50000);
        private TimeSpan DAMNSLOW = new TimeSpan(500000);



        private Point startingPoint = new Point(100, 100);
        private Point currentPosition = new Point();

        // Инициализация направления движения
        private int direction = 0;

        /* Здесь запоминается предыдущее направление движения
         * Это необходимо, чтобы змея не могла передвигаться по собственному телу.*/
        private int previousDirection = 0;

        /* Здесь можно изменить размер змеи.
         * Возможные размеры THIN, NORMAL и THICK */
        private int headSize = (int)SIZE.THICK;



        private int length = 100;
        private int score = 0;
        private Random rnd = new Random();



        public MainWindow()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);

            /* Здесь можно изменить скорость змеи.
             * Возможные скорости FAST, MODERATE, SLOW и DAMNSLOW */
            timer.Interval = MODERATE;
            timer.Start();


            this.KeyDown += new KeyEventHandler(OnButtonKeyDown);
            paintSnake(startingPoint);
            currentPosition = startingPoint;

            // Отрисовка объектов еды
            for (int n = 0; n < 10; n++)
            {
                paintBonus(n);
            }
        }



        private void paintSnake(Point currentposition)
        {

            /* Метод используется для отрисовки кадров тела змеи
             * каждый раз при вызове данного метода. */


            Ellipse newEllipse = new Ellipse();
            newEllipse.Fill = snakeColor;
            newEllipse.Width = headSize;
            newEllipse.Height = headSize;

            Canvas.SetTop(newEllipse, currentposition.Y);
            Canvas.SetLeft(newEllipse, currentposition.X);

            int count = paintCanvas.Children.Count;
            paintCanvas.Children.Add(newEllipse);
            snakePoints.Add(currentposition);


            // Стирает хвост змеи, создавая иллюзию движения
            if (count > length)
            {
                paintCanvas.Children.RemoveAt(count - length + 9);
                snakePoints.RemoveAt(count - length);
            }
        }


        private void paintBonus(int index)
        {
            Point bonusPoint = new Point(rnd.Next(5, 620), rnd.Next(5, 380));



            Ellipse newEllipse = new Ellipse();
            newEllipse.Fill = Brushes.Red;
            newEllipse.Width = headSize;
            newEllipse.Height = headSize;

            Canvas.SetTop(newEllipse, bonusPoint.Y);
            Canvas.SetLeft(newEllipse, bonusPoint.X);
            paintCanvas.Children.Insert(index, newEllipse);
            bonusPoints.Insert(index, bonusPoint);

        }


        private void timer_Tick(object sender, EventArgs e)
        {
            // Expand the body of the snake to the direction of movement

            switch (direction)
            {
                case (int)MOVINGDIRECTION.DOWNWARDS:
                    currentPosition.Y += 1;
                    paintSnake(currentPosition);
                    break;
                case (int)MOVINGDIRECTION.UPWARDS:
                    currentPosition.Y -= 1;
                    paintSnake(currentPosition);
                    break;
                case (int)MOVINGDIRECTION.TOLEFT:
                    currentPosition.X -= 1;
                    paintSnake(currentPosition);
                    break;
                case (int)MOVINGDIRECTION.TORIGHT:
                    currentPosition.X += 1;
                    paintSnake(currentPosition);
                    break;
            }

            // Restrict to boundaries of the Canvas
            if ((currentPosition.X < 5) || (currentPosition.X > 620) ||
                (currentPosition.Y < 5) || (currentPosition.Y > 380))
                GameOver();

            // Hitting a bonus Point causes the lengthen-Snake Effect
            int n = 0;
            foreach (Point point in bonusPoints)
            {

                if ((Math.Abs(point.X - currentPosition.X) < headSize) &&
                    (Math.Abs(point.Y - currentPosition.Y) < headSize))
                {
                    length += 10;
                    score += 10;

                    // In the case of food consumption, erase the food object
                    // from the list of bonuses as well as from the canvas
                    bonusPoints.RemoveAt(n);
                    paintCanvas.Children.RemoveAt(n);
                    paintBonus(n);
                    break;
                }
                n++;
            }

            // Restrict hits to body of Snake


            for (int q = 0; q < (snakePoints.Count - headSize * 2); q++)
            {
                Point point = new Point(snakePoints[q].X, snakePoints[q].Y);
                if ((Math.Abs(point.X - currentPosition.X) < (headSize)) &&
                     (Math.Abs(point.Y - currentPosition.Y) < (headSize)))
                {
                    GameOver();
                    break;
                }

            }




        }



        private void OnButtonKeyDown(object sender, KeyEventArgs e)
        {



            switch (e.Key)
            {
                case Key.Down:
                    if (previousDirection != (int)MOVINGDIRECTION.UPWARDS)
                        direction = (int)MOVINGDIRECTION.DOWNWARDS;
                    break;
                case Key.Up:
                    if (previousDirection != (int)MOVINGDIRECTION.DOWNWARDS)
                        direction = (int)MOVINGDIRECTION.UPWARDS;
                    break;
                case Key.Left:
                    if (previousDirection != (int)MOVINGDIRECTION.TORIGHT)
                        direction = (int)MOVINGDIRECTION.TOLEFT;
                    break;
                case Key.Right:
                    if (previousDirection != (int)MOVINGDIRECTION.TOLEFT)
                        direction = (int)MOVINGDIRECTION.TORIGHT;
                    break;

            }
            previousDirection = direction;

        }



        private void GameOver()
        {
            MessageBox.Show("You Lose! Your score is " + score.ToString(), "Game Over", MessageBoxButton.OK, MessageBoxImage.Hand);
            this.Close();
        }
    }
}
