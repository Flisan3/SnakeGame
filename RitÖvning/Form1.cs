using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

namespace RitÖvning
{
    public partial class Form1 : Form
    {

        //Skapar en lista av positioner som är avgörande för ormens segment
        List<PointF>snake = new List<PointF>();

        string highScoreFile = "highscore.txt";

        float SnakeXAxel = 150;
        float SnakeYAxel = 120;
        float xAutoAction = 0;
        float yAutoAction = 0;
        float xAuto = 0;
        float yAuto = 0;
        float FoodXAxel = 300;
        float FoodYAxel = 120;
        int Score = 0;
        int FoodDebug = 0;

        public Form1()
        {
            InitializeComponent();

            //Skapa en fil för highscore om den inte redan finns
            if (!File.Exists(highScoreFile))
            {
                File.WriteAllText(highScoreFile, "0");
            }

            label1.Text = "Use WASD or Arrow Keys to Move the Snake!";

            label2.Text = "High Score: " + (System.IO.File.Exists(highScoreFile) ? System.IO.File.ReadAllText(highScoreFile) : "0");

            this.KeyDown += Form1_KeyDown;

            //Skapa det första segmentet
            snake.Add(new PointF(150, 120));
        }

        private void Form1_Load(object sender, EventArgs e) 
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint |
                          ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }

        protected override void OnPaint(PaintEventArgs draw)
        {
            //Ritar ut alla segment
            foreach (PointF segment in snake)
            {
                draw.Graphics.FillRectangle(Brushes.Green, segment.X, segment.Y, 30, 30
                );
            }
            //Ritar ut maten och bordern
            draw.Graphics.FillRectangle(Brushes.Red, FoodXAxel, FoodYAxel, 30, 30);
            draw.Graphics.DrawRectangle(Pens.Black, 0, 0, 540, 300);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            xAuto = xAutoAction;
            yAuto = yAutoAction;

            //Spåra positionen av det sista segmentet innan det flyttas, så att vi kan lägga till ett nytt segment där när ormen växer
            PointF lastTail = snake[snake.Count - 1];

            //Flytta varje segment till positionen av det föregående segmentet
            for (int i = snake.Count - 1; i > 0; i--)
            {
                snake[i] = snake[i - 1];
            }

            //Flytta huvudet i den aktuella riktningen
            snake[0] = new PointF(
                snake[0].X + xAuto,
                snake[0].Y + yAuto
            );

            //Kolla om ormen har ätit maten och låt ormen växa
            if (snake[0].X == FoodXAxel && snake[0].Y == FoodYAxel)
            {
                GrowSnake(lastTail);
                SpawnFood();
            }

            Refresh();

            //Kolla om ormen har krockat med väggarna eller sig själv
            if (snake[0].X >= 540 || snake[0].X < 0 || snake[0].Y >= 300 || snake[0].Y < 0 || SnakeHitsItself())
            {
                timer1.Stop();

                //Kolla efter nytt highscore och spara det i filen
                if (System.IO.File.Exists(highScoreFile))
                {
                    string text = System.IO.File.ReadAllText(highScoreFile);
                    if (int.TryParse(text, out int high))
                        high = high;

                    if (Score > high)
                    {
                        System.IO.File.WriteAllText("highScore.txt", Score.ToString());
                        label2.Text = "High Score: " + (System.IO.File.Exists(highScoreFile) ? System.IO.File.ReadAllText(highScoreFile) : "0");

                    }
                }

                DialogResult result = MessageBox.Show("Game Over!\n\nRetry?","Snake", MessageBoxButtons.YesNo,MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    ResetGame();
                }
                else
                {
                    Application.Exit();
                }

            }

            if (Score > 179)
            {
                timer1.Stop();
                MessageBox.Show("Congratulations! You have reached the maximum score of 180!", "Snake", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            Refresh();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //Kollar knapptryck och låter ormen färdas ditåt samt förhindrar 180 graders svängar

            if ((e.KeyCode == Keys.A || e.KeyCode == Keys.Left) && xAuto != 30)
            {
                xAutoAction = -30;
                yAutoAction = 0;
            }
            else if ((e.KeyCode == Keys.D || e.KeyCode == Keys.Right) && xAuto != -30)
            {
                xAutoAction = 30;
                yAutoAction = 0;
            }
            else if ((e.KeyCode == Keys.W || e.KeyCode == Keys.Up) && yAuto != 30)
            {
                xAutoAction = 0;
                yAutoAction = -30;
            }
            else if ((e.KeyCode == Keys.S || e.KeyCode == Keys.Down) && yAuto != -30)
            {
                xAutoAction = 0;
                yAutoAction = 30;
            }

            e.SuppressKeyPress = true;

        }

        private void GrowSnake(PointF tailPosition)
        {
            snake.Add(tailPosition);
        }


        private void SpawnFood()
        {
            //Generera en slumpmässig position för maten inom spelområdet
            Random random = new Random();

            while (FoodDebug == 0)
            {
                FoodXAxel = random.Next(0, 18) * 30;
                FoodYAxel = random.Next(0, 10) * 30;

                //Kolla så att maten inte spawnar på ormen
                bool onSnake = snake.Any(segment => segment.X == FoodXAxel && segment.Y == FoodYAxel);

                if (!onSnake)
                {
                    FoodDebug = 1;
                }
            }
            //Höj poängen och uppdatera labeln
            FoodDebug = 0;
            label1.Text = "Score: " + ++Score;
        }

        private void ResetGame()
        {
            //Återställ ormens position och riktning, samt placera maten på en ny plats
            snake.Clear();
            snake.Add(new PointF(150, 120));

            xAuto = 0;
            yAuto = 0;
            xAutoAction = 0;
            yAutoAction = 0;

            FoodXAxel = 300;
            FoodYAxel = 120;

            //Återställ poängen
            Score = 0;
            label1.Text = "Score: " + Score;
            label2.Text = "High Score: " + (System.IO.File.Exists(highScoreFile) ? System.IO.File.ReadAllText(highScoreFile) : "0");


            timer1.Start();
            Refresh();
        }

        private bool SnakeHitsItself()
        {
            //Kolla om huvudet krockar med något av segmenten i kroppen
            PointF head = snake[0];

            for (int i = 1; i < snake.Count; i++)
            {
                if (snake[i].X == head.X && snake[i].Y == head.Y)
                    return true;
            }

            return false;
        }
    }
}
