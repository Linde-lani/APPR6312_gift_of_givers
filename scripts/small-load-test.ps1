param(
	[string]$BaseUrl = "http://localhost:5249",
	[int]$ConcurrentUsers = 5,
	[string]$Path = "/Index"
)

$ErrorActionPreference = "Stop"
[void][System.Reflection.Assembly]::LoadWithPartialName("System.Net.Http")
$handler = [System.Net.Http.HttpClientHandler]::new()
$handler.ServerCertificateCustomValidationCallback = { $true }
$client = [System.Net.Http.HttpClient]::new($handler)
$client.Timeout = [TimeSpan]::FromSeconds(30)

try {
	$uri = "$($BaseUrl.TrimEnd('/'))$Path"
	$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
	$tasks = 1..$ConcurrentUsers | ForEach-Object {
		$client.GetAsync($uri)
	}

	$results = foreach ($task in $tasks) {
		try {
			$response = $task.GetAwaiter().GetResult()
			[PSCustomObject]@{
				StatusCode = [int]$response.StatusCode
				Successful = $response.IsSuccessStatusCode
				Error = $null
				Response = $response
			}
		}
		catch {
			$errorMessage = $_.Exception.Message
			$innerException = $_.Exception.InnerException
			while ($null -ne $innerException) {
				$errorMessage = "$errorMessage -> $($innerException.Message)"
				$innerException = $innerException.InnerException
			}

			[PSCustomObject]@{
				StatusCode = $null
				Successful = $false
				Error = $errorMessage
				Response = $null
			}
		}
	}
	$stopwatch.Stop()
	$successful = @($results | Where-Object { $_.Successful }).Count
	$failed = $results.Count - $successful

	[PSCustomObject]@{
		Url = $uri
		ConcurrentUsers = $ConcurrentUsers
		TotalRequests = $results.Count
		Successful = $successful
		Failed = $failed
		TotalMilliseconds = $stopwatch.ElapsedMilliseconds
		AverageMilliseconds = [math]::Round($stopwatch.ElapsedMilliseconds / $results.Count, 2)
		StatusCodes = (($results | ForEach-Object StatusCode) -join ', ')
		Errors = (($results | Where-Object Error | ForEach-Object Error) -join ' | ')
	}

	$results | Where-Object Response | ForEach-Object { $_.Response.Dispose() }
}
finally {
	$client.Dispose()
	$handler.Dispose()
}
