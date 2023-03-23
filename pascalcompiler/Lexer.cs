using System.IO;
using System;
using System.Collections.Generic;

namespace pascalcompiler
{

    class Lexer
    {
        public enum KeyWords
        {
            ABSOLUTE, AND, ARRAY, ASM, BEGIN, CASE, CONST, CONSTRUCTOR, DESTRUCTOR, DIV, DO, DOWNTO, ELSE, END,
            FILE, FOR, FUNCTION, GOTO, IF, IMPLEMENTATION, IN, INHERITED, INLINE, INTERFACE, LABEL, MOD, NIL, NOT,
            OBJECT, OF, OPERATOR, OR, PACKED, PROCEDURE, PROGRAM, RECORD, REINTRODUCE, REPEAT, SELF, SET, SHL, SHR,
            STRING, THEN, TO, TYPE, UNIT, UNTIL, USES, VAR, WHILE, WITH, XOR
        }
        public char[] specialSymbols = {'<', '>', '*', ':', '+', '-', '/', '(', ')', '.', '=', '\'', '[', ']', ',', '^', '@', '{', '}', '$', '#', '&', '%', ';'};
        public string[] specialPairs = { "<<", ">>", "**", "<>", "><", "<=", ">=", ":=", "+=", "-=", "*=", "/=", "(*", "*)", "(.", ".)", "//" };
        private StreamReader _reader;
        private string buffer;
        private char currentChar;
        
        public List<Token> tokens = new List<Token>();

        private int row = 1;
        private int pos = 0;

        public Lexer(StreamReader reader)
        {
            _reader = reader;
        }
        bool ReadNext()
        {
            var a = _reader.Read();
            pos += 1;
            if (a == -1)
            {
                return false;
            }

            var c = (char)a;
            currentChar = c;
            return true;
        }

        void AddBuff(char c)
        {
            buffer += c;
        }

        void ClearBuff()
        {
            buffer = "";
        }

        public void LexStart()
        {
            GetNextLex();
        }

        public Token AddToken(int row, int pos, Type type, object value, string source)
        {
            Token token = new Token();
            token.row = row;
            token.pos = pos;
            token.type = type;
            token.value = value;
            token.source = source;
            tokens.Add(token);
            return token;
        }

        public Token GetNextLex()
        {
            if (!ReadNext())
            {
                return AddToken(row, pos, Type.EOF, 0, "");
            }
            while(currentChar == ' ' || currentChar == '\t' || currentChar == '\0' || currentChar == '\r' || currentChar == '\n')
            {
                if (currentChar == '\n')
                {
                    ++row;
                    pos = 0;
                }
                if (!ReadNext())
                {
                    return AddToken(row, pos, Type.EOF, 0, "");
                }
            }

            if ((currentChar >= 'a' && currentChar <= 'z') || (currentChar >= 'A' && currentChar <= 'Z') || (currentChar == '_'))
            {
                ClearBuff();
                AddBuff(currentChar);
                return ScanId(pos);
            }
            else if ((currentChar >= '0' && currentChar <= '9') || currentChar == '$' || currentChar == '-' || currentChar == '&' || currentChar == '%')
            {
                ClearBuff();
                AddBuff(currentChar);
                if (currentChar == '$')
                {
                    return ScanNumber(true, pos);
                }
                else
                {
                    return ScanNumber (false, pos);
                }
            }
            else if (currentChar == '\'')
            {
                ClearBuff();
                AddBuff(currentChar);
                return ScanString(pos);
            }
            else if (Array.Exists(specialSymbols, c => c == currentChar))
            {
                ClearBuff();
                AddBuff(currentChar);
                return ScanSpecialSymbol(pos);
            }

            return new Token();
        }

        private Token ScanId(int lpos)
        {
            while (true)
            {
                char peek = (char)_reader.Peek();
                if ((peek >= 'a' && peek <= 'z') || (peek >= 'A' && peek <= 'Z') || (peek == '_')
                                || (peek >= 0 && peek <= 9))
                {
                    ReadNext();
                    AddBuff(currentChar);
                }
                else
                {
                    string upper = buffer.ToUpper();
                    if (Enum.IsDefined(typeof(KeyWords), upper))
                    {
                        return AddToken(row, lpos, Type.KEYWORD, 0, buffer);
                    }
                    else
                    {
                        return AddToken(row, lpos, Type.IDENTIFIER, 0, buffer);
                    }

                }
            }
        }

        private Token ScanNumber(bool hexFlag, int lpos)
        {
            if (currentChar == '$')
            {
                while (true)
                {
                    char peek = (char)_reader.Peek();
                    if ((peek >= 'a' && peek <= 'f') || (peek >= 'A' && peek <= 'F') || (peek >= '0' && peek <= '9'))
                    {
                        ReadNext();
                        AddBuff(currentChar);
                    } else
                    {
                        return AddToken(row, lpos, Type.INTEGER, Convert.ToInt32(buffer.Substring(1), 16), buffer);
                    }
                }
            } else if (currentChar == '%')
            {
                while (true)
                {
                    char peek = (char)_reader.Peek();
                    if (peek == '0' || peek == '1')
                    {
                        ReadNext();
                        AddBuff(currentChar);
                    }
                    else
                    {
                        return AddToken(row, lpos, Type.INTEGER, Convert.ToInt32(buffer.Substring(1), 2), buffer);
                    }
                }
            } else if (currentChar == '&')
            {
                while (true)
                {
                    char peek = (char)_reader.Peek();
                    if (peek >= '0' && peek <= '7')
                    {
                        ReadNext();
                        AddBuff(currentChar);
                    }
                    else
                    {
                        return AddToken(row, lpos, Type.INTEGER, Convert.ToInt32(buffer.Substring(1), 8), buffer);
                    }
                }

            } else
            {
                bool eflag = false;
                bool dflag = false;

                while (true)
                {
                    char peek = (char)_reader.Peek();
                    if ((peek >= '0' && peek <= '9') || (!dflag && peek == '.') || (!eflag && (peek == 'e' || peek == 'E')))
                    {
                        if (!dflag && !eflag)
                        {
                            if (peek == '.') dflag = true;
                            if (peek == 'e' || peek == 'E') eflag = true;
                        }

                        ReadNext();
                        AddBuff(currentChar);
                    } else
                    {
                        if (eflag || dflag)
                        {
                            return AddToken(row, lpos, Type.DOUBLE, double.Parse(buffer, System.Globalization.CultureInfo.InvariantCulture), buffer);
                        } else
                        {
                            return AddToken(row, lpos, Type.INTEGER, int.Parse(buffer), buffer);
                        }
                    }
                }

            }
        }

        private Token ScanString(int lpos)
        {
            while (true)
            {
                char peek = (char)_reader.Peek();
                if (peek == '\'')
                {
                    ReadNext();
                    AddBuff(currentChar);
                    return AddToken(row, lpos, Type.STRING, buffer, buffer);
                } else
                {
                    ReadNext();
                    AddBuff(currentChar);
                }

            }
        }

        private Token ScanSpecialSymbol(int lpos)
        {
            char peek = (char)_reader.Peek();
            if ((Array.FindIndex(specialSymbols, c => c == currentChar) <= 10) && (Array.Exists(specialSymbols, c => c == peek)) && (Array.FindIndex(specialSymbols, c => c == peek) <= 10))
            {
                if (Array.Exists(specialPairs, c => c == buffer + peek))
                {
                    if (buffer + peek == "//")
                    {
                        while (peek != '\n')
                        {
                            ReadNext();
                            AddBuff(currentChar);
                            peek = (char)_reader.Peek();
                        }
                        return AddToken(row, lpos, Type.COMMENT, buffer, buffer);
                    } else
                    {
                        ReadNext();
                        AddBuff(currentChar);
                        return AddToken(row, lpos, Type.SPECIALSYMBOLS, Array.FindIndex(specialPairs, c => c == buffer), buffer);
                    }
                   
                } else
                {
                    throw new Exception();
                }

            } else
            {
                return AddToken(row, lpos, Type.SPECIALSYMBOLS, Array.FindIndex(specialSymbols, c => c == currentChar), buffer);
            }
        }

    }
}
