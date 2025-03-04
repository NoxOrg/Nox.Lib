﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Nox.Generator.Generators
{
    internal abstract class BaseGenerator
    {
        protected GeneratorExecutionContext Context { get; }

        private protected BaseGenerator(GeneratorExecutionContext context)
        {
            Context = context;
        }

        public static string ToLowerFirstChar(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return char.ToLower(input[0]) + input.Substring(1);
        }

        protected void GenerateFile(StringBuilder sb, string className)
        {
            var hintName = $"{className}.g.cs";
            var source = SourceText.From(sb.ToString(), Encoding.UTF8);

            Context.AddSource(hintName, source);
        }

        protected IReadOnlyList<string> AddRelationships(Dictionary<object, object> dto, StringBuilder sb, string key = "relationships")
        {
            var relatedEntities = new List<string>();
            dto.TryGetValue(key, out var relations);
            if (relations != null)
            {
                foreach (var attr in ((List<object>)relations).Cast<Dictionary<object, object>>())
                {
                    relatedEntities.Add(AddRelationship(sb, attr));
                }
            }

            return relatedEntities.Distinct().ToList();
        }

        protected string AddRelationship(StringBuilder sb, Dictionary<object, object> attr)
        {
            bool allowNavigation = GetBooleanValueOrDefault(attr, "allow-navigation", true);
            var entity = (string)attr["entity"];

            if (allowNavigation)
            {
                var relationship = (string)attr["relationship"];
                
                bool isMany = relationship.Equals("ZeroOrMany") || relationship.Equals("OneOrMany");
                bool isRequired = relationship.Equals("ExactlyOne");

                string? typeDefinition;
                if (isMany)
                {
                    typeDefinition = $"IList<{entity}>";
                }
                else
                {
                    typeDefinition = isRequired ? $"{entity}" : $"{entity}?";
                }

                AddProperty(typeDefinition, (string)attr["name"], sb);
            }

            return entity;
        }

        protected static void AddSimpleProperty(object type, object name, bool isRequired, StringBuilder sb)
        {
            var typeName = ClassDataType((string)type);
            // Do not generate "string?" - TODO: make configurable
            AddProperty(isRequired || typeName == "string" ? typeName : $"{typeName}?", (string)name, sb);
        }

        protected static void AddProperty(string type, string name, StringBuilder sb, bool initOnly = false)
        {
            sb.AppendLine($@"   public {type} {name} {{ get; {(initOnly ? "init" : "set")}; }}");
            sb.AppendLine($@"");
        }

        protected static void AddConstructor(StringBuilder sb, string className, Dictionary<string, string> parameters)
        {
            sb.AppendLine($@"   public {className}(");
            for (int i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters.ElementAt(i);
                sb.AppendLine($@"      {parameter.Key} {ToLowerFirstChar(parameter.Value)}{(i < parameters.Count - 1 ? "," : "")}");
            }
            sb.AppendLine($@"      )");
            sb.AppendLine($@"   {{");
            foreach (var value in parameters.Select(p => p.Value))
            {
                sb.AppendLine($@"      {value} = {ToLowerFirstChar(value)};");
            }
            sb.AppendLine($@"   }}");
            sb.AppendLine($@"");
        }

        protected static string GetParametersString(object entity, bool withDefaults = true)
        {
            return string.Join(", ", ((List<object>)entity).Cast<Dictionary<object, object>>()
                .Select(parameter => $"{parameter["type"]} {parameter["name"]}{(withDefaults ? GetDefaultIfDefined(parameter, "defaultValue") : string.Empty)}"));
        }

        protected static string GetParametersExecuteString(object entity)
        {
            return string.Join(", ", ((List<object>)entity).Cast<Dictionary<object, object>>()
                .Select(parameter => $"{parameter["name"]}"));
        }

        protected static void AddDbContextProperty(StringBuilder sb)
        {
            AddProperty("NoxDomainDbContext", "DbContext", sb, initOnly: true);
        }

        protected static void AddAttributes(Dictionary<object, object> entity, StringBuilder sb)
        {
            var attributes = (List<object>)entity["attributes"];

            foreach (var attr in attributes.Cast<Dictionary<object, object>>())
            {
                AddSimpleProperty(attr["type"], attr["name"], GetBooleanValueOrDefault(attr, "isRequired"), sb);
            }
        }

        protected static void AddNoxMessangerProperty(StringBuilder sb)
        {
            AddProperty("INoxMessenger", "Messenger", sb, initOnly: true);
        }

        protected static void AddBaseTypeDefinition(
            StringBuilder sb,
            string className,
            string? parent,
            string noxNamespace,
            bool isAbstract = false,
            bool isPartial = false,
            params string[] namespaces)
        {
            sb.AppendLine($@"// autogenerated");
            foreach (var val in namespaces)
            {
                sb.AppendLine($@"using {val};");
            }

            sb.AppendLine($@"");
            sb.AppendLine($@"namespace {noxNamespace};");
            sb.AppendLine($@"");
            sb.AppendLine($@"public {(isAbstract ? "abstract " : string.Empty)}{(isPartial ? "partial " : string.Empty)}class {className} {(parent == null ? string.Empty : $": {parent}")}");
            sb.AppendLine($@"{{");
        }

        protected static bool GetBooleanValueOrDefault(Dictionary<object, object> entity, string key, bool defaultValue = false)
        {
            entity.TryGetValue(key, out object val);

            var valString = (string)val;

            // cover yes/no and true/false
            return valString != null
                && (valString.Equals("yes") || !valString.Equals("no")
                && bool.Parse(valString))
                || valString == null && defaultValue;
        }

        protected static string ClassDataType(string type)
        {
            var propType = type?.ToLower() ?? "string";

            return propType switch
            {
                "string" => "string",
                "varchar" => "string",
                "nvarchar" => "string",
                "char" => "string",
                "guid" => "Guid",
                "url" => "string",
                "email" => "string",
                "date" => "DateTime",
                "time" => "DateTime",
                "timespan" => "TimeSpan",
                "datetime" => "DateTimeOffset",
                "bool" => "bool",
                "boolean" => "bool",
                "object" => "object",
                "int" => "int",
                "uint" => "uint",
                "tinyint" => "int",
                "bigint" => "long",
                "money" => "decimal",
                "smallmoney" => "decimal",
                "decimal" => "decimal",
                "real" => "single",
                "float" => "single",
                "bigreal" => "double",
                "bigfloat" => "double",
                _ => "string"
            };
        }

        private static string GetDefaultIfDefined(Dictionary<object, object> parameter, string key)
        {
            if (!parameter.TryGetValue(key, out object val))
            {
                return string.Empty;
            }

            return $" = {val ?? "null"}";
        }
    }
}
