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
                    string[] times = (await reader.ReadLineAsync() ?? "0 0").Split(' ');
                    int gameTimeX = Int32.Parse(times[0]);
                    int gameTimeO = Int32.Parse(times[1]);
                    string line = await reader.ReadLineAsync() ?? String.Empty;
                    Int32 boardSize = Int32.Parse(line); // read the size of the board
                    PotyogosTable table = new PotyogosTable(boardSize); // create the board
                    String[] values;
                    for (Int32 i = 0; i < boardSize; i++)
                    {
                        line = await reader.ReadLineAsync() ?? String.Empty;
                        values = line.Split(' ');

                        for (Int32 j = 0; j < boardSize; j++)
                        {
                            int val = Int32.Parse(values[j]);
                            if (val >= 0 && val <= 2)
                            {
                                table.AddElement(j, (Field)val);
                                table.SetValue(i, j, (Field)val);
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
                    await writer.WriteLineAsync(table.Size + "");
                    for (int i = 0; i < table.Size; i++)  // fentről lefelé vagy
                    {
                        for (int j = 0; j < table.Size; j++)
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
