using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// SQL Server connection string
var connectionString = "Server=LAPTOP-9R2PAH6V;Database=EmployeeDb;User Id=sa;Password=321;TrustServerCertificate=true;";

// Configure services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// GET all Employees
app.MapGet("/employees", async () =>
{
    var employees = new List<object>();

    using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();

    var command = new SqlCommand("SELECT EmployeeId, FirstName, LastName, Position, Salary FROM Employees", conn);

    using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        var employee = new
        {
            EmployeeId = reader.GetInt32(0),
            FirstName = reader.GetString(1),
            LastName = reader.GetString(2),
            Position = reader.GetString(3),
            Salary = reader.GetDecimal(4)
        };
        employees.Add(employee);
    }

    return Results.Json(employees);
});

// GET Employee by ID
app.MapGet("/employee/{id}", async (int id) =>
{
    using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();

    var command = new SqlCommand("SELECT EmployeeId, FirstName, LastName, Position, Salary FROM Employees WHERE EmployeeId = @id", conn);
    command.Parameters.AddWithValue("@id", id);

    using var reader = await command.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
        var employee = new
        {
            EmployeeId = reader.GetInt32(0),
            FirstName = reader.GetString(1),
            LastName = reader.GetString(2),
            Position = reader.GetString(3),
            Salary = reader.GetDecimal(4)
        };
        return Results.Json(employee);
    }

    return Results.NotFound("Employee not found");
});

// CREATE a new Employee
app.MapPost("/employee", async (Employee employee) =>
{
    using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();

    var command = new SqlCommand("INSERT INTO Employees (FirstName, LastName, Position, Salary) VALUES (@FirstName, @LastName, @Position, @Salary);", conn);
    command.Parameters.AddWithValue("@FirstName", employee.FirstName);
    command.Parameters.AddWithValue("@LastName", employee.LastName);
    command.Parameters.AddWithValue("@Position", employee.Position);
    command.Parameters.AddWithValue("@Salary", employee.Salary);

    await command.ExecuteNonQueryAsync();

    return Results.Created($"/employee/{employee.EmployeeId}", employee);
});

// UPDATE an existing Employee
app.MapPut("/employee/{id}", async (int id, Employee employee) =>
{
    using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();

    var command = new SqlCommand("UPDATE Employees SET FirstName = @FirstName, LastName = @LastName, Position = @Position, Salary = @Salary WHERE EmployeeId = @id", conn);
    command.Parameters.AddWithValue("@id", id);
    command.Parameters.AddWithValue("@FirstName", employee.FirstName);
    command.Parameters.AddWithValue("@LastName", employee.LastName);
    command.Parameters.AddWithValue("@Position", employee.Position);
    command.Parameters.AddWithValue("@Salary", employee.Salary);

    var rowsAffected = await command.ExecuteNonQueryAsync();

    if (rowsAffected == 0)
    {
        return Results.NotFound("Employee not found");
    }

    return Results.NoContent();
});

// DELETE an Employee
app.MapDelete("/employee/{id}", async (int id) =>
{
    using var conn = new SqlConnection(connectionString);
    await conn.OpenAsync();

    var command = new SqlCommand("DELETE FROM Employees WHERE EmployeeId = @id", conn);
    command.Parameters.AddWithValue("@id", id);

    var rowsAffected = await command.ExecuteNonQueryAsync();

    if (rowsAffected == 0)
    {
        return Results.NotFound("Employee not found");
    }

    return Results.NoContent();
});

app.Run();

public record Employee(int EmployeeId, string FirstName, string LastName, string Position, decimal Salary);
