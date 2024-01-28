using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace kursae
{
    public partial class Form1 : Form
    {
        int count = 10;
        bool endGame = false;
        Cell[,] playerCells;
        Cell[,] pcCells;
        List<Effect> effects = new List<Effect>();
        bool find = false;
        elem k;
        int newk;
        string filename = "Scores";
        private bool _moving;
        private Point _startLocation;
        struct elem {
            public int score;
            public string name;
        }
        List<elem> people = new List<elem>();
        elem elem1;
        elem elem2;
        public Form1()
        {
            List<string> lines = new List<string>();
            if (File.Exists(filename))
            {
                using (BinaryReader sr = new BinaryReader(File.Open(filename, FileMode.Open)))
                {
                    while (sr.PeekChar() != -1)
                    {
                        elem elem1;
                        elem1.name = sr.ReadString();
                        elem1.score = sr.ReadInt32();
                        people.Add(elem1);
                    }

                }

            }

            this.KeyPreview = true;
            InitializeComponent();
            inputbutton.Enabled = false;
            button1.FlatAppearance.MouseDownBackColor = Color.Transparent;
            button1.FlatAppearance.MouseOverBackColor = Color.Transparent;
            exitbutton.FlatAppearance.MouseDownBackColor = Color.Transparent;
            exitbutton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            leadersbutton.FlatAppearance.MouseDownBackColor = Color.Transparent;
            leadersbutton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            okbutton.FlatAppearance.MouseDownBackColor = Color.Transparent;
            okbutton.FlatAppearance.MouseOverBackColor = Color.Transparent;


            winpicture.Image = Image.FromFile("win.png");
            okbutton.BackgroundImage = Image.FromFile("okbutton.png");
            defeatpicture.Image = Image.FromFile("defeat.png");

            Ship.SetImage(Image.FromFile("ship.png"));//загружаем корабль
            Ship.SetSize(new Point(40, 40)); //загружаем размеры корабля
            Effect.SetImage(Image.FromFile("effect.png"));


            this.Controls.Remove(winpicture);
            this.Controls.Remove(okbutton);
            this.Controls.Remove(defeatpicture);

            this.Controls.Remove(inputbox);
            this.Controls.Remove(label1);
            this.Controls.Remove(nameBox);
            this.Controls.Remove(richTextBox1);
            this.Controls.Remove(inputbutton);
            Invalidate();
        }

        private int SortByScore(elem elem1, elem elem2)
        {

            return elem2.score.CompareTo(elem1.score);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string s;
            elem k;
            people.Sort(SortByScore);
            for (int i = 0; i < people.Count; i++)
            {
                if (people[i].score < 0)
                {
                    k = people[i];
                    k.score = 0;
                    people[i] = k;
                }
                richTextBox1.AppendText(people[i].name);
                int len = people[i].name.Length;
                for (int j = 0; j < 30-len; j++)
                    richTextBox1.AppendText(" ");
                richTextBox1.AppendText(people[i].score + "\n");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Controls.Add(inputbox);
            inputbox.BringToFront();
            this.Controls.Add(label1);
            label1.BringToFront();
            this.Controls.Add(nameBox);
            nameBox.BringToFront();
            this.Controls.Add(inputbutton);
            inputbutton.BringToFront();
            leadersbutton.Enabled = false;
            exitbutton.Enabled = false;          
        }
        private void inputbutton_Click(object sender, EventArgs e)
        {
            this.BackgroundImage = Image.FromFile("background.png");
            effects = new List<Effect>();
            this.Controls.Remove(button1);
            this.Controls.Remove(exitbutton);
            this.Controls.Remove(leadersbutton);
            this.Controls.Remove(inputbox);
            this.Controls.Remove(label1);         
            this.Controls.Remove(nameBox);
            this.Controls.Remove(inputbutton);
            this.Controls.Add(pictureBox2);
            for (int i = 0; i < people.Count; i++)
                if (nameBox.Text == people[i].name)
                {
                    find = true;
                    newk = i;
                    k = people[newk];
                }
            if (!find)
            {

                k.name = nameBox.Text;
                k.score = 0;
                people.Add(k);
            }
            playerCells = CreateField(200, true); //создаём ячейки игрока
            pcCells = CreateField(850, false);
            CreateShip(ref playerCells, 50); //создаём корабли
            CreateShip(ref pcCells, 897);
            Update();
        }

        private async void pictureBox2_MouseDown(object sender, MouseEventArgs e) 
        {
            if (!endGame) 
                foreach (var i in pcCells) 
                {
                    if (i.AttackCell(new Point(e.X, e.Y))) 
                    {
                        if (i.isShip )
                        {
                            AddEffect(i);
                            Update();
                        }
                        if (!i.isShip)
                        {
                            bool attack;
                            Update();
                            attack = PcAttack();
                            while (attack == true)
                            {
                                Update();
                                await Task.Delay(500);
                                attack = PcAttack();                                                              
                            }
                            Update();
                        }
                        return;
                    }
                }
        }
            void Update() //обновление кадра
        {
            Bitmap frame = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            Graphics g = Graphics.FromImage(frame);
            for (int i = 0; i < count;  i++)
                for (int j = 0; j < count; j++)
                {
                    pcCells[i, j].Draw(g);
                    playerCells[i, j].Draw(g);
                }
            if (effects.Count != 0)
                foreach (var i in effects)
                    i.Draw(g);
            if (CheckShips(pcCells, g))
            {
                this.Controls.Add(winpicture);
                this.Controls.Add(okbutton);
                winpicture.BringToFront();
                okbutton.BringToFront();
                k.score += 5;
                if (find)
                    people[newk] = k;
                else
                    people[people.Count - 1] = k;
                richTextBox1.Text = null;
                for (int i = 0; i < people.Count; i++)
                {
                    richTextBox1.AppendText(people[i].name);
                    for (int j = 0; j < 30 - people[i].name.Length; j++)
                        richTextBox1.AppendText(" ");
                    richTextBox1.AppendText(people[i].score + "\n");
                }
            }
            if (CheckShips(playerCells, g))
            {
                this.Controls.Add(defeatpicture);
                this.Controls.Add(okbutton);
                defeatpicture.BringToFront();
                okbutton.BringToFront();
                k.score -= 2;
                if (k.score < 0)
                    k.score = 0;
                if (find)
                    people[newk] = k;
                else
                    people[people.Count - 1] = k;
                richTextBox1.Text = null;
                for (int i = 0; i < people.Count; i++) {
                    richTextBox1.AppendText(people[i].name);
                    for (int j = 0; j < 30-people[i].name.Length; j++)
                        richTextBox1.AppendText(" ");
                    richTextBox1.AppendText(people[i].score + "\n");
                        }
            }
            pictureBox2.BackgroundImage = frame;
        }
        Cell[,] CreateField(int offset, bool player)
        {
            int cellSize = 50; //размер ячейки
            Cell[,] cells = new Cell[count, count];
            for (int i = 0; i < count; i++)
                for (int j = 0; j < count; j++)
                {
                    cells[i, j] = new Cell(new Point(offset + (cellSize * i), 200 + (cellSize * j)), cellSize, player); //инициализируем
                }
            return cells;
        }

        void CreateShip(ref Cell[,] cells, int offset) //создаём корабли
        {
            for (int i = 0; i < count; i++)
            {
                Random random = new Random(DateTime.Now.Millisecond - offset); //создаём рандом (если без offset то позиции кораблей могут совпадать)
                int x, y;
                do
                {
                    x = random.Next(0, count);
                    y = random.Next(0, count);
                } while (cells[x, y].isShip);
                cells[x, y].SetShip(true); //устанавливаем корабль в незанятую ячейку
            }

        }
        bool PcAttack()//атака компьютера
        {
            Random random = new Random();
            int x, y;
            Cell cell;
            do
            {
                x = random.Next(0, count); //находим случайные ячейки
                y = random.Next(0, count);
            } while ((cell = playerCells[x, y]).isBreaking); //ячейки должны быть не сломаны
            cell.AttackCell(); //помечаем ячейку
            if (cell.isShip)
            { //если попали в корабль, отображаем эффект взрыва
                AddEffect(cell);               
                return true;
            }
            return false;

        }
        bool CheckShips(Cell[,] cells, Graphics g) //проверка на наличие кораблей
        {
            foreach (var i in cells)
                if (i.isShip && !i.isBreaking) //если в ячейки корабль и по нему не попали
                {
                    return false; //то перестаём проверять       
                }
            
            endGame = true; //останавливаем игру
            return true;
        }
        void AddEffect(Cell cell) => effects.Add(new Effect(new Point(cell.X, cell.Y), new Point(50, 50)));

        private void playbutton_MouseEnter(object sender, EventArgs e)
        {
            button1.BackgroundImage = Image.FromFile("playbutton1.png");
        }
        private void playbutton_MouseLeave(object sender, EventArgs e)
        {
            button1.BackgroundImage = Image.FromFile("playbutton.png");
        }
        private void playbutton_MouseDown(object sender, MouseEventArgs e)
        {
            button1.BackgroundImage = Image.FromFile("playbutton2.png");
        }

        private void playbutton_MouseUp(object sender, MouseEventArgs e)
        {
            button1.BackgroundImage = Image.FromFile("playbutton1.png");
        }
        private void exitbutton_Click(object sender, EventArgs e)
        {           
            this.Close();
        }
        private void exitbutton_MouseDown(object sender, MouseEventArgs e)
        {
            exitbutton.BackgroundImage = Image.FromFile("exitbutton2.png");
        }

        private void exitbutton_MouseEnter(object sender, EventArgs e)
        {
            exitbutton.BackgroundImage = Image.FromFile("exitbutton1.png");
        }

        private void exitbutton_MouseLeave(object sender, EventArgs e)
        {
            exitbutton.BackgroundImage = Image.FromFile("exitbutton.png");
        }

        private void exitbutton_MouseUp(object sender, MouseEventArgs e)
        {
            exitbutton.BackgroundImage = Image.FromFile("exitbutton1.png");
        }

        private void leadersbutton_MouseDown(object sender, MouseEventArgs e)
        {
            leadersbutton.BackgroundImage = Image.FromFile("leadersbutton2.png");
        }

        private void leadersbutton_MouseEnter(object sender, EventArgs e)
        {
            leadersbutton.BackgroundImage = Image.FromFile("leadersbutton1.png");
        }

        private void leadersbutton_MouseLeave(object sender, EventArgs e)
        {
            leadersbutton.BackgroundImage = Image.FromFile("leadersbutton.png");
        }

        private void leadersbutton_MouseUp(object sender, MouseEventArgs e)
        {
            leadersbutton.BackgroundImage = Image.FromFile("leadersbutton1.png");
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void okbutton_MouseDown(object sender, MouseEventArgs e)
        {
            okbutton.BackgroundImage = Image.FromFile("okbutton2.png");
        }

        private void okbutton_MouseEnter(object sender, EventArgs e)
        {
            okbutton.BackgroundImage = Image.FromFile("okbutton1.png");
        }

        private void okbutton_MouseLeave(object sender, EventArgs e)
        {
            okbutton.BackgroundImage = Image.FromFile("okbutton.png");
        }

        private void okbutton_MouseUp(object sender, MouseEventArgs e)
        {
            okbutton.BackgroundImage = Image.FromFile("okbutton1.png");
        }

        private void okbutton_Click(object sender, EventArgs e)
        {
            this.BackgroundImage = Image.FromFile("фон21.png");
            endGame = false;
            this.Controls.Add(button1);
            this.Controls.Add(exitbutton);
            this.Controls.Add(leadersbutton);
            this.Controls.Remove(pictureBox2);
            this.Controls.Remove(okbutton);
            this.Controls.Remove(winpicture);
            this.Controls.Remove(defeatpicture);
            this.Controls.Remove(richTextBox1);
            leadersbutton.Enabled = true;
            exitbutton.Enabled = true;
            effects = null;
            using (BinaryWriter sr = new BinaryWriter(File.Open(filename, FileMode.Create)))
            {
                foreach (elem c in people)
                {
                    sr.Write(c.name);
                    sr.Write(c.score);
                }
                sr.Close();
            }
            nameBox.Text = String.Empty;
        }

        private void nameBox_TextChanged(object sender, EventArgs e)
        {
            if (nameBox.Text.Length == 0)
                inputbutton.Enabled = false;
            else
                inputbutton.Enabled = true;
 
        }

        private void leadersbutton_Click(object sender, EventArgs e)
        {
            this.Controls.Remove(button1);
            this.Controls.Remove(exitbutton);
            this.Controls.Remove(leadersbutton);
            this.Controls.Remove(inputbox);
            this.Controls.Remove(label1);
            this.Controls.Remove(nameBox);
            this.Controls.Remove(inputbutton);
            this.Controls.Add(richTextBox1);
            this.Controls.Add(okbutton);
            richTextBox1.ReadOnly = true;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            //for (int i = 0; i < people.Count; i++)
                //{
                //}
        }
    }
}
