using StudentDataAccess;
namespace StudentBusiness;

public class Student
{
    public enum enMode { AddNew = 0, Update = 1 };
    public enMode Mode = enMode.AddNew;

    public StudentDTO SDTO
    {
        get { return (new StudentDTO(this.ID, this.Name, this.Age, this.Grade, this.Email, this.Password, this.Role)); }
    }

    public int ID { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public int Grade { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }

    public Student(StudentDTO SDTO, enMode cMode = enMode.AddNew)

    {
        this.ID = SDTO.Id;
        this.Name = SDTO.Name;
        this.Age = SDTO.Age;
        this.Grade = SDTO.Grade;
        this.Email = SDTO.Email;
        this.Password = SDTO.Password;
        this.Role = SDTO.Role;

        Mode = cMode;
    }

    private bool _AddNewStudent()
    {

        this.Password = BCrypt.Net.BCrypt.HashPassword(this.Password);

        this.ID = StudentData.AddStudent(SDTO);

        return (this.ID != -1);
    }

    private bool _UpdateStudent()
    {
        this.Password = BCrypt.Net.BCrypt.HashPassword(this.Password);

        return StudentData.UpdateStudent(SDTO);
    }


    public static List<StudentDTO> GetAllStudents()
    {
        return StudentData.GetAllStudents();
    }


    public static List<StudentDTO> GetPassedStudents()
    {
        return StudentData.GetPassedStudents();
    }

    public static double GetAverageGrade()
    {
        return StudentData.GetAverageGrade();
    }

    public static Student? Find(int ID)
    {

        StudentDTO SDTO = StudentData.GetStudentById(ID)!;

        if (SDTO != null)
        {

            return new Student(SDTO, enMode.Update);
        }
        else
            return null;
    }
    public static Student? FindByEmail(string email)
    {
        StudentDTO SDTO = StudentData.GetStudentByEmail(email)!;

        if (SDTO != null)
        {
            return new Student(SDTO, enMode.Update);
        }
        else
            return null;
    }

    public bool Save()
    {
        switch (Mode)
        {
            case enMode.AddNew:
                if (_AddNewStudent())
                {

                    Mode = enMode.Update;
                    return true;
                }
                else
                {
                    return false;
                }

            case enMode.Update:

                return _UpdateStudent();

        }

        return false;
    }

    public static bool DeleteStudent(int ID)
    {
        return StudentData.DeleteStudent(ID);
    }

}
