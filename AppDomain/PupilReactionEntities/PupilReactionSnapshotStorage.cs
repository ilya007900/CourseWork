using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AppDomain.Utils;

namespace AppDomain.PupilReactionEntities
{
    public class PupilReactionSnapshotStorage
    {
        private const string WorkingDirectory = "C://CameraBaslerNET";

        private readonly List<PupilReactionSnapshot> snapshots = new List<PupilReactionSnapshot>();

        public void Add(PupilReactionSnapshot snapshot)
        {
            snapshots.Add(snapshot);
        }

        public void Save(byte startingBrightLevel, ushort currentBright)
        {
            var dirInfo = new DirectoryInfo(WorkingDirectory);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }

            var fileName = FileNameGenerator.GenerateUniqueName(dirInfo, "data");
            var imgFileName = Path.Combine(WorkingDirectory, $"{fileName}.bin");
            using (var stream = File.OpenWrite(imgFileName))
            {
                foreach (var savedImage in snapshots)
                {
                    stream.Write(savedImage.Image, 0, savedImage.Image.Length);
                }
            }

            var snapshotsData = snapshots.Select(x => new
            {
                x.DateTime,
                x.ExposureTime,
                x.Gain,
                x.PixelFormat,
                x.PWM
            }).ToArray();

            var jObject = new
            {
                StartBrightLevel = startingBrightLevel,
                LastBrightLevel = currentBright,
                Snapshots = snapshotsData,
            };

            var json = JsonConvert.SerializeObject(jObject);
            var jsonFileName = Path.Combine(WorkingDirectory, $"{fileName}.json");
            File.WriteAllText(jsonFileName, json);

            snapshots.Clear();
        }
    }
}