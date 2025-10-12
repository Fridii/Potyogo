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
    public class PotyogosGameModel
    {
        private IPotyogosDataAccess _dataAccess;
        private PotyogosTable _table = null!;
        private Field _currentPlayer;
        private ITimer _timer;
        private int _PlayerTimeX;
        private int _PlayerTimeO;
        private int _steps;

        public event EventHandler<PotyogosFieldEventArgs>? FieldChanged;
        public event EventHandler<PotyogosEventArgs>? GameRefresh;
        public event EventHandler<PotyogosEventArgs>? GameOver;


        public Int32 TableSize => _table.Size;
        public Field this[int x, int y] => _table[x, y];
        public bool isGameOver => _table.IsFilled ;//todo: ha valaki nyert
        public int PlayerTimeX => _PlayerTimeX;
        public int PlayerTimeO => _PlayerTimeO;


        public PotyogosGameModel(IPotyogosDataAccess dataAccess, ITimer timer)
        {
            _dataAccess = dataAccess;
            //_tabla = new PotyogosTable();
            _currentPlayer = Field.PlayerX;
            _timer = timer;

            _timer.Interval = 1000;
            _timer.Elapsed += new EventHandler(Timer_Elapsed);
        }
        public void PauseGame()
        {
            _timer.Stop();
        }

        public void ResumeGame()
        {
            if (!_table.IsFilled && !isGameOver)
                _timer.Start();
        }

        public void NewGame(int meret)
        {
            _table = new PotyogosTable(meret);
            _currentPlayer = Field.PlayerX;
            _steps = 0;
            _PlayerTimeX = 0;
            _PlayerTimeO = 0;
            _timer.Start();
        }

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
        // Egy oszlopba dobás logikája
        public void DropToken(Int32 oszlop)
        {
            if (isGameOver)
                return ;

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
        public async Task SaveGameAsync(string path)
        {
            await _dataAccess.SaveAsync(path, _table , _PlayerTimeX, _PlayerTimeO);
        }

        // Betöltés
        public async Task LoadGameAsync(string path)
        {
            _table = await _dataAccess.LoadAsync(path);
            _currentPlayer = Field.PlayerX;
            _steps = 0;
            _PlayerTimeX = 0;
            _PlayerTimeO = 0;
            _timer.Start();
        }
        public void LoadTableAndTimes(PotyogosTable table, int jatekIdoX, int jatekIdoO)
        {
            _table = table;
            _PlayerTimeX = jatekIdoX;
            _PlayerTimeO = jatekIdoO;
            _currentPlayer = Field.PlayerX;
            _timer.Start();

            // UI frissítéshez esemény kiváltása
            OnGameRefresh();
        }

        private void StopTimers()
        {
            _timer.Stop();
        }

        private bool helpWinRow(int kezdet, int veg, int sor)
        {
            int count = 0;
            for (int i = kezdet; i <= veg; i++)
            {
                if (_table[sor, i] != _currentPlayer) // Ha az érték nem egyezik az aktuális játékoséval
                {
                    count = 0; // Számláló visszaállítása
                }
                else
                {
                    count++; // Számláló növelése
                    if (count == 4) return true; // Ha 4 egymás után van, nyerés
                }
            }
            return false; // Ha nincs 4 egymás után
        }


        private bool helpWinColumn(int kezdet, int veg, int oszlop)
        {
            int szamlalo = 0;
            for (int i = kezdet; i <= veg; i++)
            {
                if (_table[i, oszlop] != _currentPlayer) // Ha az érték nem egyezik az aktuális játékoséval
                {
                    szamlalo = 0; // Számláló visszaállítása
                }
                else
                {
                    szamlalo++; // Számláló növelése
                    if (szamlalo == 4) return true; // Ha 4 egymás után van, nyerés
                }
            }
            return false; // Ha nincs 4 egymás után
        }

        // Segédfüggvény a főátló ellenőrzésére
        private bool helpWinMainDiagonal(int startSor, int startOszlop, int endSor, int endOszlop)
        {
            int count = 1; // Kezdőérték
            for (int i = 1; startSor + i <= endSor && startOszlop + i <= endOszlop; i++)
            {
                if (_table[startSor + i, startOszlop + i] == _table[startSor + i - 1, startOszlop + i - 1])
                {
                    count++;
                    if (count == 4) return true; // Ha 4 egymás után van, nyerés
                }
                else
                {
                    count = 1; // Reseteljük a számlálót, ha megszakad a sorozat
                }
            }
            return false;
        }

        // Segédfüggvény a mellékátló ellenőrzésére
        private bool helpWinSideDiagonal(int startSor, int startOszlop, int endSor, int endOszlop)
        {
            int count = 1; // Kezdőérték
            for (int i = 1; startSor + i <= endSor && startOszlop - i >= endOszlop; i++)
            {
                if (_table[startSor + i, startOszlop - i] == _table[startSor + i - 1, startOszlop - i + 1])
                {
                    count++;
                    if (count == 4) return true; // Ha 4 egymás után van, nyerés
                }
                else
                {
                    count = 1; // Reseteljük a számlálót, ha megszakad a sorozat
                }
            }
            return false;
        }
        private List<(int X, int Y)> CheckWin(int sor, int oszlop)
        {
            // négy irány
            var directions = new (int dSor, int dOszlop)[] {
                (0, 1),   // vízszintes →
                (1, 0),   // függőleges ↓
                (1, 1),   // főátló ↘
                (1, -1)   // mellékátló ↙
            };

            foreach (var (dSor, dOszlop) in directions)
            {
                var cells = CheckDirectionCoordinate(sor, oszlop, dSor, dOszlop);
                if (cells.Count >= 4)
                    return cells;
            }

            return new List<(int X, int Y)>(); // nincs nyerés
        }
        private List<(int X, int Y)> CheckDirectionCoordinate(int sor, int oszlop, int dSor, int dOszlop)
        {
            List<(int X, int Y)> cells = new() { (sor, oszlop) };

            // egyik irány
            cells.AddRange(GetCellsInDirection(sor, oszlop, dSor, dOszlop));

            // másik irány
            cells.AddRange(GetCellsInDirection(sor, oszlop, -dSor, -dOszlop));

            return cells;
        }

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

        // EZT A RESZT ITT NEM HASZNALOM
        private bool CheckWin7(int sor, int oszlop)
        {
           

            // négy irány: vízszintes, függőleges, főátló, mellékátló
            return CheckDirection(sor, oszlop, 0, 1 )   // vízszintes →
                || CheckDirection(sor, oszlop, 1, 0 )   // függőleges ↓
                || CheckDirection(sor, oszlop, 1, 1 )   // főátló ↘
                || CheckDirection(sor, oszlop, 1, -1 ); // mellékátló ↙
            /*if (_lepesek > 7 && !_tabla.IsFilled)
            {
                //sor ellenorzes
                if (oszlop > 2 && oszlop + 3 < _tabla.Meret) // Ha egyik irányból sem túl nagy
                {
                    if (helpWinSor(oszlop - 3, oszlop + 3, sor) != false) return true;
                }
                if (oszlop < 3) // Ha balról túl nagy
                {
                    if (helpWinSor(0, Math.Min(oszlop + 3, _tabla.Meret - 1), sor) != false) return true;
                }
                if (oszlop + 3 >= _tabla.Meret) // Ha jobbról túl nagy
                {
                    if (helpWinSor(Math.Max(oszlop - 3, 0), _tabla.Meret, sor) != false) return true;
                }

                // oszlop ellenorzes
                if (sor > 2 && sor + 3 < _tabla.Meret) // Ha egyik irányból sem túl nagy
                {
                    if (helpWinOszlop(sor - 3, sor + 3, oszlop) != false) return true;
                }
                if (sor < 3) // Ha túl közel van a tábla tetejéhez
                {
                    if (helpWinOszlop(0, Math.Min(sor + 3, _tabla.Meret - 1), oszlop) != false) return true;
                }
                if (sor + 3 >= _tabla.Meret) // Ha túl közel van a tábla aljához
                {
                    if (helpWinOszlop(Math.Max(sor - 3, 0), _tabla.Meret - 1, oszlop) != false) return true;
                }

                // Főátló ellenőrzés
                if (sor > 2 && oszlop > 2 && sor + 3 < _tabla.Meret && oszlop + 3 < _tabla.Meret)
                {
                    if (helpWinFoAtlo(sor - 3, oszlop - 3, sor + 3, oszlop + 3) != false) return true;
                }
                if (sor < 3 || oszlop < 3)
                {
                    if (helpWinFoAtlo(0, 0, Math.Min(sor + 3, _tabla.Meret - 1), Math.Min(oszlop + 3, _tabla.Meret - 1)) != false) return true;
                }

                // Mellékátló ellenőrzés
                if (sor > 2 && oszlop + 3 < _tabla.Meret && sor + 3 < _tabla.Meret && oszlop > 2)
                {
                    if (helpWinMellekAtlo(sor - 3, oszlop + 3, sor + 3, oszlop - 3) != false) return true;
                }
                if (sor < 3 || oszlop + 3 >= _tabla.Meret)
                {
                    if (helpWinMellekAtlo(0, Math.Min(oszlop + 3, _tabla.Meret - 1), Math.Min(sor + 3, _tabla.Meret - 1), Math.Max(oszlop - 3, 0)) != false) return true;
                }

            }
            return false;*/
        }
        private bool CheckDirection(int sor, int oszlop, int dSor, int dOszlop)
        {
            int count = 1; // maga a most lerakott bábu

            // egyik irányba
            count += CountDirection(sor, oszlop, dSor, dOszlop);

            // ellentétes irányba
            count += CountDirection(sor, oszlop, -dSor, -dOszlop);

            return count >= 5;
        }
        private int CountDirection(int sor, int oszlop, int dSor, int dOszlop)
        {
            int db = 0;

            int ujSor = sor + dSor;
            int ujOszlop = oszlop + dOszlop;

            while (ujSor >= 0 && ujSor < _table.Size &&
                   ujOszlop >= 0 && ujOszlop < _table.Size &&
                   _table[ujSor, ujOszlop] == _currentPlayer)
            {
                db++;
                ujSor += dSor;
                ujOszlop += dOszlop;
            }

            return db;
        }

        public Field GetValue(Int32 x, Int32 y) => _table.GetValue(x, y);

        public Boolean IsEmpty(Int32 x, Int32 y) => _table.IsEmpty(x, y);



        /*public void NewGame()
        {   int ideig_ertek = újJátékToolStripMenuItem_Click(object sender, EventArgs e);    
            tabla = new AmobaTable();

            _timer.Start();
        }*/
            /*
                    public void NewGame(Int32 meret)
                    {
                        tabla = new PotyogosTable(meret);
                        _lepesSzamlalo = 0;
                        _gameTime = 0;
                        _timer.Start();
                    }

                    public void ResumeGame()
                    {
                        if (!JatekVege)
                            _timer.Start();
                    }
                    public void Step(Int32 oszlop, Mezo ertek)
                    {
                        if (JatekVege) // ha már vége a játéknak, nem játszhatunk
                            return;

                        tabla.AddElement(oszlop, ertek); // tábla módosítása
                        OnFieldChanged(x, y);

                        _lepesSzamlalo++; // lépésszám növelés
                        //OnGameAdvanced();

                        if (tabla.IsFilled) // ha vége a játéknak, jelezzük, hogy győztünk
                        {
                            //OnGameOver(true);
                        }
                    }
                    private void OnFieldChanged(Int32 x, Int32 y, Int32 ertek)
                    {
                        FieldChanged?.Invoke(this, new PotyogosFieldEventArgs(x,y, ertek));
                    }*/

        private void OnGameRefresh()
        {
            GameRefresh?.Invoke(this, new PotyogosEventArgs(false, _currentPlayer, _currentPlayer == Field.PlayerX ? _PlayerTimeX : _PlayerTimeO));
        }

        private void OnFieldChanged(Int32 x, Int32 y,Field jatekos)
        {
            FieldChanged?.Invoke(this, new PotyogosFieldEventArgs(x,y,jatekos));
        }

        private void Timer_Elapsed(Object? sender, EventArgs e)
        {
            if (isGameOver) // ha már vége, nem folytathatjuk
                return;

            if (_currentPlayer == Field.PlayerO)
            {
                _PlayerTimeO++;
            }
            else
            {
                _PlayerTimeX++;
            }
            OnGameRefresh();
        }

    }
}
