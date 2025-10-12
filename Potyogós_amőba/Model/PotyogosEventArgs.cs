using Potyogós_amőba.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Potyogós_amőba.Model
{
    public class PotyogosEventArgs : EventArgs
    {
        private Int32 _jatekosIdo;
        private Field _currantPlayer;
        private Boolean _nyert;
        private List<(int X, int Y)> _winningCells;

        /// <summary>
        /// Játékidő lekérdezése.
        /// </summary>
        public Int32 GameTime { get { return _jatekosIdo; } }

        /// <summary>
        /// Játéklépések számának lekérdezése.
        /// </summary>
        public Field CurrantPlayer { get { return _currantPlayer; } }

        /// <summary>
        /// Győzelem lekérdezése.
        /// </summary>
        public Boolean IsWon { get { return _nyert; } }
        public IReadOnlyList<(int X, int Y)> WinningCells => _winningCells;

        /// <summary>
        /// Sudoku eseményargumentum példányosítása.
        /// </summary>
        /// <param name="isWon">Győzelem lekérdezése.</param>
        /// <param name="gameStepCount">Lépésszám.</param>
        /// <param name="gameTime">Játékidő.</param>
        public PotyogosEventArgs(Boolean nyert, Field currantPlayer, Int32 jatekIdo, List<(int X, int Y)> winningCells = null!)
        {
            _nyert = nyert;
            _currantPlayer = currantPlayer;
            _jatekosIdo = jatekIdo;
            _winningCells = winningCells ?? new List<(int X, int Y)>();
        }
    }      
}
