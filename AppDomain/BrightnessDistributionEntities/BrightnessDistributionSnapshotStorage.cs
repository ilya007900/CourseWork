using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AppDomain.Utils;

namespace AppDomain.BrightnessDistributionEntities
{
    public class BrightnessDistributionSnapshotStorage
    {
        private const string WorkingDirectory = "C://CameraBaslerNET";

        private readonly List<BrightnessDistributionSnapshot> snapshots = new List<BrightnessDistributionSnapshot>();

        public void Add(BrightnessDistributionSnapshot snapshot)
        {
            snapshots.Add(snapshot);
        }

        public void Save()
        {
            var directoryInfo = new DirectoryInfo(WorkingDirectory);
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            var fileName = FileNameGenerator.GenerateUniqueName(directoryInfo, "BrightnessDistribution");
            var imgFileName = Path.Combine(WorkingDirectory, $"{fileName}.bin");
            using (var stream = File.OpenWrite(imgFileName))
            {
                foreach (var snapshot in snapshots)
                {
                    stream.Write(snapshot.Image, 0, snapshot.Image.Length);
                }
            }

            var snapshotsData = snapshots.Select(x => new
            {
                x.DateTime,
                x.ExposureTime,
                x.Energy,
                x.PixelFormat,
            }).ToArray();

            var json = JsonConvert.SerializeObject(snapshotsData);
            var jsonFileName = Path.Combine(WorkingDirectory, $"{fileName}.json");
            File.WriteAllText(jsonFileName, json);

            snapshots.Clear();
        }
    }
}