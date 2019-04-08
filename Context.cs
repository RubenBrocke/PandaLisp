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
        public Dictionary<string,Primary> Idents;

        public Context()
        {
            AstList = new Dictionary<string,BaseType>();
            AddNativeFun("+");
            AddNativeFun("-");
            AddNativeFun("*");
            AddNativeFun("/");
            AddNativeFun("if");
            AddNativeFun("=");
            AddNativeFun("concat");

            Idents = new Dictionary<string,Primary>();
        }

        public Context(Context context) : this()
        {
            AstList = context.AstList;
        }

        private void AddNativeFun(string ident)
        {
            AstList.Add(ident, new Function(ident, null) { IsNative = true });
        }

        public string PrintContext()
        {
            string result = "";
            result += "AstList: \n";
            result += string.Join('\n', AstList.Select(n => n.Key));
            result += "\nidents: \n";
            result += string.Join('\n', Idents.Select(n => n.Key + ": " + n.Value));
            return result;
        }
    }
}