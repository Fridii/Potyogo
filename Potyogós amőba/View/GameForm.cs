using Microsoft.VisualBasic;
using Potyogós_amőba.Model;
using Potyogós_amőba.Persistence;

namespace Potyogós_amőba
{
    public partial class GameForm : Form
    {
        #region Fields
        private Int32 _boardSize; //a tabla merete
        private Button[] _dropButtons = null!;
        private TextBox[,] _board = null!;
        private PotyogosGameModel _gameModel;
        private Label[] _playerTimers = null!;
        #endregion

        #region Constructor
        public GameForm()
        {
            _boardSize = 0;
            InitializeComponent();
            _gameModel = new PotyogosGameModel(new PotyogosFileDataAccess(), new PotygosTimerAggregation());
            _gameModel.GameRefresh += new EventHandler<PotyogosEventArgs>(Game_GameRefresh);
            _gameModel.FieldChanged += new EventHandler<PotyogosFieldEventArgs>(Game_FieldChanged);
            _gameModel.GameOver += new EventHandler<PotyogosEventArgs>(Game_GameOver);

            saveGameToolStripMenuItem.Enabled = false;
            pauseGameToolStripMenuItem.Enabled = false;
        }
        #endregion

        private void pauseGameToolStripMenuItem_Click(object sender, EventArgs e)
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
                menu.Text = "Folytatás";
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
            foreach (Button button in _dropButtons)
                button.Enabled = false;

            saveGameToolStripMenuItem.Enabled = false;

            if (e.IsWon)
            {
                // nyerő mezők megjelölése
                foreach (var (x, y) in e.WinningCells)
                {
                    _board[x, y].BackColor = Color.LightCoral;
                }

                MessageBox.Show(
                    $"Gratulálok, győztél!\nJátékos: {(e.CurrantPlayer == Mezo.PlayerX ? "X" : "O")}",
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


        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (_board != null)
                {
                    foreach (var button in _board)
                    {
                        this.Controls.Remove(button);
                        button.Dispose();
                    }
                    _board = null!;
                }
                if (_dropButtons != null)
                {
                    foreach (var gomb in _dropButtons)
                    {
                        this.Controls.Remove(gomb);
                        gomb.Dispose();
                    }
                }
                string valasz = Interaction.InputBox("give me the size of table:", "New game", "");
                _boardSize = Int32.Parse(valasz);
                if (_boardSize > 18 || _boardSize < 4) { throw new ArgumentException("Your number is to big"); }

                if (_playerTimers != null)
                {
                    for (int i = 0; i < _playerTimers.Length; i++)
                    {
                        if (_playerTimers[i] != null)
                        {
                            this.Controls.Remove(_playerTimers[i]);
                            _playerTimers[i].Dispose();
                        }
                    }
                }
                GenerateTable();
                GernerateLabel();
                _gameModel.NewGame(_boardSize);

                saveGameToolStripMenuItem.Enabled = true;
                pauseGameToolStripMenuItem.Enabled = true;
            }
            catch (Exception)
            {
                MessageBox.Show("Your input is not a real number!\nPlease correct!", "Calculation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }
        private void RefreshBoard()
        {
            for (int i = 0; i < _boardSize; i++)
            {
                for (int j = 0; j < _boardSize; j++)
                {
                    Mezo ertek = _gameModel.GetValue(i, j);
                    _board[i, j].Text = ertek == Mezo.PlayerX ? "X" :
                                       ertek == Mezo.PlayerO ? "O" : "";
                }
            }

            // időcímkék frissítése (ha vannak)
            _playerTimers[0].Text = "X: ";
            _playerTimers[1].Text = "O: ";

        }


        /// <summary>
        /// Új tábla létrehozása.
        /// </summary>
        private void GenerateTable()
        {
            // a dobalo letrehozasa
            _dropButtons = new Button[_boardSize];
            for (Int32 i = 0; i < _boardSize; i++)
            {
                _dropButtons[i] = new Button();
                _dropButtons[i].Location = new Point(5 + 50 * i, 35);
                _dropButtons[i].Size = new Size(50, 50);
                _dropButtons[i].Font = new Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold);
                _dropButtons[i].TabIndex = i;
                _dropButtons[i].FlatStyle = FlatStyle.Flat;
                _dropButtons[i].MouseClick += new MouseEventHandler(dropButtons_Click);

                Controls.Add(_dropButtons[i]);
            }

            _board = new TextBox[_boardSize, _boardSize];
            for (Int32 i = 0; i < _boardSize; i++)
            {
                for (Int32 j = 0; j < _boardSize; j++)
                {
                    _board[i, j] = new TextBox();
                    _board[i, j].Location = new Point(5 + 50 * j, 85 + 50 * i);
                    _board[i, j].Size = new Size(50, 50);
                    _board[i, j].Enabled = false;
                    _board[i, j].BackColor = Color.LightBlue;
                    _board[i, j].Font = new Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold);
                    _board[i, j].TabIndex = _boardSize * i + j + _boardSize;
                    _board[i, j].MouseClick += new MouseEventHandler(dropButtons_Click);

                    Controls.Add(_board[i, j]);
                }
            }
        }
        private void GernerateLabel()
        {
            _playerTimers = new Label[2];
            for (int i = 0; i < 2; i++)
            {
                _playerTimers[i] = new Label();
                _playerTimers[i].Location = new Point(50 + 50 * _boardSize, 50 + 100 * i);
                _playerTimers[i].Size = new Size(300, 50);
                _playerTimers[i].Font = new Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold);
                _playerTimers[i].TabIndex = 10000 + i;

                Controls.Add(_playerTimers[i]);
            }
            _playerTimers[0].Text = "X: ";
            _playerTimers[1].Text = "O: ";

        }
        private void Game_FieldChanged(Object? sender, PotyogosFieldEventArgs e)
        {
            _board[e.X, e.Y].Text = e.jatekos == Mezo.PlayerX ? "X" : "O";
        }
        private void Game_GameRefresh(Object? sender, PotyogosEventArgs e)
        {
            if (InvokeRequired)//NEM vagyunk  a formnak a szálán->true
            {
                BeginInvoke(() => Game_GameRefresh(sender, e));//ujrainditja a form szálán
                return;
            }
            //idő felirat frissités
            int curentP = e.CurrantPlayer == Mezo.PlayerX ? 0 : 1;
            _playerTimers[curentP].Text = _playerTimers[curentP].Text.Substring(0, 3)
                + TimeSpan.FromSeconds(e.GameTime).ToString(@"mm\:ss");

            _playerTimers[curentP].BackColor = Color.HotPink;//aktuális játékos
            _playerTimers[e.CurrantPlayer == Mezo.PlayerX ? 1 : 0].BackColor = SystemColors.Control;//másik játékos

        }

        private void dropButtons_Click(object? sender, MouseEventArgs e)
        {
            int oszlop = (sender as Button)?.TabIndex ?? 0;
            _gameModel.DropToken(oszlop);
        }

        private void GameForm_Load(object sender, EventArgs e)
        {
            // amikor elindul az alkalmazás, még nincs játék
            saveGameToolStripMenuItem.Enabled = false;
            pauseGameToolStripMenuItem.Enabled = false;
        }

        private async void saveGameToolStripMenuItem_Click(object sender, EventArgs e)
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

        private  async void loadGameToolStripMenuItem_Click(object sender, EventArgs e)
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
                        _boardSize = _gameModel.TablaMeret;

                        // új tábla felrajzolása
                        if (_board != null)
                        {
                            foreach (var box in _board)
                            {
                                this.Controls.Remove(box);
                                box.Dispose();
                            }
                        }
                        if (_dropButtons != null)
                        {
                            foreach (var button in _dropButtons)
                            {
                                this.Controls.Remove(button);
                                button.Dispose();
                            }
                        }
                        if (_playerTimers != null)
                        {
                            for (int i = 0; i < _playerTimers.Length; i++)
                            {
                                if (_playerTimers[i] != null)
                                {
                                    this.Controls.Remove(_playerTimers[i]);
                                    _playerTimers[i].Dispose();
                                }
                            }
                        }
                        GenerateTable();
                        GernerateLabel();
                        RefreshBoard();

                        saveGameToolStripMenuItem.Enabled = true;
                        pauseGameToolStripMenuItem.Enabled = true;

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
