namespace Dalba.Financeiro.Application.Common;

public static class DbClock
{
    public static DateTime Now => DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
}
