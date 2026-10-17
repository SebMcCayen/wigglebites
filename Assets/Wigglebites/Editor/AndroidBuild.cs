using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Wigglebites.Editor
{
    public static class AndroidBuild
    {
        private const string DefaultApkPath = "Builds/Android/Wigglebites.apk";
        private const string DefaultBundlePath = "Builds/Android/Wigglebites.aab";

        [MenuItem("Wigglebites/Build Android/Development APK")]
        public static void BuildDevelopmentApk() => Build(development: true, appBundle: false, requireReleaseSigning: false);

        [MenuItem("Wigglebites/Build Android/Validation APK")]
        public static void BuildValidation() => Build(development: false, appBundle: false, requireReleaseSigning: false);

        [MenuItem("Wigglebites/Build Android/Release")]
        public static void BuildRelease()
        {
            var signing = new SigningSettings();
            try
            {
                ApplyReleaseSigningEnvironment();
                Build(
                    development: false,
                    appBundle: string.Equals(Environment.GetEnvironmentVariable("WIGGLEBITES_BUILD_AAB"), "1", StringComparison.Ordinal),
                    requireReleaseSigning: true);
            }
            finally
            {
                signing.Restore();
            }
        }

        private static void Build(bool development, bool appBundle, bool requireReleaseSigning)
        {
            if (requireReleaseSigning) ValidateReleaseSigning();
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled && !string.IsNullOrWhiteSpace(scene.path))
                .Select(scene => scene.path)
                .ToArray();
            if (scenes.Length == 0) throw new InvalidOperationException("At least one enabled build scene is required.");
            for (var index = 0; index < scenes.Length; index++)
            {
                if (!File.Exists(scenes[index])) throw new FileNotFoundException("Build scene is missing.", scenes[index]);
            }

            var configuredPath = Environment.GetEnvironmentVariable("WIGGLEBITES_BUILD_PATH");
            var relativePath = string.IsNullOrWhiteSpace(configuredPath)
                ? appBundle ? DefaultBundlePath : DefaultApkPath
                : configuredPath;
            var outputPath = ResolveProjectOutputPath(relativePath, appBundle ? ".aab" : ".apk");
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? throw new InvalidOperationException("Build output has no directory."));

            EditorUserBuildSettings.buildAppBundle = appBundle;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                throw new InvalidOperationException("Could not activate the Android build target. Install Unity Android Build Support.");
            }

            var options = BuildOptions.StrictMode;
            if (development) options |= BuildOptions.Development | BuildOptions.AllowDebugging;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = options
            });
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"Android build failed: {report.summary.result} with {report.summary.totalErrors} errors.");
            }

            Debug.Log($"Android build succeeded: {outputPath} ({report.summary.totalSize} bytes, {report.summary.totalTime}).");
        }

        private static void ValidateReleaseSigning()
        {
            if (!PlayerSettings.Android.useCustomKeystore ||
                string.IsNullOrWhiteSpace(PlayerSettings.Android.keystoreName) ||
                string.IsNullOrEmpty(PlayerSettings.Android.keystorePass) ||
                string.IsNullOrWhiteSpace(PlayerSettings.Android.keyaliasName) ||
                string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass))
            {
                throw new InvalidOperationException(
                    "Release builds require a custom Android keystore, alias and passwords supplied by the secure build environment.");
            }
        }

        private static void ApplyReleaseSigningEnvironment()
        {
            var keystorePath = Environment.GetEnvironmentVariable("WIGGLEBITES_KEYSTORE_PATH");
            var keystorePassword = Environment.GetEnvironmentVariable("WIGGLEBITES_KEYSTORE_PASSWORD");
            var alias = Environment.GetEnvironmentVariable("WIGGLEBITES_KEY_ALIAS");
            var aliasPassword = Environment.GetEnvironmentVariable("WIGGLEBITES_KEY_PASSWORD");
            if (new[] { keystorePath, keystorePassword, alias, aliasPassword }.All(string.IsNullOrEmpty)) return;
            if (new[] { keystorePath, keystorePassword, alias, aliasPassword }.Any(string.IsNullOrWhiteSpace))
            {
                throw new InvalidOperationException("All Android signing environment variables must be supplied together.");
            }

            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystorePath;
            PlayerSettings.Android.keystorePass = keystorePassword;
            PlayerSettings.Android.keyaliasName = alias;
            PlayerSettings.Android.keyaliasPass = aliasPassword;
        }

        private static string ResolveProjectOutputPath(string relativePath, string requiredExtension)
        {
            if (Path.IsPathRooted(relativePath)) throw new ArgumentException("Build output must be relative to the project root.", nameof(relativePath));
            var normalized = relativePath.Replace('\\', '/');
            var segments = normalized.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0 || segments.Any(segment => segment == ".." || segment == "."))
            {
                throw new ArgumentException("Build output must not contain relative traversal segments.", nameof(relativePath));
            }
            if (!string.Equals(segments[0], "Builds", StringComparison.Ordinal))
            {
                throw new ArgumentException("Build output must be below the project Builds directory.", nameof(relativePath));
            }
            if (!string.Equals(Path.GetExtension(normalized), requiredExtension, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Build output must use the {requiredExtension} extension.", nameof(relativePath));
            }

            var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            var outputPath = Path.GetFullPath(Path.Combine(projectRoot, normalized));
            var rootPrefix = projectRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!outputPath.StartsWith(rootPrefix, StringComparison.Ordinal))
            {
                throw new ArgumentException("Build output escaped the project root.", nameof(relativePath));
            }
            RejectLinkedOutputAncestors(projectRoot, outputPath);
            return outputPath;
        }

        private static void RejectLinkedOutputAncestors(string projectRoot, string outputPath)
        {
            var current = Path.GetDirectoryName(outputPath);
            while (!string.IsNullOrEmpty(current) && !string.Equals(current, projectRoot, StringComparison.Ordinal))
            {
                if (Directory.Exists(current) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                {
                    throw new IOException("Build output directory must not contain symbolic-link or reparse-point ancestors.");
                }
                current = Path.GetDirectoryName(current);
            }

            var outputDirectory = Path.GetDirectoryName(outputPath);
            var matchingEntries = !string.IsNullOrEmpty(outputDirectory) && Directory.Exists(outputDirectory)
                ? Directory.EnumerateFileSystemEntries(outputDirectory)
                    .Where(entry => string.Equals(Path.GetFullPath(entry), outputPath, StringComparison.Ordinal))
                    .ToArray()
                : Array.Empty<string>();
            if (matchingEntries.Length != 0)
            {
                if (!File.Exists(outputPath) || Directory.Exists(outputPath) ||
                    (File.GetAttributes(outputPath) & FileAttributes.ReparsePoint) != 0)
                {
                    throw new IOException("Build output must not be a directory, symbolic link or reparse point.");
                }
                File.Delete(outputPath);
            }
        }

        private sealed class SigningSettings
        {
            private readonly bool useCustomKeystore = PlayerSettings.Android.useCustomKeystore;
            private readonly string keystoreName = PlayerSettings.Android.keystoreName;
            private readonly string keystorePass = PlayerSettings.Android.keystorePass;
            private readonly string keyaliasName = PlayerSettings.Android.keyaliasName;
            private readonly string keyaliasPass = PlayerSettings.Android.keyaliasPass;

            public void Restore()
            {
                PlayerSettings.Android.useCustomKeystore = useCustomKeystore;
                PlayerSettings.Android.keystoreName = keystoreName;
                PlayerSettings.Android.keystorePass = keystorePass;
                PlayerSettings.Android.keyaliasName = keyaliasName;
                PlayerSettings.Android.keyaliasPass = keyaliasPass;
            }
        }
    }
}
