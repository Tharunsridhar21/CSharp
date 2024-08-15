using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EmployeeDataConsole
{
    class Program
    {
        public class Employee
        {
            public string EmployeeName { get; set; } = string.Empty;
            public DateTime StartTimeUtc { get; set; }
            public DateTime EndTimeUtc { get; set; }
        }

        public class EmployeeWorkHours
        {
            public string Name { get; set; } = string.Empty;
            public double TotalTimeWorked { get; set; }
        }

        static async Task Main(string[] args)
        {
            string apiUrl = "https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ=="; // Replace with your actual API URL
            List<Employee>? employees = await FetchEmployeeData(apiUrl);

            if (employees != null)
            {
                // Calculate total hours worked for each employee
                var employeeHours = employees
                    .GroupBy(e => e.EmployeeName)
                    .Select(g => new EmployeeWorkHours
                    {
                        Name = g.Key,
                        TotalTimeWorked = g.Sum(e => (e.EndTimeUtc - e.StartTimeUtc).TotalHours)
                    }).ToList();

                // Divide the total time worked by 10,000,000, round off to nearest whole number
                foreach (var employee in employeeHours)
                {
                    // Divide and round
                    double scaledHours = employee.TotalTimeWorked / 10000000;
                    employee.TotalTimeWorked = Math.Round(scaledHours);
                }

                // Sort by total hours worked in descending order
                employeeHours = employeeHours
                    .OrderByDescending(e => e.TotalTimeWorked)
                    .ToList();

                // Generate HTML table
                string htmlContent = GenerateHtmlTable(employeeHours);
                File.WriteAllText("employees.html", htmlContent);

                Console.WriteLine("HTML file 'employees.html' has been generated.");
            }
            else
            {
                Console.WriteLine("Failed to fetch employee data.");
            }
        }

        static async Task<List<Employee>?> FetchEmployeeData(string apiUrl)
        {
            using HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Employee>>(responseBody);
            }

            return null;
        }

        static string GenerateHtmlTable(List<EmployeeWorkHours> employeeHours)
        {
            string html = "<html><head><style>table { width: 100%; border-collapse: collapse; } " +
                          "th, td { border: 1px solid black; padding: 8px; text-align: left; } " +
                          ".highlight { background-color: #ffcccc; }</style></head><body>";
            html += "<h2>Employee Work Hours</h2>";
            html += "<table><tr><th>Name</th><th>Total Time Worked</th></tr>";

            foreach (var employee in employeeHours)
            {
                string rowClass = employee.TotalTimeWorked < 100 ? "highlight" : "";
                html += $"<tr class='{rowClass}'><td>{employee.Name}</td><td>{employee.TotalTimeWorked} hours</td></tr>";
            }

            html += "</table></body></html>";
            return html;
        }
    }
}
