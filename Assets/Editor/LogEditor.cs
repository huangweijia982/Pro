using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System;


public class LogEditor : Editor
{
    private static readonly string[] LuaFilterConfig = { "util/dispatcher", "util/PrintUtil", "[C]" };//lua过滤配置
    private static readonly string[] CsFilterConfig = { "Debuger" };//c#过滤配置
    private const string EXTERNAL_EDITOR_PATH_KEY = "mTv8";



    [UnityEditor.Callbacks.OnOpenAssetAttribute(2)]
    public static bool OnOpenAsset2(int instanceID, int line)
    {
        string logText = GetLogText();
        string filePath = "";
        int fileLine = 0;
        try
        {           
            if (logText.Contains("stack traceback:"))//用这个来判断是否是lua那边的日志
            {

                string[] stackLs = logText.Split(new string[1] { "stack traceback:" }, System.StringSplitOptions.None);
                string[] stack = Regex.Split(stackLs[1], "\\n+", RegexOptions.IgnoreCase);
                for (int i = 1; i < stack.Length; i++)
                {
                    bool isMeg = true;
                    foreach (var a in LuaFilterConfig)
                    {
                        if (stack[i].Contains(a))
                        {
                            isMeg = false;
                            break;
                        }
                    }
                    if (isMeg)
                    {
                        string[] v = stack[i].Split(new char[1] { ':' });
                        filePath = v[0];
                        fileLine = int.Parse(v[1]);
                        filePath = GameSetting.LuaDir+ filePath.Trim()+".lua";
                        OpenFileAtLineExternal(filePath, fileLine);
                        return true;
                    }
                }                            
            }
            else
            {
                string[] stack = Regex.Split(logText, "\\n+", RegexOptions.IgnoreCase);
                for (int i = 1; i < stack.Length; i++)
                {
                    bool isMeg = true;
                    foreach (var a in CsFilterConfig)
                    {
                        if (stack[i].Contains(a)||!stack[i].Contains("at"))
                        {
                            isMeg = false;
                            break;
                        }
                    }
                    if (isMeg)
                    {
                        string[] z = stack[i].Split(new string[1] { "(at "}, System.StringSplitOptions.None);
                        string[] v =z[1].Split(new char[1] { ':' });
                        filePath = v[0];
                        fileLine = int.Parse(v[1].Substring(0,v[1].Length-1));
                        filePath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets")) + filePath.Trim();
                        UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(filePath, fileLine);
                        return true;
                    }              
                }
               
            }           
        }
        catch(Exception e)
        {
            return false;
        }
        return false;
    }


    static void OpenFileAtLineExternal(string fileName, int line)
    {
        string editorPath = EditorUserSettings.GetConfigValue(EXTERNAL_EDITOR_PATH_KEY);
        if (string.IsNullOrEmpty(editorPath) || !File.Exists(editorPath))
        {   // 没有path就弹出面板设置
            SetExternalEditorPath();
        }
        OpenFileWith(fileName, line);
    }

    [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
    static extern void SwitchToThisWindow(IntPtr hWnd, bool turnOn);
    static void OpenFileWith(string fileName, int line)
    {
        string editorPath = EditorUserSettings.GetConfigValue(EXTERNAL_EDITOR_PATH_KEY);
        System.Diagnostics.Process proc = new System.Diagnostics.Process();
        proc.StartInfo.FileName = editorPath;
        proc.StartInfo.Arguments = string.Format("{0}:{1}", fileName, line);
        proc.Start();
        if (proc.Start())
            SwitchToThisWindow(proc.MainWindowHandle, true);
    }

    [MenuItem("Tools/SetEditorPath")]
    static void SetExternalEditorPath()
    {
        string path = EditorUserSettings.GetConfigValue(EXTERNAL_EDITOR_PATH_KEY);
        path = EditorUtility.OpenFilePanel(
                    "Select Editor",
                    path,
                    "exe");

        if (path != "")
        {
            EditorUserSettings.SetConfigValue(EXTERNAL_EDITOR_PATH_KEY, path);
            Debug.Log("设置lua编辑器路径: " + path);
        }
    }


    static string GetLogText()
    {
        // 找到UnityEditor.EditorWindow的assembly
        var assembly_unity_editor = Assembly.GetAssembly(typeof(UnityEditor.EditorWindow));
        if (assembly_unity_editor == null) return null;

        // 找到类UnityEditor.ConsoleWindow
        var type_console_window = assembly_unity_editor.GetType("UnityEditor.ConsoleWindow");
        if (type_console_window == null) return null;
        // 找到UnityEditor.ConsoleWindow中的成员ms_ConsoleWindow
        var field_console_window = type_console_window.GetField("ms_ConsoleWindow", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        if (field_console_window == null) return null;
        // 获取ms_ConsoleWindow的值
        var instance_console_window = field_console_window.GetValue(null);
        if (instance_console_window == null) return null;

        // 如果console窗口时焦点窗口的话，获取stacktrace
        if ((object)UnityEditor.EditorWindow.focusedWindow == instance_console_window)
        {
            // 通过assembly获取类ListViewState
            var type_list_view_state = assembly_unity_editor.GetType("UnityEditor.ListViewState");
            if (type_list_view_state == null) return null;

            // 找到类UnityEditor.ConsoleWindow中的成员m_ListView
            var field_list_view = type_console_window.GetField("m_ListView", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field_list_view == null) return null;

            // 获取m_ListView的值
            var value_list_view = field_list_view.GetValue(instance_console_window);
            if (value_list_view == null) return null;

            // 找到类UnityEditor.ConsoleWindow中的成员m_ActiveText
            var field_active_text = type_console_window.GetField("m_ActiveText", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field_active_text == null) return null;

            // 获得m_ActiveText的值，就是我们需要的stacktrace
            string value_active_text = field_active_text.GetValue(instance_console_window).ToString();
            return value_active_text;
        }
        return null;
    }
    /// <summary>
    /// 获取字符串中特定两个字符中间的字符串
    /// </summary>
    /// <param name="sourse"></param>
    /// <param name="startstr"></param>
    /// <param name="endstr"></param>
    /// <returns></returns>
    public static string MidStrEx_New(string sourse, string startstr, string endstr)
    {
        string result = string.Empty;
        int startindex, endindex;
        try
        {
            startindex = sourse.IndexOf(startstr);
            if (startindex == -1)
                return result;
            string tmpstr = sourse.Substring(startindex + startstr.Length);
            endindex = tmpstr.IndexOf(endstr);
            if (endindex == -1)
                return result;
            result = tmpstr.Remove(endindex);
        }
        catch (Exception ex)
        {
            Debug.Log("获取字符串错误:" + ex.Message);
        }
        return result;
    }
}