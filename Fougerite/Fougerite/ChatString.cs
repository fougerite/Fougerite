namespace Fougerite
{
    using System;

    public class ChatString
    {
        public string NewText;
        private string origText;

        public ChatString(string str)
        {
            this.NewText = this.origText = str;
        }

        public bool Contains(string str)
        {
            return this.origText.Contains(str);
        }

        public static implicit operator string(ChatString cs)
        {
            return cs.origText;
        }

        public static implicit operator ChatString(string str)
        {
            return new ChatString(str);
        }

        public string Replace(string find, string replacement)
        {
            return this.origText.Replace(find, replacement);
        }

        public string Substring(int start, int length)
        {
            return this.origText.Substring(start, length);
        }

        public override string ToString()
        {
            return this.origText;
        }
    }
}