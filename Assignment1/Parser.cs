using System;
using System.Collections.Generic;
using System.Text;

namespace Assignment1
{
    class Parser
    {
/*        private Stack<char> stack = new Stack<char>();
        public int numLines = -1;
       
        public int Parse(int index, string line, char delimiter)
        {
            bool closing;
            if (delimiter != ',')
                closing = false;
            else
                closing = true;
            stack.Push(delimiter);
            do
            {
                if (line[++index] == delimiter)
                {
                    if (!closing)
                        stack.Push(delimiter);
                    else
                        stack.Pop();
                }
                else if (stack.Count > 0)
                {
                    closing = true;
                }
                else
                {
                    closing = false;
                }
                if (stack.Count == 0)
                {
                    closing = true;
                }
            } while (index < line.Length-1 && (!closing || stack.Count > 0));
            if (stack.Count > 0)
                currentIndex = index;
            return index;
        }*/

    }
}
