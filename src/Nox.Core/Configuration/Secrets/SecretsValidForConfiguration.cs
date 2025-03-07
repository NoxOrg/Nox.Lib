using Nox.Core.Interfaces.Secrets;

namespace Nox.Core.Configuration.Secrets;

public class SecretsValidForConfiguration : ISecretsValidFor
{
    public int? Days { get; set; }
    public int? Hours { get; set; }
    public int? Minutes { get; set; }
    public int? Seconds { get; set; }
}