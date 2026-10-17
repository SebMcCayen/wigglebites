using System;
using System.IO;
using NUnit.Framework;
using Wigglebites.Persistence;

namespace Wigglebites.Tests
{
    public sealed class LocalSaveStoreTests
    {
        private string _directory;
        private string _path;

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "wigglebites-tests-" + Guid.NewGuid().ToString("N"));
            _path = Path.Combine(_directory, "save.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory)) Directory.Delete(_directory, true);
        }

        [Test]
        public void SaveAndLoad_RoundTripsCurrentVersion()
        {
            var store = new LocalSaveStore(_path);
            store.Save(new LocalSaveData { bestMass = 42.5f });

            var loaded = store.Load();

            Assert.That(loaded.version, Is.EqualTo(LocalSaveData.CurrentVersion));
            Assert.That(loaded.bestMass, Is.EqualTo(42.5f));
        }

        [Test]
        public void Load_RejectsUnknownVersion()
        {
            Directory.CreateDirectory(_directory);
            File.WriteAllText(_path, "{\"version\":999,\"bestMass\":500}");

            var loaded = new LocalSaveStore(_path).Load();

            Assert.That(loaded.version, Is.EqualTo(LocalSaveData.CurrentVersion));
            Assert.That(loaded.bestMass, Is.EqualTo(0f));
        }
    }
}
