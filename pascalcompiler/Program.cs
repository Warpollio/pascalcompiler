using System;
using System.IO;

namespace pascalcompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            StreamReader reader = new StreamReader(@"..\..\..\LexerTests\test1.txt");
            Lexer lexer = new Lexer(reader);
            Token next;

            string a = "1";
            while (a != "0")
            {
                next = lexer.GetNextLex();
                if (next.type != Type.COMMENT)
                {
                    Console.WriteLine(next);
                }
                if (next.type == Type.EOF) break;
                a = Console.ReadLine();
            }
        }
    }
}
