# CrazyStorm MCP Server

This project exposes CrazyStorm project-file utilities through the Model Context Protocol over stdio.

The MCP server itself is headless. It is launched in the background by MCP clients.
To show the real CrazyStorm UI, use `crazy_storm_open_editor`, which launches
`CrazyStorm.exe` and optionally opens a project file.

When the editor is already open, the editor process listens on a local named pipe.
Use `crazy_storm_ping_running_editor` to check that the window is reachable, or
`crazy_storm_open_in_running_editor` to ask the running window to open/reload a project.
Editing tools can also pass `reloadInRunningEditor=true` so the open editor refreshes
immediately after the `.bgp` file is saved.

## Build

Build the solution or just this project:

```powershell
dotnet msbuild .\CrazyStorm.McpServer\CrazyStorm.McpServer.csproj /p:Configuration=Debug /p:Platform=AnyCPU
```

To build the editor for local MCP window testing on a machine without the shader
compiler or Sphinx documentation dependencies, use:

```powershell
dotnet msbuild .\CrazyStorm2.0.csproj /p:Configuration=Debug /p:Platform=AnyCPU /p:SkipShaderCompile=true /p:BuildDocs=false
```

Normal editor/player builds still expect the MonoGame shader compiler to generate
`shader.mgfxo`.

## Client configuration

Use the built executable as a stdio MCP server:

```json
{
  "mcpServers": {
    "crazystorm": {
      "command": "D:\\Csharp\\crazystorm\\CrazyStorm2.0\\CrazyStorm.McpServer\\bin\\Debug\\CrazyStorm.McpServer.exe"
    }
  }
}
```

## Tools

### Inspection and export

- `crazy_storm_project_summary`: loads a `.bgp` or legacy `.mbg` project and returns project counts plus invalid resources.
- `crazy_storm_validate_project`: validates loadability and can compile play data in memory with `compile=true`.
- `crazy_storm_export_play_data`: exports playable `.bg` data. Existing output files require `overwrite=true`.
- `crazy_storm_component_types`: lists component types available from `CrazyStorm.Core`.

### Project editing

- `crazy_storm_create_project`: creates a `.bgp` project with one particle system, one layer, and an optional default `Center`.
- `crazy_storm_add_layer`: adds a layer to a `.bgp` project.
- `crazy_storm_add_multi_emitter`: adds a `MultiEmitter` and configures common emitter and particle-template parameters.
- `crazy_storm_set_property`: sets a component or particle-template property by name. It supports runtime values and expression-backed properties.
- `crazy_storm_add_event_group`: adds raw CS2 event-group text to a component or emitter particle template.
- `crazy_storm_open_editor`: launches the CrazyStorm 2.0 editor window, optionally opening a `.bgp` or `.mbg` file.
- `crazy_storm_ping_running_editor`: checks whether an open CrazyStorm 2.0 editor window is accepting MCP commands.
- `crazy_storm_open_in_running_editor`: opens or reloads a project in an already-running editor window.

Editing tools load a project, modify it through `CrazyStorm.Core`, save it, then return a fresh summary. Legacy `.mbg` inputs require `outputPath` so the original CS1 text file is not overwritten by CS2 XML.
Set `reloadInRunningEditor=true` on create/edit tools to save through MCP and then
refresh the already-open editor window through local IPC.

`crazy_storm_open_editor` needs a built `CrazyStorm.exe`. By default it looks for the editor build output next to this repository. If you use a release package or a custom build location, pass `editorPath`.

Example flow:

```text
crazy_storm_create_project(path="pattern.bgp")
crazy_storm_add_multi_emitter(path="pattern.bgp", name="BlueBurst", emitCount=16, emitCycle=8)
crazy_storm_set_property(path="pattern.bgp", component="BlueBurst", target="particle", property="RGB", value={ "r": 80, "g": 180, "b": 255 })
crazy_storm_validate_project(path="pattern.bgp", compile=true)
crazy_storm_export_play_data(path="pattern.bgp", outputPath="pattern.bg")
crazy_storm_open_editor(path="pattern.bgp")
crazy_storm_ping_running_editor()
crazy_storm_add_multi_emitter(path="pattern.bgp", name="PinkRing", emitCount=24, reloadInRunningEditor=true)
```

`crazy_storm_add_event_group` expects event strings in the current CS2 serialized event format. Use validation with `compile=true` after adding events.

## User-facing commands

Run the executable manually only for help or diagnostics:

```powershell
.\CrazyStorm.McpServer\bin\Debug\CrazyStorm.McpServer.exe --help
.\CrazyStorm.McpServer\bin\Debug\CrazyStorm.McpServer.exe --self-test
```

MCP clients should start it with stdio. If no arguments are passed from an
interactive console, the server prints help instead of silently waiting.

## Codex install helper

For Codex Desktop, build and add the MCP server to the global Codex config:

```powershell
powershell -ExecutionPolicy Bypass -File .\CrazyStorm.McpServer\Install-CodexMcp.ps1
```

Restart Codex after installation so the MCP tool list is reloaded.
