using QuickFix;
using QuickFix.Transport;
using QuickFix.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickFixManager.Data;

namespace QuickFixManager.Acceptor
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1) System.Environment.Exit(2);
            string file = args[0];
            IApplication application = new QuickFixService(file, QuickFixService.QuickFixType.Acceptor);
        }
    }
}
