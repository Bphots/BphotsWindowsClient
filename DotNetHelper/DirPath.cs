using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotNetHelper
{
    /// <summary>
    ///     The complete absolute Path to a Directory
    /// </summary>
    public class DirPath : StrongTypingNotNull<DirectoryInfo>
    {
        public DirPath(string dirPath)
            : base(CheckValid(dirPath)) { }

        /// <summary>
        ///     Returns true if the Directory exists.
        /// </summary>
        public bool Exists { get { return Value.Exists; } }

        public string Name { get { return Value.Name; } }

        /// <summary>
        ///     Returns the Parent Directory.
        /// </summary>
        public DirPath Parent
        {
            get
            {
                DirectoryInfo di = null;
                try
                {
                    di = Value.Parent;
                }
                catch { }
                return di == null ? null : new DirPath(di.FullName);
            }
        }

        /// <summary>
        ///     Create a new DirPath from a string representing a valid DirPath.
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static implicit operator DirPath(string dirPath)
        {
            return dirPath == null ? null : new DirPath(dirPath);
        }

        /// <summary>
        ///     Return a string representation of the DirPath.
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static implicit operator string(DirPath dirPath)
        {
            return dirPath == null ? null : dirPath.ToString();
        }

        /// <summary>
        ///     Tries to create a DirPath from a string.
        ///     Returns false if not succeeded.
        /// </summary>
        /// <param name="dirPath">The string representation of the valid Path.</param>
        /// <param name="outDirPath">The resulting FilePath or null if failed.</param>
        /// <returns>True if success, false if fail.</returns>
        public static bool TryCreateDirPath(string dirPath, out DirPath outDirPath)
        {
            outDirPath = null;

            if (string.IsNullOrEmpty(dirPath))
                return false;

            string trimmedFp = dirPath.RemoveNewLines().Trim();

            try
            {
                if (!Path.IsPathRooted(dirPath))
                    return false;

                outDirPath = new DirPath(trimmedFp);
            }
            catch (Exception)
            {
                outDirPath = null;
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return Value.FullName;
        }

        /// <summary>
        ///     Deletes this directory.
        /// </summary>
        /// <param name="recursively">If true, deletes all files in it as well.</param>
        public void Delete(bool recursively = true)
        {
            Directory.Delete(this, recursively);
        }

        /// <summary>
        ///     Deletes all files in the directory.
        /// </summary>
        /// <param name="recursive">If true it will delete all files inside sub-directories and the sub-directories themselves.</param>
        /// <param name="ignoreErrors">If true any files that can't be deleted will be ignored.</param>
        public void DeleteFiles(bool recursive = true, bool ignoreErrors = true)
        {
            foreach (FileInfo files in Value.GetFiles())
            {
                try
                {
                    files.Delete();
                }
                catch
                {
                    if (!ignoreErrors)
                        throw;
                }
            }

            if (!recursive)
                return;

            foreach (DirectoryInfo dirs in Value.GetDirectories())
            {
                try
                {
                    dirs.Delete(true);
                }
                catch
                {
                    if (!ignoreErrors)
                        throw;
                }
            }
        }

        /// <summary>
        ///     Returns the subdirectories of the current directory.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DirPath> GetDirs()
        {
            return Value.GetDirectories().Select(di => new DirPath(di.FullName));
        }

        /// <summary>
        ///     Returns a file list from the current directory.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FilePath> GetFiles()
        {
            return Value.GetFiles().Select(fi => new FilePath(fi.FullName));
        }

        /// <summary>
        ///     Returns a file list from the current directory.
        /// </summary>
        /// <param name="pattern">File pattern to search for.</param>
        /// <returns></returns>
        public IEnumerable<FilePath> GetFiles(string pattern)
        {
            return Value.GetFiles(pattern).Select(fi => new FilePath(fi.FullName));
        }

        private static DirectoryInfo CheckValid(string dirPath)
        {
            if (!Path.IsPathRooted(dirPath))
                throw new ArgumentException("DirPath " + dirPath + " is not an absolute path.");

            dirPath = dirPath.RemoveNewLines();
            if (dirPath.Last() != '\\')
                dirPath = dirPath + '\\';
            return new DirectoryInfo(dirPath);
        }
    }
}