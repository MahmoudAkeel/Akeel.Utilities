using System;
using System.Collections.Generic;
using System.Linq;
using Akeel.Utilities.EF.Extensions.Interfaces;
using Akeel.Utilities.EF.Extensions.Models;
using Microsoft.EntityFrameworkCore;

namespace Akeel.Utilities.EF.Extensions.Tests
{
    public enum RowOrder { GroupAsc, GroupDesc, AmountAsc, AmountDesc, IdAsc, IdDesc, Skipped }

    public sealed class OrderingRow
    {
        public int Id { get; set; }
        public int Group { get; set; }
        public decimal Amount { get; set; }
        public List<OrderingNote> Notes { get; set; } = [];
    }

    public sealed class OrderingNote
    {
        public int Id { get; set; }
        public int RowId { get; set; }
        public string Text { get; set; } = "";
    }

    internal sealed class RowOrderHelper : IOrderableRange<OrderingRow, RowOrder>
    {
        public OrderColumn<OrderingRow> GetOrderColumn(RowOrder order)
            => order switch
            {
                RowOrder.GroupAsc or RowOrder.GroupDesc =>
                    OrderColumn<OrderingRow, RowOrder>.CreateOrder(order, row => row.Group),
                RowOrder.AmountAsc or RowOrder.AmountDesc =>
                    OrderColumn<OrderingRow, RowOrder>.CreateOrder(order, row => row.Amount),
                RowOrder.IdAsc or RowOrder.IdDesc =>
                    OrderColumn<OrderingRow, RowOrder>.CreateOrder(order, row => row.Id),
                RowOrder.Skipped => null!,
                _ => throw new ArgumentOutOfRangeException(nameof(order))
            };
    }

    internal sealed class OrderingDbContext(DbContextOptions<OrderingDbContext> options) : DbContext(options)
    {
        public DbSet<OrderingRow> Rows => Set<OrderingRow>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var row = modelBuilder.Entity<OrderingRow>();
            row.HasKey(item => item.Id);
            row.Property(item => item.Id).ValueGeneratedNever();
            row.Property(item => item.Amount).HasPrecision(18, 4);
            row.HasMany(item => item.Notes).WithOne().HasForeignKey(note => note.RowId);
            modelBuilder.Entity<OrderingNote>().Property(note => note.Id).ValueGeneratedNever();
        }
    }

    public static class OrderingCases
    {
        public static OrderingRow[] Rows() =>
        [
            new() { Id = 8, Group = 2, Amount = 20m },
            new() { Id = 3, Group = 1, Amount = 20m },
            new() { Id = 5, Group = 2, Amount = 20m },
            new() { Id = 1, Group = 1, Amount = 20m },
            new() { Id = 6, Group = 2, Amount = 10m },
            new() { Id = 7, Group = 1, Amount = 10m },
            new() { Id = 4, Group = 2, Amount = 10m },
            new() { Id = 2, Group = 1, Amount = 10m }
        ];

        public static IEnumerable<object[]> TwoColumns()
        {
            yield return [new[] { RowOrder.AmountAsc, RowOrder.IdAsc }, new[] { 2, 4, 6, 7, 1, 3, 5, 8 }];
            yield return [new[] { RowOrder.AmountAsc, RowOrder.IdDesc }, new[] { 7, 6, 4, 2, 8, 5, 3, 1 }];
            yield return [new[] { RowOrder.AmountDesc, RowOrder.IdAsc }, new[] { 1, 3, 5, 8, 2, 4, 6, 7 }];
            yield return [new[] { RowOrder.AmountDesc, RowOrder.IdDesc }, new[] { 8, 5, 3, 1, 7, 6, 4, 2 }];
        }

        public static IEnumerable<object[]> ThreeColumns()
        {
            yield return [new[] { RowOrder.GroupAsc, RowOrder.AmountAsc, RowOrder.IdAsc }, new[] { 2, 7, 1, 3, 4, 6, 5, 8 }];
            yield return [new[] { RowOrder.GroupAsc, RowOrder.AmountAsc, RowOrder.IdDesc }, new[] { 7, 2, 3, 1, 6, 4, 8, 5 }];
            yield return [new[] { RowOrder.GroupAsc, RowOrder.AmountDesc, RowOrder.IdAsc }, new[] { 1, 3, 2, 7, 5, 8, 4, 6 }];
            yield return [new[] { RowOrder.GroupAsc, RowOrder.AmountDesc, RowOrder.IdDesc }, new[] { 3, 1, 7, 2, 8, 5, 6, 4 }];
            yield return [new[] { RowOrder.GroupDesc, RowOrder.AmountAsc, RowOrder.IdAsc }, new[] { 4, 6, 5, 8, 2, 7, 1, 3 }];
            yield return [new[] { RowOrder.GroupDesc, RowOrder.AmountAsc, RowOrder.IdDesc }, new[] { 6, 4, 8, 5, 7, 2, 3, 1 }];
            yield return [new[] { RowOrder.GroupDesc, RowOrder.AmountDesc, RowOrder.IdAsc }, new[] { 5, 8, 4, 6, 1, 3, 2, 7 }];
            yield return [new[] { RowOrder.GroupDesc, RowOrder.AmountDesc, RowOrder.IdDesc }, new[] { 8, 5, 6, 4, 3, 1, 7, 2 }];
        }

        public static List<RowOrder>? EdgeOrders(string scenario) => scenario switch
        {
            "null" => null,
            "empty" => [],
            "skipped" => [RowOrder.Skipped, RowOrder.Skipped],
            "interleaved" => [RowOrder.Skipped, RowOrder.AmountAsc, RowOrder.Skipped, RowOrder.IdDesc, RowOrder.Skipped],
            "single" => [RowOrder.IdDesc],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };

        public static void AssertResult(string scenario, IQueryable<OrderingRow> source, IQueryable<OrderingRow> query, int[] ids)
        {
            if (scenario is "null" or "empty" or "skipped")
            {
                Xunit.Assert.Same(source, query);
                Xunit.Assert.Equal(Enumerable.Range(1, 8), ids.OrderBy(id => id));
            }
            else
            {
                var expected = scenario == "single" ? new[] { 8, 7, 6, 5, 4, 3, 2, 1 } : new[] { 7, 6, 4, 2, 8, 5, 3, 1 };
                Xunit.Assert.Equal(expected, ids);
            }
        }
    }
}
