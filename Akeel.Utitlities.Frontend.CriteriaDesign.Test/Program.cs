using System;
using System.Collections.Generic;
using Akeel.Utilities.Frontend.CriteriaDesign;

namespace Akeel.Utitlities.Frontend.CriteriaDesign.Test
{
    internal class Program
    {
        static void Main(string[] args)
        {

            ListCriteria<FilterDto> criteria = new ListCriteria<FilterDto>().AddNewInclude(IncludeEnum.IncludeWorkFlow);



            string queryString, queryString2;
            //var queryString = QueryStringBuilder.Instance
            //    .Add(new QueryStringRecord("Id", "5"),
            //        new QueryStringRecord("Name", "Test"),
            //        new QueryStringRecord("Name", "Test"))
            //    .Build();

            ListCriteriaDto<FilterDto, OrderEnum, IncludeEnum> criteria = new();
            criteria.AddNewInclude(IncludeEnum.IncludeWorkFlow).AddNewInclude(IncludeEnum.IncludeWorkFlowVersions);

            criteria.ListOrderOptions = new List<string>() { "IdAsc", "IdDesc" };


            criteria.Entity = new FilterDto
            {
                Id = 1,
                IsActive = true,
                NameAr = "Ahmed",
                Numbers = new List<int>() { 10, 20, 30 },
                Orders = new List<OrderEnum>() { OrderEnum.IdAsc, OrderEnum.IdDesc },
                AnotherFilter = new AnotherFilter()
                {
                    Id = 500
                }
            };
            queryString = QueryStringBuilder.Instance.AddComplexObject(criteria).Build();

            Console.WriteLine(queryString);

            Console.WriteLine("======================================");
            Console.WriteLine();

            queryString2 = QueryStringBuilder.Instance.AddComplexObjectRecursive(criteria).Build();
            Console.WriteLine(queryString2);

            Console.WriteLine("======================================");
            Console.WriteLine(queryString == queryString2);


            Console.ReadLine();

        }



    }


    class FilterDto
    {
        public string NameAr { get; set; }

        public int? Id { get; set; }

        public bool? IsActive { get; set; }

        public List<int> Numbers { get; set; }

        public List<OrderEnum> Orders { get; set; }

        public AnotherFilter AnotherFilter { get; set; }
    }


    class AnotherFilter
    {
        public int Id { get; set; }
    }

    enum OrderEnum
    {
        IdAsc,
        IdDesc
    }

    enum IncludeEnum
    {
        IncludeWorkFlow,
        IncludeWorkFlowVersions
    }
}
