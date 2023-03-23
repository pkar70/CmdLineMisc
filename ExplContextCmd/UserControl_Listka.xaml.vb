
Namespace User_Controls

    Public Class UserControl_Listka

        Public Property ItemsSource As Object
            Get
                Return uiUCLista.ItemsSource
            End Get
            Set(value As Object)
                uiUCLista.ItemsSource = value
            End Set
        End Property

        Public Event SelectionChanged As SelectionChangedEventHandler

        'Custom Event SelectionChanged As SelectionChangedEventHandler
        '    AddHandler(ByVal value As SelectionChangedEventHandler)
        '        AddHandler uiUCLista.SelectionChanged, value
        '    End AddHandler
        '    RemoveHandler(ByVal value As SelectionChangedEventHandler)
        '        RemoveHandler uiUCLista.SelectionChanged, value
        '    End RemoveHandler
        '    RaiseEvent(sender As Object, e As SelectionChangedEventArgs)
        '        RaiseEvent uiUCLista.SelectionChanged(sender,e)
        '    End RaiseEvent
        'End Event

        '    RemoveHandler(ByVal value As SelectionChangedEventHandler)
        '    numericUpDown1.ValueChanged -= value
        'End RemoveHandler

        Private Sub uiListka_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            RaiseEvent SelectionChanged(sender, e)
        End Sub
    End Class

End Namespace