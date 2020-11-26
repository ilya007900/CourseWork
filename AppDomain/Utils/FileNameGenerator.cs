using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AppDomain.Utils
{
    public static class FileNameGenerator
    {
        public static string GenerateUniqueName(DirectoryInfo directoryInfo, string baseName)
        {
            var files = directoryInfo.GetFiles();
            var indexes = new List<int>();
            foreach (var fileInfo in files)
            {
                var leftBracketIndex = fileInfo.Name.LastIndexOf('(');
                var rightBracketIndex = fileInfo.Name.LastIndexOf(')');
                if (leftBracketIndex == -1 || rightBracketIndex == -1 || leftBracketIndex > rightBracketIndex)
                {
                    continue;
                }

                var indexBuilder = new StringBuilder();
                for (var i = leftBracketIndex + 1; i < rightBracketIndex; i++)
                {
                    if (char.IsDigit(fileInfo.Name[i]))
                    {
                        indexBuilder.Append(fileInfo.Name[i]);
                    }
                }

                var index = int.Parse(indexBuilder.ToString());
                indexes.Add(index);
            }

            if (!indexes.Any())
            {
                if (files.Any(x => Path.GetFileNameWithoutExtension(x.Name) == baseName))
                {
                    return $"{baseName}(1)";
                }

                return baseName;
            }

            var maxIndex = indexes.Max();
            return $"{baseName}({maxIndex + 1})";
        }
    }
}