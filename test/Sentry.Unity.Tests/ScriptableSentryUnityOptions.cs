using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Sentry.Extensibility;
using Sentry.Unity.Json;
using Sentry.Unity.Tests.Stubs;
using UnityEngine;

namespace Sentry.Unity.Tests
{
    [TestFixture]
    public class ScriptableSentryUnityOptionsTests
    {
        private const string TestSentryOptionsFileName = "TestSentryOptions.json";

        class Fixture
        {
            public TestApplication Application { get; set; } = new(
                productName: "TestApplication",
                version: "0.1.0",
                persistentDataPath: "test/persistent/data/path");
        }

        class TestOptionsConfiguration : ScriptableOptionsConfiguration
        {
            public bool GotCalled;
            public override void Configure(SentryUnityOptions options) => GotCalled = true;
        }

        [SetUp]
        public void Setup() => _fixture = new Fixture();
        private Fixture _fixture = null!;

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ToSentryOptions_OptionsCreated_AreEqualToNewOptions(bool isBuilding)
        {
            var expectedOptions = new SentryUnityOptions(_fixture.Application, isBuilding);

            var scriptableOptions = ScriptableObject.CreateInstance<ScriptableSentryUnityOptions>();

            // These are config window specific differences in default values we actually want
            scriptableOptions.Debug = false;
            scriptableOptions.DebugOnlyInEditor = false;
            scriptableOptions.DiagnosticLevel = SentryLevel.Debug;

            var actualOptions = scriptableOptions.ToSentryUnityOptions(isBuilding, _fixture.Application);

            AssertOptions(expectedOptions, actualOptions);
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(false, false)]
        public void ToSentryUnityOptions_ValueMapping_AreEqual(bool isBuilding, bool enableOfflineCaching)
        {
            var expectedOptions = new SentryUnityOptions
            {
                Enabled = false,
                Dsn = "test",
                CaptureInEditor = false,
                EnableLogDebouncing = true,
                TracesSampleRate = 1.0f,
                AutoSessionTracking = false,
                AutoSessionTrackingInterval = TimeSpan.FromSeconds(1),
                AttachStacktrace = true,
                AttachScreenshot = true,
                ScreenshotMaxWidth = 1,
                ScreenshotMaxHeight = 1,
                ScreenshotQuality = 1,
                MaxBreadcrumbs = 1,
                ReportAssembliesMode = ReportAssembliesMode.None,
                SendDefaultPii = true,
                IsEnvironmentUser = true,
                MaxCacheItems = 1,
                CacheDirectoryPath = enableOfflineCaching ? _fixture.Application.PersistentDataPath : null,
                InitCacheFlushTimeout = TimeSpan.FromSeconds(1),
                SampleRate = 0.5f,
                ShutdownTimeout = TimeSpan.FromSeconds(1),
                MaxQueueItems = 1,
                Release = "testRelease",
                Environment = "testEnvironment",
                Debug = true,
                DiagnosticLevel = SentryLevel.Info,
            };

            var scriptableOptions = ScriptableObject.CreateInstance<ScriptableSentryUnityOptions>();
            scriptableOptions.Enabled = expectedOptions.Enabled;
            scriptableOptions.Dsn = expectedOptions.Dsn;
            scriptableOptions.CaptureInEditor = expectedOptions.CaptureInEditor;
            scriptableOptions.EnableLogDebouncing = expectedOptions.EnableLogDebouncing;
            scriptableOptions.TracesSampleRate = expectedOptions.TracesSampleRate;
            scriptableOptions.AutoSessionTracking = expectedOptions.AutoSessionTracking;
            scriptableOptions.AutoSessionTrackingInterval = (int)expectedOptions.AutoSessionTrackingInterval.TotalMilliseconds;
            scriptableOptions.AttachStacktrace = expectedOptions.AttachStacktrace;
            scriptableOptions.AttachScreenshot = expectedOptions.AttachScreenshot;
            scriptableOptions.ScreenshotMaxWidth = expectedOptions.ScreenshotMaxWidth;
            scriptableOptions.ScreenshotMaxHeight = expectedOptions.ScreenshotMaxHeight;
            scriptableOptions.ScreenshotQuality = expectedOptions.ScreenshotQuality;
            scriptableOptions.MaxBreadcrumbs = expectedOptions.MaxBreadcrumbs;
            scriptableOptions.ReportAssembliesMode = expectedOptions.ReportAssembliesMode;
            scriptableOptions.SendDefaultPii = expectedOptions.SendDefaultPii;
            scriptableOptions.IsEnvironmentUser = expectedOptions.IsEnvironmentUser;
            scriptableOptions.MaxCacheItems = expectedOptions.MaxCacheItems;
            scriptableOptions.EnableOfflineCaching = enableOfflineCaching;
            scriptableOptions.InitCacheFlushTimeout = (int)expectedOptions.InitCacheFlushTimeout.TotalMilliseconds;
            scriptableOptions.SampleRate = expectedOptions.SampleRate;
            scriptableOptions.ShutdownTimeout = (int)expectedOptions.ShutdownTimeout.TotalMilliseconds;
            scriptableOptions.MaxQueueItems = expectedOptions.MaxQueueItems;
            scriptableOptions.ReleaseOverride = expectedOptions.Release;
            scriptableOptions.EnvironmentOverride = expectedOptions.Environment;
            scriptableOptions.Debug = expectedOptions.Debug;
            scriptableOptions.DebugOnlyInEditor = false; // Affects Debug otherwise
            scriptableOptions.DiagnosticLevel = expectedOptions.DiagnosticLevel;

            var optionsActual = scriptableOptions.ToSentryUnityOptions(isBuilding, _fixture.Application);

            AssertOptions(expectedOptions, optionsActual);
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(false, false)]
        public void ShouldDebug_DebugOnlyInEditor_ReturnsExpectedDebug(bool isEditorPlayer, bool expectedDebug)
        {
            var scriptableOptions = ScriptableObject.CreateInstance<ScriptableSentryUnityOptions>();
            scriptableOptions.Debug = true;
            scriptableOptions.DebugOnlyInEditor = true;

            var actualDebug = scriptableOptions.ShouldDebug(isEditorPlayer);

            Assert.AreEqual(expectedDebug, actualDebug);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ToSentryUnityOptions_HasOptionsConfiguration_GetsCalled(bool isBuilding)
        {
            var optionsConfiguration = ScriptableObject.CreateInstance<TestOptionsConfiguration>();
            var scriptableOptions = ScriptableObject.CreateInstance<ScriptableSentryUnityOptions>();
            scriptableOptions.OptionsConfiguration = optionsConfiguration;

            scriptableOptions.ToSentryUnityOptions(isBuilding);

            Assert.IsTrue(optionsConfiguration.GotCalled);
        }

        public static void AssertOptions(SentryUnityOptions expected, SentryUnityOptions actual)
        {
            Assert.AreEqual(expected.Enabled, actual.Enabled);
            Assert.AreEqual(expected.Dsn, actual.Dsn);
            Assert.AreEqual(expected.CaptureInEditor, actual.CaptureInEditor);
            Assert.AreEqual(expected.EnableLogDebouncing, actual.EnableLogDebouncing);
            Assert.AreEqual(expected.TracesSampleRate, actual.TracesSampleRate);
            Assert.AreEqual(expected.AutoSessionTracking, actual.AutoSessionTracking);
            Assert.AreEqual(expected.AutoSessionTrackingInterval, actual.AutoSessionTrackingInterval);
            Assert.AreEqual(expected.AttachStacktrace, actual.AttachStacktrace);
            Assert.AreEqual(expected.AttachScreenshot, actual.AttachScreenshot);
            Assert.AreEqual(expected.ScreenshotMaxWidth, actual.ScreenshotMaxWidth);
            Assert.AreEqual(expected.ScreenshotMaxHeight, actual.ScreenshotMaxHeight);
            Assert.AreEqual(expected.ScreenshotQuality, actual.ScreenshotQuality);
            Assert.AreEqual(expected.MaxBreadcrumbs, actual.MaxBreadcrumbs);
            Assert.AreEqual(expected.ReportAssembliesMode, actual.ReportAssembliesMode);
            Assert.AreEqual(expected.SendDefaultPii, actual.SendDefaultPii);
            Assert.AreEqual(expected.IsEnvironmentUser, actual.IsEnvironmentUser);
            Assert.AreEqual(expected.MaxCacheItems, actual.MaxCacheItems);
            Assert.AreEqual(expected.InitCacheFlushTimeout, actual.InitCacheFlushTimeout);
            Assert.AreEqual(expected.SampleRate, actual.SampleRate);
            Assert.AreEqual(expected.ShutdownTimeout, actual.ShutdownTimeout);
            Assert.AreEqual(expected.MaxQueueItems, actual.MaxQueueItems);
            Assert.AreEqual(expected.Release, actual.Release);
            Assert.AreEqual(expected.Environment, actual.Environment);
            Assert.AreEqual(expected.CacheDirectoryPath, actual.CacheDirectoryPath);
            Assert.AreEqual(expected.Debug, actual.Debug);
            Assert.AreEqual(expected.DiagnosticLevel, actual.DiagnosticLevel);
        }

        private static string GetTestOptionsFilePath()
        {
            var assemblyFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Assert.NotNull(assemblyFolderPath);
            return Path.Combine(assemblyFolderPath!, TestSentryOptionsFileName);
        }
    }
}
