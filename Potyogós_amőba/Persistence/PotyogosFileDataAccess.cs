using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Potyogós_amőba.Persistence
{
    public class PotyogosFileDataAccess : IPotyogosDataAccess
    {
        public async Task<PotyogosTable> LoadAsync(String path)
        {
            try
            {
                using (StreamReader reader = new StreamReader(path)) // fájl megnyitása
                {
                    string[] idok = (await reader.ReadLineAsync() ?? "0 0").Split(' ');
                    int jatekIdoX = Int32.Parse(idok[0]);
                    int jatekIdoO = Int32.Parse(idok[1]);
                    string sor = await reader.ReadLineAsync() ?? String.Empty;
                    Int32 tablaMeret = Int32.Parse(sor); // beolvassuk a tábla méretét
                    PotyogosTable table = new PotyogosTable(tablaMeret); // létrehozzuk a táblát
                    String[] ertekek; 
                    for (Int32 i = 0; i < tablaMeret; i++)
                    {
                        sor = await reader.ReadLineAsync() ?? String.Empty;
                        ertekek = sor.Split(' ');

                        for (Int32 j = 0; j < tablaMeret; j++)
                        {
                            int val = Int32.Parse(ertekek[j]);
                            if (val >= 0 && val <= 2)
                            {
                                table.AddElement(j, (Mezo)val);
                                table.SetValue(i, j, (Mezo)val);
                            }
                            else
                            {
                                throw new PotyogosDataException();
                            }
                        }
                    }
                    return table;
                } 
            }
            catch 
            {
                throw new PotyogosDataException();
            }
        }

        public async Task SaveAsync(String path, PotyogosTable table, int _jatekIdoX, int _jatekIdoO)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path)) // fájl megnyitása
                {
                    // Játékidők
                    await writer.WriteLineAsync(_jatekIdoX + " " + _jatekIdoO);
                    await writer.WriteLineAsync(table.Meret + "");
                    for (int i = 0; i < table.Meret; i++)  // fentről lefelé vagy
                    {
                        for (int j = 0; j < table.Meret; j++)
                        {
                            await writer.WriteAsync(((int)table[i, j]).ToString() + " ");
                        }
                        await writer.WriteLineAsync();
                    }
                }
            }
            catch
            {
                throw new PotyogosDataException();
            }
        }
    }
}
