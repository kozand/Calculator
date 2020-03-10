using System;
using System.Collections.Generic;
using System.Linq;


namespace ConsoleApp2
{
    class Program
    {
        static void Main()
        {
            try
            {
                string input;

                Console.WriteLine("Calculator App.");
                Console.WriteLine("Supported operators: + - * /");
                Console.WriteLine("suported variables: UInt64.");
                Console.WriteLine("Please write a math expression as in example and press enter:  6 + 55 * 5 - 1 ");
                Console.WriteLine("For exit - type 'exit' and press enter.");
                while (true)
                {
                    try
                    {
                        input = Console.ReadLine();
                        if (input == "exit")
                            return;

                        using RPN rpn = new RPN(input);
                        {
                            if (rpn.CheckInputExp())
                            {

                                //  Console.WriteLine("RPN: " + rpn.GetRpn);
                                Console.WriteLine("=" + rpn.Calculate.ToString());
                            }
                            else
                                Console.WriteLine("Wrong math expression - please chek it, and try again.");


                        }
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }



        }

                //Perfekcyjne rozwiazanie ;P
                //   Console.WriteLine(new System.Data.DataTable().Compute(input, null).ToString());
            }

            catch (Exception ex)
            {
                Console.WriteLine( "Error: " + ex.Message );
                Console.ReadLine();
            }

        }

    }



    class RPN : IDisposable
    {

       
        public static Dictionary<string, int> OpPrio = new Dictionary<string, int>
        {
            {"*", 2 },
            {"/", 2 },
            {"+", 1 },
            {"-", 1 }
        };


        readonly string cmd;
        Queue<string> NumColl = new Queue<string>();
        Stack<string> OpCollc = new Stack<string>();
        Queue<string> OutputQ = new Queue<string>();


        public RPN(string _cmd) 
        {
            cmd = System.Text.RegularExpressions.Regex.Replace(_cmd, @"\s+", "");
            ParceStr(cmd);
        }

        public string GetRpn
        {
            get
            {
                return string.Join<string>(" ", OutputQ.ToList());
            }
        }

        public Double Calculate
        {
            get { return Calc(OutputQ); }
        }


        int GetPrecedence(string opr1, string opr2)
        {
            if (!OpPrio.ContainsKey(opr1))
                throw new Exception("Unsported operator!!");
            if (!OpPrio.ContainsKey(opr2))
                throw new Exception("Unsported operator!!");

            if (OpPrio[opr1] > OpPrio[opr2])
                return -1;

            if (OpPrio[opr1] <= OpPrio[opr2])
                return 1;

            return 0;
        }

        public bool CheckInputExp()
        {
            var regex = new System.Text.RegularExpressions.Regex(@"(?x)^(?> (?<p> \( )* (?>-?\d+(?:\.\d+)?) (?<-p> \) )* )(?>(?:[-+*/](?> (?<p> \( )* (?>-?\d+(?:\.\d+)?) (?<-p> \) )* ))*)(?(p)(?!))$");

            if (regex.IsMatch(cmd) && !OpPrio.ContainsValue(cmd[0]))
                return true;
            else
                return false;

        }

        bool IsElement(string _input)
        {
            UInt64 res;
            return UInt64.TryParse(_input, out res);
        }

        char[] GetOperators()
        {
            char[] op = new char[OpPrio.Count];
            List<string> tmp = OpPrio.Keys.ToList<string>();
            int i = 0;

            foreach ( string s in tmp)
            {
                op[i] = s[0];
                i++;
            }
            return op;
 
        }

        bool IsElement(string _input, ref int _offset, out ulong _element)
        {
            int pos = _input.IndexOfAny(GetOperators(), _offset);
            
            bool res;

            if (pos > 0)
            {
                if (UInt64.MaxValue.ToString().Length < pos - _offset)
                    throw new Exception("Too big UInt64 value. Try again.");

                res = UInt64.TryParse(_input.Substring(_offset, pos - _offset), out _element);
                _offset = pos;
                
            }
            else
            {
                if (UInt64.MaxValue.ToString().Length < pos - _offset)
                    throw new Exception("Too big UInt64 value. Try again.");

                res = UInt64.TryParse(_input.Substring(_offset, _input.Length - _offset), out _element);
                _offset = _offset + (_input.Length - _offset);
            }

            return res;
        }

        bool IsElement(string _input, int length)
        {
            UInt64 res;
            return UInt64.TryParse(_input, out res);
        }


        bool IsOperator(string _input)
        {

            return OpPrio.ContainsKey(_input);

        }


        void ParseChar(string _cmd)
        {


            foreach (char ch in _cmd)
            {

                if (IsElement(ch.ToString()))
                {
                    NumColl.Enqueue(ch.ToString());
                    OutputQ.Enqueue(ch.ToString());
                }

                else if (IsOperator(ch.ToString()))
                {
                    int i = 0;
                    while (OpCollc.Count > 0 && OpCollc.Count >= i && GetPrecedence(ch.ToString(), OpCollc.Peek().ToString()) == 1)
                    {
                        OutputQ.Enqueue(OpCollc.Pop());
                        i++;
                    }

                    OpCollc.Push(ch.ToString());

                }

            }

            while (OpCollc.Count > 0)
                OutputQ.Enqueue(OpCollc.Pop());
            
        }



        void ParceStr(string _cmd)
        {
            int offset = 0;
            ulong el;

            while (_cmd.Length > offset )
            {

                if (IsElement(_cmd,ref offset, out el))
                {
                    NumColl.Enqueue(el.ToString());
                    OutputQ.Enqueue(el.ToString().ToString());
                    
                }
                if (_cmd.Length > offset  && IsOperator(_cmd[offset].ToString()))
                {
                    int i = 0;
                    while (OpCollc.Count > 0 && OpCollc.Count >= i && GetPrecedence(_cmd[offset].ToString(), OpCollc.Peek().ToString()) == 1)
                    {
                        OutputQ.Enqueue(OpCollc.Pop());
                        i++;
                    }

                    OpCollc.Push(_cmd[offset].ToString());
                    offset++;
                }
                
            }

            while (OpCollc.Count > 0)
                OutputQ.Enqueue(OpCollc.Pop());

        }

        Double Calc(Queue<string> _rpn)
        {
            Double arg1;
            Double arg2;


            LinkedList<double> temp = new LinkedList<double>();



            while (_rpn.Count > 0)
            {
                if (IsOperator(_rpn.Peek()))
                {
                    arg1 = temp.Last.Value;
                    temp.RemoveLast();
                    arg2 = temp.Last.Value;
                    temp.RemoveLast();

                    temp.AddLast(
                        Exe(_rpn.Dequeue(), arg2, arg1));

                }
                else
                    temp.AddLast(Convert.ToUInt64(_rpn.Dequeue()));
                

            }

            return temp.First.Value;
        }



        Double Exe(string _op, Double _arg1, Double _arg2)
        {
            UInt64 result = 0;

            switch (_op)
            {
                case "+":
                    return _arg1 + _arg2;
                case "*":
                    return _arg1 * _arg2;
                case "/":
                    return _arg1 / _arg2;
                case "-":
                    return _arg1 - _arg2;
            }


            return result;


        }

        public void Dispose()
        {
            
        }
    }



    abstract class  Arg 
    {

        //public Arg(string _arg)
        //{


        //}


        public  virtual  Arg Parse(string _arg)
        {
            return this;

        }

    }


    class ArgInt: Arg
    {
        long arg;

        public ArgInt(string _arg)
        {
            Parse(_arg);
        }

        void Parse (string _arg)
        {
            if (!long.TryParse(_arg, out arg))
                throw new Exception("Error on INT64 parsing argument.");
        }


    }


    class ArgDouble : Arg
    {
        double arg;

        public ArgDouble(string _arg)
        {
            Parse(_arg);
        }

        void Parse(string _arg)
        {
            if (!double.TryParse(_arg, out arg))
                throw new Exception("Error on INT64 parsing argument.");
        }

    }

    class Operator:Arg
    {

        public Operator(string _op)
        {


        }

        void Parse(string _op)
        {
            
            
        }


    }
}





