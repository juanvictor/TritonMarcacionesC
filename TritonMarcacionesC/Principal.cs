using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TritonMarcacionesC
{
    class Principal
    {
        static void Main(string[] args)
        {
            UtilBiometrico utilBiometrico = new UtilBiometrico();

            utilBiometrico.Marcaciones();

            //Console.ReadKey();
        }
    }
}
