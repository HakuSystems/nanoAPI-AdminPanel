using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nanoAPIAdminPanel
{
    public static class Program
    {
        [STAThreadAttribute]
        public static void Main()
        {
            Console.WriteLine("Init");
            Console.ReadKey(true);

            App.Main();
        }
    }
}
