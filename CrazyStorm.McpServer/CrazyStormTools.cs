/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Xml;
using CsFile = CrazyStorm.Core.File;
using IoFile = System.IO.File;

namespace CrazyStorm.McpServer
{
    internal sealed class CrazyStormTools
    {
        private readonly JavaScriptSerializer json = new JavaScriptSerializer();

        public object[] ListTools()
        {
            return new object[]
            {
                Tool(
                    "crazy_storm_project_summary",
                    "Load a CrazyStorm .bgp/.mbg project and return counts for particle systems, layers, components, resources, and invalid resource references.",
                    new Dictionary<string, object>
                    {
                        { "path", StringProperty("Project file path. Relative paths resolve from the MCP server working directory.") }
                    },
                    new[] { "path" }),

                Tool(
                    "crazy_storm_validate_project",
                    "Validate that a CrazyStorm project can be loaded. Optionally compile it in memory to catch expression and event errors.",
                    new Dictionary<string, object>
                    {
                        { "path", StringProperty("Project file path. Relative paths resolve from the MCP server working directory.") },
                        { "compile", new Dictionary<string, object>
                            {
                                { "type", "boolean" },
                                { "description", "When true, also generates play data in memory to compile expressions and events." },
                                { "default", false }
                            }
                        }
                    },
                    new[] { "path" }),

                Tool(
                    "crazy_storm_export_play_data",
                    "Load a CrazyStorm project and export playable .bg data.",
                    new Dictionary<string, object>
                    {
                        { "path", StringProperty("Project file path. Relative paths resolve from the MCP server working directory.") },
                        { "outputPath", StringProperty("Optional output .bg path. Defaults to the project file name with a .bg extension.") },
                        { "overwrite", new Dictionary<string, object>
                            {
                                { "type", "boolean" },
                                { "description", "Set true to overwrite an existing output file." },
                                { "default", false }
                            }
                        }
                    },
                    new[] { "path" },
                    new Dictionary<string, object>
                    {
                        { "destructiveHint", true },
                        { "idempotentHint", false }
                    }),

                Tool(
                    "crazy_storm_component_types",
                    "List component types available in CrazyStorm.Core.",
                    new Dictionary<string, object>(),
                    new string[0]),

                Tool(
                    "crazy_storm_create_project",
                    "Create a new CrazyStorm .bgp project with one particle system, one layer, and an optional default Center component.",
                    new Dictionary<string, object>
                    {
                        { "path", StringProperty("Output .bgp project path.") },
                        { "particleSystemName", StringProperty("Optional particle system name. Defaults to the file name.") },
                        { "layerName", StringProperty("Optional initial layer name. Defaults to Layer.") },
                        { "totalFrame", NumberProperty("Optional initial layer length in frames. Defaults to 300.") },
                        { "centerX", NumberProperty("Optional Center X position. Defaults to 0.") },
                        { "centerY", NumberProperty("Optional Center Y position. Defaults to 0.") },
                        { "includeCenter", BoolProperty("When true, add a default Center component. Defaults to true.") },
                        { "overwrite", BoolProperty("Set true to overwrite an existing output file.") }
                    },
                    new[] { "path" },
                    new Dictionary<string, object>
                    {
                        { "destructiveHint", true },
                        { "idempotentHint", false }
                    }),

                Tool(
                    "crazy_storm_add_layer",
                    "Add a layer to a CrazyStorm .bgp project.",
                    new Dictionary<string, object>
                    {
                        { "path", StringProperty("Input .bgp project path.") },
                        { "outputPath", StringProperty("Optional output path. Defaults to editing path in place.") },
                        { "particleSystem", StringProperty("Particle system name or index. Defaults to 0.") },
                        { "name", StringProperty("Layer name.") },
                        { "beginFrame", NumberProperty("Layer begin frame. Defaults to 1.") },
                        { "totalFrame", NumberProperty("Layer length in frames. Defaults to 300.") },
                        { "color", StringProperty("Layer color name: Blue, Purple, Red, Green, Yellow, Orange, Pink.") },
                        { "visible", BoolProperty("Layer visibility. Defaults to true.") },
                        { "overwrite", BoolProperty("Set true to overwrite outputPath when it already exists.") }
                    },
                    new[] { "path", "name" },
                    WriteAnnotations()),

                Tool(
                    "crazy_storm_add_multi_emitter",
                    "Add a MultiEmitter to a layer and configure common emitter/particle parameters.",
                    new Dictionary<string, object>
                    {
                        { "path", StringProperty("Input .bgp project path.") },
                        { "outputPath", StringProperty("Optional output path. Defaults to editing path in place.") },
                        { "particleSystem", StringProperty("Particle system name or index. Defaults to 0.") },
                        { "layer", StringProperty("Layer name or index. Defaults to 0.") },
                        { "name", StringProperty("Emitter name.") },
                        { "parent", StringProperty("Optional parent component name or id. Defaults to Center when present.") },
                        { "x", NumberProperty("Emitter X position. Defaults to 0.") },
                        { "y", NumberProperty("Emitter Y position. Defaults to 0.") },
                        { "beginFrame", NumberProperty("Emitter begin frame. Defaults to 1.") },
                        { "totalFrame", NumberProperty("Emitter lifetime in frames. Defaults to the layer length.") },
                        { "emitCount", NumberProperty("Particles emitted per cycle. Defaults to 1.") },
                        { "emitCycle", NumberProperty("Frames between emission cycles. Defaults to 10.") },
                        { "emitAngle", NumberProperty("Base emit angle in degrees. Defaults to 0.") },
                        { "emitRange", NumberProperty("Emit spread in degrees. Defaults to 360.") },
                        { "emitRadius", NumberProperty("Radial spawn radius. Defaults to 0.") },
                        { "emitRoundAngle", NumberProperty("Radial spawn angle in degrees. Defaults to 0.") },
                        { "speed", NumberProperty("Component speed. Defaults to 0.") },
                        { "speedAngle", NumberProperty("Component speed angle. Defaults to 0.") },
                        { "particleLife", NumberProperty("Particle max life. Defaults to 120.") },
                        { "particleSpeed", NumberProperty("Particle speed. Defaults to 3.") },
                        { "particleSpeedAngle", NumberProperty("Particle speed angle. Defaults to 0.") },
                        { "particleWidthScale", NumberProperty("Particle width scale. Defaults to 1.") },
                        { "particleHeightScale", NumberProperty("Particle height scale. Defaults to width scale.") },
                        { "particleOpacity", NumberProperty("Particle opacity, 0-100. Defaults to 100.") },
                        { "particleR", NumberProperty("Particle red channel. Defaults to 255.") },
                        { "particleG", NumberProperty("Particle green channel. Defaults to 255.") },
                        { "particleB", NumberProperty("Particle blue channel. Defaults to 255.") },
                        { "collision", BoolProperty("Whether emitted particles collide with player. Defaults to true.") },
                        { "blendType", StringProperty("Particle blend type: AlphaBlend, Additive, Substraction, Multiply, None.") },
                        { "overwrite", BoolProperty("Set true to overwrite outputPath when it already exists.") }
                    },
                    new[] { "path", "name" },
                    WriteAnnotations()),

                Tool(
                    "crazy_storm_set_property",
                    "Set a component or emitted particle template property by name.",
                    new Dictionary<string, object>
                    {
                        { "path", StringProperty("Input .bgp project path.") },
                        { "outputPath", StringProperty("Optional output path. Defaults to editing path in place.") },
                        { "particleSystem", StringProperty("Particle system name or index. Defaults to 0.") },
                        { "layer", StringProperty("Layer name or index. Defaults to 0.") },
                        { "component", StringProperty("Component name or id.") },
                        { "target", StringProperty("Set to component or particle. Defaults to component.") },
                        { "property", StringProperty("Property name, for example EmitCount, Position, RGB, WidthScale.") },
                        { "value", new Dictionary<string, object>
                            {
                                { "description", "Property value. Supports numbers, booleans, strings, {x,y}, {r,g,b}, and enum names." }
                            }
                        },
                        { "expression", BoolProperty("When true, store value as a CrazyStorm expression instead of setting the runtime field.") },
                        { "overwrite", BoolProperty("Set true to overwrite outputPath when it already exists.") }
                    },
                    new[] { "path", "component", "property", "value" },
                    WriteAnnotations()),

                Tool(
                    "crazy_storm_add_event_group",
                    "Add an event group to a component or to a MultiEmitter's particle template.",
                    new Dictionary<string, object>
                    {
                        { "path", StringProperty("Input .bgp project path.") },
                        { "outputPath", StringProperty("Optional output path. Defaults to editing path in place.") },
                        { "particleSystem", StringProperty("Particle system name or index. Defaults to 0.") },
                        { "layer", StringProperty("Layer name or index. Defaults to 0.") },
                        { "component", StringProperty("Component name or id.") },
                        { "target", StringProperty("Set to component or particle. Defaults to component.") },
                        { "name", StringProperty("Event group name.") },
                        { "condition", StringProperty("CrazyStorm expression condition, for example CurrentFrame=1.") },
                        { "events", ArrayProperty("Event strings to append to the group.") },
                        { "overwrite", BoolProperty("Set true to overwrite outputPath when it already exists.") }
                    },
                    new[] { "path", "component", "name", "events" },
                    WriteAnnotations())
            };
        }

        public Dictionary<string, object> Call(string name, Dictionary<string, object> arguments)
        {
            try
            {
                switch (name)
                {
                    case "crazy_storm_project_summary":
                        return TextResult(ToJson(BuildSummary(LoadProject(GetRequiredPath(arguments)))));

                    case "crazy_storm_validate_project":
                        return TextResult(ToJson(ValidateProject(arguments)));

                    case "crazy_storm_export_play_data":
                        return TextResult(ToJson(ExportPlayData(arguments)));

                    case "crazy_storm_component_types":
                        return TextResult(ToJson(GetComponentTypes()));

                    case "crazy_storm_create_project":
                        return TextResult(ToJson(CreateProject(arguments)));

                    case "crazy_storm_add_layer":
                        return TextResult(ToJson(AddLayer(arguments)));

                    case "crazy_storm_add_multi_emitter":
                        return TextResult(ToJson(AddMultiEmitter(arguments)));

                    case "crazy_storm_set_property":
                        return TextResult(ToJson(SetProperty(arguments)));

                    case "crazy_storm_add_event_group":
                        return TextResult(ToJson(AddEventGroup(arguments)));

                    default:
                        return ErrorResult("Unknown tool: " + name);
                }
            }
            catch (Exception ex)
            {
                return ErrorResult(ex.Message);
            }
        }

        private Dictionary<string, object> ValidateProject(Dictionary<string, object> arguments)
        {
            string path = GetRequiredPath(arguments);
            bool compile = McpProtocol.GetBool(arguments, "compile", false);

            var loaded = LoadProject(path);
            byte[] playBytes = null;
            if (compile) playBytes = loaded.Project.GeneratePlayFile();

            var summary = BuildSummary(loaded);
            summary["valid"] = true;
            summary["compiled"] = compile;
            if (playBytes != null) summary["playDataBytes"] = playBytes.Length;
            return summary;
        }

        private Dictionary<string, object> ExportPlayData(Dictionary<string, object> arguments)
        {
            string path = GetRequiredPath(arguments);
            string outputPath = McpProtocol.GetString(arguments, "outputPath");
            bool overwrite = McpProtocol.GetBool(arguments, "overwrite", false);

            var loaded = LoadProject(path);
            string resolvedOutputPath = ResolveOutputPath(loaded.Path, outputPath);
            if (IoFile.Exists(resolvedOutputPath) && !overwrite)
            {
                throw new InvalidOperationException("Output already exists. Pass overwrite=true to replace it: " + resolvedOutputPath);
            }

            byte[] bytes = loaded.Project.GeneratePlayFile();
            IoFile.WriteAllBytes(resolvedOutputPath, bytes);

            return new Dictionary<string, object>
            {
                { "projectPath", loaded.Path },
                { "outputPath", resolvedOutputPath },
                { "bytes", bytes.Length },
                { "overwritten", overwrite }
            };
        }

        private Dictionary<string, object> CreateProject(Dictionary<string, object> arguments)
        {
            string path = ResolvePath(GetRequiredPath(arguments));
            bool overwrite = McpProtocol.GetBool(arguments, "overwrite", false);
            if (IoFile.Exists(path) && !overwrite)
                throw new InvalidOperationException("Output already exists. Pass overwrite=true to replace it: " + path);

            string particleSystemName = GetOptionalString(arguments, "particleSystemName", Path.GetFileNameWithoutExtension(path));
            string layerName = GetOptionalString(arguments, "layerName", "Layer");
            int totalFrame = GetOptionalInt(arguments, "totalFrame", 300);
            bool includeCenter = McpProtocol.GetBool(arguments, "includeCenter", true);

            var project = new CsFile();
            var system = new ParticleSystem(project, particleSystemName);
            var layer = new Layer(layerName)
            {
                BeginFrame = 1,
                TotalFrame = totalFrame,
                Visible = true
            };
            system.AddLayer(layer);
            project.ParticleSystems.Add(system);

            if (includeCenter)
            {
                var center = new Center
                {
                    Name = CsFile.DefaultCenterName,
                    ID = system.GetComponentIndex(),
                    System = system,
                    Globals = project.Globals,
                    BeginFrame = 1,
                    TotalFrame = totalFrame,
                    Position = new Vector2(GetOptionalFloat(arguments, "centerX", 0), GetOptionalFloat(arguments, "centerY", 0))
                };
                system.GetAndIncreaseComponentIndex(center.GetType().ToString());
                system.AddComponentToLayer(layer, center);
            }

            SaveProject(project, path);
            var loaded = LoadProject(path);
            var summary = BuildSummary(loaded);
            summary["created"] = true;
            return summary;
        }

        private Dictionary<string, object> AddLayer(Dictionary<string, object> arguments)
        {
            var edit = LoadEditableProject(arguments);
            var system = FindParticleSystem(edit.Project, McpProtocol.GetString(arguments, "particleSystem"));
            var layer = new Layer(McpProtocol.GetString(arguments, "name"))
            {
                BeginFrame = GetOptionalInt(arguments, "beginFrame", 1),
                TotalFrame = GetOptionalInt(arguments, "totalFrame", 300),
                Visible = McpProtocol.GetBool(arguments, "visible", true)
            };

            string color = McpProtocol.GetString(arguments, "color");
            if (!string.IsNullOrWhiteSpace(color))
                layer.Color = ParseEnum<LayerColor>(color);

            system.AddLayer(layer);
            return SaveAndSummarize(edit.Project, edit.OutputPath, "layerAdded", layer.Name);
        }

        private Dictionary<string, object> AddMultiEmitter(Dictionary<string, object> arguments)
        {
            var edit = LoadEditableProject(arguments);
            var system = FindParticleSystem(edit.Project, McpProtocol.GetString(arguments, "particleSystem"));
            var layer = FindLayer(system, McpProtocol.GetString(arguments, "layer"));
            var parent = FindOptionalParent(layer, McpProtocol.GetString(arguments, "parent"));

            var emitter = new MultiEmitter
            {
                Name = McpProtocol.GetString(arguments, "name"),
                ID = system.GetComponentIndex(),
                System = system,
                Globals = edit.Project.Globals,
                Parent = parent,
                ParentID = parent != null ? parent.ID : -1,
                BeginFrame = GetOptionalInt(arguments, "beginFrame", 1),
                TotalFrame = GetOptionalInt(arguments, "totalFrame", layer.TotalFrame),
                Position = new Vector2(GetOptionalFloat(arguments, "x", 0), GetOptionalFloat(arguments, "y", 0)),
                Speed = GetOptionalFloat(arguments, "speed", 0),
                SpeedAngle = GetOptionalFloat(arguments, "speedAngle", 0),
                EmitCount = GetOptionalInt(arguments, "emitCount", 1),
                EmitCycle = GetOptionalInt(arguments, "emitCycle", 10),
                EmitAngle = GetOptionalFloat(arguments, "emitAngle", 0),
                EmitRange = GetOptionalFloat(arguments, "emitRange", 360),
                EmitRadius = GetOptionalFloat(arguments, "emitRadius", 0),
                EmitRoundAngle = GetOptionalFloat(arguments, "emitRoundAngle", 0)
            };
            system.GetAndIncreaseComponentIndex(emitter.GetType().ToString());

            var particle = emitter.InitialTemplate as Particle;
            if (particle != null)
            {
                float widthScale = GetOptionalFloat(arguments, "particleWidthScale", 1);
                particle.MaxLife = GetOptionalInt(arguments, "particleLife", 120);
                particle.PSpeed = GetOptionalFloat(arguments, "particleSpeed", 3);
                particle.PSpeedAngle = GetOptionalFloat(arguments, "particleSpeedAngle", 0);
                particle.WidthScale = widthScale;
                particle.HeightScale = GetOptionalFloat(arguments, "particleHeightScale", widthScale);
                particle.Opacity = GetOptionalFloat(arguments, "particleOpacity", 100);
                particle.RGB = new RGB(
                    GetOptionalFloat(arguments, "particleR", 255),
                    GetOptionalFloat(arguments, "particleG", 255),
                    GetOptionalFloat(arguments, "particleB", 255));
                particle.Collision = McpProtocol.GetBool(arguments, "collision", true);

                string blendType = McpProtocol.GetString(arguments, "blendType");
                if (!string.IsNullOrWhiteSpace(blendType))
                    particle.BlendType = ParseEnum<BlendType>(blendType);
            }

            system.AddComponentToLayer(layer, emitter);
            return SaveAndSummarize(edit.Project, edit.OutputPath, "componentAdded", emitter.Name);
        }

        private Dictionary<string, object> SetProperty(Dictionary<string, object> arguments)
        {
            var edit = LoadEditableProject(arguments);
            var system = FindParticleSystem(edit.Project, McpProtocol.GetString(arguments, "particleSystem"));
            var layer = FindLayer(system, McpProtocol.GetString(arguments, "layer"));
            var component = FindComponent(layer, McpProtocol.GetString(arguments, "component"));
            var target = GetPropertyTarget(component, McpProtocol.GetString(arguments, "target"));
            string propertyName = McpProtocol.GetString(arguments, "property");
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentException("property is required.");

            object rawValue;
            if (!arguments.TryGetValue("value", out rawValue)) throw new ArgumentException("value is required.");

            bool expression = McpProtocol.GetBool(arguments, "expression", false);
            if (expression)
            {
                target.Properties[propertyName] = new PropertyValue
                {
                    Expression = true,
                    Value = Convert.ToString(rawValue)
                };
            }
            else
            {
                SetRuntimeProperty(target, propertyName, rawValue);
            }

            return SaveAndSummarize(edit.Project, edit.OutputPath, "propertySet", propertyName);
        }

        private Dictionary<string, object> AddEventGroup(Dictionary<string, object> arguments)
        {
            var edit = LoadEditableProject(arguments);
            var system = FindParticleSystem(edit.Project, McpProtocol.GetString(arguments, "particleSystem"));
            var layer = FindLayer(system, McpProtocol.GetString(arguments, "layer"));
            var component = FindComponent(layer, McpProtocol.GetString(arguments, "component"));
            string target = GetOptionalString(arguments, "target", "component");

            var group = new EventGroup
            {
                Name = McpProtocol.GetString(arguments, "name"),
                Condition = GetOptionalString(arguments, "condition", string.Empty)
            };

            foreach (var item in GetStringArray(arguments, "events"))
                group.Events.Add(item);

            if (string.Equals(target, "particle", StringComparison.OrdinalIgnoreCase))
            {
                var emitter = component as Emitter;
                if (emitter == null) throw new InvalidOperationException("Particle event groups can only be added to emitters.");
                emitter.ParticleEventGroups.Add(group);
            }
            else
            {
                component.ComponentEventGroups.Add(group);
            }

            return SaveAndSummarize(edit.Project, edit.OutputPath, "eventGroupAdded", group.Name);
        }

        private EditableProject LoadEditableProject(Dictionary<string, object> arguments)
        {
            var loaded = LoadProject(GetRequiredPath(arguments));
            string outputPath = McpProtocol.GetString(arguments, "outputPath");
            if (loaded.IsLegacyCs1 && string.IsNullOrWhiteSpace(outputPath))
                throw new InvalidOperationException("Editing legacy .mbg input requires outputPath so the original CS1 text file is not overwritten.");

            string resolvedOutput = string.IsNullOrWhiteSpace(outputPath) ? loaded.Path : ResolvePath(outputPath);
            bool overwrite = McpProtocol.GetBool(arguments, "overwrite", false);
            if (!string.Equals(loaded.Path, resolvedOutput, StringComparison.OrdinalIgnoreCase) &&
                IoFile.Exists(resolvedOutput) && !overwrite)
            {
                throw new InvalidOperationException("Output already exists. Pass overwrite=true to replace it: " + resolvedOutput);
            }

            return new EditableProject
            {
                Project = loaded.Project,
                SourcePath = loaded.Path,
                OutputPath = resolvedOutput
            };
        }

        private Dictionary<string, object> SaveAndSummarize(CsFile project, string path, string action, object value)
        {
            SaveProject(project, path);
            var loaded = LoadProject(path);
            var summary = BuildSummary(loaded);
            summary[action] = value;
            return summary;
        }

        private void SaveProject(CsFile project, string path)
        {
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            project.Save(path);
        }

        private ParticleSystem FindParticleSystem(CsFile project, string selector)
        {
            if (project.ParticleSystems.Count == 0) throw new InvalidOperationException("Project has no particle systems.");
            if (string.IsNullOrWhiteSpace(selector)) return project.ParticleSystems[0];

            int index;
            if (int.TryParse(selector, out index))
            {
                if (index < 0 || index >= project.ParticleSystems.Count)
                    throw new IndexOutOfRangeException("Particle system index out of range: " + selector);
                return project.ParticleSystems[index];
            }

            var system = project.ParticleSystems.FirstOrDefault(item => string.Equals(item.Name, selector, StringComparison.OrdinalIgnoreCase));
            if (system == null) throw new InvalidOperationException("Particle system not found: " + selector);
            return system;
        }

        private Layer FindLayer(ParticleSystem system, string selector)
        {
            if (system.Layers.Count == 0) throw new InvalidOperationException("Particle system has no layers.");
            if (string.IsNullOrWhiteSpace(selector)) return system.Layers[0];

            int index;
            if (int.TryParse(selector, out index))
            {
                if (index < 0 || index >= system.Layers.Count)
                    throw new IndexOutOfRangeException("Layer index out of range: " + selector);
                return system.Layers[index];
            }

            var layer = system.Layers.FirstOrDefault(item => string.Equals(item.Name, selector, StringComparison.OrdinalIgnoreCase));
            if (layer == null) throw new InvalidOperationException("Layer not found: " + selector);
            return layer;
        }

        private Component FindComponent(Layer layer, string selector)
        {
            if (string.IsNullOrWhiteSpace(selector)) throw new ArgumentException("component is required.");

            long id;
            if (long.TryParse(selector, out id))
            {
                var byId = layer.Components.FirstOrDefault(item => item.ID == id);
                if (byId != null) return byId;
            }

            var byName = layer.Components.FirstOrDefault(item => string.Equals(item.Name, selector, StringComparison.OrdinalIgnoreCase));
            if (byName == null) throw new InvalidOperationException("Component not found: " + selector);
            return byName;
        }

        private Component FindOptionalParent(Layer layer, string selector)
        {
            if (!string.IsNullOrWhiteSpace(selector)) return FindComponent(layer, selector);
            return layer.Components.FirstOrDefault(item => item is Center && string.Equals(item.Name, CsFile.DefaultCenterName, StringComparison.OrdinalIgnoreCase));
        }

        private PropertyContainer GetPropertyTarget(Component component, string target)
        {
            if (string.IsNullOrWhiteSpace(target) || string.Equals(target, "component", StringComparison.OrdinalIgnoreCase))
                return component;

            if (string.Equals(target, "particle", StringComparison.OrdinalIgnoreCase))
            {
                var emitter = component as Emitter;
                if (emitter == null) throw new InvalidOperationException("target=particle requires an emitter component.");
                return emitter.InitialTemplate;
            }

            throw new InvalidOperationException("Unknown property target: " + target);
        }

        private void SetRuntimeProperty(PropertyContainer target, string propertyName, object rawValue)
        {
            var property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property == null) throw new InvalidOperationException("Property not found on " + target.GetType().Name + ": " + propertyName);
            if (!property.CanWrite) throw new InvalidOperationException("Property is read-only: " + propertyName);

            object value = ConvertPropertyValue(property.PropertyType, rawValue);
            property.GetSetMethod().Invoke(target, new[] { value });
        }

        private object ConvertPropertyValue(Type type, object rawValue)
        {
            if (type == typeof(string)) return Convert.ToString(rawValue);
            if (type == typeof(bool)) return ConvertToBool(rawValue);
            if (type == typeof(int)) return Convert.ToInt32(rawValue);
            if (type == typeof(float)) return Convert.ToSingle(rawValue);
            if (type == typeof(double)) return Convert.ToDouble(rawValue);
            if (type == typeof(Vector2)) return ConvertToVector2(rawValue);
            if (type == typeof(RGB)) return ConvertToRgb(rawValue);
            if (type.IsEnum) return ParseEnum(type, Convert.ToString(rawValue));

            throw new InvalidOperationException("Unsupported property type: " + type.Name);
        }

        private bool ConvertToBool(object rawValue)
        {
            if (rawValue is bool) return (bool)rawValue;
            return bool.Parse(Convert.ToString(rawValue));
        }

        private Vector2 ConvertToVector2(object rawValue)
        {
            var map = rawValue as Dictionary<string, object>;
            if (map != null)
            {
                return new Vector2(GetRequiredFloat(map, "x"), GetRequiredFloat(map, "y"));
            }

            string text = Convert.ToString(rawValue);
            var parts = text.Split(',');
            if (parts.Length != 2) throw new InvalidOperationException("Vector2 value must be {x,y} or \"x,y\".");
            return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
        }

        private RGB ConvertToRgb(object rawValue)
        {
            var map = rawValue as Dictionary<string, object>;
            if (map != null)
            {
                return new RGB(GetRequiredFloat(map, "r"), GetRequiredFloat(map, "g"), GetRequiredFloat(map, "b"));
            }

            string text = Convert.ToString(rawValue);
            var parts = text.Split(',');
            if (parts.Length != 3) throw new InvalidOperationException("RGB value must be {r,g,b} or \"r,g,b\".");
            return new RGB(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
        }

        private T ParseEnum<T>(string value) where T : struct
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        private object ParseEnum(Type type, string value)
        {
            return Enum.Parse(type, value, true);
        }

        private string GetOptionalString(Dictionary<string, object> arguments, string name, string defaultValue)
        {
            string value = McpProtocol.GetString(arguments, name);
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        private int GetOptionalInt(Dictionary<string, object> arguments, string name, int defaultValue)
        {
            object value;
            if (!arguments.TryGetValue(name, out value) || value == null) return defaultValue;
            return Convert.ToInt32(value);
        }

        private float GetOptionalFloat(Dictionary<string, object> arguments, string name, float defaultValue)
        {
            object value;
            if (!arguments.TryGetValue(name, out value) || value == null) return defaultValue;
            return Convert.ToSingle(value);
        }

        private float GetRequiredFloat(Dictionary<string, object> arguments, string name)
        {
            object value;
            if (!arguments.TryGetValue(name, out value) || value == null) throw new ArgumentException(name + " is required.");
            return Convert.ToSingle(value);
        }

        private string[] GetStringArray(Dictionary<string, object> arguments, string name)
        {
            object value;
            if (!arguments.TryGetValue(name, out value) || value == null) return new string[0];

            var array = value as object[];
            if (array != null) return array.Select(Convert.ToString).ToArray();

            var list = value as IEnumerable<object>;
            if (list != null) return list.Select(Convert.ToString).ToArray();

            var arrayList = value as ArrayList;
            if (arrayList != null) return arrayList.Cast<object>().Select(Convert.ToString).ToArray();

            return new[] { Convert.ToString(value) };
        }

        private LoadedProject LoadProject(string path)
        {
            string fullPath = ResolvePath(path);
            if (!IoFile.Exists(fullPath)) throw new FileNotFoundException("Project file not found.", fullPath);

            var project = new CsFile();
            project.Load(fullPath);

            return new LoadedProject
            {
                Path = fullPath,
                Project = project,
                Version = ReadProjectVersion(fullPath),
                IsLegacyCs1 = CsFile.IsCS1(fullPath)
            };
        }

        private Dictionary<string, object> BuildSummary(LoadedProject loaded)
        {
            var systems = new List<object>();
            int layerCount = 0;
            int componentCount = 0;
            int noteCount = 0;
            var componentTypes = new Dictionary<string, int>();

            foreach (var system in loaded.Project.ParticleSystems)
            {
                int systemComponentCount = 0;
                var layerSummaries = new List<object>();

                foreach (var layer in system.Layers)
                {
                    layerCount++;
                    componentCount += layer.Components.Count;
                    systemComponentCount += layer.Components.Count;

                    foreach (var component in layer.Components)
                    {
                        string typeName = component.GetType().Name;
                        if (!componentTypes.ContainsKey(typeName)) componentTypes[typeName] = 0;
                        componentTypes[typeName]++;
                    }

                    layerSummaries.Add(new Dictionary<string, object>
                    {
                        { "name", layer.Name },
                        { "visible", layer.Visible },
                        { "beginFrame", layer.BeginFrame },
                        { "totalFrame", layer.TotalFrame },
                        { "componentCount", layer.Components.Count }
                    });
                }

                noteCount += system.Notes.Count;
                systems.Add(new Dictionary<string, object>
                {
                    { "name", system.Name },
                    { "totalFrame", system.TotalFrame },
                    { "layerCount", system.Layers.Count },
                    { "componentCount", systemComponentCount },
                    { "customTypeCount", system.CustomTypes.Count },
                    { "customDistortTypeCount", system.CustomDistortTypes.Count },
                    { "customMaskTypeCount", system.CustomMaskTypes.Count },
                    { "noteCount", system.Notes.Count },
                    { "layers", layerSummaries.ToArray() }
                });
            }

            var invalidResources = GetInvalidResources(loaded.Project);

            return new Dictionary<string, object>
            {
                { "path", loaded.Path },
                { "format", loaded.IsLegacyCs1 ? "Crazy Storm 1.x mbg" : "Crazy Storm 2 bgp" },
                { "version", loaded.Version },
                { "particleSystemCount", loaded.Project.ParticleSystems.Count },
                { "layerCount", layerCount },
                { "componentCount", componentCount },
                { "noteCount", noteCount },
                { "imageCount", loaded.Project.Images.Count },
                { "soundCount", loaded.Project.Sounds.Count },
                { "globalVariableCount", loaded.Project.Globals.Count },
                { "invalidResourceCount", invalidResources.Count },
                { "invalidResources", invalidResources.ToArray() },
                { "componentTypes", componentTypes },
                { "particleSystems", systems.ToArray() }
            };
        }

        private List<object> GetInvalidResources(CsFile project)
        {
            var invalid = new List<object>();
            AppendInvalidResources(invalid, "image", project.Images);
            AppendInvalidResources(invalid, "sound", project.Sounds);
            return invalid;
        }

        private void AppendInvalidResources(List<object> invalid, string kind, IEnumerable<FileResource> resources)
        {
            foreach (var resource in resources)
            {
                if (resource.IsValid) continue;
                invalid.Add(new Dictionary<string, object>
                {
                    { "kind", kind },
                    { "id", resource.ID },
                    { "label", resource.Label },
                    { "path", resource.AbsolutePath }
                });
            }
        }

        private Dictionary<string, object> GetComponentTypes()
        {
            var types = typeof(Component).Assembly.GetTypes()
                .Where(t => t != typeof(Component) && !t.IsAbstract && typeof(Component).IsAssignableFrom(t))
                .OrderBy(t => t.Name)
                .Select(t => t.Name)
                .ToArray();

            return new Dictionary<string, object>
            {
                { "componentTypes", types }
            };
        }

        private string ReadProjectVersion(string path)
        {
            if (CsFile.IsCS1(path)) return "1.01";

            var doc = new XmlDocument();
            doc.Load(path);
            var root = (XmlElement)doc.SelectSingleNode(VersionInfo.AppName.Replace(" ", ""));
            if (root == null) return null;
            return root.GetAttribute("version");
        }

        private static string ResolvePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("path is required.");
            return Path.GetFullPath(Environment.ExpandEnvironmentVariables(path));
        }

        private static string ResolveOutputPath(string projectPath, string outputPath)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                return Path.ChangeExtension(projectPath, ".bg");
            }

            return Path.GetFullPath(Environment.ExpandEnvironmentVariables(outputPath));
        }

        private static string GetRequiredPath(Dictionary<string, object> arguments)
        {
            string path = McpProtocol.GetString(arguments, "path");
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("path is required.");
            return path;
        }

        private string ToJson(object value)
        {
            return json.Serialize(value);
        }

        private static Dictionary<string, object> TextResult(string text)
        {
            return new Dictionary<string, object>
            {
                { "content", new object[]
                    {
                        new Dictionary<string, object>
                        {
                            { "type", "text" },
                            { "text", text }
                        }
                    }
                }
            };
        }

        private static Dictionary<string, object> ErrorResult(string text)
        {
            var result = TextResult(text);
            result["isError"] = true;
            return result;
        }

        private static Dictionary<string, object> Tool(string name, string description, Dictionary<string, object> properties, string[] required)
        {
            return Tool(name, description, properties, required, null);
        }

        private static Dictionary<string, object> Tool(string name, string description, Dictionary<string, object> properties, string[] required, Dictionary<string, object> annotations)
        {
            var tool = new Dictionary<string, object>
            {
                { "name", name },
                { "description", description },
                { "inputSchema", new Dictionary<string, object>
                    {
                        { "type", "object" },
                        { "properties", properties },
                        { "required", required },
                        { "additionalProperties", false }
                    }
                }
            };

            if (annotations != null) tool["annotations"] = annotations;
            return tool;
        }

        private static Dictionary<string, object> StringProperty(string description)
        {
            return new Dictionary<string, object>
            {
                { "type", "string" },
                { "description", description }
            };
        }

        private static Dictionary<string, object> NumberProperty(string description)
        {
            return new Dictionary<string, object>
            {
                { "type", "number" },
                { "description", description }
            };
        }

        private static Dictionary<string, object> BoolProperty(string description)
        {
            return new Dictionary<string, object>
            {
                { "type", "boolean" },
                { "description", description }
            };
        }

        private static Dictionary<string, object> ArrayProperty(string description)
        {
            return new Dictionary<string, object>
            {
                { "type", "array" },
                { "description", description },
                { "items", new Dictionary<string, object>
                    {
                        { "type", "string" }
                    }
                }
            };
        }

        private static Dictionary<string, object> WriteAnnotations()
        {
            return new Dictionary<string, object>
            {
                { "destructiveHint", true },
                { "idempotentHint", false }
            };
        }

        private sealed class LoadedProject
        {
            public string Path { get; set; }
            public CsFile Project { get; set; }
            public string Version { get; set; }
            public bool IsLegacyCs1 { get; set; }
        }

        private sealed class EditableProject
        {
            public CsFile Project { get; set; }
            public string SourcePath { get; set; }
            public string OutputPath { get; set; }
        }
    }
}
