using GOL_Project;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GOL_Project
{
    public partial class Form1 : Form
    {
        //main x,y initializers
        public static int mainX = 10;
        public static int mainY = 10;

        // The universe array
        bool[,] universe = new bool[mainX, mainY];
        bool[,] scratchPad = new bool[mainX, mainY];

        // Drawing colors
        Color gridColor = Color.Black;
        Color cellColor = Color.Cyan;
        Color numColor = Color.Red;
        Color backColor = Color.White;

        //fonts
        Font font = new Font("Arial", 10);
        Font font2 = new Font("Arial", 10);

        // The Timer class
        Timer timer = new Timer();

        // bottom strip trackers
        int NeighborCount;
        int generations = 0;
        int alive = 0;
        int seed;

        //bool settings
        bool HUDVisible = true;
        bool CountVisible = true;
        bool GridVisible = true;
        bool ToroidalVisible = true;
        bool FiniteVisible = false;

        //main form
        public Form1()
        {
            InitializeComponent();

            // Setup the timer
            timer.Interval = 100; // milliseconds
            timer.Tick += Timer_Tick;
            timer.Enabled = false; // start timer running

            backColor = Properties.Settings.Default.PanelColor;
            cellColor = Properties.Settings.Default.CellColor;
            gridColor = Properties.Settings.Default.GridColor;
        }

        //count neighbors finite
        private int CountNeighborsFinite(int x, int y)
        {
            int count = 0;
            int xLen = universe.GetLength(0);
            int yLen = universe.GetLength(1);
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    int xCheck = x + xOffset;
                    int yCheck = y + yOffset;
                    if (xOffset == 0 && yOffset == 0)
                    {
                        continue;
                    }
                    if (xCheck < 0)
                    {
                        continue;
                    }
                    if (yCheck < 0)
                    {
                        continue;
                    }
                    if (xCheck >= xLen)
                    {
                        continue;
                    }
                    if (yCheck >= yLen)
                    {
                        continue;
                    }
                    if (universe[xCheck, yCheck] == true)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        //count neighbors toroidal
        private int CountNeighborsToroidal(int x, int y)
        {
            int count = 0;
            int xLen = universe.GetLength(0);
            int yLen = universe.GetLength(1);
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    int xCheck = x + xOffset;
                    int yCheck = y + yOffset;
                    if (xOffset == 0 && yOffset == 0)
                    {
                        continue;
                    }
                    if (xCheck < 0)
                    {
                        xCheck = xLen - 1;
                    }
                    if (yCheck < 0)
                    {
                        yCheck = yLen - 1;
                    }
                    if (xCheck >= xLen)
                    {
                        xCheck = 0;
                    }
                    if (yCheck >= yLen)
                    {
                        yCheck = 0;
                    }
                    if (universe[xCheck, yCheck] == true)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        // Calculate the next generation of cells
        private void NextGeneration()
        {
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    if (FiniteVisible == true)
                    {
                        NeighborCount = CountNeighborsFinite(x, y);
                    }
                    if (ToroidalVisible == true)
                    {
                        NeighborCount = CountNeighborsToroidal(x, y);
                    }

                    if (universe[x, y] == true && NeighborCount < 2)
                    {
                        scratchPad[x, y] = false;
                    }
                    if (universe[x, y] == true && NeighborCount > 3)
                    {
                        scratchPad[x, y] = false;
                    }
                    if (universe[x, y] == true)
                    {
                        if (NeighborCount == 2 | NeighborCount == 3)
                        {
                            scratchPad[x, y] = true;
                        }
                    }
                    if (universe[x, y] == false)
                    {
                        if (NeighborCount == 3)
                        {
                            scratchPad[x, y] = true;
                        }
                        if (NeighborCount != 3)
                        {
                            scratchPad[x, y] = false;
                        }
                    }
                }
            }

            bool[,] temp = universe;
            universe = scratchPad;
            scratchPad = temp;

            // Increment generation count
            generations++;

            // Update status strip generations
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();           
            graphicsPanel1.Invalidate();
        }

        // The event called by the timer every Interval milliseconds.
        private void Timer_Tick(object sender, EventArgs e)
        {
            NextGeneration();
            graphicsPanel1.Invalidate();
        }

        private void graphicsPanel1_Paint(object sender, PaintEventArgs e)
        {
            // Calculate the width and height of each cell in pixels
            // CELL WIDTH = WINDOW WIDTH / NUMBER OF CELLS IN X
            int cellWidth = graphicsPanel1.ClientSize.Width / universe.GetLength(0);
            // CELL HEIGHT = WINDOW HEIGHT / NUMBER OF CELLS IN Y
            int cellHeight = graphicsPanel1.ClientSize.Height / universe.GetLength(1);

            Pen gridPen = new Pen(gridColor, 1);
            Brush cellBrush = new SolidBrush(cellColor);
            Brush numBrush = new SolidBrush(numColor);
            Brush backBrush = new SolidBrush(backColor);

            alive = 0;
            // Iterate through the universe in the y, top to bottom
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    // A rectangle to represent each cell in pixels
                    Rectangle cellRect = Rectangle.Empty;
                    cellRect.X = x * cellWidth;
                    cellRect.Y = y * cellHeight;
                    cellRect.Width = cellWidth;
                    cellRect.Height = cellHeight;

                    // Fill the cell with a brush if alive
                    if (universe[x, y] == true)
                    {
                        e.Graphics.FillRectangle(cellBrush, cellRect);
                        alive++;
                    }
                    if (universe[x, y] == false)
                    {
                        e.Graphics.FillRectangle(backBrush, cellRect);
                    }

                    toolStripStatusLabel1.Text = "Alive = " + alive.ToString();

                    if (FiniteVisible == true)
                    {
                        NeighborCount = CountNeighborsFinite(x, y);
                    }
                    if (ToroidalVisible == true)
                    {
                        NeighborCount = CountNeighborsToroidal(x, y);
                    }
                    if (CountVisible == true)
                    {
                        if (NeighborCount != 0)
                        {
                            StringFormat stringFormat = new StringFormat();
                            stringFormat.Alignment = StringAlignment.Center;
                            stringFormat.LineAlignment = StringAlignment.Center;
                            Rectangle rect = new Rectangle(cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                            e.Graphics.DrawString(NeighborCount.ToString(), font, numBrush, rect, stringFormat);
                        }
                    }
                    if (GridVisible == true)
                    {
                        e.Graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                    }
                    if (HUDVisible == true)
                    {
                        //NO idea why my cell count hud is overlapping, when nothing else is.                       
                        StringFormat stringFormat2 = new StringFormat();
                        stringFormat2.Alignment = StringAlignment.Near;
                        stringFormat2.LineAlignment = StringAlignment.Near;
                        Rectangle rect2 = new Rectangle(0, 0, 300, 200);
                        e.Graphics.DrawString("Universe Size: {Width=" + mainX + ", Height=" + mainY + "}" + "\nBoundary Type: " +
                        "\nGenerations: " + generations.ToString() + "\nCell Count: " + alive.ToString(), font2, numBrush, rect2, stringFormat2);
                    }                   
                }
            }

            // Cleaning up pens and brushes
            gridPen.Dispose();
            cellBrush.Dispose();
            numBrush.Dispose();
            backBrush.Dispose();
        }

        //mouse clicker on form
        private void graphicsPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            // If the left mouse button was clicked
            if (e.Button == MouseButtons.Left)
            {
                // Calculate the width and height of each cell in pixels
                int cellWidth = graphicsPanel1.ClientSize.Width / universe.GetLength(0);
                int cellHeight = graphicsPanel1.ClientSize.Height / universe.GetLength(1);

                // Calculate the cell that was clicked in
                // CELL X = MOUSE X / CELL WIDTH
                int x = e.X / cellWidth;
                // CELL Y = MOUSE Y / CELL HEIGHT
                int y = e.Y / cellHeight;

                // Toggle the cell's state
                universe[x, y] = !universe[x, y];

                // Tell Windows you need to repaint
                graphicsPanel1.Invalidate();
            }
        }

        //exit button
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //form loads
        private void Form1_Load(object sender, EventArgs e)
        {
            //loaded
        }

        //new file button
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            universe = new bool[mainX, mainY];
            scratchPad = new bool[mainX, mainY];
            generations = 0;
            alive = 0;
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();
            toolStripStatusLabel1.Text = "Alive = " + alive.ToString();
            toolStripStatusLabel2.Text = "Seed = " + seed.ToString();
            graphicsPanel1.Invalidate();
        }
        
        //Open logs
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamReader reader = new StreamReader(dlg.FileName);

                while (!reader.EndOfStream)
                {
                    string row = reader.ReadLine();
                    if (row.Contains("!"))
                    {
                        continue;
                    }
                    if (!row.Contains("!"))
                    {
                        mainX = row.Length;
                        mainY++;
                    }
                }

                //new arrays
                universe = new bool[mainX, mainY];
                scratchPad = new bool[mainX, mainY];
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                int yPos = 0;
                while (!reader.EndOfStream)
                {
                    string row = reader.ReadLine();
                    if (row.Contains("!"))
                    {
                        continue;
                    }
                    if (!row.Contains("!"))
                    {
                        for (int xPos = 0; xPos < row.Length; xPos++)
                        {
                            if (row[xPos] == 'O')
                            {
                                universe[xPos, yPos] = true;
                                scratchPad[xPos, yPos] = true;
                            }
                            if (row[xPos] == '.')
                            {
                                universe[xPos, yPos] = false;
                                scratchPad[xPos, yPos] = false;
                            }
                        }
                        yPos++;
                    }
                }
                reader.Close();
                //close and reset
                generations = 0;
                alive = 0;
                toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();
                toolStripStatusLabel1.Text = "Alive = " + alive.ToString();
                toolStripStatusLabel2.Text = "Seed = " + seed.ToString();
                graphicsPanel1.Invalidate();
            }
        }

        //save files
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2; dlg.DefaultExt = "cells";

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamWriter writer = new StreamWriter(dlg.FileName);
                writer.WriteLine("! Save File for Game of Life");

                for (int y = 0; y < universe.GetLength(1); y++)
                {
                    String currentRow = string.Empty;
                    for (int x = 0; x < universe.GetLength(0); x++)
                    {
                        if (universe[x, y] == true)
                        {
                            currentRow += "O";
                        }
                        else
                        {
                            currentRow += ".";
                        }
                    }
                    writer.WriteLine(currentRow);
                }
                writer.Close();
            }
        }

        //hud visibility
        private void hUDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (HUDVisible == false)
            {
                HUDVisible = true;
                hUDToolStripMenuItem.Checked = true;
                hUDToolStripMenuItem1.Checked = true;
            }
            else if (HUDVisible == true)
            {
                HUDVisible = false;
                hUDToolStripMenuItem.Checked = false;
                hUDToolStripMenuItem1.Checked = false;
            }
            graphicsPanel1.Invalidate();
        }

        //neighborcount visibility
        private void neighborCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CountVisible == false)
            {
                CountVisible = true;
                neighborCountToolStripMenuItem.Checked = true;
                neighborCountToolStripMenuItem1.Checked = true;
            }
            else if (CountVisible == true)
            {
                CountVisible = false;
                neighborCountToolStripMenuItem.Checked = false;
                neighborCountToolStripMenuItem1.Checked = false;
            }
            graphicsPanel1.Invalidate();
        }

        //Grid visibility
        private void gridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GridVisible == false)
            {
                GridVisible = true;
                gridToolStripMenuItem.Checked = true;
                gridToolStripMenuItem1.Checked = true;
            }
            else if (GridVisible == true)
            {
                GridVisible = false;
                gridToolStripMenuItem.Checked = false;
                gridToolStripMenuItem1.Checked = false;
            }
            graphicsPanel1.Invalidate();
        }

        //toroidal visibility
        private void toroidalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ToroidalVisible == false)
            {
                ToroidalVisible = true;
                FiniteVisible = false;
                toroidalToolStripMenuItem.Checked = true;
                finiteToolStripMenuItem.Checked = false;
            }
            else if (ToroidalVisible == true)
            {
                ToroidalVisible = true;
                toroidalToolStripMenuItem.Checked = true;
            }
            graphicsPanel1.Invalidate();
        }

        //finite visibility
        private void finiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FiniteVisible == false)
            {
                FiniteVisible = true;
                ToroidalVisible = false;
                finiteToolStripMenuItem.Checked = true;
                toroidalToolStripMenuItem.Checked = false;
            }
            else if (FiniteVisible == true)
            {
                FiniteVisible = true;
                finiteToolStripMenuItem.Checked = true;
            }
            graphicsPanel1.Invalidate();
        }

        //from seed settings
        private void fromSeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SeedDialog dlg = new SeedDialog();
            dlg.ModalSeed = seed;
            int num;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                seed = dlg.ModalSeed;
                Random rand = new Random(seed);

                for (int y = 0; y < universe.GetLength(1); y++)
                {
                    for (int x = 0; x < universe.GetLength(0); x++)
                    {
                        num = rand.Next(0, 2);
                        if (num == 0)
                        {
                            universe[x, y] = true;
                        }
                        else
                        {
                            universe[x, y] = false;
                        }
                    }
                }
                //reset all
                generations = 0;
                alive = 0;
                toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();
                toolStripStatusLabel1.Text = "Alive = " + alive.ToString();
                toolStripStatusLabel2.Text = "Seed = " + seed.ToString();
                graphicsPanel1.Invalidate();
            }
        }

        //from current seed
        private void fromCurrentSeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int num;
            Random rand = new Random(seed);

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    num = rand.Next(0, 2);
                    if (num == 0)
                    {
                        universe[x, y] = true;
                    }
                    else
                    {
                        universe[x, y] = false;
                    }
                }
            }
            //reset all
            generations = 0;
            alive = 0;
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();
            toolStripStatusLabel1.Text = "Alive = " + alive.ToString();
            toolStripStatusLabel2.Text = "Seed = " + seed.ToString();
            graphicsPanel1.Invalidate();
        }

        //from time of day seed
        private void fromTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int num;
            Random rand = new Random(DateTime.Now.Day);
            seed = rand.Next();

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    num = rand.Next(0, 2);
                    if (num == 0)
                    {
                        universe[x, y] = true;
                    }
                    else
                    {
                        universe[x, y] = false;
                    }
                }
            }
            //reset all
            generations = 0;
            alive = 0;
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();
            toolStripStatusLabel1.Text = "Alive = " + alive.ToString();
            toolStripStatusLabel2.Text = "Seed = " + seed.ToString();
            graphicsPanel1.Invalidate();
        }

        //back color settings
        private void backColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = backColor;
            if (DialogResult.OK == dlg.ShowDialog())
            {
                backColor = dlg.Color;
                graphicsPanel1.Invalidate();
            }
        }

        //cell color settings
        private void cellColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = cellColor;
            if (DialogResult.OK == dlg.ShowDialog())
            {
                cellColor = dlg.Color;
                graphicsPanel1.Invalidate();
            }
        }

        //grid color settings
        private void gridColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog();
            dlg.Color = gridColor;
            if (DialogResult.OK == dlg.ShowDialog())
            {
                gridColor = dlg.Color;
                graphicsPanel1.Invalidate();
            }
        }

        //options in settings button, for modal
        private void optionsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ModalDialog dlg = new ModalDialog();
            dlg.ModalTimer = timer.Interval;
            dlg.ModalWidth = mainX;
            dlg.ModalHeight = mainY;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                timer.Interval = dlg.ModalTimer;
                mainX = dlg.ModalWidth;
                mainY = dlg.ModalHeight;

                universe = new bool[mainX, mainY];
                scratchPad = new bool[mainX, mainY];

                graphicsPanel1.Invalidate();
            }
        }

        //reset button in settings
        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();
            backColor = Properties.Settings.Default.PanelColor;
            cellColor = Properties.Settings.Default.CellColor;
            gridColor = Properties.Settings.Default.GridColor;
            graphicsPanel1.Invalidate();
        }

        //reload button in settings
        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();
            backColor = Properties.Settings.Default.PanelColor;
            cellColor = Properties.Settings.Default.CellColor;
            gridColor = Properties.Settings.Default.GridColor;
            graphicsPanel1.Invalidate();
        }

        //right clicker
        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            //opens the right click
        }

        //play button
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            timer.Enabled = true;
        }

        //pause button
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            timer.Enabled = false;
        }

        //next button
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            NextGeneration();
            graphicsPanel1.Invalidate();
        }

        //exiting program
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default.PanelColor = backColor;
            Properties.Settings.Default.CellColor = cellColor;
            Properties.Settings.Default.GridColor = gridColor;
            Properties.Settings.Default.Save();
        }
    }
}