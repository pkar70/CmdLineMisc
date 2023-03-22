Imports System.Runtime.CompilerServices
Imports Microsoft.Win32

Partial Public Module Extensions

    <Extension>
    Public Function NotContains(baseValue As String, value As String) As Boolean
        Return Not baseValue.Contains(value)
    End Function

    '<Extension>
    'Public Function GetSubkey(baseKey As RegistryKey, subkey As String) As RegistryKey
    '    Dim sName As String = baseKey.Name

    '    Dim hive As RegistryKey = baseKey.GetHive
    '    If hive Is Nothing Then Return hive

    '    Return hive.OpenSubKey()

    'End Function

    '<Extension>
    'Public Function GetHive(baseKey As RegistryKey) As RegistryKey

    '    If baseKey.Name.StartsWith("HKEY_LOCAL_MACHINE") Then Return Microsoft.Win32.Registry.LocalMachine
    '    If baseKey.Name.StartsWith("HKEY_CLASSES_ROOT") Then Return Microsoft.Win32.Registry.ClassesRoot
    '    If baseKey.Name.StartsWith("HKEY_CURRENT_USER") Then Return Microsoft.Win32.Registry.CurrentUser

    '    Return Nothing
    'End Function

End Module
