using System;

namespace AutoStep.Projects.Files
{
    /// <summary>
    /// Represetns a single file entry in a file set.
    /// </summary>
    public struct FileSetEntry : IEquatable<FileSetEntry>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSetEntry"/> struct.
        /// </summary>
        /// <param name="absolute">The absolute path to the file.</param>
        /// <param name="relative">The relative path to the file.</param>
        public FileSetEntry(string absolute, string relative)
        {
            Absolute = absolute;
            Relative = relative;
        }

        /// <summary>
        /// Gets the absolute path to the file.
        /// </summary>
        public string Absolute { get; }

        /// <summary>
        /// Gets the relative path to the file.
        /// </summary>
        public string Relative { get; }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is FileSetEntry entry && Equals(entry);
        }

        /// <inheritdoc/>
        public bool Equals(FileSetEntry other)
        {
            return Absolute == other.Absolute &&
                   Relative == other.Relative;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(Absolute, Relative);
        }

        /// <summary>
        /// = operator for <see cref="FileSetEntry"/>.
        /// </summary>
        /// <param name="left">Left.</param>
        /// <param name="right">Right.</param>
        /// <returns>Equality.</returns>
        public static bool operator ==(FileSetEntry left, FileSetEntry right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// != operator for <see cref="FileSetEntry"/>.
        /// </summary>
        /// <param name="left">Left.</param>
        /// <param name="right">Right.</param>
        /// <returns>!Equality.</returns>
        public static bool operator !=(FileSetEntry left, FileSetEntry right)
        {
            return !(left == right);
        }
    }
}
