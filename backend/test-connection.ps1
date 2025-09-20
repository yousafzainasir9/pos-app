# Test SQL Server Connection
$connectionString = "Server=Zai;Database=master;User Id=sa;Password=1234;TrustServerCertificate=True"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Testing SQL Server Connection" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    
    Write-Host "Connecting to SQL Server 'Zai'..." -ForegroundColor Yellow
    $connection.Open()
    
    if ($connection.State -eq "Open") {
        Write-Host "✅ Successfully connected to SQL Server!" -ForegroundColor Green
        
        # Check if POSDatabase exists
        $command = $connection.CreateCommand()
        $command.CommandText = "SELECT DB_ID('POSDatabase')"
        $result = $command.ExecuteScalar()
        
        if ($result -ne [System.DBNull]::Value) {
            Write-Host "✅ Database 'POSDatabase' exists!" -ForegroundColor Green
            
            # Get table count
            $command.CommandText = "SELECT COUNT(*) FROM POSDatabase.sys.tables"
            $tableCount = $command.ExecuteScalar()
            Write-Host "   Found $tableCount tables in the database" -ForegroundColor Cyan
            
            # Get some statistics
            $command.CommandText = @"
                USE POSDatabase;
                SELECT 
                    (SELECT COUNT(*) FROM Users WHERE IsDeleted = 0) as UserCount,
                    (SELECT COUNT(*) FROM Categories WHERE IsDeleted = 0) as CategoryCount,
                    (SELECT COUNT(*) FROM Products WHERE IsDeleted = 0) as ProductCount,
                    (SELECT COUNT(*) FROM Stores WHERE IsDeleted = 0) as StoreCount
"@
            $reader = $command.ExecuteReader()
            if ($reader.Read()) {
                Write-Host ""
                Write-Host "Database Statistics:" -ForegroundColor Yellow
                Write-Host "  • Stores: $($reader['StoreCount'])" -ForegroundColor White
                Write-Host "  • Users: $($reader['UserCount'])" -ForegroundColor White
                Write-Host "  • Categories: $($reader['CategoryCount'])" -ForegroundColor White
                Write-Host "  • Products: $($reader['ProductCount'])" -ForegroundColor White
            }
            $reader.Close()
        }
        else {
            Write-Host "⚠️  Database 'POSDatabase' does not exist yet" -ForegroundColor Yellow
            Write-Host "   Run 'refresh-db.bat' to create and seed the database" -ForegroundColor White
        }
        
        $connection.Close()
    }
}
catch {
    Write-Host "❌ Failed to connect to SQL Server!" -ForegroundColor Red
    Write-Host "   Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please check:" -ForegroundColor Yellow
    Write-Host "  1. SQL Server 'Zai' is running" -ForegroundColor White
    Write-Host "  2. SQL Server Authentication is enabled" -ForegroundColor White
    Write-Host "  3. 'sa' account is enabled with password '1234'" -ForegroundColor White
    Write-Host "  4. TCP/IP protocol is enabled in SQL Server Configuration Manager" -ForegroundColor White
}

Write-Host ""
Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
