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
        private bool eof = false;

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
                eof = true;
                currentChar = char.MinValue;
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
            ReadNext();
            GetNextToken();
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

        private void SkipWhiteSpace()
        {
            while (currentChar == ' ' || currentChar == '\t' || currentChar == '\0' || currentChar == '\r' || currentChar == '\n')
            {
                if (currentChar == '\n')
                {
                    ++row;
                    pos = 0;
                }
                ReadNext();
                if (eof)
                {
                    break;
                }
            }
        }

        public Token GetNextToken()
        {
            
            ClearBuff();
            if (currentChar == ' ' || currentChar == '\t' || currentChar == '\0' || currentChar == '\r' || currentChar == '\n')
            {
                SkipWhiteSpace();
            }
            if (eof)
            {
                return AddToken(row, pos, Type.EOF, 0, buffer);
            }
            if (currentChar is '+' or '-' or '$' or '%' or '&' || char.IsDigit(currentChar))
            {
                return ScanNumber(pos);
            }
            else if (currentChar is '\'')
            {
                return ScanString(pos);
            }
            else if ((currentChar >='a' && currentChar <='z') || (currentChar >= 'A' && currentChar <= 'Z') || currentChar == '_')
            {
                return ScanID(pos);
            }
            else
            {
                return ScanSymbol(pos);
            }
        }
        
        enum Symbols
        {
            LessThan, // <
            GreaterThan, // >
            Multiply, // *
            Colon, // :
            Plus, // +
            Minus, // -
            Divide, // /
            LeftParen, // (
            RightParen, // )
            Dot, // .
            Equals, // =
            SingleQuote, // '
            LeftBracket, // [
            RightBracket, // ]
            Comma, // ,
            Caret, // ^
            AtSign, // @
            LeftBrace, // {
            RightBrace, // }
            Dollar, // $
            Hash, // #
            Ampersand, // &
            Percent, // %
            Semicolon, // ;
            DoubleLessThan, // <<
            DoubleGreaterThan, // >>
            DoubleAsterisk, // **
            NotEqual, // <>
            Exchange, // ><
            LessThanOrEqual, // <=
            GreaterThanOrEqual, // >=
            Assign, // :=
            PlusAssign, // +=
            MinusAssign, // -=
            MultiplyAssign, // *=
            DivideAssign, // /=
            LeftParenStar, // (*
            RightParenStar, // *)
            LeftParenDot, // (.
            DotRightParen, // .)
            DoubleSlash // //
        }
        public Token ScanSymbol(int pos)
        {
            char first = currentChar;
            AddBuff(currentChar);
            ReadNext();
            switch (first)
            {
                
                case '/':
                    switch (currentChar)
                    {
                        case '/':
                            while (ReadNext())
                            {
                                if (currentChar == '\n')
                                {
                                    //++row;
                                    return AddToken(row, pos, Type.COMMENT, buffer, buffer);
                                }
                                AddBuff(currentChar);
                            }
                            return AddToken(row, pos, Type.COMMENT, buffer, buffer);
                        case '=':
                            AddBuff(currentChar);
                            ReadNext();
                            return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.DivideAssign, buffer);
                        default:
                            return AddToken(row, pos, Type.OPERATOR, Symbols.Divide, buffer);
                    }
                case '<':
                    switch (currentChar)
                    {
                        case '<':
                            AddBuff(currentChar);
                            ReadNext();
                            return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.DoubleLessThan, buffer);
                        case '=':
                            AddBuff(currentChar);
                            ReadNext();
                            return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.LessThanOrEqual, buffer);
                        case '>':
                            AddBuff(currentChar);
                            ReadNext();
                            return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.NotEqual, buffer);
                        default:
                            return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.LessThan, buffer);
                    }
                case '>':
                    switch (currentChar)
                    {
                        case '<':
                            AddBuff(currentChar);
                            ReadNext();
                            return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.Exchange, buffer);
                        case '=':
                            AddBuff(currentChar);
                            ReadNext();
                            return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.GreaterThanOrEqual, buffer);
                        case '>':
                            AddBuff(currentChar);
                            ReadNext();
                            return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.DoubleGreaterThan, buffer);
                        default:
                            return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.GreaterThan, buffer);
                    }
                case '*':
                    switch (currentChar)
                    {
                        case '*':
                            AddBuff(currentChar);
                            ReadNext();
                            return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.DoubleAsterisk, buffer);
                        case '=':
                            AddBuff(currentChar);
                            ReadNext();
                            return AddToken(row, pos, Type.OPERATOR, Symbols.MultiplyAssign, buffer);
                        case ')':
                            AddBuff(currentChar);
                            ReadNext();
                            return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.RightParenStar, buffer);
                        default:
                            return AddToken(row, pos, Type.OPERATOR, Symbols.Multiply, buffer);
                    }
                case ':':
                    switch (currentChar)
                    {
                        case '=':
                            AddBuff(currentChar);
                            ReadNext();
                            return AddToken(row, pos, Type.OPERATOR, Symbols.Assign, buffer);
                        default:
                            return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.Colon, buffer);
                    }
                case '+':
                    switch (currentChar)
                    {
                        case '=':
                            AddBuff(currentChar);
                            ReadNext();
                            return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.PlusAssign, buffer);
                        default:
                            return AddToken(row, pos, Type.OPERATOR, Symbols.Plus, buffer);
                    }
                case '-':
                    switch (currentChar)
                    {
                        case '=':
                            AddBuff(currentChar);
                            ReadNext();
                            return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.MinusAssign, buffer);
                        default:
                            return AddToken(row, pos, Type.OPERATOR, Symbols.Minus, buffer);
                    }
                case '(':
                    switch (currentChar)
                    {
                        case '*':
                            AddBuff(currentChar);
                            while (ReadNext())
                            {
                                AddBuff(currentChar);
                                if (currentChar == '\n')
                                {
                                    ++row;
                                }
                                else if (currentChar == '*')
                                {
                                    ReadNext();
                                    if (currentChar == ')')
                                    {
                                        AddBuff(currentChar);
                                        ReadNext();
                                        return AddToken(row, pos, Type.COMMENT, buffer, buffer);

                                    }
                                    else
                                    {
                                        AddBuff(currentChar);
                                    }
                                }

                            }
                            return AddToken(row, pos, Type.ERROR, "Comment not closed error", buffer);
                        case '.':
                            AddBuff(currentChar);
                            ReadNext();
                            return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.LeftParenDot, buffer);
                        default:
                            return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.LeftParen, buffer);
                    }
                case ')':
                    return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.RightParen, buffer);
                case '.':
                    switch (currentChar)
                    {
                        case ')':
                            AddBuff(currentChar);
                            ReadNext();
                            return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.DotRightParen, buffer);
                        default:
                            return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.Dot, buffer);
                    }
                case '=':
                    return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.Equals, buffer);
                case '\'':
                    return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.SingleQuote, buffer);
                case '[':
                    return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.LeftBracket, buffer);
                case ']':
                    return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.RightBracket, buffer);
                case ',':
                    return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.Comma, buffer);
                case '^':
                    return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.Caret, buffer);
                case '@':
                    return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.AtSign, buffer);
                case '{':
                    AddBuff(currentChar);
                    if (currentChar == '}')
                    {
                        return AddToken(row, pos, Type.COMMENT, buffer, buffer);
                    }
                    while (ReadNext())
                    {
                        AddBuff(currentChar);
                        if (currentChar == '\n')
                        {
                            ++row;
                        }
                        else if (currentChar == '}')
                        {
                            AddBuff(currentChar);
                            ReadNext();
                            return AddToken(row, pos, Type.COMMENT, buffer, buffer);
                        }

                    }
                    return AddToken(row, pos, Type.ERROR, "Comment not closed error", buffer);
                case '}':
                    return AddToken(row, pos, Type.ERROR, "Not opened comment error", buffer);
                case '$':
                    return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.Dollar, buffer);
                case '#':
                    return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.Hash, buffer);
                case '&':
                    return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.Ampersand, buffer);
                case '%':
                    return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.Percent, buffer);
                case ';':
                    return AddToken(row, pos, Type.SPECIALSYMBOLS, Symbols.Semicolon, buffer);
                default:
                    return AddToken(row, pos, Type.ERROR, "Unknown symbol error", buffer);

            }
        }

        enum NStates
        {
            START,
            SIGN,
            HEX,
            HEXSEQUENCE,
            OCTAL,
            OCTALSEQUENCE,
            BIN,
            BINSEQUENCE,
            DIGIT,
            SEPARATOR,
            AFTERSEPARATOR,
            EXPONENT,
            AFTEREXPONENT,
            AFTEREXPONENTSEQUENCE,
            EXPONENTSIGN
        }

        public Token ScanNumber(int pos)
        {
            NStates state = NStates.START;
            bool scanning = true;
            while (scanning)
            {
                switch (state)
                {
                    case NStates.START:
                        if (currentChar == '+' || currentChar == '-')
                        {
                            state = NStates.SIGN;
                        }
                        else if (currentChar == '$')
                        {
                            state = NStates.HEX;
                        }
                        else if (currentChar == '&')
                        {
                            state = NStates.OCTAL;
                        }
                        else if (currentChar == '%')
                        {
                            state = NStates.BIN;
                        }
                        else if (char.IsDigit(currentChar))
                        {
                            state = NStates.DIGIT;
                        }
                        else
                        {
                            scanning = false;
                            break;
                        }
                        AddBuff(currentChar);
                        break;

                    case NStates.SIGN:
                        if (currentChar == '$')
                        {
                            state = NStates.HEX;
                        }
                        else if (currentChar == '&')
                        {
                            state = NStates.OCTAL;
                        }
                        else if (currentChar == '%')
                        {
                            state = NStates.BIN;
                        }
                        else if (char.IsDigit(currentChar))
                        {
                            state = NStates.DIGIT;
                        }
                        else if (currentChar == '=')
                        {
                            if (buffer == "+=")
                            {
                                return AddToken(row, pos, Type.OPERATOR, Symbols.PlusAssign, buffer);
                            } 
                            else if (buffer == "-=")
                            {
                                return AddToken(row, pos, Type.OPERATOR, Symbols.MinusAssign, buffer);
                            }
                            else
                            {
                                return AddToken(row, pos, Type.ERROR, "unknown symbol", buffer);
                            }
                        }
                        else
                        {
                            scanning = false;
                            break;
                        }
                        AddBuff(currentChar);
                        break;

                    case NStates.HEX:
                        if ((currentChar >= 'a' && currentChar <= 'f') || (currentChar >= 'A' && currentChar <= 'F') || (currentChar >= '0' && currentChar <= '9'))
                        {
                            state = NStates.HEXSEQUENCE;
                        }
                        else
                        {
                            scanning = false;
                            break;
                        }
                        AddBuff(currentChar);
                        break;

                    case NStates.HEXSEQUENCE:
                        if ((currentChar >= 'a' && currentChar <= 'f') || (currentChar >= 'A' && currentChar <= 'F') || (currentChar >= '0' && currentChar <= '9'))
                        {
                            //States.HEXSEQUENCE
                        }
                        else
                        {
                            scanning = false;
                            break;
                        }
                        AddBuff(currentChar);
                        break;

                    case NStates.OCTAL:
                        if (currentChar >= '0' && currentChar <= '7')
                        {
                            state = NStates.OCTALSEQUENCE;
                        }
                        else
                        {
                            scanning = false;
                            break;
                        }
                        AddBuff(currentChar);
                        break;

                    case NStates.OCTALSEQUENCE:
                        if (currentChar >= '0' && currentChar <= '7')
                        {
                            state = NStates.OCTALSEQUENCE;
                        }
                        else
                        {
                            scanning = false;
                            break;
                        }
                        AddBuff(currentChar);
                        break;

                    case NStates.BIN:
                        if (currentChar == '0' || currentChar == '1')
                        {
                            state = NStates.BINSEQUENCE;
                        }
                        else
                        {
                            scanning = false;
                            break;
                        }
                        AddBuff(currentChar);
                        break;

                    case NStates.BINSEQUENCE:
                        if (currentChar == '0' || currentChar == '1')
                        {
                            //States.BINSEQUENCE
                        }
                        else
                        {
                            scanning = false;
                            break;
                        }
                        AddBuff(currentChar);
                        break;

                    case NStates.DIGIT:
                        if (char.IsDigit(currentChar))
                        {
                            //States.DIGIT
                        }
                        else if (currentChar == '.')
                        {
                            state = NStates.SEPARATOR;
                        }
                        else if (currentChar == 'e' || currentChar == 'E')
                        {
                            state = NStates.EXPONENT;
                        }
                        else
                        {
                            scanning = false;
                            break;
                        }
                        AddBuff(currentChar);
                        break;

                    case NStates.SEPARATOR:
                        if (char.IsDigit(currentChar))
                        {
                            state = NStates.AFTERSEPARATOR;
                        }
                        else
                        {
                            scanning = false;
                            break;
                        }
                        AddBuff(currentChar);
                        break;

                    case NStates.AFTERSEPARATOR:
                        if (char.IsDigit(currentChar))
                        {
                            //States.AFTERSEPARATOR
                        }
                        else if (currentChar == 'e' || currentChar == 'E')
                        {
                            state = NStates.EXPONENT;
                        }
                        else
                        {
                            scanning = false;
                            break;
                        }
                        AddBuff(currentChar);
                        break;

                    case NStates.EXPONENT:
                        if (char.IsDigit(currentChar))
                        {
                            state = NStates.AFTEREXPONENT;
                        }
                        else if (currentChar == '+' || currentChar == '-')
                        {
                            state = NStates.EXPONENTSIGN;
                        }
                        else
                        {
                            scanning = false;
                            break;
                        }
                        AddBuff(currentChar);
                        break;

                    case NStates.EXPONENTSIGN:
                        if (char.IsDigit(currentChar))
                        {
                            state = NStates.AFTEREXPONENT;
                        }
                        else
                        {
                            scanning = false;
                            break;
                        }
                        AddBuff(currentChar);
                        break;
                    case NStates.AFTEREXPONENT:
                        if (char.IsDigit(currentChar))
                        {
                            //States.AFTEREXPONENT
                        }
                        else
                        {
                            scanning = false;
                            break;
                        }
                        AddBuff(currentChar);
                        break;
                }
                if (!scanning)
                {
                    break;
                } else
                {
                    ReadNext();
                }
            }
            switch (state)
            {
                case NStates.AFTEREXPONENT:
                    return AddToken(row, pos, Type.DOUBLE, Double.Parse(buffer, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture), buffer);
                case NStates.AFTERSEPARATOR:
                    return AddToken(row, pos, Type.DOUBLE, Double.Parse(buffer, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture), buffer);
                case NStates.BINSEQUENCE:
                    return AddToken(row, pos, Type.INTEGER, Convert.ToInt32(buffer.Remove(0, 1), 2), buffer);
                case NStates.HEXSEQUENCE:
                    return AddToken(row, pos, Type.INTEGER, Convert.ToInt32(buffer.Remove(0, 1), 16), buffer);
                case NStates.OCTALSEQUENCE:
                    return AddToken(row, pos, Type.INTEGER, Convert.ToInt32(buffer.Remove(0, 1), 8), buffer);
                case NStates.DIGIT:
                    return AddToken(row, pos, Type.INTEGER, Convert.ToInt32(buffer), buffer);
                case NStates.SIGN:
                    if (buffer == "+")
                    {
                        return AddToken(row, pos, Type.OPERATOR, Symbols.Plus, buffer);
                    } 
                    else if (buffer == "-")
                    {
                        return AddToken(row, pos, Type.OPERATOR, Symbols.Minus, buffer);
                    } 
                    else
                    {
                        return AddToken(row, pos, Type.ERROR, "Bad number format", buffer);

                    }
                default:
                    return AddToken(row, pos, Type.ERROR, "Bad number format", buffer);
            }
        }

        enum SStates
        {
            START,
            SEQUENCE,
            QUOTE
        }

        public Token ScanString(int pos)
        {
            SStates state = SStates.START;
            bool scanning = true;
            while (scanning)
            {
                switch (state)
                {
                    case SStates.START:
                        if (currentChar == '\'')
                        {
                            state = SStates.SEQUENCE;
                        }
                        else
                        {
                            scanning = false;
                            break;
                        }
                        AddBuff(currentChar);
                        break;
                    case SStates.SEQUENCE:
                        if (currentChar == '\'')
                        {
                            state = SStates.QUOTE;
                        }
                        else
                        {
                            //States.SEQUENCE
                        }
                        AddBuff(currentChar);
                        break;
                    case SStates.QUOTE:
                        if (currentChar == '\'')
                        {
                            state = SStates.SEQUENCE;
                        }
                        else
                        {
                            scanning = false;
                            break;
                        }
                        AddBuff(currentChar);
                        break;
                }
                if (!scanning)
                {
                    break;
                }
                else
                {
                    ReadNext();
                }
            }

            switch (state)
            {
                case SStates.QUOTE:
                    return AddToken(row, pos, Type.STRING, buffer, buffer);
                default:
                    return AddToken(row, pos, Type.ERROR, "Bad string format", buffer);
            }
        }

        enum IStates
        {
            START,
            SEQUENCE
        }

        public Token ScanID(int pos)
        {
            IStates state = IStates.START;
            bool scanning = true;
            while (scanning)
            {
                switch (state)
                {
                    case IStates.START:
                        if ((currentChar >= 'a' && currentChar <= 'z') || (currentChar >= 'A' && currentChar <= 'Z') || currentChar == '_')
                        {
                            state = IStates.SEQUENCE;
                        }
                        else
                        {
                            scanning = false;
                            break;
                        }
                        AddBuff(currentChar);
                        break;
                    case IStates.SEQUENCE:
                        if (char.IsDigit(currentChar) || (currentChar >= 'a' && currentChar <= 'z') || (currentChar >= 'A' && currentChar <= 'Z') || currentChar == '_')
                        {
                            //States.SEQUENCE
                        }
                        else
                        {
                            scanning = false;
                            break;
                        }
                        AddBuff(currentChar);
                        break;
                }

                if (!scanning)
                {
                    break;
                } else
                {
                    ReadNext();
                }
            }

            switch (state)
            {
                case IStates.SEQUENCE:
                    if (Enum.IsDefined(typeof(KeyWords), buffer.ToUpper()))
                    {
                        return AddToken(row, pos, Type.KEYWORD, (KeyWords)System.Enum.Parse(typeof(KeyWords), buffer.ToUpper()), buffer);
                    } else
                    {
                        return AddToken(row, pos, Type.IDENTIFIER, buffer, buffer);
                    }
                default:
                    return AddToken(row, pos, Type.ERROR, "Bad ID format", buffer);
            }
        }
    }
}
