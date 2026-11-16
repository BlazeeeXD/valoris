# Unity compilation repair

This patch addresses the three errors visible in the 12 November 2026 Unity Console screenshot.

1. `Valoris.Game.asmdef` now references `Unity.TextMeshPro`, which is the assembly containing `TMPro.TMP_Text` and `TextMeshProUGUI` in the project's uGUI 2.5.0 package.
2. `Directory.Build.props` redirects only `Valoris.Core` build output and intermediate files to the repository-root `.dotnet/` folder. Unity will no longer discover `Packages/com.valoris.core/bin/.../Valoris.Core.dll` beside the `Valoris.Core.asmdef`.
3. `Repair-UnityCompilation.ps1` removes any stale `Packages/com.valoris.core/bin` and `obj` folders created by earlier local builds.
4. `.gitignore` excludes the redirected `.dotnet/` folder.

## Apply

1. Close Unity.
2. Extract this patch into the project root, preserving folder paths and allowing `Valoris.Game.asmdef` and `.gitignore` to be overwritten.
3. Open PowerShell in the project root and run `powershell -ExecutionPolicy Bypass -File .\Repair-UnityCompilation.ps1`.
4. Reopen Unity and wait for script compilation. Do not press Exit Safe Mode until the Console has zero errors.
5. Run the Core build again. Its DLL should now appear under `.dotnet\bin\Valoris.Core\Debug\netstandard2.1\`, not under `Packages\com.valoris.core\bin\`.

If TMP remains unresolved, verify that the built-in `com.unity.ugui` package is present and that the `Unity.TextMeshPro` assembly is available in Package Manager. The project already has `com.unity.ugui` 2.5.0 in its package manifest/lock.

The `CS0436` warnings shown on `ProceduralMapGenerator.cs` are expected to be a downstream effect of Unity importing the duplicate `Valoris.Core.dll`. Removing that duplicate DLL should eliminate that conflict. If they remain after this repair, capture the expanded warning text; then they need a separate source-level investigation.

This patch does not claim to fix unrelated warnings that may appear after Unity recompiles.
