﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Calculate
{
    public class StringCalculator
    {
        protected IReadOnlyCollection<char> KeyOperators = new List<char>() { '*', '/', '+', '-', '(', ')' };

        protected virtual bool IsConvertingToDouble(string elemet)
        {
            if (double.TryParse(elemet, out _))
                return true;
            return false;
        }

        protected virtual List<string> Separation(string enterText)
        {
            var remainder = enterText.Replace(" ", "").Replace("\t", "").Replace("\n", "");
            var split_list = new List<string>();
            if (String.IsNullOrEmpty(remainder))
                throw new SeparationException("Нет данных для вычисления");
            if (IsConvertingToDouble(remainder))
            {
                split_list.Add(remainder);
                return split_list;
            }
            for (int i = 0; i < enterText.Length; i++)
            {
                if (KeyOperators.Contains(enterText[i]))
                {
                    split_list.Add(remainder.Split(enterText[i]).First());
                    remainder = enterText.Remove(0, i + 1);
                    split_list.Add(enterText[i].ToString());
                    if (remainder.Length != 0 && IsConvertingToDouble(remainder) && !KeyOperators.Contains(remainder.First()))
                        split_list.Add(remainder);
                }
            }
            return split_list;
        }

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

        protected virtual List<ElementInfo> List_Edit(List<string> split_list)
        {
            var elements = new List<ElementInfo>();
            for (int i = 0; i < split_list.Count; i++)
                elements.Add(new ElementInfo(split_list[i], TypeDefinition(split_list[i]), i));
            elements = elements.Where(x => x.type != Types.Trash).ToList();
            CleanList(ref elements);
            SetNewIndex(ref elements);
            return elements;
        }

        protected virtual void CleanList(ref List<ElementInfo> elements)
        {
            int CountList = elements.Count;
            for (int i = 0; i < CountList; i++)
            {
                ConcatenationSubtraction(ref elements);
                if (elements.First().type == Types.ArithmeticOperators)
                    elements.Remove(elements.First());
                else if (elements.Last().type == Types.ArithmeticOperators)
                    elements.Remove(elements.Last());
                else if (elements.First().type != Types.ArithmeticOperators && elements.Last().type != Types.ArithmeticOperators)
                    break;
            }
            for (int i = 0, Cbrace = 0; i < elements.Count; i++)
            {
                if (elements[i].type == Types.ArithmeticOperators && elements[i + 1].type == Types.ArithmeticOperators)
                    throw new KeyOperatorsException("Два арефметических оператора подряд");
                if (elements[i].content == "(")
                    Cbrace++;
                else if (elements[i].content == ")")
                    Cbrace--;
                if (i == elements.Count - 1 && Cbrace != 0)
                    throw new KeyOperatorsException("Скобка не закрывается");
            }
        }

        protected virtual double Finding_Solutions(List<string> split_list)
        {
            List<ElementInfo> elements = List_Edit(split_list);
            return BraceSearch(elements);
        }

        protected virtual double BraceSearch(List<ElementInfo> elements)
        {
            if (FindRangeBrace(elements, out IntPair intPair) == true)
            {
                List<ElementInfo> part = elements.GetRange(intPair.istart, intPair.count);
                SetNewIndex(ref part);
                elements[intPair.istart - 1] = new ElementInfo(BraceSearch(part).ToString(), Types.Number, intPair.istart - 1);
                elements.RemoveRange(intPair.istart, intPair.count + 1);
                SetNewIndex(ref elements);
                return BraceSearch(elements);
            }
            else
                return CourseOfAction(elements);
        }

        protected virtual bool FindRangeBrace(List<ElementInfo> elements, out IntPair intPair)
        {
            intPair = new IntPair();
            if (!elements.Any(x => x.type == Types.Brace))
                return false;
            for (int i = 0, brace = 0; i < elements.Count; i++)
            {
                if (elements[i].content == "(")
                {
                    if (brace == 0)
                        intPair.istart = i + 1;
                    brace++;
                }
                else if (elements[i].content == ")")
                {
                    brace--;
                    if (brace == 0)
                    {
                        intPair.count = i - intPair.istart;
                        break;
                    }
                }
            }
            return true;
        }

        protected virtual double CourseOfAction(List<ElementInfo> elements)
        {
            if (IsEmptyOrArifOper(elements))
                return 0;
            ConcatenationSubtraction(ref elements);
            int count = elements.Where(x => x.type == Types.ArithmeticOperators).ToList().Count;
            for (int i = 0; i < count; i++)
                CalcuationElement(ref elements);
            return Convert.ToDouble(elements.First().content);
        }

        protected virtual bool IsEmptyOrArifOper(List<ElementInfo> elements)
        {
            if (elements.Count == 0)
                return true;
            if (elements.Count == 1)
                if (elements.First().type == Types.ArithmeticOperators)
                    return true;
            return false;
        }

        protected virtual void ConcatenationSubtraction(ref List<ElementInfo> elements)
        {
            if (elements.First().content == "-" && elements[1].type == Types.Number)
            {
                elements[0] = new ElementInfo(elements[0].content + elements[1].content, Types.Number, 0);
                elements.RemoveAt(1);
            }
            else if (elements.First().type == Types.ArithmeticOperators && elements[1].type == Types.Number)
            {
                elements.RemoveAt(0);
            }
            if (elements.Last().type == Types.ArithmeticOperators)
            {
                elements.Remove(elements.Last());
            }
        }

        protected virtual void CalcuationElement(ref List<ElementInfo> elements)
        {
            int index = FindFirstAct(elements);
            elements[index] = new ElementInfo(Calcutating(Convert.ToDouble(elements[index - 1].content), Convert.ToDouble(elements[index + 1].content), elements[index].content).ToString(), Types.Number, index);
            elements.RemoveAt(index + 1);
            elements.RemoveAt(index - 1);
            SetNewIndex(ref elements);
        }

        protected virtual void SetNewIndex(ref List<ElementInfo> elements)
        {
            for (int i = 0; i < elements.Count; i++)
                elements[i] = new ElementInfo(elements[i].content, elements[i].type, i);
        }

        protected virtual int FindFirstAct(List<ElementInfo> elements)
        {
            if (elements.Any(x => x.content == "*") || elements.Any(x => x.content == "/"))
                return (elements.Where(x => x.content == "*").DefaultIfEmpty(new ElementInfo("", new Types(), int.MaxValue)).First().indexn < elements.Where(x => x.content == "/").DefaultIfEmpty(new ElementInfo("", new Types(), int.MaxValue)).First().indexn) ?
                        elements.FirstOrDefault(x => x.content == "*").indexn :
                        elements.FirstOrDefault(x => x.content == "/").indexn;
            if (elements.Any(x => x.content == "+") || elements.Any(x => x.content == "-"))
                return (elements.Where(x => x.content == "+").DefaultIfEmpty(new ElementInfo("", new Types(), int.MaxValue)).First().indexn < elements.Where(x => x.content == "-").DefaultIfEmpty(new ElementInfo("", new Types(), int.MaxValue)).First().indexn) ?
                        elements.FirstOrDefault(x => x.content == "+").indexn :
                        elements.FirstOrDefault(x => x.content == "-").indexn;
            throw new CalcutatingException("Невозможно найти первое действие");
        }

        protected virtual double Calcutating(double number1, double number2, string operation)
        {
            switch (operation)
            {
                case "+":
                    return number1 + number2;
                case "-":
                    return number1 - number2;
                case "/":
                    return number2 != 0 ? number1 / number2 : throw new CalcutatingException("Запрещенно делить на 0");
                case "*":
                    return number1 * number2;
                default:
                    throw new CalcutatingException("Неизвестная операция");
            }
        }

        public virtual double Converting(string enterText)
        {
            return Finding_Solutions(Separation(enterText));
        }

        protected struct IntPair
        {
            internal int istart;
            internal int count;

            internal IntPair(int istart, int count)
            {
                this.istart = istart;
                this.count = count;
            }
        }

        protected class ElementInfo
        {
            internal string content;
            internal Types type;
            internal int indexn;

            internal ElementInfo(string elem, Types typ)
            {
                content = elem;
                type = typ;
                indexn = -1;
            }

            internal ElementInfo(string elem, int num)
            {
                content = elem;
                type = new Types();
                indexn = num;
            }

            internal ElementInfo(string elem, Types typ, int num)
            {
                content = elem;
                type = typ;
                indexn = num;
            }
        }

        protected enum Types
        {
            ArithmeticOperators,
            Brace,
            Number,
            Trash
        }

        protected class SeparationException : Exception
        {
            public SeparationException(string message) : base(message)
            { }
        }

        protected class CalcutatingException : Exception
        {
            public CalcutatingException(string message) : base(message)
            { }
        }

        protected class KeyOperatorsException : Exception
        {
            public KeyOperatorsException(string message) : base(message)
            { }
        }
    }
}

