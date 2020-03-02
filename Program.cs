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

                while (true)
                {
                    input = Console.ReadLine();
                    if (input == "exit")
                        return;
   
                   // else if (!System.Text.RegularExpressions.Regex.IsMatch(input, @"^ ([-+] ? ? (\d +|\(\g < 1 >\))( ?[-+*\/] ?\g < 1 >)?)$"))
                  //     Console.WriteLine("Wrong math exxpression - please chek it. And try again.");

                    System.Text.RegularExpressions.Regex rgx = new System.Text.RegularExpressions.Regex(@"^([-+]? ?(\d+|\(\g<1>\))( ?[-+*\/] ?\g<1>)?)$");
                    bool chk = rgx.IsMatch(input);

                    using RPN rpn = new RPN(input);
                    {
                        Console.WriteLine("RPN: " + rpn.GetRpn);
                        Console.WriteLine("=" + rpn.Calculate.ToString());

                    }

                    //Perfekcyjne rozwiazanie ;P
                    //   Console.WriteLine(new System.Data.DataTable().Compute(input, null).ToString());

                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message + " " + ex.StackTrace);
                Console.ReadLine();
            }

        }

    }



    class RPN : IDisposable
    {

        // chyba lepiej zamineic na const enum
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
                 res = UInt64.TryParse(_input.Substring(_offset, pos - _offset), out _element);
                _offset = pos;
                
            }
            else
            {
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
            }


            return result;


        }

        public void Dispose()
        {
            
        }
    }
}





