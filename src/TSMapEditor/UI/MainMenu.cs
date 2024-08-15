using Microsoft.Xna.Framework;
using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.IO;
using TSMapEditor.Settings;
using TSMapEditor.Resources;
using TSMapEditor.UI.Controls;
using TSMapEditor.UI.Windows;
using TSMapEditor.UI.Windows.MainMenuWindows;
using MessageBoxButtons = TSMapEditor.UI.Windows.MessageBoxButtons;

#if WINDOWS
using System.Windows.Forms;
using Microsoft.Win32;
#endif

namespace TSMapEditor.UI
{
    public class MainMenu : EditorPanel
    {
        private const string DirectoryPrefix = "<DIR> ";
        private const int BrowseButtonWidth = 70;

        public MainMenu(WindowManager windowManager) : base(windowManager)
        {
        }

        private string gameDirectory;

        private EditorTextBox tbGameDirectory;
        private EditorButton btnBrowseGameDirectory;
        private EditorTextBox tbMapPath;
        private EditorButton btnBrowseMapPath;
        private EditorButton btnLoad;
        private FileBrowserListBox lbFileList;

        private SettingsPanel settingsPanel;

        private int loadingStage;

        public override void Initialize()
        {
            Name = nameof(MainMenu);
            Width = 570;

            var lblGameDirectory = new XNALabel(WindowManager);
            lblGameDirectory.Name = "lblGameDirectory";
            lblGameDirectory.X = Constants.UIEmptySideSpace;
            lblGameDirectory.Y = Constants.UIEmptyTopSpace;
            string gameDirectoryText = stringtrans.ResourceManager.GetString("GameDirectoryText");
            lblGameDirectory.Text = gameDirectoryText;
            AddChild(lblGameDirectory);

            tbGameDirectory = new EditorTextBox(WindowManager);
            tbGameDirectory.Name = "tbGameDirectory";
            tbGameDirectory.AllowSemicolon = true;
            tbGameDirectory.X = Constants.UIEmptySideSpace;
            tbGameDirectory.Y = lblGameDirectory.Bottom + Constants.UIVerticalSpacing;
            tbGameDirectory.Width = Width - Constants.UIEmptySideSpace * 3 - BrowseButtonWidth;
            tbGameDirectory.Text = UserSettings.Instance.GameDirectory;
            if (string.IsNullOrWhiteSpace(tbGameDirectory.Text))
            {
                ReadGameInstallDirectoryFromRegistry();
            }

#if DEBUG
            // 当调试时，可能经常在不同的配置之间切换 - 使其更方便一些
            string expectedPath = Path.Combine(tbGameDirectory.Text, Constants.ExpectedClientExecutableName);
            if (!File.Exists(expectedPath))
            {
                ReadGameInstallDirectoryFromRegistry();
            }
#endif

            tbGameDirectory.TextChanged += TbGameDirectory_TextChanged;
            AddChild(tbGameDirectory);

            btnBrowseGameDirectory = new EditorButton(WindowManager);
            btnBrowseGameDirectory.Name = "btnBrowseGameDirectory";
            btnBrowseGameDirectory.Width = BrowseButtonWidth;
            btnBrowseGameDirectory.Text = stringtrans.ResourceManager.GetString("BrowseButtonText");
            btnBrowseGameDirectory.Y = tbGameDirectory.Y;
            btnBrowseGameDirectory.X = tbGameDirectory.Right + Constants.UIEmptySideSpace;
            btnBrowseGameDirectory.Height = tbGameDirectory.Height;
            AddChild(btnBrowseGameDirectory);
            btnBrowseGameDirectory.LeftClick += BtnBrowseGameDirectory_LeftClick;

            var lblMapPath = new XNALabel(WindowManager);
            lblMapPath.Name = "lblMapPath";
            lblMapPath.X = Constants.UIEmptySideSpace;
            lblMapPath.Y = tbGameDirectory.Bottom + Constants.UIEmptyTopSpace;
            lblMapPath.Text = stringtrans.ResourceManager.GetString("MapPathLabelText");
            AddChild(lblMapPath);

            tbMapPath = new EditorTextBox(WindowManager);
            tbMapPath.Name = "tbMapPath";
            tbMapPath.AllowSemicolon = true;
            tbMapPath.X = Constants.UIEmptySideSpace;
            tbMapPath.Y = lblMapPath.Bottom + Constants.UIVerticalSpacing;
            tbMapPath.Width = Width - Constants.UIEmptySideSpace * 3 - BrowseButtonWidth;
            tbMapPath.Text = UserSettings.Instance.LastScenarioPath;
            AddChild(tbMapPath);

            btnBrowseMapPath = new EditorButton(WindowManager);
            btnBrowseMapPath.Name = "btnBrowseMapPath";
            btnBrowseMapPath.Width = BrowseButtonWidth;
            btnBrowseMapPath.Text = stringtrans.ResourceManager.GetString("BrowseButtonText");
            btnBrowseMapPath.Y = tbMapPath.Y;
            btnBrowseMapPath.X = tbMapPath.Right + Constants.UIEmptySideSpace;
            btnBrowseMapPath.Height = tbMapPath.Height;
            AddChild(btnBrowseMapPath);
            btnBrowseMapPath.LeftClick += BtnBrowseMapPath_LeftClick;

            var lblDirectoryListing = new XNALabel(WindowManager);
            lblDirectoryListing.Name = "lblDirectoryListing";
            lblDirectoryListing.X = Constants.UIEmptySideSpace;
            lblDirectoryListing.Y = tbMapPath.Bottom + Constants.UIVerticalSpacing * 2;
            lblDirectoryListing.Text = stringtrans.ResourceManager.GetString("DirectoryListingText");
            AddChild(lblDirectoryListing);

            lbFileList = new FileBrowserListBox(WindowManager);
            lbFileList.Name = "lbFileList";
            lbFileList.X = Constants.UIEmptySideSpace;
            lbFileList.Y = lblDirectoryListing.Bottom + Constants.UIVerticalSpacing;
            lbFileList.Width = Width - Constants.UIEmptySideSpace * 2;
            lbFileList.Height = 420;
            lbFileList.FileSelected += LbFileList_FileSelected;
            lbFileList.FileDoubleLeftClick += LbFileList_FileDoubleLeftClick;
            AddChild(lbFileList);

            btnLoad = new EditorButton(WindowManager);
            btnLoad.Name = "btnLoad";
            btnLoad.Width = 150;
            btnLoad.Text = stringtrans.ResourceManager.GetString("LoadButtonText");
            btnLoad.Y = lbFileList.Bottom + Constants.UIEmptyTopSpace;
            btnLoad.X = lbFileList.Right - btnLoad.Width; // 确保一致性
            AddChild(btnLoad);
            btnLoad.LeftClick += BtnLoad_LeftClick;

            var btnCreateNewMap = new EditorButton(WindowManager);
            btnCreateNewMap.Name = "btnCreateNewMap";
            btnCreateNewMap.Width = 150;
            btnCreateNewMap.Text = stringtrans.ResourceManager.GetString("CreateNewMapButtonText");
            btnCreateNewMap.X = lbFileList.X;
            btnCreateNewMap.Y = btnLoad.Y;
            AddChild(btnCreateNewMap);
            btnCreateNewMap.LeftClick += BtnCreateNewMap_LeftClick;

            Height = btnLoad.Bottom + Constants.UIEmptyBottomSpace;

            settingsPanel = new SettingsPanel(WindowManager);
            settingsPanel.Name = "settingsPanel";
            settingsPanel.X = Width;
            settingsPanel.Y = Constants.UIEmptyTopSpace;
            settingsPanel.Height = Height - Constants.UIEmptyTopSpace - Constants.UIEmptyBottomSpace;
            AddChild(settingsPanel);
            Width += settingsPanel.Width + Constants.UIEmptySideSpace;

            string directoryPath = string.Empty;

            if (!string.IsNullOrWhiteSpace(tbGameDirectory.Text))
            {
                directoryPath = tbGameDirectory.Text;

                if (!string.IsNullOrWhiteSpace(tbMapPath.Text))
                {
                    if (Path.IsPathRooted(tbMapPath.Text))
                    {
                        directoryPath = Path.GetDirectoryName(tbMapPath.Text);
                    }
                    else
                    {
                        directoryPath = Path.GetDirectoryName(tbGameDirectory.Text + tbMapPath.Text);
                    }
                }

                directoryPath = directoryPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            }

            lbFileList.DirectoryPath = directoryPath;

            base.Initialize();

            if (Program.args.Length > 0 && !string.IsNullOrWhiteSpace(Program.args[0]))
            {
                if (CheckGameDirectory())
                {
                    tbMapPath.Text = Program.args[0];
                    loadingStage++;
                }
            }
        }

private void LbFileList_FileSelected(object sender, FileSelectionEventArgs e)
        {
            tbMapPath.Text = e.FilePath;
        }

        private void BtnCreateNewMap_LeftClick(object sender, EventArgs e)
        {
            if (!CheckGameDirectory())
                return;

            ApplySettings();
            WindowManager.RemoveControl(this);
            var createMapWindow = new CreateNewMapWindow(WindowManager, false);
            createMapWindow.OnCreateNewMap += CreateMapWindow_OnCreateNewMap;
            WindowManager.AddAndInitializeControl(createMapWindow);
        }

        private void CreateMapWindow_OnCreateNewMap(object sender, CreateNewMapEventArgs e)
        {
            string error = MapSetup.InitializeMap(gameDirectory, true, null, e, WindowManager);
            if (!string.IsNullOrWhiteSpace(error))
                throw new InvalidOperationException("Failed to create new map! Returned error message: " + error);

            MapSetup.LoadTheaterGraphics(WindowManager, gameDirectory);
            ((CreateNewMapWindow)sender).OnCreateNewMap -= CreateMapWindow_OnCreateNewMap;
        }

        private void ReadGameInstallDirectoryFromRegistry()
        {
            try
            {
                RegistryKey key;

                if (Constants.InstallPathAtHKLM)
                {
                    key = Registry.LocalMachine.OpenSubKey(Constants.GameRegistryInstallPath);
                }
                else
                {
                    key = Registry.CurrentUser.OpenSubKey(Constants.GameRegistryInstallPath);
                }

                object value = key.GetValue("InstallPath", string.Empty);
                if (!(value is string valueAsString))
                {
                    tbGameDirectory.Text = string.Empty;
                }
                else
                {
                    if (File.Exists(valueAsString))
                        tbGameDirectory.Text = Path.GetDirectoryName(valueAsString);
                    else
                        tbGameDirectory.Text = valueAsString;
                }

                key.Close();
            }
            catch (Exception ex)
            {
                tbGameDirectory.Text = string.Empty;
                Logger.Log("Failed to read game installation path from the Windows registry! Exception message: " + ex.Message);
            }
        }

        private void TbGameDirectory_TextChanged(object sender, EventArgs e)
        {
            lbFileList.DirectoryPath = tbGameDirectory.Text;
        }

        private void LbFileList_FileDoubleLeftClick(object sender, EventArgs e)
        {
            BtnLoad_LeftClick(this, EventArgs.Empty);
        }

        private bool CheckGameDirectory()
        {
            string[] files = Directory.GetFiles(tbGameDirectory.Text, "*Client.exe");

            bool hasClientInName = false;
            string clientExecutablePath = string.Empty;

            foreach (string file in files)
            {
                if (Path.GetFileName(file).Contains("Client"))
                {
                    hasClientInName = true;
                    clientExecutablePath = file;
                    break;
                }
            }

            if (!hasClientInName)
            {
                string title = stringtrans.ResourceManager.GetString("InvalidFileSelected_Title");
                string message = stringtrans.ResourceManager.GetString("InvalidFileSelected_Message");
                EditorMessageBox.Show(WindowManager,
                    title,
                    string.Format(message, Constants.ExpectedClientExecutableName),
                    MessageBoxButtons.OK);
                return false;
            }

            gameDirectory = tbGameDirectory.Text;
            // 确保gameDirectory以目录分隔符结尾
            if (!gameDirectory.EndsWith("/") && !gameDirectory.EndsWith("\\"))
                gameDirectory += Path.DirectorySeparatorChar;

            // 更新文本框为实际选中的客户端可执行文件路径
            tbGameDirectory.Text = clientExecutablePath;

            return true;
        }

        private void ApplySettings()
        {
            settingsPanel.ApplySettings();

            UserSettings.Instance.GameDirectory.UserDefinedValue = tbGameDirectory.Text;
            UserSettings.Instance.LastScenarioPath.UserDefinedValue = tbMapPath.Text;

            bool fullscreenWindowed = UserSettings.Instance.FullscreenWindowed.GetValue();
            bool borderless = UserSettings.Instance.Borderless.GetValue();
            if (fullscreenWindowed && !borderless)
                throw new InvalidOperationException("Borderless= cannot be set to false if FullscreenWindowed= is enabled.");

            var gameForm = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(Game.Window.Handle);

            double renderScale = UserSettings.Instance.RenderScale.GetValue();



            WindowManager.CenterControlOnScreen(this);

            _ = UserSettings.Instance.SaveSettingsAsync();
        }

        private void BtnBrowseGameDirectory_LeftClick(object sender, EventArgs e)
        {
#if WINDOWS
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = tbGameDirectory.Text;
                openFileDialog.Filter =
                    $"Game executable|{Constants.ExpectedClientExecutableName}"; // 保留原有逻辑
                openFileDialog.Filter += "|Client executable|*Client.exe"; // 添加额外的过滤器，匹配任何exe文件

                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    string fileName = Path.GetFileName(selectedFilePath);

                    // 检查选中的文件是否是预期的客户端可执行文件
                    if (Path.GetFileName(selectedFilePath).Equals(Constants.ExpectedClientExecutableName, StringComparison.OrdinalIgnoreCase))
                    {
                        tbGameDirectory.Text = Path.GetDirectoryName(selectedFilePath);
                    }
                    // 检查选中的文件是否是可执行文件且文件名中包含"Client"
                    else if (Path.GetExtension(selectedFilePath).Equals(".exe", StringComparison.OrdinalIgnoreCase) && fileName.Contains("Client"))
                    {
                        tbGameDirectory.Text = Path.GetDirectoryName(selectedFilePath);
                    }
                    else
                    {
                        // 如果用户没有选择预期的文件或文件名中不包含"Client"的可执行文件，显示错误消息
                        // 使用 ResourceManager 获取资源字符串
                        string invalidFileSelectedTitle = stringtrans.ResourceManager.GetString("InvalidFileSelectedMD_Title");
                        string invalidFileSelectedMessage = stringtrans.ResourceManager.GetString("InvalidFileSelectedMD_Message");

                        // 格式化消息文本，插入 Constants.ExpectedClientExecutableName
                        string formattedMessage = string.Format(invalidFileSelectedMessage, Constants.ExpectedClientExecutableName);

                        // 显示消息框
                        EditorMessageBox.Show(WindowManager,
                            invalidFileSelectedTitle, // 使用从资源文件获取的标题
                            formattedMessage, // 使用从资源文件获取并格式化的消息文本
                            MessageBoxButtons.OK);
                    }
                }
            }
#endif
        }

        private void BtnBrowseMapPath_LeftClick(object sender, EventArgs e)
        {
#if WINDOWS
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = tbMapPath.Text;
                openFileDialog.Filter = Constants.OpenFileDialogFilter.Replace(':', ';');
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    tbMapPath.Text = openFileDialog.FileName;
                }
            }
#endif
        }

        private void BtnLoad_LeftClick(object sender, EventArgs e)
        {
            if (!CheckGameDirectory())
                return;

            UserSettings.Instance.GameDirectory.UserDefinedValue = gameDirectory;

            string mapPath = Path.Combine(gameDirectory, tbMapPath.Text);
            if (Path.IsPathRooted(tbMapPath.Text))
                mapPath = tbMapPath.Text;

            if (!File.Exists(mapPath))
            {
                string title = stringtrans.ResourceManager.GetString("InvalidFileSelected_Title");
                string message = string.Format(stringtrans.ResourceManager.GetString("InvalidFileSelected_Message"), Constants.ExpectedClientExecutableName);

                EditorMessageBox.Show(WindowManager,
                    title,
                    message,
                    MessageBoxButtons.OK);

                return;
            }

            loadingStage = 1;
        }

        public override void Update(GameTime gameTime)
        {
            if (loadingStage == 3)
                LoadMap(tbMapPath.Text);
            else if (loadingStage == 5)
                LoadTheater();

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (loadingStage > 0)
            {
                loadingStage++;
            }
        }

        private void LoadMap(string mapPath)
        {
            string error = MapSetup.InitializeMap(gameDirectory, false, mapPath, null, WindowManager);

            if (error == null)
            {
                ApplySettings();

                // 使用 ResourceManager 获取资源字符串
                string loadingMessageBoxText = stringtrans.ResourceManager.GetString("LoadingMessageBoxText");

                // 创建消息框并设置文本
                var messageBox = new EditorMessageBox(WindowManager, "Loading", loadingMessageBoxText, MessageBoxButtons.None);

                // 创建遮罩面板并添加到父控件
                var dp = new DarkeningPanel(WindowManager);
                AddChild(dp);
                dp.AddChild(messageBox);

                return;
            }

            loadingStage = 0;
            string errorMessageTitle = stringtrans.ResourceManager.GetString("ErrorMessageTitle");
            string errorMessageLoadingFile = stringtrans.ResourceManager.GetString("ErrorMessageLoadingFile");

            // 显示消息框，将错误信息格式化为用户友好的文本
            EditorMessageBox.Show(WindowManager,
                errorMessageTitle, // 使用从资源文件获取的标题
                errorMessageLoadingFile + (error != null ? ": " + error : string.Empty), // 使用从资源文件获取的消息文本，并附加具体错误信息（如果有）
                MessageBoxButtons.OK);
        }

        private void LoadTheater()
        {
            MapSetup.LoadTheaterGraphics(WindowManager, gameDirectory);
            WindowManager.RemoveControl(this);
        }
    }
}
