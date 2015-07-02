namespace Fougerite
{
    using System;

    public class ChatString
    {
        private string ntext;
        private string origText;

        public ChatString(string str)
        {
            this.ntext = this.origText = str;
        }

        public string OriginalMessage
        {
            get
            {
                return origText;
            }
        }

        public string NewText
        {
            get
            {
                return ntext;
            }
            set
            {
                // Rust doesn't like empty strings on chat
                if (string.IsNullOrEmpty(value))
                {
                    ntext = "          ";
                }
                else
                {
                    ntext = value;
                }
            }
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

        [Obsolete("Use OriginalMessage instead", false)]
        public override string ToString()
        {
            return this.origText;
        }
    }
}