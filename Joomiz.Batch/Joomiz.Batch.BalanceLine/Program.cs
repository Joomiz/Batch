using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joomiz.Batch.BalanceLine
{
    class Program
    {
       
        static void Main(string[] args)
        {
            bool debug = true;

            if (!debug && args.Length != 4)
            {
                Console.WriteLine(@"O programa espera 4 parâmetros de entrada: C:\FILE1.TXT 0,3;6,2 C:\FILE2.TXT 0,3;9,2");
                return;
            }            

            int totalLines1 = 0;
            int totalLines2 = 0;
            int totalLinesDesp1 = 0;
            int totalLinesDesp2 = 0;
            string filePath1 = debug ? @"G:\Projetos\Joomiz.Batch\Arquivos\File1.txt" : args[0];
            string key1Position = debug ? "1,8;14,3" : args[1];          
            string filePath2 = debug ? @"G:\Projetos\Joomiz.Batch\Arquivos\File2.txt" : args[2];
            string key2Position = debug ? "1,8;14,3" : args[3];
            string line1, line2, keyValue1, keyValue2, oldKeyValue1, oldKeyValue2;

            StreamReader sr1 = new StreamReader(filePath1);
            StreamReader sr2 = new StreamReader(filePath2);
            StreamWriter sw1 = new StreamWriter(filePath1.Replace(".txt", "_ok.txt"), true);
            StreamWriter sw2 = new StreamWriter(filePath2.Replace(".txt", "_ok.txt"), true);
            StreamWriter sw3 = new StreamWriter(filePath1.Replace(".txt", "_despr.txt"), true);
            StreamWriter sw4 = new StreamWriter(filePath2.Replace(".txt", "_despr.txt"), true);

            totalLines1 = ReadLine(totalLines1, key1Position, sr1, out line1, out keyValue1);
            totalLines2 = ReadLine(totalLines2, key2Position, sr2, out line2, out keyValue2);

            while (line1 != null || line2 != null)
            {                

                if (keyValue1 == keyValue2)
                {
                    sw1.WriteLine(line1);
                    sw2.WriteLine(line2);


                    oldKeyValue1 = keyValue1;
                    do
                    {
                        totalLines1 = ReadLine(totalLines1, key1Position, sr1, out line1, out keyValue1);

                        if (oldKeyValue1 == keyValue1)
                            sw1.WriteLine(line1);                        

                    } while (oldKeyValue1 == keyValue1);


                    oldKeyValue2 = keyValue2;
                    do
                    {
                        totalLines2 = ReadLine(totalLines2, key2Position, sr2, out line2, out keyValue2);

                        if (oldKeyValue2 == keyValue2)
                            sw2.WriteLine(line2);

                    } while (oldKeyValue2 == keyValue2);
                }

                // se não existir o registro no arquivo 1, despreza o registro do arquivo 1 e lê o próximo
                if (string.Compare(keyValue1, keyValue2) > 0)
                {
                    sw3.WriteLine(line1);
                    totalLinesDesp1++;

                    // despreza as próximas linhas com mesma chave até encontrar uma chave diferente
                    oldKeyValue1 = keyValue1;
                    do
                    {
                        totalLines1 = ReadLine(totalLines1, key1Position, sr1, out line1, out keyValue1);

                        if (oldKeyValue1 == keyValue1)
                        {
                            sw3.WriteLine(line1);
                            totalLinesDesp1++;
                        }

                    } while (oldKeyValue1 == keyValue1);

                    
                }

                // se não existir o registro no arquivo 2, despreza o registro do arquivo 2 e lê o próximo
                if (string.Compare(keyValue1, keyValue2) < 0)
                {
                    sw4.WriteLine(line2);
                    totalLinesDesp2++;

                    oldKeyValue2 = keyValue2;

                    do
                    {
                        totalLines2 = ReadLine(totalLines2, key2Position, sr2, out line2, out keyValue2);

                        // despreza as próximas linhas com mesma chave até encontrar uma chave diferente
                        if (oldKeyValue2 == keyValue2)
                        {
                            sw4.WriteLine(line2);
                            totalLinesDesp2++;
                        }

                    } while (oldKeyValue2 == keyValue2);

                }

            } // while


            sr1.Close();
            sr2.Close();
            sw1.Close();
            sw2.Close();
            sw3.Close();
            sw4.Close();

            Console.WriteLine("Registros lidos no arquivo 1: {0}", totalLines1);
            Console.WriteLine("Registros lidos no arquivo 2: {0}", totalLines2);
            Console.WriteLine("Registros desprezados no arquivo 1: {0}", totalLinesDesp1);
            Console.WriteLine("Registros desprezados no arquivo 2: {0}", totalLinesDesp2);

        }

        private static int ReadLine(int totalLines, string keyPosition, StreamReader sr, out string line, out string keyValue)
        {
            line = sr.ReadLine();

            if (line != null)
            {
                keyValue = ReadKey(line, keyPosition);
                totalLines++;
            }
            else
            {
                keyValue = null;
            }
            return totalLines;
        }        


        static string ReadKey(string line, string positions)
        {
            string key = string.Empty;
            string[] pos = new string[2];

            foreach (string seg in positions.Split(';'))
            {
                pos = seg.Split(',');
                key = string.Concat(key, line.Substring(int.Parse(pos[0]) - 1, int.Parse(pos[1])));
            }

            return key;
        }

        
    }
}
