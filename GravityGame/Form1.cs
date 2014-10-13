using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GravityGame
{
    public partial class Form1 : Form
    {
        // create the player ellipse instance
        Ellipse playerEllipse = new Ellipse();
        // create a list for multiple enemy ellipses
        List<Ellipse> enemyEllipses = new List<Ellipse>();

        // set some initial variables
        int initialEnemies = 5;
        double stepDistanceX = 0.0;
        double stepDistanceY = 0.0;
        int thisStep = 0;
        int totalSteps = 0;
        int enemySteps = 0;
        int timeForNewEnemy = 250;
        int playerPoints = 0;
        int maxEnemySpeed = 10;
        double basePlayerSize = 60.0;
        bool redrawFlag = true;
        int enemyInitSize = 20;

        public Form1()
        {
            InitializeComponent();
        }

        // Create a class to define an ellipse
        class Ellipse
        {
            // create variables for the ellipse position, size, and fill color
            public Point startPoint;
            public Point currentPoint;
            public Point endPoint;
            public double size;
            public double maxSize;
            // create variables for the enemy step distance
            public int stepX;
            public int stepY;
        }

        // create a random number class so i can call it as fast as i want and still get new values
        private static Random rnd;
        static Form1()
        {
            rnd = new Random();
        }

        // initially create the user's ellipse
        private void initPlayer1Ellipse()
        {            
            // put the player in the middle of the box
            playerEllipse.currentPoint.X = pictureBox1.Width / 2;
            playerEllipse.currentPoint.Y = pictureBox1.Height / 2;
            // set the initial size, color, name
            playerEllipse.size = basePlayerSize;
            // invalidate in order to repaint the picture box
            pictureBox1.Invalidate();
        }

        // create a new enemy ellipse with a random size, location, speed, and heading
        private Ellipse makeANewEnemy()
        {
            // create a new ellipse called enemyEllipse
            Ellipse enemyEllipse = new Ellipse();
            // pick a random size
            enemyEllipse.maxSize = rnd.Next(enemyInitSize, 100);
            enemyEllipse.size = enemyInitSize;
            // pick a random start location (ensure it won't go off the side or start too close to the player in the center)
            enemyEllipse.currentPoint.X = rnd.Next(Convert.ToInt32(enemyEllipse.size / 2), pictureBox1.Width / 2 - 60);
            enemyEllipse.currentPoint.Y = rnd.Next(Convert.ToInt32(enemyEllipse.size / 2), pictureBox1.Height / 2 - 60);
            if (rnd.Next(0, 2) == 1) // flip a coin to determine if the enemy should go on the left or right of center
            {
                enemyEllipse.currentPoint.X = pictureBox1.Width - enemyEllipse.currentPoint.X;
            }
            if (rnd.Next(0, 2) == 1) // flip a coin to determine if the enemy should go above or below the center
            {
                enemyEllipse.currentPoint.Y = pictureBox1.Height - enemyEllipse.currentPoint.Y;
            }
            // pick a random direction (this ensures the enemy speed will be the maxEnemySpeed)
            enemyEllipse.stepX = rnd.Next(1, maxEnemySpeed-1);
            enemyEllipse.stepY = Convert.ToInt32(Math.Sqrt(Math.Pow(maxEnemySpeed,2) - Math.Pow(enemyEllipse.stepX, 2)));
            if (rnd.Next(0, 2) == 1) // flip a coin to determine if the enemy should move left or right
            {
                enemyEllipse.stepX = -enemyEllipse.stepX;
            }
            if (rnd.Next(0, 2) == 1) // flip a coin to determine if the enemy should move up or down
            {
                enemyEllipse.stepY = -enemyEllipse.stepY;
            }
            // add the ellipse to the list of ellipses
            enemyEllipses.Add(enemyEllipse);
            // return the enemyEllipse
            return enemyEllipse;
        }

        // initially create the enemy ellipses
        private void initEnemyEllipses()
        {
            // loop through the number of initial enemies, creating them one at a time
            for (int i = 1; i <= initialEnemies; i++)
            {
                Ellipse enemyEllipse = makeANewEnemy();
            }
            // invalidate in order to repaint the picture box
            pictureBox1.Invalidate();
        }

        // draws a single ellipse on the form
        private void drawEllipse(Ellipse myEllipse, PaintEventArgs e, Boolean playerFlag)
        {
            // give the appropriate X,Y that offsets the width/height of the ellipse
            int drawingX = myEllipse.currentPoint.X - Convert.ToInt32(myEllipse.size) / 2;
            int drawingY = myEllipse.currentPoint.Y - Convert.ToInt32(myEllipse.size) / 2;
            // now adding an image to that location
            Image newImage = Image.FromFile(@"../../../Images/Asteroid.png");
           if (playerFlag == true)
           {
              newImage = Image.FromFile(@"../../../Images/Player.png");
           }
            e.Graphics.DrawImage(newImage, drawingX, drawingY, Convert.ToInt32(myEllipse.size), Convert.ToInt32(myEllipse.size));
        }

        // determine where the user wants their ellipse to move
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            // set the start point to where the ellipse is currently
            playerEllipse.startPoint.X = playerEllipse.currentPoint.X;
            playerEllipse.startPoint.Y = playerEllipse.currentPoint.Y;
            // determine where the player ellipse is supposed to go (where the user clicked)
            playerEllipse.endPoint.X = e.X;
            playerEllipse.endPoint.Y = e.Y;
            // call the method to move the ellipse from the start point to the end point
            moveEllipse(playerEllipse);
        }

        // set up how the player ellipse will move at each time step and initiates that timer
        private void moveEllipse(Ellipse myEllipse)
        {
            // calculate the distance to move
            double moveDistanceTotal = Math.Sqrt(Math.Pow(playerEllipse.startPoint.X - playerEllipse.endPoint.X, 2) + Math.Pow(playerEllipse.startPoint.Y - playerEllipse.endPoint.Y, 2));
            // calculate the number of steps for the move
            int maxStepSize = 100;
            totalSteps = Convert.ToInt32(maxStepSize * moveDistanceTotal / (Math.Sqrt(Math.Pow(pictureBox1.Width, 2) + Math.Pow(pictureBox1.Height, 2))));
            // calculate how far to move each step, this ensures a constant speed
            stepDistanceX = (playerEllipse.endPoint.X - playerEllipse.startPoint.X + 0.0) / totalSteps;
            stepDistanceY = (playerEllipse.endPoint.Y - playerEllipse.startPoint.Y + 0.0) / totalSteps;
            // set things up for the timer to take care of moving the ellipse
            thisStep = 0;
            // enables the timer
            playerTimer.Enabled = true;
        }

        // start the game from scratch
        private void button1_Click(object sender, EventArgs e)
        {
            // in case of restart, remove any existing enemy ellipses from the list and repaint the form
            if (enemyEllipses.Count > 0)
            {
                enemyEllipses.Clear();
                Form1.ActiveForm.Refresh();
            }
            // now start everything up
            button1.Visible = false; // hides this button
            pictureBox1.Update(); // redraws the background
            initPlayer1Ellipse(); // initializes the player ellipse
            initEnemyEllipses(); // initializes the enemy ellipses
            enemyTimer.Enabled = true; // starts the enemy timer so they'll start moving
            redrawFlag = true;
            // display the updated points
            Form1.ActiveForm.Text = "Gravity Game -- Points: " + Convert.ToString(playerPoints);
        }

        // move and shrink the player at each time step
        private void playerTimer_Tick_1(object sender, EventArgs e)
        {
            // shrink the ellipse slightly (it takes mass to move)
            double shrinkPerStep = 0.1;
            playerEllipse.size = playerEllipse.size - shrinkPerStep;
            // update the current point
            playerEllipse.currentPoint.X = Convert.ToInt32(playerEllipse.currentPoint.X + stepDistanceX);
            playerEllipse.currentPoint.Y = Convert.ToInt32(playerEllipse.currentPoint.Y + stepDistanceY);
            // invalidate in order to repaint the picture box
            pictureBox1.Invalidate();
            // increment the step integer
            thisStep++;
            // stop moving if the player has reached the final location
            bool stopMoving = thisStep >= totalSteps;
            if (stopMoving)
            {
                // disable the timer
                playerTimer.Enabled = false;
            }
        }

        // move an enemy ellipse one step, account for hitting the edge of the screen
        private void enemyPositionUpdate(Ellipse myEllipse)
        {
            // move the ellipse one step size
            myEllipse.currentPoint.X = myEllipse.currentPoint.X + myEllipse.stepX;
            myEllipse.currentPoint.Y = myEllipse.currentPoint.Y + myEllipse.stepY;
            // check if the ellipse is hitting the left edge of the box and send it back the other way
            if (myEllipse.currentPoint.X - myEllipse.size / 2 <= 0 & myEllipse.stepX < 0)
            {
                myEllipse.stepX = -myEllipse.stepX;
            }
            // check if the ellipse is hitting the right edge of the box
            if (myEllipse.currentPoint.X + myEllipse.size / 2 >= pictureBox1.Width & myEllipse.stepX > 0)
            {
                myEllipse.stepX = -myEllipse.stepX;
            }
            // check if the ellipse is hitting the top edge of the box
            if (myEllipse.currentPoint.Y - myEllipse.size / 2 <= 0 & myEllipse.stepY < 0)
            {
                myEllipse.stepY = -myEllipse.stepY;
            }
            // check if the ellipse is hitting the bottom edge of the box
            if (myEllipse.currentPoint.Y + myEllipse.size / 2 >= pictureBox1.Height & myEllipse.stepY > 0)
            {
                myEllipse.stepY = -myEllipse.stepY;
            }
        }

        // move the enemy ellipses at every timestep
        private void enemyTimer_Tick(object sender, EventArgs e)
        {
            // increment the number of enemy steps
            enemySteps++;
            // add a new enemy at regular intervals
            if (enemySteps >= timeForNewEnemy)
            {
                enemySteps = 0;
                Ellipse enemyEllipse = makeANewEnemy();
                if (initialEnemies < 10) // there's a max of 10 enemies on the screen at a time
                {
                    initialEnemies++;
                }
                if (enemyInitSize < 30) // there's a max initial enemy size of 30
                {
                    enemyInitSize++;
                }
            }
            // loop through each enemy ellipse
            for (int i = 0; i < enemyEllipses.Count; i ++)
            {
                // increase the enemy size if not at the max already
                if (enemyEllipses[i].size < enemyEllipses[i].maxSize)
                {
                    enemyEllipses[i].size++;
                }
                // move it to the new location
                enemyPositionUpdate(enemyEllipses[i]);
            }
            // invalidate in order to repaint the picture box
            pictureBox1.Invalidate();
            // check if there are any player / enemy interactions
            detectInteractions();
        }

        // determine if the player ellipse touches an enemy
        private void detectInteractions()
        {
            // loop through each enemy ellipse
            for (int i = 0; i < enemyEllipses.Count; i++)
            {
                // calculate the distance between the player and enemy
                double distanceDelta = Math.Sqrt(Math.Pow(enemyEllipses[i].currentPoint.X - playerEllipse.currentPoint.X,2) + Math.Pow(enemyEllipses[i].currentPoint.Y - playerEllipse.currentPoint.Y,2));
                // check if they touch
                if (distanceDelta <= playerEllipse.size/2 + enemyEllipses[i].size/2)
                {
                    // check if the player is larger than the enemy
                    if (playerEllipse.size > enemyEllipses[i].size)
                    {
                        // grow the player, how much depends on the size of the enemy
                        playerEllipse.size = playerEllipse.size + enemyEllipses[i].size / 4;
                        // remove the enemy from the playing field
                        enemyEllipses[i].currentPoint = new Point(10000, 10000);
                        enemyEllipses[i].stepX = 0;
                        enemyEllipses[i].stepY = 0;
                        // increase the player's points based on the size of the enemy
                        playerPoints = playerPoints + Convert.ToInt32(enemyEllipses[i].size);
                        // display the updated points
                        Form1.ActiveForm.Text = "Gravity Game -- Points: " + Convert.ToString(playerPoints);
                        // determine if any active enemies remain
                        Boolean allEnemiesVanquishedFlag = true;
                        for (int j = 0; j < enemyEllipses.Count; j++)
                        {
                            if (enemyEllipses[j].stepX != 0)
                            {
                                allEnemiesVanquishedFlag = false;
                            }
                        }
                        // handle the case if all enemies are gone
                        if (allEnemiesVanquishedFlag)
                        {
                            // give a point bonus to the player
                            playerPoints = playerPoints + 500;
                            // display the updated points
                            Form1.ActiveForm.Text = "Gravity Game -- Points: " + Convert.ToString(playerPoints);
                            // increase the enemy speed
                            maxEnemySpeed++;
                            // make the player smaller
                            playerEllipse.size = basePlayerSize;
                            // add a new set of enemies
                            initEnemyEllipses(); // initializes the enemy ellipses
                        }
                    }
                    // handle the case when the enemy is larger than the player (you lose)
                    else
                    {
                        // disable the timers so the ellipses won't move
                        playerTimer.Enabled = false;
                        enemyTimer.Enabled = false;
                        redrawFlag = false;
                        // display a message with the current points
                        MessageBox.Show("You Lost - " + playerPoints + " points");
                        // put the restart button up
                        button1.Visible = true;
                        button1.Text = "Restart?";
                        // reset everything
                        enemySteps = 0;
                        playerPoints = 0;
                    }
                    // invalidate in order to repaint the picture box
                    pictureBox1.Invalidate();
                }
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void pictureBox1_Paint_1(object sender, PaintEventArgs e)
        {
            if (redrawFlag)
            {
                // loop through each enemy ellipse
                for (int i = 0; i < enemyEllipses.Count; i++)
                {
                    // draw the enemy ellipse in the new location
                    drawEllipse(enemyEllipses[i], e, false);
                }
                // draw the player ellipse in the new location
                drawEllipse(playerEllipse, e, true);
            }
        }
    }
}
