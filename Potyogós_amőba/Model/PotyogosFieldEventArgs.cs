using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Potyogós_amőba.Persistence;


namespace Potyogós_amőba.Model
{
    public class PotyogosFieldEventArgs : EventArgs
    {
        public Int32 X { get; }
        public Int32 Y { get; }
        public Field jatekos { get; }

        public PotyogosFieldEventArgs(Int32 x, Int32 y, Field jatekos)
        {
            X = x;
            Y = y;
            this.jatekos = jatekos;
        }
    }
}
