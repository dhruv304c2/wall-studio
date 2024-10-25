using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build.Reporting;

public class OculusBuildUploaderWindow : EditorWindow
{
    private string appId = "";
    private string appSecret = "";
    private ReleaseChannel channel = ReleaseChannel.Beta;
    private string apkName = "YourApp.apk";
    private string buildVersion = "";
    private int buildVersionCode = 1;

    // Keys for Editor Prefs (to store cached values)
    private const string AppIdKey = "OculusAppId";
    private const string AppSecretKey = "OculusAppSecret";
    private const string UploadChannelKey = "OculusUploadChannelKey";
    private const string ApkNameKey = "OculusApkName";
    private const string BuildVersionKey = "BuildVersionKey";
    private const string BuildVersionCodeKey = "BuildVersionCodeKey";

    // Add menu item to open the custom build window
    [MenuItem("Build/Oculus Build and Upload")]
    public static void ShowWindow()
    {
        OculusBuildUploaderWindow window = GetWindow<OculusBuildUploaderWindow>("Oculus Build and Upload");
        window.LoadCachedData();  // Load cached data when window is opened
    }

    public void OnDisable(){
        SaveCachedData();
    }

#region Release Channels
    public enum ReleaseChannel{
        Alpha,
        Beta,
        RC,
        Live
    }

    Dictionary<ReleaseChannel,string> releaseChannelStringMap = new Dictionary<ReleaseChannel, string>(){
        {ReleaseChannel.Alpha, "alpha"},
        {ReleaseChannel.Beta, "beta"},
        {ReleaseChannel.RC, "rc"},
        {ReleaseChannel.Live, "live"}
    };

    private string GetChannelString(ReleaseChannel c){
            if(releaseChannelStringMap.ContainsKey(c)){
                return releaseChannelStringMap[c];
            }

            throw new System.Exception("ReleaseChannel does not exist in \"release channel string map\"");
    }

    private ReleaseChannel GetChannelEnumFromString(string channel){
        if(releaseChannelStringMap.ContainsValue(channel)){
            return releaseChannelStringMap.Where(pair => pair.Value == channel).Select(pair => pair.Key).First();
        }

        throw new System.Exception("Relsase channel string not found!");
    }
#endregion

    private void OnGUI()
    {
        GUILayout.Label("Meta (Oculus) App Build and Upload", EditorStyles.boldLabel);

        // Input fields for App ID, App Secret, APK Name, and ovr-platform-util path
        appId = EditorGUILayout.TextField("App ID", appId);
        appSecret = EditorGUILayout.TextField("App Secret", appSecret);
        channel = (ReleaseChannel)EditorGUILayout.EnumPopup("UploadChannel", channel);

        GUILayout.Space(10);
        GUILayout.Label("Build Settings", EditorStyles.boldLabel);

        apkName = EditorGUILayout.TextField("APK Name", apkName);
        buildVersion = EditorGUILayout.TextField("Build Version", buildVersion);
        buildVersionCode = EditorGUILayout.IntField("Build Version Code", buildVersionCode);

        GUILayout.Space(10);
        if (GUILayout.Button("Build and Upload"))
        {
            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
            {
                EditorUtility.DisplayDialog("Error", "Please provide all required fields", "OK");
                return;
            }

            // Save the input data to cache before building
            SaveCachedData();

            // Call the function to build and upload the APK
            BuildAndUploadToMeta();
        }
    }

    private void BuildAndUploadToMeta()
    {
        //Apply version settings
        PlayerSettings.bundleVersion = buildVersion;
        PlayerSettings.Android.bundleVersionCode = buildVersionCode;

        // Build the APK
        string apkPath = Path.Combine(Application.dataPath, "../Builds/" + $"{apkName}_v{buildVersion}_({buildVersionCode}).apk");
        var report = BuildPipeline.BuildPlayer(GetScenes(), apkPath, BuildTarget.Android, BuildOptions.None);

        if(report.summary.result == BuildResult.Failed) return;

        // Call the Oculus CLI to upload the APK
        UploadToOculus(apkPath);

        //Increment version code on successful build
        buildVersionCode++;
        SaveCachedData();
        LoadCachedData();
    }

    private static string[] GetScenes()
    {
        // Automatically grab all scenes in the build settings
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }
        return scenes;
    }


    private void UploadToOculus(string apkPath)
    {
        // Assume ovr-platform-util is placed in the same folder as the script (relative to this script's location)
        string scriptPath = Path.GetDirectoryName(Application.dataPath + "/Editor/Build/"); // Adjust this path if necessary
        string ovrPlatformUtilPath = Path.Combine(scriptPath, "ovr-platform-util");  // Assuming ovr-platform-util is in the same folder

        if (!File.Exists(ovrPlatformUtilPath))
        {
            EditorUtility.DisplayDialog("Error", "ovr-platform-util not found in the expected location.", "OK");
            return;
        }

        // Construct the upload command using the inputs
        string args = $"upload-quest-build --app_id {appId} --app_secret {appSecret} --apk \"{apkPath}\" --channel \"{GetChannelString(channel)}\" ";

        // Run the ovr-platform-util command line tool to upload the APK
        Process process = new Process();
        process.StartInfo.FileName = ovrPlatformUtilPath;
        process.StartInfo.Arguments = args;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        process.WaitForExit();

        // Output the result in the console
        UnityEngine.Debug.Log(process.StandardOutput.ReadToEnd());
        UnityEngine.Debug.LogError(process.StandardError.ReadToEnd());

        if (process.ExitCode == 0)
        {
            EditorUtility.DisplayDialog("Success", "Build and Upload Completed", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Upload Failed. Check Console for Details.", "OK");
        }
    }

    // Save the data entered in the GUI to PlayerPrefs
    private void SaveCachedData()
    {
        PlayerPrefs.SetString(AppIdKey, appId);
        PlayerPrefs.SetString(AppSecretKey, appSecret);
        PlayerPrefs.SetString(ApkNameKey, apkName);
        PlayerPrefs.SetString(BuildVersionKey, buildVersion);
        PlayerPrefs.SetInt(BuildVersionCodeKey, buildVersionCode);
        PlayerPrefs.SetString(UploadChannelKey, GetChannelString(channel));
    }

    // Load cached data from PlayerPrefs
    private void LoadCachedData()
    {
        appId = PlayerPrefs.GetString(AppIdKey, "");
        appSecret = PlayerPrefs.GetString(AppSecretKey, "");
        apkName = PlayerPrefs.GetString(ApkNameKey, "YourApp.apk");
        buildVersion = PlayerPrefs.GetString(BuildVersionKey, "0.1");
        buildVersionCode = PlayerPrefs.GetInt(BuildVersionCodeKey, 1);
        channel = GetChannelEnumFromString(PlayerPrefs.GetString(UploadChannelKey));
    }
}
