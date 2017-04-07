using QuickFix;
using QuickFix.Fields;
using QuickFix.Transport;
using QuickFixManager.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickFixManager.Initiator
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length != 1) System.Environment.Exit(2);

            string file = args[0];

            try
            {
                QuickFixService application = new QuickFixService(file, QuickFixService.QuickFixType.Initiator);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            Environment.Exit(1);
        }
    }
}
