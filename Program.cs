using System.Diagnostics;
using System.Drawing;
using System.Security.Principal;
using System.Windows.Forms;

namespace DellUpdateToolCSharp;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

public sealed class MainForm : Form
{
    private readonly Label _statusLabel;
    private readonly Button _scanButton;
    private readonly Button _applyButton;
    private readonly Button _openButton;

    private readonly string? _cliPath;
    private readonly string? _uiPath;

    public MainForm()
    {
        Text = "Dell Update Tool";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        ClientSize = new Size(700, 520);
        BackColor = Color.White;

        (_cliPath, _uiPath) = FindDellCommandUpdatePaths();
        bool hasAny = !string.IsNullOrWhiteSpace(_cliPath) || !string.IsNullOrWhiteSpace(_uiPath);

        var headerPanel = new Panel
        {
            Size = new Size(700, 120),
            Location = new Point(0, 0),
            BackColor = Color.FromArgb(0, 51, 102)
        };
        Controls.Add(headerPanel);

        var uniLabel = new Label
        {
            Text = "The University of Auckland | Waipapa Taumata Rau",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            AutoSize = false,
            Size = new Size(640, 40),
            Location = new Point(30, 20),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.White
        };
        headerPanel.Controls.Add(uniLabel);

        var subHeader = new Label
        {
            Text = "Dell Command Update Launcher",
            Font = new Font("Segoe UI", 11, FontStyle.Regular),
            AutoSize = false,
            Size = new Size(640, 30),
            Location = new Point(30, 65),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.White
        };
        headerPanel.Controls.Add(subHeader);

        var appTitle = new Label
        {
            Text = "System Update Utility",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            AutoSize = true,
            ForeColor = Color.Black,
            Location = new Point(235, 145)
        };
        Controls.Add(appTitle);

        _statusLabel = new Label
        {
            AutoSize = false,
            Size = new Size(520, 50),
            Location = new Point(90, 190),
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 11)
        };

        if (hasAny)
        {
            _statusLabel.Text = !string.IsNullOrWhiteSpace(_cliPath)
                ? "Dell Command Update found. CLI ready for scan/install."
                : "Dell Command Update UI found. CLI not found, UI mode only.";
        }
        else
        {
            _statusLabel.Text = "Dell Command Update was not found on this PC.";
        }
        Controls.Add(_statusLabel);

        _scanButton = new Button
        {
            Text = "Check for Updates",
            Size = new Size(220, 45),
            Location = new Point(90, 255),
            Font = new Font("Segoe UI", 11)
        };
        _scanButton.Click += (_, _) => RunDellCli("/scan", "Update scan finished.");
        Controls.Add(_scanButton);

        _applyButton = new Button
        {
            Text = "Install Updates",
            Size = new Size(220, 45),
            Location = new Point(370, 255),
            Font = new Font("Segoe UI", 11)
        };
        _applyButton.Click += (_, _) => RunDellCli("/applyUpdates -silent", "Install updates command finished.");
        Controls.Add(_applyButton);

        _openButton = new Button
        {
            Text = "Open Dell Update",
            Size = new Size(220, 40),
            Location = new Point(230, 320),
            Font = new Font("Segoe UI", 10)
        };
        _openButton.Click += (_, _) => OpenDellUpdate();
        Controls.Add(_openButton);

        _scanButton.Enabled = !string.IsNullOrWhiteSpace(_cliPath);
        _applyButton.Enabled = !string.IsNullOrWhiteSpace(_cliPath);
        _openButton.Enabled = !string.IsNullOrWhiteSpace(_uiPath) || !string.IsNullOrWhiteSpace(_cliPath);

        var footerName = new Label
        {
            Text = "Laxman Sunkari",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(265, 390)
        };
        Controls.Add(footerName);

        var footerEmail = new LinkLabel
        {
            Text = "l.sunkari@auckland.ac.nz",
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            LinkColor = Color.Blue,
            Location = new Point(240, 420)
        };
        footerEmail.Click += (_, _) =>
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "mailto:l.sunkari@auckland.ac.nz",
                    UseShellExecute = true
                });
            }
            catch
            {
                // ignore
            }
        };
        Controls.Add(footerEmail);
    }

    private void RunDellCli(string arguments, string successMessage)
    {
        if (string.IsNullOrWhiteSpace(_cliPath))
        {
            MessageBox.Show("Dell Command Update CLI not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _cliPath,
                Arguments = arguments,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(_cliPath)
            };

            using var process = Process.Start(startInfo);
            process?.WaitForExit();

            MessageBox.Show(successMessage, "Done", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "The action could not be completed.\n\nThis may require Administrator privileges.\n\n" + ex.Message,
                "Permission Notice",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void OpenDellUpdate()
    {
        var pathToOpen = !string.IsNullOrWhiteSpace(_uiPath) ? _uiPath : _cliPath;

        if (string.IsNullOrWhiteSpace(pathToOpen))
        {
            MessageBox.Show("Dell Command Update not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = pathToOpen,
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(pathToOpen)
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Could not open Dell Command Update.\n\n" + ex.Message,
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static (string? cliPath, string? uiPath) FindDellCommandUpdatePaths()
    {
        string? cliPath = null;
        string? uiPath = null;

        var cliCandidates = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Dell", "CommandUpdate", "dcu-cli.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Dell", "CommandUpdate", "dcu-cli.exe")
        };

        foreach (var path in cliCandidates)
        {
            if (File.Exists(path))
            {
                cliPath = path;
                break;
            }
        }

        var uiCandidates = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Dell", "CommandUpdate", "DellCommandUpdate.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Dell", "CommandUpdate", "DellCommandUpdate.exe")
        };

        foreach (var path in uiCandidates)
        {
            if (File.Exists(path))
            {
                uiPath = path;
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(uiPath))
        {
            var shortcutCandidates = new[]
            {
                @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Dell\Command Update\Dell Command Update.lnk",
                @"C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Dell\Command Update\Command Update.lnk"
            };

            foreach (var shortcutPath in shortcutCandidates)
            {
                var target = ResolveShortcutTarget(shortcutPath);
                if (!string.IsNullOrWhiteSpace(target) && File.Exists(target))
                {
                    uiPath = target;
                    break;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(cliPath) && !string.IsNullOrWhiteSpace(uiPath))
        {
            var parent = Path.GetDirectoryName(uiPath);
            if (!string.IsNullOrWhiteSpace(parent))
            {
                var possibleCli = Path.Combine(parent, "dcu-cli.exe");
                if (File.Exists(possibleCli))
                {
                    cliPath = possibleCli;
                }
            }
        }

        return (cliPath, uiPath);
    }

    private static string? ResolveShortcutTarget(string shortcutPath)
    {
        try
        {
            if (!File.Exists(shortcutPath))
                return null;

            Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
            if (shellType is null)
                return null;

            dynamic shell = Activator.CreateInstance(shellType)!;
            dynamic shortcut = shell.CreateShortcut(shortcutPath);
            string targetPath = shortcut.TargetPath as string;

            return string.IsNullOrWhiteSpace(targetPath) ? null : targetPath;
        }
        catch
        {
            return null;
        }
    }
}
