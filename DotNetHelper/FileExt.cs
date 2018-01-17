using System;
using System.IO;

namespace DotNetHelper
{
    /// <summary>
    ///     A File Extension.
    /// </summary>
    public class FileExt : StrongTypingNotNull<string>
    {
        public FileExt(string fileExt)
            : base(fileExt.RemoveNewLines())
        {
            if (!IsValid())
                throw new ArgumentException("File extension " + fileExt + " is not a valid extension");
        }

        public FileExt(FileExt fileExt)
            : this(fileExt.ToString()) {}

        /// <summary>
        ///     Create a new FileExt from a string representing a valid FileExt.
        /// </summary>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        public static implicit operator FileExt(string fileExt)
        {
            return new FileExt(fileExt);
        }

        /// <summary>
        ///     Return a string representation of the FileExt.
        /// </summary>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        public static implicit operator string(FileExt fileExt)
        {
            return fileExt.ToString();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Value.Equals(((FileExt)obj).Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        /// <summary>
        ///     Returns a <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.String" /> that represents the current <see cref="T:System.Object" />.
        /// </returns>
        public override string ToString()
        {
            return Value;
        }

        private bool IsValid()
        {
            // File extensions start with a .
            string ext = Path.GetExtension(Value);
            return Value[0] == '.' && !String.IsNullOrEmpty(ext) && ext == Value;
        }
    }
}