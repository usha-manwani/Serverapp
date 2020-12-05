using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyExecution
{
    class Program
    {
        static void Main(string[] args)
        {
            StrategyExec sec= new StrategyExec();
            sec.GetData("16:00:00");
            Console.Read();
        }
    }
}
