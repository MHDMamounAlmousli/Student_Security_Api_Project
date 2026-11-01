using System;
using System.Data;
using Microsoft.Data.SqlClient;
namespace StudentDataAccess;

public class StudentDTO
{
    public StudentDTO(int id, string name, int age, int grade, string email, string password, string role)
    {
        this.Id = id;
        this.Name = name;
        this.Age = age;
        this.Grade = grade;
        this.Email = email;
        this.Password = password;
        this.Role = role;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public int Grade { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
}
public class StudentData
{
    private static readonly string _connectionString = DatabaseConnection.ConnectionString;

    public StudentData()
    {
    }

    private static StudentDTO MapStudentDTO(SqlDataReader reader)
    {
        return new StudentDTO(
            reader.GetInt32(reader.GetOrdinal("Id")),
            reader.GetString(reader.GetOrdinal("Name")),
            reader.GetInt32(reader.GetOrdinal("Age")),
            reader.GetInt32(reader.GetOrdinal("Grade")),
            reader.IsDBNull(reader.GetOrdinal("Email")) ? "" : reader.GetString(reader.GetOrdinal("Email")),
            reader.IsDBNull(reader.GetOrdinal("Password")) ? "" : reader.GetString(reader.GetOrdinal("Password")),
            reader.IsDBNull(reader.GetOrdinal("Role")) ? "Student" : reader.GetString(reader.GetOrdinal("Role"))
        );
    }

    public static List<StudentDTO> GetAllStudents()
    {
        var StudentsList = new List<StudentDTO>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand("SP_GetAllStudents", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            conn.Open();

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    StudentsList.Add(MapStudentDTO(reader));
                }
            }
        }

        return StudentsList;
    }

    public static List<StudentDTO> GetPassedStudents()
    {
        var StudentsList = new List<StudentDTO>();

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand("SP_GetPassedStudents", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            conn.Open();

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    StudentsList.Add(MapStudentDTO(reader));
                }
            }
        }

        return StudentsList;
    }

    public static double GetAverageGrade()
    {
        double averageGrade = 0;

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand("SP_GetAverageGrade", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            conn.Open();

            object result = cmd.ExecuteScalar();
            if (result != null && result != DBNull.Value)
            {
                averageGrade = Convert.ToDouble(result);
            }
        }

        return averageGrade;
    }

    public static StudentDTO? GetStudentById(int studentId)
    {
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand("SP_GetStudentById", connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@StudentId", studentId);

            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return MapStudentDTO(reader);
                }
                return null;
            }
        }
    }

    public static StudentDTO? GetStudentByEmail(string email)
    {
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand("SP_GetStudentByEmail", connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Email", email);

            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return MapStudentDTO(reader);
                }
                return null;
            }
        }
    }
    public static int AddStudent(StudentDTO StudentDTO)
    {
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand("SP_AddStudent", connection))
        {
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Name", StudentDTO.Name);
            command.Parameters.AddWithValue("@Age", StudentDTO.Age);
            command.Parameters.AddWithValue("@Grade", StudentDTO.Grade);
            command.Parameters.AddWithValue("@Email", StudentDTO.Email);
            command.Parameters.AddWithValue("@Password", StudentDTO.Password);
            command.Parameters.AddWithValue("@Role", StudentDTO.Role);

            var outputIdParam = new SqlParameter("@NewStudentId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(outputIdParam);

            connection.Open();
            command.ExecuteNonQuery();

            return (int)outputIdParam.Value;
        }
    }

    public static bool UpdateStudent(StudentDTO StudentDTO)
    {
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand("SP_UpdateStudent", connection))
        {
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@StudentId", StudentDTO.Id);
            command.Parameters.AddWithValue("@Name", StudentDTO.Name);
            command.Parameters.AddWithValue("@Age", StudentDTO.Age);
            command.Parameters.AddWithValue("@Grade", StudentDTO.Grade);
            command.Parameters.AddWithValue("@Email", StudentDTO.Email);
            command.Parameters.AddWithValue("@Password", StudentDTO.Password);
            command.Parameters.AddWithValue("@Role", StudentDTO.Role);

            connection.Open();
            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }
    }

    public static bool DeleteStudent(int studentId)
    {

        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand("SP_DeleteStudent", connection))
        {
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@StudentId", studentId);

            connection.Open();

            int rowsAffected = (int)command.ExecuteScalar();
            return (rowsAffected == 1);
        }
    }
}