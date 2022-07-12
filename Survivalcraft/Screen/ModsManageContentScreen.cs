﻿using Engine;
using Engine.Graphics;
using Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

public class ModsManageContentScreen : Screen
{
    public static string fName = "ModsManageContentScreen";

    public enum StateFilter { UninstallState, InstallState };

    public class ModItem
    {
        public string Name;
        public ModInfo ModInfo;
        public ExternalContentEntry ExternalContentEntry;
        public Subtexture Subtexture;
        public ModItem() { }
        public ModItem(string Name, ModInfo ModInfo, ExternalContentEntry ExternalContentEntry, Subtexture Subtexture)
        {
            this.Name = Name;
            this.ModInfo = ModInfo;
            this.ExternalContentEntry = ExternalContentEntry;
            this.Subtexture = Subtexture;
        }
    }

    public ListPanelWidget m_modsContentList;

    public LabelWidget m_topBarLabel;

    public LabelWidget m_modsContentLabel;

    public LabelWidget m_filterLabel;

    public ButtonWidget m_actionButton;

    public ButtonWidget m_actionButton2;

    public ButtonWidget m_changeFilterButton;

    public ButtonWidget m_upDirectoryButton;

    public StateFilter m_filter;

    public List<ModItem> m_installModList = new List<ModItem>();

    public List<ModItem> m_uninstallModList = new List<ModItem>();

    public List<ModInfo> m_installModInfo = new List<ModInfo>();

    public List<ModInfo> m_lastInstallModInfo = new List<ModInfo>();

    public int m_count;

    public bool androidSystem;

    public string m_path;

    public string m_lastPath;

    public string m_uninstallPath = "app:/ModsCache";

    public string m_installPath = "app:/Mods";

    public bool m_updatable;

    public bool m_firstEnterInstallScreen;

    public bool m_firstEnterScreen;

    public bool m_cancelScan;

    public List<string> m_scanFailPaths = new List<string>();

    public CancellableBusyDialog m_cancellableBusyDialog;

    public List<string> m_commonPathList = new List<string>();

    public string[] m_commonPaths = new string[10]
    {
        "android:/Download",
        "android:/Android/data/com.tencent.mobileqq/Tencent/QQfile_recv",
        "android:/Android/data/com.tencent.tim/Tencent/TIMfile_recv",
        "android:tencent/TIMfile_recv",
        "android:tencent/QQfile_recv",
        "android:/Quark/Download",
        "android:/BaiduNetdisk",
        "android:/UCDownloads",
        "android:/baidu/searchbox/downloads",
        "android:/SurvivalCraft2.3/Mods"
    };

    public ModsManageContentScreen()
    {
        androidSystem = Environment.CurrentDirectory == "/";
        if (androidSystem)
        {
            m_uninstallPath = m_uninstallPath.Replace("app:", "android:/SurvivalCraft2.3");
            m_installPath = m_installPath.Replace("app:", "android:/SurvivalCraft2.3");
        }
        m_updatable = true;
        XElement node = ContentManager.Get<XElement>("Screens/ModsManageContentScreen");
        LoadContents(this, node);
        m_modsContentList = Children.Find<ListPanelWidget>("ModsContentList");
        m_topBarLabel = Children.Find<LabelWidget>("TopBar.Label");
        m_modsContentLabel = Children.Find<LabelWidget>("ModsContentLabel");
        m_filterLabel = Children.Find<LabelWidget>("Filter");
        m_actionButton = Children.Find<ButtonWidget>("ActionButton");
        m_actionButton2 = Children.Find<ButtonWidget>("ActionButton2");
        m_changeFilterButton = Children.Find<ButtonWidget>("ChangeFilter");
        m_upDirectoryButton = Children.Find<ButtonWidget>("UpDirectory");
        m_topBarLabel.Text = LanguageControl.Get(fName, 1);
        m_filterLabel.Text = LanguageControl.Get(fName, 36);
        m_firstEnterScreen = false;
        m_modsContentList.ItemWidgetFactory = delegate (object item)
        {
            ModItem modItem = (ModItem)item;
            XElement node2 = ContentManager.Get<XElement>("Widgets/ExternalContentItem");
            ContainerWidget containerWidget = (ContainerWidget)Widget.LoadWidget(this, node2, null);
            string details = LanguageControl.Get(fName, 2);
            if (modItem.ExternalContentEntry.Type == ExternalContentType.Mod)
            {
                details = string.Format(LanguageControl.Get(fName, 3), modItem.ModInfo.Version, modItem.ModInfo.Author, MathUtils.Round(modItem.ExternalContentEntry.Size / 1000));
            }
            containerWidget.Children.Find<LabelWidget>("ExternalContentItem.Text").Text = modItem.Name;
            containerWidget.Children.Find<LabelWidget>("ExternalContentItem.Details").Text = details;
            RectangleWidget iconWidget = containerWidget.Children.Find<RectangleWidget>("ExternalContentItem.Icon");
            iconWidget.Subtexture = modItem.Subtexture;
            iconWidget.Size = new Vector2(50, 50);
            iconWidget.Margin = new Vector2(10, 10);
            return containerWidget;
        };
        m_modsContentList.ItemClicked += delegate (object item)
        {
            if (item != null && m_modsContentList.SelectedItem == item)
            {
                ModItem modItem = (ModItem)item;
                if (modItem.ExternalContentEntry.Type == ExternalContentType.Directory && modItem.ExternalContentEntry.Path != m_installPath)
                {
                    try
                    {
                        SetPath(modItem.ExternalContentEntry.Path);
                        UpdateListWithBusyDialog();
                    }
                    catch
                    {
                        DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 4), LanguageControl.Get(fName, 5) + "\n" + modItem.ExternalContentEntry.Path, LanguageControl.Get("Usual", "ok"), null, null));
                    }
                }
                else if (modItem.ExternalContentEntry.Type == ExternalContentType.Mod)
                {
                    if (m_filter == StateFilter.UninstallState)
                    {
                        string modDescription = LanguageControl.Get(fName, 6) + modItem.ModInfo.Description + "\n" + LanguageControl.Get(fName, 7) + modItem.ModInfo.PackageName + "，" + LanguageControl.Get(fName, 8);
                        DialogsManager.ShowDialog(null, new MessageDialog(modItem.ModInfo.Name, modDescription, LanguageControl.Get(fName, 9), LanguageControl.Get(fName, 10), delegate (MessageDialogButton result)
                        {
                            if (result == MessageDialogButton.Button1)
                            {
                                Storage.DeleteFile(modItem.ExternalContentEntry.Path);
                                UpdateListWithBusyDialog();
                            }
                        }));
                    }
                    else
                    {
                        string modDescription = LanguageControl.Get(fName, 6) + modItem.ModInfo.Description + "\n" + LanguageControl.Get(fName, 7) + modItem.ModInfo.PackageName;
                        DialogsManager.ShowDialog(null, new MessageDialog(modItem.ModInfo.Name, modDescription, "更新", "返回", delegate (MessageDialogButton result)
                        {
                            if (result == MessageDialogButton.Button1)
                            {
                                //UpdateModFromCommunity(modItem.ModInfo);
                                DialogsManager.ShowDialog(null, new MessageDialog("该功能正在开发中", null, LanguageControl.Ok, null, null));
                            }
                        }));
                    }
                }
            }
        };
    }

    public override void Enter(object[] parameters)
    {
        if (!Storage.DirectoryExists(m_uninstallPath)) Storage.CreateDirectory(m_uninstallPath);
        BusyDialog busyDialog = new BusyDialog(LanguageControl.Get(fName, 26), LanguageControl.Get(fName, 32));
        DialogsManager.ShowDialog(null, busyDialog);
        foreach (string commonPath in m_commonPaths)
        {
            if ((androidSystem && commonPath.StartsWith("android:")) || (!androidSystem && !commonPath.StartsWith("android:")))
            {
                AddCommonPath(commonPath);
            }
        }
        string commonPathsFile = Storage.CombinePaths(m_uninstallPath, "CommonPaths.txt");
        if (Storage.FileExists(commonPathsFile))
        {
            Stream stream = Storage.OpenFile(commonPathsFile, OpenFileMode.Read);
            StreamReader streamReader = new StreamReader(stream);
            string line;
            while((line = streamReader.ReadLine()) != null)
            {
                AddCommonPath(line.Replace("\n", "").Replace("\r", ""));
            }
            stream.Dispose();
        }
        if (!m_firstEnterScreen)
        {
            m_firstEnterScreen = true;
            string explanation = "";
            if (androidSystem)
            {
                bool androidDataPathEnterEnabled = true;
                string androidDataPath = "android:/Android/data";
                try
                {
                    Storage.ListFileNames(androidDataPath);
                }
                catch
                {
                    androidDataPathEnterEnabled = false;
                }
                if (!androidDataPathEnterEnabled)
                {
                    explanation += "注意：检测到你的设备不能访问/Android/data目录,无法扫描QQ下载的mod文件，建议用自带浏览器到中文社区下载mod。\n\n";
                }
            }
            explanation += "下载mod后可以进行一次全盘扫描，扫描到mod的目录会自动列入自动扫描目录。";
            if (m_commonPathList.Count > 0)
            {
                explanation += "\n\n以下为进入管理器时自动扫描的目录:";
                for (int i = 0; i < m_commonPathList.Count; i++)
                {
                    explanation += "\n" + (i + 1) + ". " + m_commonPathList[i];
                }
                explanation += "\n\n" + LanguageControl.Get(fName, 12);
            }
            DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 14), explanation, LanguageControl.Get(fName, 15), null, null));
        }
        Task.Run(delegate
        {
            FastScanModFile(false);
            SetPath(m_installPath);
            m_filter = StateFilter.InstallState;
            UpdateList();
            SetPath(m_uninstallPath);
            m_filter = StateFilter.UninstallState;
            UpdateList();
            m_updatable = true;
            m_firstEnterInstallScreen = false;
            Dispatcher.Dispatch(delegate
            {
                foreach (ModInfo modInfo in m_installModInfo)
                {
                    m_lastInstallModInfo.Add(modInfo);
                }
                if (parameters.Length > 0 && (bool)parameters[0])
                {
                    SetPath(m_installPath);
                    m_filter = StateFilter.InstallState;
                    UpdateList();
                }
                UpdateList(true);
                DialogsManager.HideDialog(busyDialog);
            });
        });
    }

    public override void Leave()
    {
        m_modsContentList.ClearItems();
        m_installModInfo.Clear();
        m_lastInstallModInfo.Clear();
        m_uninstallModList.Clear();
        m_installModList.Clear();
        m_scanFailPaths.Clear();
        if (!Storage.DirectoryExists(m_uninstallPath)) Storage.CreateDirectory(m_uninstallPath);
        string commonPathsFile = Storage.CombinePaths(m_uninstallPath, "CommonPaths.txt");
        if(m_commonPathList.Count > 0)
        {
            Stream stream = Storage.OpenFile(commonPathsFile, OpenFileMode.Create);
            StreamWriter streamWriter = new StreamWriter(stream);
            foreach (string commonPath in m_commonPathList)
            {
                streamWriter.WriteLine(commonPath);
            }
            streamWriter.Flush();
            stream.Dispose();
        }
        m_commonPathList.Clear();
    }

    public override void Update()
    {
        if (m_filter == StateFilter.InstallState)
        {
            m_upDirectoryButton.IsVisible = false;
            m_actionButton2.IsVisible = false;
        }
        else
        {
            m_upDirectoryButton.IsVisible = true;
            m_upDirectoryButton.IsEnabled = true;
            m_actionButton2.IsVisible = true;
            m_actionButton2.IsEnabled = true;
            m_actionButton2.Text = (m_path == m_uninstallPath) ? LanguageControl.Get(fName, 16) : LanguageControl.Get(fName, 17);
        }
        ModItem modItem = null;
        if (m_modsContentList.SelectedIndex.HasValue)
        {
            modItem = (m_modsContentList.Items[m_modsContentList.SelectedIndex.Value] as ModItem);
        }
        if (modItem != null && modItem.ExternalContentEntry.Type == ExternalContentType.Mod)
        {
            m_actionButton.Text = (m_filter == StateFilter.InstallState) ? LanguageControl.Get(fName, 18) : LanguageControl.Get(fName, 19);
            m_actionButton.IsEnabled = true;
        }
        else if (modItem != null && modItem.ExternalContentEntry.Type == ExternalContentType.Directory)
        {
            m_actionButton.Text = LanguageControl.Get(fName, 20);
            m_actionButton.IsEnabled = (modItem.ExternalContentEntry.Path != "android:/Android");
        }
        else
        {
            m_actionButton.Text = LanguageControl.Get(fName, 21);
            m_actionButton.IsEnabled = false;
        }

        if (m_actionButton.IsClicked)
        {
            if (modItem != null && modItem.ExternalContentEntry.Type == ExternalContentType.Mod)
            {
                string fileName = Storage.GetFileName(modItem.ExternalContentEntry.Path);
                string installPathName = Storage.CombinePaths(m_installPath, fileName);
                string uninstallPathName = modItem.ExternalContentEntry.Path;
                if (m_filter == StateFilter.InstallState)
                {
                    string modDescription = LanguageControl.Get(fName, 6) + modItem.ModInfo.Description + "\n" + LanguageControl.Get(fName, 7) + modItem.ModInfo.PackageName + "，" + LanguageControl.Get(fName, 8);
                    DialogsManager.ShowDialog(null, new MessageDialog("确定卸载该Mod?", modDescription, "确定", "取消", delegate (MessageDialogButton result)
                    {
                        if (result == MessageDialogButton.Button1)
                        {
                            try
                            {
                                Storage.DeleteFile(installPathName);
                                UpdateListWithBusyDialog();
                                m_updatable = false;
                            }
                            catch (Exception e)
                            {
                                DialogsManager.ShowDialog(null, new MessageDialog("Mod卸载失败", "发生了异常:" + e.Message, LanguageControl.Get("Usual", "ok"), null, null));
                            }
                        }
                    }));
                }
                else
                {
                    ModInfo samePackmModInfo = null;
                    foreach (ModInfo modInfo in m_installModInfo)
                    {
                        if (modInfo.PackageName == modItem.ModInfo.PackageName)
                        {
                            samePackmModInfo = modInfo;
                        }
                    }
                    if (!Storage.FileExists(installPathName) && samePackmModInfo == null)
                    {
                        Storage.CopyFile(uninstallPathName, installPathName);
                        m_installModInfo.Add(modItem.ModInfo);
                        m_installModList.Add(modItem);
                        DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 23), fileName, LanguageControl.Get("Usual", "ok"), null, null));
                    }
                    else if (samePackmModInfo != null)
                    {
                        if (samePackmModInfo.Version == modItem.ModInfo.Version)
                        {
                            DialogsManager.ShowDialog(null, new MessageDialog("已安装相同的Mod", "安装的Mod版本一致，无需重复安装", LanguageControl.Get("Usual", "ok"), null, null));
                        }
                        else
                        {
                            string tips = string.Format("当前Mod版本为{0},已安装Mod版本为{1},是否替换", modItem.ModInfo.Version, samePackmModInfo.Version);
                            DialogsManager.ShowDialog(null, new MessageDialog("已安装相同的Mod", tips, "替换", "取消", delegate (MessageDialogButton result)
                            {
                                if (result == MessageDialogButton.Button1)
                                {
                                    foreach (ModItem modItem3 in m_installModList)
                                    {
                                        if (modItem3.ModInfo.PackageName == samePackmModInfo.PackageName)
                                        {
                                            try
                                            {
                                                Storage.DeleteFile(modItem3.ExternalContentEntry.Path);
                                                Storage.CopyFile(uninstallPathName, installPathName);
                                                m_updatable = true;
                                            }
                                            catch (Exception e)
                                            {
                                                DialogsManager.ShowDialog(null, new MessageDialog("Mod替换失败", "发生了异常:" + e.Message, LanguageControl.Get("Usual", "ok"), null, null));
                                            }
                                            break;
                                        }
                                    }
                                }
                            }));
                        }
                    }
                    else if (Storage.FileExists(installPathName))
                    {
                        DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 24), fileName + "文件已存在", LanguageControl.Get("Usual", "ok"), null, null));
                    }
                }
            }
            else if (modItem != null && modItem.ExternalContentEntry.Type == ExternalContentType.Directory)
            {
                CancellableBusyDialog busyDialog = new CancellableBusyDialog(LanguageControl.Get(fName, 26), true);
                ReadyForScan(busyDialog);
                Task.Run(delegate
                {
                    string scanPath = modItem.ExternalContentEntry.Path;
                    int allCount = ScanModFile(scanPath, busyDialog);
                    DialogsManager.HideDialog(busyDialog);
                    DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 28), string.Format(LanguageControl.Get(fName, 29), allCount), LanguageControl.Get(fName, 30), LanguageControl.Get(fName, 31), delegate (MessageDialogButton result)
                    {
                        if (result == MessageDialogButton.Button1)
                        {
                            SetPath(m_uninstallPath);
                            UpdateListWithBusyDialog();
                        }
                    }));
                });
            }
        }
        if (m_actionButton2.IsClicked)
        {
            if (m_path == m_uninstallPath)
            {
                if (m_cancellableBusyDialog != null)
                {
                    DialogsManager.ShowDialog(null, m_cancellableBusyDialog);
                    return;
                }
                m_cancellableBusyDialog = new CancellableBusyDialog(LanguageControl.Get(fName, 26), true, true);
                ReadyForScan(m_cancellableBusyDialog);
                Task.Run(delegate
                {
                    string scanPath;
                    if (androidSystem)
                    {
                        scanPath = "android:";
                    }
                    else
                    {
                        string systemPath = Storage.GetSystemPath(m_path);
                        systemPath = systemPath.Replace("\\", "/");
                        int index = systemPath.IndexOf('/');
                        scanPath = "system:" + systemPath.Substring(0, index) + "/";
                    }
                    int allCount = ScanModFile(scanPath, m_cancellableBusyDialog);
                    DialogsManager.HideDialog(m_cancellableBusyDialog);
                    m_cancellableBusyDialog = null;
                    if (allCount == 0)
                    {
                        DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 4), LanguageControl.Get(fName, 33), LanguageControl.Get(fName, 34), LanguageControl.Get(fName, 10), delegate (MessageDialogButton result)
                        {
                            if (result == MessageDialogButton.Button1)
                            {
                                ScreensManager.SwitchScreen("CommunityContent", "Mod");
                            }
                        }));
                    }
                    else
                    {
                        string tips = string.Format(LanguageControl.Get(fName, 35), allCount);
                        if(m_scanFailPaths.Count > 0)
                        {
                            tips += "\n\n以下路径无法扫描:\n";
                            foreach (string p in m_scanFailPaths)
                            {
                                tips += p + "\n";
                            }
                        }
                        if (ScreensManager.CurrentScreen == this)
                        {
                            DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 28), tips, LanguageControl.Get(fName, 30), null, delegate (MessageDialogButton result)
                            {
                                SetPath(m_uninstallPath);
                                UpdateListWithBusyDialog();
                            }));
                        }
                        else
                        {
                            DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 28), tips, LanguageControl.Ok, null, null));
                        }
                    }
                });
            }
            else
            {
                SetPath(m_uninstallPath);
                UpdateListWithBusyDialog();
            }
        }
        if (m_changeFilterButton.IsClicked)
        {
            if(m_filter == StateFilter.UninstallState)
            {
                m_filter = StateFilter.InstallState;
                SetPath(m_installPath);
                if (!m_firstEnterInstallScreen)
                {
                    m_firstEnterInstallScreen = true;
                    m_updatable = true;
                }
                UpdateList(true);
            }
            else
            {
                m_filter = StateFilter.UninstallState;
                SetPath(m_uninstallPath);
                UpdateList(true);
            }
          
        }
        if (m_upDirectoryButton.IsClicked)
        {
            string directory = Storage.GetDirectoryName(m_path);
            if (m_path != "android:" && m_path != "app:")
            {
                if (directory.StartsWith("system:") && !directory.Contains("/")) directory = directory + "/";
                SetPath(directory);
                UpdateListWithBusyDialog();
            }
            else if (m_path == "app:")
            {
                string systemPath = Storage.GetSystemPath(m_path);
                systemPath = systemPath.Replace("\\", "/");
                int index = systemPath.LastIndexOf('/');
                directory = "system:" + systemPath.Substring(0, index);
                SetPath(directory);
                UpdateListWithBusyDialog();
            }
        }
        if (base.Input.Back || base.Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
        {
            if (InstallModChange())
            {
                DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 4), LanguageControl.Get(fName, 38), LanguageControl.Get(fName, 39), LanguageControl.Get(fName, 31), delegate (MessageDialogButton result)
                {
                    if (result == MessageDialogButton.Button1)
                    {
                        Environment.Exit(0);
                    }
                    if (result == MessageDialogButton.Button2)
                    {
                        ScreensManager.SwitchScreen("Content");
                    }
                }));
            }
            else
            {
                ScreensManager.SwitchScreen("Content");
            }
        }
    }

    public void UpdateListWithBusyDialog(bool fast = false)
    {
        BusyDialog busyDialog = new BusyDialog(LanguageControl.Get(fName, 43), null);
        DialogsManager.ShowDialog(null, busyDialog);
        Task.Run(delegate
        {
            UpdateList(fast);
            Dispatcher.Dispatch(delegate
            {
                UpdateList(true);
                DialogsManager.HideDialog(busyDialog);
            });
        });
    }

    public void UpdateList(bool fast = false)
    {
        m_modsContentLabel.Text = LanguageControl.Get(fName, 40) + SetPathText(m_path);
        m_filterLabel.Text = (m_filter == StateFilter.UninstallState) ? LanguageControl.Get(fName, 36) : LanguageControl.Get(fName, 37);
        if (!fast || m_updatable)
        {
            SetModItemList();
            if (fast) m_updatable = false;
        }
        m_modsContentList.ClearItems();
        if (m_filter == StateFilter.InstallState)
        {
            foreach (ModItem modItem in m_installModList)
            {
                m_modsContentList.AddItem(modItem);
            }
        }
        else
        {
            foreach (ModItem modItem in m_uninstallModList)
            {
                m_modsContentList.AddItem(modItem);
            }
        }
    }

    public void SetModItemList()
    {
        m_updatable = true;
        if (m_filter == StateFilter.InstallState)
        {
            m_installModInfo.Clear();
            m_installModList.Clear();
        }
        else
        {
            m_uninstallModList.Clear();
        }
        IEnumerable<string> fileNameList = Storage.ListFileNames(m_path);
        foreach (string fileName in fileNameList)
        {
            string extension = Storage.GetExtension(fileName);
            if (!string.IsNullOrEmpty(extension) && extension.ToLower() == ".scmod")
            {
                ModItem modItem = GetModItem(fileName, false);
                if (modItem == null || modItem.ModInfo == null || string.IsNullOrEmpty(modItem.ModInfo.PackageName) || !modItem.ModInfo.ApiVersion.Contains("1.4")) continue;
                if (m_filter == StateFilter.InstallState)
                {
                    m_installModInfo.Add(modItem.ModInfo);
                    m_installModList.Add(modItem);
                }
                else
                {
                    m_uninstallModList.Add(modItem);
                }
            }
        }
        IEnumerable<string> directoryNameList = Storage.ListDirectoryNames(m_path);
        foreach (string directoryName in directoryNameList)
        {
            ModItem modItem = GetModItem(directoryName, true);
            if (m_filter == StateFilter.InstallState)
            {
                m_installModList.Add(modItem);
            }
            else
            {
                m_uninstallModList.Add(modItem);
            }
        }
    }

    public void ReadyForScan(CancellableBusyDialog busyDialog)
    {
        m_cancelScan = false;
        m_scanFailPaths.Clear();
        m_count = 0;
        DialogsManager.ShowDialog(null, busyDialog);
        busyDialog.ShowProgressMessage = false;
        busyDialog.Progress.Cancelled += delegate
        {
            m_cancelScan = true;
        };
    }

    public int ScanModFile(string path, CancellableBusyDialog busyDialog = null)
    {
        string validPath = path;
        if (m_cancelScan) return m_count;
        try
        {
            string systemPath = Storage.GetSystemPath(path);
            if(systemPath != Storage.GetSystemPath(m_uninstallPath))
            {
                foreach (string fileName in Storage.ListFileNames(validPath))
                {
                    if (m_cancelScan) return m_count;
                    if (validPath.EndsWith("/"))
                    {
                        validPath = path.Substring(0, validPath.Length - 1);
                    }
                    if (busyDialog != null)
                    {
                        string showName = validPath;
                        if (validPath.Length > 40)
                        {
                            showName = validPath.Substring(0, 40) + "...";
                        }
                        busyDialog.SmallMessage = string.Format("已扫描到{0}个有效的MOD文件\n" + showName, m_count);
                    }
                    string extension = Storage.GetExtension(fileName);
                    if (!string.IsNullOrEmpty(extension) && extension.ToLower() == ".scmod")
                    {
                        string pathName = Storage.CombinePaths(validPath, fileName);
                        Stream stream = null;
                        ModInfo modInfo = null;
                        try
                        {
                            stream = Storage.OpenFile(pathName, OpenFileMode.Read);
                            ZipArchive zipArchive = ZipArchive.Open(stream, true);
                            foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.ReadCentralDir())
                            {
                                if (zipArchiveEntry.FilenameInZip == "modinfo.json")
                                {
                                    MemoryStream memoryStream = new MemoryStream();
                                    zipArchive.ExtractFile(zipArchiveEntry, memoryStream);
                                    memoryStream.Position = 0L;
                                    modInfo = ModsManager.DeserializeJson<ModInfo>(ModsManager.StreamToString(memoryStream));
                                    memoryStream.Dispose();
                                    break;
                                }
                            }
                            stream.Dispose();
                        }
                        catch
                        {
                        }
                        if (stream == null || modInfo == null) continue;
                        if (string.IsNullOrEmpty(modInfo.PackageName) || !modInfo.ApiVersion.Contains("1.4")) continue;
                        string uninstallPathName = Storage.CombinePaths(m_uninstallPath, fileName);
                        if (!Storage.FileExists(uninstallPathName))
                        {
                            Storage.CopyFile(pathName, uninstallPathName);
                            if (systemPath != Storage.GetSystemPath(m_installPath))
                            {
                                Storage.DeleteFile(pathName);
                            }
                            AddCommonPath(validPath);
                            m_count++;
                        }
                        if (stream != null) stream.Close();
                    }
                }
            }
            foreach (string directory in Storage.ListDirectoryNames(path))
            {
                if (m_cancelScan) return m_count;
                if (validPath.EndsWith("/"))
                {
                    validPath = path.Substring(0, validPath.Length - 1);
                }
                string subPath = Storage.CombinePaths(validPath, directory);
                ScanModFile(subPath, busyDialog);
            }
        }
        catch
        {
            m_scanFailPaths.Add(validPath);
        }
        return m_count;
    }

    public int FastScanModFile(bool showTips = true)
    {
        int allCount = 0;
        foreach (string commonPath in m_commonPathList)
        {
            if ((androidSystem && commonPath.StartsWith("android:")) || (!androidSystem && !commonPath.StartsWith("android:")))
            {
                try
                {
                    if (Storage.DirectoryExists(commonPath))
                    {
                        m_count = 0;
                        int sucesssCount = ScanModFile(commonPath);
                        allCount += sucesssCount;
                    }
                }
                catch
                {
                    if (showTips)
                    {
                        DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Get(fName, 4), string.Format(LanguageControl.Get(fName, 41), commonPath), LanguageControl.Get(fName, 15), null, null));
                    }
                }
            }
        }
        return allCount;
    }

    public ModItem GetModItem(string fileName, bool IsDirectory)
    {
        ModItem modItem = new ModItem();
        string pathName = Storage.CombinePaths(m_path, fileName);
        modItem.Name = fileName;
        modItem.Subtexture = ExternalContentManager.GetEntryTypeIcon((IsDirectory) ? ExternalContentType.Directory : ExternalContentType.Mod);
        modItem.ExternalContentEntry = new ExternalContentEntry
        {
            Type = (IsDirectory) ? ExternalContentType.Directory : ExternalContentType.Mod,
            Path = pathName,
            Size = (IsDirectory) ? 0 : Storage.GetFileSize(pathName),
            Time = Storage.GetFileLastWriteTime(pathName)
        };
        if (IsDirectory) return modItem;
        Stream stream = Storage.OpenFile(pathName, OpenFileMode.Read);
        try
        {
            ZipArchive zipArchive = ZipArchive.Open(stream, true);
            foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.ReadCentralDir())
            {
                if (zipArchiveEntry.FilenameInZip == "icon.png")
                {
                    MemoryStream memoryStream = new MemoryStream();
                    zipArchive.ExtractFile(zipArchiveEntry, memoryStream);
                    memoryStream.Position = 0L;
                    modItem.Subtexture = new Subtexture(Texture2D.Load(memoryStream), Vector2.Zero, Vector2.One);
                    memoryStream.Dispose();
                }
                else if (zipArchiveEntry.FilenameInZip == "modinfo.json")
                {
                    MemoryStream memoryStream = new MemoryStream();
                    zipArchive.ExtractFile(zipArchiveEntry, memoryStream);
                    memoryStream.Position = 0L;
                    modItem.ModInfo = ModsManager.DeserializeJson<ModInfo>(ModsManager.StreamToString(memoryStream));
                    memoryStream.Dispose();
                }
            }
        }
        catch
        {
            modItem = null;
        }
        finally
        {
            stream.Dispose();
        }
        return modItem;
    }

    public bool InstallModChange()
    {
        bool IsChange = false;
        if (m_installModInfo.Count != m_lastInstallModInfo.Count)
        {
            return true;
        }
        foreach (ModInfo modInfo in m_installModInfo)
        {
            if (!m_lastInstallModInfo.Contains(modInfo))
            {
                IsChange = true;
                break;
            }
        }
        return IsChange;
    }

    public void SetPath(string path)
    {
        path = path.Replace("\\", "/");
        if (path != m_path)
        {
            m_lastPath = m_path;
            m_path = path;
        }
    }

    public string SetPathText(string path)
    {
        string newText = Storage.GetSystemPath(path);
        string[] arPath = path.Split(new char[] { '/' });
        if (arPath.Length > 5)
        {
            newText = ".../" + arPath[arPath.Length - 3] + "/" +  arPath[arPath.Length - 2] + "/" + arPath[arPath.Length - 1];
        }
        return newText;
    }

    public void AddCommonPath(string path)
    {
        if (!m_commonPathList.Contains(path) && !string.IsNullOrEmpty(path))
        {
            m_commonPathList.Add(path);
        }
    }

    public void UpdateModFromCommunity(ModInfo modInfo)
    {
        //从社区拉取MOD并更新
        //CommunityContentScreen communityContentScreen = ScreensManager.FindScreen<CommunityContentScreen>("CommunityContent");
        //communityContentScreen.m_filter = ExternalContentType.Mod;
        //communityContentScreen.PopulateList(null);
        //foreach(var item2 in communityContentScreen.m_listPanel.Items)
        //{
        //    CommunityContentEntry communityContentEntry = item2 as CommunityContentEntry;
        //}
    }
}
