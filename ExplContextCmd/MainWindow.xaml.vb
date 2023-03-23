Imports System.Buffers
Imports System.Reflection
Imports System.Security
Imports System.Security.Principal
Imports Microsoft.Win32
Imports pkar.DotNetExtensions

Class MainWindow

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        FillComboPrefixes()

        If CheckAdmin() Then uiAdmin.Visibility = Visibility.Visible
    End Sub


#Region "prefiksy"

    Private Sub uiPrefixCombo_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Dim currPrefix As String = uiPrefixCombo.SelectedValue
        _listaKomend = ReadListaKomend(currPrefix)
        If _listaKomend IsNot Nothing Then
            uiListaFile.ItemsSource = _listaKomend.Where(Function(x) x.name.NotContains("Dir"))
            uiListaDir.ItemsSource = _listaKomend.Where(Function(x) x.name.Contains("Dir"))
        End If

        '_listaExts = ReadListaExts(currPrefix)
        'If _listaExts IsNot Nothing Then uiListaExt.ItemsSource = _listaExts

    End Sub

    Private Sub uiAddPrefix_Click(sender As Object, e As RoutedEventArgs)

        Dim newName As String = InputBox("Podaj nowy prefiks:")
        If newName.Length < 3 Then Return

        If _listaPrefixow.Contains(newName) Then
            MessageBox.Show("Juz taki istnieje!")
            Return
        End If

        _listaPrefixow = _listaPrefixow & "|" & newName & "|"
        Dim ind As Integer = uiPrefixCombo.Items.Add(newName)
        uiPrefixCombo.SelectedIndex = ind
    End Sub

    Private _listaPrefixow As String = ""

    Private Const CMD_REG_KEY As String = "SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell"

    Private Sub FillComboPrefixes()
        uiPrefixCombo.Items.Clear()

        Dim oKey As RegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(CMD_REG_KEY)
        If oKey Is Nothing Then
            MessageBox.Show("ERROR: nie ma '..\CommandStore\shell'")
            Return
        End If

        For Each sKey As String In oKey.GetSubKeyNames
            If sKey.NotContains(".") Then Continue For
            Dim sPrefix As String = sKey.TrimAfter(".").Replace(".", "")
            If _listaPrefixow.Contains(sPrefix) Then Continue For

            If sPrefix = "Windows" Then Continue For

            _listaPrefixow = _listaPrefixow & "|" & sPrefix & "|"
            uiPrefixCombo.Items.Add(sPrefix)
        Next

        oKey.Close()

        If uiPrefixCombo.Items.Count = 1 Then
            uiPrefixCombo.SelectedIndex = 0
        End If

    End Sub

#End Region

    Private _listaKomend As List(Of JednaKomenda)

    Private Function ReadListaKomend(sPrefix As String) As List(Of JednaKomenda)
        Dim listaKomend As New List(Of JednaKomenda)

        Dim oKey As RegistryKey = Registry.LocalMachine.OpenSubKey(CMD_REG_KEY)
        If oKey Is Nothing Then Return Nothing

        Dim used As String = ";" & ReadActiveCommands(False, sPrefix) & ";" & ReadActiveCommands(True, sPrefix) & ";"
        used = used.ToLowerInvariant

        For Each sKey As String In oKey.GetSubKeyNames
            If sKey.NotContains(".") Then Continue For
            If Not sKey.StartsWithOrdinal(sPrefix) Then Continue For

            Dim oKeyCmd As RegistryKey = oKey.OpenSubKey(sKey)

            Dim oNew As JednaKomenda = ReadCommandKey(oKeyCmd)
            oNew.name = sKey '.TrimBefore(".").Substring(1)

            If used.Contains(sKey.ToLowerInvariant) Then oNew.active = True

            oKeyCmd.Close()

            listaKomend.Add(oNew)
        Next

        Return listaKomend

    End Function


    Private Sub uiListaFile_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If e.AddedItems Is Nothing Then Return
        If e.AddedItems.Count < 1 Then Return

        uiEditKomendaFile.DataContext = e.AddedItems(0)
        'uiEditKomendaFile.DataContext = e.AddedItems(0)
        uiEditKomendaFile.ButtonText = " Save "
        'uiAddSetFile.Content = " Save "
    End Sub

    Private Sub uiListaDir_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If e.AddedItems Is Nothing Then Return
        If e.AddedItems.Count < 1 Then Return

        uiEditKomendaDir.DataContext = e.AddedItems(0)
        'uiEditKomendaFile.DataContext = e.AddedItems(0)
        uiEditKomendaDir.ButtonText = " Save "
        'uiAddSetFile.Content = " Save "

    End Sub

    Private Sub AddOrSet(sender As Object, isDir As Boolean)

        If Not CheckAdmin() Then Return

        Dim oFE As FrameworkElement = sender
        If oFE Is Nothing Then Return

        Dim oItem As JednaKomenda = oFE.DataContext
        If oItem Is Nothing Then Return

        Dim keyName As String = CMD_REG_KEY & "\" & oItem.name
        Try

            Dim oKey As RegistryKey = Registry.LocalMachine.OpenSubKey(keyName, True)
            If oKey Is Nothing Then
                MessageBox.Show("Dziwne, nie mam klucza mimo że powinienem mieć")
                Return
            End If

            WriteCommandKey(oKey, oItem)

            oKey.Close()

        Catch ex As SecurityException
            'Clipboard.SetText("HKEY_LOCAL_MACHINE\" & keyName)
            'MessageBox.Show("Ale nie masz uprawnień! Zmień manualnie (key w clipboard) bądź export")
        End Try

        Try

        Catch ex As Exception

        End Try

        Dim currPrefix As String = uiPrefixCombo.SelectedValue
        WriteActiveCommands(isDir, currPrefix, ListActiveCommands(isDir))

        ' %L long wersja 1 parametru
        ' %W parent directory

    End Sub

    Private Sub uiAddSetFile_Click(sender As Object, e As RoutedEventArgs)
        AddOrSet(sender, False)
    End Sub

    Private Sub uiAddSetDir_Click(sender As Object, e As RoutedEventArgs)
        AddOrSet(sender, True)
    End Sub


#Region "exts"

    Private _listaExts As List(Of JednoExt)


    'Private Function ReadListaExts(sPrefix As String) As List(Of JednoExt)
    '    Dim listaExts As New List(Of JednoExt)

    '    Dim oKey As RegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(EXT_REG_KEY)
    '    If oKey Is Nothing Then Return Nothing

    '    For Each sKey As String In oKey.GetSubKeyNames

    '        'Computer\ HKEY_LOCAL_MACHINE \ SOFTWARE \ Classes \ [ext] \ shell \ [prefix] \ SubCommands = [prefix].[name];...

    '        Dim oKeyExt As RegistryKey = oKey.OpenSubKey(sKey)

    '        Dim oKeyShell As RegistryKey = oKeyExt.OpenSubKey("shell")

    '        If oKeyShell IsNot Nothing Then
    '            Dim oKeyPrefix As RegistryKey = oKeyShell.OpenSubKey(sPrefix)

    '            If oKeyPrefix IsNot Nothing Then
    '                Dim oNew As New JednoExt
    '                oNew.fileExt = sKey
    '                oNew.subcommands = oKeyPrefix.GetValue("subcommands")
    '                listaExts.Add(oNew)
    '                oKeyPrefix.Close()
    '            End If

    '            oKeyShell.Close()
    '        End If

    '        oKeyExt.Close()

    '    Next

    '    Return listaExts

    'End Function

    'Private Sub uiItemExt_MDown(sender As Object, e As MouseButtonEventArgs)
    '    Dim oFE As FrameworkElement = sender
    '    uiEditExt.DataContext = oFE.DataContext
    'End Sub
#End Region


#Region "export"
    Private Sub uiExport_Click(sender As Object, e As RoutedEventArgs)
        ' exportowanie do pliku

        Dim oPicker As New Microsoft.Win32.SaveFileDialog
        oPicker.Title = "Select directory for export"
        oPicker.FileName = "ExplContext_" & Date.Now.ToString("yyyyMMdd") & ".reg"
        oPicker.CheckPathExists = True

        ' Show open file dialog box
        Dim result? As Boolean = oPicker.ShowDialog()
        If result <> True Then Return

        Dim filename As String = oPicker.FileName

        Dim sExport As String = ExportHeader() & ExportFileCommands() & ExportDirCommands() & ExportActiveCommands(uiPrefixCombo.SelectedValue) & ExportExtCommands()

        IO.File.WriteAllText(filename, sExport)

    End Sub

    Private Function ExportHeader() As String
        Return $"Windows Registry Editor Version 5.00

; explorer context submenus, dumped @{Date.Now.ToString("yyyy.MM.dd")}

"
    End Function

    Private Function ExportFileCommands() As String

        Return $"

; commands for files

{String.Join("", _listaKomend.Where(Function(x) x.name.NotContains("Dir")).Select(Function(x) x.Export))}
"

    End Function

    Private Function ExportDirCommands() As String
        Return $"

; commands for directories

{String.Join("", _listaKomend.Where(Function(x) x.name.Contains("Dir")).Select(Function(x) x.Export))}
"

    End Function

    Private Function ExportActiveCommands(prefix As String) As String

        Return $"

; definition of submenus

[HKEY_LOCAL_MACHINE\{GetActiveCommandsRegPath(False)}{prefix}]
""subcommands""=""{ListActiveCommands(False)}

[HKEY_LOCAL_MACHINE\{GetActiveCommandsRegPath(True)}{prefix}]
""subcommands""=""{ListActiveCommands(True)}

"
    End Function

    Private Function ListActiveCommands(forDir As Boolean)

        Return String.Join(";",
                           _listaKomend.
                           Where(Function(x) x.name.Contains("Dir") = forDir).
                           Select(Function(x) x.name))
    End Function


    Private Function ExportExtCommands() As String
        ' *TODO* zrobic
        Return ""
    End Function

#End Region

    Private Sub uiOK_Click(sender As Object, e As RoutedEventArgs)
        Me.Close()
    End Sub








End Class

Public Class JednaKomenda
    Inherits pkar.BaseStruct

    Public Property name As String
    Public Property defDescription As String
    Public Property appliesTo As String
    Public Property command As String
    Public Property active As Boolean = False

    Public Function Export() As String
        Dim sRet As String = $"[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\{name}]
@=""{ToRegString(defDescription)}""" & vbCrLf

        If Not String.IsNullOrEmpty(appliesTo) Then
            sRet = $"{sRet}""AppliesTo""=""{ToRegString(appliesTo)}""
""DefaultAppliesTo""=""{ToRegString(appliesTo)}"""
        End If

        sRet &= vbCrLf & vbCrLf & $"[HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\CommandStore\shell\{name}\command]
@=""{ToRegString(command)}""" & vbCrLf & vbCrLf & vbCrLf

        Return sRet
    End Function

    Private Shared Function ToRegString(value As String) As String
        Return value.Replace("\", "\\").Replace("""", "\""")
    End Function

End Class

Public Class JednoExt
    Inherits pkar.BaseStruct

    Public Property fileExt As String
    Public Property subcommands As String
End Class

Public Module RegWrap

    Private Const EXT_REG_KEY As String = "SOFTWARE\Classes"

#Region "active commands"

    ''' <summary>
    ''' sciezka w registry, razem z \ na końcu
    ''' </summary>
    Public Function GetActiveCommandsRegPath(isDir As Boolean) As String
        If isDir Then
            Return $"{EXT_REG_KEY}\Directory\shell\"
        Else
            Return $"{EXT_REG_KEY}\*\shell\"
        End If
    End Function

    Public Function ReadActiveCommands(isDir As Boolean, prefix As String) As String
        Dim sRegPath As String = GetActiveCommandsRegPath(isDir) & prefix
        Dim oKey As RegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(sRegPath)
        If oKey Is Nothing Then Return ""

        Dim used As String = oKey.GetValue("subcommands", "")
        oKey.Close()

        Return used
    End Function

    Public Function WriteActiveCommands(isDir As Boolean, prefix As String, commands As String) As Boolean
        If Not IsRunningAsAdministrator() Then Return False

        Try

            Dim sRegPath As String = GetActiveCommandsRegPath(isDir) & prefix
            Dim oKey As RegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(sRegPath, True)
            If oKey Is Nothing Then Return ""

            oKey.SetValue("subcommands", commands)
            oKey.Close()

            Return True
        Catch ex As Exception

        End Try

        Return False
    End Function
#End Region


    Public Function ReadCommandKey(oKey As RegistryKey) As JednaKomenda

        Dim oNew As New JednaKomenda
        oNew.appliesTo = oKey.GetValue("AppliesTo", "")
        oNew.defDescription = oKey.GetValue("", "")

        Dim oKeyCmd As RegistryKey = oKey.OpenSubKey("command")
        oNew.command = oKeyCmd.GetValue("", "")
        oKeyCmd.Close()

        Return oNew
    End Function

    Public Function WriteCommandKey(oKey As RegistryKey, oItem As JednaKomenda) As Boolean

        If Not IsRunningAsAdministrator() Then Return False

        oKey.SetValue("AppliesTo", oItem.appliesTo)
        oKey.SetValue("", oItem.defDescription)

        Dim oKeyCmd As RegistryKey = oKey.OpenSubKey("command", True)
        oKeyCmd.SetValue("", oItem.command)

        oKeyCmd.Close()

        Return True
    End Function


End Module

Public Module PkarWpf
    Public Function CheckAdmin() As Boolean
        If IsRunningAsAdministrator() Then Return True

        If MessageBox.Show("Nie jesteś adminem, więc tylko View jest możliwe. Przejść na Admina?", Application.Current.MainWindow.GetType().Assembly.GetName.Name, MessageBoxButton.YesNo) <> MessageBoxResult.Yes Then
            Return False
        End If

        ' ewentualne przełączanie: https://stackoverflow.com/questions/5276674/how-to-force-a-wpf-application-to-run-in-administrator-mode
        Dim exePath As String = Assembly.GetEntryAssembly().Location.Replace(".dll", ".exe")
        Dim procStart As New ProcessStartInfo(exePath)

        ' Using operating shell And setting the ProcessStartInfo.Verb to “runas” will let it run as admin
        procStart.UseShellExecute = True
        procStart.Verb = "runas"

        '// Start the application as New process
        Process.Start(procStart)

        '// Shut down the current (old) process
        Process.GetCurrentProcess.Kill()
        ' Me.Close()
        ' ale tu nie dojdzie raczej
        Return False
    End Function

    Public Function IsRunningAsAdministrator() As Boolean

        '// Get current Windows user
        Dim identity As WindowsIdentity = WindowsIdentity.GetCurrent()

        '// Get current Windows user principal
        Dim principal As WindowsPrincipal = New WindowsPrincipal(identity)

        '// Return TRUE if user Is in role "Administrator"
        Return principal.IsInRole(WindowsBuiltInRole.Administrator)
    End Function

End Module


'programik do robienia menu kaskadowego w Explorer

'Prefix: [pkar]

'po wpisaniu prefix pokazuje wszystkie zdefiniowane komendy, może też szukac ich wywolania

'Command name :  [pkar.][name]
'For extension: .[ext]
'Command: [cmd]

'Computer\ HKEY_LOCAL_MACHINE \ SOFTWARE \ Classes \ [ext] \ shell \ [prefix] \ SubCommands = [prefix].[name];...

'HKEY_LOCAL_MACHINE\ Software \ Microsoft \ Windows \ CurrentVersion \ Explorer \ CommandStore \ Shell \ [prefix].[name] \ command \@=[cmd]
'HKEY_LOCAL_MACHINE\ Software \ Microsoft \ Windows \ CurrentVersion \ Explorer \ CommandStore \ Shell \ [prefix].[name] \ AppliesTo = System.ItemName = epub
'HKEY_LOCAL_MACHINE\ Software \ Microsoft \ Windows \ CurrentVersion \ Explorer \ CommandStore \ Shell \ [prefix].[name] \ DefaultAppliesTo = System.ItemName = epub
'HKEY_LOCAL_MACHINE\ Software \ Microsoft \ Windows \ CurrentVersion \ Explorer \ CommandStore \ Shell \ [prefix].[name] \@=Epub -> &Mobi

'System.ItemName : (".azw" Or ".mobi")
'System.Size : <1mb


'Commandline from winui3? list, remove, add?
'linki do help wyrazen (System.Size:<1mb)
