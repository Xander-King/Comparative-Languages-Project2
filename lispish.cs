// <Program> ::= {<SExpr>}
// <SExpr> ::= <Atom> | <List>
// <List> ::= () | ( <Seq> )
// <Seq> ::= <SExpr> <Seq> | <SExpr>
// <Atom> ::= ID | INT | REAL | STRING


using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class LispishParser {
   
    public class Parser {

        List<Node> tokens = new List<Node>(); 

        int cur = 0;

        public Parser(Node [] tokens) {
            this.tokens = new List<Node>(tokens);
            this.tokens.Add(new Node(Symbols.INVALID, ""));
        }

        public Node ParseProgram() {
        //  <Program> ::= {<SExpr>}
            var children = new List<Node>();
            //Console.WriteLine($"{tokens[0].Text}");
            while (tokens[cur].Symbol != Symbols.INVALID) {
                
                children.Add(ParseSExpr());
                // Console.WriteLine("here");
            }
            //Console.WriteLine("here");
            return new Node(Symbols.Program, children.ToArray());
        }

        public Node ParseSExpr() {
        // <SExpr> ::= <Atom> | <List>
         // <Atom> ::= ID | INT | REAL | STRING
            if (isAtom(cur)) {
                //Console.WriteLine($"{cur}");
                //Console.WriteLine($"{tokens[cur].Symbol}");
                return new Node(Symbols.SExpr, ParseAtom());
            } else {
                //Console.WriteLine($"{tokens[cur].Symbol}");
                //Console.WriteLine("here");
                return new Node(Symbols.SExpr, ParseList());
            }
        }

        public Node ParseList() {
        // <List> ::= () | ( <Seq> )
            var lparam = ParseLiteral("(");
           // Console.WriteLine($"{cur}");
            var seq = ParseSeq();

           // Console.WriteLine($"{cur}");
            var rparam = ParseLiteral(")");

            // Console.WriteLine($"{cur}");
            return new Node(Symbols.List, lparam, seq, rparam);
            // if (tokens[cur + 1].Symbol == Symbols.Seq) {
            //     return new Node(Symbols.List, lparam, seq, rparam);
            // } else {
            //     return new Node(Symbols.List, lparam, rparam);
            // }
            
            
        }

        private bool isAtom(int ix) {
           return tokens[ix].Symbol == Symbols.ID || tokens[ix].Symbol == Symbols.INT || tokens[ix].Symbol == Symbols.REAL || tokens[ix].Symbol == Symbols.STRING;
        }

        public Node ParseSeq() {
        // <Seq> ::= <SExpr> <Seq> | <SExpr>
            Node expr = ParseSExpr();
            if (isAtom(cur)||tokens[cur].Text == "(") {
                return new Node(Symbols.Seq, expr, ParseSeq());

            } else {
                // Console.WriteLine($"else {cur}");
                return new Node(Symbols.Seq,expr);
                //Console.WriteLine($"{cur}");
            }
        }

        public Node ParseAtom() {
            // <Atom> ::= ID | INT | REAL | STRING
            if (isAtom(cur)) {
            // Console.WriteLine($"{tokens[cur].Text}");
                return new Node(Symbols.Atom, tokens[cur++]);
            } else {
                return null;
            }
        }

        public Node ParseLiteral(string lit) {
            if (tokens[cur].Text == lit) {
                return tokens[cur++];
            } else {
                throw new Exception("Syntax error");
            }
        }

        
    }

    public enum Symbols {
        WS, LITERAL, REAL, INT, STRING, ID,
        Program, SExpr, List, Seq, Atom, INVALID
    }

    public class Node {
        public string Text="";
        List<Node> children = new List<Node>();
        public Symbols Symbol;
        // public void Print(string prefix = "")
        public Node(Symbols symbol, string text) {
            this.Symbol = symbol;
            this.Text = text;
        }

        public Node(Symbols symbol, params Node[] children) {
            this.Symbol = symbol;
            this.Text = "";
            this.children = new List<Node>(children);
        }
       
        public void Print(string prefix = "") {

            Console.WriteLine($"{prefix}{Symbol.ToString().PadRight(42-prefix.Length)} {Text}");
            foreach (var child in children) {
                child.Print(prefix+"   ");
            }
        }
       

    }



    static public List<Node> Tokenize(String src) {
        
        var result = new List<Node>();
        int pos = 0;
        Match m;

        var WS = new Regex(@"\G\s");
        var LITERAL = new Regex(@"\G[\(\)]");
        var REAL = new Regex(@"\G[+-]?[0-9]*\.[0-9]+");
        var INT = new Regex(@"\G[+-]?[0-9]+");
        var STRING = new Regex(@"\G""(?>\\.|[^\\""])*""");
        var ID = new Regex(@"\G[^\s""\(\)]+");
       
        while(pos < src.Length) {
            if ((m = WS.Match(src, pos)).Success) { 
                pos += m.Length;
            } else if ((m = LITERAL.Match(src, pos)).Success) {
                result.Add(new Node(Symbols.LITERAL, m.Value));
                pos += m.Length;
            } else if ((m = REAL.Match(src, pos)).Success) {
                result.Add(new Node(Symbols.REAL, m.Value));
                pos += m.Length;
            } else if ((m = INT.Match(src, pos)).Success) {
                result.Add(new Node(Symbols.INT, m.Value));
                pos += m.Length;
            } else if ((m = STRING.Match(src, pos)).Success) {
                result.Add(new Node(Symbols.STRING, m.Value));
                pos += m.Length;
            } else if ((m = ID.Match(src, pos)).Success) {
                result.Add(new Node(Symbols.ID, m.Value));
                pos += m.Length;
            } else {
                throw new Exception("Lexer error");
            }
        }

        return result;
    }

    static public Node Parse(Node[] tokens) {
        var p = new Parser(tokens);
        var tree = p.ParseProgram();
        return tree;
    }

    static private void CheckString(string lispcode) {
        try
        {
            Console.WriteLine(new String('=', 50));
            Console.Write("Input: ");
            Console.WriteLine(lispcode);
            Console.WriteLine(new String('-', 50));

            Node[] tokens = Tokenize(lispcode).ToArray();

            Console.WriteLine("Tokens");
            Console.WriteLine(new String('-', 50));
            foreach (Node node in tokens)
            {
                Console.WriteLine($"{node.Symbol,-21}: {node.Text}");
            }
            Console.WriteLine(new String('-', 50));
            //Console.WriteLine("here");
            Node parseTree = Parse(tokens);

            Console.WriteLine("Parse Tree");
            Console.WriteLine(new String('-', 50));
            parseTree.Print();
            Console.WriteLine(new String('-', 50));
        }
        catch (Exception ex)
        {
            
            Console.WriteLine("Threw an exception on invalid input.");
        }
    }


    public static void Main(string[] args)
    {
        //Here are some strings to test on in 
        //your debugger. You should comment 
        //them out before submitting!

        // CheckString(@"(define foo 3)");
         //CheckString(@"(define foo ""bananas"")");
        // CheckString(@"(define foo ""Say \\""Chease!\\"" "")");
        // CheckString(@"(define foo ""Say \\""Chease!\\)");
        // CheckString(@"(+ 3 4)");      
        // CheckString(@"(+ 3.14 (* 4 7))");
        // CheckString(@"(+ 3.14 (* 4 7)");

        CheckString(Console.In.ReadToEnd());
    }
}

