Imports System
Imports System.IO

Module Program

    Private bRenameDirs As Boolean = False
    Private bRenameFiles As Boolean = True
    Private bStrictMode As Boolean = False
    Private bRecursive As Boolean = False


    Sub Main(args As String())
        Console.WriteLine("File renamer (to POSIX portable filename charset)" & vbCrLf)

        If ProcessArgs(args) Then
            Dim currDir As String = Directory.GetCurrentDirectory
            RunRenamer(currDir)
        End If

    End Sub

    Private Sub PrintHelp()
        Console.WriteLine("Options:")
        Console.WriteLine("/?, /h : this help")
        Console.WriteLine("/r, /s : recursive")
        Console.WriteLine("/d     : rename folders")
        Console.WriteLine("/f-    : don't rename files")
        Console.WriteLine("/strict: strict POSIX portable")
        Console.WriteLine(vbCrLf & "defaults: only strip accents; filenames, no directories")
    End Sub

    Private Function ProcessArgs(args As String()) As Object

        For Each param In args
            Select Case param.ToLowerInvariant
                Case "/?", "/h"
                    PrintHelp()
                    Return False
                Case "/r", "/s"
                    bRecursive = True
                Case "/d"
                    bRenameDirs = True
                Case "/f-"
                    bRenameFiles = False
                Case "/strict"
                    bStrictMode = True
                Case Else
                    Console.WriteLine("Unknown option: " & param)
                    PrintHelp()
                    Return False
            End Select
        Next

        Return True
    End Function


    Private Sub RunRenamer(inDir As String)

        ' recursive
        If bRecursive Then
            For Each subdir As String In Directory.GetDirectories(inDir)
                RunRenamer(subdir)
            Next
        End If


        ' directory
        If bRenameDirs Then
            For Each subdir As String In Directory.GetDirectories(inDir)
                Dim newName As String = GetNewFilename(Path.GetFileName(subdir))
                If newName <> "" Then Directory.Move(subdir, Path.Combine(Path.GetDirectoryName(subdir), newName))
            Next
        End If

        ' files
        If bRenameFiles Then
            For Each filename As String In Directory.GetFiles(inDir)
                Dim newName As String = GetNewFilename(Path.GetFileName(filename))
                If newName <> "" Then File.Move(filename, Path.Combine(Path.GetDirectoryName(filename), newName))
            Next
        End If


    End Sub

    Public Function GetNewFilename(oldfilename As String) As String
        Dim newFilename As String = If(bStrictMode, oldfilename.ToPOSIXportableFilename, oldfilename.DropAccents)
        If oldfilename = newFilename Then Return ""
        Console.WriteLine($"{oldfilename} -> {newFilename}")
        Return newFilename
    End Function

    ''' <summary>
    ''' try to convert string to valid filename (very strict: POSIX portable filename, see IEEE 1003.1, 3.282), dropping accents etc., and all other characters change to '_'
    ''' POSIX allows only latin letters, digits, dot, underscore and minus.
    ''' </summary>
    <Runtime.CompilerServices.Extension()>
    Public Function ToPOSIXportableFilename(ByVal basestring As String) As String
        Dim FKD As String = basestring.Normalize(Text.NormalizationForm.FormKD)
        Dim sRet As String = ""

        For Each cTmp As Char In FKD
            If "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789._-".Contains(cTmp) Then
                sRet &= cTmp
            ElseIf AscW(cTmp) >= &H300 AndAlso AscW(cTmp) < &H36F Then
                ' combining - skip
            Else
                ' nie wiadomo co, wiêc podmieniamy
                sRet &= "_"
            End If
        Next

        Return sRet
    End Function

    ''' <summary>
    ''' try to convert string to filename, dropping accents
    ''' </summary>
    <Runtime.CompilerServices.Extension()>
    Public Function DropAccents(ByVal basestring As String) As String
        Dim FKD As String = basestring.Normalize(Text.NormalizationForm.FormKD)
        Dim sRet As String = ""

        For Each cTmp As Char In FKD
            If AscW(cTmp) >= &H2B0 AndAlso AscW(cTmp) < &H36F Then
                ' 02B0 - 02FF	Spacing Modifier Letters
                ' 0300 - 036F	Combining Diacritical Marks
            ElseIf AscW(cTmp) >= &H1AB0 AndAlso AscW(cTmp) < &H1B00 Then
                ' 1AB0 - 1AFF	Combining Diacritical Marks Extended
            ElseIf AscW(cTmp) >= &H1DC0 AndAlso AscW(cTmp) < &H1E00 Then
                ' 1DC0 - 1DFF	Combining Diacritical Marks Supplement
            ElseIf AscW(cTmp) >= &H20D0 AndAlso AscW(cTmp) < &H2100 Then
                ' 20D0 - 20FF	Combining Diacritical Marks for Symbols
            ElseIf AscW(cTmp) >= &HFE20 AndAlso AscW(cTmp) < &HFE30 Then
                ' FE20 - FE2F	Combining Half Marks
            Else
                sRet &= cTmp
            End If
        Next

        Return sRet
    End Function

End Module
