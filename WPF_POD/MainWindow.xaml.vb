Imports System.Drawing.Imaging
Imports System.IO
Imports ADFScanner
Imports System.Drawing
Imports iTextSharp.text
Imports iTextSharp.text.pdf

Class MainWindow

    Private Sub btnScan_Click(sender As Object, e As RoutedEventArgs) Handles btnScan.Click
        Dim _scanner As ADFScanner.ADFScan
        If My.Settings.SCANNERID = "" Then
            _scanner = New ADFScanner.ADFScan()
            My.Settings.SCANNERID = _scanner.deviceID
            My.Settings.Save()
        Else
            _scanner = New ADFScanner.ADFScan(My.Settings.SCANNERID)
        End If
        AddHandler _scanner.Scanning, New EventHandler(Of WiaImageEventArgs)(AddressOf _scanner_scanning)
        AddHandler _scanner.ScanComplete, New EventHandler(AddressOf _scanner_scancomplete)
        ClearThumbnails()
        DeleteFilesFromFolder(Path.GetTempPath + "\POD")
        _listFileNames.Clear()
        _listPDFFileNames.Clear()
        _scanner.BeginScan(ScanColor.Color, 150)

    End Sub
    Dim _listFileNames As New List(Of String)
    Dim _listPDFFileNames As New List(Of String)
    Private Sub _scanner_scanning(sender As Object, e As WiaImageEventArgs)
        If Not System.IO.Directory.Exists(System.IO.Path.GetTempPath + "\POD") Then
            System.IO.Directory.CreateDirectory(System.IO.Path.GetTempPath + "\POD")
        End If
        Dim filename As String = System.IO.Path.GetRandomFileName().Split("."c)(0)
        filename = System.IO.Path.GetTempPath + "\POD\" + filename + ".jpg"
        e.ScannedImage.Save(filename, ImageFormat.Jpeg)
        Dim fs As FileStream = File.Open(filename, FileMode.Open)
        Dim dImg As New System.Drawing.Bitmap(fs)
        Dim ms As New MemoryStream()
        dImg.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg)
        Dim bImg As New System.Windows.Media.Imaging.BitmapImage()
        bImg.BeginInit()
        bImg.CacheOption = BitmapCacheOption.OnLoad
        bImg.StreamSource = New MemoryStream(ms.ToArray())
        bImg.EndInit()
        AddImageThumbnail(bImg)
        _listFileNames.Add(filename)
        fs.Dispose()
        ms.Dispose()
    End Sub
    Private Sub _scanner_scancomplete(sender As Object, e As EventArgs)
        For Each filename As String In _listFileNames
            Dim document As iTextSharp.text.Document = New iTextSharp.text.Document()
            iTextSharp.text.Document.Compress = True
            document.SetPageSize(PageSize.A4)
            document.SetMargins(0.0!, 0.0!, 0.0!, 0.0!)
            Dim _pdfFileName As String = System.IO.Path.GetRandomFileName().Split("."c)(0)
            _pdfFileName = System.IO.Path.GetTempPath + "\POD\" + _pdfFileName + ".pdf"
            PdfWriter.GetInstance(document, New FileStream(_pdfFileName, FileMode.CreateNew))
            document.Open()
            Dim instance As iTextSharp.text.Image = iTextSharp.text.Image.GetInstance(filename)
            instance.ScaleToFit(document.PageSize.Width, document.PageSize.Height)
            instance.SetDpi(300, 300)
            document.Add(instance)
            document.Close()
            _listPDFFileNames.Add(_pdfFileName)
        Next
        MessageBox.Show("Scan Completed")
    End Sub

    Sub DeleteFilesFromFolder(Folder As String)
        System.GC.Collect()
        System.GC.WaitForPendingFinalizers()
        If Directory.Exists(Folder) Then
            For Each _file As String In Directory.GetFiles(Folder)
                File.Delete(_file)
            Next
            For Each _folder As String In Directory.GetDirectories(Folder)
                DeleteFilesFromFolder(_folder)
            Next
        End If
    End Sub

    Private Sub Thumbnail_Click(sender As Object, e As RoutedEventArgs)
        Dim btn As Button = DirectCast(sender, Button)
        Dim img As System.Windows.Controls.Image = DirectCast(btn.Content, System.Windows.Controls.Image)
        imgPreview.Source = img.Source
    End Sub

    Public Sub AddImageThumbnail(src As ImageSource)
        Dim img As New System.Windows.Controls.Image()
        img.Source = src
        img.Stretch = Stretch.Uniform
        Dim btn As New Button()
        'btn.Tag =
        btn.Content = img
        btn.Width = [Double].NaN
        ' This is how you specify auto sizing behavior 
        btn.Height = [Double].NaN
        AddHandler btn.Click, New RoutedEventHandler(AddressOf Thumbnail_Click)
        ThumbnailStackPanel.Children.Add(btn)
        ' auto select if not set
        If imgPreview.Source Is Nothing Then
            imgPreview.Source = src
        End If
    End Sub

    Public Sub ClearThumbnails()
        For Each btn As Button In ThumbnailStackPanel.Children
            btn.Content = Nothing
        Next
        ThumbnailStackPanel.Children.Clear()
        imgPreview.Source = Nothing
    End Sub
    Private con As New Entities_POD
    Private hdr As SAP_DOCs_HDR
    Private Sub txtDlvNo_KeyDown(sender As Object, e As KeyEventArgs) Handles txtDlvNo.KeyDown
        Select Case e.Key
            Case Key.Enter
                'txtDlvNo.Text = "1710302256"
                If txtDlvNo.Text.Length > 0 Then
                    hdr = (From rw In con.SAP_DOCs_HDR
                           Where rw.DocID = txtDlvNo.Text
                           Select rw).DefaultIfEmpty(Nothing).SingleOrDefault
                    If IsNothing(hdr) Then Exit Sub
                    lblCustomer.Content = (From rw In con.Customers Where rw.CustomreID = hdr.CustomerID_ShipTo
                                           Select rw.EnName).DefaultIfEmpty(Nothing).SingleOrDefault
                    lblPGI.Content = hdr.PGIDate
                    lblPO.Content = hdr.PoNumber
                    lblSO.Content = hdr.OrderNo
                    cbxDriver.SelectedValue = hdr.DriverID
                    If IsNothing(cbxDriver.SelectedValue) And Trim(hdr.Drvr_SAP_CODE) <> "" Then
                        cbxDriver.SelectedValue = (From rw In con.Drivers Where rw.SAP_CODE = hdr.Drvr_SAP_CODE Select rw.DriverID).DefaultIfEmpty(0).ToList(0)
                    End If
                    txtDriverMobile.Text = hdr.DriverMobile
                    txtDriverName.Text = hdr.DriverName
                    If Not IsNothing(cbxDriver.SelectedValue) Then
                        Dim drv As Driver = (From rw In con.Drivers Where rw.DriverID = cbxDriver.SelectedValue Select rw).DefaultIfEmpty(Nothing).SingleOrDefault
                        txtDriverMobile.Text = drv.Mobile
                        txtDriverName.Text = drv.EnName
                    End If
                    cbxTransporter.SelectedValue = hdr.TransporterID
                    If IsNothing(cbxTransporter.SelectedValue) Then
                        cbxTransporter.SelectedValue = (From rw In con.Transporters Where rw.SAP_CODE = hdr.Trns_SAP_CODE Select rw.TransporterID).DefaultIfEmpty(0).ToList(0)
                    End If

                    If Trim(hdr.TrnsRefInv) <> "" And Trim(hdr.TrnsRefInv) <> "0.0" Then txtInvRef.Text = hdr.TrnsRefInv
                    txtPlateNo.Text = hdr.PlateNumber
                    dtpOffload.SelectedDate = hdr.CustomerRecieve
                    dtpShip.SelectedDate = hdr.TrucksRelease
                Else
                    MessageBox.Show("Insert Delivery Number.")
                End If
        End Select
    End Sub

    Private Sub txtDlvNo_PreviewKeyDown(sender As Object, e As KeyEventArgs) Handles txtDlvNo.PreviewKeyDown
        Select Case e.Key
            Case Key.Enter
            Case Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D5,
                    Key.D6, Key.D7, Key.D8, Key.D9, Key.NumPad0, Key.NumPad1,
                    Key.NumPad2, Key.NumPad3, Key.NumPad4, Key.NumPad5, Key.NumPad6, Key.NumPad7,
                    Key.NumPad8, Key.NumPad9, Key.Back
            Case Else
                e.Handled = True
        End Select
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

    End Sub

    Private Sub Window_Initialized(sender As Object, e As EventArgs)
        Try
            Dim _Drivers As List(Of Driver) = (From rw In con.Drivers Select rw).DefaultIfEmpty(Nothing).ToList
            Dim _Transporters As List(Of Transporter) = (From rw In con.Transporters Select rw).DefaultIfEmpty(Nothing).ToList
            cbxDriver.ItemsSource = _Drivers
            cbxDriver.DisplayMemberPath = "EnName"
            cbxDriver.SelectedValuePath = "DriverID"
            cbxDriver.SelectedIndex = -1
            cbxTransporter.ItemsSource = _Transporters
            cbxTransporter.DisplayMemberPath = "EnName"
            cbxTransporter.SelectedValuePath = "TransporterID"
            cbxTransporter.SelectedIndex = -1
            cbxTrnsInv_Transporter.ItemsSource = _Transporters
            cbxTrnsInv_Transporter.DisplayMemberPath = "EnName"
            cbxTrnsInv_Transporter.SelectedValuePath = "TransporterID"
            cbxTrnsInv_Transporter.SelectedIndex = -1
        Catch ex As Exception

        End Try
    End Sub

    Private Sub btnScanner_Cancel_Click(sender As Object, e As RoutedEventArgs) Handles btnScanner_Cancel.Click

        ClearThumbnails()
        DeleteFilesFromFolder(Path.GetTempPath + "\POD")
        _listFileNames.Clear()
        _listPDFFileNames.Clear()
        If _pod Then
            txtDlvNo.Clear()
            txtDriverName.Clear()
            txtPlateNo.Clear()
            lblCustomer.Content = ""
            lblPO.Content = ""
            lblSO.Content = ""
            lblPGI.Content = ""
            dtpOffload.SelectedDate = Nothing
            dtpShip.SelectedDate = Nothing
            cbxDriver.SelectedIndex = -1
            cbxTransporter.SelectedIndex = -1
            'txtInvRef.Clear()
            txtDriverMobile.Clear()
        Else
            cbxTrnsInv_Transporter.SelectedIndex = -1
            txtTrnsInv_Amount.Clear()
            txtTrnsInv_InvNo.Clear()
        End If

    End Sub

    Private Sub btnScanner_Submit_Click(sender As Object, e As RoutedEventArgs) Handles btnScanner_Submit.Click
        If _pod Then
            If IsNothing(hdr) Then Exit Sub
            If hdr.CustomerRecieve <> Convert.ToDateTime(dtpOffload.SelectedDate) Or
            hdr.DriverID = cbxDriver.SelectedValue Or
            hdr.DriverMobile = txtDriverMobile.Text Or
            hdr.DriverName = txtDriverName.Text Or
            hdr.ModDate = DateTime.Now Or
            hdr.ModUser = "ADMIN" Or
            hdr.PlateNumber = txtPlateNo.Text Or
            hdr.TransporterID = cbxTransporter.SelectedValue Or
            hdr.TrnsRefInv = txtInvRef.Text Or
            hdr.TrucksRelease = Convert.ToDateTime(dtpShip.SelectedDate) Then
                Dim hdrBefore As New LOG_SAP_DOCS_HDR
                Dim hdrAfter As New LOG_SAP_DOCS_HDR
                With hdrBefore
                    .C___operation = 3
                    .CustomerRecieve = hdr.CustomerRecieve
                    .DocID = hdr.DocID
                    .DriverID = hdr.DriverID
                    .DriverMobile = hdr.DriverMobile
                    .DriverName = hdr.DriverName
                    .Drvr_SAP_CODE = hdr.Drvr_SAP_CODE
                    .ModDate = hdr.ModDate
                    .ModUser = hdr.ModUser
                    .PlateNumber = hdr.PlateNumber
                    .TransporterID = hdr.TransporterID
                    .trDt = DateTime.Now
                    .Trns_SAP_CODE = hdr.Trns_SAP_CODE
                    .TrucksRelease = hdr.TrucksRelease
                End With


                hdr.CustomerRecieve = Convert.ToDateTime(dtpOffload.SelectedDate)
                If IsNothing(cbxDriver.SelectedValue) Then
                    hdr.DriverID = 0
                    hdr.Drvr_SAP_CODE = " "
                Else
                    hdr.DriverID = cbxDriver.SelectedValue
                    hdr.Drvr_SAP_CODE = (From rw In con.Drivers Where rw.DriverID = hdr.DriverID Select rw.SAP_CODE).DefaultIfEmpty(Nothing).SingleOrDefault
                End If
                hdr.DriverMobile = txtDriverMobile.Text
                If txtDriverName.Text.Length > 0 Then
                    hdr.DriverName = txtDriverName.Text
                Else
                    hdr.DriverName = " "
                End If
                hdr.ModDate = DateTime.Now
                hdr.ModUser = appUser
                hdr.PlateNumber = txtPlateNo.Text
                hdr.TransporterID = cbxTransporter.SelectedValue
                hdr.Trns_SAP_CODE = (From rw In con.Transporters Where rw.TransporterID = hdr.TransporterID Select rw.SAP_CODE).DefaultIfEmpty(Nothing).SingleOrDefault
                hdr.TrnsRefInv = txtInvRef.Text
                hdr.TrucksRelease = Convert.ToDateTime(dtpShip.SelectedDate)

                With hdrAfter
                    .C___operation = 4
                    .CustomerRecieve = hdr.CustomerRecieve
                    .DocID = hdr.DocID
                    .DriverID = hdr.DriverID
                    .DriverMobile = hdr.DriverMobile
                    .DriverName = hdr.DriverName
                    .Drvr_SAP_CODE = hdr.Drvr_SAP_CODE
                    .ModDate = hdr.ModDate
                    .ModUser = hdr.ModUser
                    .PlateNumber = hdr.PlateNumber
                    .TransporterID = hdr.TransporterID
                    .trDt = DateTime.Now
                    .Trns_SAP_CODE = hdr.Trns_SAP_CODE
                    .TrucksRelease = hdr.TrucksRelease
                End With
                con.LOG_SAP_DOCS_HDR.Add(hdrBefore)
                con.LOG_SAP_DOCS_HDR.Add(hdrAfter)
            End If
            Dim indx As Integer = (From rw In con.Docs Where rw.SessionDate = sessionDt Select rw.AttachmentID).DefaultIfEmpty(0).Max + 1
            For Each fileName As String In _listPDFFileNames
                Dim dtl As New Doc
                Dim logDtl As New LOG_DOCS
                dtl.AttachmentID = indx
                dtl.DOCID = hdr.DocID
                dtl.FileContents = ConvertImageFiletoBytes(fileName)
                dtl.FileExt = "pdf"
                dtl.ModUser = appUser
                dtl.SessionDate = sessionDt
                dtl.ModDate = DateTime.Now
                dtl.SAP_FileName = ""
                dtl.FailedError = ""
                dtl.ReviewedBy = ""
                logDtl.SessionDate = dtl.SessionDate
                logDtl.AttachmentID = dtl.AttachmentID
                logDtl.C___operation = 2
                logDtl.DOCID = dtl.DOCID
                logDtl.trDt = DateTime.Now
                logDtl.ModUser = dtl.ModUser
                logDtl.Moddate = dtl.ModDate
                con.Docs.Add(dtl)
                con.LOG_DOCS.Add(logDtl)
                indx = indx + 1
            Next
            Try
                Dim x As List(Of System.Data.Entity.Validation.DbEntityValidationResult) = con.GetValidationErrors()
                con.SaveChanges()
                MessageBox.Show("Saved Successfully")
                btnScanner_Cancel_Click(Nothing, Nothing)
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        Else
            If IsNothing(cbxTrnsInv_Transporter.SelectedValue) Or
                txtTrnsInv_InvNo.Text.Length = 0 Then Exit Sub
            Dim hdr As New TransporterInvHDR
            hdr.TransporterID = cbxTrnsInv_Transporter.SelectedValue
            hdr.TransporterSAPCODE = (From rw In con.Transporters Where rw.TransporterID = hdr.TransporterID Select rw.SAP_CODE).DefaultIfEmpty(Nothing).SingleOrDefault
            hdr.InvoiceRefNo = txtTrnsInv_InvNo.Text
            hdr.TotalAmount = txtTrnsInv_Amount.Text
            hdr.year = DateTime.Now.Year
            hdr.ID = (From rw In con.TransporterInvHDRs Where rw.year = hdr.year Select rw.ID).DefaultIfEmpty(0).Max + 1
            con.TransporterInvHDRs.Add(hdr)
            Dim indx As Integer = (From rw In con.TransporterInvFiles Where rw.Year = hdr.year And rw.ID = hdr.ID Select rw.SEQID).DefaultIfEmpty(0).Max + 1
            For Each fileName As String In _listPDFFileNames
                Dim dtl As New TransporterInvFile
                dtl.FileContents = ConvertImageFiletoBytes(fileName)
                dtl.Year = hdr.year
                dtl.ID = hdr.ID
                dtl.SEQID = indx
                indx = indx + 1
                con.TransporterInvFiles.Add(dtl)
            Next
            Try
                Dim x As List(Of System.Data.Entity.Validation.DbEntityValidationResult) = con.GetValidationErrors()
                con.SaveChanges()
                MessageBox.Show("Saved Successfully")
                btnScanner_Cancel_Click(Nothing, Nothing)
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        End If
    End Sub

    Public Function ConvertImageFiletoBytes(ByVal ImageFilePath As String) As Byte()
        Dim _tempByte() As Byte = Nothing
        If String.IsNullOrEmpty(ImageFilePath) = True Then
            Throw New ArgumentNullException("Image File Name Cannot be Null or Empty", "ImageFilePath")
            Return Nothing
        End If
        Try
            Dim _fileInfo As New IO.FileInfo(ImageFilePath)
            Dim _NumBytes As Long = _fileInfo.Length
            Dim _FStream As New IO.FileStream(ImageFilePath, IO.FileMode.Open, IO.FileAccess.Read)
            Dim _BinaryReader As New IO.BinaryReader(_FStream)
            _tempByte = _BinaryReader.ReadBytes(Convert.ToInt32(_NumBytes))
            _fileInfo = Nothing
            _NumBytes = 0
            _FStream.Close()
            _FStream.Dispose()
            _BinaryReader.Close()
            Return _tempByte
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    Private Sub txtDriverNumber_KeyDown(sender As Object, e As KeyEventArgs) Handles txtDriverNumber.KeyDown
        Select Case e.Key
            Case Key.Enter
                cbxDriver.SelectedValue = (From rw In con.Drivers Where rw.SAP_CODE = txtDriverNumber.Text Select rw.DriverID).DefaultIfEmpty(0).SingleOrDefault
            Case Key.Escape
                cbxDriver.SelectedIndex = -1
                txtDriverNumber.Clear()
        End Select
    End Sub
    Dim _pod As Boolean = True
    Private Sub tabControl_ScannerData_SelectionChanged(sender As Object, e As SelectionChangedEventArgs) Handles tabControl_ScannerData.SelectionChanged
        If tbiScannerPOD.IsSelected Then
            _pod = True
        ElseIf tbiScannerTransporterInv.IsSelected Then
            _pod = False
        End If
    End Sub
End Class
