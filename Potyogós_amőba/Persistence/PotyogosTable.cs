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

        private Field[,] tablaErtekei;

        #endregion

        #region Properties

        /// <summary>
        /// Igaz, ha a tábla teljesen tele van.
        /// </summary>
        public Boolean IsFilled
        {
            get
            {
                foreach (Int32 ertek in tablaErtekei)
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
            get { return tablaErtekei.GetLength(0); }
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
            tablaErtekei = new Field[meret, meret];
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
            if (x < 0 || x >= tablaErtekei.GetLength(0))
                throw new ArgumentOutOfRangeException(nameof(x), "The X coordinate is out of range.");
            if (y < 0 || y >= tablaErtekei.GetLength(1))
                throw new ArgumentOutOfRangeException(nameof(y), "The Y coordinate is out of range.");

            return tablaErtekei[x, y] == 0;
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
            if (x < 0 || x >= tablaErtekei.GetLength(0))
                throw new ArgumentOutOfRangeException(nameof(x), "The X coordinate is out of range.");
            if (y < 0 || y >= tablaErtekei.GetLength(1))
                throw new ArgumentOutOfRangeException(nameof(y), "The Y coordinate is out of range.");

            return tablaErtekei[x, y];
        }

        /// <summary>
        /// Bábu hozzáadása az oszlop legalsó üres helyére.
        /// </summary>
        /// <param name="oszlop">Oszlop index.</param>
        /// <param name="ertek">Hely értéke (X vagy O).</param>
        /// <exception cref="ArgumentOutOfRangeException">Ha az oszlop kívül esik a tábla határain.</exception>
        /// <exception cref="InvalidOperationException">Ha az oszlop már tele van.</exception>
        public void AddElement(Int32 oszlop, Field ertek)
        {
            if (oszlop < 0 || oszlop >= tablaErtekei.GetLength(0))
                throw new ArgumentOutOfRangeException(nameof(oszlop), "The X coordinate is out of range.");

            int uresHely = EmptySpot(oszlop);
            if (uresHely != -1)
            {
                tablaErtekei[uresHely, oszlop] = ertek;
            }
            else
            {
                throw new InvalidOperationException("The column is full.");
            }
        }

        /// <summary>
        /// Mező értékének beállítása adott sorban és oszlopban.
        /// </summary>
        /// <param name="sor">Sor index.</param>
        /// <param name="oszlop">Oszlop index.</param>
        /// <param name="ertek">Beállítandó érték.</param>
        /// <exception cref="ArgumentOutOfRangeException">Ha a sor vagy oszlop kívül esik a tábla határain.</exception>
        public void SetValue(int sor, int oszlop, Field ertek)
        {
            if (sor < 0 || sor >= Size || oszlop < 0 || oszlop >= Size)
                throw new ArgumentOutOfRangeException();
            tablaErtekei[sor, oszlop] = ertek;
        }

        /// <summary>
        /// Visszaadja az oszlop legalsó üres helyének sor indexét.
        /// </summary>
        /// <param name="oszlop">Oszlop index.</param>
        /// <returns>Legalsó üres sor index, -1 ha az oszlop tele van.</returns>
        public Int32 EmptySpot(Int32 oszlop)
        {
            for (Int32 i = tablaErtekei.GetLength(1) - 1; i > -1; i--)
            {
                if (tablaErtekei[i, oszlop] == Field.Ures)
                    return i;
            }
            return -1;
        }

        #endregion
    }
}
