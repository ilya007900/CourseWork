using AppDomain.Utils;
using System.IO;

namespace AppDomain.Cameras
{
    public class SnapshotsStorage
    {
        private const string DirPath = "C:/CameraBaslerNet";

        public void Save(byte[] bytes)
        {
            var dirInfo = new DirectoryInfo(DirPath);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }

            var fileName = FileNameGenerator.GenerateUniqueName(dirInfo, "snapshot");
            var path = Path.Combine(DirPath, fileName) + ".bin";
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (var binaryWriter = new BinaryWriter(stream))
                {
                    binaryWriter.Write(bytes);
                }
            }
        }
    }
}