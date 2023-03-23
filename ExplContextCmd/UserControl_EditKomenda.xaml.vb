Namespace User_Controls

    Public Class UserControl_EditKomenda
        Public Property ButtonText As String
            Get
                Return uiAddSetFile.Content
            End Get
            Set(value As String)
                uiAddSetFile.Content = value
            End Set
        End Property

        Public Event Click As RoutedEventHandler

        Private Sub uiAddSetFile_Click(sender As Object, e As RoutedEventArgs)
            RaiseEvent Click(sender, e)
        End Sub

        Private Sub Hyperlink_RequestNavigate(sender As Object, e As RequestNavigateEventArgs)
            Dim si As New ProcessStartInfo With {.FileName = e.Uri.AbsoluteUri, .UseShellExecute = True}
            Dim proc As New Process With {.StartInfo = si}
            proc.Start()

            'Process.Start(New ProcessStartInfo(e.Uri.AbsoluteUri))
            e.Handled = True
        End Sub


    End Class


End Namespace