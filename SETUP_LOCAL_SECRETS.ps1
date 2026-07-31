$ErrorActionPreference = 'Stop'

function ConvertFrom-SecureValue {
    param([Security.SecureString]$Value)

    $pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($Value)
    try {
        return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
    }
}

function Set-ProjectSecret {
    param(
        [string]$Project,
        [string]$Key,
        [string]$Value
    )

    & dotnet user-secrets set $Key $Value --project $Project | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Could not save the local setting: $Key"
    }
}

$email = Read-Host 'Local administrator email (default: admin@smartit.local)'
if ([string]::IsNullOrWhiteSpace($email)) {
    $email = 'admin@smartit.local'
}

$displayName = Read-Host 'Display name (default: SmartIT Administrator)'
if ([string]::IsNullOrWhiteSpace($displayName)) {
    $displayName = 'SmartIT Administrator'
}

$firstPassword = Read-Host 'Create a local administrator password (minimum 8 characters)' -AsSecureString
$secondPassword = Read-Host 'Enter the password again' -AsSecureString
$plainPassword = ConvertFrom-SecureValue $firstPassword
$confirmation = ConvertFrom-SecureValue $secondPassword

try {
    if ($plainPassword.Length -lt 8) {
        throw 'The password must contain at least 8 characters.'
    }

    if ($plainPassword -cne $confirmation) {
        throw 'The passwords do not match.'
    }

    $webProject = Join-Path $PSScriptRoot 'SmartIT.Web\SmartIT.Web.csproj'
    $apiProject = Join-Path $PSScriptRoot 'SmartIT.API\SmartIT.API.csproj'

    Set-ProjectSecret $webProject 'Seed:AdminEmail' $email
    Set-ProjectSecret $webProject 'Seed:AdminPassword' $plainPassword
    Set-ProjectSecret $webProject 'Seed:AdminDisplayName' $displayName

    Set-ProjectSecret $apiProject 'Seed:AdminEmail' $email
    Set-ProjectSecret $apiProject 'Seed:AdminPassword' $plainPassword
    Set-ProjectSecret $apiProject 'Seed:AdminDisplayName' $displayName

    $jwtBytes = New-Object byte[] 48
    $generator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $generator.GetBytes($jwtBytes)
    }
    finally {
        $generator.Dispose()
    }

    $jwtKey = [Convert]::ToBase64String($jwtBytes)
    Set-ProjectSecret $apiProject 'Jwt:Key' $jwtKey
}
finally {
    $plainPassword = $null
    $confirmation = $null
}
