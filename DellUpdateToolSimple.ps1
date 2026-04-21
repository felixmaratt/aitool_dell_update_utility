Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

function Get-AppDirectory {
    if ($PSScriptRoot) { return $PSScriptRoot }
    if ($MyInvocation -and $MyInvocation.MyCommand -and $MyInvocation.MyCommand.Path) {
        return (Split-Path -Parent $MyInvocation.MyCommand.Path)
    }
    if ($PSCommandPath) { return (Split-Path -Parent $PSCommandPath) }
    if ([System.AppDomain]::CurrentDomain.BaseDirectory) {
        return [System.AppDomain]::CurrentDomain.BaseDirectory
    }
    return (Get-Location).Path
}

function Get-ShortcutTarget($shortcutPath) {
    try {
        if (Test-Path $shortcutPath) {
            $ws = New-Object -ComObject WScript.Shell
            $shortcut = $ws.CreateShortcut($shortcutPath)
            if ($shortcut.TargetPath -and (Test-Path $shortcut.TargetPath)) {
                return $shortcut.TargetPath
            }
        }
    } catch {}
    return $null
}

function Get-DCUPaths {
    $result = [ordered]@{
        CliPath = $null
        UiPath  = $null
    }

    $cliCandidates = @(
        "$env:ProgramFiles\Dell\CommandUpdate\dcu-cli.exe",
        "$env:ProgramFiles(x86)\Dell\CommandUpdate\dcu-cli.exe"
    )

    foreach ($p in $cliCandidates) {
        if ($p -and (Test-Path $p)) {
            $result.CliPath = $p
            break
        }
    }

    $uiCandidates = @(
        "$env:ProgramFiles\Dell\CommandUpdate\DellCommandUpdate.exe",
        "$env:ProgramFiles(x86)\Dell\CommandUpdate\DellCommandUpdate.exe"
    )

    foreach ($p in $uiCandidates) {
        if ($p -and (Test-Path $p)) {
            $result.UiPath = $p
            break
        }
    }

    if (-not $result.UiPath) {
        $shortcutCandidates = @(
            "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Dell\Command Update\Dell Command Update.lnk",
            "C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Dell\Command Update\Command Update.lnk"
        )

        foreach ($s in $shortcutCandidates) {
            $target = Get-ShortcutTarget $s
            if ($target) {
                $result.UiPath = $target
                break
            }
        }
    }

    if (-not $result.CliPath -and $result.UiPath) {
        $parentDir = Split-Path -Parent $result.UiPath
        if ($parentDir) {
            $possibleCli = Join-Path $parentDir "dcu-cli.exe"
            if (Test-Path $possibleCli) {
                $result.CliPath = $possibleCli
            }
        }
    }

    return [PSCustomObject]$result
}

$appDir = Get-AppDirectory
$dcu = Get-DCUPaths
$hasAny = ($dcu.CliPath -or $dcu.UiPath)

$form = New-Object System.Windows.Forms.Form
$form.Text = "Dell Update Tool"
$form.Size = New-Object System.Drawing.Size(700, 520)
$form.StartPosition = "CenterScreen"
$form.FormBorderStyle = "FixedDialog"
$form.MaximizeBox = $false
$form.BackColor = [System.Drawing.Color]::White

$headerPanel = New-Object System.Windows.Forms.Panel
$headerPanel.Size = New-Object System.Drawing.Size(700, 120)
$headerPanel.Location = New-Object System.Drawing.Point(0, 0)
$headerPanel.BackColor = [System.Drawing.Color]::FromArgb(0, 51, 102)
$form.Controls.Add($headerPanel)

$uniLabel = New-Object System.Windows.Forms.Label
$uniLabel.Text = "The University of Auckland | Waipapa Taumata Rau"
$uniLabel.Font = New-Object System.Drawing.Font("Segoe UI", 16, [System.Drawing.FontStyle]::Bold)
$uniLabel.AutoSize = $false
$uniLabel.Size = New-Object System.Drawing.Size(640, 40)
$uniLabel.Location = New-Object System.Drawing.Point(30, 20)
$uniLabel.TextAlign = "MiddleCenter"
$uniLabel.ForeColor = [System.Drawing.Color]::White
$headerPanel.Controls.Add($uniLabel)

$subHeader = New-Object System.Windows.Forms.Label
$subHeader.Text = "Dell Command Update Launcher"
$subHeader.Font = New-Object System.Drawing.Font("Segoe UI", 11, [System.Drawing.FontStyle]::Regular)
$subHeader.AutoSize = $false
$subHeader.Size = New-Object System.Drawing.Size(640, 30)
$subHeader.Location = New-Object System.Drawing.Point(30, 65)
$subHeader.TextAlign = "MiddleCenter"
$subHeader.ForeColor = [System.Drawing.Color]::White
$headerPanel.Controls.Add($subHeader)

$appTitle = New-Object System.Windows.Forms.Label
$appTitle.Text = "System Update Utility"
$appTitle.Font = New-Object System.Drawing.Font("Segoe UI", 16, [System.Drawing.FontStyle]::Bold)
$appTitle.AutoSize = $true
$appTitle.ForeColor = [System.Drawing.Color]::Black
$appTitle.Location = New-Object System.Drawing.Point(235, 145)
$form.Controls.Add($appTitle)

$status = New-Object System.Windows.Forms.Label
$status.AutoSize = $false
$status.Size = New-Object System.Drawing.Size(520, 50)
$status.Location = New-Object System.Drawing.Point(90, 190)
$status.TextAlign = "MiddleCenter"
$status.Font = New-Object System.Drawing.Font("Segoe UI", 11)

if ($hasAny) {
    if ($dcu.CliPath) {
        $status.Text = "Dell Command Update found. CLI ready for scan/install."
    } else {
        $status.Text = "Dell Command Update UI found. CLI not found, UI mode only."
    }
} else {
    $status.Text = "Dell Command Update was not found on this PC."
}
$form.Controls.Add($status)

$btnScan = New-Object System.Windows.Forms.Button
$btnScan.Text = "Check for Updates"
$btnScan.Size = New-Object System.Drawing.Size(220, 45)
$btnScan.Location = New-Object System.Drawing.Point(90, 255)
$btnScan.Font = New-Object System.Drawing.Font("Segoe UI", 11)
$form.Controls.Add($btnScan)

$btnApply = New-Object System.Windows.Forms.Button
$btnApply.Text = "Install Updates"
$btnApply.Size = New-Object System.Drawing.Size(220, 45)
$btnApply.Location = New-Object System.Drawing.Point(370, 255)
$btnApply.Font = New-Object System.Drawing.Font("Segoe UI", 11)
$form.Controls.Add($btnApply)

$btnOpen = New-Object System.Windows.Forms.Button
$btnOpen.Text = "Open Dell Update"
$btnOpen.Size = New-Object System.Drawing.Size(220, 40)
$btnOpen.Location = New-Object System.Drawing.Point(230, 320)
$btnOpen.Font = New-Object System.Drawing.Font("Segoe UI", 10)
$form.Controls.Add($btnOpen)

$btnScan.Enabled = [bool]$dcu.CliPath
$btnApply.Enabled = [bool]$dcu.CliPath
$btnOpen.Enabled = [bool]($dcu.UiPath -or $dcu.CliPath)

$btnScan.Add_Click({
    try {
        Start-Process -FilePath $dcu.CliPath -ArgumentList "/scan" -Wait
        [System.Windows.Forms.MessageBox]::Show("Update scan finished.", "Done")
    } catch {
        [System.Windows.Forms.MessageBox]::Show("Could not run update scan.`n`n$($_.Exception.Message)", "Error")
    }
})

$btnApply.Add_Click({
    try {
        Start-Process -FilePath $dcu.CliPath -ArgumentList "/applyUpdates -silent" -Wait
        [System.Windows.Forms.MessageBox]::Show("Install updates command finished.", "Done")
    } catch {
        [System.Windows.Forms.MessageBox]::Show("Update install may require Administrator privileges.`n`nTry running this app as Administrator.", "Permission Notice")
    }
})

$btnOpen.Add_Click({
    try {
        if ($dcu.UiPath) {
            Start-Process -FilePath $dcu.UiPath
        } elseif ($dcu.CliPath) {
            Start-Process -FilePath $dcu.CliPath
        } else {
            [System.Windows.Forms.MessageBox]::Show("Dell Command Update not found.", "Error")
        }
    } catch {
        [System.Windows.Forms.MessageBox]::Show("Could not open Dell Command Update.`n`n$($_.Exception.Message)", "Error")
    }
})

$footerName = New-Object System.Windows.Forms.Label
$footerName.Text = "Laxman Sunkari"
$footerName.Font = New-Object System.Drawing.Font("Segoe UI", 12, [System.Drawing.FontStyle]::Bold)
$footerName.AutoSize = $true
$footerName.Location = New-Object System.Drawing.Point(265, 390)
$form.Controls.Add($footerName)

$footerEmail = New-Object System.Windows.Forms.Label
$footerEmail.Text = "l.sunkari@auckland.ac.nz"
$footerEmail.Font = New-Object System.Drawing.Font("Segoe UI", 10)
$footerEmail.AutoSize = $true
$footerEmail.ForeColor = [System.Drawing.Color]::Blue
$footerEmail.Location = New-Object System.Drawing.Point(240, 420)
$form.Controls.Add($footerEmail)

[void]$form.ShowDialog()