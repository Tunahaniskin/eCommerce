using System;

namespace ECommerce.API.Infrastructure.Logging;

[AttributeUsage(AttributeTargets.Class)]
public class LoggableAttribute : Attribute
{
    public string Description { get; }
    public AppLogLevel Level { get; }

    public LoggableAttribute(string description = "", AppLogLevel level = AppLogLevel.Info)
    {
        Description = description;
        Level = level;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class DoNotLogAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class PerformanceThresholdAttribute : Attribute
{
    public int MaxMilliseconds { get; }

    public PerformanceThresholdAttribute(int maxMilliseconds)
    {
        MaxMilliseconds = maxMilliseconds;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class SensitiveDataAttribute : Attribute
{
}
