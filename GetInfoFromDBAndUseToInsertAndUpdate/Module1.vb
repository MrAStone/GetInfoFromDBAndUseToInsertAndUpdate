
Imports MySql.Data.MySqlClient


Module Module1

    Sub Main()

        Dim con = CreateConnection()
        Do

            'find the tables within the database
            Dim tables = getTables(con)
            Console.Write("Which table do you want to use?: ")
            Dim tableChoice = Console.ReadLine()
            Dim menuChoice = menu()
            Console.WriteLine(menuChoice)
            Select Case menuChoice
                Case 1
                    viewData(con, tables, tableChoice)
                Case 2
                    insertData(con, tables, tableChoice)
                Case 3
                    updateData(con, tables, tableChoice)
            End Select


        Loop
    End Sub
    Sub updateData(con As MySqlConnection, tables As List(Of String), menuChoice As Integer)
        con.Open()
        Dim reader As MySqlDataReader
        Dim cmd As New MySqlCommand
        Dim SQL As String = "SELECT * FROM " & tables(menuChoice - 1)
        cmd.CommandText = SQL
        cmd.Connection = con
        reader = cmd.ExecuteReader
        Dim lopk As New List(Of String)
        While reader.Read

            lopk.Add(reader.GetString(0))
            Console.Write(lopk.Count & ": ")
            For i = 0 To reader.FieldCount - 1
                Console.Write(reader.GetString(i) & " ")
            Next
            Console.WriteLine()
        End While
        con.Close()
        Console.Write("Which record do you want to update: ")
        Dim recChoice As Integer = Console.ReadLine()
        Dim PK = lopk(recChoice - 1)
        con.Open()
        reader = getFieldNames(con, tables, menuChoice)
        Dim fields As New List(Of String)
        While reader.Read()
            fields.Add(reader.GetString(0))
            Console.WriteLine(fields.Count & ":" & reader.GetString(0))
        End While

        con.Close()
        Console.Write("Which filed do you want to update: ")
        Dim fieldChoice As Integer = Console.ReadLine
        Console.Write("What is the new value: ")
        Dim newVal = Console.ReadLine
        Dim field = fields(fieldChoice - 1)
        SQL = "UPDATE " & tables(menuChoice - 1) & " SET " & field & "=@value WHERE " & fields(0) & "=@val"
        con.Open()
        cmd.Connection = con
        cmd.CommandText = SQL
        cmd.Parameters.AddWithValue("@value", newVal)
        cmd.Parameters.AddWithValue("@val", PK)
        Console.WriteLine(SQL)
        cmd.ExecuteNonQuery()
        con.Close()

    End Sub
    Sub viewData(con As MySqlConnection, tables As List(Of String), menuChoice As Integer)
        con.Open()
        Dim reader = getFieldNames(con, tables, menuChoice)
        Dim fields As New List(Of String)
        While reader.Read()
            fields.Add(reader.GetString(0))
            Console.WriteLine(fields.Count & ":" & reader.GetString(0))
        End While

        con.Close()
        Console.Write("Which field do you want to set criteria for choose 0 for all data: ")
        Dim fieldChoice As Integer = Console.ReadLine
        Dim fieldCriteria As String
        Dim criteria As String
        If fieldChoice <> 0 Then

            fieldCriteria = fields(fieldChoice - 1)

            fieldCriteria &= getComparitor()
            Console.Write("What value do you want to filter by: ")
            criteria = Console.ReadLine
        End If

        Dim cmd As New MySqlCommand
        Dim SQL As String
        If fieldChoice = 0 Then
            SQL = "SELECT * FROM " & tables(menuChoice - 1)
        Else
            SQL = "SELECT * FROM " & tables(menuChoice - 1) & " WHERE " & fieldCriteria & "@param;"
        End If

        Console.WriteLine(SQL)
        con.Open()
        cmd.Connection = con
        cmd.CommandText = SQL
        ' cmd.Parameters.AddWithValue("@field", "NoOfLicences")
        cmd.Parameters.AddWithValue("@param", criteria)

        reader = cmd.ExecuteReader
        While reader.Read
            For i = 0 To reader.FieldCount - 1
                Console.Write(reader.GetString(i) & " ")
            Next
            Console.WriteLine()
        End While
        con.Close()
    End Sub
    Function getComparitor() As String

        Dim comparers = {"<", ">", "<=", ">=", "=", "!="}
        For i = 0 To comparers.Length - 1
            Console.WriteLine(i + 1 & " : " & comparers(i))
        Next

        Console.Write("Pick a comparitor: ")
        Dim choice As Integer = Console.ReadLine()

        Return comparers(choice - 1)

    End Function
    Function menu() As Integer
        Console.WriteLine("DATABASE")
        Console.WriteLine("--------")
        Console.WriteLine("1: View data")
        Console.WriteLine("2: Insert data")
        Console.WriteLine("3: Update data")
        Console.Write("Enter menu choice: ")
        Dim choice As Integer = Console.ReadLine
        Return choice
    End Function
    Function getTables(con As MySqlConnection) As List(Of String)
        Dim cmd As New MySqlCommand
        Dim SQL As String = "SELECT DISTINCT(TABLE_NAME) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = 'agsTest_12BSoftware'"
        cmd.CommandText = SQL
        con.Open()
        cmd.Connection = con
        Dim reader As MySqlDataReader
        reader = cmd.ExecuteReader
        Dim tables As New List(Of String)
        'output the tables to the console and get the user choice
        While reader.Read
            tables.Add(reader.GetString(0))
            Console.WriteLine(tables.Count & ":" & reader.GetString(0))
        End While
        con.Close()
        Return tables

    End Function
    Function CreateConnection() As MySqlConnection
        'Get database server password
        Dim pwd As String
        Console.Write("Enter database password: ")
        pwd = Console.ReadLine
        Console.Clear()
        Dim con As New MySqlConnection("server=192.168.35.126;uid=agsTest;pwd=" & pwd & ";database=agsTest_12BSoftware")
        Return con

    End Function
    Function getFieldNames(con As MySqlConnection, tables As List(Of String), menuChoice As Integer) As MySqlDataReader
        Dim cmd As New MySqlCommand
        Dim reader As MySqlDataReader
        Dim SQL As String
        'Select the fields from the chosen table and add them to a list
        SQL = "SELECT COLUMN_NAME FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = 'agsTest_12BSoftware' AND TABLE_NAME=@Tname"
        cmd.CommandText = SQL
        cmd.Connection = con
        cmd.Parameters.AddWithValue("@Tname", tables(menuChoice - 1))
        reader = cmd.ExecuteReader()
        Return reader
    End Function
    Sub insertData(con As MySqlConnection, tables As List(Of String), menuChoice As Integer)
        con.Open()
        Dim cmd As New MySqlCommand
        Dim reader = getFieldNames(con, tables, menuChoice)
        Dim fields As New List(Of String)
        While reader.Read
            fields.Add(reader.GetString(0))
        End While
        con.Close()
        Dim sql As String


        'Get the user to enter values for each field
        Dim values As New List(Of String)
        For i = 0 To fields.Count - 1
            Console.Write("Enter {0} value: ", fields(i))
            values.Add(Console.ReadLine)
        Next
        'Parameterise the SQL
        'This is done in a loop for each field there is
        'it will add a , to the last one
        SQL = "INSERT INTO " & tables(menuChoice - 1) & " VALUES ("
        For i = 0 To fields.Count - 1
            SQL &= "@param" & i & ","
        Next

        'remove the last ,
        SQL = SQL.TrimEnd(",")
        'close the SQL statement
        SQL &= ")"

        'clear all parameters used before
        cmd.Parameters.Clear()


        cmd.CommandText = SQL
        ' loop through parameters adding values from the value list
        For i = 0 To fields.Count - 1
            cmd.Parameters.AddWithValue("@param" & i, values(i))
        Next

        'Run the query
        con.Open()
        cmd.Connection = con
        cmd.ExecuteNonQuery()
        con.Close()

        'clean up the parameters
        cmd.Parameters.Clear()
    End Sub

End Module
