using System;
using System.IO;
using UnityEngine;

namespace Wigglebites.Persistence
{
    [Serializable]
    public sealed class LocalSaveData
    {
        public const int CurrentVersion = 2;
        public int version = CurrentVersion;
        public float bestMass;
        public bool reducedEffects;
        public bool soundEnabled = true;
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
            if (TryLoad(_path, out var primary)) return primary;
            if (TryLoad(_path + ".tmp", out var temporary)) return temporary;
            if (TryLoad(_path + ".bak", out var backup)) return backup;
            return new LocalSaveData();
        }

        public void Save(LocalSaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            data.version = LocalSaveData.CurrentVersion;
            data.bestMass = Math.Max(0f, data.bestMass);

            var directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            var temporaryPath = _path + ".tmp";
            var backupPath = _path + ".bak";
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            File.WriteAllText(temporaryPath, JsonUtility.ToJson(data));
            if (!File.Exists(_path))
            {
                File.Move(temporaryPath, _path);
                return;
            }

            if (File.Exists(backupPath)) File.Delete(backupPath);
            File.Move(_path, backupPath);
            try
            {
                File.Move(temporaryPath, _path);
            }
            catch
            {
                if (!File.Exists(_path) && File.Exists(backupPath)) File.Move(backupPath, _path);
                throw;
            }

            try
            {
                File.Delete(backupPath);
            }
            catch (Exception exception) when (
                exception is IOException ||
                exception is UnauthorizedAccessException ||
                exception is ArgumentException)
            {
                _ = exception;
                // The new primary is durable; a stale backup is safe and recoverable.
            }
        }

        private static bool TryLoad(string path, out LocalSaveData data)
        {
            data = null;
            try
            {
                if (!File.Exists(path) || new FileInfo(path).Length > 16 * 1024) return false;
                var json = File.ReadAllText(path);
                var envelope = JsonUtility.FromJson<SaveVersionEnvelope>(json);
                if (envelope != null && envelope.version == 1)
                {
                    var legacy = JsonUtility.FromJson<LegacyLocalSaveData>(json);
                    if (legacy == null || !IsValidMass(legacy.bestMass)) return false;
                    data = new LocalSaveData { bestMass = legacy.bestMass };
                    return true;
                }

                var candidate = JsonUtility.FromJson<LocalSaveData>(json);
                if (candidate == null ||
                    candidate.version != LocalSaveData.CurrentVersion ||
                    !IsValidMass(candidate.bestMass))
                {
                    return false;
                }
                data = candidate;
                return true;
            }
            catch (Exception exception) when (
                exception is IOException ||
                exception is UnauthorizedAccessException ||
                exception is ArgumentException)
            {
                return false;
            }
        }

        private static bool IsValidMass(float mass) =>
            mass >= 0f && !float.IsNaN(mass) && !float.IsInfinity(mass);

        [Serializable]
        private sealed class SaveVersionEnvelope
        {
            public int version;
        }

        [Serializable]
        private sealed class LegacyLocalSaveData
        {
            public float bestMass;
        }
    }
}
