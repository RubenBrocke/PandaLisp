using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PandaLisp
{
    public class Context
    {
        public Dictionary<string,BaseType> AstList;
        public Dictionary<string,object> Idents;

        public Context()
        {
            AstList = new Dictionary<string,BaseType>();
            AstList.Add("+", new Function("+", null) { IsNative = true });
            AstList.Add("-", new Function("-", null) { IsNative = true });

            Idents = new Dictionary<string,object>();
        }

        public Context(Context context) : this()
        {
            AstList = context.AstList;
        }
    }
}