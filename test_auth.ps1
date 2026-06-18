param([string]$Url = "http://localhost:5073")

try {
	$response = Invoke-WebRequest -Uri "$Url/Dashboard" -SkipHttpErrorCheck -ErrorAction Continue -UseBasicParsing
	Write-Host "Status Code: $($response.StatusCode)"
	Write-Host "Contains Login: $($response.Content -like '*login*')"
} catch {
	Write-Host "Error: $($_.Exception.Message)"
}

# Test login endpoint
try {
	$loginResponse = Invoke-WebRequest -Uri "$Url/Auth/Auth/Login" -SkipHttpErrorCheck -ErrorAction Continue -UseBasicParsing
	Write-Host "Login Page Status: $($loginResponse.StatusCode)"
	Write-Host "Login Page Size: $($loginResponse.Content.Length)"
} catch {
	Write-Host "Login Error: $($_.Exception.Message)"
}
