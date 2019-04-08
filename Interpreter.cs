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
            Primary result;
            if (basetype.Primaries?.Count ==0)
                return new Null();

            if (basetype.Function != null)
            {
                return basetype.Function.Accept(this);
            }
            else if (basetype.Primaries[0] is Identifier i && currentContext.AstList.FirstOrDefault(n => n.Key.Equals(i.Value)).Value is Function f)
            {
                // Interprete primaries before call with args (unless if)
                if (f.Identifier != "if")
                {
                    List<Primary> args = new List<Primary>();
                    List<Primary> oldargs = basetype.Primaries.GetRange(1, basetype.Primaries.Count() - 1);
                    foreach(Primary p in oldargs)
                    {
                        args.Add(p.Accept(this));
                    }
                    return f.AcceptCall(this, args.ToArray());
                }
                return f.AcceptCall(this, basetype.Primaries.GetRange(1, basetype.Primaries.Count() - 1).ToArray());
            }
            else
            {
                // Check for lonely lisp
                if (basetype.Primaries.Count == 1) return basetype.Primaries[0].Accept(this);
                
                List<Primary> ps = new List<Primary>();
                foreach (Primary p in basetype.Primaries)
                {
                    ps.Add(p.Accept(this));
                }
                return new Lisp(null, ps);
            }
        }

        public Primary VisitCall(Function function, params Primary[] args)
        {
            // Start pattern matching
            foreach(Pattern p in function.Patterns)
            {
                Context previousContext = currentContext;
                currentContext = new Context(previousContext);
                //System.Console.WriteLine("Changed context for function " + function.Identifier + " with arg count " + args.Length);

                if (p.Matcher.Primaries.Count() != args.Length)
                    continue;

                // Match with Matcher (first gets to start first)
                // When an identifier is found it gets the args value
                bool isMatch = true;
                for (int i = 0; i < args.Length; i++)
                {

                    // Check for identifier (to set) and make unique too
                    if (p.Matcher.Primaries[i] is Identifier ident)
                    {
                        //System.Console.WriteLine("Added " + (string)ident.Value + " to context with value " + args[i].Accept(this));
                        currentContext.Idents.Add((string)ident.Value, args[i].Accept(this));
                    }
                    else
                    {
                        // Check for equality 
                        if (!p.Matcher.Primaries[i].Accept(this).Value.Equals(args[i].Accept(this).Value))
                        {
                            isMatch = false;
                        }
                    }
                }
                if (!isMatch)
                {
                    currentContext = previousContext;
                    continue;
                }

                // Run Result
                Primary result = p.Result.Accept(this);
                currentContext = previousContext;
                return result;
            }
            throw new CompilerException("Could not match function with args " + args);
        }

        public Primary VisitFunction(Function basetype)
        {
            currentContext.AstList.Add(basetype.Identifier, basetype);
            List<object> results = new List<object>();

            return new Null();
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

        public Primary VisitNativeCall(Function function, params Primary[] args)
        {
            switch (function.Identifier)
            {
                case "=":
                    return NativeEquals(args);
                case "+":
                    return NativePlus(args);
                case "-":
                    return NativeMinus(args);
                case "/":
                    return NativeDivision(args);
                case "*":
                    return NativeMultiply(args);
                case "if":
                    return NativeIf(args);
                case "concat":
                    return NativeConcat(args);
            }
            throw new CompilerException("Native function " + function.Identifier + " is not known");
        }

        private Boolean NativeEquals(params Primary[] args)
        {
            for (int i = 0; i < args.Count() - 1; i++)
            {
                if (!args[i].Accept(this).Value.Equals(args[i + 1].Accept(this).Value))
                {
                    return new Boolean(false);
                }
            }
            return new Boolean(true);
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

            if ((bool)NativeEquals(new Primary[] { new Boolean(true), args[0]}).Value)
            {
                return args[1].Accept(this);
            }
            else if (args.Length == 3)
            {
                return args[2].Accept(this);
            }
            return new Null();
        }

        private Primary NativeConcat(params Primary[] args)
        {
            List<Primary> concatList = new List<Primary>();
            foreach(Primary p in args)
            {
                if (p is Lisp l)
                {
                    foreach (Primary p2 in l.Primaries)
                    {
                        concatList.Add(p2);
                    }
                }
                else 
                {
                    concatList.Add(p);
                }
            }
            return new Lisp(null, concatList);
        }
    }
}