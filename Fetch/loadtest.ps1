# Define parameters
$endpoint = "http://138.197.66.14:8080/Request/New"
$statsEndpoint = "http://138.197.66.14:8080/admin/stats"
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoidGVzdHJlcXVlc3RvciIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IlJlcXVlc3RvciIsImV4cCI6MTczMzc3OTU4OCwiaXNzIjoiRmV0Y2giLCJhdWQiOiJGZXRjaFVzZXJzIn0.Z33dEe1HG2DpjYv1p657iKNqfKguORyBT0BB2JqYSlk"
$payloadTemplate = @{
    request = @{
        id = 0
        title = ""
        description = "test"
        price = 100
        dueDate = "2024-12-09T20:27:14.597Z"
        category = 0
    }
}

# Number of requests for the load test
$iterations = 10000

# Initialize results storage
$results = @()

# Start load testing
Write-Host "Starting load test with $iterations iterations..."
for ($i = 1; $i -le $iterations; $i++) {
    $payloadTemplate.request.title = "test<$i>"
    $payload = $payloadTemplate | ConvertTo-Json -Depth 10

    try {
        $response = Invoke-RestMethod -Uri $endpoint -Method Post -Headers @{ Authorization = "Bearer $token" } -Body $payload -ContentType "application/json"
        $results += @{ Iteration = $i; Status = "Success"; Response = $response }
    } catch {
        $results += @{ Iteration = $i; Status = "Failed"; Error = $_.Exception.Message }
    }
}

# Log the results
Write-Host "`nLoad test complete. Summary of results:"
$successCount = $results | Where-Object { $_.Status -eq "Success" } | Measure-Object | Select-Object -ExpandProperty Count
$failureCount = $results | Where-Object { $_.Status -eq "Failed" } | Measure-Object | Select-Object -ExpandProperty Count
Write-Host "Successful Requests: $successCount"
Write-Host "Failed Requests: $failureCount"

# Call the stats endpoint and display its output
Write-Host "`nFetching stats from the server..."
try {
    $statsResponse = Invoke-RestMethod -Uri $statsEndpoint -Method Get -Headers @{ Authorization = "Bearer $token" }
    Write-Host "`nServer Stats Response:" -ForegroundColor Green
    Write-Host ($statsResponse | ConvertTo-Json -Depth 10)
} catch {
    Write-Host "Failed to fetch stats: $($_.Exception.Message)" -ForegroundColor Red
}
