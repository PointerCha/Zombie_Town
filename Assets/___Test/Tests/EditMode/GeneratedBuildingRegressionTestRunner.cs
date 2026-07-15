using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestTools;

namespace ZombieTown.GeneratedBuilding.Tests
{
    [InitializeOnLoad]
    public static class GeneratedBuildingRegressionTestRunner
    {
        private const string TestAssemblyName = "ZombieTown.GeneratedBuilding.Tests";
        private const string PlayModeTestAssemblyName = "ZombieTown.GeneratedBuilding.PlayModeTests";
        private const string PlayModeRunningSessionKey = "ZombieTown.GeneratedBuilding.PlayModeTests.Running";

        static GeneratedBuildingRegressionTestRunner()
        {
            if (SessionState.GetBool(PlayModeRunningSessionKey, false))
            {
                RegisterPlayModeCallbacks();
            }
        }

        [MenuItem("Tools/Zombie Town/Run Building Regression Tests _F8")]
        public static void Run()
        {
            TestRunnerApi api = ScriptableObject.CreateInstance<TestRunnerApi>();
            RegressionCallbacks callbacks = new RegressionCallbacks(api);
            api.RegisterCallbacks(callbacks);

            ExecutionSettings settings = new ExecutionSettings(
                new Filter
                {
                    testMode = TestMode.EditMode,
                    assemblyNames = new[] { TestAssemblyName }
                }
            )
            {
                runSynchronously = true
            };

            Debug.Log("Generated building regression tests started.");
            api.Execute(settings);
        }

        [MenuItem("Tools/Zombie Town/Validate Building Family Matrix _F9")]
        public static void ValidateBuildingFamilyMatrix()
        {
            GeneratedGroundGenerator generator = Object.FindFirstObjectByType<GeneratedGroundGenerator>();

            if (generator == null)
            {
                Debug.LogError("Generated building family matrix could not run. GeneratedGroundGenerator was not found in the active scene.");
                return;
            }

            generator.ValidateBuildingFamilyMatrix();
        }

        [MenuItem("Tools/Zombie Town/Run Multi-Building Play Mode Tests _F6")]
        public static void RunMultiBuildingPlayModeTests()
        {
            SessionState.SetBool(PlayModeRunningSessionKey, true);
            RegisterPlayModeCallbacks();
            TestRunnerApi api = ScriptableObject.CreateInstance<TestRunnerApi>();
            Debug.Log("Generated multi-building Play Mode tests started.");
            api.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.PlayMode,
                assemblyNames = new[] { PlayModeTestAssemblyName }
            }));
        }

        private static void RegisterPlayModeCallbacks()
        {
            TestRunnerApi callbackApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            callbackApi.RegisterCallbacks(new PlayModeCallbacks(callbackApi));
        }

        private sealed class RegressionCallbacks : ICallbacks
        {
            private readonly TestRunnerApi api;

            public RegressionCallbacks(TestRunnerApi api)
            {
                this.api = api;
            }

            public void RunStarted(ITestAdaptor testsToRun)
            {
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                string summary =
                    "Generated Building Regression Test Result\n" +
                    $"- Passed: {result.PassCount}\n" +
                    $"- Failed: {result.FailCount}\n" +
                    $"- Skipped: {result.SkipCount}\n" +
                    $"- Inconclusive: {result.InconclusiveCount}\n" +
                    $"- Duration: {result.Duration:F3}s";

                if (result.FailCount > 0)
                {
                    Debug.LogError(summary);
                }
                else
                {
                    Debug.Log(summary);
                }

                api.UnregisterCallbacks(this);
                Object.DestroyImmediate(api);
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result.Test.IsSuite || result.FailCount <= 0)
                {
                    return;
                }

                Debug.LogError(
                    $"Generated building regression failed: {result.FullName}\n" +
                    $"{result.Message}\n{result.StackTrace}"
                );
            }
        }

        private sealed class PlayModeCallbacks : ICallbacks
        {
            private readonly TestRunnerApi api;

            public PlayModeCallbacks(TestRunnerApi api)
            {
                this.api = api;
            }

            public void RunStarted(ITestAdaptor testsToRun) { }
            public void TestStarted(ITestAdaptor test) { }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result != null && result.TestStatus == TestStatus.Failed)
                {
                    Debug.LogError($"Generated multi-building Play Mode test failed: {result.Name}\n{result.Message}\n{result.StackTrace}");
                }
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                SessionState.SetBool(PlayModeRunningSessionKey, false);
                Debug.Log(
                    "Generated Multi-Building Play Mode Test Result\n" +
                    $"- Passed: {result.PassCount}\n" +
                    $"- Failed: {result.FailCount}\n" +
                    $"- Skipped: {result.SkipCount}\n" +
                    $"- Inconclusive: {result.InconclusiveCount}\n" +
                    $"- Duration: {result.Duration:0.000}s"
                );
                api.UnregisterCallbacks(this);
                Object.DestroyImmediate(api);
            }
        }
    }
}
