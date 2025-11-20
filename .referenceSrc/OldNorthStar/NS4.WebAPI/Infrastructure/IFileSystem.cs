using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NorthStar4.Infrastructure
{
    public interface IFileSystem
    {
        Task<bool> ExistsAsync(string filePath);

        Task<Stream> OpenWriteAsync(string filePath);

        Task DeleteAsync(string filePath);

        Task<Stream> OpenReadAsync(string filePath);

        Task<IEnumerable<string>> ListDirectoryAsync(string directory);

        Task DeleteDirectoryAsync(string directory);
    }
}
