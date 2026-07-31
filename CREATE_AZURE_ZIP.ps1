param(
    [Parameter(Mandatory = $true)]
    [string]$SourceDirectory,

    [Parameter(Mandatory = $true)]
    [string]$DestinationFile
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$sourcePath = (Resolve-Path $SourceDirectory).Path
$destinationPath = [System.IO.Path]::GetFullPath($DestinationFile)

if (Test-Path $destinationPath) {
    Remove-Item -LiteralPath $destinationPath -Force
}

$archiveStream = [System.IO.File]::Open(
    $destinationPath,
    [System.IO.FileMode]::CreateNew,
    [System.IO.FileAccess]::ReadWrite,
    [System.IO.FileShare]::None)

try {
    $archive = [System.IO.Compression.ZipArchive]::new(
        $archiveStream,
        [System.IO.Compression.ZipArchiveMode]::Create,
        $false)

    try {
        Get-ChildItem -LiteralPath $sourcePath -File -Recurse | ForEach-Object {
            $relativePath = $_.FullName.Substring($sourcePath.Length).TrimStart('\', '/')
            $entryName = $relativePath.Replace('\', '/')
            [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
                $archive,
                $_.FullName,
                $entryName,
                [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
        }
    }
    finally {
        $archive.Dispose()
    }
}
finally {
    $archiveStream.Dispose()
}

Write-Host "Azure ZIP created: $destinationPath"
