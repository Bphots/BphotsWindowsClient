using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using DotNetHelper.Properties;

namespace DotNetHelper
{
    /// <summary>
    ///     A complete absolute Path to a File
    /// </summary>
    public class FilePath : StrongTypingNotNull<FileInfo>, IXmlSerializable
    {
        public FilePath(string filePath)
            : base(CheckValid(filePath))
        {}

        [UsedImplicitly]
        private FilePath()
            : base(CheckValid("C:"))
        {
            /* used only for serialisation/deserialisation */
        }

        /// <summary>
        ///     The Path of the original file that was copied into the temp folder.
        ///     Null if there is no such thing for this file.
        /// </summary>
        public FilePath OriginalFilePath { get; private set; }

        /// <summary>
        ///     Create a new FilePath from a string representing a valid FilePath.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static implicit operator FilePath(string filePath)
        {
            return filePath == null ? null : new FilePath(filePath);
        }

        /// <summary>
        ///     Return a string representation of the FilePath.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static implicit operator string(FilePath filePath)
        {
            return filePath == null ? null : filePath.ToString();
        }

        /// <summary>
        ///     Return the FileInfo object inside of this object.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static implicit operator FileInfo(FilePath filePath)
        {
            return filePath == null ? null : filePath.Value;
        }

        /// <summary>
        ///     Tries to create a FilePath from a string.
        ///     Returns false if not succeeded.
        /// </summary>
        /// <param name="filePath">The string representation of the valid Path.</param>
        /// <param name="outFilePath">The resulting FilePath or null if failed.</param>
        /// <returns>True if success, false if fail.</returns>
        public static bool TryCreate(string filePath, out FilePath outFilePath)
        {
            outFilePath = null;

            if (string.IsNullOrEmpty(filePath))
                return false;

            string trimmedFp = filePath.RemoveNewLines().Trim().Trim('"');

            try
            {
                if (!Path.IsPathRooted(trimmedFp))
                    return false;

                outFilePath = new FilePath(trimmedFp);
            }
            catch (Exception)
            {
                outFilePath = null;
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Copy this file to another FilePath.
        /// </summary>
        /// <param name="dstFilePath"></param>
        /// <param name="overwrite"></param>
        /// <param name="makeWritable"></param>
        public void CopyTo(FilePath dstFilePath, bool overwrite = false, bool makeWritable = false)
        {
            OriginalFilePath = new FilePath(this);
            Value.CopyTo(dstFilePath, overwrite);
            if (dstFilePath.Value.IsReadOnly && makeWritable)
                dstFilePath.Value.IsReadOnly = false;

            dstFilePath.Value.Refresh();
        }
        
        /// <summary>
        ///     Delete this file if it exists only.
        /// </summary>
        /// <returns></returns>
        public bool DeleteIfExists()
        {
            try
            {
                if (!Exists())
                    return false;

                Value.IsReadOnly = false;
                Value.Delete();
            }
            catch (Exception e)
            {
                return false;
            }

            Value.Refresh();
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Value.Equals(((FilePath)obj).Value);
        }

        /// <summary>
        ///     Returns true if the file exists.
        /// </summary>
        /// <returns></returns>
        public bool Exists()
        {
            Value.Refresh();
            return Value.Exists;
        }

        public DirPath GetDirPath()
        {
            return new DirPath(Value.DirectoryName);
        }

        /// <summary>
        ///     Returns the FilePath's extension.
        /// </summary>
        /// <returns></returns>
        public FileExt GetFileExt()
        {
            return new FileExt(Value.Extension);
        }

        /// <summary>
        ///     Returns the file name of the specified path string with the extension.
        /// </summary>
        /// <returns>The string returned by GetFileName.</returns>
        public string GetFileName()
        {
            return Path.GetFileName(this);
        }

        /// <summary>
        ///     Returns the file name of the specified path string without the extension.
        /// </summary>
        /// <returns>The string returned by GetFileName, minus the last period (.) and all characters following it.</returns>
        public string GetFileNameWithoutExtension()
        {
            return Path.GetFileNameWithoutExtension(this);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        ///     Returns true if the file exists and you have permission to write to it.
        /// </summary>
        /// <returns></returns>
        public bool IsWritable()
        {
            Value.Refresh();
            return Exists() && !Value.IsReadOnly;
        }

        /// <summary>
        ///     The size, in bytes, of the current file.
        /// </summary>
        /// <returns></returns>
        public long Length()
        {
            Value.Refresh();
            return Exists() ? Value.Length : 0;
        }

        /// <summary>
        ///     Reads the current file as a string.
        ///     Returns null if current file does not exist.
        /// </summary>
        /// <returns></returns>
        [CanBeNull]
        public string ReadAsString()
        {
            return !Exists() ? null : File.ReadAllText(ToString());
        }

        /// <summary>
        ///     Read this file into a byte array.
        ///     Returns null if current file does not exist.
        /// </summary>
        /// <returns></returns>
        [CanBeNull]
        public byte[] ReadBytes()
        {
            return !Exists() ? null : File.ReadAllBytes(ToString());
        }
        
        /// <summary>
        ///     Reads a stream into this file.
        /// </summary>
        /// <param name="stream"></param>
        public void ReadFromStream([NotNull] Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            stream.Position = 0;
            using (FileStream fileStream = Value.OpenWrite())
                stream.CopyTo(fileStream);
        }

        public void ReadXml(XmlReader reader)
        {
            try
            {
                Value = CheckValid(reader.ReadString());
                reader.ReadEndElement();
            }
            catch (Exception)
            {
                Value = null;
            }
        }

        /// <summary>
        ///     Returns the full path to the FilePath as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Value.FullName;
        }

        /// <summary>
        ///     Write to this file using a byte array.
        /// </summary>
        /// <param name="bytesToWrite"></param>
        public void WriteBytes(byte[] bytesToWrite)
        {
            File.WriteAllBytes(ToString(), bytesToWrite);
            Value.Refresh();
        }

        /// <summary>
        ///     Write a string to this file.
        /// </summary>
        /// <param name="stringToWriteFrom">String to write to the file.</param>
        /// <param name="shouldOverwrite">Should the current contents be overwritten?</param>
        public void WriteString(string stringToWriteFrom, bool shouldOverwrite = true)
        {
            if (shouldOverwrite)
                File.WriteAllText(ToString(), stringToWriteFrom);
            else
                File.AppendAllText(ToString(), stringToWriteFrom);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteValue(ToString());
        }

        private static FileInfo CheckValid(string filePath)
        {
            string trimmedFp = filePath.RemoveNewLines().Trim();
            if (string.IsNullOrEmpty(trimmedFp))
                throw new ArgumentException("Empty file path.");
            if (!Path.IsPathRooted(trimmedFp))
                throw new ArgumentException("FilePath '" + filePath + "' is not an absolute path.");

            return new FileInfo(trimmedFp);
        }
        
    }
}