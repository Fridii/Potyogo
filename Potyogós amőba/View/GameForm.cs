using Microsoft.VisualBasic;
using Potyogós_amőba.Model;
using Potyogós_amőba.Persistence;

namespace Potyogós_amőba
{
    public partial class GameForm : Form
    {
        #region Adattagok
        private Int32 meret; //a tabla merete
        private Button[] dobalo = null!;
        private TextBox[,] palya = null!;
        private PotyogosGameModel _gameModel;
        private Label[] jatekosokIdeje = null!;
        #endregion

        #region Constructor
        public GameForm()
        {
            meret = 0;
            InitializeComponent();
            _gameModel = new PotyogosGameModel(new PotyogosFileDataAccess(), new PotygosTimerAggregation());
            _gameModel.GameRefresh += new EventHandler<PotyogosEventArgs>(Game_GameRefresh);
            _gameModel.FieldChanged += new EventHandler<PotyogosFieldEventArgs>(Game_FieldChanged);
            _gameModel.GameOver += new EventHandler<PotyogosEventArgs>(Game_GameOver);

            játékMentéséreToolStripMenuItem.Enabled = false;
            játékSzüneteltetéséreToolStripMenuItem.Enabled = false;
        }
        #endregion

        private void játékSzüneteltetéséreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_gameModel == null)  // még nincs játék
                return;

            var menu = sender as ToolStripMenuItem;
            if (menu == null) return;

            // ha még nem volt beállítva semmi
            if (menu.Tag == null)
                menu.Tag = false; // false = nem szünetel, true = szünetel

            bool isPaused = (bool)menu.Tag;

            if (!isPaused)
            {
                // játék fut → most állítsuk le
                _gameModel.PauseGame();
                menu.Text = "Folytatas";
                menu.Tag = true;
            }
            else
            {
                // játék szünetel → most indítsuk újra
                _gameModel.ResumeGame();
                menu.Text = "Játék szüneteltetése";
                menu.Tag = false;
            }
        }

        private void Game_GameOver(Object? sender, PotyogosEventArgs e)
        {
            foreach (Button button in dobalo)
                button.Enabled = false;

            játékMentéséreToolStripMenuItem.Enabled = false;

            if (e.IsWon)
            {
                // 🔥 nyerő mezők megjelölése
                foreach (var (x, y) in e.WinningCells)
                {
                    palya[x, y].BackColor = Color.LightCoral;
                }

                MessageBox.Show(
                    $"Gratulálok, győztél!\nJátékos: {(e.CurrantPlayer == Mezo.JatekosX ? "X" : "O")}",
                    "Potyogós amőba",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk
                );
            }
            else
            {
                MessageBox.Show("Döntetlen! Senki sem nyert.",
                    "Potyogós amőba",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }


        private void újJátékToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (palya != null)
                {
                    foreach (var button in palya)
                    {
                        this.Controls.Remove(button);
                        button.Dispose();
                    }
                    palya = null!;
                }
                if (dobalo != null)
                {
                    foreach (var gomb in dobalo)
                    {
                        this.Controls.Remove(gomb);
                        gomb.Dispose();
                    }
                }
                string valasz = Interaction.InputBox("give me the size of table:", "New game", "");
                meret = Int32.Parse(valasz);
                if (meret > 18 || meret < 4) { throw new ArgumentException("Your number is to big"); }

                if (jatekosokIdeje != null)
                {
                    for (int i = 0; i < jatekosokIdeje.Length; i++)
                    {
                        if (jatekosokIdeje[i] != null)
                        {
                            this.Controls.Remove(jatekosokIdeje[i]);
                            jatekosokIdeje[i].Dispose();
                        }
                    }
                }
                GenerateTable();
                GernerateLabel();
                _gameModel.NewGame(meret);

                játékMentéséreToolStripMenuItem.Enabled = true;
                játékSzüneteltetéséreToolStripMenuItem.Enabled = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Your input is not a real number!\nPlease correct!", "Calculation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }
        private void RefreshBoard()
        {
            for (int i = 0; i < meret; i++)
            {
                for (int j = 0; j < meret; j++)
                {
                    Mezo ertek = _gameModel.GetValue(i, j);
                    palya[i, j].Text = ertek == Mezo.JatekosX ? "X" :
                                       ertek == Mezo.JatekosO ? "O" : "";
                }
            }

            // időcímkék frissítése (ha vannak)
            jatekosokIdeje[0].Text = "X: ";
            jatekosokIdeje[1].Text = "O: ";

        }


        /// <summary>
        /// Új tábla létrehozása.
        /// </summary>
        private void GenerateTable()
        {
            // a dobalo letrehozasa
            dobalo = new Button[meret];
            for (Int32 i = 0; i < meret; i++)
            {
                dobalo[i] = new Button();
                dobalo[i].Location = new Point(5 + 50 * i, 35);
                dobalo[i].Size = new Size(50, 50);
                dobalo[i].Font = new Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold);
                dobalo[i].TabIndex = i;
                dobalo[i].FlatStyle = FlatStyle.Flat;
                dobalo[i].MouseClick += new MouseEventHandler(Dobalo_Click);

                Controls.Add(dobalo[i]);
            }

            palya = new TextBox[meret, meret];
            for (Int32 i = 0; i < meret; i++)
            {
                for (Int32 j = 0; j < meret; j++)
                {
                    palya[i, j] = new TextBox();
                    palya[i, j].Location = new Point(5 + 50 * j, 85 + 50 * i);
                    palya[i, j].Size = new Size(50, 50);
                    palya[i, j].Enabled = false;
                    palya[i, j].BackColor = Color.LightBlue;
                    palya[i, j].Font = new Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold);
                    palya[i, j].TabIndex = meret * i + j + meret;
                    palya[i, j].MouseClick += new MouseEventHandler(Dobalo_Click);

                    Controls.Add(palya[i, j]);
                }
            }
        }
        private void GernerateLabel()
        {
            jatekosokIdeje = new Label[2];
            for (int i = 0; i < 2; i++)
            {
                jatekosokIdeje[i] = new Label();
                jatekosokIdeje[i].Location = new Point(50 + 50 * meret, 50 + 100 * i);
                jatekosokIdeje[i].Size = new Size(300, 50);
                jatekosokIdeje[i].Font = new Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold);
                jatekosokIdeje[i].TabIndex = 10000 + i;

                Controls.Add(jatekosokIdeje[i]);
            }
            jatekosokIdeje[0].Text = "X: ";
            jatekosokIdeje[1].Text = "O: ";

        }
        private void Game_FieldChanged(Object? sender, PotyogosFieldEventArgs e)
        {
            palya[e.X, e.Y].Text = e.jatekos == Mezo.JatekosX ? "X" : "O";
        }
        private void Game_GameRefresh(Object? sender, PotyogosEventArgs e)
        {
            if (InvokeRequired)//NEM vagyunk  a formnak a szálán->true
            {
                BeginInvoke(() => Game_GameRefresh(sender, e));//ujrainditja a form szálán
                return;
            }
            //idő felirat frissités
            int curentP = e.CurrantPlayer == Mezo.JatekosX ? 0 : 1;
            jatekosokIdeje[curentP].Text = jatekosokIdeje[curentP].Text.Substring(0, 3)
                + TimeSpan.FromSeconds(e.GameTime).ToString(@"mm\:ss");

            jatekosokIdeje[curentP].BackColor = Color.HotPink;//aktuális játékos
            jatekosokIdeje[e.CurrantPlayer == Mezo.JatekosX ? 1 : 0].BackColor = SystemColors.Control;//másik játékos

        }

        private void Dobalo_Click(object? sender, MouseEventArgs e)
        {
            int oszlop = (sender as Button)?.TabIndex ?? 0;
            _gameModel.DropToken(oszlop);
        }

        private void GameForm_Load(object sender, EventArgs e)
        {
            // amikor elindul az alkalmazás, még nincs játék
            játékMentéséreToolStripMenuItem.Enabled = false;
            játékSzüneteltetéséreToolStripMenuItem.Enabled = false;
        }

        private async void játékMentéséreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_gameModel == null) return;

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Potyogós save files (*.pga)|*.pga|All files (*.*)|*.*";
                saveFileDialog.Title = "Save Game";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        await _gameModel.SaveGameAsync(saveFileDialog.FileName);
                        MessageBox.Show("Game saved successfully!", "Potyogós Amőba", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        MessageBox.Show("Error while saving the game!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private  async void betöltéséreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Potyogós save files (*.pga)|*.pga|All files (*.*)|*.*";
                openFileDialog.Title = "Load Game";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        await _gameModel.LoadGameAsync(openFileDialog.FileName);
                        meret = _gameModel.TablaMeret;

                        // új tábla felrajzolása
                        if (palya != null)
                        {
                            foreach (var box in palya)
                            {
                                this.Controls.Remove(box);
                                box.Dispose();
                            }
                        }
                        if (dobalo != null)
                        {
                            foreach (var button in dobalo)
                            {
                                this.Controls.Remove(button);
                                button.Dispose();
                            }
                        }
                        if (jatekosokIdeje != null)
                        {
                            for (int i = 0; i < jatekosokIdeje.Length; i++)
                            {
                                if (jatekosokIdeje[i] != null)
                                {
                                    this.Controls.Remove(jatekosokIdeje[i]);
                                    jatekosokIdeje[i].Dispose();
                                }
                            }
                        }
                        GenerateTable();
                        GernerateLabel();
                        RefreshBoard();

                        játékMentéséreToolStripMenuItem.Enabled = true;
                        játékSzüneteltetéséreToolStripMenuItem.Enabled = true;

                        MessageBox.Show("Game loaded successfully!", "Potyogós Amőba", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        MessageBox.Show("Error while loading the game!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
