using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Potyogós_amőba.Persistence
{       
    public enum Mezo
    {
        Ures = 0,
        PlayerX = 1,
        PlayerO = 2
    }
    public class PotyogosTable
    {
        
        private Mezo[,] tablaErtekei;

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

        public Int32 Meret
        {
            get { return tablaErtekei.GetLength(0); }
        }

        public Mezo this[Int32 x, Int32 y]
        {
            get { return GetValue(x, y); }
        }

        //public PotyogosTable() : this(4) { }
        public PotyogosTable(Int32 meret)
        {
            tablaErtekei = new Mezo[meret, meret];
        }

        public Boolean IsEmpty(Int32 x, Int32 y)
        {
            if (x < 0 || x >= tablaErtekei.GetLength(0))
                throw new ArgumentOutOfRangeException(nameof(x), "The X coordinate is out of range.");
            if (y < 0 || y >= tablaErtekei.GetLength(1))
                throw new ArgumentOutOfRangeException(nameof(y), "The Y coordinate is out of range.");

            return tablaErtekei[x, y] == 0;
        }

        public Mezo GetValue(Int32 x, Int32 y)
        {
            if (x < 0 || x >= tablaErtekei.GetLength(0))
                throw new ArgumentOutOfRangeException(nameof(x), "The X coordinate is out of range.");
            if (y < 0 || y >= tablaErtekei.GetLength(1))
                throw new ArgumentOutOfRangeException(nameof(y), "The Y coordinate is out of range.");

            return tablaErtekei[x, y];
        }

        public void AddElement(Int32 oszlop, Mezo ertek)
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
        /*public void AddElement(Int32 x, Int32 y, Mezo ertek)
        {
            if(x < 0 || x >= tablaErtekei.GetLength(0))
                throw new ArgumentOutOfRangeException(nameof(x), "The X coordinate is out of range.");
            if (y < 0 || y >= tablaErtekei.GetLength(1))
                throw new ArgumentOutOfRangeException(nameof(y), "The Y coordinate is out of range.");
            if (ertek != Mezo.Ures && ertek != Mezo.JatekosX && ertek != Mezo.JatekosO)
                throw new ArgumentOutOfRangeException(nameof(ertek), "The value is out of range.");
            switch(ertek)
            {
                case 0:
                    tablaErtekei[x, y] = Mezo.Ures; break;
                case 1:
                    tablaErtekei[x,y] = Mezo.JatekosX; break;
                case 2:
                    tablaErtekei[x,y] = Mezo.JatekosO; break;
            }
           
        }*/
        public void SetValue(int sor, int oszlop, Mezo ertek)
        {
            if (sor < 0 || sor >= Meret || oszlop < 0 || oszlop >= Meret)
                throw new ArgumentOutOfRangeException();
            tablaErtekei[sor, oszlop] = ertek;
        }

        public Int32 EmptySpot(Int32 oszlop)
        {
            for (Int32 i = tablaErtekei.GetLength(1) - 1; i > -1 ; i--)
            {
                if (tablaErtekei[i, oszlop] == Mezo.Ures)
                    return i;
            }
            return -1;
        }
    }
}
