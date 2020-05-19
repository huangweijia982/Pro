
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using System;
using TMPro;
using static UnityEngine.UI.Button;

public class LuaModule : EditorWindow
{
    private static LuaModule myWindow;
    [MenuItem("Lua工具/创建lua模块")]
    static void ShowWindow()
    {
        luaModulePath = GameSetting.LuaDir + "modules/";

        myWindow = EditorWindow.CreateInstance<LuaModule>();
        myWindow.Show();
    }

    private static string luaModulePath;
    private string ModuleFactoryPath = "";
    private string WindowDefPath = "";
    GameObject obj;
    GameObject itemObj;
    private Dictionary<string, Type> ComDic;
    NameInfo nameInfo;
    string itemModuleName = "";

    void ShowMsg(string msg)
    {
        EditorUtility.DisplayDialog("提示", msg, "确定");
    }
    void OnGUI()
    {
        //GameObject obj = Selection.activeGameObject;   
        GUILayout.Label(new GUIContent("当前Lua模块路径：" + luaModulePath));
        String name = Selection.activeGameObject == null ? "未选中" : Selection.activeGameObject.name;
        GUILayout.Label(new GUIContent("当前选中的物体：" + name));
        ModuleFactoryPath = GameSetting.ProjectRoot + "/lua/game/config/ModuleFactory.lua";
        WindowDefPath = GameSetting.ProjectRoot + "/lua/game/config/WindowDef.lua";
        EditorGUILayout.BeginVertical();
        if (GUILayout.Button(new GUIContent("创建Lua模块"), GUILayout.Width(120)))
        {
            obj = Selection.activeGameObject;
            if (obj == null)
            {

                ShowMsg("当前moduel预制体为空");
                return;
            }
            nameInfo = new NameInfo(obj.name.Replace("Panel", "") + "Module");
            string panelNameText = nameInfo.UpModuleName + "Panel";
            if (Directory.Exists(luaModulePath + nameInfo.LowModuleName))
            {
                ShowMsg(nameInfo.LowModuleName + "已存在，请手动删除后再重新生成模块");
            }
            else
            {
                //module
                Directory.CreateDirectory(luaModulePath + nameInfo.LowModuleName); //先创建一个文件夹
                StringBuilder stb = new StringBuilder();
                stb.AppendLine("module('" + nameInfo.UpModuleName + "Module',package.seeall)");
                stb.AppendLine("require(\"modules." + nameInfo.LowModuleName + "." + nameInfo.UpModuleName + "DataSever\")");
                stb.AppendLine("require(\"modules." + nameInfo.LowModuleName + ".view." + nameInfo.UpModuleName + "Panel\")");
                stb.AppendLine("local " + nameInfo.LowModuleName + "Panel=nil\r\n\r\n");
                stb.AppendLine("function initModule()\r\n\tinitListener()\r\nend");
                stb.AppendLine("function initListener()\r\n\r\nend");
                stb.AppendLine("function open" + nameInfo.UpModuleName + "Panel()\r\n\tif " + nameInfo.LowModuleName + "Panel==nil then\r\n\t\t" + nameInfo.LowModuleName + "Panel=" + nameInfo.UpModuleName + "Panel.new(WindowDef." + nameInfo.UpModuleName + "Panel)\r\n\tend\r\n\t" + nameInfo.LowModuleName + "Panel:openPanel()" + "\r\nend");
                string moudelPath = luaModulePath + nameInfo.LowModuleName + "/" + nameInfo.UpModuleName + "Module.text";
                writeFile(moudelPath, stb.ToString());

                //dataSever
                StringBuilder serString = new StringBuilder();
                serString.AppendLine("module('" + nameInfo.UpModuleName + "DataServer',package.seeall)");
                string serverPath = luaModulePath + nameInfo.LowModuleName + "/" + nameInfo.UpModuleName + "DataSever.text";
                writeFile(serverPath, serString.ToString());


                //panel
                Directory.CreateDirectory(luaModulePath + nameInfo.LowModuleName + "/view"); //先创建一个文件夹
                                                                                             //File.Create(luaModulePath + "view/" + panelNameText + ".text");
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("module('" + nameInfo.UpModuleName + "Module" + "',package.seeall)");
                builder.AppendLine("---@class "+panelNameText+ ":BasePanel");
                builder.AppendLine(panelNameText + " = class(\"" + panelNameText + "\",BasePanel)");

                //initui方法模板开始
                builder.AppendLine("function " + panelNameText + ":initUI()");
                if (obj != null)
                {
                    string code = GenPrefabComponentsLuaCode(obj);
                    builder.AppendLine(code);
                }
                builder.AppendLine("end");


                //打开面板方法开始
                builder.AppendLine("function " + panelNameText + ":openPanel()");
                builder.AppendLine("\tLuaWinMgr.showPanel(self.panelName)");
                builder.AppendLine("end");

                //关闭面板方法开始
                builder.AppendLine("function " + panelNameText + ":onClose()");
                builder.AppendLine("\tLuaWinMgr.closePanel(self.panelName)");
                builder.AppendLine("end");

                string panelPath = luaModulePath + nameInfo.LowModuleName + "/view/" + panelNameText + ".text";
                writeFile(panelPath, builder.ToString());

                //windowDef
                string newWindowDefPath = WindowDefPath.Replace(".text", ".lua");
                File.Move(WindowDefPath, newWindowDefPath);
                string _sb = File.ReadAllText(newWindowDefPath);
                string _context = panelNameText + "=\"" + obj.name + "\"";
                if (!_sb.Contains(_context))
                {
                    StreamWriter fs = File.AppendText(newWindowDefPath);
                    fs.Write("\r\n" + _context);
                    fs.Close();
                    fs.Dispose();
                    File.Move(newWindowDefPath, WindowDefPath);
                }
                else
                    Debug.Log("ModulePanel中已经存在该panel");


                //ModuleFactory
                string newModuleFactoryPath = ModuleFactoryPath.Replace(".text", ".lua");
                File.Move(ModuleFactoryPath, newModuleFactoryPath);
                string sb = File.ReadAllText(newModuleFactoryPath);
                string context = "'modules." + nameInfo.LowModuleName + "." + nameInfo.UpModuleName + "Module',";
                if (!sb.Contains(context))
                {
                    File.WriteAllText(newModuleFactoryPath, string.Empty);
                    sb = sb.Insert(sb.IndexOf("}"), "\r\t" + context + "\r");
                    writeFile(ModuleFactoryPath, sb);
                }
                else
                    Debug.Log("ModuleFactory中已经注册了该module");

                itemModuleName = nameInfo.MoudleName;
                Selection.activeGameObject = null;
                Debug.Log("模块创建完成");

            }


            Process.Start(luaModulePath + nameInfo.LowModuleName);




        }

        itemModuleName = EditorGUILayout.TextField("module名字:", itemModuleName);
        if (GUILayout.Button(new GUIContent("创建Item"), GUILayout.Width(120)))
        {
            NameInfo ni = nameInfo;
            if (itemModuleName != "")
            {
                ni = new NameInfo(itemModuleName);
            }
            itemObj = Selection.activeGameObject;
            if (ni == null)
            {
                ShowMsg("请先输入module名字");
                return;
            }
            if (itemObj == null)
            {
                ShowMsg("请先选择item");
                return;
            }
            if (!Directory.Exists(luaModulePath + ni.LowModuleName + "/" + "view"))
            {
                ShowMsg("该module不存在");
                return;
            }
            if (itemObj != null)
            {
                string UpItemName = itemObj.name.Substring(0, 1).ToUpper() + itemObj.name.Substring(1);
                //string LowItemName = itemObj.name.Substring(0, 1).ToLower() + itemObj.name.Substring(1);
                StringBuilder stb = new StringBuilder();
                stb.AppendLine("module('" + ni.UpModuleName + "Module',package.seeall)");
                stb.AppendLine(UpItemName + "=class(\"" + UpItemName + "\",BaseUnit)");
                stb.AppendLine("function " + UpItemName + ":setObj(obj)\n\tself.gameObject = obj\n\tself.transform = self.gameObject.transform\n\tself:initUI()\nend");
                stb.AppendLine("function " + UpItemName + ":initUI()");
                string code = GenPrefabComponentsLuaCode(itemObj);
                stb.AppendLine(code);
                stb.AppendLine("end");
                stb.AppendLine("\nfunction " + UpItemName + ":setData(data)");
                stb.AppendLine("\n\n\nend");
                stb.AppendLine("\nfunction " + UpItemName + ":updateStatu(data)");
                stb.AppendLine("\n\n\nend");
                string itemPath = luaModulePath + ni.LowModuleName + "/view/" + UpItemName + ".text";
                writeFile(itemPath, stb.ToString());
                //修改module
                string temp = luaModulePath + ni.LowModuleName + "/" + ni.UpModuleName + "Module.lua";
                string newItemPath = temp.Replace(".text", ".lua");
                File.Move(temp, newItemPath);
                string sb = File.ReadAllText(newItemPath);
                string context = "require(\"modules." + ni.LowModuleName + ".view." + UpItemName + "\")";
                if (!sb.Contains(context))
                {
                    File.WriteAllText(newItemPath, string.Empty);
                    sb = sb.Insert(sb.IndexOf(")") + 1, "\n" + context);
                    writeFile(temp, sb);
                }
                else
                    Debug.Log("module中已经加载了该item");

                Debug.Log("item创建完成");
                Process.Start(luaModulePath + ni.LowModuleName + "/view");
            }
        }
        EditorGUILayout.LabelField("1.点击预设后，再选择创建模块");
        EditorGUILayout.LabelField("2.点击预设后，再选择创建item");
        EditorGUILayout.LabelField("3.需要获取的组件要以特定的后缀命名，Btn，Txt，Sld，Img，Tsf，Tgl");
        EditorGUILayout.EndVertical();


    }

    /// <summary>
    /// 获取对应gameObject下所有对应的需要的component,并且生成lua代码
    /// </summary>
    /// <param name="go"></param>
    string GenPrefabComponentsLuaCode(GameObject go)
    {
        ComDic = new Dictionary<string, Type>()
        {
         {"Btn",typeof(ZButton)},
         {"Txt",typeof(Text)},
         {"Tgl",typeof(Toggle)},
         {"Sld",typeof(Slider)},
         {"Img",typeof(Image)},
         {"Tsf",typeof(Transform)},
        };
        List<Component> btns = ArrayToList(go.GetComponentsInChildren<ZButton>(true));
        List<Component> texts = ArrayToList(go.GetComponentsInChildren<Text>(true));
        List<Component> imgs = ArrayToList(go.GetComponentsInChildren<Image>(true));
        List<Component> tgl = ArrayToList(go.GetComponentsInChildren<Toggle>(true));
        List<Component> sld = ArrayToList(go.GetComponentsInChildren<Slider>(true));
        List<Component> tsf = ArrayToList(go.GetComponentsInChildren<Transform>(true));
        StringBuilder stb = new StringBuilder();

        stb.AppendLine(GetLuaCodeByType(ref btns, go.transform, "ZButton"));
        stb.AppendLine(GetLuaCodeByType(ref texts, go.transform, "Text"));
        stb.AppendLine(GetLuaCodeByType(ref imgs, go.transform, "Image"));
        stb.AppendLine(GetLuaCodeByType(ref tgl, go.transform, "Toggle"));
        stb.AppendLine(GetLuaCodeByType(ref sld, go.transform, "Slider"));
        stb.AppendLine(GetLuaCodeByType(ref tsf, go.transform, "Transform"));

        if (tgl.Count > 0)
        {
            string s = "\tself.tgls={";
            for (int i = 0; i < tgl.Count; i++)
            {
                s += "[" + (i + 1).ToString() + "]=self." + tgl[i].name.Substring(0, 1).ToLower() + tgl[i].name.Substring(1) + ",";
            }
            s = s.Substring(0, s.Length - 1);
            s += "}";
            stb.AppendLine(s);
            stb.AppendLine("\tfor i=1,#self.tgls do");
            stb.AppendLine("\tself.tgls[i].onValueChanged:AddListener(function ()");
            stb.AppendLine("\t\tSoundMgr.Instance:PlayAudioClipAsync(soundDef.Click)");
            stb.AppendLine("\r\n\t\tend)");
            stb.AppendLine("\r\n\t\tend");
        }
        //添加按钮监听事件的代码
        foreach (var btn in btns)
        {
            stb.AppendLine("\tself." + btn.name.Substring(0, 1).ToLower() + btn.name.Substring(1) + ".onClick:AddListener(function ()");
            //stb.AppendLine("\t\tSoundMgr.Instance:PlayAudioClipAsync(soundDef.Click)");
            stb.AppendLine("\r\n\tend)");

        }


        return stb.ToString();
    }

    string GetLuaCodeByType(ref List<Component> cpmLs, Transform parent, string componentType)
    {
        StringBuilder stb = new StringBuilder();
        stb.AppendLine("\t--生成" + componentType + "代码");
        DesNeedLess(ref cpmLs);
        foreach (var cpm in cpmLs)
        {
            if (cpm.transform == parent)
            {
                stb.AppendLine("\tself." + cpm.name.Substring(0, 1).ToLower() + cpm.name.Substring(1) + " = self:getComponent(\"" + cpm.name + "\",\"" + componentType + "\")");
            }
            else
            {
                string content = " = self:getChildComponent(\"" + GetPathFromTo(parent, cpm.transform) + "\",\"" + componentType + "\")";
                if (componentType == "Transform")
                {
                    content = " = self:getChildByName(\"" + GetPathFromTo(parent, cpm.transform) + "\")";
                }
                stb.AppendLine("\tself." + cpm.name.Substring(0, 1).ToLower() + cpm.name.Substring(1) + content);
            }
        }

        return stb.ToString();
    }
    string GetPathFromTo(Transform root, Transform t)
    {
        string componentPath = t.name;
        while (t.parent != root)
        {
            componentPath = t.parent.name + "/" + componentPath;
            t = t.parent;
        }
        return componentPath;
    }


    void writeFile(string path, string content)
    {
        FileStream fs = new FileStream(path, FileMode.Append, FileAccess.Write);
        StreamWriter sr = new StreamWriter(fs);
        sr.WriteLine(content);//开始写入值
        sr.Close(); sr.Dispose();
        fs.Close(); fs.Dispose();
        File.Move(path, path.Replace(".text", ".lua"));
    }

    void DesNeedLess(ref List<Component> coms)
    {
        List<Component> des = new List<Component>();
        foreach (var com in coms)
        {
            if (com.gameObject.name.Length < 3)
            {
                des.Add(com);
                continue;
            }
            string x = com.gameObject.name.Substring(com.gameObject.name.Length - 3, 3);
            Type t;
            ComDic.TryGetValue(x, out t);
            if (t != com.GetType())
            //if (x != "Btn" && x != "Tgl" && x != "Img" && x != "Tsf" && x != "Sld" && x != "Txt")
            {
                if (t != typeof(Transform))
                {
                    des.Add(com);
                }

            }
        }
        if (des.Count > 0)
        {
            foreach (var a in des)
            {
                coms.Remove(a);
            }
        }

    }
    List<Component> ArrayToList(Component[] coms)
    {
        List<Component> lists = new List<Component>();
        foreach (var s in coms)
        {
            lists.Add(s);
        }
        return lists;

    }
    public class NameInfo
    {
        public string MoudleName;
        public string UpModuleName;
        public string LowModuleName;
        public string FileName;
        public NameInfo(string name)
        {
            MoudleName = name;
            FileName = MoudleName.Replace("Module", "");
            UpModuleName = FileName.Substring(0, 1).ToUpper() + FileName.Substring(1);
            LowModuleName = UpModuleName.Substring(0, 1).ToLower() + UpModuleName.Substring(1);
        }
    }

    [MenuItem("GameObject/便捷工具/复制路径  #C", false, 11)]
    public static void CopyPath()
    {
        Transform target = Selection.transforms[0];
        string path = target.name;
        string temp = "";
        while (true)
        {
            if (target.parent.parent != null)
            {
                target = target.parent;
                temp = path;
                path = target.name + "/" + temp;
            }
            else
                break;
        }
        path = path.Replace(target.name + "/", "");
        GUIUtility.systemCopyBuffer = path.ToString();
        Debug.Log("路径已经复制:" + path);

    }
    [MenuItem("GameObject/便捷工具/创建AB预设文件夹", false, 11)]
    public static void CreatFile()
    {
        Transform target = Selection.transforms[0];
        string name = target.name.Replace("Panel", "");
        string path = GameSetting.ProjectRoot + "/Assets/AB/" + name + "_UI";
        if (Directory.Exists(path))
        {
            EditorUtility.DisplayDialog("提示", "文件目录已存在，创建取消", "确定");
            return;
        }
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(path + "/" + name + "Atlas");
        Directory.CreateDirectory(path + "/" + name + "Texture");
        DirectoryInfo dir = Directory.CreateDirectory(path + "/" + name + "Prefabs");
        try
        {
            PrefabUtility.SaveAsPrefabAssetAndConnect(target.gameObject, dir.FullName + "/" + target.name + ".prefab", InteractionMode.UserAction);
        }
        catch
        {
            EditorUtility.DisplayDialog("提示", "解除GameObject的预设绑定再尝试", "确定");
        }

        AssetDatabase.Refresh();
        VerBuild.CreateResini();
        Debug.Log("创建完成");
    }

    [MenuItem("GameObject/便捷工具/Button转ZButton", false, 11)]
    public static void ButtonToZButton()
    {
        Transform target = Selection.transforms[0];
        Button[] bt = target.GetComponentsInChildren<Button>(true);
        foreach (var a in bt)
        {
            GameObject g = a.gameObject;
            ButtonClickedEvent et = a.onClick;
            GameObject.DestroyImmediate(a);
            g.gameObject.AddComponent<ZButton>().onClick = et;
        }
        Debug.Log("OK");

    }
    [MenuItem("GameObject/便捷工具/TextPro转Text", false, 11)]
    public static void TextProToText()
    {
        Transform[] targets = Selection.transforms;
        foreach(var target in  targets)
        {
            TextMeshProUGUI[] txt = target.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var a in txt)
            {
                GameObject g = a.gameObject;
                UnityEditorInternal.ComponentUtility.CopyComponent(g.GetComponent<RectTransform>());
                GameObject.DestroyImmediate(a);
                Text txtPro = g.gameObject.AddComponent<Text>();
                UnityEditorInternal.ComponentUtility.PasteComponentValues(g.GetComponent<RectTransform>());

                Font font_ttf = AssetDatabase.LoadAssetAtPath("Assets/Ab/Font/fzcyjt.TTF", typeof(Font)) as Font;
                txtPro.font = font_ttf;

                txtPro.text = a.text;
                txtPro.fontSize = (int)a.fontSize;
                txtPro.alignment = getTextProAlig(a.alignment);
                txtPro.fontStyle = (UnityEngine.FontStyle)a.fontStyle;
                txtPro.color = a.color;
                txtPro.raycastTarget = a.raycastTarget;
                txtPro.gameObject.SetActive(false);
                txtPro.gameObject.SetActive(true);
            }
            TMP_InputField[] pf = target.GetComponentsInChildren<TMP_InputField>(true);
            foreach (var a in pf)
            {
                GameObject g = a.gameObject;
                GameObject.DestroyImmediate(a);
                InputField txtPro = g.gameObject.AddComponent<InputField>();
                txtPro.targetGraphic = a.targetGraphic;
                txtPro.characterLimit = a.characterLimit;
                txtPro.contentType = (InputField.ContentType)a.contentType;
                txtPro.lineType = (InputField.LineType)a.lineType;
                txtPro.selectionColor = a.selectionColor;
                for (int i = 0; i < g.transform.childCount; i++)
                {
                    if (g.transform.GetChild(i).gameObject.name == "Placeholder")
                    {
                        txtPro.placeholder = g.transform.GetChild(i).GetComponent<Text>();
                    }
                    if (g.transform.GetChild(i).gameObject.name == "Content")
                    {
                        txtPro.textComponent = g.transform.GetChild(i).GetComponent<Text>();
                    }
                }
            }
            Debug.Log("OK");
        }
    }

    [MenuItem("GameObject/便捷工具/Text转TextPro", false, 11)]
    public static void TextToTextPro()
    {
        Transform target = Selection.transforms[0];
        Text[] txt = target.GetComponentsInChildren<Text>(true);
        foreach (var a in txt)
        {
            GameObject g = a.gameObject;
            UnityEditorInternal.ComponentUtility.CopyComponent(g.GetComponent<RectTransform>());
            GameObject.DestroyImmediate(a);
            TextMeshProUGUI txtPro = g.gameObject.AddComponent<TextMeshProUGUI>();
            UnityEditorInternal.ComponentUtility.PasteComponentValues(g.GetComponent<RectTransform>());
            txtPro.text = a.text;
            txtPro.fontSize = a.fontSize;
            txtPro.alignment = getTextAlig(a.alignment);
            txtPro.fontStyle = (FontStyles)a.fontStyle;
            txtPro.color = a.color;
            txtPro.raycastTarget = a.raycastTarget;
        }
        InputField[] pf = target.GetComponentsInChildren<InputField>(true);
        foreach (var a in pf)
        {
            GameObject g = a.gameObject;
            GameObject.DestroyImmediate(a);
            TMP_InputField txtPro = g.gameObject.AddComponent<TMP_InputField>();
            txtPro.targetGraphic = a.targetGraphic;
            txtPro.characterLimit = a.characterLimit;
            txtPro.contentType = (TMP_InputField.ContentType)a.contentType;
            txtPro.lineType = (TMP_InputField.LineType)a.lineType;
            txtPro.selectionColor = a.selectionColor;
            for (int i = 0; i < g.transform.childCount; i++)
            {
                if (g.transform.GetChild(i).gameObject.name == "Placeholder")
                {
                    txtPro.placeholder = g.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
                }
                if (g.transform.GetChild(i).gameObject.name == "Content")
                {
                    txtPro.textComponent = g.transform.GetChild(i).GetComponent<TextMeshProUGUI>();
                }

            }
        }
        Debug.Log("OK");

    }
    //字体对齐
    public static TextAlignmentOptions getTextAlig(TextAnchor tx)
    {
        switch (tx)
        {
            case TextAnchor.LowerCenter:
                return TextAlignmentOptions.Bottom;
            case TextAnchor.LowerLeft:
                return TextAlignmentOptions.BottomLeft;
            case TextAnchor.LowerRight:
                return TextAlignmentOptions.BottomRight;
            case TextAnchor.MiddleCenter:
                return TextAlignmentOptions.Center;
            case TextAnchor.MiddleLeft:
                return TextAlignmentOptions.Left;
            case TextAnchor.MiddleRight:
                return TextAlignmentOptions.Right;
            case TextAnchor.UpperCenter:
                return TextAlignmentOptions.Top;
            case TextAnchor.UpperLeft:
                return TextAlignmentOptions.TopLeft;
            case TextAnchor.UpperRight:
                return TextAlignmentOptions.TopRight;
            default:
                return TextAlignmentOptions.TopLeft;
        }
    }

    //字体对齐
    public static TextAnchor getTextProAlig(TextAlignmentOptions tx)
    {
        switch (tx)
        {
            case TextAlignmentOptions.Bottom:            
                return TextAnchor.LowerCenter;
            case TextAlignmentOptions.BottomLeft:            
                return TextAnchor.LowerLeft;
            case TextAlignmentOptions.BottomRight:               
                return TextAnchor.LowerRight;
            case TextAlignmentOptions.Center:             
                return TextAnchor.MiddleCenter;
            case TextAlignmentOptions.Left:              
                return TextAnchor.MiddleLeft;
            case TextAlignmentOptions.Right:              
                return TextAnchor.MiddleRight;
            case TextAlignmentOptions.Top:            
                return TextAnchor.UpperCenter;
            case TextAlignmentOptions.TopLeft:            
                return TextAnchor.UpperLeft;
            case TextAlignmentOptions.TopRight:
                return TextAnchor.UpperRight;
            default:
                return TextAnchor.UpperLeft;
        }
    }
}




