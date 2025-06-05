using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace PascalCompiler
{
    class Stack<T>
    {
        T[] mas;
        int top;
        public Stack(int initialCapacity = 16)
        {
            if (initialCapacity <= 0) initialCapacity = 16;
            mas = new T[initialCapacity];
            top = -1;
        }
        public void Push(T a)
        {
            // Если нет места, расширяем массив вдвое
            if (top >= mas.Length - 1)
            {
                // Удваиваем размер
                int newSize = mas.Length * 2;
                if (newSize < mas.Length + 1)
                    newSize = mas.Length + 1; // страхуемся от переполнения целого

                T[] newMas = new T[newSize];
                Array.Copy(mas, newMas, mas.Length);
                mas = newMas;
            }

            mas[++top] = a;
        }
        public T Read()
        {
            return mas[top];
        }

        public T Pop()
        {
            if (top >= 0)
            {
                return mas[top--];
            }
            else
            {
                throw new InvalidOperationException("Ошибка: попытка извлечения из пустого стека.");
            }
        }
        public int Count
        {
            get { return top + 1; }
        }
        public bool isEmpty()
        {
            return top < 0;
        }
        public bool isFull()
        {
            return top >= mas.Length;
        }
    }

    public class ParserLRPascal
    {
        private List<Token> tokens;
        private TextBox errorOutput;

        private Stack<int> StackSost;      // Стек состояний
        private Stack<string> StackRazbor; // Стек разбора (символов)

        private int pos = 0;    // Позиция текущего токена
        private Token currToken;

        public bool HasError { get; private set; } = false;

        public class Quartet
        {
            public string Operator { get; set; }
            public string Operand1 { get; set; }
            public string Operand2 { get; set; }
            public string Result { get; set; }

            public Quartet(string op, string o1, string o2, string res)
            {
                Operator = op;
                Operand1 = o1;
                Operand2 = o2;
                Result = res;
            }
        }

        // Промежуточный код (четверки)
        private List<Quartet> intermediateCode = new List<Quartet>();
        public List<Quartet> GetIntermediateCode() => intermediateCode;

        // Счетчик временных переменных (T1, T2, ...)
        private int tempVarCounter = 0;

        // Словарь для хранения типов переменных
        private Dictionary<string, string> variableTypes = new Dictionary<string, string>();

        private ListBox listBox4; // Для вывода промежуточного кода
        public ParserLRPascal(List<Token> tokens, TextBox errorOutput, ListBox listBox4)
        {
            this.tokens = tokens;
            this.errorOutput = errorOutput;
            this.listBox4 = listBox4;
        }

        public void Parse()
        {
            StackSost = new Stack<int>();
            StackRazbor = new Stack<string>();
            StackSost.Push(0);           // Начальное состояние
            currToken = NextToken();


            while (true)
            {
                Debug.WriteLine($">> State={StackSost.Read()},  Token.Type={currToken.Type},  Token.Value=\"{currToken.Value}\"");

                if (HasError)
                    return;

                int state = StackSost.Read();

                switch (state)
                {
                    // Состояние 0: ожидание "var"
                    case 0:
                        if (currToken.Value == "var") { Sdvig(1, "var"); continue; }
                        Error("Ожидался ключевое слово 'var'"); return;

                    // Состояние 1: после "var" – ожидаем идентификатор
                    case 1:
                        if (currToken.Type == TokenType.Identifier) { GenerateCode($"НАЧАЛО_ПРОГРАММЫ"); Sdvig(2, "id"); continue; }
                        Error("Ожидался идентификатор после 'var'"); return;

                    // Состояние 2: <спис_перем> ::= id@
                    case 2:
                        if (currToken.Value == ",") { Sdvig(9, ","); continue; }
                        if (currToken.Value == ":") { Privedenie(1, "<спис_перем>"); continue; }
                        Error("Ожидался ',' или ':'"); return;

                    // Состояние 3: <спис_опис> ::= <опис>@
                    case 3:
                        Privedenie(1, "<спис_опис>"); continue;

                    // Состояние 4: <спис_перем> ::= <спис_перем> @ : <тип>  или  @','
                    case 4:
                        if (currToken.Value == ":") { Sdvig(5, ":"); continue; }
                        if (currToken.Value == ",") { Sdvig(9, ","); continue; }
                        Error("Ожидался ':' или ','"); return;

                    // Состояние 5: <опис> ::= <спис_перем> : @ <тип>
                    case 5:
                        if (currToken.Value == "integer") { Sdvig(6, "integer"); continue; }
                        if (currToken.Value == "shortint") { Sdvig(7, "shortint"); continue; }
                        Error("Ожидался 'integer' или 'shortint'"); return;

                    // Состояния 6 и 7: <тип> ::= integer@   и   <тип> ::= shortint@
                    case 6:
                    case 7:
                        Privedenie(1, "<тип>"); continue;

                    // Состояние 8: <опис> ::= <спис_перем> : <тип>@
                    case 8:
                        Privedenie(3, "<опис>"); continue;

                    // Состояние 9: <спис_перем> ::= <спис_перем> , @ id
                    case 9:
                        if (currToken.Type == TokenType.Identifier) { Sdvig(10, "id"); continue; }
                        Error("Ожидался идентификатор после ','"); return;

                    // Состояние 10: <спис_перем> ::= <спис_перем> , id@
                    case 10:
                        Privedenie(3, "<спис_перем>"); continue;

                    // Состояние 11: <прог> ::= var <спис_опис> @ ;
                    case 11:
                        if (currToken.Value == ";") { Sdvig(12, ";"); continue; }
                        Error("Ожидался ';'"); return;

                    // Состояние 12: <прог> ::= var <спис_опис> ; @ begin  или  <спис_опис> ::= <спис_опис> ; @ <опис>
                    case 12:
                        if (currToken.Value == "begin") { Sdvig(13, "begin"); continue; }
                        if (currToken.Type == TokenType.Identifier) { Sdvig(2, "id"); continue; }
                        Error("Ожидался 'begin' или объявление переменной '" + currToken.Value + "'");
                        return;

                    // Состояние 13: <прог> ::= var <спис_опис> ; begin @ <спис_опер>
                    case 13:
                        if (currToken.Type == TokenType.Identifier) { Sdvig(17, "id"); continue; }
                        if (currToken.Value == "for") { Sdvig(39, "for"); continue; }
                        if (currToken.Value == "begin") { Sdvig(49, "begin"); continue; }
                        if (currToken.Value == "end") { Sdvig(47, "end"); continue; }
                        //if (currToken.Value == ";") { Privedenie(1, "<спис_опер>"); continue; }
                        Error("Ожидался оператор (id, for, begin) или 'end'"); return;

                    // Состояние 14: <спис_опер> ::= <опер>@
                    case 14:
                        if (currToken.Value == ";") { Privedenie(1, "<спис_опер>"); continue; }
                        Error("Ожидался ';' после оператора"); return;

                    // Состояние 15: <опер> ::= <сложн_ар>@
                    case 15:
                        if (currToken.Value == ";") { Privedenie(1, "<опер>"); continue; }
                        Error("Ожидался ';' после сложного оператора"); return;

                    // Состояние 16: <опер> ::= <цикл_for>@
                    case 16:
                        if (currToken.Value == ";") { Privedenie(1, "<опер>"); continue; }
                        Error("Ожидался ';' после цикла for"); return;

                    // Состояние 17: <сложн_ар> ::= id @ := <выраж>
                    case 17:
                        if (currToken.Value == ":=") { Sdvig(18, ":="); continue; }
                        Error($"Ожидалось ':=' после идентификатора, получено: {currToken.Value}"); return;

                    // Состояние 18: <сложн_ар> ::= id := @ <выраж>
                    case 18: // <сложн_ар> ::= id := @ <выраж>
                             // 1) Если видим "(", уходим в логику первичных выражений:
                        if (currToken.Value == "(")
                        {
                            // Переходим в state 34, чтобы внутри скобок разобрать <выраж>
                            Sdvig(34, "(");
                            continue;
                        }
                        // 2) Если видим идентификатор или число — сначала захватываем это как <первичное_выражение>,
                        //    т.е. идём в state 22, который сразу делает Privedenie(1, "<элемент>").
                        if (currToken.Type == TokenType.Identifier
                         || currToken.Type == TokenType.Number)
                        {
                            Sdvig(22, "<элем>");
                            continue;
                        }
                        // 3) Если видим окончание «сложн_ар» без выражения (например, точка с запятой или to/downto/do),
                        //    редуцируем текущее <сложн_ар>, как раньше:
                        if (currToken.Value == ";"
                         || currToken.Value == "to"
                         || currToken.Value == "downto"
                         || currToken.Value == "do")
                        {
                            Privedenie(3, "<сложн_ар>");
                            continue;
                        }
                        Error("Ожидался элемент выражения");
                        return;


                    // Состояние 19: внутри <выраж> (после первоначального <выраж>)
                    case 19:
                        if (currToken.Value == "+"){Sdvig(20, "+");continue;}
                        if (currToken.Value == "-"){Sdvig(21, "-");continue; }
                        if (currToken.Value == "*"|| currToken.Value == "/"|| currToken.Value == "div"|| currToken.Value == "mod"){Privedenie(1, "<элем>");continue;}
                        if (currToken.Value == "("){Privedenie(1, "<элем>");continue;}
                        if (currToken.Value == ";"|| currToken.Value == "to"|| currToken.Value == "downto"|| currToken.Value == "do"){Privedenie(3, "<сложн_ар>");continue;}
                        Error("Ожидался '+', '-', '*', '/', 'div', 'mod', '(', ';', 'to', 'downto' или 'do'");
                        return;

                    // Состояние 20: <выраж> ::= <выраж> + @ <элем>
                    case 20:
                        if (currToken.Type == TokenType.Identifier || currToken.Type == TokenType.Number || currToken.Value == "(")
                        { Sdvig(23, "<элем>"); continue; }
                        Error("Ожидался элемент выражения после '+'"); return;

                    // Состояние 21: <выраж> ::= <выраж> - @ <элем>
                    case 21:
                        if (currToken.Type == TokenType.Identifier || currToken.Type == TokenType.Number || currToken.Value == "(")
                        { Sdvig(24, "<элем>"); continue; }
                        Error("Ожидался элемент выражения после '-'"); return;

                    // Состояние 22: <элем> ::= <первичное_выражение>@
                    case 22:
                        Privedenie(1, "<элем>"); continue;

                    // Состояние 23: <выраж> ::= <выраж> + <элем>@
                    case 23:
                        if (currToken.Value == "*") { Sdvig(26, "*"); continue; }
                        if (currToken.Value == "/") { Sdvig(26, "/"); continue; }
                        if (currToken.Value == "div") { Sdvig(27, "div"); continue; }
                        if (currToken.Value == "mod") { Sdvig(28, "mod"); continue; }
                        Privedenie(3, "<выраж>"); continue;

                    // Состояние 24: <выраж> ::= <выраж> - <элем>@
                    case 24:
                        if (currToken.Value == "*") { Sdvig(26, "*"); continue; }
                        if (currToken.Value == "/") { Sdvig(26, "/"); continue; }
                        if (currToken.Value == "div") { Sdvig(27, "div"); continue; }
                        if (currToken.Value == "mod") { Sdvig(28, "mod"); continue; }
                        Privedenie(3, "<выраж>"); continue;

                    // Состояние 25: <выраж> ::= <элем>@
                    case 25:
                        if (currToken.Value == "*") { Sdvig(26, "*"); continue; }
                        if (currToken.Value == "/") { Sdvig(26, "/"); continue; }
                        if (currToken.Value == "div") { Sdvig(27, "div"); continue; }
                        if (currToken.Value == "mod") { Sdvig(28, "mod"); continue; }
                        Privedenie(1, "<выраж>");
                        continue;

                    // Состояние 26: <элем> ::= <элем> * @ <первичное_выражение>
                    case 26:
                        if (currToken.Type == TokenType.Identifier
                         || currToken.Type == TokenType.Number
                         || currToken.Value == "(")
                        {
                            Sdvig(29, "<первичное_выражение>");
                            continue;
                        }
                        Error("Ожидался первичный элемент после '*' или '/'"); return;


                    // Состояние 27: <элем> ::= <элем> div @ <первичное_выражение>
                    case 27:
                        if (currToken.Type == TokenType.Identifier
                         || currToken.Type == TokenType.Number
                         || currToken.Value == "(")
                        {
                            Sdvig(30, "<первичное_выражение>"); continue;
                        }
                        Error("Ожидался первичный элемент после 'div'"); return;

                    // Состояние 28: <элем> ::= <элем> mod @ <первичное_выражение>
                    case 28:
                        if (currToken.Type == TokenType.Identifier
                         || currToken.Type == TokenType.Number
                         || currToken.Value == "(")
                        {
                            Sdvig(31, "<первичное_выражение>"); continue;
                        }
                        Error("Ожидался первичный элемент после 'mod'"); return;

                    // Состояния 29, 30, 31: <первичное_выражение> ::= id@  или  Lit@  или  (@…)
                    case 29:
                    case 30:
                    case 31:
                        Privedenie(3, "<элем>"); continue;

                    // Состояния 32, 33: <первичное_выражение> ::= id@  и  Lit@
                    case 32:
                    case 33:
                        Privedenie(1, "<первичное_выражение>"); continue;

                    // Состояние 34: <первичное_выражение> ::= (@ <выраж> )
                    case 34: // <первичное_выражение> ::= ( @ <выраж> )
                             // 1) Если «(» – вложенная скобка, углубляемся
                        if (currToken.Value == "(")
                        {
                            Sdvig(34, "("); // ещё одна открывающая скобка
                            continue;
                        }
                        // 2) Если идентификатор или число – начинаем разбор <выраж>
                        if (currToken.Type == TokenType.Identifier
                         || currToken.Type == TokenType.Number)
                        {
                            Sdvig(35, "<выраж>");
                            continue;
                        }
                        Error("Ожидался идентификатор, число или '(' внутри скобок");
                        return;

                    // Состояние 35: <первичное_выражение> ::= ( <выраж> @ ) 
                    // Состояние 35: <первичное_выражение> ::= ( <выраж> @ )
                    // После того как из <выраж> внутри скобок уже «пробросили» всё, сюда может прийти не только ')',
                    // но и '+', '-', '*', '/', 'div', 'mod', '(', идентификатор или число (начало вложенного выражения).
                    case 35:
                        // 1) Если внутри скобок опять «(» — входим в state 34 рекурсивно:
                        if (currToken.Value == "(")
                        {
                            Sdvig(34, "(");
                            continue;
                        }
                        // 2) Если видим операторы '+' или '-' — переходим в state 20/21, чтобы разобрать сложное выражение:
                        if (currToken.Value == "+")
                        {
                            Sdvig(20, "+");
                            continue;
                        }
                        if (currToken.Value == "-")
                        {
                            Sdvig(21, "-");
                            continue;
                        }
                        // 3) Если видим операторы более высокого приоритета '*' или '/' — в state 26:
                        if (currToken.Value == "*")
                        {
                            Sdvig(26, "*");
                            continue;
                        }
                        if (currToken.Value == "/")
                        {
                            Sdvig(26, "/");
                            continue;
                        }
                        // 4) Если 'div' или 'mod' — в state 27/28 соответственно:
                        if (currToken.Value == "div")
                        {
                            Sdvig(27, "div");
                            continue;
                        }
                        if (currToken.Value == "mod")
                        {
                            Sdvig(28, "mod");
                            continue;
                        }
                        // 5) Если далее идёт идентификатор или число — начинаем новое <первичное_выражение> (state 32/33):
                        if (currToken.Type == TokenType.Identifier)
                        {
                            Sdvig(32, currToken.Value); // 32 — это state, где <первичное_выражение> ::= id@
                            continue;
                        }
                        if (currToken.Type == TokenType.Number)
                        {
                            Sdvig(33, currToken.Value); // 33 — это state, где <первичное_выражение> ::= Lit@
                            continue;
                        }
                        // 6) Наконец, если токен — ')' — закрываем скобку:
                        if (currToken.Value == ")")
                        {
                            Sdvig(36, ")");
                            continue;
                        }
                        // 7) Во всех остальных случаях — ошибка:
                        Error("Ожидался ')', '+', '-', '*', '/', 'div', 'mod', '(', идентификатор или число внутри скобок");
                        return;

                    // Состояние 36: <первичное_выражение> ::= ( <выраж> )@
                    case 36:
                        Privedenie(3, "<первичное_выражение>");
                        continue;

                    // Состояние 37: <спис_опер> ::= <спис_опер> @ ; <опер>
                    case 37:
                        if (currToken.Value == ";") { Sdvig(38, ";"); continue; }
                        Error("Ожидался ';' после списка операций"); return;

                    // Состояние 38: <прог> ::= … <спис_опер> @ ; end.   или  <спис_опер> ::= <спис_опер> ; @ <опер>
                    case 38:
                        if (currToken.Value == "end") { Sdvig(47, "end"); continue; }
                        if (currToken.Type == TokenType.Identifier) { Sdvig(17, "id"); continue; }
                        if (currToken.Value == "for") { Sdvig(39, "for"); continue; }
                        if (currToken.Value == "begin") { Sdvig(49, "begin"); continue; }
                        Error("Ожидался 'end' или оператор"); return;

                    // Состояние 39: <цикл_for> ::= for @ <сложн_ар> <шаг> <выраж> do <опер>
                    case 39:
                        if (currToken.Type == TokenType.Identifier) { Sdvig(17, "id"); continue; }
                        Error("Ожидался идентификатор после 'for'"); return;

                    // Состояние 40: <цикл_for> ::= for <сложн_ар> @ <шаг> <выраж> do <опер>
                    case 40:
                        if (currToken.Value == "downto") { Sdvig(41, "downto"); continue; }
                        if (currToken.Value == "to") { Sdvig(42, "to"); continue; }
                        Error("Ожидался 'to' или 'downto'"); return;

                    // Состояния 41 и 42: <шаг> ::= downto@    и    <шаг> ::= to@
                    case 41:
                    case 42:
                        Privedenie(1, "<шаг>"); continue;

                    case 43:
                        if (currToken.Type == TokenType.Identifier || currToken.Type == TokenType.Number || currToken.Value == "(")
                        { Sdvig(44, "<выраж>"); continue; }
                        Error("Ожидался элемент выражения"); return;

                    // Состояние 44: <цикл_for> ::= for <сложн_ар> <шаг> <выраж> @ do <опер>
                    case 44: 
                        if (currToken.Value == "do")
                        {
                            Sdvig(45, "do");
                            continue;
                        }
                        // Допустимые операторы внутри выражения (если граница — сложное выражение)
                        else if (currToken.Value == "+") { Sdvig(20, "+"); continue; }
                        else if (currToken.Value == "-") { Sdvig(21, "-"); continue; }
                        // Явная ошибка, если не do и не часть выражения
                        else
                        {
                            Error($"Ожидалось 'do' после границ цикла (получено: {currToken.Value})");
                            return; // Прерываем анализ
                        }


                    // Состояние 45: <цикл_for> ::= for <…> do @ <опер>
                    case 45:
                        if (currToken.Value == ";") { Sdvig(50, ";"); continue; }
                        if (currToken.Type == TokenType.Identifier) { Sdvig(17, "id"); continue; }
                        if (currToken.Value == "for") { Sdvig(39, "for"); continue; }
                        if (currToken.Value == "begin") { Sdvig(49, "begin"); continue; }
                        if (currToken.Value == "end") { Sdvig(52, "end"); continue; }
                        Error("Ожидался ';' или оператор");
                        return;

                    // Состояние 46: <цикл_for> ::= for <…> do <опер>@
                    case 46:
                        Privedenie(6, "<цикл_for>"); continue;

                    // Состояние 47: <прог> ::= var <спис_опис> ; begin <спис_опер> ; end @ .
                    case 47:
                        if (currToken.Value == ".")
                        {
                            GenerateCode("КОНЕЦ ПРОГРАММЫ");
                            Success();
                            return;
                        }
                        Error("Ожидался '.' в конце программы");
                        return;

                    // Состояние 48: <спис_опер> ::= <спис_опер> ; <опер>@
                    case 48:
                        Privedenie(3, "<спис_опер>"); continue;

                    // Состояние 49: <блок> ::= begin @ <спис_опер> ; end  
                    case 49:
                        if (currToken.Type == TokenType.Identifier) { Sdvig(17, "id"); continue; }
                        if (currToken.Value == "for") { Sdvig(39, "for"); continue; }
                        if (currToken.Value == "begin") { Sdvig(49, "begin"); continue; }
                        if (currToken.Value == ";") { Sdvig(50, ";"); continue; }
                        if (currToken.Value == "end") { Sdvig(52, "end"); continue; }
                        Error($"Ожидался оператор (id, for, begin, ;, end), получено: {currToken.Value}"); return;

                    // Состояние 50: <спис_опер> ::= <спис_опер> @ ; <опер>
                    case 50:
                        if (currToken.Value == ";") { Sdvig(51, ";"); continue; }
                        if (currToken.Value == "end") { Sdvig(52, "end"); continue; }
                        Error("Ожидался ';' или 'end'"); return;

                    // Состояние 51: <блок> ::= begin <спис_опер> ; @ end  
                    case 51:
                        if (currToken.Value == "end") { Sdvig(52, "end"); continue; }
                        if (currToken.Type == TokenType.Identifier) { Sdvig(17, "id"); continue; }
                        if (currToken.Value == "for") { Sdvig(39, "for"); continue; }
                        if (currToken.Value == "begin") { Sdvig(49, "begin"); continue; }
                        if (currToken.Value == ";") { Sdvig(50, ";"); continue; }
                        Error("Ожидался 'end' или оператор");
                        return;

                    // Состояние 52: <блок> ::= begin <спис_опер> ; end@ 
                    case 52:  
                        Privedenie(4, "<блок>");   
                        if (currToken.Value == ".")
                        {
                            GenerateCode("КОНЕЦ ПРОГРАММЫ");
                            Success();
                            return;
                        }
                        continue;


                    // Состояние 53: <опер> ::= <блок>@ 
                    case 53:
                        Privedenie(1, "<опер>"); continue;

                    default:
                        Error($"Неизвестное состояние {state}"); return;
                }
            }
        }

        private Stack<string> SemanticStack = new Stack<string>();
        private void Sdvig(int newState, string symbol)
        {
            StackSost.Push(newState);
            StackRazbor.Push(symbol);

            // Если это идентификатор, число или литерал — кладем в SemanticStack
            if (currToken.Type == TokenType.Identifier || currToken.Type == TokenType.Number)
            {
                SemanticStack.Push(currToken.Value);
            }
            else if (currToken.Value == "integer" || currToken.Value == "shortint")
            {
                SemanticStack.Push(currToken.Value);
            }
            else if (currToken.Value == "to" || currToken.Value == "downto")
            {
                // Теперь слово "to" попадёт в SemanticStack
                SemanticStack.Push(currToken.Value);
            }
            else if (symbol == ":=" || symbol == "+" || symbol == "-" || symbol == "*" ||
                     symbol == "div" || symbol == "mod" || symbol == "(" || symbol == ")")
            {
                SemanticStack.Push(symbol);
            }
            else
            {
                // Для грамматических нетерминалов пустую строку
                SemanticStack.Push("");
            }
            currToken = NextToken();
        }

        private string NewTempVar()
        {
            string tempName = $"T{++tempVarCounter}";
            GenerateCode($"СОЗДАНИЕ ВРЕМЕННОЙ ПЕРЕМЕННОЙ: {tempName}");
            return tempName;
        }

        private void GenerateCode(string line)
        {
            if (listBox4 != null)
            {
                // Добавляем отступы для вложенных блоков
                string indentedLine = new string(' ', currentIndent * 4) + line;
                listBox4.Items.Add(indentedLine);
            }
        }
        private int currentIndent = 0; // Текущий уровень вложенности

        private void AddQuadruple(string oper, string op1, string op2, string res)
{
        var quad = new Quartet(oper, op1, op2, res);
        intermediateCode.Add(quad);

        string line = "";
        string detailedLine = "";

            switch (oper)
            {
                case ":=":
                    if (string.IsNullOrEmpty(op2))
                    {
                        line = $"ПРИСВОИТЬ ЗНАЧЕНИЕ {res} = {op1}";
                        detailedLine = $"ПЕРЕМЕННОЙ '{res}' ПРИСВОЕНО ЗНАЧЕНИЕ '{op1}'";
                    }
                    else
                    {
                        line = $"ПРИСВОИТЬ РЕЗУЛЬТАТ {op1} {op2} В {res}";
                        detailedLine = $"РЕЗУЛЬТАТ ОПЕРАЦИИ {op1} {op2} ЗАПИСАН В '{res}'";
                    }
                    break;
            
                case "+":
                    line = $"СЛОЖИТЬ: {op1} + {op2} → {res}";
                    detailedLine = $"АРИФМЕТИЧЕСКАЯ ОПЕРАЦИЯ: {op1} + {op2} = {res}";
                    break;
            
                case "-":
                    line = $"ВЫЧЕСТЬ: {op1} - {op2} → {res}";
                    detailedLine = $"АРИФМЕТИЧЕСКАЯ ОПЕРАЦИЯ: {op1} - {op2} = {res}";
                    break;
            
                case "*":
                    line = $"УМНОЖИТЬ: {op1} * {op2} → {res}";
                    detailedLine = $"АРИФМЕТИЧЕСКАЯ ОПЕРАЦИЯ: {op1} * {op2} = {res}";
                    break;
                
                case "/":
                    line = $"ДЕЛЕНИЕ: {op1} / {op2} → {res}";
                    detailedLine = $"АРИФМЕТИЧЕСКАЯ ОПЕРАЦИЯ: {op1} / {op2} = {res}";
                    break;

                case "div":
                    line = $"ДЕЛЕНИЕ: {op1} / {op2} → {res}";
                    detailedLine = $"АРИФМЕТИЧЕСКАЯ ОПЕРАЦИЯ: {op1} / {op2} = {res}";
                    break;
            
                case "mod":
                    line = $"ОСТАТОК: {op1} % {op2} → {res}";
                    detailedLine = $"АРИФМЕТИЧЕСКАЯ ОПЕРАЦИЯ: {op1} % {op2} = {res}";
                    break;
            
                case "to":
                case "downto":
                    line = $"ЦИКЛ: {op1} {oper} {op2}";
                    detailedLine = $"ОПРЕДЕЛЕНИЕ ГРАНИЦ ЦИКЛА: от {op1} до {op2}";
                    break;
            
                default:
                    line = $"ОПЕРАЦИЯ: {oper} {op1} {op2} → {res}";
                    detailedLine = $"ВЫПОЛНЕНА ОПЕРАЦИЯ: {oper} над {op1} и {op2}";
                    break;
            }

            GenerateCode(line);
            GenerateCode(detailedLine);
            GenerateCode($"ЧЕТВЕРКА: ({oper}, {op1}, {op2}, {res})");
        }

        private Stack<string> loopStarts = new Stack<string>();
        private Stack<string> loopEnds = new Stack<string>();
        void Privedenie(int n, string res)
        {
            var removedSymbols = new List<string>();
            var removedSemantic = new List<string>();
            for (int i = 0; i < n; i++)
            {
                removedSymbols.Insert(0, StackRazbor.Pop());
                removedSemantic.Insert(0, SemanticStack.Pop());
                StackSost.Pop();
            }

            StackRazbor.Push(res);
            SemanticStack.Push(""); // По умолчанию

            switch (res)
            {
                // <спис_перем> ::= id
                case "<спис_перем>":
                // если n == 1, значит правило вида: <спис_перем> -> id
                // тогда removedSemantic[0] содержит именно имя переменной
                if (n == 1)
                {
                    string varName2 = removedSemantic[0];
                    SemanticStack.Pop();
                    SemanticStack.Push(varName2);
                }
                // если n == 3, значит правило: <спис_перем> -> <спис_перем> , id
                // тогда removedSemantic[0] = семантика предыдущего <спис_перем> ("a,b"),
                // а removedSemantic[2] = новый id ("c")
                else if (n == 3)
                {
                    string prevList = removedSemantic[0]; // например "a,b"
                    string newId = removedSemantic[2];    // например "c"
                                                            // Собираем через запятую (без пробелов)
                    string combined = prevList + "," + newId;
                    SemanticStack.Pop();
                    SemanticStack.Push(combined);
                }
                break;

                // <тип> ::= integer  или  <тип> ::= shortint
                case "<тип>":
                    // правило всегда n == 1
                    {
                        string typeName = removedSemantic[0]; // либо "integer", либо "shortint"
                        SemanticStack.Pop();
                        SemanticStack.Push(typeName);
                    }
                    break;

                case "<опис>":
                    {
                        string varList = removedSemantic[0];
                        string type = removedSemantic[2];
                        string[] vars = varList.Split(',');
                        foreach (string declaredVar in vars)
                        {
                            string v = declaredVar.Trim();
                            if (v.Length == 0) continue;

                            variableTypes[declaredVar] = type;
                            GenerateCode($"ОБЪЯВИТЬ ПЕРЕМЕННУЮ '{declaredVar}' ТИПА {type.ToUpper()}");
                        }
                        SemanticStack.Pop();
                        SemanticStack.Push("");
                    }
                    break;

                case "<прог>":
                    GenerateCode("НАЧАЛО ПРОГРАММЫ");
                    currentIndent++;
                    break;

                case "<сложн_ар>":
                    {
                        // 1) «leftVar» — это имя переменной (id), exprVal — то, что справа от := 
                        string leftVar = removedSemantic[0]; // например "a"
                        string exprVal = removedSemantic[2]; // например "5" или "T1"

                        // 2) Генерируем четырёхкратку присваивания:
                        AddQuadruple(":=", exprVal, "", leftVar);

                        // 3) Вместо того чтобы запихивать в SemanticStack только leftVar,
                        //    склеиваем «имя,значение» через запятую:
                        //    например, "a,5" или "a,T1" (если справа было сложное выражение).
                        string combined = leftVar + "," + exprVal;

                        // 4) Убираем старое пустое значение и кладём «имя,значение»:
                        SemanticStack.Pop();         // очищаем ту ячейку, куда ранее дописали "" при Sdvig
                        SemanticStack.Push(combined);
                    }
                    break;


                case "<выраж>":
                    // 1) Если из лексера/парсера пришло ровно один элемент,
                    //    значит «просто прокидываем» <элемент> → <выраж>:
                    if (removedSemantic.Count == 1)
                    {
                        string onlyOperand = removedSemantic[0];
                        SemanticStack.Push(onlyOperand);
                    }
                    // 2) Если три элемента – это <выраж> op <элемент>:
                    else if (removedSemantic.Count == 3)
                    {
                        string leftOperand = removedSemantic[0];
                        string opSymbol = removedSemantic[1];
                        string rightOperand = removedSemantic[2];
                        // Генерируем код и создаём новую временную переменную:
                        string tempVar = NewTempVar();
                        GenerateCode($"ВЫЧИСЛИТЬ: {leftOperand} {opSymbol} {rightOperand}");
                        AddQuadruple(opSymbol, leftOperand, rightOperand, tempVar);

                        // Кладём в семантический стек результат:
                        SemanticStack.Push(tempVar);
                    }
                    else
                    {
                        // На всякий случай: если придёт неожиданно другой размер списка,
                        // его можно либо бросить как ошибку, либо попробовать «прокинуть» 
                        // последний элемент (или причинно-следственную логику).
                        throw new InvalidOperationException(
                            $"Неподдерживаемая длина removedSemantic ({removedSemantic.Count}) в <выраж>");
                    }
                    break;


                case "<элем>":
                    // Точно так же: <элемент> ::= <элемент> op <первичное>, 
                    // или просто проброс «<первичное>» → <элемент>
                    if (removedSemantic.Count == 1)
                    {
                        // просто пробросили <первичное_выражение>
                        SemanticStack.Push(removedSemantic[0]);
                    }
                    else if (removedSemantic.Count == 3)
                    {
                        string leftOperand = removedSemantic[0];
                        string opSymbol = removedSemantic[1]; // "*", "/", "div" или "mod"
                        string rightOperand = removedSemantic[2];
                        string tempVar = NewTempVar();
                        GenerateCode($"ВЫЧИСЛИТЬ: {leftOperand} {opSymbol} {rightOperand}");
                        AddQuadruple(opSymbol, leftOperand, rightOperand, tempVar);
                        SemanticStack.Push(tempVar);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Неподдерживаемая длина removedSemantic ({removedSemantic.Count}) в <элем>");
                    }
                    break;

                // <первичное_выражение> ::= id | Lit | ( <выраж> )
                case "<первичное_выражение>":
                    // 1) Если просто идентификатор или число – прокидываем:
                    if (removedSemantic.Count == 1 && removedSymbols.Count == 1)
                    {
                        // removedSemantic[0] – это либо имя переменной, либо literal
                        SemanticStack.Push(removedSemantic[0]);
                    }
                    // 2) Если правило ( <выраж> ) – проверяем removedSymbols:
                    else if (removedSymbols.Count == 3
                          && removedSymbols[0] == "("
                          && removedSymbols[2] == ")"
                          && removedSemantic.Count == 3)
                    {
                        // removedSemantic[1] – это то, что получилось внутри скобок 
                        string inner = removedSemantic[1];
                        SemanticStack.Push(inner);
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Неподдерживаемая длина removedSemantic ({removedSemantic.Count}) в <первичное_выражение>");
                    }
                    break;

                case "<цикл_for>":
                    {
                        string loopVar = removedSemantic[0];
                        string startVal = removedSemantic[2];
                        string stepType = removedSemantic[3]; // "to" или "downto"
                        string endVal = removedSemantic[4];

                        // 1) Собираем метки и кладём в стеки
                        string loopStartLabel = $"МЕТКА_ЦИКЛА_{loopVar}_НАЧАЛО";
                        string loopEndLabel = $"МЕТКА_ЦИКЛА_{loopVar}_КОНЕЦ";

                        loopStarts.Push(loopStartLabel);
                        loopEnds.Push(loopEndLabel);

                        // 2) Генерируем начало цикла
                        GenerateCode($"НАЧАЛО ЦИКЛА FOR: {loopVar} := {startVal} {stepType} {endVal}");
                        GenerateCode($"{loopStartLabel}:");
                        currentIndent++;

                        // 3) Генерируем проверку условия
                        GenerateCode($"ПРОВЕРКА УСЛОВИЯ: {loopVar} {stepType} {endVal}");
                        AddQuadruple(stepType, loopVar, endVal, loopEndLabel);

                        // После этого парсер перейдёт к разбору тела (<блок> или <опер>).
                    }
                    break;

                case "<блок>":
                    // 1) Снижаем уровень вложенности (отступ)
                    if (currentIndent > 0)
                        currentIndent--;
                    GenerateCode("КОНЕЦ БЛОКА");
                    break;

                default:
                    break;
            }

            // Переход в следующее состояние
            int prevState = StackSost.Read();
            if (!transitions.TryGetValue((prevState, res), out int nextState))
            {
                Error($"Неизвестный переход после приведения: состояние {prevState}, символ {res}");
                return;
            }
            if (nextState >= 0)
                StackSost.Push(nextState);
            else if (nextState == -7)
            {
                currentIndent--;
                GenerateCode("КОНЕЦ ПРОГРАММЫ");
                Success();
            }
            else
                Privedenie(-nextState, res);
        }

        private readonly Dictionary<(int, string), int> transitions = new Dictionary<(int, string), int>
        {
            {(0, "var"), 1},
            {(1, "<спис_опис>"), 11}, 
            {(1, "<опис>"), 3}, 
            {(1, "<спис_перем>"), 4}, 
            {(1, "id"), 2},
            {(2, "any"), -1},
            {(2, ","), 9}, 
            {(2, ":"), -1},
            {(3, "any"), -1},
            {(4, ":"), 5}, 
            {(4, ","), 9},
            {(5, "integer"), 6}, 
            {(5, "shortint"), 7}, 
            {(5, "<тип>"), 8},
            {(6, "any"), -1}, 
            {(7, "any"), -1},
            {(8, "any"), -3}, 
            {(9, "id"), 10},
            {(10, "any"), -3},
            {(11, ";"), 12}, 
            {(12, "begin"), 13},
            {(12, "id"), 2},              
            {(12, "<опис>"), 37},
            {(12, "<спис_перем>"), 4},
            {(13, "<спис_опер>"), 37}, 
            {(13, "<опер>"), 14}, 
            {(13, "<сложн_ар>"), 15}, 
            {(13, "<цикл_for>"), 16},
            {(13, "<блок>"), 53}, 
            {(13, "id"), 17}, 
            {(13, "for"), 39}, 
            {(13, "begin"), 49},
            {(14, "any"), -1}, 
            {(15, "any"), -1}, 
            {(16, "any"), -1},
            {(17, ":="), 18},
            {(18, "<выраж>"), 19}, 
            {(18, "id"), 32}, 
            {(18, "Lit"), 33}, 
            {(18, "("), 34}, 
            {(18, "<элем>"), 25}, 
            {(18, "<первичное_выражение>"), 22},
            {(19, "+"), 20}, 
            {(19, "-"), 21},
            {(19, "any"), -3},
            {(20, "<элем>"), 23}, 
            {(20, "id"), 32}, 
            {(20, "Lit"), 33}, 
            {(20, "("), 34}, 
            {(20, "<первичное_выражение>"), 22},
            {(21, "<элем>"), 24}, 
            {(21, "id"), 32}, 
            {(21, "Lit"), 33}, 
            {(21, "("), 34}, 
            {(21, "<первичное_выражение>"), 22},
            {(22, "any"), -1}, 
            {(23, "*"), 26}, 
            {(23, "div"), 27}, 
            {(23, "mod"), 28}, 
            {(23, "any"), -3},
            {(24, "*"), 26}, 
            {(24, "div"), 27}, 
            {(24, "mod"), 28}, 
            {(24, "any"), -3},
            {(25, "*"), 26}, 
            {(25, "div"), 27}, 
            {(25, "mod"), 28}, 
            {(25, "any"), -1},
            {(26, "id"), 32}, 
            {(26, "Lit"), 33}, 
            {(26, "("), 34}, 
            {(26, "<первичное_выражение>"), 29},
            {(27, "id"), 32}, 
            {(27, "Lit"), 33}, 
            {(27, "("), 34}, 
            {(27, "<первичное_выражение>"), 30},
            {(28, "id"), 32}, 
            {(28, "Lit"), 33}, 
            {(28, "("), 34}, 
            {(28, "<первичное_выражение>"), 31},
            {(29, "any"), -3}, 
            {(30, "any"), -3}, 
            {(31, "any"), -3},
            {(32, "any"), -1}, 
            {(33, "any"), -1},
            {(34, "<выраж>"), 35}, 
            {(34, "<элем>"), 25},
            {(34, "<первичное_выражение>"), 22},
            {(34, "id"), 32},
            {(34, "Lit"), 33},
            {(34, "("), 34}, //
            {(35, ")"), 36}, 
            {(35, "+"), 20}, 
            {(35, "-"), 21},
            {(36, "any"), -3},
            {(37, ";"), 38},
            {(38, "end"), 47}, 
            {(38, "id"), 17}, 
            {(38, "for"), 39}, 
            {(38, "begin"), 49},
            {(38, "<опер>"), 48}, 
            {(38, "<сложн_ар>"), 15}, 
            {(38, "<цикл_for>"), 16}, 
            {(38, "<блок>"), 53},
            {(39, "id"), 17}, 
            {(39, "<сложн_ар>"), 40},
            {(40, "to"), 42}, 
            {(40, "downto"), 41}, 
            {(40, "<шаг>"), 43},
            {(41, "any"), -1}, 
            {(42, "any"), -1}, 
            {(43, "<выраж>"), 44}, 
            {(43, "id"), 32}, 
            {(43, "Lit"), 33}, 
            {(43, "("), 34}, 
            {(43, "<элем>"), 25}, 
            {(43, "<первичное_выражение>"), 22},
            {(44, "do"), 45}, 
            {(44, "+"), 20}, 
            {(44, "-"), 21},
            {(45, "id"), 17}, 
            {(45, "for"), 39}, 
            {(45, "begin"), 49}, 
            {(45, "<опер>"), 46}, 
            {(45, "<сложн_ар>"), 15}, 
            {(45, "<цикл_for>"), 16}, 
            {(45, "<блок>"), 53},
            {(46, "any"), -6},
            {(47, "."), -7},
            {(48, "any"), -3},
            {(49, "<спис_опер>"), 50}, 
            {(49, "<опер>"), 14}, 
            {(49, "<сложн_ар>"), 15},
            {(49, "<цикл_for>"), 16}, 
            {(49, "<блок>"), 53}, 
            {(49, "id"), 17}, 
            {(49, "for"), 39}, 
            {(49, "begin"), 49},
            {(50, ";"), 51},
            {(51, "end"), 52}, 
            {(51, "<опер>"), 48}, 
            {(51, "<сложн_ар>"), 15}, 
            {(51, "<цикл_for>"), 16}, 
            {(51, "<блок>"), 53}, 
            {(51, "id"), 17}, 
            {(51, "for"), 39}, 
            {(51, "begin"), 49},
            {(52, "any"), -4},
            {(53, "any"), -1}
        };

        private void Error(string message)
        {
            errorOutput.AppendText($"Синтаксическая ошибка: {message} (токен: {currToken})\r\n");
            HasError = true;
        }

        public bool HasSuccess { get; private set; } = false;

        private void Success()
        {
            errorOutput.AppendText("Программа корректна. Синтаксический анализ успешно завершён.\r\n");
            HasSuccess = true;
        }

        private Token NextToken()
        {
            if (pos < tokens.Count)
                return tokens[pos++];
            else
                return new Token(TokenType.EOF, "EOF");
        }
        public class SemanticAnalyzer
        {
            private List<Token> tokens;
            private TextBox txtSemanticErrors;
            private HashSet<string> declaredVariables = new HashSet<string>();
            private HashSet<string> assignedVariables = new HashSet<string>();
            private bool hasSemanticErrors = false;

            public SemanticAnalyzer(List<Token> tokens, TextBox txtSemanticErrors)
            {
                this.tokens = tokens;
                this.txtSemanticErrors = txtSemanticErrors;
            }

            public void Analyze()
            {
                declaredVariables.Clear();
                assignedVariables.Clear();
                txtSemanticErrors.Clear();

                bool inVarSection = false;

                for (int i = 0; i < tokens.Count; i++)
                {
                    var token = tokens[i];

                    // Начало секции var
                    if (token.Type == TokenType.Keyword && token.Value == "var")
                    {
                        inVarSection = true;
                    }
                    else if (token.Type == TokenType.Keyword && token.Value == "begin")
                    {
                        inVarSection = false;
                    }
                    else if (inVarSection && token.Type == TokenType.Identifier)
                    {
                        // Проверка повторного объявления
                        if (declaredVariables.Contains(token.Value))
                        {
                            AddSemanticError(40, token.Line, token.Value);
                        }
                        else
                        {
                            declaredVariables.Add(token.Value);
                        }
                    }
                    else if (!inVarSection && token.Type == TokenType.Identifier)
                    {
                        // Использование необъявленной переменной
                        if (!declaredVariables.Contains(token.Value))
                        {
                            AddSemanticError(41, token.Line, token.Value);
                        }
                        // Если это присваивание (смотрим следующий токен)
                        if (i + 1 < tokens.Count && tokens[i + 1].Value == ":=")
                        {
                            assignedVariables.Add(token.Value);
                        }
                        // Если используется, но нигде не присвоено
                        if (!assignedVariables.Contains(token.Value))
                        {
                            AddSemanticError(42, token.Line, token.Value);
                        }
                    }
                }

                if (!hasSemanticErrors)
                {
                    AddSemanticError(-1, 0); // Сообщение об отсутствии ошибок
                }
            }

            private void AddSemanticError(int kod, int index, string varName = "")
            {
                if (kod != -1)
                {
                    hasSemanticErrors = true;
                }
                string errorMessage = $"Семантическая ошибка #{kod} на строке {index}: ";
                switch (kod)
                {
                    case 40: errorMessage += $"Повторное объявление переменной '{varName}'"; break;
                    case 41: errorMessage += $"Использование необъявленной переменной '{varName}'"; break;
                    case 42: errorMessage += $"Использование переменной '{varName}', которой не присвоено значение"; break;
                    case -1: errorMessage = "Семантических ошибок нет."; break;
                    default: errorMessage += "Неизвестная семантическая ошибка."; break;
                }
                txtSemanticErrors.AppendText(errorMessage + "\r\n");
            }
        }
    }
}