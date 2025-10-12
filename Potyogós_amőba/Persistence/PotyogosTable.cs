using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Potyogós_amőba.Persistence
{
    #region Enums

    /// <summary>
    /// Egy mező állapotát jelző felsorolt típus.
    /// </summary>
    public enum Field
    {
        /// <summary>Üres mező.</summary>
        Ures = 0,

        /// <summary>X játékos mezője.</summary>
        PlayerX = 1,

        /// <summary>O játékos mezője.</summary>
        PlayerO = 2
    }

    #endregion

    /// <summary>
    /// A Potyogós tábla logikáját kezelő osztály.
    /// </summary>
    public class PotyogosTable
    {
        #region Fields

        private Field[,] tableOfValues;

        #endregion

        #region Properties

        /// <summary>
        /// Igaz, ha a tábla teljesen tele van.
        /// </summary>
        public Boolean IsFilled
        {
            get
            {
                foreach (Int32 ertek in tableOfValues)
                    if (ertek == 0)
                        return false;
                return true;
            }
        }

        /// <summary>
        /// A tábla mérete (n x n).
        /// </summary>
        public Int32 Size
        {
            get { return tableOfValues.GetLength(0); }
        }

        /// <summary>
        /// Indexelő, amely visszaadja az adott mező értékét.
        /// </summary>
        /// <param name="x">Sor index.</param>
        /// <param name="y">Oszlop index.</param>
        public Field this[Int32 x, Int32 y]
        {
            get { return GetValue(x, y); }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Potyogós tábla létrehozása adott mérettel.
        /// </summary>
        /// <param name="meret">A tábla mérete (n x n).</param>
        public PotyogosTable(Int32 meret)
        {
            tableOfValues = new Field[meret, meret];
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Ellenőrzi, hogy a megadott mező üres-e.
        /// </summary>
        /// <param name="x">Sor index.</param>
        /// <param name="y">Oszlop index.</param>
        /// <returns>Igaz, ha a mező üres.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Ha az x vagy y kívül esik a tábla határain.</exception>
        public Boolean IsEmpty(Int32 x, Int32 y)
        {
            if (x < 0 || x >= tableOfValues.GetLength(0))
                throw new ArgumentOutOfRangeException(nameof(x), "The X coordinate is out of range.");
            if (y < 0 || y >= tableOfValues.GetLength(1))
                throw new ArgumentOutOfRangeException(nameof(y), "The Y coordinate is out of range.");

            return tableOfValues[x, y] == 0;
        }

        /// <summary>
        /// Mező értékének lekérdezése.
        /// </summary>
        /// <param name="x">Sor index.</param>
        /// <param name="y">Oszlop index.</param>
        /// <returns>Mező értéke.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Ha az x vagy y kívül esik a tábla határain.</exception>
        public Field GetValue(Int32 x, Int32 y)
        {
            if (x < 0 || x >= tableOfValues.GetLength(0))
                throw new ArgumentOutOfRangeException(nameof(x), "The X coordinate is out of range.");
            if (y < 0 || y >= tableOfValues.GetLength(1))
                throw new ArgumentOutOfRangeException(nameof(y), "The Y coordinate is out of range.");

            return tableOfValues[x, y];
        }

        /// <summary>
        /// Bábu hozzáadása az oszlop legalsó üres helyére.
        /// </summary>
        /// <param name="column">Oszlop index.</param>
        /// <param name="value">Hely értéke (X vagy O).</param>
        /// <exception cref="ArgumentOutOfRangeException">Ha az oszlop kívül esik a tábla határain.</exception>
        /// <exception cref="InvalidOperationException">Ha az oszlop már tele van.</exception>
        public void AddElement(Int32 column, Field value)
        {
            if (column < 0 || column >= tableOfValues.GetLength(0))
                throw new ArgumentOutOfRangeException(nameof(column), "The X coordinate is out of range.");

            int uresHely = EmptySpot(column);
            if (uresHely != -1)
            {
                tableOfValues[uresHely, column] = value;
            }
            else
            {
                throw new InvalidOperationException("The column is full.");
            }
        }

        /// <summary>
        /// Mező értékének beállítása adott sorban és oszlopban.
        /// </summary>
        /// <param name="row">Sor index.</param>
        /// <param name="column">Oszlop index.</param>
        /// <param name="value">Beállítandó érték.</param>
        /// <exception cref="ArgumentOutOfRangeException">Ha a sor vagy oszlop kívül esik a tábla határain.</exception>
        public void SetValue(int row, int column, Field value)
        {
            if (row < 0 || row >= Size || column < 0 || column >= Size)
                throw new ArgumentOutOfRangeException();
            tableOfValues[row, column] = value;
        }

        /// <summary>
        /// Visszaadja az oszlop legalsó üres helyének sor indexét.
        /// </summary>
        /// <param name="column">Oszlop index.</param>
        /// <returns>Legalsó üres sor index, -1 ha az oszlop tele van.</returns>
        public Int32 EmptySpot(Int32 column)
        {
            for (Int32 i = tableOfValues.GetLength(1) - 1; i > -1; i--)
            {
                if (tableOfValues[i, column] == Field.Ures)
                    return i;
            }
            return -1;
        }

        #endregion
    }
}
