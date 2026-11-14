# Editor dependency audit

Run after restoring and building the three WPF applications:

```powershell
dotnet run --project Tools/DependencyAudit -- "Build (x64)/net6.0-windows/TombEditor.dll" "Build (x64)/net6.0-windows/WadTool.dll" "Build (x64)/net6.0-windows/SoundTool.dll"
```

The audit reads each application's generated dependency manifest, checks its declared local runtime assemblies and follows their managed assembly references. It also checks the runtime framework configuration. It rejects references to the previous presentation assemblies and the Windows Forms assemblies. It does not mistake unrelated binaries left in the shared build directory for application dependencies.

The existing SharpDX and SharpDX.Toolkit.Graphics DLLs are explicitly exempted for their internal System.Windows.Forms references. These vendor binaries remain unchanged pending a future renderer migration. The exception does not apply to application code or other libraries.

Exit codes: 0 means the application graph passes with only the documented vendor exceptions; 1 means forbidden dependencies were found; 2 means an input or inspection error.
