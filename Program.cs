using System.Globalization;
using System.Text.RegularExpressions;

namespace EmployeePayroll;

internal static class Program
{
    // ===== MERGE-CONFLICT TARGET LINE (change this line differently on main and on your feature branch) =====
    private const string AppTitle = "EMPLOYEE PAYROLL & HR MANAGEMENT SYSTEM v1.0 - DeOsioElla Edition";

    // Starts EMPTY: 0 pre-defined records
    private static readonly List<Employee> Employees = new();
    private static int _nextId = 1001;

    private const string RowFormat = "{0,-5} {1,-10} {2,-10} {3,-8} {4,-15} {5,10} {6,5} {7,-10} {8,-6} {9}";

    // ------------------------------------------------------------------
    // MAIN MENU LOOP
    // ------------------------------------------------------------------
    private static void Main()
    {
        while (true)
        {
            ShowMenu();
            switch (Console.ReadLine()?.Trim())
            {
                case "1": AddEmployee(); break;
                case "2": ViewAll(); break;
                case "3": SearchMenu(); break;
                case "4": UpdateEmployee(); break;
                case "5": DeleteEmployee(); break;
                case "6": PayrollSummary(); break;
                case "0":
                    Console.WriteLine("\nThank you for using the system. Goodbye!");
                    return;
                default:
                    Error("Invalid choice. Please enter a number from 0 to 6.");
                    break;
            }
            Pause();
        }
    }

    private static void ShowMenu()
    {
        Console.Clear();
        Console.WriteLine(new string('=', 60));
        Console.WriteLine($"  {AppTitle}");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine($"  Records in memory: {Employees.Count}");
        Console.WriteLine();
        Console.WriteLine("  1. Add Employee            (Create)");
        Console.WriteLine("  2. View All Employees      (Read)");
        Console.WriteLine("  3. Search / Filter         (Search)");
        Console.WriteLine("  4. Update Employee         (Update)");
        Console.WriteLine("  5. Delete Employee         (Delete)");
        Console.WriteLine("  6. Payroll Summary by Department");
        Console.WriteLine("  0. Exit");
        Console.WriteLine(new string('-', 60));
        Console.Write("  Choose an option: ");
    }

    // ------------------------------------------------------------------
    // CREATE
    // ------------------------------------------------------------------
    private static void AddEmployee()
    {
        Header("ADD NEW EMPLOYEE");
        Console.WriteLine($"(Next ID will be {_nextId})");

        var emp = new Employee
        {
            FirstName = ReadString("First name: "),
            LastName = ReadString("Last name: "),
            Department = ReadString("Department (e.g., IT, HR, Sales, Finance): "),
            JobTitle = ReadString("Job title: "),
            MonthlySalary = ReadDecimal("Monthly salary: ")!.Value,
            TaxRate = ReadRate("Tax rate (0 to 1, e.g., 0.12 for 12%): ")!.Value,
            HireDate = ReadDate("Hire date (yyyy-MM-dd): ")!.Value,
            IsActive = ReadBool("Active? (Y/N): ")!.Value,
            Email = ReadEmail("Email: ")
        };

        emp.Id = _nextId++;   // auto-increment starting at 1001
        Employees.Add(emp);

        Success($"Employee added successfully with ID {emp.Id}.");
        PrintTable(new[] { emp });
    }

    // ------------------------------------------------------------------
    // READ
    // ------------------------------------------------------------------
    private static void ViewAll()
    {
        Header("ALL EMPLOYEES");
        PrintTable(Employees.OrderBy(e => e.Id));
        Console.WriteLine($"\nTotal records: {Employees.Count}");
    }

    // ------------------------------------------------------------------
    // SEARCH / FILTER (LINQ)
    // ------------------------------------------------------------------
    private static void SearchMenu()
    {
        Header("SEARCH / FILTER");
        Console.WriteLine("1. Search by ID");
        Console.WriteLine("2. Search by name (first or last, partial match)");
        Console.WriteLine("3. Filter by department");
        Console.WriteLine("4. Filter by status (Active / Inactive)");
        Console.Write("Choose: ");

        switch (Console.ReadLine()?.Trim())
        {
            case "1":
                {
                    int id = ReadInt("Enter Employee ID: ");
                    var result = Employees.Where(e => e.Id == id);
                    PrintTable(result);
                    break;
                }
            case "2":
                {
                    string term = ReadString("Enter name to search: ");
                    var result = Employees
                        .Where(e => e.FirstName.Contains(term, StringComparison.OrdinalIgnoreCase)
                                 || e.LastName.Contains(term, StringComparison.OrdinalIgnoreCase)
                                 || e.FullName.Contains(term, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(e => e.LastName);
                    PrintTable(result);
                    break;
                }
            case "3":
                {
                    var depts = Employees.Select(e => e.Department)
                                         .Distinct(StringComparer.OrdinalIgnoreCase)
                                         .OrderBy(d => d)
                                         .ToList();
                    if (depts.Any())
                        Console.WriteLine("Available departments: " + string.Join(", ", depts));

                    string dept = ReadString("Enter department: ");
                    var result = Employees
                        .Where(e => e.Department.Equals(dept, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(e => e.LastName);
                    PrintTable(result);
                    break;
                }
            case "4":
                {
                    bool active = ReadBool("Show active employees? (Y = Active, N = On Leave/Inactive): ")!.Value;
                    var result = Employees.Where(e => e.IsActive == active).OrderBy(e => e.Id);
                    PrintTable(result);
                    break;
                }
            default:
                Error("Invalid choice.");
                break;
        }
    }

    // ------------------------------------------------------------------
    // UPDATE
    // ------------------------------------------------------------------
    private static void UpdateEmployee()
    {
        Header("UPDATE EMPLOYEE");
        if (!Employees.Any()) { Console.WriteLine("No records to update."); return; }

        int id = ReadInt("Enter Employee ID to update: ");
        var emp = Employees.FirstOrDefault(e => e.Id == id);
        if (emp is null) { Error($"No employee found with ID {id}."); return; }

        PrintTable(new[] { emp });
        Console.WriteLine("\nPress ENTER on any field to keep its current value.\n");

        emp.FirstName = Keep(ReadString($"First name [{emp.FirstName}]: ", optional: true), emp.FirstName);
        emp.LastName = Keep(ReadString($"Last name [{emp.LastName}]: ", optional: true), emp.LastName);
        emp.Department = Keep(ReadString($"Department [{emp.Department}]: ", optional: true), emp.Department);
        emp.JobTitle = Keep(ReadString($"Job title [{emp.JobTitle}]: ", optional: true), emp.JobTitle);
        emp.MonthlySalary = ReadDecimal($"Monthly salary [{emp.MonthlySalary:N2}]: ", optional: true) ?? emp.MonthlySalary;
        emp.TaxRate = ReadRate($"Tax rate [{emp.TaxRate}]: ", optional: true) ?? emp.TaxRate;
        emp.HireDate = ReadDate($"Hire date [{emp.HireDate:yyyy-MM-dd}]: ", optional: true) ?? emp.HireDate;
        emp.IsActive = ReadBool($"Active? (Y/N) [{(emp.IsActive ? "Y" : "N")}]: ", optional: true) ?? emp.IsActive;
        emp.Email = Keep(ReadEmail($"Email [{emp.Email}]: ", optional: true, excludeId: emp.Id), emp.Email);

        Success("Employee updated successfully.");
        PrintTable(new[] { emp });
    }

    // ------------------------------------------------------------------
    // DELETE
    // ------------------------------------------------------------------
    private static void DeleteEmployee()
    {
        Header("DELETE EMPLOYEE");
        if (!Employees.Any()) { Console.WriteLine("No records to delete."); return; }

        int id = ReadInt("Enter Employee ID to delete: ");
        var emp = Employees.FirstOrDefault(e => e.Id == id);
        if (emp is null) { Error($"No employee found with ID {id}."); return; }

        PrintTable(new[] { emp });
        bool confirm = ReadBool("Are you sure you want to delete this record? (Y/N): ")!.Value;
        if (!confirm) { Console.WriteLine("Deletion cancelled."); return; }

        int removed = Employees.RemoveAll(e => e.Id == id);
        if (removed > 0) Success($"Employee {id} deleted. Records remaining: {Employees.Count}");
    }

    // ------------------------------------------------------------------
    // PAYROLL SUMMARY (LINQ GroupBy / Sum) - active employees only
    // ------------------------------------------------------------------
    private static void PayrollSummary()
    {
        Header("PAYROLL SUMMARY BY DEPARTMENT (ACTIVE EMPLOYEES)");
        var active = Employees.Where(e => e.IsActive).ToList();
        if (!active.Any()) { Console.WriteLine("No active employees."); return; }

        var groups = active
            .GroupBy(e => e.Department, StringComparer.OrdinalIgnoreCase)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                Dept = g.Key,
                Count = g.Count(),
                Gross = g.Sum(e => e.MonthlySalary),
                Net = g.Sum(e => e.NetPay)
            });

        Console.WriteLine("{0,-15} {1,6} {2,16} {3,16}", "Department", "Staff", "Gross Monthly", "Net Monthly");
        Console.WriteLine(new string('-', 56));
        foreach (var g in groups)
            Console.WriteLine("{0,-15} {1,6} {2,16:N2} {3,16:N2}", Trunc(g.Dept, 14), g.Count, g.Gross, g.Net);
        Console.WriteLine(new string('-', 56));
        Console.WriteLine("{0,-15} {1,6} {2,16:N2} {3,16:N2}", "TOTAL", active.Count,
            active.Sum(e => e.MonthlySalary), active.Sum(e => e.NetPay));
    }

    // ------------------------------------------------------------------
    // TABLE OUTPUT
    // ------------------------------------------------------------------
    private static void PrintTable(IEnumerable<Employee> source)
    {
        var rows = source.ToList();
        if (!rows.Any()) { Console.WriteLine("No records found."); return; }

        string header = string.Format(RowFormat, "ID", "First", "Last", "Dept", "Job Title",
                                      "Salary", "Tax", "Hired", "Status", "Email");
        Console.WriteLine();
        Console.WriteLine(header);
        Console.WriteLine(new string('-', header.Length + 12));

        foreach (var e in rows)
        {
            Console.WriteLine(string.Format(RowFormat,
                e.Id,
                Trunc(e.FirstName, 10),
                Trunc(e.LastName, 10),
                Trunc(e.Department, 8),
                Trunc(e.JobTitle, 15),
                e.MonthlySalary.ToString("N2", CultureInfo.InvariantCulture),
                (e.TaxRate * 100).ToString("0.##", CultureInfo.InvariantCulture) + "%",
                e.HireDate.ToString("yyyy-MM-dd"),
                e.IsActive ? "Active" : "Leave",
                e.Email));
        }
    }

    // ------------------------------------------------------------------
    // INPUT HELPERS WITH VALIDATION
    // ------------------------------------------------------------------
    private static string ReadString(string prompt, bool optional = false)
    {
        while (true)
        {
            Console.Write(prompt);
            string s = (Console.ReadLine() ?? "").Trim();
            if (s.Length > 0) return s;
            if (optional) return "";
            Error("This field cannot be empty.");
        }
    }

    private static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine()?.Trim(), out int v) && v > 0) return v;
            Error("Please enter a valid positive whole number.");
        }
    }

    private static decimal? ReadDecimal(string prompt, bool optional = false)
    {
        while (true)
        {
            Console.Write(prompt);
            string s = (Console.ReadLine() ?? "").Trim();
            if (optional && s.Length == 0) return null;
            if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal v) && v > 0)
                return v;
            Error("Enter a positive amount (e.g., 45000.50).");
        }
    }

    private static double? ReadRate(string prompt, bool optional = false)
    {
        while (true)
        {
            Console.Write(prompt);
            string s = (Console.ReadLine() ?? "").Trim();
            if (optional && s.Length == 0) return null;
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v) && v >= 0 && v <= 1)
                return v;
            Error("Enter a rate between 0 and 1 (e.g., 0.12 for 12%).");
        }
    }

    private static DateTime? ReadDate(string prompt, bool optional = false)
    {
        while (true)
        {
            Console.Write(prompt);
            string s = (Console.ReadLine() ?? "").Trim();
            if (optional && s.Length == 0) return null;
            if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d)
                && d.Date <= DateTime.Today)
                return d;
            Error("Enter a valid date (yyyy-MM-dd) that is not in the future.");
        }
    }

    private static bool? ReadBool(string prompt, bool optional = false)
    {
        while (true)
        {
            Console.Write(prompt);
            string s = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
            if (optional && s.Length == 0) return null;
            if (s is "y" or "yes") return true;
            if (s is "n" or "no") return false;
            Error("Please enter Y or N.");
        }
    }

    private static string ReadEmail(string prompt, bool optional = false, int excludeId = 0)
    {
        while (true)
        {
            Console.Write(prompt);
            string s = (Console.ReadLine() ?? "").Trim();
            if (optional && s.Length == 0) return "";

            if (!Regex.IsMatch(s, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                Error("Enter a valid email address (e.g., name@company.com).");
                continue;
            }
            if (Employees.Any(e => e.Id != excludeId && e.Email.Equals(s, StringComparison.OrdinalIgnoreCase)))
            {
                Error("That email is already used by another employee.");
                continue;
            }
            return s;
        }
    }

    // ------------------------------------------------------------------
    // SMALL UTILITIES
    // ------------------------------------------------------------------
    private static string Keep(string input, string current) =>
        string.IsNullOrWhiteSpace(input) ? current : input;

    private static string Trunc(string s, int max) =>
        s.Length <= max ? s : s[..(max - 1)] + "~";

    private static void Header(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"--- {title} ---");
    }

    private static void Success(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n[OK] {msg}");
        Console.ResetColor();
    }

    private static void Error(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[!] {msg}");
        Console.ResetColor();
    }

    private static void Pause()
    {
        Console.WriteLine("\nPress ENTER to return to the menu...");
        Console.ReadLine();
    }
}