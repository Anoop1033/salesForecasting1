using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.IO;

namespace salesForecasting1
{
    class Program
    {
        static void Main(string[] args)
        {
           
            var connectionString = "Data Source=ANOOP;Initial Catalog=SalesForecasting;Integrated Security=True";
            var connection = new SqlConnection(connectionString);

           
            while (true)
            {
                Console.WriteLine("Sales Forecasting Application");
                Console.WriteLine("1. Query sales for a specific year");
                Console.WriteLine("2. Apply percentage of increment to sales");
                              
                Console.Write("Enter choice: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        QuerySales(connection);
                        break;

                    case "2":
                        ApplyIncrement(connection);
                        break;

                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }

                Console.WriteLine();
            }
        }

        static void QuerySales(SqlConnection connection)
        {
            
            Console.Write("Enter year to query (YYYY): ");
            var year = Console.ReadLine();

            
            var query = @"
        SELECT
            [SalesForecasting].[dbo].[Orders$].[State],
            SUM([SalesForecasting].[dbo].[Products$].[Sales]) AS TotalSales,
            SUM(CASE WHEN [SalesForecasting].[dbo].[OrdersReturns$].[OrderId] IS NOT NULL THEN [SalesForecasting].[dbo].[Products$].[Sales] ELSE 0 END) AS TotalReturns,
            SUM([SalesForecasting].[dbo].[Products$].[Sales]) - SUM(CASE WHEN [SalesForecasting].[dbo].[OrdersReturns$].[OrderId] IS NOT NULL THEN [SalesForecasting].[dbo].[Products$].[Sales] ELSE 0 END) AS NetSales
        FROM
            [SalesForecasting].[dbo].[Products$]
            INNER JOIN [SalesForecasting].[dbo].[Orders$] ON [SalesForecasting].[dbo].[Products$].[OrderId] = [SalesForecasting].[dbo].[Orders$].[OrderId]
            LEFT JOIN [SalesForecasting].[dbo].[OrdersReturns$] ON [SalesForecasting].[dbo].[Orders$].[OrderId] = [SalesForecasting].[dbo].[OrdersReturns$].[OrderId]
        WHERE
            YEAR(CONVERT(DATE, [SalesForecasting].[dbo].[Orders$].[OrderDate], 120)) = @Year
        GROUP BY
            [SalesForecasting].[dbo].[Orders$].[State] ";

            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Year", year);

            
            var dataTable = new DataTable();
            using (var adapter = new SqlDataAdapter(command))
            {
                adapter.Fill(dataTable);
            }

            
            if (dataTable.Rows.Count > 0)
            {
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        Console.Write("{0,-15}", dataTable.Rows[i][j]);
                    }
                    Console.WriteLine();
                }
            }
            else
            {
                Console.WriteLine("No data found.");
            }
        }





        static void ApplyIncrement(SqlConnection connection)
        {
            
            Console.Write("Enter year to query (YYYY): ");
            var year = Console.ReadLine();

            Console.Write("Enter percentage increment (e.g. 0.05): ");
            var incrementPercentage = decimal.Parse(Console.ReadLine());

         
            var query = @"
    SELECT
        [SalesForecasting].[dbo].[Orders$].[State],
        SUM([SalesForecasting].[dbo].[Products$].[Sales]) AS TotalSales,
        @IncrementPercentage AS IncrementPercentage,
        SUM([SalesForecasting].[dbo].[Products$].[Sales]) * @IncrementPercentage AS IncrementSales,
        SUM([SalesForecasting].[dbo].[Products$].[Sales]) + SUM([SalesForecasting].[dbo].[Products$].[Sales]) * @IncrementPercentage AS ProjectedSales
    FROM
        [SalesForecasting].[dbo].[Orders$]
        INNER JOIN [SalesForecasting].[dbo].[Products$] ON [SalesForecasting].[dbo].[Orders$].[OrderId] = [SalesForecasting].[dbo].[Products$].[OrderId]
    WHERE
        YEAR(CONVERT(DATE, [SalesForecasting].[dbo].[Orders$].[OrderDate], 120)) = @Year
    GROUP BY
        [SalesForecasting].[dbo].[Orders$].[State]
    ";

            var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Year", year);
            command.Parameters.AddWithValue("@IncrementPercentage", incrementPercentage);

            // Populate a DataTable with the query result
            var dataTable = new DataTable();
            using (var adapter = new SqlDataAdapter(command))
            {
                adapter.Fill(dataTable);
            }

            // Print the content of the DataTable
            if (dataTable.Rows.Count > 0)
            {
                for (int i = 0; i < dataTable.Rows.Count; i++)
                {
                    for (int j = 0; j < dataTable.Columns.Count; j++)
                    {
                        Console.Write("{0,-15}", dataTable.Rows[i][j]);
                    }
                    Console.WriteLine();
                }
            }

            if (dataTable.Rows.Count > 0)
            {
                var filePath = @"D:\CSV_File\sales_data_" + year + ".csv";
                using (var writer = new StreamWriter(filePath))
                {
                    // Write the column headers
                    var headerRow = string.Join(",", dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
                    writer.WriteLine(headerRow);

                    // Write the data rows
                    foreach (DataRow dataRow in dataTable.Rows)
                    {
                        var dataRowValues = string.Join(",", dataRow.ItemArray.Select(v => v.ToString()));
                        writer.WriteLine(dataRowValues);
                    }
                }

                Console.WriteLine($"Data exported to {filePath}");
            }
            else
            {
                Console.WriteLine("No data found.");
            }


        }
    }
}