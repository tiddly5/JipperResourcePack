﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using JipperResourcePack.Installer.Resource;

namespace JipperResourcePack.Installer.Screen;

public class FinishScreen : Screen {
    public Label TitleLabel;
    public Label DescriptionLabel;
    public CheckBox CheckBox;
    public PictureBox PictureBox;
    public Button CheckLog;
    public string Folder;

    public FinishScreen(Screen screen) {
        PrevScreen = screen;
        Cancelable = false;
        Prevable = false;
        UpPanelVisible = false;
    }

    public override void OnEnter() {
        NextButton.Text = Resources.Current.Install_Finish;
        Exception exception = ((InstallScreen) PrevScreen).exception;
        TitleLabel = new Label {
            Text = exception == null ? Resources.Current.FinishScreen_Title : Resources.Current.FinishScreen_Title_Error,
            Font = new Font("Arial", 22, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(420, 50)
        };
        DescriptionLabel = new Label {
            Text = exception == null ? Resources.Current.FinishScreen_Description : Resources.Current.FinishScreen_Description_Error,
            Font = new Font("Arial", 15),
            AutoSize = true,
            Location = new Point(420, 120)
        };
        PictureBox = new PictureBox {
            Image = Image.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Installer.Resource.JipperResourceSide.png")),
            Size = new Size(403, 561)
        };
        MainPanel.Controls.Add(TitleLabel);
        MainPanel.Controls.Add(DescriptionLabel);
        MainPanel.Controls.Add(PictureBox);
        if(exception == null) {
            CheckBox = new CheckBox {
                Text = Resources.Current.FinishScreen_RunAdofai,
                Font = new Font("Arial", 15),
                AutoSize = true,
                Location = new Point(420, 240),
                Checked = true
            };
            MainPanel.Controls.Add(CheckBox);
        } else {
            CheckLog = new Button {
                Text = Resources.Current.FinishScreen_CheckLog,
                Font = new Font("Arial", 15),
                AutoSize = true,
                Location = new Point(420, 240)
            };
            CheckLog.Click += CheckLog_Click;
            MainPanel.Controls.Add(CheckLog);
        }
    }

    private void CheckLog_Click(object sender, EventArgs e) {
        string logPath;
        if(Folder == null) {
            Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp", "JipperResourcePack");
            Directory.CreateDirectory(Folder);
            try {
                Application.ApplicationExit += DeleteFolder;
                logPath = Path.Combine(Folder, "log.txt");
                StringBuilder builder = new();
                InstallScreen installScreen = (InstallScreen) PrevScreen;
                foreach(Control control in installScreen.LogPanel.Controls) builder.AppendLine(control.Text);
                builder.Append(installScreen.exception);
                File.WriteAllText(logPath, builder.ToString());
            } catch {
                DeleteFolder();
                throw;
            }
        } else logPath = Path.Combine(Folder, "log.txt");
        Process.Start(logPath);
    }

    private void DeleteFolder() {
        Directory.Delete(Folder, true);
        Application.ApplicationExit -= DeleteFolder;
    }

    private void DeleteFolder(object obj, EventArgs args) {
        DeleteFolder();
    }

    public override void OnLeave() {
        if(CheckBox is { Checked: true }) {
            if(GlobalSetting.Instance.InstallPath == Utility.GetAdofaiPath()) Process.Start("steam://rungameid/977950");
            else Process.Start(new ProcessStartInfo {
                FileName = Path.Combine(GlobalSetting.Instance.InstallPath, "A Dance of Fire and Ice.exe"),
                WorkingDirectory = GlobalSetting.Instance.InstallPath
            });
        }
        if(Folder != null) DeleteFolder();
    }
}