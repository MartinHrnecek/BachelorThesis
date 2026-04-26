# .NET Reactor integration

.NET Reactor is a commercial obfuscator from Eziriz. This directory
**does not include any binaries or license keys** — users must obtain a demo
or commercial license themselves before running the obfuscation pipeline.

## Installation

1. Download .NET Reactor from
   [https://www.eziriz.com/dotnet_reactor.htm](https://www.eziriz.com/dotnet_reactor.htm)
2. Run the installer (default install path on Windows is
   `C:\Program Files (x86)\Eziriz\.NET Reactor\`)
3. Activate demo or commercial license
4. The pipeline (`scripts\run_all.ps1`) auto-detects the standard install
   location; if you install elsewhere, edit the `$reactor` variable in
   `run_all.ps1`.

## Configuration used in the experiments

The thesis uses the protections available in the **demo version** invoked
from the command line:

| Flag                  | Value | Effect                                    |
|-----------------------|-------|-------------------------------------------|
| `-obfuscate`          | 1     | Master obfuscation switch                 |
| `-control_flow`       | 1     | Control flow obfuscation                  |
| `-string_encryption`  | 1     | String literal encryption                 |
| `-rename_fields`      | 1     | Field renaming                            |
| `-rename_methods`     | 1     | Method renaming                           |
| `-rename_properties`  | 1     | Property renaming                         |

The CLI is invoked with:

```powershell
& dotNET_Reactor.Console.exe `
    -file <input.dll> `
    -targetfile <output.dll> `
    -obfuscate 1 `
    -control_flow 1 `
    -string_encryption 1 `
    -rename_fields 1 `
    -rename_methods 1 `
    -rename_properties 1
```

## Version used in experiments

**.NET Reactor 6.9** (demo license).

The demo version produces fully functional obfuscated assemblies with all
of the features above enabled. Note that the demo version may add a
"Protected by .NET Reactor" notice on the first use of the assembly.

If you use a different version, the measured metrics (especially the
characteristic `CC_max` values in the 300+ range and the M_IL inflation
factor of approximately 14×) may differ slightly.

## License notice

.NET Reactor is **commercial software** licensed by Eziriz. This repository
contains **no .NET Reactor binaries, project files, or license keys**.
Users must obtain their own demo or commercial license to reproduce the
experiments.

## Pipeline behaviour without .NET Reactor

If .NET Reactor is not installed when `run_all.ps1` is executed, the
pipeline emits a soft warning during the prerequisite check and skips the
.NET Reactor obfuscation step automatically. Other obfuscators continue
unaffected.