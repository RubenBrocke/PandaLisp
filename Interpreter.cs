using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PandaLisp
{
    public class Interpreter : IVisitor<object>
    {
        public Context currentContext;
        public Interpreter()
        {
            currentContext = new Context();
        }

        public object VisitRoot(Root basetype)
        {
            List<object> results = new List<object>();
            foreach(Lisp l in basetype.Lisps)
            {
                results.Add(l.Accept(this));
            }
            return results;
        }

        public object VisitLisp(Lisp basetype)
        {
            List<object> results = new List<object>();

            if (basetype.Function != null)
                results.Add(basetype.Function.Accept(this));
            else if (basetype.Primaries[0] is Identifier i && currentContext.AstList.First(n => n.Key.Equals(i.Accept(this))).Value is Function f)
            {
                results.Add(f.AcceptCall(this, new Lisp(null, basetype.Primaries.GetRange(1, basetype.Primaries.Count() - 1))));
            }
            else
            {
                foreach (Primary p in basetype.Primaries)
                {
                    results.Add(p.Accept(this));
                }
            }
            return results;
        }

        public object VisitCall(Function function, Lisp args)
        {
            // Start pattern matching
            // Get = function from Context
            foreach(Pattern p in function.Patterns)
            {
                if (p.Matcher.Primaries.Count() != args.Primaries.Count())
                    continue;

                // Match with Matcher (first gets to start first)
                // When an identifier is found it gets the args value
                bool isMatch = true;
                for (int i = 0; i < args.Primaries.Count(); i++)
                {
                    Context previousContext = currentContext;
                    currentContext = new Context(previousContext);

                    // Check for identifier (to set) and make unique too
                    if (p.Matcher.Primaries[i] is Identifier ident)
                    {
                        currentContext.Idents.Add(ident.Value, args.Primaries[i].Accept(this));
                    }
                    else
                    {
                        // Check for equality 
                        if (p.Matcher.Primaries[i].Accept(this) != args.Primaries[i].Accept(this))
                        {
                            System.Console.WriteLine("test1 " + p.Matcher.Primaries[i].Accept(this));
                            System.Console.WriteLine("test2 " + args.Primaries[i].Accept(this));
                            isMatch = false;
                        }
                    }
                    
                    currentContext = previousContext;
                }
                if (!isMatch) continue;

                // Run Result
                System.Console.WriteLine("Running result");
                System.Console.WriteLine(p.Result.Primaries[0]);
                System.Console.WriteLine(p.Result.Primaries[1]);
                System.Console.WriteLine(p.Result.Primaries[2]);
                return p.Result.Accept(this);
            }
            throw new CompilerException("Could not match function with args " + args.Primaries);
        }

        public object VisitFunction(Function basetype)
        {
            currentContext.AstList.Add(basetype.Identifier, basetype);
            List<object> results = new List<object>();

            foreach (Pattern p in basetype.Patterns)
            {
                results.Add(p.Accept(this));
            }

            return results;
        }

        public object VisitMatcher(Matcher basetype)
        {
            return null;
        }

        public object VisitPattern(Pattern basetype)
        {
            return null;
        }

        public object VisitPrimary(Primary basetype)
        {
            return null;
        }
        public object VisitNumber(Number basetype)
        {
            return Convert.ToInt32(basetype.Value);
        }
        public object VisitString(String basetype)
        {
            return basetype.Value;
        }
        public object VisitIdentifier(Identifier basetype)
        {
            System.Console.WriteLine("Ident: " + basetype.Value);
            System.Console.WriteLine("Context");
            foreach (object o in currentContext.Idents.Keys)
            {
                System.Console.WriteLine(o);
            }
            if (currentContext.Idents.Any(n => n.Key == basetype.Value))
            {
                return currentContext.Idents.First(n => n.Key == basetype.Value).Value;
            }
            return basetype.Value;
        }
        public object VisitBoolean(Boolean basetype)
        {
            if (basetype.Value == "true")
                return true;
            if (basetype.Value == "false")
                return false;
            throw new CompilerException("Boolean not true of false");
        }

        public object VisitNativeCall(Function function, Lisp args)
        {
            List<Primary> values = args.Primaries;
            switch (function.Identifier)
            {
                case "=":
                    for (int i = 0; i < values.Count() - 1; i++)
                    {
                        if (values[i] != values[i + 1])
                        {
                            return false;
                        }
                    }
                    return true;
                case "+":
                    int result = (int)values[0].Accept(this);
                    for (int i = 1; i < values.Count(); i++)
                    {
                        result += (int)values[i].Accept(this);
                    }
                    return result;
                case "-":
                    result = (int)values[0].Accept(this);
                    for (int i = 1; i < values.Count(); i++)
                    {
                        var test = (List<object>)values[i].Accept(this);
                        result -= (int)test.Aggregate((x, y) => (int)x - (int)y);
                    }
                    return result;
                case "/":
                    result = (int)values[0].Accept(this);
                    for (int i = 1; i < values.Count(); i++)
                    {
                        result /= (int)values[i].Accept(this);
                    }
                    return result;
                case "*":
                    result = (int)values[0].Accept(this);
                    for (int i = 1; i < values.Count(); i++)
                    {
                        result *= (int)values[i].Accept(this);
                    }
                    return result; 
            }
            throw new CompilerException("Native function " + function.Identifier + " is not known");
        }
    }
}