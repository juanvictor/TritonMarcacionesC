using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TritonMarcacionesC
{
    public class Parameter
    {
        public string key { get; set; }
        public object value { get; set; }

        public Parameter()
        {
            this.key = null;
            this.value = null;
        }

        public Parameter(string key, object value)
        {
            this.key = key;
            this.value = value;
        }
    }
}
