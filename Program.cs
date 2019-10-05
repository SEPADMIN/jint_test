using System;
using System.IO;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace FocusStart
{
    class Program
    {
        static void Main(string[] args)
        {
            CLP clp = new CLP();
            clp.Parse(args);

            ThreadRunner thr = new ThreadRunner(clp.OutName, clp.Inputs);
            //thr.Run();
            thr.Merge(new MergeTask(clp.Inputs, clp.OutName));

            //Console.ReadLine();
        }
    }

    class CLP
    {
        private char order = 'n';
        private char type;
        private string outName;
        private int inputLength;
        private string[] inputs;
        public char Order { get { return order; }  }
        public char Type { get { return type; } }
        public string OutName { get { return outName; } }
        public int InputLength { get { return inputLength; } }
        public string[] Inputs { get { return inputs; } }

        public void Parse(string[] args)
        {
            int i = 0; // счетчик номера аргумента
            if ((args[0] == "-d") || (args[0] == "-a"))
                order = args[i++][1];
            else
                order = 'a';

            type = args[i++][1];
            outName = args[i++];

            inputLength = args.Length - i;
            inputs = new string[inputLength];
            for (int j = i, k = 0; j < args.Length; j++, k++)
                inputs[k] = args[j];
        } 
    }

    class ThreadRunner
    {
        static EventWaitHandle[] waitHandles; 
        private string[] inputs;
        private string output;
        public string[] results;

        public ThreadRunner(string output, string[] inputs)
        {
            this.inputs = new string[inputs.Length];
            this.inputs = inputs;
            this.output = output;
            results = new string[inputs.Length];
            waitHandles = new EventWaitHandle[inputs.Length];
            for (int i = 0; i < inputs.Length; i++)
                waitHandles[i] = new AutoResetEvent(false);
        }

        public int FindMinIndex(List<PeekableScanner> collection)
        {
            int rIndex = 0;
            for (int i = 0; i < collection.Count; i++)
            {
                if (collection[i] == null)
                {
                    if (rIndex == i)
                        rIndex++;
                    continue;
                }
                    
                if (collection[i].CompareTo(collection[rIndex]) < 0)
                    rIndex = i;
            }
            return (rIndex >= collection.Count) ? -1 : rIndex;
        }

        //public void Run()
        //{
        //    for (int i = 0; i < inputs.Length; i++)
        //    {
        //        ThreadData td = new ThreadData();
        //        td.input = inputs[i];
        //        td.index = i;

        //        Thread th = new Thread(ReadNext);
        //        th.Start(td);
        //        Thread.Sleep(50);
        //    }

        //    int k;
        //    using (StreamWriter sw = new StreamWriter(output))
        //        while (results.Any(x => x != null))
        //        {
        //            k = FindMinIndex(results);
        //            sw.WriteLine(results[k]);
        //            waitHandles[k].Set();
        //        }
        //}

        public void Merge(MergeTask task)
        {
            //PeekableScanner smallest;

            //using (StreamWriter sw = new StreamWriter(output))
            //{
            //    Queue<PeekableScanner> q = new Queue<PeekableScanner>(inputs.Length);
            //    foreach(string input in inputs)
            //    {
            //        PeekableScanner ps = new PeekableScanner(input);
            //        q.Enqueue(ps);
            //    }

            //    smallest = q.ToList<PeekableScanner>()[FindMinIndex(q.ToList<PeekableScanner>())];
            //    while (smallest != null)
            //    {
            //        if (smallest.peek() != null)
            //        {
            //            sw.WriteLine(smallest.nextString());
            //            Console.WriteLine(smallest.nextString());
            //        }

            //        if (smallest.hasNext())
            //        {
            //            q.Enqueue(smallest);
            //        }
            //    }

            //    smallest = q.Dequeue();
            //}
            //Queue<MyScanner> scanner = new Queue<MyScanner>(inputs.Length);
            //using (StreamWriter sw = new StreamWriter(output))
            //{
            //    int i = 0;
            //    foreach (string input in inputs)
            //    {
            //        MyScanner ms = new MyScanner(input, i++);
            //        scanner.Enqueue(ms);
            //        foreach (string line in ms.GetEnum(input))
            //        {
            //            results[i] = line;
            //        }
            //    }


            //}


        }

        public static IEnumerable<string> GetNextLine()
        {
            string line;
            using (StreamReader sr = new StreamReader(""))
                while ((line = sr.ReadLine()) != null)
                    yield return line;
            yield return null;
        }

        public void ReadNext(object threadData)
        {
            ThreadData td = new ThreadData();
            td = (ThreadData)threadData;
            string line;

            using (StreamReader sr = new StreamReader(td.input))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    lock (results)
                    {
                        results[td.index] = line;
                    }
                    waitHandles[td.index].WaitOne();
                }
                results[td.index] = null;
            }
        }
    }

    public class ThreadData
    {
        public string input;
        public int index; 
    }

    public class PeekableScanner : IComparable<PeekableScanner>
    {
        private Scanner scan;
        private string next;

        public PeekableScanner(string source)
        {
            scan = new Scanner(source);
            next = (scan.hasNext() ? scan.nextString() : null);
        }

        public bool hasNext()
        {
            return (next != null);
        }

        public string nextString()
        {
            string current = next;
            next = (scan.hasNext() ? scan.nextString() : null);
            return current;
        }

        public string peek()
        {
            return next;
        }

        public int CompareTo(PeekableScanner other)
        {
            if (String.Compare(peek(), other.peek()) == 0)
                return 0;
            else if (String.Compare(peek(), other.peek()) > 0)
                return 1;
            else
                return -1;
        }
    }

    class Scanner : StringReader
    {
        string currentWord;
        private string source;

        public Scanner(string source) : base(source)
        {
            readNextWord();
            this.source = source;
        }

        private void readNextWord()
        {
            //System.Text.StringBuilder sb = new System.Text.StringBuilder();
            //char nextChar;
            //int next;
            //do
            //{
            //    next = this.Read();
            //    if (next < 0)
            //        break;
            //    nextChar = (char)next;
            //    if (char.IsWhiteSpace(nextChar))
            //        break;
            //    sb.Append(nextChar);
            //} while (true);
            //while ((this.Peek() >= 0) && (char.IsWhiteSpace((char)this.Peek())))
            //    this.Read();
            //if (sb.Length > 0)
            //    currentWord = sb.ToString();
            //else
            //    currentWord = null;
            using (StreamReader sr = new StreamReader(source))
            {
                currentWord = sr.ReadLine() != null ? currentWord : null;
            }
        }

        public bool hasNextInt()
        {
            if (currentWord == null)
                return false;
            int dummy;
            return int.TryParse(currentWord, out dummy);
        }

        public int nextInt()
        {
            try
            {
                return int.Parse(currentWord);
            }
            finally
            {
                readNextWord();
            }
        }

        public string nextString()
        {
            try
            {
                return currentWord.ToString();
            }
            finally
            {
                readNextWord();
            }
        }

        public bool hasNext()
        {
            return currentWord != null;
        }
    }

    class MergeTask
    {
        string output;
        List<string> inputs;

        public MergeTask(string[] inputs, string output)
        {
            this.output = output;
            this.inputs = inputs.ToList();
        }
    }

    //class MyScanner : IEnumerable
    //{
    //    string name;
    //    int index;

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        //string line;
    //        //using (StreamReader sr = new StreamReader(source))
    //        //    while ((line = sr.ReadLine()) != null)
    //        //        yield return line;
    //        //yield return null;
    //        return (IEnumerator) GetEnumerator();
    //    }
    //}
}