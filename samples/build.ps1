$GCC     = "riscv-none-elf-gcc"
$OBJCOPY = "riscv-none-elf-objcopy"
$FLAGS   = @("-march=rv32i", "-mabi=ilp32", "-nostdlib", "-static", "-T", "link.ld")

foreach ($src in Get-ChildItem -Filter "*.s") {
    $name = $src.BaseName
    Write-Host "Building $name..."
    & $GCC @FLAGS $src.Name -o "$name.elf"
    if ($LASTEXITCODE -ne 0) { Write-Host "  ERROR: compile failed"; continue }
    & $OBJCOPY -O binary "$name.elf" "$name.bin"
    Remove-Item "$name.elf"
    Write-Host "  -> $name.bin"
}

Write-Host "`nDone. Run with: dotnet run <name>.bin"
