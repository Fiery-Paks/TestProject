# проект Калькулятор из строки


## Пояснение класса StringCalculator
Описание перечесления **Types** 
```C#
        protected enum Types
        {
            ArithmeticOperators,
            Brace,
            Number,
            Trash
        }
```
Описание метода для определения типа 
```C#
        protected virtual Types TypeDefinition(string element)
        {
            if (IsConvertingToDouble(element))
                return Types.Number;
            else if (KeyOperators.Any(x => element == x.ToString() && element != "(" && element != ")"))
                return Types.ArithmeticOperators;
            else if (element == "(" || element == ")")
                return Types.Brace;
            return Types.Trash;
        }

```
