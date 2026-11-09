namespace EmployeePayroll;

/// <summary>
/// Employee record with the 10 required columns.
/// </summary>
public class Employee
{
    public int Id { get; set; }                    // 1  Unique ID (auto-increment from 1001)
    public string FirstName { get; set; } = "";    // 2
    public string LastName { get; set; } = "";     // 3
    public string Department { get; set; } = "";   // 4
    public string JobTitle { get; set; } = "";     // 5
    public decimal MonthlySalary { get; set; }     // 6
    public double TaxRate { get; set; }            // 7  e.g., 0.12 = 12%
    public DateTime HireDate { get; set; }         // 8
    public bool IsActive { get; set; }             // 9  true = Active, false = On Leave/Inactive
    public string Email { get; set; } = "";        // 10

    // Computed helpers (NOT stored columns)
    public string FullName => $"{FirstName} {LastName}";
    public decimal NetPay => Math.Round(MonthlySalary - MonthlySalary * (decimal)TaxRate, 2);
}