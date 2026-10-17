using System;
using System.IO;
using UnityEngine;

namespace Wigglebites.Persistence
{
    [Serializable]
    public sealed class LocalSaveData
    {
        public const int CurrentVersion = 1;
        public int version = CurrentVersion;
        public float bestMass;
    }

    public sealed class LocalSaveStore
    {
        private readonly string _path;

        public LocalSaveStore(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Save path is required.", nameof(path));
            _path = path;
        }

        public LocalSaveData Load()
        {
            try
            {
                if (!File.Exists(_path)) return new LocalSaveData();
                var data = JsonUtility.FromJson<LocalSaveData>(File.ReadAllText(_path));
                if (data == null || data.version != LocalSaveData.CurrentVersion || data.bestMass < 0f)
                {
                    return new LocalSaveData();
                }
                return data;
            }
            catch (Exception exception) when (
                exception is IOException ||
                exception is UnauthorizedAccessException ||
                exception is ArgumentException)
            {
                return new LocalSaveData();
            }
        }

        public void Save(LocalSaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            data.version = LocalSaveData.CurrentVersion;
            data.bestMass = Math.Max(0f, data.bestMass);

            var directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            var temporaryPath = _path + ".tmp";
            File.WriteAllText(temporaryPath, JsonUtility.ToJson(data));
            if (File.Exists(_path)) File.Delete(_path);
            File.Move(temporaryPath, _path);
        }
    }
}
