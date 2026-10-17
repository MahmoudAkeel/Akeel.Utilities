using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using Akeel.Utilities.EF.Extensions.Models;
using Xunit;
using Criteria = Akeel.Utilities.EF.Extensions.Models.ListCriteriaDto<int,
    Akeel.Utilities.EF.Extensions.Tests.CriteriaCompatibilityTests.TestOrder,
    Akeel.Utilities.EF.Extensions.Tests.CriteriaCompatibilityTests.Related>;

namespace Akeel.Utilities.EF.Extensions.Tests
{
    public sealed class CriteriaCompatibilityTests
    {
        [Theory]
        [InlineData(null, null)]
        [InlineData(0, 0)]
        [InlineData(-1, 3)]
        [InlineData(2, -1)]
        [InlineData(2, 3)]
        public void Pagination_preserves_constructor_setter_identity_and_JSON_roundtrip(int? index, int? size)
        {
            var criteria = new Criteria();
            Assert.Null(criteria.PaginationInfo);
            var page = index.HasValue ? new RequestPaginationInfo { PageIndex = index.Value, PageSize = size!.Value } : null;
            criteria.PaginationInfo = page!;
            Assert.Same(page, criteria.PaginationInfo);
            criteria.Entity = 17;
            criteria.ListOrderOptions = [nameof(TestOrder.Id)];
            criteria.ListRelatedObjects = [nameof(Related.Notes)];

            var json = JsonSerializer.Serialize(criteria);
            var restored = JsonSerializer.Deserialize<Criteria>(json)!;
            Assert.Equal(17, restored.Entity);
            Assert.Equal(criteria.ListOrderOptions, restored.ListOrderOptions);
            Assert.Equal(criteria.ListRelatedObjects, restored.ListRelatedObjects);
            if (page is null) Assert.Null(restored.PaginationInfo);
            else
            {
                Assert.Equal(page.PageIndex, restored.PaginationInfo.PageIndex);
                Assert.Equal(page.PageSize, restored.PaginationInfo.PageSize);
            }
            Assert.DoesNotContain("ListOrderOptionsEnum", json);
            Assert.DoesNotContain("ListRelatedObjectsEnum", json);
        }

        [Fact]
        public void CreateDefault_returns_independent_legacy_state_without_reading_shared_Default()
        {
            var original = Criteria.Default;
            var shared = new Criteria
            {
                Entity = 99,
                PaginationInfo = new RequestPaginationInfo { PageIndex = 9, PageSize = 9 },
                ListOrderOptions = [nameof(TestOrder.Id)],
                ListRelatedObjects = [nameof(Related.Owner)]
            };
            try
            {
                Criteria.Default = shared;
                var first = Criteria.CreateDefault();
                var second = Criteria.CreateDefault();
                Assert.NotSame(first, second);
                Assert.NotSame(shared, first);
                Assert.Null(first.PaginationInfo);
                Assert.Null(first.ListOrderOptions);
                Assert.Null(first.ListRelatedObjects);
                first.Entity = 1;
                first.PaginationInfo = new RequestPaginationInfo { PageIndex = 1, PageSize = 2 };
                first.ListOrderOptions = [];
                first.ListRelatedObjects = [];
                first.AddNewInclude(Related.Notes);
                Assert.Equal(0, second.Entity);
                Assert.Null(second.PaginationInfo);
                Assert.Null(second.ListOrderOptions);
                Assert.Null(second.ListRelatedObjects);
                Assert.Same(shared, Criteria.Default);
                Assert.Equal(99, shared.Entity);
                Assert.Equal(9, shared.PaginationInfo.PageSize);
                Assert.Equal(new[] { nameof(TestOrder.Id) }, shared.ListOrderOptions);
                Assert.Equal(new[] { nameof(Related.Owner) }, shared.ListRelatedObjects);

                Criteria.Default = null!;
                Assert.NotNull(Criteria.CreateDefault());
            }
            finally { Criteria.Default = original; }
        }

        [Fact]
        public void Legacy_static_fields_keep_declared_types_and_assignment()
        {
            var defaultField = typeof(Criteria).GetField("Default", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)!;
            var sizeField = typeof(StaticValueDto).GetField("PageSize", BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)!;
            Assert.Equal(typeof(Criteria), defaultField.FieldType);
            Assert.Equal(typeof(int), sizeField.FieldType);
            Assert.False(defaultField.IsInitOnly);
            Assert.False(defaultField.IsLiteral);
            Assert.False(sizeField.IsInitOnly);
            Assert.False(sizeField.IsLiteral);
            var original = Criteria.Default;
            var originalSize = StaticValueDto.PageSize;
            try
            {
                var replacement = new Criteria();
                Criteria.Default = replacement;
                StaticValueDto.PageSize = 43;
                Assert.Same(replacement, Criteria.Default);
                Assert.Equal(43, StaticValueDto.PageSize);
                Assert.Null(new Criteria().PaginationInfo);
            }
            finally
            {
                Criteria.Default = original;
                StaticValueDto.PageSize = originalSize;
            }
        }

        [Fact]
        public void Base_Default_property_still_returns_fresh_instances()
        {
            var first = GetCriteriaDto<Related>.Default;
            var second = GetCriteriaDto<Related>.Default;
            Assert.NotSame(first, second);
            first.AddNewInclude(Related.Notes);
            Assert.Null(second.ListRelatedObjects);
        }

        [Fact]
        public void Derived_and_base_include_methods_keep_return_identity_and_deduplication()
        {
            var criteria = new Criteria();
            Assert.Same(criteria, criteria.AddNewInclude(Related.Notes));
            Assert.Same(criteria, criteria.AddNewInclude(Related.Notes));
            GetCriteriaDto<Related> asBase = criteria;
            Assert.Same(criteria, asBase.AddNewInclude(Related.Owner));
            Assert.Same(criteria, asBase.AddNewInclude(Related.Owner));
            Assert.Equal(new[] { nameof(Related.Notes), nameof(Related.Owner) }, criteria.ListRelatedObjects);
            var fromNull = Criteria.AddNewInclude(null!, Related.Notes);
            Assert.Equal(new[] { nameof(Related.Notes) }, fromNull.ListRelatedObjects);
            Assert.Same(fromNull, Criteria.AddNewInclude(fromNull, Related.Notes));
        }

        public enum TestOrder { Id }
        public enum Related { Notes, Owner }
    }
}
