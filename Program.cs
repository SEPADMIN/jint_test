using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace FocusStart
{
    class Program
    {
        static void Main(string[] args)
        {
            CLP clp = new CLP();
            CLArgs clargs = clp.Parse(args);

            ThreadRunner thr = new ThreadRunner(clargs);
            thr.Merge(thr.Run());

            Logger.Free();
        }
    }

    // структура для хранения аргументов командной строки
    class CLArgs
    {
        public char order;
        public char type;
        public string output;
        public string[] inputs;
    }

    // класс для парсинга параметров командной строки
    class CLP
    {
        CLArgs clargs;
        private char order = 'n';
        private char type;
        private string output;
        private int inputLength;
        private string[] inputs;

        public CLArgs Parse(string[] args)
        {
            int i = 0; // счетчик номера аргумента
            if ((args[0] == "-d") || (args[0] == "-a"))
                order = args[i++][1];
            else
                order = 'a';

            type = args[i++][1];
            output = args[i++];

            inputLength = args.Length - i;
            inputs = new string[inputLength];
            for (int j = i, k = 0; j < args.Length; j++, k++)
                inputs[k] = args[j];

            clargs = new CLArgs();
            clargs.inputs = inputs;
            clargs.output = output;
            clargs.order = order;
            clargs.type = type;
            return clargs;
        } 
    }

    // структура для обмена данными с генераторами
    class ThreadData
    {
        public string name;
        public string data;
        public int index;
    }

    // класс-ядро, реализующий работу с генераторами и слияние
    class ThreadRunner
    {
        CLArgs clargs;
        public string[] results; // здесь хранятся строки, готовые для слияния

        public ThreadRunner(CLArgs clargs)
        {
            this.clargs = new CLArgs();
            this.clargs.inputs = new string[clargs.inputs.Length];
            this.clargs = clargs;
            results = new string[clargs.inputs.Length];
        }

        // функция для определения индекса минимального элемента на текущем этапе слияния
        public int FindMinElementIndex(string[] collection, char type)
        {
            int rIndex = 0;
            for (int i = 0; i < collection.Length; i++)
            {
                if (collection[i] == null)
                {
                    if (rIndex == i)
                        rIndex++;
                    continue;
                }

                if (type == 's')
                {
                    if (collection[i].CompareTo(collection[rIndex]) < 0)
                        rIndex = i;
                }

                else
                {
                    if (Int32.Parse(collection[i]) < Int32.Parse(collection[rIndex]))
                        rIndex = i;
                }
            }
            return (rIndex >= collection.Length) ? -1 : rIndex;
        }

        // функция для определения индекса максимального элемента на текущем этапе слияния
        public int FindMaxElementIndex(string[] collection, char type)
        {
            int rIndex = collection.Length - 1;
            for (int i = collection.Length - 1; i >= 0; i--)
            {
                if (collection[i] == null)
                {
                    if (rIndex == i)
                        rIndex--;
                    continue;
                }

                if (type == 's')
                {
                    if (collection[i].CompareTo(collection[rIndex]) > 0)
                        rIndex = i;
                }

                else
                {
                    if (Int32.Parse(collection[i]) > Int32.Parse(collection[rIndex]))
                        rIndex = i;
                }
            }
            return (rIndex >= collection.Length) ? -1 : rIndex;
        }

        // функция для создания генераторов, возвращает коллекцию их перечислителей
        public List<IEnumerator<ThreadData>> Run()
        {
            // здесь храним перечислители генераторов
            List<IEnumerator<ThreadData>> generators = new List<IEnumerator<ThreadData>>();
            
            // за один проход создаем генераторы и помещаем их в коллекцию
            for (int i = 0; i < clargs.inputs.Length; i++)
            {
                ThreadData td = new ThreadData();
                td.name = clargs.inputs[i];
                td.index = i;

                var generator = GetNextLine(td, clargs.type).GetEnumerator();
                if (generator.MoveNext())
                    generators.Add(generator);
                results[i] = generator.Current.data;
                generator.MoveNext();
            }

            return generators;
        }

        // функция слияния текущих данных из массива results в выходной файл
        // не забываем вызывать MoveNext() у генератора, значение которого использовали
        public void Merge(List<IEnumerator<ThreadData>> generators)
        {
            int k;
            using (StreamWriter sw = new StreamWriter(clargs.output))
                while (results.Any(x => x != null))
                {
                    // выбираем порядок сортировки
                    k = (clargs.order == 'a') ? FindMinElementIndex(results, clargs.type)
                        : FindMaxElementIndex(results, clargs.type);
                    sw.WriteLine(results[k]);

                    if (clargs.order == 'a')
                    {
                        if (clargs.type == 's')
                        {
                            while (String.Compare(results[k],
                                generators[k].Current.data) > 0)
                            {
                                Logger.OrderError(generators[k].Current.name,
                                    generators[k].Current.index, generators[k].Current.data);
                                generators[k].MoveNext();
                                if ((generators[k].Current.data) == null)
                                    break;
                            }
                        }
                        else
                        {
                            while (Int32.Parse(results[k]) > 
                                Int32.Parse(generators[k].Current.data))
                            {
                                Logger.OrderError(generators[k].Current.name,
                                    generators[k].Current.index, generators[k].Current.data);
                                generators[k].MoveNext();
                                if ((generators[k].Current.data) == null)
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (clargs.type == 's')
                        {
                            try
                            {
                                while (String.Compare(results[k],
                                generators[k].Current.data) < 0)
                                {
                                    Logger.OrderError(generators[k].Current.name,
                                        generators[k].Current.index, generators[k].Current.data);
                                    generators[k].MoveNext();
                                    if ((generators[k].Current.data) == null)
                                        break;
                                }
                            }
                            catch
                            {
                                generators[k].Current.data = null;
                            }
                        }
                        else
                        {
                            try
                            {
                                while (Int32.Parse(results[k]) <
                                Int32.Parse(generators[k].Current.data))
                                {
                                    Logger.OrderError(generators[k].Current.name,
                                        generators[k].Current.index, generators[k].Current.data);
                                    generators[k].MoveNext();
                                    if ((generators[k].Current.data) == null)
                                        break;
                                }
                            }
                            catch
                            {
                                generators[k].Current.data = null;
                            }
                        }
                    }
                    
                    results[k] = generators[k].Current.data;
                    if (!generators[k].MoveNext())
                        generators[k] = null;
                }
        }

        // выглядит как функция, а поведение как у итератора
        // генератор отдает по одному значению за вызов
        public static IEnumerable<ThreadData> GetNextLine(ThreadData input, char type)
        {
            int index = -1;
            ThreadData response = new ThreadData();
            response.name = input.name;
            response.index = input.index;
            string line;

            using (StreamReader sr = new StreamReader(input.name))
                while ((line = sr.ReadLine()) != null)
                {
                    if (type == 'i')
                    {
                        try
                        {
                            Int32.Parse(line);
                        }
                        catch
                        {
                            Logger.ParseError(input.name, ++index, line);
                            continue;
                        }
                    }
                    response.data = line;
                    response.index = ++index;
                    yield return response;
                }

            response.data = null;
            yield return response;
        }
    }

    // класс для записи ошибок
    static class Logger
    {
        private static StreamWriter sr = new StreamWriter("log.txt");
        private static int count = 0;

        public static void ParseError(string file, int index, string line)
        {
            sr.WriteLine("Type error in row {0} in file \"{1}\": " +
                "\"{2}\" is not an integer", index, file, line);
            count++;
        }

        public static void OrderError(string file, int index, string line)
        {
            sr.WriteLine("Order error in row {0} in file \"{1}\": " +
                "\"{2}\" is not in order", index + 1, file, line);
            count++;
        }

        public static void Free()
        {
            try
            {
                sr.WriteLine("Total error amount: " + count.ToString());
                sr.Dispose();
            }
            catch
            {
                throw new Exception();
            }
        }
    }
}
