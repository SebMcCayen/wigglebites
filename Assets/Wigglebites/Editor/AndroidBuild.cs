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
        public static void BuildRelease() => Build(
            development: false,
            appBundle: string.Equals(Environment.GetEnvironmentVariable("WIGGLEBITES_BUILD_AAB"), "1", StringComparison.Ordinal),
            requireReleaseSigning: true);

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
            if (File.Exists(outputPath) && (File.GetAttributes(outputPath) & FileAttributes.ReparsePoint) != 0)
            {
                throw new IOException("Build output file must not be a symbolic link or reparse point.");
            }
            var current = Path.GetDirectoryName(outputPath);
            while (!string.IsNullOrEmpty(current) && !string.Equals(current, projectRoot, StringComparison.Ordinal))
            {
                if (Directory.Exists(current) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                {
                    throw new IOException("Build output directory must not contain symbolic-link or reparse-point ancestors.");
                }
                current = Path.GetDirectoryName(current);
            }
        }
    }
}
