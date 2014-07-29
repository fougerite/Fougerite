using System.Diagnostics.Contracts;

namespace Fougerite
{
    using System;

    public class ChatString
    {
        public string NewText;
        public readonly string origText;

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(origText != null);
        }

        public ChatString(string str)
        {
            Contract.Requires(str != null);
            this.NewText = str;
            this.origText = str;
        }

        [Pure]
        public bool Contains(string str)
        {
            Contract.Requires(!string.IsNullOrEmpty(str));
            return this.origText.Contains(str);
        }

        [Pure]
        public static implicit operator string(ChatString cs)
        {
            Contract.Requires(cs != null);
            return cs.origText;
        }

        [Pure]
        public static implicit operator ChatString(string str)
        {
            Contract.Requires(str != null);
            return new ChatString(str);
        }

        [Pure]
        public string Replace(string find, string replacement)
        {
            Contract.Requires(!string.IsNullOrEmpty(find));
            Contract.Requires(replacement != null);
            return this.origText.Replace(find, replacement);
        }

        [Pure]
        public string Substring(int start, int length)
        {
            Contract.Requires(start >= 0);
            Contract.Requires(length >= 0);
            Contract.Requires(start < origText.Length);
            Contract.Requires(start + length <= origText.Length);
            return this.origText.Substring(start, length);
        }

        [Pure]
        public override string ToString()
        {
            return this.origText;
        }
    }
}