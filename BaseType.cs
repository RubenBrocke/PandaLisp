using System;
using System.Collections.Generic;
using System.Linq;

namespace PandaLisp
{
    public interface IVisitor<T>
    {
        T VisitRoot(Root basetype);
        T VisitLisp(Lisp basetype);
        T VisitFunction(Function basetype);
        T VisitMatcher(Matcher basetype);
        T VisitPrimary(Primary basetype);
        T VisitPattern(Pattern basetype);
        T VisitCall(Function function, params Primary[] args);
        T VisitNativeCall(Function function, params Primary[] args);
        T VisitNumber(Number basetype);
        T VisitString(String basetype);
        T VisitIdentifier(Identifier basetype);
        T VisitBoolean(Boolean basetype);
    }
    public abstract class BaseType
    {
        public abstract T Accept<T>(IVisitor<T> visitor);
    }

    public class Root : BaseType
    {
        public List<Lisp> Lisps { get; set; }
        public Root(List<Lisp> lisps)
        {
            Lisps = lisps;
        }
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitRoot(this);
        }
    }

    public class Lisp : Primary
    {
        public Function Function;
        public List<Lisp> Lisps { get; set; }
        public List<Primary> Primaries { get; set; }
        public Lisp(Function function = null, List<Primary> primaries = null)
        {
            Function = function;
            Primaries = primaries;
        }
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitLisp(this);
        }

        public override string ToString(){
            if (Function != null)
            {
                return "(" + Function.Identifier + ")";
            }
            else if (Primaries?.Count > 0)
            {
                return "(" + string.Join(" ", Primaries.Select(n => n.ToString())) + ")";
            }
            return "()";
        }
    }

    public class Function : BaseType
    {
        public string Identifier;
        public List<Pattern> Patterns { get; set; }
        public bool IsNative = false;
        public Function(string identifier, List<Pattern> patterns)
        {
            Identifier = identifier;
            Patterns = patterns;
        }

        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitFunction(this);
        }

        public T AcceptCall<T>(IVisitor<T> visitor, params Primary[] args)
        {
            if (IsNative)
                return visitor.VisitNativeCall(this, args);
            return visitor.VisitCall(this, args);
        }
    }

    public class Pattern : BaseType
    {
        public Matcher Matcher;
        public Lisp Result;
        public Pattern(Matcher matcher, Lisp result)
        {
            Matcher = matcher;
            Result = result;
        }
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitPattern(this);
        }
    }

    public class Matcher : BaseType
    {
        public List<Primary> Primaries;
        public bool isEmpty;

        public Matcher(List<Primary> primaries = null)
        {
            isEmpty = primaries == null;
            Primaries = primaries;
        }
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitMatcher(this);
        } 
    }

    abstract public class Primary : BaseType
    {
        public object Value;
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitPrimary(this);
        } 
    }

    public class Number : Primary
    {
        public Number(int value)
        {
            Value = value;
        }
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitNumber(this);
        } 

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public class String : Primary
    {
        public String(string value)
        {
            Value = value;
        }
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitString(this);
        } 

        public override string ToString()
        {
            return (string)Value;
        }
    }

    public class Identifier : Primary
    {
        public Identifier(string value)
        {
            Value = value;
        }
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitIdentifier(this);
        } 

        public override string ToString()
        {
            return Value == null ? "" : (string)Value;
        }
    }

    public class Boolean : Primary
    {
        public Boolean(bool value)
        {
            Value = value;
        }
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.VisitBoolean(this);
        }

        public override string ToString()
        {
            return (bool)Value == true ? "true" : "false";
        }
    }

    public class Null : Primary
    {
        public override string ToString()
        {
            return "";
        }
    }
}