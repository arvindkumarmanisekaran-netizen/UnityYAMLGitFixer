#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Softcircuits.IniFileParser;
using System.Runtime.CompilerServices;
using System.Linq;

[InitializeOnLoad]
public static class FixUnityYAMLGit
{
    public enum eStatus
    {
        NO_GIT_FOLDER,
        FIXED_YAML_MERGE,
        FIXED_WITH_DEFAULT_PATH,
        PLASTIC_SCM_NOT_FOUND_IN_DEFAULT_PATH,
        FIXED_WITH_CUSTOM_PATH,
        PLASTIC_SCM_NOT_FOUND_IN_CUSTOM_PATH
    }

    private const string gitFolderName = ".git";
    private const string configFileName = "config";
    private const string excludeFileName = "info/exclude";
    private const string gitAttributesFileName = ".gitAttributes";

    private static string projectRootPath;
    private static string gitFolderPath;
    private static string gitConfigPath;

    private static string yamlMergerPath = "";

    private static IniFile iniFile = new IniFile();

    private static string unityYAMLMergeToolSectionName = "mergetool \"unityyamlmerge\"";
    private static string unityYAMLMergeToolCMDSettingValue => $"\"{yamlMergerPath}\" merge -p \"$BASE\" \"$REMOTE\" \"$LOCAL\" \"$MERGED\"";
    
    private static string unityYAMLMergeSectionName = "merge \"unityyamlmerge\"";
    private static string unityYAMLMergeName = "Unity SmartMerge";
    private static string unityYAMLMergeDriverSetting => $"\"{yamlMergerPath}\" merge -p %O %A %B %L %P";

    private static string unityYamlGitAttributesFile = "gitattributesyaml";

    private static string fixGitPlasticSCMSOName = "FixUnityYAMLGitSO";

    private static string plasticSCMFileName = "";

    public static eStatus CurrentStatus = eStatus.NO_GIT_FOLDER;

    public static Color orangeColor = new Color(1f, 0.5f, 0);

    private static string defaultPlasticSCMPath = "";
    private static string defaultPlasticSCMBinaryPath => Path.Combine(defaultPlasticSCMPath, plasticSCMFileName);

    private static string plasticSCMFixPath = "";

    private static string YamlMergerDirectory =>
    #if UNITY_EDITOR_WIN 
     "Data\\Tools";
    #elif UNITY_EDITOR_LINUX
     "Data/Tools";
    #elif UNITY_EDITOR_OSX
        "Tools"
    #endif
    private static string yamlFileName = "";

    private static bool initialized = false;

    private static string GetFilePath(
        [CallerFilePath] string path = null) 
    {
        return path;
    }

    static FixUnityYAMLGit()
    {
        if(initialized)
        return;

        string editorPath = Path.GetDirectoryName(EditorApplication.applicationPath);

#if UNITY_EDITOR_WIN
        yamlFileName = "UnityYAMLMerge.exe";
        yamlMergerPath = Path.Combine(editorPath, YamlMergerDirectory, yamlFileName);
        defaultPlasticSCMPath = "C:\\Program Files\\PlasticSCM5\\client";
        plasticSCMFileName = "plastic.exe";
#elif UNITY_EDITOR_OSX
        yamlFileName = "UnityYAMLMerge";
        editorPath = Path.GetDirectoryName(editorPath);
        yamlMergerPath = Path.Combine(editorPath, YamlMergerDirectory, yamlFileName);
        defaultPlasticSCMPath = "/Applications/PlasticSCM.app/Contents/MacOS/";
#elif  UNITY_EDITOR_LINUX
        yamlFileName = "UnityYAMLMerge";
        yamlMergerPath = Path.Combine(editorPath, YamlMergerDirectory, yamlFileName);
        defaultPlasticSCMPath = "/usr/bin";
        plasticSCMFileName = "plasticgui";
#endif

        string assetsPath = Application.dataPath;
        projectRootPath = Path.GetDirectoryName(assetsPath);

        gitFolderPath = Path.Combine(projectRootPath, gitFolderName);
        gitConfigPath = Path.Combine(gitFolderPath, configFileName);

        if (Directory.Exists(gitFolderPath))
        {
            string gitExcludeFullPath = Path.Combine(gitFolderPath, excludeFileName);

            string fixGitPlasticSCMSOPath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(GetFilePath())), fixGitPlasticSCMSOName).Trim();

            if(File.Exists(gitExcludeFullPath))
            {
                string[] excludeLines = File.ReadAllLines(gitExcludeFullPath);
              
                if(!excludeLines.Contains(fixGitPlasticSCMSOPath))
                {
                    using(FileStream fileStream = File.Open(gitExcludeFullPath, FileMode.Append))
                    {
                        StreamWriter streamWriter = new StreamWriter(fileStream);
                    
                        streamWriter.WriteLine(fixGitPlasticSCMSOPath);
                        streamWriter.Close();
                    }
                }
            }
        }
 
        Fix(defaultPlasticSCMPath);

        initialized = true;
    }

    public static string GetCurrentStatus(out Color color)
    {
        string status = "";
        color = Color.red;

        switch(CurrentStatus)
        {
            case eStatus.NO_GIT_FOLDER:
                status = "No .git Folder";
                color = Color.red;
                break;
            
            case eStatus.FIXED_YAML_MERGE:
                status = $"Fixed YAML Merge with binary<br> {yamlMergerPath}";
                color = Color.green;
                break;

            case eStatus.FIXED_WITH_DEFAULT_PATH:
                status = $"Fixed YAML Merge with binary<br> {yamlMergerPath} <br> Fixed PlasticSCM with Binary <br> {defaultPlasticSCMBinaryPath} in DEFAULT path <br> ALL DONE!";
                color = Color.green;
                break;

            case eStatus.PLASTIC_SCM_NOT_FOUND_IN_DEFAULT_PATH:
                status = $"Fixed YAML Merge Only<br> {yamlMergerPath} <br> PlasticSCM Binary <br> {defaultPlasticSCMBinaryPath} <br> NOT FOUND IN DEFAULT path";
                color = orangeColor;
                break;
            
            case eStatus.FIXED_WITH_CUSTOM_PATH:
                status = $"Fixed YAML Merge with binary<br> {yamlMergerPath} <br> Fixed PlasticSCM with Binary <br> {plasticSCMFixPath} in CUSTOM path <br> ALL DONE!";
                color = Color.green;
                break;

            case eStatus.PLASTIC_SCM_NOT_FOUND_IN_CUSTOM_PATH:
                status = $"Fixed YAML Merge Only with binary<br> {yamlMergerPath} <br> PlasticSCM Binary <br> {plasticSCMFixPath} <br> NOT FOUND IN CUSTOM path";
                color = orangeColor;
                break;
        }

        return status;
    }
    
    public static void FixYamlMerge()
    {
        if (Directory.Exists(gitFolderPath))
        {
            if (File.Exists(gitConfigPath))
            {
                iniFile.Load(gitConfigPath);

                iniFile.SetSetting(unityYAMLMergeToolSectionName, "cmd", unityYAMLMergeToolCMDSettingValue);
                iniFile.SetSetting(unityYAMLMergeToolSectionName, "trustExitCode", false);
                iniFile.SetSetting(unityYAMLMergeSectionName, "name", unityYAMLMergeName);
                iniFile.SetSetting(unityYAMLMergeSectionName, "driver", unityYAMLMergeDriverSetting);
            
                iniFile.Save(gitConfigPath);

                string attributesFilePath = Path.Combine(projectRootPath, gitAttributesFileName);

                if(!File.Exists(attributesFilePath))
                {
                    FileStream fileStream = File.Open(attributesFilePath, FileMode.CreateNew);
                    fileStream.Close();
                }

                string[] gitAttributesLines = File.ReadAllLines(attributesFilePath);

                using(FileStream fileStream = File.Open(attributesFilePath, FileMode.Append))
                {
                    string unityYamlGitAttributesFilePath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(GetFilePath())), 
                                                                unityYamlGitAttributesFile).Trim();

                    string[] unityyamlLines = File.ReadAllLines(unityYamlGitAttributesFilePath);

                    StreamWriter streamWriter = new StreamWriter(fileStream);
                    
                    foreach(string unityyamlLine in unityyamlLines)
                    {
                        if(!gitAttributesLines.Contains(unityyamlLine))
                        {
                            streamWriter.WriteLine(unityyamlLine);
                        }    
                    }

                    streamWriter.Close();
                }


                CurrentStatus = eStatus.FIXED_YAML_MERGE;
            }  
        }
        else
        {
            CurrentStatus = eStatus.NO_GIT_FOLDER;
            Debug.Log("Unity YAML Merge: No .git foder");
        }
    }

    public static void Fix(string plasticSCMPath)
    {
        FixYamlMerge();

        // if(CurrentStatus != eStatus.NO_GIT_FOLDER)
        // {
        //     if (Directory.Exists(gitFolderPath))
        //     {
        //         if (File.Exists(gitConfigPath))
        //         {
        //             string plasticSCMBinaryPath = Path.Combine(plasticSCMPath, plasticSCMFileName);

        //             if(File.Exists(plasticSCMBinaryPath))
        //             {
        //                 plasticSCMFixPath = plasticSCMBinaryPath;

        //                 iniFile.Load(gitConfigPath);

        //                 iniFile.SetSetting(unityYAMLMergeToolSectionName, "cmd", unityYAMLMergeToolCMDSettingValue);
        //                 iniFile.SetSetting(unityYAMLMergeToolSectionName, "trustExitCode", false);
        //                 iniFile.SetSetting(unityYAMLMergeSectionName, "name", unityYAMLMergeName);
        //                 iniFile.SetSetting(unityYAMLMergeSectionName, "driver", unityYAMLMergeDriverSetting);


        //                 if(plasticSCMFixPath == defaultPlasticSCMBinaryPath)
        //                     CurrentStatus = eStatus.FIXED_WITH_DEFAULT_PATH;
        //                  else
        //                     CurrentStatus = eStatus.FIXED_WITH_CUSTOM_PATH;
        //             }
        //             else
        //             {
        //                 if(plasticSCMFixPath == defaultPlasticSCMBinaryPath)
        //                     CurrentStatus = eStatus.PLASTIC_SCM_NOT_FOUND_IN_DEFAULT_PATH;
        //                 else
        //                     CurrentStatus = eStatus.PLASTIC_SCM_NOT_FOUND_IN_CUSTOM_PATH;

        //                 Debug.Log($"PlasticSCM binary {plasticSCMBinaryPath} not found");
        //             }
        //         }  
        //     }
        // }
    }
}
#endif