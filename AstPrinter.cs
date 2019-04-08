using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PandaLisp
{
    public class AstPrinter : IVisitor<string>
    {
        public string VisitRoot(Root basetype)
        {
            return Parenthesize("Root", basetype.Lisps.ToArray());
        }

        public string VisitLisp(Lisp basetype)
        {
            if (basetype.Function != null)
                return Parenthesize("Lisp fun", basetype.Function);
            else
            {
                string returnString = "";
                foreach (Primary p in basetype.Primaries)
                {
                    returnString += Parenthesize("Primary", p);
                }

                return returnString;
            }
        }

        public string VisitFunction(Function basetype)
        {
            return Parenthesize("Function " + basetype.Identifier, basetype.Patterns.ToArray());
        }

        public string VisitCall(Function function, params Primary[] args)
        {
            return Parenthesize("Call " + function.Identifier, args);
        }

        public string VisitNativeCall(Function function, params Primary[] args)
        {
            return Parenthesize("Native Call " + function.Identifier, args);
        }

        public string VisitMatcher(Matcher basetype)
        {
            if (basetype.isEmpty)
                return Parenthesize("Matcher empty");
            else 
                return Parenthesize("Matcher Primary", basetype.Primaries.ToArray());
        }

        public string VisitPattern(Pattern basetype)
        {
            string returnString = "";
            returnString += basetype.Matcher.Accept(this);
            returnString += Parenthesize("Result", basetype.Result);
            return returnString;
        }

        public string VisitPrimary(Primary basetype)
        {
            return Parenthesize("Primary " + basetype.Value);
        }
        public string VisitNumber(Number basetype)
        {
            return Parenthesize("Number " + basetype.Value);
        }
        public string VisitString(String basetype)
        {
            return Parenthesize("String " + basetype.Value);
        }
        public string VisitIdentifier(Identifier basetype)
        {
            return Parenthesize("Identifier " + basetype.Value);
        }
        public string VisitBoolean(Boolean basetype)
        {
            return Parenthesize("Boolean " + basetype.Value);
        }

        private string Parenthesize(string name, params BaseType[] basetypes)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("(").Append(name);
            foreach (BaseType basetype in basetypes)
            {
                stringBuilder.Append(" ");
                stringBuilder.Append(basetype.Accept(this));
            }
            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }
    }
}