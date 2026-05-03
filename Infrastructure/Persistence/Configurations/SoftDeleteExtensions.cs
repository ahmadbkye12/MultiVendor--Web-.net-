using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Configurations;

internal static class SoftDeleteExtensions
{
    public static void ApplySoftDeleteFilters(this ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            if (clrType == typeof(BaseAuditableEntity))
                continue;

            if (!typeof(BaseAuditableEntity).IsAssignableFrom(clrType))
                continue;

            typeof(SoftDeleteExtensions)
                .GetMethod(nameof(SetSoftDeleteFilter), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!
                .MakeGenericMethod(clrType)
                .Invoke(null, new object[] { builder });
        }
    }

    private static void SetSoftDeleteFilter<TEntity>(ModelBuilder builder)
        where TEntity : BaseAuditableEntity =>
        builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
}
