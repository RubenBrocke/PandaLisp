using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PandaLisp
{
    public class Interpreter : IVisitor<Primary>
    {
        public Context currentContext;
        public Interpreter()
        {
            currentContext = new Context();
        }

        public Primary VisitRoot(Root basetype)
        {
            List<Primary> results = new List<Primary>();
            foreach(Primary p in basetype.Lisps)
            {
                results.Add(p.Accept(this));
            }
            return new Lisp(null, results);
        }

        public Primary VisitLisp(Lisp basetype)
        {
            List<Primary> results = new List<Primary>();
            if (basetype.Function != null)
            {
                results.Add(basetype.Function.Accept(this));
            }
            else if (basetype.Primaries[0] is Identifier i && currentContext.AstList.First(n => n.Key.Equals(i.Value)).Value is Function f)
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
            return new Lisp(null, results);
        }

        public Primary VisitCall(Function function, Lisp args)
        {
            // Start pattern matching
            foreach(Pattern p in function.Patterns)
            {
                Context previousContext = currentContext;
                currentContext = new Context(previousContext);

                if (p.Matcher.Primaries.Count() != args.Primaries.Count())
                    continue;

                // Match with Matcher (first gets to start first)
                // When an identifier is found it gets the args value
                bool isMatch = true;
                for (int i = 0; i < args.Primaries.Count(); i++)
                {

                    // Check for identifier (to set) and make unique too
                    if (p.Matcher.Primaries[i] is Identifier ident)
                    {
                        System.Console.WriteLine("Added " + (string)ident.Value + " to context");
                        currentContext.Idents.Add((string)ident.Value, args.Primaries[i].Accept(this));
                    }
                    else
                    {
                        // Check for equality 
                        if (!p.Matcher.Primaries[i].Accept(this).Value.Equals(args.Primaries[i].Accept(this).Value))
                        {
                            System.Console.WriteLine("test1 " + p.Matcher.Primaries[i].Accept(this));
                            System.Console.WriteLine("test2 " + args.Primaries[i].Accept(this));
                            isMatch = false;
                        }
                    }
                }
                if (!isMatch) continue;

                // Run Result
                System.Console.WriteLine("Running result");
                Primary result = p.Result.Accept(this);
                currentContext = previousContext;
                return new Lisp(null, new List<Primary>() { result });
            }
            throw new CompilerException("Could not match function with args " + args.Primaries);
        }

        public Primary VisitFunction(Function basetype)
        {
            currentContext.AstList.Add(basetype.Identifier, basetype);
            List<object> results = new List<object>();

            return new Lisp(null, null);
        }

        public Primary VisitMatcher(Matcher basetype)
        {
            return null;
        }

        public Primary VisitPattern(Pattern basetype)
        {
            return null;
        }

        public Primary VisitPrimary(Primary basetype)
        {
            return null;
        }
        public Primary VisitNumber(Number basetype)
        {
            return basetype;
        }
        public Primary VisitString(String basetype)
        {
            return basetype;
        }
        public Primary VisitIdentifier(Identifier basetype)
        {
            foreach (object o in currentContext.Idents.Keys)
            {
                System.Console.WriteLine(o);
            }
            if (currentContext.Idents.Any(n => n.Key == (string)basetype.Value))
            {
                return currentContext.Idents.First(n => n.Key == (string)basetype.Value).Value;
            }
            return basetype;
        }
        public Primary VisitBoolean(Boolean basetype)
        {
            return basetype;
        }

        public Primary VisitNativeCall(Function function, Lisp args)
        {
            List<Primary> values = args.Primaries;
            switch (function.Identifier)
            {
                case "=":
                    for (int i = 0; i < values.Count() - 1; i++)
                    {
                        if (values[i] != values[i + 1])
                        {
                            return new Boolean(false);
                        }
                    }
                    return new Boolean(true);
                case "+":
                    return NativePlus(values.ToArray());
                case "-":
                    return NativeMinus(values.ToArray());
                case "/":
                    return NativeDivision(values.ToArray());
                case "*":
                    return NativeMultiply(values.ToArray());
                case "if":
                    return NativeIf(values.ToArray());
            }
            throw new CompilerException("Native function " + function.Identifier + " is not known");
        }

        private Number NativePlus(params Primary[] args)
        {
            int? result = null;
            for (int i = 0; i < args.Length; i++)
            {
                // Check for basic int
                var value = args[i].Accept(this);
                if (value as Number != null)
                {
                    // Check for first value
                    if (result == null)
                        result = (int)value.Value;
                    else
                        result += (int)value.Value;
                }
                // Else recurse with the object list
                else
                {
                    // Check for first value
                    if (result == null)
                        result = (int)NativePlus(args[i]).Value;
                    else
                        result += (int)NativePlus(args[i]).Value;
                }
            }
            return new Number((int)result);
        }

        private Number NativeMinus(params Primary[] args)
        {
            int? result = null;
            for (int i = 0; i < args.Length; i++)
            {
                // Check for basic int
                var value = args[i].Accept(this);
                if (value as Number != null)
                {
                    // Check for first value
                    if (result == null)
                        result = (int)value.Value;
                    else
                        result -= (int)value.Value;
                }
                // Else recurse with the object list
                else
                {
                    // Check for first value
                    if (result == null)
                        result = (int)NativeMinus(args[i]).Value;
                    else
                        result -= (int)NativeMinus(args[i]).Value;
                }
            }
            return new Number((int)result);
        }

        private Number NativeMultiply(params Primary[] args)
        {
            int? result = null;
            for (int i = 0; i < args.Length; i++)
            {
                // Check for basic int
                var value = args[i].Accept(this);
                if (value as Number != null)
                {
                    // Check for first value
                    if (result == null)
                        result = (int)value.Value;
                    else
                        result *= (int)value.Value;
                }
                // Else recurse with the object list
                else
                {
                    // Check for first value
                    if (result == null)
                        result = (int)NativeMultiply(args[i]).Value;
                    else
                        result *= (int)NativeMultiply(args[i]).Value;
                }
            }
            return new Number((int)result);
        }

        private Number NativeDivision(params Primary[] args)
        {
            int? result = null;
            for (int i = 0; i < args.Length; i++)
            {
                // Check for basic int
                var value = args[i].Accept(this);
                if (value as Number != null)
                {
                    // Check for first value
                    if (result == null)
                        result = (int)value.Value;
                    else
                        result /= (int)value.Value;
                }
                // Else recurse with the object list
                else
                {
                    // Check for first value
                    if (result == null)
                        result = (int)NativeDivision(args[i]).Value;
                    else
                        result /= (int)NativeDivision(args[i]).Value;
                }
            }
            return new Number((int)result);
        }
    
        private Primary NativeIf(params Primary[] args)
        {
            if (args.Length != 3 && args.Length != 2)
                throw new CompilerException("If only has 2 or 3 options");

            if ((bool)args[0].Accept(this).Value)
            {
                return args[1].Accept(this);
            }
            else if (args.Length == 3)
            {
                return args[2].Accept(this);
            }
            throw new CompilerException("Could not execute if");
        }
    }
}