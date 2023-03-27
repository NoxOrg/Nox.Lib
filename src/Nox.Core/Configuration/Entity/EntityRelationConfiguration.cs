using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EntityRelationConfiguration : MetaBase
{
    public string Name { get; set; } = string.Empty;

    public string Entity { get; set; } = string.Empty;

    public bool IsCollection { get; set; } = false;

    public bool IsRequired { get; set; } = false;
}