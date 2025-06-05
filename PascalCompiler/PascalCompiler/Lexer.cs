using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace PascalCompiler
{
    public class Lexer
    {
        private enum State { Start, Identifier, Number, Operator, Comment, Assignment }

        private readonly string[] keywords = { "var", "begin", "end", "for", "to", "downto", "do", "integer", "shortint" };
        private readonly char[] singleCharOps = { '+', '-', '*', '/', ';', ',', '(', ')', '.' };

        public List<Token> Tokenize(string source, DataGridView dataGridView, TextBox txtLexErrors)
        {
            List<Token> tokens = new List<Token>();
            StringBuilder currentToken = new StringBuilder();
            State state = State.Start;
            int line = 1;
            int pos = 0;

            // Очищаем DataGridView перед новым анализом
            dataGridView.Rows.Clear();
            dataGridView.Columns.Clear();

            // Настраиваем колонки DataGridView
            dataGridView.Columns.Add("Type", "Тип токена");
            dataGridView.Columns.Add("Value", "Значение");
            dataGridView.Columns.Add("Code", "Код");
            dataGridView.Columns.Add("Line", "Строка");

            while (pos < source.Length)
            {
                char c = source[pos];

                switch (state)
                {
                    case State.Start:
                        if (char.IsWhiteSpace(c))
                        {
                            if (c == '\n') line++;
                            pos++;
                        }
                        // Сначала проверяем: “две косые черты” = начало комментария
                        else if (c == '/' && pos + 1 < source.Length && source[pos + 1] == '/')
                        {
                            // пропустить всё до конца строки
                            while (pos < source.Length && source[pos] != '\n')
                                pos++;
                        }
                        // Если одиночный символ из singleCharOps ('+', '-', '*', '/', ',', '(', ')', '.', ':')
                        else if (Array.IndexOf(singleCharOps, c) >= 0)
                        {
                            // Разовые операторы “/”, “*” и др.
                            var token = new Token(TokenType.Operator, c.ToString(), line);
                            tokens.Add(token);
                            dataGridView.Rows.Add(token.Type, token.Value, (int)token.GetTokenCode(), line);
                            pos++;
                        }
                        else if (char.IsLetter(c))
                        {
                            state = State.Identifier;
                            currentToken.Append(c);
                            pos++;
                        }
                        else if (char.IsDigit(c)
                              || (c == '-' && pos + 1 < source.Length && char.IsDigit(source[pos + 1])))
                        {
                            state = State.Number;
                            currentToken.Append(c);
                            pos++;
                        }
                        else if (c == ':')
                        {
                            state = State.Assignment;
                            currentToken.Append(c);
                            pos++;
                        }
                        else
                        {
                            txtLexErrors.AppendText($"Лексическая ошибка в строке {line}: Неожиданный символ '{c}'\r\n");
                            pos++;
                        }
                        break;


                    case State.Identifier:
                        if (char.IsLetterOrDigit(c))
                        {
                            currentToken.Append(c);
                            pos++;
                        }
                        else
                        {
                            string ident = currentToken.ToString().ToLower();
                            // Проверка для end. 
                            if (ident == "end" && pos < source.Length && source[pos] == '.')
                            {
                                var endToken = new Token(TokenType.Keyword, "end", line);
                                var dotToken = new Token(TokenType.Operator, ".", line);
                                tokens.Add(endToken);
                                tokens.Add(dotToken);
                                dataGridView.Rows.Add(endToken.Type, endToken.Value, (int)endToken.GetTokenCode(), line);
                                dataGridView.Rows.Add(dotToken.Type, dotToken.Value, (int)dotToken.GetTokenCode(), line);
                                pos++;

                                currentToken.Clear();
                                state = State.Start;
                                continue;
                            }

                            Token token;
                            if (Array.IndexOf(keywords, ident) >= 0)
                            {
                                token = new Token(TokenType.Keyword, ident, line);
                            }
                            else if (ident == "div" || ident == "mod")
                            {
                                token = new Token(TokenType.Operator, ident, line);
                            }
                            else
                            {
                                if (ident.Length > 20)
                                    ident = ident.Substring(0, 20);
                                token = new Token(TokenType.Identifier, ident, line);
                            }

                            tokens.Add(token);
                            dataGridView.Rows.Add(token.Type, token.Value, (int)token.GetTokenCode(), line);
                            currentToken.Clear();
                            state = State.Start;
                            continue;
                        }
                        break;

                    case State.Number:
                        if (char.IsDigit(c))
                        {
                            currentToken.Append(c);
                            pos++;
                        }
                        else
                        {
                            if (int.TryParse(currentToken.ToString(), out int num))
                            {
                                if (num < 0 || num > 65535)
                                {
                                    txtLexErrors.AppendText($"Лексическая ошибка в строке {line}: Константа {num} должна быть 1-байтным (0-255) или 2-байтным (0-65535).\r\n");
                                }

                                var token = new Token(TokenType.Number, currentToken.ToString(), line);
                                tokens.Add(token);
                                dataGridView.Rows.Add(token.Type, token.Value, (int)token.GetTokenCode(), line);
                            }
                            else
                            {
                                txtLexErrors.AppendText($"Лексическая ошибка в строке {line}: Неверный формат числа\r\n");
                            }
                            currentToken.Clear();
                            state = State.Start;
                        }
                        break;

                    case State.Assignment:
                        if (c == '=')
                        {
                            var token = new Token(TokenType.Operator, ":=", line);
                            tokens.Add(token);
                            dataGridView.Rows.Add(token.Type, token.Value, (int)token.GetTokenCode(), line);
                            pos++;
                        }
                        else
                        {
                            var token = new Token(TokenType.Operator, ":", line);
                            tokens.Add(token);
                            dataGridView.Rows.Add(token.Type, token.Value, (int)token.GetTokenCode(), line);
                        }
                        currentToken.Clear();
                        state = State.Start;
                        break;

                    case State.Comment:
                        if (c == '}')
                        {
                            state = State.Start;
                            pos++;
                        }
                        else
                        {
                            pos++;
                        }
                        break;
                }
            }

            // Добавляем последний токен, если он есть
            if (currentToken.Length > 0)
            {
                Token token;
                switch (state)
                {
                    case State.Identifier:
                        string ident = currentToken.ToString().ToLower();
                        if (Array.IndexOf(keywords, ident) >= 0)
                        {
                            token = new Token(TokenType.Keyword, ident, line);
                        }
                        else
                        {
                            token = new Token(TokenType.Identifier, ident, line);
                        }
                        tokens.Add(token);
                        dataGridView.Rows.Add(token.Type, token.Value, (int)token.GetTokenCode(), line);
                        break;
                    case State.Number:
                        token = new Token(TokenType.Number, currentToken.ToString(), line);
                        tokens.Add(token);
                        dataGridView.Rows.Add(token.Type, token.Value, (int)token.GetTokenCode(), line);
                        break;
                    default:
                        txtLexErrors.AppendText($"Лексическая ошибка: Неожиданный конец файла в состоянии {state}\r\n");
                        break;
                }
            }

            // Добавляем EOF
            var eofToken = new Token(TokenType.EOF, "EOF", line);
            tokens.Add(eofToken);
            dataGridView.Rows.Add(eofToken.Type, eofToken.Value, (int)eofToken.GetTokenCode(), line);

            if (txtLexErrors.TextLength == 0)
            {
                txtLexErrors.AppendText("Лексический анализ завершён успешно. Ошибок не обнаружено.\r\n");
            }


            return tokens;
        }
    }

    public enum TokenType { Keyword, Identifier, Number, Operator, Delimiter, EOF }

    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        public int Line { get; }

        public Token(TokenType type, string value, int line = 1)
        {
            Type = type;
            Value = value;
            Line = line;
        }

        public TokenCode GetTokenCode()
        {
            switch (Type)
            {
                case TokenType.Keyword:
                    return GetKeywordCode(Value.ToLower());
                case TokenType.Identifier:
                    return TokenCode.Identifier;
                case TokenType.Number:
                    return TokenCode.IntegerNumber;
                case TokenType.Operator:
                    return GetOperatorCode(Value);
                case TokenType.EOF:
                    return TokenCode.EOF;
                default:
                    return TokenCode.UnknownSymbol;
            }
        }

        private TokenCode GetKeywordCode(string keyword)
        {
            switch (keyword)
            {
                case "var": return TokenCode.KeywordVar;
                case "begin": return TokenCode.KeywordBegin;
                case "end": return TokenCode.KeywordEnd;
                case "integer": return TokenCode.KeywordInteger;
                case "shortint": return TokenCode.KeywordShortInt;
                case "for": return TokenCode.KeywordFor;
                case "to": return TokenCode.KeywordTo;
                case "downto": return TokenCode.KeywordDownTo;
                case "do": return TokenCode.KeywordDo;
                case "div": return TokenCode.OperatorDiv;
                case "mod": return TokenCode.OperatorMod;
                default: return TokenCode.UnknownSymbol;
            }
        }

        private TokenCode GetOperatorCode(string op)
        {
            switch (op)
            {
                case "+": return TokenCode.OperatorPlus;
                case "-": return TokenCode.OperatorMinus;
                case "*": return TokenCode.OperatorMultiply;
                case "/": return TokenCode.OperatorDivide;
                case ":=": return TokenCode.OperatorAssign;
                case ":": return TokenCode.OperatorColon;
                case ";": return TokenCode.DelimiterSemicolon;
                case ",": return TokenCode.DelimiterComma;
                case "(": return TokenCode.DelimiterOpenParenthesis;
                case ")": return TokenCode.DelimiterCloseParenthesis;
                case ".": return TokenCode.DelimiterDot;
                default: return TokenCode.UnknownSymbol;
            }
        }

        public override string ToString()
        {
            return $"{Type}: {Value} (Line: {Line})";
        }
    }

    public enum TokenCode
    {
        UnknownSymbol = 0,
        // Ключевые слова
        KeywordVar = 1,
        KeywordBegin = 2,
        KeywordEnd = 3,
        KeywordInteger = 4,
        KeywordShortInt = 5,
        KeywordFor = 6,
        KeywordTo = 7,
        KeywordDownTo = 8,
        KeywordDo = 9,
        // Идентификаторы
        Identifier = 10,
        // Числа
        IntegerNumber = 11,
        // Операторы
        OperatorPlus = 20,
        OperatorMinus = 21,
        OperatorMultiply = 22,
        OperatorDivide = 23,
        OperatorAssign = 24,
        OperatorColon = 25,
        OperatorDiv = 26,
        OperatorMod = 27,
        // Разделители
        DelimiterSemicolon = 30,
        DelimiterComma = 31,
        DelimiterOpenParenthesis = 32,
        DelimiterCloseParenthesis = 33,
        DelimiterDot = 34,
        // Специальные
        EOF = 99
    }
}