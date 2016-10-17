Imports System.Data.SqlClient

Public Class LoginWindow

    Private Sub btnCancel_Click(sender As Object, e As RoutedEventArgs) Handles btnCancel.Click
        If txtP.Password.Length > 0 Or _
            txtU.Text.Length > 0 Then
            txtP.Clear()
            txtU.Clear()
        Else
            Me.Close()
        End If
    End Sub
    Private con As New Entities_POD
    Private Sub btnOK_Click(sender As Object, e As RoutedEventArgs) Handles btnOK.Click
        appUser = txtU.Text.ToUpper
        If (From rw In con.Users Where rw.UserName = appUser Select rw).Any Then
            Dim pwdConfirm As Boolean = con.Database.SqlQuery(Of Boolean)("SELECT cast(PWDCOMPARE(@P, Password) as bit) AS Password  FROM [User] WHERE (UserName = upper(@U)) AND (Lock = 0)", _
                                                                      New SqlParameter("@P", txtP.Password), New SqlParameter("@U", appUser))(0)
            If pwdConfirm Then
                sessionDt = con.Database.SqlQuery(Of DateTime)("Select GETDATE()")(0)
                Dim x As New MainWindow
                x.Show()
                Me.Close()
            Else
                MessageBox.Show("Wrong Password")
            End If
        Else
            MessageBox.Show("Wrong User")
        End If
    End Sub
End Class
