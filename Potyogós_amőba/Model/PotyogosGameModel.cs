using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Potyogós_amőba.Persistence;

namespace Potyogós_amőba.Model
{
    /// <summary>
    /// A Potyogós amőba játék modellje, amely kezeli a játék logikáját, időzítést és eseményeket.
    /// </summary>
    public class PotyogosGameModel
    {
        #region Fields

        private IPotyogosDataAccess _dataAccess;
        private PotyogosTable _table = null!;
        private Field _currentPlayer;
        private ITimer _timer;
        private int _PlayerTimeX;
        private int _PlayerTimeO;
        private int _steps;

        #endregion

        #region Events

        /// <summary>
        /// Esemény, amely akkor hívódik meg, ha egy mező megváltozik.
        /// </summary>
        public event EventHandler<PotyogosFieldEventArgs>? FieldChanged;

        /// <summary>
        /// Esemény, amely a játék frissítésekor hívódik meg (pl. idő változása).
        /// </summary>
        public event EventHandler<PotyogosEventArgs>? GameRefresh;

        /// <summary>
        /// Esemény, amely a játék végén hívódik meg.
        /// </summary>
        public event EventHandler<PotyogosEventArgs>? GameOver;

        #endregion

        #region Properties

        /// <summary>
        /// A tábla méretét adja vissza.
        /// </summary>
        public Int32 TableSize => _table.Size;

        /// <summary>
        /// Indexelő, amely visszaadja az adott mező értékét.
        /// </summary>
        /// <param name="x">Sor index.</param>
        /// <param name="y">Oszlop index.</param>
        public Field this[int x, int y] => _table[x, y];

        /// <summary>
        /// Igaz, ha a játék véget ért.
        /// </summary>
        public bool isGameOver => _table.IsFilled;

        /// <summary>
        /// X játékos által eltöltött játékidő.
        /// </summary>
        public int PlayerTimeX => _PlayerTimeX;

        /// <summary>
        /// O játékos által eltöltött játékidő.
        /// </summary>
        public int PlayerTimeO => _PlayerTimeO;

        #endregion

        #region Constructor

        /// <summary>
        /// A PotyogosGameModel példányosítása.
        /// </summary>
        /// <param name="dataAccess">Adatkezelő interfész.</param>
        /// <param name="timer">Időzítő interfész.</param>
        public PotyogosGameModel(IPotyogosDataAccess dataAccess, ITimer timer)
        {
            _dataAccess = dataAccess;
            _currentPlayer = Field.PlayerX;
            _timer = timer;

            _timer.Interval = 1000;
            _timer.Elapsed += new EventHandler(Timer_Elapsed);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Játék szüneteltetése.
        /// </summary>
        public void PauseGame()
        {
            _timer.Stop();
        }

        /// <summary>
        /// Játék folytatása, ha az még nem ért véget.
        /// </summary>
        public void ResumeGame()
        {
            if (!_table.IsFilled && !isGameOver)
                _timer.Start();
        }

        /// <summary>
        /// Új játék indítása adott méretű táblán.
        /// </summary>
        /// <param name="meret">A tábla mérete.</param>
        public void NewGame(int meret)
        {
            _table = new PotyogosTable(meret);
            _currentPlayer = Field.PlayerX;
            _steps = 0;
            _PlayerTimeX = 0;
            _PlayerTimeO = 0;
            _timer.Start();
        }

        /// <summary>
        /// Korong bedobása a megadott oszlopba.
        /// </summary>
        /// <param name="oszlop">Az oszlop indexe, ahova a bábu kerül.</param>
        public void DropToken(Int32 oszlop)
        {
            if (isGameOver)
                return;

            try
            {
                Int32 sor = _table.EmptySpot(oszlop);
                if (sor == -1)
                    return; // ha tele van az oszlop

                _table.AddElement(oszlop, _currentPlayer);
                _steps++;

                OnFieldChanged(sor, oszlop, _currentPlayer);

                var winningCells = CheckWin(sor, oszlop);
                if (winningCells.Count >= 4)
                {
                    StopTimers();
                    GameOver?.Invoke(this, new PotyogosEventArgs(
                        true,
                        _currentPlayer,
                        _currentPlayer == Field.PlayerX ? _PlayerTimeX : _PlayerTimeO,
                        winningCells
                    ));
                    return;
                }

                SwitchPlayer();
            }
            catch (Exception)
            {
                throw new Exception("Hiba a lépés során.");
            }
        }

        /// <summary>
        /// Játék mentése aszinkron módon.
        /// </summary>
        /// <param name="path">A fájl elérési útja.</param>
        public async Task SaveGameAsync(string path)
        {
            await _dataAccess.SaveAsync(path, _table, _PlayerTimeX, _PlayerTimeO);
        }

        /// <summary>
        /// Játék betöltése aszinkron módon.
        /// </summary>
        /// <param name="path">A mentett fájl elérési útja.</param>
        public async Task LoadGameAsync(string path)
        {
            _table = await _dataAccess.LoadAsync(path);
            _currentPlayer = Field.PlayerX;
            _steps = 0;
            _PlayerTimeX = 0;
            _PlayerTimeO = 0;
            _timer.Start();
        }

        /// <summary>
        /// Tábla és játékidők betöltése, majd a játék frissítése.
        /// </summary>
        /// <param name="table">A játék tábla objektum.</param>
        /// <param name="jatekIdoX">X játékos ideje.</param>
        /// <param name="jatekIdoO">O játékos ideje.</param>
        public void LoadTableAndTimes(PotyogosTable table, int jatekIdoX, int jatekIdoO)
        {
            _table = table;
            _PlayerTimeX = jatekIdoX;
            _PlayerTimeO = jatekIdoO;
            _currentPlayer = Field.PlayerX;
            _timer.Start();
            OnGameRefresh();
        }

        /// <summary>
        /// Mező értékének lekérdezése.
        /// </summary>
        /// <param name="x">Vízszintes koordináta.</param>
        /// <param name="y">Függőleges koordináta.</param>
        /// <returns>Mező értéke.</returns>
        public Field GetValue(Int32 x, Int32 y) => _table.GetValue(x, y);

        /// <summary>
        /// Ellenőrzi, hogy a megadott mező üres-e.
        /// </summary>
        /// <param name="x">Vízszintes koordináta.</param>
        /// <param name="y">Függőleges koordináta.</param>
        /// <returns>Igaz, ha a mező üres.</returns>
        public Boolean IsEmpty(Int32 x, Int32 y) => _table.IsEmpty(x, y);

        #endregion

        #region Private methods

        /// <summary>
        /// Játékos váltása.
        /// </summary>
        private void SwitchPlayer()
        {
            if (_currentPlayer == Field.PlayerX)
            {
                _currentPlayer = Field.PlayerO;
            }
            else
            {
                _currentPlayer = Field.PlayerX;
            }
        }

        /// <summary>
        /// Időzítők leállítása.
        /// </summary>
        private void StopTimers()
        {
            _timer.Stop();
        }

        /// <summary>
        /// Segédfüggvény vízszintes irányú győzelem ellenőrzésére.
        /// </summary>
        private bool helpWinRow(int kezdet, int veg, int sor)
        {
            int count = 0;
            for (int i = kezdet; i <= veg; i++)
            {
                if (_table[sor, i] != _currentPlayer)
                {
                    count = 0;
                }
                else
                {
                    count++;
                    if (count == 4) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Segédfüggvény függőleges irányú győzelem ellenőrzésére.
        /// </summary>
        private bool helpWinColumn(int kezdet, int veg, int oszlop)
        {
            int szamlalo = 0;
            for (int i = kezdet; i <= veg; i++)
            {
                if (_table[i, oszlop] != _currentPlayer)
                {
                    szamlalo = 0;
                }
                else
                {
                    szamlalo++;
                    if (szamlalo == 4) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Segédfüggvény főátlón való győzelem ellenőrzésére.
        /// </summary>
        private bool helpWinMainDiagonal(int startSor, int startOszlop, int endSor, int endOszlop)
        {
            int count = 1;
            for (int i = 1; startSor + i <= endSor && startOszlop + i <= endOszlop; i++)
            {
                if (_table[startSor + i, startOszlop + i] == _table[startSor + i - 1, startOszlop + i - 1])
                {
                    count++;
                    if (count == 4) return true;
                }
                else
                {
                    count = 1;
                }
            }
            return false;
        }

        /// <summary>
        /// Segédfüggvény mellékátlón való győzelem ellenőrzésére.
        /// </summary>
        private bool helpWinSideDiagonal(int startSor, int startOszlop, int endSor, int endOszlop)
        {
            int count = 1;
            for (int i = 1; startSor + i <= endSor && startOszlop - i >= endOszlop; i++)
            {
                if (_table[startSor + i, startOszlop - i] == _table[startSor + i - 1, startOszlop - i + 1])
                {
                    count++;
                    if (count == 4) return true;
                }
                else
                {
                    count = 1;
                }
            }
            return false;
        }

        /// <summary>
        /// Ellenőrzi, hogy a legutóbbi lépés nyerést eredményezett-e.
        /// </summary>
        private List<(int X, int Y)> CheckWin(int sor, int oszlop)
        {
            var directions = new (int dSor, int dOszlop)[] {
                (0, 1),
                (1, 0),
                (1, 1),
                (1, -1)
            };

            foreach (var (dSor, dOszlop) in directions)
            {
                var cells = CheckDirectionCoordinate(sor, oszlop, dSor, dOszlop);
                if (cells.Count >= 4)
                    return cells;
            }

            return new List<(int X, int Y)>();
        }

        /// <summary>
        /// Egy irányban ellenőrzi az egymás utáni azonos bábu-kat.
        /// </summary>
        private List<(int X, int Y)> CheckDirectionCoordinate(int sor, int oszlop, int dSor, int dOszlop)
        {
            List<(int X, int Y)> cells = new() { (sor, oszlop) };
            cells.AddRange(GetCellsInDirection(sor, oszlop, dSor, dOszlop));
            cells.AddRange(GetCellsInDirection(sor, oszlop, -dSor, -dOszlop));
            return cells;
        }

        /// <summary>
        /// Visszaadja az azonos bábu-kat egy adott irányban.
        /// </summary>
        private List<(int X, int Y)> GetCellsInDirection(int sor, int oszlop, int dSor, int dOszlop)
        {
            List<(int X, int Y)> cells = new();
            int ujSor = sor + dSor;
            int ujOszlop = oszlop + dOszlop;

            while (ujSor >= 0 && ujSor < _table.Size &&
                   ujOszlop >= 0 && ujOszlop < _table.Size &&
                   _table[ujSor, ujOszlop] == _currentPlayer)
            {
                cells.Add((ujSor, ujOszlop));
                ujSor += dSor;
                ujOszlop += dOszlop;
            }

            return cells;
        }

        #endregion

        #region Private event methods

        /// <summary>
        /// Játék frissítése eseménykiváltáskor.
        /// </summary>
        private void OnGameRefresh()
        {
            GameRefresh?.Invoke(this, new PotyogosEventArgs(false, _currentPlayer, _currentPlayer == Field.PlayerX ? _PlayerTimeX : _PlayerTimeO));
        }

        /// <summary>
        /// Mező módosulása esetén eseménykiváltás.
        /// </summary>
        private void OnFieldChanged(Int32 x, Int32 y, Field jatekos)
        {
            FieldChanged?.Invoke(this, new PotyogosFieldEventArgs(x, y, jatekos));
        }

        /// <summary>
        /// Időzítő eseménykezelője – növeli az aktuális játékos idejét.
        /// </summary>
        private void Timer_Elapsed(Object? sender, EventArgs e)
        {
            if (isGameOver)
                return;

            if (_currentPlayer == Field.PlayerO)
                _PlayerTimeO++;
            else
                _PlayerTimeX++;

            OnGameRefresh();
        }

        #endregion
    }
}
