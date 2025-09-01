Imports System.Data.SQLite
Imports System.IO
Imports System.Text.Json
Imports System.Diagnostics

Public Class FormGestioneSetCategorie

    ' Strutture dati per gestire i set
    Public Class SetCategoria
        Public Property MacroCategoria As String
        Public Property Categoria As String
        Public Property NumeroPattern As Integer
        Public Property NumeroTransazioni As Integer
        Public Property UltimoUtilizzo As Date?
        Public Property Parole As List(Of String)

        Public Sub New()
            Parole = New List(Of String)
        End Sub

        Public ReadOnly Property ChiaveSet As String
            Get
                Return $"{MacroCategoria}|{Categoria}"
            End Get
        End Property

        Public ReadOnly Property NomeCompleto As String
            Get
                Return $"{MacroCategoria} > {Categoria}"
            End Get
        End Property
    End Class

    ' Dichiarazioni variabili membro della classe
    Private dgvParole As DataGridView
    Private txtFiltraParole As TextBox
    Private cmbFiltraSet As ComboBox

    ' Variabili di classe
    Private setList As List(Of SetCategoria)
    Private setSelezionato As SetCategoria

    ' Controlli per le tab (verranno creati dinamicamente)
    Private dgvDettaglioSet As DataGridView
    Private txtMacroCategoria As TextBox
    Private txtCategoria As TextBox
    Private btnSalvaSet As Button
    Private btnEliminaSet As Button
    Private btnUnisciSet As Button

    ' Tab Statistiche
    Private lblTotaleSet As Label
    Private lblTotalePattern As Label
    Private lblTotaleTransazioni As Label
    Private chartDistribuzione As DataVisualization.Charting.Chart

    Private Sub FormGestioneSetCategorie_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            CreaControlliDettaglioSet()
            CreaControlliStatistiche()
            CreaControlliAnalisi()
            CreaControlliExportImport()

            CaricaSetCategorie()
            PopolaTreeView()
            AggiornaStatistiche()

        Catch ex As Exception
            MessageBox.Show("Errore durante l'inizializzazione: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

#Region "Creazione Controlli Dinamici"

    Private Function TrovaPulsante(container As Control, nomePulsante As String) As Button
        For Each ctrl As Control In container.Controls
            If TypeOf ctrl Is Button AndAlso ctrl.Name = nomePulsante Then
                Return CType(ctrl, Button)
            End If

            ' Cerca ricorsivamente nei contenitori
            If ctrl.HasChildren Then
                Dim risultato = TrovaPulsante(ctrl, nomePulsante)
                If risultato IsNot Nothing Then
                    Return risultato
                End If
            End If
        Next

        Return Nothing
    End Function

    Private Sub CreaControlliDettaglioSet()
        ' Panel principale per dettaglio set
        Dim pnlDettaglio As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(10)}
        tabDettaglioSet.Controls.Add(pnlDettaglio)

        ' Informazioni set selezionato
        Dim lblTitoloSet As New Label With {
        .Text = "Modifica Set Selezionato",
        .Font = New Font("Segoe UI", 12, FontStyle.Bold),
        .Dock = DockStyle.Top,
        .Height = 40,
        .TextAlign = ContentAlignment.MiddleLeft
    }
        pnlDettaglio.Controls.Add(lblTitoloSet)

        ' Panel per i campi di modifica (RIDOTTO - solo 2 livelli)
        Dim pnlCampi As New Panel With {.Dock = DockStyle.Top, .Height = 90}
        pnlDettaglio.Controls.Add(pnlCampi)

        ' MacroCategoria
        Dim lblMacro As New Label With {.Text = "Macro Categoria:", .Location = New Point(10, 15), .Size = New Size(120, 20)}
        txtMacroCategoria = New TextBox With {.Location = New Point(140, 12), .Size = New Size(200, 27)}
        pnlCampi.Controls.AddRange({lblMacro, txtMacroCategoria})

        ' Categoria  
        Dim lblCat As New Label With {.Text = "Categoria:", .Location = New Point(10, 45), .Size = New Size(120, 20)}
        txtCategoria = New TextBox With {.Location = New Point(140, 42), .Size = New Size(200, 27)}
        pnlCampi.Controls.AddRange({lblCat, txtCategoria})

        ' RIMUOVI COMPLETAMENTE txtSottoCategoria

        ' Pulsanti azione
        Dim pnlPulsanti As New Panel With {.Dock = DockStyle.Top, .Height = 50}
        pnlDettaglio.Controls.Add(pnlPulsanti)

        btnSalvaSet = New Button With {
        .Text = "Salva Modifiche",
        .Location = New Point(10, 10),
        .Size = New Size(120, 30),
        .BackColor = Color.FromArgb(46, 125, 50),
        .ForeColor = Color.White
    }
        AddHandler btnSalvaSet.Click, AddressOf BtnSalvaSet_Click

        btnEliminaSet = New Button With {
        .Text = "Elimina Set",
        .Location = New Point(140, 10),
        .Size = New Size(120, 30),
        .BackColor = Color.FromArgb(244, 67, 54),
        .ForeColor = Color.White
    }
        AddHandler btnEliminaSet.Click, AddressOf BtnEliminaSet_Click

        btnUnisciSet = New Button With {
        .Text = "Unisci Set",
        .Location = New Point(270, 10),
        .Size = New Size(120, 30),
        .BackColor = Color.FromArgb(255, 152, 0),
        .ForeColor = Color.White
    }
        AddHandler btnUnisciSet.Click, AddressOf BtnUnisciSet_Click

        pnlPulsanti.Controls.AddRange({btnSalvaSet, btnEliminaSet, btnUnisciSet})

        ' DataGridView per pattern associati (INVARIATO)
        dgvDettaglioSet = New DataGridView With {
    .Dock = DockStyle.Fill,
    .AllowUserToAddRows = False,
    .AllowUserToDeleteRows = False,
    .ReadOnly = True,
    .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
    .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
}

        dgvDettaglioSet.Columns.AddRange({
    New DataGridViewTextBoxColumn With {.Name = "Parola", .HeaderText = "Parola Pattern"},
    New DataGridViewTextBoxColumn With {.Name = "Peso", .HeaderText = "Peso"},
    New DataGridViewTextBoxColumn With {.Name = "Fonte", .HeaderText = "Fonte"},
    New DataGridViewTextBoxColumn With {.Name = "NumTransazioni", .HeaderText = "Transazioni"}
})

        AddHandler dgvDettaglioSet.CellDoubleClick, AddressOf DgvDettaglioSet_CellDoubleClick

        pnlDettaglio.Controls.Add(dgvDettaglioSet)
        dgvDettaglioSet.BringToFront()
    End Sub

    ' Gestore doppio click sulla griglia dettaglio set (parole pattern)
    Private Sub DgvDettaglioSet_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 OrElse dgvDettaglioSet.Rows.Count = 0 Then Return
        If setSelezionato Is Nothing Then Return

        Dim row As DataGridViewRow = dgvDettaglioSet.Rows(e.RowIndex)
        Dim parolaPattern As String = row.Cells("Parola").Value.ToString()
        Dim numeroTransazioni = Convert.ToInt32(row.Cells("NumTransazioni").Value)

        If numeroTransazioni = 0 Then
            MessageBox.Show($"La parola '{parolaPattern}' non ha transazioni associate.", "Nessuna Transazione", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        Using frmFinanza As New FormFinanza()
            frmFinanza.ImpostaFiltroParolaDettagliato(parolaPattern, setSelezionato.MacroCategoria, setSelezionato.Categoria)
            frmFinanza.ShowDialog()
        End Using
    End Sub

    '
    ' Tab Statistiche
    '
    Private Sub CreaControlliStatistiche()
        Dim pnlStat As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(10)}
        tabStatistiche.Controls.Add(pnlStat)

        ' Chart per distribuzione categorie - AGGIUNTO PRIMA
        chartDistribuzione = New DataVisualization.Charting.Chart With {
        .Dock = DockStyle.Fill
    }

        Dim chartArea As New DataVisualization.Charting.ChartArea("MainArea")
        chartDistribuzione.ChartAreas.Add(chartArea)

        Dim series As New DataVisualization.Charting.Series("Distribuzione") With {
        .ChartType = DataVisualization.Charting.SeriesChartType.Pie
    }
        chartDistribuzione.Series.Add(series)

        ' PRIMA aggiungi il chart al panel
        pnlStat.Controls.Add(chartDistribuzione)

        ' Panel superiore per statistiche numeriche - AGGIUNTO DOPO (quindi sopra)
        Dim pnlNumeri As New Panel With {
        .Dock = DockStyle.Top,
        .Height = 100,
        .BackColor = Color.FromArgb(245, 245, 245)
    }

        lblTotaleSet = New Label With {
        .Text = "Totale Set: 0",
        .Font = New Font("Segoe UI", 14, FontStyle.Bold),
        .Location = New Point(20, 20),
        .Size = New Size(200, 30),
        .ForeColor = Color.FromArgb(33, 150, 243)
    }

        lblTotalePattern = New Label With {
        .Text = "Totale Pattern: 0",
        .Font = New Font("Segoe UI", 14, FontStyle.Bold),
        .Location = New Point(240, 20),
        .Size = New Size(200, 30),
        .ForeColor = Color.FromArgb(76, 175, 80)
    }

        lblTotaleTransazioni = New Label With {
        .Text = "Transazioni Categorizzate: 0",
        .Font = New Font("Segoe UI", 14, FontStyle.Bold),
        .Location = New Point(460, 20),
        .Size = New Size(300, 30),
        .ForeColor = Color.FromArgb(255, 152, 0)
    }

        pnlNumeri.Controls.AddRange({lblTotaleSet, lblTotalePattern, lblTotaleTransazioni})

        ' DOPO aggiungi il panel numeri (che andrà sopra al chart)
        pnlStat.Controls.Add(pnlNumeri)
    End Sub
    '
    ' Aggiorna statistiche numeriche e chart
    '
    Private Sub CreaControlliAnalisi()
        Dim pnlParole As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(10)}
        tabAnalisiParole.Controls.Add(pnlParole)

        ' PRIMA: Crea e aggiungi la DataGridView (che occuperà tutto lo spazio)
        dgvParole = New DataGridView With {
        .Dock = DockStyle.Fill,
        .AllowUserToAddRows = False,
        .AllowUserToDeleteRows = False,
        .ReadOnly = False,
        .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        .AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    }

        Dim cmbColumn As New DataGridViewComboBoxColumn With {
        .Name = "NuovoSet",
        .HeaderText = "Riassegna a Set"
    }

        dgvParole.Columns.AddRange(
        New DataGridViewTextBoxColumn() With {.Name = "Parola", .HeaderText = "Parola", .ReadOnly = True},
        New DataGridViewTextBoxColumn() With {.Name = "SetCorrente", .HeaderText = "Set Corrente", .ReadOnly = True},
        New DataGridViewTextBoxColumn() With {.Name = "Frequenza", .HeaderText = "Freq. Uso", .ReadOnly = True},
        New DataGridViewTextBoxColumn() With {.Name = "UltimoUso", .HeaderText = "Ultimo Uso", .ReadOnly = True},
        cmbColumn,
        New DataGridViewButtonColumn() With {.Name = "Riassegna", .HeaderText = "Azione", .Text = "Riassegna", .UseColumnTextForButtonValue = True}
    )

        AddHandler dgvParole.CellClick, AddressOf DgvParole_CellClick
        AddHandler dgvParole.CellDoubleClick, AddressOf dgvParole_CellDoubleClick

        ' PRIMA aggiungi la griglia
        pnlParole.Controls.Add(dgvParole)

        ' DOPO: Crea e aggiungi il panel dei filtri (andrà sopra)
        Dim pnlFiltri As New Panel With {.Dock = DockStyle.Top, .Height = 60}

        Dim lblFiltro As New Label With {.Text = "Filtra parole:", .Location = New Point(10, 15), .Size = New Size(80, 20)}
        Dim txtFiltro As New TextBox With {.Location = New Point(100, 12), .Size = New Size(200, 27)}

        Dim lblSet As New Label With {.Text = "Set:", .Location = New Point(320, 15), .Size = New Size(30, 20)}
        Dim cmbSet As New ComboBox With {.Location = New Point(360, 12), .Size = New Size(200, 27), .DropDownStyle = ComboBoxStyle.DropDownList}

        AddHandler txtFiltro.TextChanged, AddressOf FiltroParole_Changed
        AddHandler cmbSet.SelectedIndexChanged, AddressOf FiltroParole_Changed

        pnlFiltri.Controls.AddRange({lblFiltro, txtFiltro, lblSet, cmbSet})

        Me.txtFiltraParole = txtFiltro
        Me.cmbFiltraSet = cmbSet

        ' DOPO aggiungi il panel filtri (che andrà sopra alla griglia)
        pnlParole.Controls.Add(pnlFiltri)
    End Sub

    ' Gestore doppio click sulla griglia parole
    Private Sub dgvParole_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex < 0 OrElse dgvParole.Rows.Count = 0 Then Return

        Dim parolaSelezionata As String = dgvParole.Rows(e.RowIndex).Cells("Parola").Value.ToString()

        Using frmFinanza As New FormFinanza()
            frmFinanza.ImpostaFiltroParola(parolaSelezionata)
            frmFinanza.ShowDialog()
        End Using
    End Sub

    Private Sub CreaControlliExportImport()
        Dim pnlExport As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(20)}
        tabExportImport.Controls.Add(pnlExport)

        Dim lblTitolo As New Label With {
        .Text = "Backup e Template",
        .Font = New Font("Segoe UI", 16, FontStyle.Bold),
        .Dock = DockStyle.Top,
        .Height = 50,
        .TextAlign = ContentAlignment.MiddleLeft
    }
        pnlExport.Controls.Add(lblTitolo)

        ' Pulsanti esistenti in alto
        Dim btnEsportaBackup As New Button With {
        .Text = "Esporta Backup Completo",
        .Location = New Point(20, 70),
        .Size = New Size(200, 40),
        .BackColor = Color.FromArgb(33, 150, 243),
        .ForeColor = Color.White
    }
        AddHandler btnEsportaBackup.Click, AddressOf BtnEsportaBackup_Click
        pnlExport.Controls.Add(btnEsportaBackup)

        Dim btnImportaBackup As New Button With {
        .Text = "Importa Backup",
        .Location = New Point(240, 70),
        .Size = New Size(200, 40),
        .BackColor = Color.FromArgb(76, 175, 80),
        .ForeColor = Color.White
    }
        AddHandler btnImportaBackup.Click, AddressOf BtnImportaBackup_Click
        pnlExport.Controls.Add(btnImportaBackup)

        Dim btnTemplateBase As New Button With {
        .Text = "Carica Template Base",
        .Location = New Point(460, 70),
        .Size = New Size(200, 40),
        .BackColor = Color.FromArgb(255, 152, 0),
        .ForeColor = Color.White
    }
        AddHandler btnTemplateBase.Click, AddressOf BtnTemplateBase_Click
        pnlExport.Controls.Add(btnTemplateBase)

        ' --- Nuovo Label sopra i pulsanti Esporta Set ---
        Dim lblEsportaSet As New Label With {
        .Text = "Esporta Set",
        .Font = New Font("Segoe UI", 12, FontStyle.Bold),
        .Location = New Point(20, 130),
        .Size = New Size(150, 30)
    }
        pnlExport.Controls.Add(lblEsportaSet)

        ' Pulsante Esporta CSV
        Dim btnEsportaCsv As New Button With {
        .Text = "Esporta CSV",
        .Location = New Point(20, 170),
        .Size = New Size(150, 40),
        .BackColor = Color.FromArgb(54, 162, 235),
        .ForeColor = Color.White
    }
        AddHandler btnEsportaCsv.Click, AddressOf BtnEsportaCsv_Click
        pnlExport.Controls.Add(btnEsportaCsv)
    End Sub

#End Region

#Region "Caricamento Dati e TreeView"

    Private Sub CaricaSetCategorie()
        setList = New List(Of SetCategoria)

        Try
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()

                ' QUERY CORRETTA per SQLite
                Dim sql As String = "
                SELECT 
                    p.MacroCategoria,
                    p.Categoria, 
                    COUNT(DISTINCT p.ID) as NumeroPattern,
                    COUNT(DISTINCT t.ID) as NumeroTransazioni,
                    MAX(t.Data) as UltimoUtilizzo
                FROM Pattern p
                LEFT JOIN Transazioni t ON (
                    t.MacroCategoria = p.MacroCategoria AND
                    t.Categoria = p.Categoria
                )
                WHERE p.MacroCategoria IS NOT NULL 
                    AND p.MacroCategoria != ''
                    AND p.Categoria IS NOT NULL
                    AND p.Categoria != ''
                GROUP BY p.MacroCategoria, p.Categoria
                ORDER BY p.MacroCategoria, p.Categoria"

                Using cmd As New SQLiteCommand(sql, conn)
                    Using reader As SQLiteDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim setCategoria As New SetCategoria With {
                            .MacroCategoria = reader("MacroCategoria").ToString(),
                            .Categoria = reader("Categoria").ToString(),
                            .NumeroPattern = Convert.ToInt32(reader("NumeroPattern")),
                            .NumeroTransazioni = Convert.ToInt32(reader("NumeroTransazioni"))
                        }

                            If Not IsDBNull(reader("UltimoUtilizzo")) Then
                                setCategoria.UltimoUtilizzo = Convert.ToDateTime(reader("UltimoUtilizzo"))
                            End If

                            ' Carica le parole con una query separata
                            setCategoria.Parole = CaricaParolePerSet(conn, setCategoria.MacroCategoria, setCategoria.Categoria)

                            setList.Add(setCategoria)
                        End While
                    End Using
                End Using
            End Using

        Catch ex As Exception
            MessageBox.Show("Errore caricamento set categorie: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Metodo helper per caricare le parole di un set
    Private Function CaricaParolePerSet(conn As SQLiteConnection, macroCategoria As String, categoria As String) As List(Of String)
        Dim parole As New List(Of String)

        Try
            Dim sql As String = "SELECT DISTINCT Parola FROM Pattern WHERE MacroCategoria = @macro AND Categoria = @cat ORDER BY Parola"

            Using cmd As New SQLiteCommand(sql, conn)
                cmd.Parameters.AddWithValue("@macro", macroCategoria)
                cmd.Parameters.AddWithValue("@cat", categoria)

                Using reader As SQLiteDataReader = cmd.ExecuteReader()
                    While reader.Read()
                        parole.Add(reader("Parola").ToString())
                    End While
                End Using
            End Using
        Catch ex As Exception
            ' Log errore ma continua
            Debug.WriteLine($"Errore caricamento parole per {macroCategoria}>{categoria}: {ex.Message}")
        End Try

        Return parole
    End Function
    Private Sub PopolaTreeView(Optional filtro As String = "")
        treeViewSet.ShowNodeToolTips = True
        treeViewSet.Nodes.Clear()

        ' Applica filtro...
        Dim setFiltrati = setList.AsEnumerable()
        If Not String.IsNullOrWhiteSpace(filtro) Then
            Dim filtroLower = filtro.ToLower()
            setFiltrati = setFiltrati.Where(Function(s)
                                                Return s.MacroCategoria.ToLower().Contains(filtroLower) OrElse
                   s.Categoria.ToLower().Contains(filtroLower) OrElse
                   s.NomeCompleto.ToLower().Contains(filtroLower) OrElse
                   (s.Parole IsNot Nothing AndAlso s.Parole.Any(Function(p) p.ToLower().Contains(filtroLower)))
                                            End Function)
        End If

        ' Raggruppa e crea nodi
        For Each gruppoMacro In setFiltrati.GroupBy(Function(s) s.MacroCategoria).OrderBy(Function(g) g.Key)
            Dim macroNome = gruppoMacro.Key
            Dim nodoMacro As New TreeNode(macroNome) With {
            .ForeColor = Color.FromArgb(33, 150, 243),
            .NodeFont = New Font(treeViewSet.Font, FontStyle.Bold),
            .ToolTipText = macroNome
        }
            For Each s In gruppoMacro.OrderBy(Function(x) x.Categoria)
                Dim testo = $"{s.Categoria} ({s.NumeroPattern}p, {s.NumeroTransazioni}t)"
                Dim nodoCat As New TreeNode(testo) With {
                .Tag = s,
                .ForeColor = If(s.NumeroPattern = 0, Color.Red, If(s.NumeroTransazioni = 0, Color.Orange, Color.Black)),
                .ToolTipText = testo
            }
                nodoMacro.Nodes.Add(nodoCat)
            Next
            treeViewSet.Nodes.Add(nodoMacro)
        Next

        If Not String.IsNullOrWhiteSpace(filtro) OrElse treeViewSet.Nodes.Count <= 5 Then
            treeViewSet.ExpandAll()
        End If

        If treeViewSet.Nodes.Count = 0 AndAlso Not String.IsNullOrWhiteSpace(filtro) Then
            Dim nodoVuoto As New TreeNode($"Nessun risultato per '{filtro}'") With {.ForeColor = Color.Gray}
            treeViewSet.Nodes.Add(nodoVuoto)
        End If
    End Sub

#End Region

#Region "Eventi Controlli"

    Private Sub treeViewSet_AfterSelect(sender As Object, e As TreeViewEventArgs) Handles treeViewSet.AfterSelect
        If e.Node IsNot Nothing AndAlso e.Node.Tag IsNot Nothing Then
            setSelezionato = CType(e.Node.Tag, SetCategoria)
            CaricaDettaglioSet()
            AbilitaPulsantiSet(True)
        Else
            setSelezionato = Nothing
            PulisciDettaglioSet()
            AbilitaPulsantiSet(False)
        End If
    End Sub
    Private Sub AbilitaPulsantiSet(abilita As Boolean)
        btnSalvaSet.Enabled = abilita
        btnEliminaSet.Enabled = abilita
        btnUnisciSet.Enabled = abilita
    End Sub
    Private Sub btnRicerca_Click(sender As Object, e As EventArgs) Handles btnRicerca.Click
        PopolaTreeView(txtRicerca.Text)
    End Sub
    Private Sub btnResetFiltro_Click(sender As Object, e As EventArgs) Handles btnResetFiltro.Click
        txtRicerca.Clear()
        PopolaTreeView("")
    End Sub
    Private Function TrovaTextBox(container As Control, nomeTextBox As String) As TextBox
        For Each ctrl As Control In container.Controls
            If TypeOf ctrl Is TextBox AndAlso ctrl.Name = nomeTextBox Then
                Return CType(ctrl, TextBox)
            End If

            If ctrl.HasChildren Then
                Dim risultato = TrovaTextBox(ctrl, nomeTextBox)
                If risultato IsNot Nothing Then
                    Return risultato
                End If
            End If
        Next

        Return Nothing
    End Function
    Private Sub txtRicerca_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtRicerca.KeyPress
        If e.KeyChar = Chr(13) Then ' Enter
            PopolaTreeView(txtRicerca.Text)
        End If
    End Sub

    Private Sub btnNuovoSet_Click(sender As Object, e As EventArgs) Handles btnNuovoSet.Click
        Using frm As New FormNuovoSet()
            If frm.ShowDialog() = DialogResult.OK Then
                CaricaSetCategorie()
                PopolaTreeView()
                AggiornaStatistiche()
            End If
        End Using
    End Sub

    Private Sub btnTrovaDuplicati_Click(sender As Object, e As EventArgs) Handles btnTrovaDuplicati.Click
        TrovaDuplicati()
    End Sub

    Private Sub btnStrumentiPulizia_Click(sender As Object, e As EventArgs) Handles btnStrumentiPulizia.Click
        MostraStrumentiPulizia()
    End Sub

    Private Sub BtnSalvaSet_Click(sender As Object, e As EventArgs)
        If setSelezionato Is Nothing Then Return

        SalvaModificheSet()
    End Sub

    Private Sub BtnEliminaSet_Click(sender As Object, e As EventArgs)
        If setSelezionato Is Nothing Then Return

        EliminaSet()
    End Sub

    Private Sub BtnUnisciSet_Click(sender As Object, e As EventArgs)
        If setSelezionato Is Nothing Then Return

        UnisciSet()
    End Sub

#End Region

#Region "Gestione Dettaglio Set"

    Private Sub CaricaDettaglioSet()
        If setSelezionato Is Nothing Then Return

        ' Popola campi di modifica (solo 2 campi ora)
        txtMacroCategoria.Text = setSelezionato.MacroCategoria
        txtCategoria.Text = setSelezionato.Categoria

        ' Popola griglia pattern
        dgvDettaglioSet.Rows.Clear()

        Try
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()

                Dim sql As String = "
                SELECT 
                    p.Parola,
                    p.Peso,
                    p.Fonte,
                    COUNT(DISTINCT t.ID) as NumTransazioni
                FROM Pattern p
                LEFT JOIN Transazioni t ON (
                    LOWER(t.Descrizione) LIKE '%' || LOWER(p.Parola) || '%'
                )
                WHERE p.MacroCategoria = @macro 
                    AND p.Categoria = @cat
                GROUP BY p.Parola, p.Peso, p.Fonte
                ORDER BY p.Peso DESC, p.Parola"

                Using cmd As New SQLiteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@macro", setSelezionato.MacroCategoria)
                    cmd.Parameters.AddWithValue("@cat", setSelezionato.Categoria)

                    Using reader As SQLiteDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            dgvDettaglioSet.Rows.Add(
                            reader("Parola").ToString(),
                            reader("Peso").ToString(),
                            reader("Fonte").ToString(),
                            reader("NumTransazioni").ToString()
                        )
                        End While
                    End Using
                End Using
            End Using

        Catch ex As Exception
            MessageBox.Show("Errore caricamento dettaglio set: " & ex.Message)
        End Try
    End Sub

    Private Sub PulisciDettaglioSet()
        txtMacroCategoria.Clear()
        txtCategoria.Clear()
        ' RIMUOVI txtSottoCategoria.Clear()
        dgvDettaglioSet.Rows.Clear()
    End Sub

    Private Sub SalvaModificheSet()
        If String.IsNullOrWhiteSpace(txtMacroCategoria.Text) OrElse
       String.IsNullOrWhiteSpace(txtCategoria.Text) Then
            MessageBox.Show("Tutti i campi sono obbligatori!", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        Try
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Using trans = conn.BeginTransaction()

                    ' Aggiorna Pattern
                    Dim sqlPattern As String = "
                    UPDATE Pattern SET 
                        MacroCategoria = @nuovaMacro,
                        Categoria = @nuovaCat
                    WHERE MacroCategoria = @vecchiaMacro 
                        AND Categoria = @vecchiaCat"

                    Using cmd As New SQLiteCommand(sqlPattern, conn, trans)
                        cmd.Parameters.AddWithValue("@nuovaMacro", txtMacroCategoria.Text)
                        cmd.Parameters.AddWithValue("@nuovaCat", txtCategoria.Text)
                        cmd.Parameters.AddWithValue("@vecchiaMacro", setSelezionato.MacroCategoria)
                        cmd.Parameters.AddWithValue("@vecchiaCat", setSelezionato.Categoria)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Aggiorna Transazioni
                    Dim sqlTrans As String = "
                    UPDATE Transazioni SET
                        MacroCategoria = @nuovaMacro,
                        Categoria = @nuovaCat
                    WHERE MacroCategoria = @vecchiaMacro
                        AND Categoria = @vecchiaCat"

                    Using cmd As New SQLiteCommand(sqlTrans, conn, trans)
                        cmd.Parameters.AddWithValue("@nuovaMacro", txtMacroCategoria.Text)
                        cmd.Parameters.AddWithValue("@nuovaCat", txtCategoria.Text)
                        cmd.Parameters.AddWithValue("@vecchiaMacro", setSelezionato.MacroCategoria)
                        cmd.Parameters.AddWithValue("@vecchiaCat", setSelezionato.Categoria)
                        cmd.ExecuteNonQuery()
                    End Using

                    trans.Commit()
                End Using
            End Using

            MessageBox.Show("Set modificato con successo!", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information)
            CaricaSetCategorie()
            PopolaTreeView()
            AggiornaStatistiche()

        Catch ex As Exception
            MessageBox.Show("Errore durante il salvataggio: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

#End Region

#Region "Funzioni Avanzate - Duplicati e Pulizia"

    Private Sub TrovaDuplicati()
        Dim duplicati As New List(Of (Set1 As SetCategoria, Set2 As SetCategoria, Similitudine As Double))

        ' Confronta ogni set con tutti gli altri
        For i = 0 To setList.Count - 2
            For j = i + 1 To setList.Count - 1
                Dim set1 = setList(i)
                Dim set2 = setList(j)
                Dim similitudine = CalcolaSimilitudine(set1, set2)

                If similitudine >= 0.6 Then ' Soglia di similitudine
                    duplicati.Add((set1, set2, similitudine))
                End If
            Next
        Next

        If duplicati.Count = 0 Then
            MessageBox.Show("Non sono stati trovati set duplicati o simili.", "Ricerca Duplicati", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' Mostra form con i duplicati trovati
        Using frmDuplicati As New FormDuplicati(duplicati)
            If frmDuplicati.ShowDialog() = DialogResult.OK Then
                CaricaSetCategorie()
                PopolaTreeView()
                AggiornaStatistiche()
            End If
        End Using
    End Sub

    Private Function CalcolaSimilitudine(set1 As SetCategoria, set2 As SetCategoria) As Double
        Dim punteggio As Double = 0

        ' Similitudine nomi categorie (60% del punteggio - aumentato perché abbiamo solo 2 livelli)
        If String.Compare(set1.MacroCategoria, set2.MacroCategoria, True) = 0 Then punteggio += 0.3
        If String.Compare(set1.Categoria, set2.Categoria, True) = 0 Then punteggio += 0.3

        ' Similitudine Levenshtein per nomi simili (20% del punteggio)
        Dim distMacro = 1.0 - (CalcolaDistanzaLevenshtein(set1.MacroCategoria, set2.MacroCategoria) / Math.Max(set1.MacroCategoria.Length, set2.MacroCategoria.Length))
        Dim distCat = 1.0 - (CalcolaDistanzaLevenshtein(set1.Categoria, set2.Categoria) / Math.Max(set1.Categoria.Length, set2.Categoria.Length))

        punteggio += (distMacro + distCat) * 0.1

        ' Similitudine parole condivise (20% del punteggio)
        Dim paroleComuni = set1.Parole.Intersect(set2.Parole, StringComparer.OrdinalIgnoreCase).Count()
        Dim totaleParole = set1.Parole.Count + set2.Parole.Count - paroleComuni
        If totaleParole > 0 Then
            punteggio += (paroleComuni / totaleParole) * 0.2
        End If

        Return punteggio
    End Function


    Private Function CalcolaDistanzaLevenshtein(s1 As String, s2 As String) As Integer
        If String.IsNullOrEmpty(s1) Then Return If(String.IsNullOrEmpty(s2), 0, s2.Length)
        If String.IsNullOrEmpty(s2) Then Return s1.Length

        Dim matrix(s1.Length, s2.Length) As Integer

        For i = 0 To s1.Length
            matrix(i, 0) = i
        Next
        For j = 0 To s2.Length
            matrix(0, j) = j
        Next

        For i = 1 To s1.Length
            For j = 1 To s2.Length
                Dim cost = If(s1(i - 1) = s2(j - 1), 0, 1)
                matrix(i, j) = Math.Min(Math.Min(matrix(i - 1, j) + 1, matrix(i, j - 1) + 1), matrix(i - 1, j - 1) + cost)
            Next
        Next

        Return matrix(s1.Length, s2.Length)
    End Function

    Private Sub MostraStrumentiPulizia()
        Dim risultato As String = ""

        ' Trova set orfani (senza pattern)
        Dim setOrfani = setList.Where(Function(s) s.NumeroPattern = 0).ToList()
        risultato &= $"🔍 Set senza pattern: {setOrfani.Count}" & vbCrLf

        ' Trova set senza transazioni
        Dim setSenzaTransazioni = setList.Where(Function(s) s.NumeroTransazioni = 0).ToList()
        risultato &= $"💤 Set senza transazioni: {setSenzaTransazioni.Count}" & vbCrLf

        ' Trova pattern orfani
        Dim patternOrfani = TrovaPatternOrfani()
        risultato &= $"⚠️ Pattern senza transazioni: {patternOrfani}" & vbCrLf & vbCrLf

        risultato &= "Vuoi procedere con la pulizia automatica?"

        If MessageBox.Show(risultato, "Strumenti di Pulizia", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            EseguiPuliziaAutomatica(setOrfani, setSenzaTransazioni)
        End If
    End Sub

    Private Function TrovaPatternOrfani() As Integer
        Try
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Dim sql As String = "
                    SELECT COUNT(*) FROM Pattern p
                    WHERE NOT EXISTS (
                        SELECT 1 FROM Transazioni t 
                        WHERE LOWER(t.Descrizione) LIKE '%' || LOWER(p.Parola) || '%'
                    )"

                Using cmd As New SQLiteCommand(sql, conn)
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        Catch
            Return 0
        End Try
    End Function

    Private Sub EseguiPuliziaAutomatica(setOrfani As List(Of SetCategoria), setSenzaTransazioni As List(Of SetCategoria))
        Try
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Using trans = conn.BeginTransaction()

                    Dim eliminati As Integer = 0

                    ' Elimina set completamente orfani (senza pattern E senza transazioni)
                    For Each setOrfano In setOrfani.Where(Function(s) s.NumeroTransazioni = 0)
                        Dim sql As String = "DELETE FROM Pattern WHERE MacroCategoria = @macro AND Categoria = @cat"
                        Using cmd As New SQLiteCommand(sql, conn, trans)
                            cmd.Parameters.AddWithValue("@macro", setOrfano.MacroCategoria)
                            cmd.Parameters.AddWithValue("@cat", setOrfano.Categoria)
                            eliminati += cmd.ExecuteNonQuery()
                        End Using
                    Next

                    ' Elimina pattern orfani
                    Dim sqlPatternOrfani As String = "
                        DELETE FROM Pattern 
                        WHERE NOT EXISTS (
                            SELECT 1 FROM Transazioni t 
                            WHERE LOWER(t.Descrizione) LIKE '%' || LOWER(Pattern.Parola) || '%'
                        )"
                    Using cmd As New SQLiteCommand(sqlPatternOrfani, conn, trans)
                        eliminati += cmd.ExecuteNonQuery()
                    End Using

                    trans.Commit()
                    MessageBox.Show($"Pulizia completata! Eliminati {eliminati} elementi.", "Pulizia", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End Using
            End Using

            CaricaSetCategorie()
            PopolaTreeView()
            AggiornaStatistiche()

        Catch ex As Exception
            MessageBox.Show("Errore durante la pulizia: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub EliminaSet()
        Dim numTransazioni = setSelezionato.NumeroTransazioni

        Dim messaggio As String = $"Eliminare il set '{setSelezionato.NomeCompleto}'?" & vbCrLf &
                             $"Questo eliminerà {setSelezionato.NumeroPattern} pattern."

        If numTransazioni > 0 Then
            messaggio &= vbCrLf & $"ATTENZIONE: {numTransazioni} transazioni perderanno la categorizzazione!"
        End If

        If MessageBox.Show(messaggio, "Conferma Eliminazione", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then
            Return
        End If

        Try
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Using trans = conn.BeginTransaction()

                    ' Rimuovi categorizzazione dalle transazioni
                    Dim sqlTrans As String = "
                    UPDATE Transazioni SET 
                        MacroCategoria = '',
                        Categoria = ''
                    WHERE MacroCategoria = @macro AND Categoria = @cat"

                    Using cmd As New SQLiteCommand(sqlTrans, conn, trans)
                        cmd.Parameters.AddWithValue("@macro", setSelezionato.MacroCategoria)
                        cmd.Parameters.AddWithValue("@cat", setSelezionato.Categoria)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Elimina pattern
                    Dim sqlPattern As String = "DELETE FROM Pattern WHERE MacroCategoria = @macro AND Categoria = @cat"
                    Using cmd As New SQLiteCommand(sqlPattern, conn, trans)
                        cmd.Parameters.AddWithValue("@macro", setSelezionato.MacroCategoria)
                        cmd.Parameters.AddWithValue("@cat", setSelezionato.Categoria)
                        cmd.ExecuteNonQuery()
                    End Using

                    trans.Commit()
                End Using
            End Using

            MessageBox.Show("Set eliminato con successo!", "Eliminazione", MessageBoxButtons.OK, MessageBoxIcon.Information)
            CaricaSetCategorie()
            PopolaTreeView()
            AggiornaStatistiche()
            PulisciDettaglioSet()

        Catch ex As Exception
            MessageBox.Show("Errore durante l'eliminazione: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub UnisciSet()
        ' Mostra form per selezione set destinazione
        Using frmUnisci As New FormUnisciSet(setList, setSelezionato)
            If frmUnisci.ShowDialog() = DialogResult.OK Then
                CaricaSetCategorie()
                PopolaTreeView()
                AggiornaStatistiche()
            End If
        End Using
    End Sub

#End Region

#Region "Statistiche e Analisi"

    Private Sub AggiornaStatistiche()
        Try
            lblTotaleSet.Text = $"Totale Set: {setList.Count}"
            lblTotalePattern.Text = $"Totale Pattern: {setList.Sum(Function(s) s.NumeroPattern)}"
            lblTotaleTransazioni.Text = $"Transazioni Categorizzate: {setList.Sum(Function(s) s.NumeroTransazioni)}"

            AggiornaGraficoDistribuzione()
            CaricaAnalisiParole()

        Catch ex As Exception
            MessageBox.Show("Errore aggiornamento statistiche: " & ex.Message)
        End Try
    End Sub

    Private Sub AggiornaGraficoDistribuzione()
        chartDistribuzione.Series("Distribuzione").Points.Clear()

        Dim gruppi = setList.GroupBy(Function(s) s.MacroCategoria).
                           OrderByDescending(Function(g) g.Sum(Function(s) s.NumeroTransazioni)).
                           Take(10) ' Top 10 macro categorie

        For Each gruppo In gruppi
            Dim totaleTransazioni = gruppo.Sum(Function(s) s.NumeroTransazioni)
            If totaleTransazioni > 0 Then
                chartDistribuzione.Series("Distribuzione").Points.AddXY(gruppo.Key, totaleTransazioni)
            End If
        Next

        chartDistribuzione.Titles.Clear()
        chartDistribuzione.Titles.Add("Distribuzione Transazioni per Macro Categoria")
    End Sub

    Private Sub CaricaAnalisiParole()
        Try
            ' Popola combo filtro set
            cmbFiltraSet.Items.Clear()
            cmbFiltraSet.Items.Add("Tutti i Set")
            For Each item In setList.OrderBy(Function(s) s.NomeCompleto)
                cmbFiltraSet.Items.Add(item.NomeCompleto)
            Next
            cmbFiltraSet.SelectedIndex = 0

            AggiornaAnalisiParole()

        Catch ex As Exception
            MessageBox.Show("Errore caricamento analisi parole: " & ex.Message)
        End Try
    End Sub


    Private Sub AggiornaAnalisiParole()
        dgvParole.Rows.Clear()

        Try
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()

                Dim sql As String = "
                SELECT 
                    p.Parola,
                    p.MacroCategoria || ' > ' || p.Categoria as SetCompleto,
                    COUNT(DISTINCT t.ID) as Frequenza,
                    MAX(t.Data) as UltimoUso,
                    p.MacroCategoria,
                    p.Categoria
                FROM Pattern p
                LEFT JOIN Transazioni t ON (LOWER(t.Descrizione) LIKE '%' || LOWER(p.Parola) || '%')
                WHERE 1=1"

                ' Applica filtri
                Dim filtroParola = txtFiltraParole.Text.Trim()
                If Not String.IsNullOrEmpty(filtroParola) Then
                    sql &= " AND LOWER(p.Parola) LIKE @filtroParola"
                End If

                If cmbFiltraSet.SelectedIndex > 0 Then
                    Dim setSelezionato = cmbFiltraSet.SelectedItem.ToString()
                    ' CORREZIONE: ora aspetta solo 2 parti (MacroCategoria > Categoria)
                    Dim parti = setSelezionato.Split(New String() {" > "}, StringSplitOptions.None)
                    If parti.Length = 2 Then
                        sql &= " AND p.MacroCategoria = @macro AND p.Categoria = @cat"
                    End If
                End If

                sql &= " GROUP BY p.Parola, p.MacroCategoria, p.Categoria ORDER BY Frequenza DESC, p.Parola"

                Using cmd As New SQLiteCommand(sql, conn)
                    If Not String.IsNullOrEmpty(filtroParola) Then
                        cmd.Parameters.AddWithValue("@filtroParola", "%" & filtroParola.ToLower() & "%")
                    End If

                    If cmbFiltraSet.SelectedIndex > 0 Then
                        Dim setSelezionato = cmbFiltraSet.SelectedItem.ToString()
                        Dim parti = setSelezionato.Split(New String() {" > "}, StringSplitOptions.None)
                        If parti.Length = 2 Then
                            cmd.Parameters.AddWithValue("@macro", parti(0))
                            cmd.Parameters.AddWithValue("@cat", parti(1))
                        End If
                    End If

                    Using reader As SQLiteDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim ultimoUso = If(IsDBNull(reader("UltimoUso")), "", Convert.ToDateTime(reader("UltimoUso")).ToString("dd/MM/yyyy"))

                            dgvParole.Rows.Add(
                            reader("Parola").ToString(),
                            reader("SetCompleto").ToString(),
                            reader("Frequenza").ToString(),
                            ultimoUso,
                            "", ' Colonna per nuovo set (sarà popolata dinamicamente)
                            "Riassegna"
                        )
                        End While
                    End Using
                End Using
            End Using

            ' Popola combo colonne per riassegnazione
            PopolaComboNuovoSet()

        Catch ex As Exception
            MessageBox.Show("Errore aggiornamento analisi parole: " & ex.Message)
        End Try
    End Sub

    Private Sub PopolaComboNuovoSet()
        Dim cmbColumn As DataGridViewComboBoxColumn = CType(dgvParole.Columns("NuovoSet"), DataGridViewComboBoxColumn)
        cmbColumn.Items.Clear()
        cmbColumn.Items.Add("") ' Opzione vuota

        For Each item In setList.OrderBy(Function(s) s.NomeCompleto)
            cmbColumn.Items.Add(item.NomeCompleto)
        Next
    End Sub

    Private Sub FiltroParole_Changed(sender As Object, e As EventArgs)
        ' Aggiorna la lista parole in base ai filtri applicati
        AggiornaAnalisiParole()
    End Sub

    Private Sub DgvParole_CellClick(sender As Object, e As DataGridViewCellEventArgs)
        If e.RowIndex >= 0 AndAlso e.ColumnIndex = dgvParole.Columns("Riassegna").Index Then
            RiassegnaParola(e.RowIndex)
        End If
    End Sub

    Private Sub RiassegnaParola(rowIndex As Integer)
        Dim row = dgvParole.Rows(rowIndex)
        Dim parola = row.Cells("Parola").Value.ToString()
        Dim nuovoSetCompleto = row.Cells("NuovoSet").Value?.ToString()

        If String.IsNullOrEmpty(nuovoSetCompleto) Then
            MessageBox.Show("Seleziona un set di destinazione!", "Attenzione", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        ' CORREZIONE: ora aspetta solo 2 parti (MacroCategoria > Categoria)
        Dim parti = nuovoSetCompleto.Split(New String() {" > "}, StringSplitOptions.None)
        If parti.Length <> 2 Then Return

        Try
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()

                Dim sql As String = "UPDATE Pattern SET MacroCategoria = @macro, Categoria = @cat WHERE Parola = @parola"
                Using cmd As New SQLiteCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@macro", parti(0))
                    cmd.Parameters.AddWithValue("@cat", parti(1))
                    cmd.Parameters.AddWithValue("@parola", parola)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            MessageBox.Show($"Parola '{parola}' riassegnata con successo!", "Riassegnazione", MessageBoxButtons.OK, MessageBoxIcon.Information)
            CaricaSetCategorie()
            AggiornaAnalisiParole()
            AggiornaStatistiche()

        Catch ex As Exception
            MessageBox.Show("Errore durante la riassegnazione: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

#End Region

#Region "Export/Import e Backup"

    Private Sub BtnEsportaCsv_Click(sender As Object, e As EventArgs)
        Try
            ' Percorso automatico: Cartella Export/Set_Pattern sotto la cartella applicativo
            Dim percorsoBase As String = IO.Path.GetDirectoryName(Application.ExecutablePath)
            Dim percorsoExport As String = IO.Path.Combine(percorsoBase, "Export", "Pattern", "CSV")

            If Not IO.Directory.Exists(percorsoExport) Then
                IO.Directory.CreateDirectory(percorsoExport)
            End If

            Dim nomeFile As String = $"set_categorie_{Date.Now:yyyyMMdd_HHmmss}.csv"
            Dim percorsoCompleto As String = IO.Path.Combine(percorsoExport, nomeFile)

            ' Esporta
            EsportaSetInCsv(percorsoCompleto)

            MessageBox.Show($"Set categorie esportati con successo in CSV!" & vbCrLf &
                        $"File: {nomeFile}" & vbCrLf &
                        $"Percorso: {percorsoExport}",
                        "Esportazione CSV Completata", MessageBoxButtons.OK, MessageBoxIcon.Information)

            ' Chiedi se aprire il file
            If MessageBox.Show("Vuoi aprire il file esportato?", "Apri File", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
                Process.Start(New ProcessStartInfo(percorsoCompleto) With {.UseShellExecute = True})
            End If

        Catch ex As Exception
            MessageBox.Show("Errore durante l'esportazione CSV: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub EseguiEsportazioneCsv()
        Using sfd As New SaveFileDialog() With {
        .Title = "Esporta Set Categorie in CSV",
        .Filter = "File CSV (*.csv)|*.csv",
        .FileName = $"set_categorie_{Date.Now:yyyyMMdd_HHmmss}.csv"
    }
            If sfd.ShowDialog() <> DialogResult.OK Then Return

            EsportaSetInCsv(sfd.FileName)

            MessageBox.Show($"Set categorie esportati con successo in CSV!" & vbCrLf &
                       $"File: {IO.Path.GetFileName(sfd.FileName)}",
                       "Esportazione CSV Completata", MessageBoxButtons.OK, MessageBoxIcon.Information)

            ApriFileEsportato(sfd.FileName)
        End Using
    End Sub

    Private Sub EsportaSetInCsv(filePath As String)
        Try
            Using writer As New IO.StreamWriter(filePath, False, System.Text.Encoding.UTF8)
                ' Intestazioni CSV con separatore punto e virgola
                writer.WriteLine("ID;PAROLA;MacroCategoria;Categoria;Necessita;Frequenza;Stagionalita;Fonte;Peso;NumeroPattern;NumeroTransazioni;UltimoUtilizzo")

                Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                    conn.Open()

                    Dim sql As String = "
    SELECT 
        p.ID,
        p.Parola,
        p.MacroCategoria,
        p.Categoria,
        p.Necessita,
        p.Frequenza,
        p.Stagionalita,
        p.Fonte,
        p.Peso,
        -- Numero di pattern nel set
        (SELECT COUNT(*) FROM Pattern p2 
         WHERE p2.MacroCategoria = p.MacroCategoria 
           AND p2.Categoria = p.Categoria) AS NumeroPattern,
        -- Numero di transazioni classificate con quella parola
        (SELECT COUNT(DISTINCT t.ID) FROM Transazioni t 
         WHERE LOWER(t.Descrizione) LIKE '%' || LOWER(p.Parola) || '%') AS NumeroTransazioni,
        -- Ultimo utilizzo **per quella parola**
        (SELECT MAX(t2.Data) FROM Transazioni t2 
         WHERE LOWER(t2.Descrizione) LIKE '%' || LOWER(p.Parola) || '%') AS UltimoUtilizzo
    FROM Pattern p
    WHERE p.MacroCategoria <> '' 
      AND p.Categoria <> ''
    ORDER BY p.MacroCategoria, p.Categoria, p.Parola;"

                    Using cmd As New SQLiteCommand(sql, conn)
                        Using reader As SQLiteDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim id = reader("ID").ToString()
                                Dim parola = reader("Parola").ToString()
                                Dim macroCategoria = reader("MacroCategoria").ToString()
                                Dim categoria = reader("Categoria").ToString()
                                Dim necessita = If(IsDBNull(reader("Necessita")), "", reader("Necessita").ToString())
                                Dim frequenza = If(IsDBNull(reader("Frequenza")), "", reader("Frequenza").ToString())
                                Dim stagionalita = If(IsDBNull(reader("Stagionalita")), "", reader("Stagionalita").ToString())
                                Dim fonte = If(IsDBNull(reader("Fonte")), "", reader("Fonte").ToString())
                                Dim peso = reader("Peso").ToString()
                                Dim numeroPattern = reader("NumeroPattern").ToString()
                                Dim numeroTransazioni = reader("NumeroTransazioni").ToString()
                                Dim ultimoUtilizzo = If(IsDBNull(reader("UltimoUtilizzo")),
                                                   "Mai utilizzato",
                                                   Convert.ToDateTime(reader("UltimoUtilizzo")).ToString("dd/MM/yyyy"))

                                ' Costruisci la riga CSV usando punto e virgola e escape campi
                                Dim rigaCsv = String.Join(";", {
                                EscapaCampoCsv(id),
                                EscapaCampoCsv(parola),
                                EscapaCampoCsv(macroCategoria),
                                EscapaCampoCsv(categoria),
                                EscapaCampoCsv(necessita),
                                EscapaCampoCsv(frequenza),
                                EscapaCampoCsv(stagionalita),
                                EscapaCampoCsv(fonte),
                                peso,
                                numeroPattern,
                                numeroTransazioni,
                                EscapaCampoCsv(ultimoUtilizzo)
                            })
                                writer.WriteLine(rigaCsv)
                            End While
                        End Using
                    End Using
                End Using
            End Using

            MessageBox.Show($"Esportazione completata in {filePath}", "Successo", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            MessageBox.Show("Errore durante l'esportazione CSV: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    ' Funzione di escape per campi CSV
    Private Function EscapaCampoCsv(campo As String) As String
        If String.IsNullOrEmpty(campo) Then Return ""
        If campo.Contains(";") OrElse campo.Contains("""") OrElse campo.Contains(vbCrLf) OrElse campo.Contains(vbLf) Then
            Return """" & campo.Replace("""", """""") & """"
        Else
            Return campo
        End If
    End Function


    Private Sub EsportaSetInCsvConEstensioneExcel(filePath As String)
        ' Crea le cartelle se non esistono
        Dim directoryPath = IO.Path.GetDirectoryName(filePath)
        If Not IO.Directory.Exists(directoryPath) Then
            IO.Directory.CreateDirectory(directoryPath)
        End If

        Using writer As New IO.StreamWriter(filePath, False, System.Text.Encoding.UTF8)
            ' Intestazioni (con tabulazione come separatore)
            writer.WriteLine("ID" & vbTab & "PAROLA" & vbTab & "MacroCategoria" & vbTab & "Categoria" & vbTab & "Necessita" & vbTab & "Frequenza" & vbTab & "Stagionalita" & vbTab & "Fonte" & vbTab & "Peso" & vbTab & "Numero Pattern" & vbTab & "Numero Transazioni" & vbTab & "Ultimo Utilizzo")

            Try
                Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                    conn.Open()

                    ' Stessa query del metodo CSV
                    Dim sql As String = "
                    SELECT 
                        p.ID,
                        p.Parola,
                        p.MacroCategoria,
                        p.Categoria,
                        p.Necessita,
                        p.Frequenza,
                        p.Stagionalita,
                        p.Fonte,
                        p.Peso,
                        COUNT(DISTINCT p2.ID) as NumeroPattern,
                        COUNT(DISTINCT t.ID) as NumeroTransazioni,
                        MAX(t.Data) as UltimoUtilizzo
                    FROM Pattern p
                    LEFT JOIN Pattern p2 ON (
                        p2.MacroCategoria = p.MacroCategoria AND
                        p2.Categoria = p.Categoria AND  
                    )
                    LEFT JOIN Transazioni t ON (
                        LOWER(t.Descrizione) LIKE '%' || LOWER(p.Parola) || '%'
                    )
                    WHERE p.MacroCategoria IS NOT NULL 
                        AND p.MacroCategoria != ''
                        AND p.Categoria IS NOT NULL
                        AND p.Categoria != ''
                    GROUP BY p.ID, p.Parola, p.MacroCategoria, p.Categoria, p.Necessita, p.Frequenza, p.Stagionalita, p.Fonte, p.Peso
                    ORDER BY p.MacroCategoria, p.Categoria, p.Parola"

                    Using cmd As New SQLiteCommand(sql, conn)
                        Using reader As SQLiteDataReader = cmd.ExecuteReader()
                            While reader.Read()
                                Dim id = reader("ID").ToString()
                                Dim parola = reader("Parola").ToString()
                                Dim macroCategoria = reader("MacroCategoria").ToString()
                                Dim categoria = reader("Categoria").ToString()
                                'Dim sottoCategoria = reader("SottoCategoria").ToString()
                                Dim necessita = If(IsDBNull(reader("Necessita")), "", reader("Necessita").ToString())
                                Dim frequenza = If(IsDBNull(reader("Frequenza")), "", reader("Frequenza").ToString())
                                Dim stagionalita = If(IsDBNull(reader("Stagionalita")), "", reader("Stagionalita").ToString())
                                Dim fonte = If(IsDBNull(reader("Fonte")), "", reader("Fonte").ToString())
                                Dim peso = reader("Peso").ToString()
                                Dim numeroPattern = reader("NumeroPattern").ToString()
                                Dim numeroTransazioni = reader("NumeroTransazioni").ToString()
                                Dim ultimoUtilizzo = If(IsDBNull(reader("UltimoUtilizzo")), "Mai utilizzato", Convert.ToDateTime(reader("UltimoUtilizzo")).ToString("dd/MM/yyyy"))

                                writer.WriteLine($"{id}" & vbTab &
                                           $"{parola}" & vbTab &
                                           $"{macroCategoria}" & vbTab &
                                           $"{categoria}" & vbTab &
                                           $"{necessita}" & vbTab &
                                           $"{frequenza}" & vbTab &
                                           $"{stagionalita}" & vbTab &
                                           $"{fonte}" & vbTab &
                                           $"{peso}" & vbTab &
                                           $"{numeroPattern}" & vbTab &
                                           $"{numeroTransazioni}" & vbTab &
                                           $"{ultimoUtilizzo}")
                            End While
                        End Using
                    End Using
                End Using

            Catch ex As Exception
                MessageBox.Show("Errore durante l'estrazione dati per export Excel: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Using
    End Sub

    Private Sub ApriFileEsportato(filePath As String)
        If MessageBox.Show("Vuoi aprire il file esportato?", "Apri File", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = DialogResult.Yes Then
            Try
                Process.Start(New ProcessStartInfo(filePath) With {.UseShellExecute = True})
            Catch ex As Exception
                MessageBox.Show($"Non è possibile aprire il file automaticamente: {ex.Message}" & vbCrLf &
                           $"Puoi trovarlo in: {filePath}", "Apertura File", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End Try
        End If
    End Sub
    Private Sub BtnEsportaSetExcel_Click(sender As Object, e As EventArgs)
        Try
            ' Finestra di dialogo per scegliere il formato
            Dim sceltaFormato As DialogResult = MessageBox.Show(
            "Scegli il formato di esportazione:" & vbCrLf & vbCrLf &
            "SÌ = Excel nativo (.xlsx) - Richiede EPPlus" & vbCrLf &
            "NO = CSV (.csv) - Compatibile con Excel, più veloce" & vbCrLf &
            "ANNULLA = Annulla esportazione",
            "Formato Esportazione",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2)

            Select Case sceltaFormato
                Case DialogResult.Yes
                    ' Esportazione Excel nativo

                Case DialogResult.No
                    ' Esportazione CSV
                    EseguiEsportazioneCsv()

                Case DialogResult.Cancel
                    ' Annulla - non fare niente
                    Return
            End Select

        Catch ex As Exception
            MessageBox.Show("Errore durante l'esportazione: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BtnEsportaBackup_Click(sender As Object, e As EventArgs)
        Try
            Using sfd As New SaveFileDialog() With {
                .Title = "Salva Backup Pattern e Set",
                .Filter = "File JSON (*.json)|*.json",
                .FileName = $"backup_pattern_{Date.Now:yyyyMMdd_HHmmss}.json"
            }
                If sfd.ShowDialog() <> DialogResult.OK Then Return

                Dim backup As New Dictionary(Of String, Object)
                backup("DataCreazione") = Date.Now
                backup("Versione") = "1.0"
                backup("Set") = setList
                backup("Pattern") = EsportaPattern()

                Dim json = JsonSerializer.Serialize(backup, New JsonSerializerOptions With {.WriteIndented = True})
                File.WriteAllText(sfd.FileName, json)

                MessageBox.Show("Backup esportato con successo!", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End Using

        Catch ex As Exception
            MessageBox.Show("Errore durante l'export: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Function EsportaPattern() As List(Of Object)
        Dim pattern As New List(Of Object)

        Try
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Dim sql As String = "SELECT * FROM Pattern ORDER BY MacroCategoria, Categoria, Parola"

                Using cmd As New SQLiteCommand(sql, conn)
                    Using reader As SQLiteDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            pattern.Add(New With {
                                .ID = reader("ID"),
                                .Parola = reader("Parola").ToString(),
                                .MacroCategoria = reader("MacroCategoria").ToString(),
                                .Categoria = reader("Categoria").ToString(),
                                .Peso = reader("Peso"),
                                .Fonte = reader("Fonte").ToString()
                            })
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show("Errore esportazione pattern: " & ex.Message)
        End Try

        Return pattern
    End Function

    Private Sub BtnImportaBackup_Click(sender As Object, e As EventArgs)
        Try
            Using ofd As New OpenFileDialog() With {
                .Title = "Seleziona Backup da Importare",
                .Filter = "File JSON (*.json)|*.json"
            }
                If ofd.ShowDialog() <> DialogResult.OK Then Return

                Dim json = File.ReadAllText(ofd.FileName)
                Dim backup = JsonSerializer.Deserialize(Of Dictionary(Of String, JsonElement))(json)

                If MessageBox.Show("ATTENZIONE: L'importazione sovrascriverà tutti i pattern esistenti. Continuare?",
                                 "Conferma Import", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) <> DialogResult.Yes Then
                    Return
                End If

                ImportaPattern(backup("Pattern"))

                MessageBox.Show("Backup importato con successo!", "Import", MessageBoxButtons.OK, MessageBoxIcon.Information)
                CaricaSetCategorie()
                PopolaTreeView()
                AggiornaStatistiche()
            End Using

        Catch ex As Exception
            MessageBox.Show("Errore durante l'import: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ImportaPattern(patternJson As JsonElement)
        Try
            Using conn As New SQLiteConnection(DatabaseManager.GetConnectionString())
                conn.Open()
                Using trans = conn.BeginTransaction()

                    ' Elimina pattern esistenti
                    Using cmd As New SQLiteCommand("DELETE FROM Pattern", conn, trans)
                        cmd.ExecuteNonQuery()
                    End Using

                    ' Inserisci nuovi pattern
                    Dim sql As String = "INSERT INTO Pattern (Parola, MacroCategoria, Categoria, Peso, Fonte) VALUES (@parola, @macro, @cat, @peso, @fonte)"

                    For Each elemento In patternJson.EnumerateArray()
                        Using cmd As New SQLiteCommand(sql, conn, trans)
                            cmd.Parameters.AddWithValue("@parola", elemento.GetProperty("Parola").GetString())
                            cmd.Parameters.AddWithValue("@macro", elemento.GetProperty("MacroCategoria").GetString())
                            cmd.Parameters.AddWithValue("@cat", elemento.GetProperty("Categoria").GetString())
                            'cmd.Parameters.AddWithValue("@sotto", elemento.GetProperty("SottoCategoria").GetString())
                            cmd.Parameters.AddWithValue("@peso", elemento.GetProperty("Peso").GetInt32())
                            cmd.Parameters.AddWithValue("@fonte", elemento.GetProperty("Fonte").GetString())
                            cmd.ExecuteNonQuery()
                        End Using
                    Next

                    trans.Commit()
                End Using
            End Using

        Catch ex As Exception
            Throw New Exception("Errore importazione pattern: " & ex.Message)
        End Try
    End Sub

    Private Sub BtnTemplateBase_Click(sender As Object, e As EventArgs)
        If MessageBox.Show("Vuoi caricare il template base di categorie? Questo aggiungerà categorie predefinite utili.",
                         "Template Base", MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then
            Return
        End If

        Try
            CaricaTemplateBase()
            MessageBox.Show("Template base caricato con successo!", "Template", MessageBoxButtons.OK, MessageBoxIcon.Information)
            CaricaSetCategorie()
            PopolaTreeView()
            AggiornaStatistiche()

        Catch ex As Exception
            MessageBox.Show("Errore caricamento template: " & ex.Message, "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Structure TemplatePattern
        Public Parola As String
        Public Macro As String
        Public Cat As String
        Public Sotto As String
        Public Peso As Integer
    End Structure

    Private Sub CaricaTemplateBase()
        Dim templatePattern As New List(Of TemplatePattern) From {
        New TemplatePattern With {.Parola = "stipendio", .Macro = "Entrate", .Cat = "Lavoro", .Sotto = "Stipendio", .Peso = 10},
        New TemplatePattern With {.Parola = "salary", .Macro = "Entrate", .Cat = "Lavoro", .Sotto = "Stipendio", .Peso = 10},
        New TemplatePattern With {.Parola = "bonus", .Macro = "Entrate", .Cat = "Lavoro", .Sotto = "Bonus", .Peso = 8},
        New TemplatePattern With {.Parola = "rimborso", .Macro = "Entrate", .Cat = "Rimborsi", .Sotto = "Vari", .Peso = 7},
        New TemplatePattern With {.Parola = "dividendo", .Macro = "Entrate", .Cat = "Investimenti", .Sotto = "Dividendi", .Peso = 9},
        New TemplatePattern With {.Parola = "interesse", .Macro = "Entrate", .Cat = "Investimenti", .Sotto = "Interessi", .Peso = 8},
        New TemplatePattern With {.Parola = "supermercato", .Macro = "Uscite", .Cat = "Casa", .Sotto = "Spesa", .Peso = 9},
        New TemplatePattern With {.Parola = "conad", .Macro = "Uscite", .Cat = "Casa", .Sotto = "Spesa", .Peso = 10},
        New TemplatePattern With {.Parola = "coop", .Macro = "Uscite", .Cat = "Casa", .Sotto = "Spesa", .Peso = 10},
        New TemplatePattern With {.Parola = "esselunga", .Macro = "Uscite", .Cat = "Casa", .Sotto = "Spesa", .Peso = 10},
        New TemplatePattern With {.Parola = "carrefour", .Macro = "Uscite", .Cat = "Casa", .Sotto = "Spesa", .Peso = 10},
        New TemplatePattern With {.Parola = "ristorante", .Macro = "Uscite", .Cat = "Svago", .Sotto = "Ristorazione", .Peso = 8},
        New TemplatePattern With {.Parola = "pizzeria", .Macro = "Uscite", .Cat = "Svago", .Sotto = "Ristorazione", .Peso = 8},
        New TemplatePattern With {.Parola = "bar", .Macro = "Uscite", .Cat = "Svago", .Sotto = "Ristorazione", .Peso = 7},
        New TemplatePattern With {.Parola = "mcdonald", .Macro = "Uscite", .Cat = "Svago", .Sotto = "Fast Food", .Peso = 9},
        New TemplatePattern With {.Parola = "burger", .Macro = "Uscite", .Cat = "Svago", .Sotto = "Fast Food", .Peso = 7},
        New TemplatePattern With {.Parola = "benzina", .Macro = "Uscite", .Cat = "Trasporti", .Sotto = "Carburante", .Peso = 9},
        New TemplatePattern With {.Parola = "esso", .Macro = "Uscite", .Cat = "Trasporti", .Sotto = "Carburante", .Peso = 10},
        New TemplatePattern With {.Parola = "shell", .Macro = "Uscite", .Cat = "Trasporti", .Sotto = "Carburante", .Peso = 10},
        New TemplatePattern With {.Parola = "ip", .Macro = "Uscite", .Cat = "Trasporti", .Sotto = "Carburante", .Peso = 10},
        New TemplatePattern With {.Parola = "autobus", .Macro = "Uscite", .Cat = "Trasporti", .Sotto = "Mezzi Pubblici", .Peso = 8},
        New TemplatePattern With {.Parola = "treno", .Macro = "Uscite", .Cat = "Trasporti", .Sotto = "Mezzi Pubblici", .Peso = 8},
        New TemplatePattern With {.Parola = "taxi", .Macro = "Uscite", .Cat = "Trasporti", .Sotto = "Taxi", .Peso = 9},
        New TemplatePattern With {.Parola = "farmacia", .Macro = "Uscite", .Cat = "Salute", .Sotto = "Farmaci", .Peso = 9},
        New TemplatePattern With {.Parola = "medico", .Macro = "Uscite", .Cat = "Salute", .Sotto = "Visite", .Peso = 8},
        New TemplatePattern With {.Parola = "dentista", .Macro = "Uscite", .Cat = "Salute", .Sotto = "Dentista", .Peso = 9},
        New TemplatePattern With {.Parola = "affitto", .Macro = "Uscite", .Cat = "Casa", .Sotto = "Affitto", .Peso = 10},
        New TemplatePattern With {.Parola = "bolletta", .Macro = "Uscite", .Cat = "Casa", .Sotto = "Utenze", .Peso = 9},
        New TemplatePattern With {.Parola = "luce", .Macro = "Uscite", .Cat = "Casa", .Sotto = "Utenze", .Peso = 8},
        New TemplatePattern With {.Parola = "gas", .Macro = "Uscite", .Cat = "Casa", .Sotto = "Utenze", .Peso = 8},
        New TemplatePattern With {.Parola = "acqua", .Macro = "Uscite", .Cat = "Casa", .Sotto = "Utenze", .Peso = 8},
        New TemplatePattern With {.Parola = "telefono", .Macro = "Uscite", .Cat = "Casa", .Sotto = "Telecomunicazioni", .Peso = 8},
        New TemplatePattern With {.Parola = "internet", .Macro = "Uscite", .Cat = "Casa", .Sotto = "Telecomunicazioni", .Peso = 8},
        New TemplatePattern With {.Parola = "tim", .Macro = "Uscite", .Cat = "Casa", .Sotto = "Telecomunicazioni", .Peso = 9},
        New TemplatePattern With {.Parola = "vodafone", .Macro = "Uscite", .Cat = "Casa", .Sotto = "Telecomunicazioni", .Peso = 9},
        New TemplatePattern With {.Parola = "cinema", .Macro = "Uscite", .Cat = "Svago", .Sotto = "Intrattenimento", .Peso = 8},
        New TemplatePattern With {.Parola = "netflix", .Macro = "Uscite", .Cat = "Svago", .Sotto = "Streaming", .Peso = 9},
        New TemplatePattern With {.Parola = "spotify", .Macro = "Uscite", .Cat = "Svago", .Sotto = "Streaming", .Peso = 9},
        New TemplatePattern With {.Parola = "amazon", .Macro = "Uscite", .Cat = "Shopping", .Sotto = "Online", .Peso = 8},
        New TemplatePattern With {.Parola = "banca", .Macro = "Uscite", .Cat = "Servizi", .Sotto = "Bancari", .Peso = 7},
        New TemplatePattern With {.Parola = "commissione", .Macro = "Uscite", .Cat = "Servizi", .Sotto = "Bancari", .Peso = 8},
        New TemplatePattern With {.Parola = "assicurazione", .Macro = "Uscite", .Cat = "Servizi", .Sotto = "Assicurazioni", .Peso = 9}
    }

        Using conn As New SQLite.SQLiteConnection(DatabaseManager.GetConnectionString())
            conn.Open()
            Using trans = conn.BeginTransaction()
                Dim sql As String = "INSERT OR IGNORE INTO Pattern (Parola, MacroCategoria, Categoria, Peso, Fonte) VALUES (@parola, @macro, @cat, @peso, @fonte)"
                For Each p In templatePattern
                    Using cmd As New SQLite.SQLiteCommand(sql, conn, trans)
                        cmd.Parameters.AddWithValue("@parola", p.Parola)
                        cmd.Parameters.AddWithValue("@macro", p.Macro)
                        cmd.Parameters.AddWithValue("@cat", p.Cat)
                        cmd.Parameters.AddWithValue("@peso", p.Peso)
                        cmd.Parameters.AddWithValue("@fonte", "Template")
                        cmd.ExecuteNonQuery()
                    End Using
                Next
                trans.Commit()
            End Using
        End Using
    End Sub

    Private Sub lblTitolo_Click(sender As Object, e As EventArgs) Handles lblTitolo.Click

    End Sub


#End Region

End Class
