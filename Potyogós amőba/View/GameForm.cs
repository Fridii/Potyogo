using Microsoft.VisualBasic;
using Potyogós_amőba.Model;
using Potyogós_amőba.Persistence;

namespace Potyogós_amőba
{
    /// <summary>
    /// A játék főablakát megvalósító osztály.
    /// </summary>
    public partial class GameForm : Form
    {
        #region Fields
        private Int32 _boardSize; // a tábla mérete
        private Button[] _dropButtons = null!;
        private TextBox[,] _board = null!;
        private PotyogosGameModel _gameModel;
        private Label[] _playerTimers = null!;
        #endregion

        #region Constructor
        /// <summary>
        /// Inicializálja a <see cref="GameForm"/> új példányát.
        /// </summary>
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

        #region Menu event handlers
        /// <summary>
        /// A játék szüneteltetését vagy folytatását vezérlő menüpont eseménykezelője.
        /// </summary>
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

        /// <summary>
        /// Új játék indítását kezelő menüpont eseménykezelője.
        /// </summary>
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

                string valasz = Interaction.InputBox("Add meg a tábla méretét:", "Új játék", "");
                _boardSize = Int32.Parse(valasz);
                if (_boardSize > 18 || _boardSize < 4) { throw new ArgumentException("Túl kicsi vagy túl nagy méret!"); }

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
                MessageBox.Show("A megadott érték nem szám!\nKérlek, javítsd!", "Beviteli hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Játék mentésének menüpont eseménykezelője.
        /// </summary>
        private async void saveGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_gameModel == null) return;

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Potyogós mentések (*.pga)|*.pga|Minden fájl (*.*)|*.*";
                saveFileDialog.Title = "Játék mentése";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        await _gameModel.SaveGameAsync(saveFileDialog.FileName);
                        MessageBox.Show("A játék sikeresen elmentve!", "Potyogós amőba", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        MessageBox.Show("Hiba történt a játék mentése közben!", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Játék betöltésének menüpont eseménykezelője.
        /// </summary>
        private async void loadGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Potyogós mentések (*.pga)|*.pga|Minden fájl (*.*)|*.*";
                openFileDialog.Title = "Játék betöltése";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        await _gameModel.LoadGameAsync(openFileDialog.FileName);
                        _boardSize = _gameModel.TableSize;

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

                        MessageBox.Show("A játék sikeresen betöltve!", "Potyogós amőba", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        MessageBox.Show("Hiba történt a játék betöltése közben!", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Az űrlap betöltésekor fut le – alapértelmezetten letiltja a mentés és szünet gombokat.
        /// </summary>
        private void GameForm_Load(object sender, EventArgs e)
        {
            saveGameToolStripMenuItem.Enabled = false;
            pauseGameToolStripMenuItem.Enabled = false;
        }
        #endregion

        #region Private event methods
        /// <summary>
        /// A játék végének eseménykezelője.
        /// </summary>
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
                    $"Gratulálok, győzelem!\nJátékos: {(e.CurrantPlayer == Field.PlayerX ? "X" : "O")}",
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

        /// <summary>
        /// A játék mezőváltozás eseménykezelője.
        /// </summary>
        private void Game_FieldChanged(Object? sender, PotyogosFieldEventArgs e)
        {
            _board[e.X, e.Y].Text = e.player == Field.PlayerX ? "X" : "O";
        }

        /// <summary>
        /// A játék állapotfrissítés eseménykezelője.
        /// </summary>
        private void Game_GameRefresh(Object? sender, PotyogosEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => Game_GameRefresh(sender, e));
                return;
            }

            // idő felirat frissítés
            int curentP = e.CurrantPlayer == Field.PlayerX ? 0 : 1;
            _playerTimers[curentP].Text = _playerTimers[curentP].Text.Substring(0, 3)
                + TimeSpan.FromSeconds(e.GameTime).ToString(@"mm\:ss");

            _playerTimers[curentP].BackColor = Color.HotPink; // aktuális játékos
            _playerTimers[e.CurrantPlayer == Field.PlayerX ? 1 : 0].BackColor = SystemColors.Control; // másik játékos
        }

        /// <summary>
        /// A dobógombok kattintásának eseménykezelője.
        /// </summary>
        private void dropButtons_Click(object? sender, MouseEventArgs e)
        {
            int oszlop = (sender as Button)?.TabIndex ?? 0;
            _gameModel.DropToken(oszlop);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// A játékmező újrarajzolása az aktuális állapot alapján.
        /// </summary>
        private void RefreshBoard()
        {
            for (int i = 0; i < _boardSize; i++)
            {
                for (int j = 0; j < _boardSize; j++)
                {
                    Field ertek = _gameModel.GetValue(i, j);
                    _board[i, j].Text = ertek == Field.PlayerX ? "X" :
                                       ertek == Field.PlayerO ? "O" : "";
                }
            }

            // időcímkék frissítése (ha vannak)
            _playerTimers[0].Text = "X: ";
            _playerTimers[1].Text = "O: ";
        }

        /// <summary>
        /// Új tábla és dobógombok létrehozása.
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

        /// <summary>
        /// A játékosidőket megjelenítő címkék létrehozása.
        /// </summary>
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
            _playerTimers[0].Text = "X: "+TimeSpan.FromSeconds(0).ToString(@"mm\:ss");
            _playerTimers[1].Text = "O: " + TimeSpan.FromSeconds(0).ToString(@"mm\:ss");
        }
        #endregion
    }
}
