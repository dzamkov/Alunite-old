using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using OPath = System.IO.Path;

namespace Alunite
{
    /// <summary>
    /// A very small abstraction representing a path in the file system.
    /// </summary>
    public struct Path
    {
        public Path(string Path)
        {
            this._Path = Path;
        }

        /// <summary>
        /// Gets a sub-path with the specified name.
        /// </summary>
        public Path this[string Name]
        {
            get
            {
                return new Path(this._Path + System.IO.Path.DirectorySeparatorChar + Name);
            }
        }

        /// <summary>
        /// Gets the parent path for this path.
        /// </summary>
        public Path Parent
        {
            get
            {
                return new Path(Directory.GetParent(this._Path).FullName);
            }
        }

        /// <summary>
        /// Finds the absolute path for the specified relative path.
        /// </summary>
        public Path Lookup(string Relative)
        {
            Relative = Relative.Replace('/', OPath.DirectorySeparatorChar).Replace('\\', OPath.DirectorySeparatorChar);
            return new Path(OPath.GetFullPath(this._Path + OPath.DirectorySeparatorChar + Relative));
        }

        /// <summary>
        /// Gets if this path is currently a valid file.
        /// </summary>
        public bool ValidFile
        {
            get
            {
                return File.Exists(this._Path);
            }
        }

        /// <summary>
        /// Gets if this path is currently a valid directory.
        /// </summary>
        public bool ValidDirectory
        {
            get
            {
                return Directory.Exists(this._Path);
            }
        }

        /// <summary>
        /// Gets the string representation for the path.
        /// </summary>
        public string PathString
        {
            get
            {
                return this._Path;
            }
        }

        /// <summary>
        /// Gets the application startup path.
        /// </summary>
        public static Path ApplicationStartup
        {
            get
            {
                return new Path(System.IO.Path.GetFullPath(Application.StartupPath));
            }
        }

        /// <summary>
        /// Reads the entire text from the file located at the specified path.
        /// </summary>
        public static string ReadText(Path Path)
        {
            return File.ReadAllText(Path._Path);
        }

        public static implicit operator string(Path Path)
        {
            return Path._Path;
        }

        private string _Path;
    }
}