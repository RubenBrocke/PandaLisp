//------------------
//--ANTLR GRAMMAR--
//------------------
grammar panda ;

root      : list+ ;
list      : '(' (function | primary+)')' ;

function  : 'fun' pattern;
defaults  : '+' | '-' | '*' | '/' | '%' | '&' | '|' | ;
pattern   : '[' (primary | '_')+ ']' list ;
primary   : NUMBER | STRING | 'true' | 'false' | 'nil' | list;

// Literals
IDENT     : [A-z]+ ;
NUMBER 	  : [0-9]+ ;
STRING 	  : '"'[A-z]+'"' ; 

// Default functions are:
// fun (arguments: name, patterns)
// operators (arguments: operands)

// Implicit file inclusion is a must (i think)
// Type inference with static types
// Traits (shapes?) (multiple inheritance with ambiguity checks)

// Hello World example
// (print "hello world")

// fibonacci example
// (fun fib 
//  [0] (0)
//  [1] (1)
//  [i] (+ (fib (- i 1)) (fib (- i 2)))
// )

// With a primary in the mather the = function is used
// The identifier used should be unique


// anoymous functions are possible (allows for conditionals)
//  (fun 
//   [0] (print "anonymous 0")
//   [1] (print "anonymous 1")
//  )