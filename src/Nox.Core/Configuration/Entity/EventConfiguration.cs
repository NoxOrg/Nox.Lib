using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EventConfiguration : MetaBase
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}