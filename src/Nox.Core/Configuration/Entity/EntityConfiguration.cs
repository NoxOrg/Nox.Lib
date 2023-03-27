using Nox.Core.Components;

namespace Nox.Core.Configuration;

public class EntityConfiguration : MetaBase
{
    public List<EntityAttributeConfiguration> Attributes { get; set; } = new();
    public List<MessageTargetConfiguration>? Messaging { get; set; } = new();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PluralName { get; set; } = string.Empty;
    public List<string>? RelatedParents { get; set; }
    public string Schema { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
    public string Table { get; set; } = string.Empty;

    public bool IsAggregateRoot { get; set; } = false;
    public bool HasDefaultCrudEvents { get; set; } = false;

    // TODO: add proper configuration entities
    public object? RaiseCrudEvents { get; set; } = new();
    public List<EntityRelationConfiguration>? Relations { get; set; } = new();
    public List<object>? Commands { get; set; } = new();
    public List<object>? Events { get; set; } = new();
    public List<object>? Queries { get; set; } = new();
}