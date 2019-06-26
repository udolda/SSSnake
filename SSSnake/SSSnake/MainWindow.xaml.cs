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
using System.Speech.Synthesis;

namespace SSSnake
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 

    using System.Windows.Threading;

    public partial class MainWindow : Window
    {
        ///<remarks>Этот список описывает Бонусные красные куски еды на объекте Canvas</remarks>
        private List<Point> bonusPoints = new List<Point>();

        ///<remarks>Этот список описывает тело змеи на объекте Canvas</remarks>
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

        /*
        private TimeSpan FAST = new TimeSpan(1000);
        private TimeSpan MODERATE = new TimeSpan(10000);
        private TimeSpan SLOW = new TimeSpan(50000);
        private TimeSpan DAMNSLOW = new TimeSpan(100000);
        */
        protected int intSnakeSpeed = 300000;
        protected TimeSpan SnakeSpeed = new TimeSpan(300000);
        protected uint tbSpeed = 0;

        private Point startingPoint = new Point(100, 100);
        private Point currentPosition;

        ///<remarks>Инициализация направления движения</remarks>
        private int direction = 0;

        /* Здесь запоминается предыдущее направление движения
         * Это необходимо, чтобы змея не могла передвигаться по собственному телу.*/
        private int previousDirection = 0;

        /* Здесь можно изменить размер змеи.
         * Возможные размеры THIN, NORMAL и THICK */
        private int headSize = (int)SIZE.THIN;


        private int length = 100;
        private int score = 0;
        private Random rnd = new Random();

        DispatcherTimer timer = new DispatcherTimer();

        private SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();

        private bool isGameOver = false;

        public MainWindow()
        {
            InitializeComponent();
            bdrEndOfGame.Visibility = Visibility.Hidden;

            timer.Tick += new EventHandler(timer_Tick);

            btnPause.Click += new RoutedEventHandler(BtnPause_Click);
            btnContinue.Click += new RoutedEventHandler(BtnContinue_Click);

            this.KeyDown += new KeyEventHandler(OnButtonKeyDown);
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

            int count = GameArea.Children.Count;
            GameArea.Children.Add(newEllipse);
            snakePoints.Add(currentposition);


            ///<remarks>Стирает хвост змеи, создавая иллюзию движения.</remarks>
            if (count > length)
            {
                GameArea.Children.RemoveAt(count - length + 9);
                snakePoints.RemoveAt(count - length);
            }
        }

        private void paintBonus(int index)
        {
            Point bonusPoint = new Point(rnd.Next(5, 610), rnd.Next(5, 410));

            Ellipse newEllipse = new Ellipse();
            newEllipse.Fill = Brushes.Red;
            newEllipse.Width = headSize;
            newEllipse.Height = headSize;

            Canvas.SetTop(newEllipse, bonusPoint.Y);
            Canvas.SetLeft(newEllipse, bonusPoint.X);
            GameArea.Children.Insert(index, newEllipse);
            bonusPoints.Insert(index, bonusPoint);

        }

        private void timer_Tick(object sender, EventArgs e)
        {
            ///<remarks>Разворачиваем тело змеи по направлению движения.</remarks>
            switch (direction)
            {
                case (int)MOVINGDIRECTION.DOWNWARDS:
                    {
                        currentPosition.Y = (currentPosition.Y + 1) % 423;
                        paintSnake(currentPosition);
                    }
                    break;

                case (int)MOVINGDIRECTION.UPWARDS:
                    {
                        currentPosition.Y = Math.Abs((currentPosition.Y - 1) * Math.Pow((currentPosition.Y + 422), (1 - Math.Sign(currentPosition.Y))));
                        paintSnake(currentPosition);
                    }
                    break;

                case (int)MOVINGDIRECTION.TOLEFT:
                    {
                        currentPosition.X = Math.Abs((currentPosition.X - 1)*Math.Pow((currentPosition.X+624),(1-Math.Sign(currentPosition.X))));
                        paintSnake(currentPosition);
                    }
                    break;

                case (int)MOVINGDIRECTION.TORIGHT:
                    {
                        currentPosition.X = (currentPosition.X + 1) % 625;
                        paintSnake(currentPosition);
                    }
                    break;
            }

            ///<remarks>Ограничиваем поле.</remarks>
            //if ((currentPosition.X < 0) || (currentPosition.X > 622) || (currentPosition.Y < 0) || (currentPosition.Y > 380)) GameOver();

            ///<remarks>Попадание в бонусное очко вызывает эффект удлинения змеи.</remarks>
            int n = 0;
            foreach (Point point in bonusPoints)
            {

                if ((Math.Abs(point.X - currentPosition.X) < headSize) && (Math.Abs(point.Y - currentPosition.Y) < headSize))
                {
                    speechSynthesizer.SpeakAsync("оо как вкусно");

                    length += 10;
                    score += 10;
                    intSnakeSpeed = (intSnakeSpeed > 0) ? intSnakeSpeed - 1000 : intSnakeSpeed;
                    tbSpeed = (intSnakeSpeed > 0) ? tbSpeed + 1 : tbSpeed;

                    timer.Interval = new TimeSpan(intSnakeSpeed);

                    tbStatusScore.Text = score.ToString();
                    tbStatusSpeed.Text = tbSpeed.ToString();

                    ///<remarks>Когда бонусное очко съедено, удаляется объект питания из списка бонусов, а также с холста.</remarks>
                    bonusPoints.RemoveAt(n);
                    GameArea.Children.RemoveAt(n);
                    paintBonus(n);
                    break;
                }
                n++;
            }

            ///<remarks>Запрещаем змее врезаться в своё тело.</remarks>
            for (int q = 0; q < (snakePoints.Count - headSize * 2); q++)
            {
                Point point = new Point(snakePoints[q].X, snakePoints[q].Y);
                if ((Math.Abs(point.X - currentPosition.X) < (headSize)) && (Math.Abs(point.Y - currentPosition.Y) < (headSize)))
                {
                    GameOver();
                    break;
                }
            }
        }

        ///<remarks>Обработчик нажатий на клавиатуру.</remarks>
        private void OnButtonKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    {
                        if (previousDirection != (int)MOVINGDIRECTION.UPWARDS) direction = (int)MOVINGDIRECTION.DOWNWARDS;
                    }
                    break;

                case Key.Up:
                    {
                        if (previousDirection != (int)MOVINGDIRECTION.DOWNWARDS) direction = (int)MOVINGDIRECTION.UPWARDS;
                    }
                    break;

                case Key.Left:
                    {
                        if (previousDirection != (int)MOVINGDIRECTION.TORIGHT) direction = (int)MOVINGDIRECTION.TOLEFT;
                    }
                    break;

                case Key.Right:
                    {
                        if (previousDirection != (int)MOVINGDIRECTION.TOLEFT) direction = (int)MOVINGDIRECTION.TORIGHT;
                    }
                    break;

                case Key.Space:
                    {
                        if (bdrWelcomeMessage.IsVisible || bdrEndOfGame.IsVisible || isGameOver)
                        {
                            NewGame(sender, e);
                        }
                    }
                    break;
            }
            previousDirection = direction;
        }

        ///<remarks>Метод. Возобновляет игру.</remarks>
        private void BtnContinue_Click(object sender, EventArgs e)
        {
            timer.Start();
        }

        ///<remarks>Метод. Ставит игру на паузу.</remarks>
        private void BtnPause_Click(object sender, EventArgs e)
        {
            timer.Stop();
        }

        ///<remarks>Метод. Новая игра.</remarks> 
        private void NewGame(object sender, KeyEventArgs e)
        {
            bdrWelcomeMessage.Visibility = Visibility.Collapsed;
            bdrEndOfGame.Visibility = Visibility.Collapsed;

            isGameOver = false;
            score = 0;
            length = 100;
            tbSpeed = 0;
            intSnakeSpeed = 300000;
            SnakeSpeed = new TimeSpan(intSnakeSpeed);
            tbStatusScore.Text = "0";
            tbStatusSpeed.Text = "0";

            GameArea.Children.Clear();
            snakePoints.Clear();

            ///<remarks>Отрисовка объектов еды</remarks>
            for (int n = 0; n < 10; n++)
            {
                paintBonus(n);
            }

            timer.Interval = SnakeSpeed;
            timer.Start();

            startingPoint = new Point(100, 100);
            paintSnake(startingPoint);
            currentPosition = startingPoint;
        }

        ///<remarks>Метод. Конец игры.</remarks> 
        private void GameOver()
        {
            timer.Stop();
            isGameOver = true;

            /*
            bdrEndOfGame.Visibility = Visibility.Visible;
            tbFinalScore.Text = score.ToString();
            */

            PromptBuilder promptBuilder = new PromptBuilder();
            promptBuilder.StartStyle(new PromptStyle()
            {
                Emphasis = PromptEmphasis.Reduced,
                Rate = PromptRate.Slow,
                Volume = PromptVolume.ExtraLoud
            });
            promptBuilder.AppendText("оооо неееееет");
            promptBuilder.AppendBreak(TimeSpan.FromMilliseconds(200));
            promptBuilder.AppendText("я мертвааааа");
            promptBuilder.EndStyle();
            speechSynthesizer.SpeakAsync(promptBuilder);

            MessageBox.Show("Количество очков " + score.ToString()+".\nЧтобы начать новую игру нажмите пробел.", "Потрачено", MessageBoxButton.OK, MessageBoxImage.Hand);
        }
    }
}