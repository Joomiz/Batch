using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joomiz.Batch.Sort
{
    class Program
    {
        static void Main(string[] args)
        {
            bool debug = false;
            bool gerarArquivoTeste = false;           

            if (!debug && args.Length != 3)
            {
                Console.WriteLine(@"O programa espera 3 parâmetros de entrada: C:\INPUT.TXT C:\SORTED.TXT 0,3;6,2");
                return;
            }

            string filePathInput = debug ? @"G:\Projetos\Joomiz.Batch\Arquivos\File_BIG.txt" : args[0];
            string filePathOutput = debug ? filePathInput.Replace(".txt", "_sorted.txt") : args[1];
            string keyPosition = debug ? "1,8;14,6" : args[2];


            File.WriteAllText(filePathOutput, null);

            if (debug && gerarArquivoTeste)
            {
                using (StreamWriter sw = new StreamWriter(filePathInput))
                {
                    DateTime startDate = new DateTime(1900, 1, 1);
                    for (int j = 0; j < 950000; j++)
                    {
                        if (j % 100 == 0)
                        {
                            startDate = startDate.AddMonths(1);
                        }

                        sw.WriteLine("{0}{1}01BBAAA{2}A88888888888888888888888888888888888888888888888888888888888888888888888888888X88888888888888888888888888888888888888888888888888888888888888888888888888888", startDate.Year, startDate.Month.ToString().PadLeft(2, '0'), Guid.NewGuid().ToString().Replace("-", "").Substring(0,6));
                    }
                }
            }

            FileInfo fileInfo1 = new FileInfo(filePathInput);            
            double bytesMax = 1048576 * 1; // número de megabytes por arquivo
            double totalFiles = Math.Ceiling(fileInfo1.Length / bytesMax);
            List<string> files = new List<string>();
            string tempOutputFile = Path.GetTempFileName();
            string currentFileName = null;
            string currentLine = null;
            int i = 0;

            Console.WriteLine("O arquivo de entrada possui {0} bytes", fileInfo1.Length);
            Console.WriteLine("O tamanho máximo por arquivo é de {0} bytes", bytesMax);
            Console.WriteLine("Serão gerados {0} arquivos temporários", totalFiles);

            try
            {

                // Passo 1
                // Dividir o arquivo de entrada em vários arquivos, divididos por suas chaves até X bytes
                if (totalFiles <= 1)
                {
                    files.Add(filePathInput);
                }
                else
                {
                    using (StreamReader sr = new StreamReader(filePathInput))
                    {
                        currentFileName = Path.GetTempFileName();
                        StreamWriter sw = new StreamWriter(currentFileName);
                        sw.AutoFlush = true;
                        i = 1;
                        Console.WriteLine("Gerando arquivo {0} de {1} em {2}", i.ToString().PadLeft(6, '0'), totalFiles.ToString().PadLeft(6, '0'), currentFileName);

                        while ((currentLine = sr.ReadLine()) != null)
                        {
                            sw.WriteLine(currentLine);

                            if (sw.BaseStream.Length > bytesMax)
                            {
                                sw.Close();
                                files.Add(currentFileName);
                                currentFileName = Path.GetTempFileName();
                                sw = new StreamWriter(currentFileName);
                                sw.AutoFlush = true;
                                i++;
                                Console.WriteLine("Gerando arquivo {0} de {1} em {2}", i.ToString().PadLeft(6, '0'), totalFiles.ToString().PadLeft(6, '0'), currentFileName);
                            }
                        }

                        if (sw.BaseStream.Length > 0)
                        {
                            sw.Close();
                            files.Add(currentFileName);
                        }

                    }
                }

                // Passo 2
                // Ordenar todos os arquivos
                i = 0;
                foreach (string file in files)
                {
                    i++;
                    Console.WriteLine("Ordenando arquivo {0} de {1} em {2}", i.ToString().PadLeft(6, '0'), totalFiles.ToString().PadLeft(6, '0'), file);
                    string[] content = File.ReadAllLines(file);
                    File.WriteAllLines(file, content.OrderBy(x => x, new KeyComparer(keyPosition)).ToArray());
                }

                // Passo 3
                // Fazer merge de todos os arquivos ordenando na saída                

                var keyComparer = new KeyComparer(keyPosition);
                i = 0;
                foreach (string file in files)
                {
                    i++;
                    Console.WriteLine("Fazendo merge do arquivo {0} de {1} em {2}", i.ToString().PadLeft(6, '0'), totalFiles.ToString().PadLeft(6, '0'), file);

                    File.Copy(filePathOutput, tempOutputFile, true);

                    string lineOutput, linePiece, keyOutput, keyPiece;

                    using (StreamWriter swOutput = new StreamWriter(filePathOutput))
                    using (StreamReader srOutput = new StreamReader(tempOutputFile))
                    using (StreamReader srPiece = new StreamReader(file))
                    {
                        lineOutput = srOutput.ReadLine();
                        linePiece = srPiece.ReadLine();

                        keyOutput = ReadKey(lineOutput, keyPosition);
                        keyPiece = ReadKey(linePiece, keyPosition);

                        while (keyOutput != null || keyPiece != null)
                        {

                            if (keyOutput == null)
                            {
                                swOutput.WriteLine(linePiece);
                                linePiece = srPiece.ReadLine();
                                keyPiece = ReadKey(linePiece, keyPosition);
                                continue;
                            }

                            if (keyPiece == null)
                            {
                                swOutput.WriteLine(lineOutput);
                                lineOutput = srOutput.ReadLine();
                                keyOutput = ReadKey(lineOutput, keyPosition);   
                                continue;
                            }

                            //if (keyOutput != null && keyComparer.Compare(keyOutput, keyPiece) == 0)
                            if (string.Compare(keyOutput, keyPiece) == 0)
                            {
                                swOutput.WriteLine(lineOutput);
                                swOutput.WriteLine(linePiece);

                                lineOutput = srOutput.ReadLine();
                                linePiece = srPiece.ReadLine();

                                keyOutput = ReadKey(lineOutput, keyPosition);
                                keyPiece = ReadKey(linePiece, keyPosition);
                            }
                            
                            //if (keyComparer.Compare(keyOutput, keyPiece) > 0)
                            if (string.Compare(keyOutput, keyPiece) < 0)
                            {
                                swOutput.WriteLine(lineOutput);
                                lineOutput = srOutput.ReadLine();
                                keyOutput = ReadKey(lineOutput, keyPosition);                                
                            }

                            //if (keyComparer.Compare(keyOutput, keyPiece) < 0)
                            if (string.Compare(keyOutput, keyPiece) > 0)
                            {
                                swOutput.WriteLine(linePiece);
                                linePiece = srPiece.ReadLine();
                                keyPiece = ReadKey(linePiece, keyPosition);
                            }
                        }
                    }
                }

            }
            catch
            {
                throw;
            }
            finally
            {
                File.Delete(tempOutputFile);
                foreach (String file in files)
                {
                    File.Delete(file);
                }
            }

        }

        

        static string ReadKey(string line, string keyPosition)
        {

            if (line == null)
                return null;

            string key = string.Empty;
            string[] pos = new string[2];

            foreach (string seg in keyPosition.Split(';'))
            {
                pos = seg.Split(',');
                key = string.Concat(key, line.Substring(int.Parse(pos[0]) - 1, int.Parse(pos[1])));
            }

            return key;

        }

       
        public class KeyComparer : IComparer<string>
        {

            private string KeyPosition { get; set; }

            public KeyComparer(string keyPosition)
            {
                this.KeyPosition = keyPosition;
            }

            public int Compare(string x, string y)
            {
                if (x == null && y == null)
                    return 0;

                if (x == null)
                    return 1;

                if (y == null)
                    return -1;

                return string.Compare(this.ReadKey(x), this.ReadKey(y));
            }

            private string ReadKey(string line)
            {

                if (line == null || line == string.Empty)
                    return null;

                string key = string.Empty;

                try
                {                    
                    string[] pos = new string[2];

                    foreach (string seg in this.KeyPosition.Split(';'))
                    {
                        pos = seg.Split(',');
                        key = string.Concat(key, line.Substring(int.Parse(pos[0]) - 1, int.Parse(pos[1])));
                    }

                }
                catch(Exception ex)
                {

                }

                return key;

            }
        }
    }
}
