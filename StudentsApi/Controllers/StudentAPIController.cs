using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using StudentBusiness;
using StudentDataAccess;

[Authorize]
[ApiController]
[Route("api/Students")]

public class StudentsController : ControllerBase
{

    private readonly ILogger<StudentsController> _logger;
    public StudentsController(ILogger<StudentsController> logger)
    {
        _logger = logger;
    }
    [Authorize(Roles = "admin")]
    [HttpGet("All", Name = "GetAllStudents")]
    public async Task<ActionResult<IEnumerable<StudentDTO>>> GetAllStudents()
    {
        List<StudentDTO> StudentsList = await Task.Run(() => Student.GetAllStudents());
        if (StudentsList.Count == 0)
        {
            return NotFound("No Students Found!");
        }
        return Ok(StudentsList);
    }

    [AllowAnonymous]
    [HttpGet("Passed", Name = "GetPassedStudents")]
    public async Task<ActionResult<IEnumerable<StudentDTO>>> GetPassedStudents()
    {

        List<StudentDTO> PassedStudentsList = await Task.Run(() => Student.GetPassedStudents());
        if (PassedStudentsList.Count == 0)
        {
            return NotFound("No Students Found!");
        }

        return Ok(PassedStudentsList);
    }

    [AllowAnonymous]
    [HttpGet("AverageGrade", Name = "GetAverageGrade")]
    public async Task<ActionResult<double>> GetAverageGrade()
    {
        double averageGrade = await Task.Run(() => Student.GetAverageGrade());
        return Ok(averageGrade);
    }

    [HttpGet("{id}", Name = "GetStudentById")]
    public async Task<ActionResult<StudentDTO>> GetStudentById(int id, [FromServices] IAuthorizationService authorizationService)
    {

        if (id < 1)
        {
            return BadRequest($"Not accepted ID {id}");
        }

        Student student = Student.Find(id)!;

        if (student == null)
        {
            return NotFound($"Student with ID {id} not found.");
        }

        var authResult = await authorizationService.AuthorizeAsync(User, id, "StudentOwnerOrAdmin");

        if (!authResult.Succeeded)
            return Forbid();

        StudentDTO SDTO = student.SDTO;

        return Ok(SDTO);

    }

    [Authorize(Roles = "admin")]
    [HttpPost(Name = "AddStudent")]
    public async Task<ActionResult<StudentDTO>> AddStudent(StudentDTO newStudentDTO)
    {
        if (newStudentDTO == null || string.IsNullOrEmpty(newStudentDTO.Name) || newStudentDTO.Age < 0 || newStudentDTO.Grade < 0 || string.IsNullOrEmpty(newStudentDTO.Email) || string.IsNullOrEmpty(newStudentDTO.Password) || string.IsNullOrEmpty(newStudentDTO.Role))
        {
            return BadRequest("Invalid student data.");
        }
        Student student = new Student(new StudentDTO(newStudentDTO.Id, newStudentDTO.Name, newStudentDTO.Age, newStudentDTO.Grade, newStudentDTO.Email, newStudentDTO.Password, newStudentDTO.Role), cMode: Student.enMode.AddNew);
        bool isSaved = await Task.Run(() => student.Save());
        if (student.Save())
        {
            newStudentDTO.Id = student.ID;
            return CreatedAtRoute("GetStudentById", new { id = newStudentDTO.Id }, newStudentDTO);
        }
        else
        {
            return StatusCode(500, new { message = "Error Adding new Student" });
        }

    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id}", Name = "UpdateStudent")]
    public async Task<ActionResult<StudentDTO>> UpdateStudent(int id, StudentDTO updatedStudent)
    {
        if (id < 1 || updatedStudent == null || string.IsNullOrEmpty(updatedStudent.Name) || updatedStudent.Age < 0 || updatedStudent.Grade < 0)
        {
            return BadRequest("Invalid student data.");
        }
        Student student = await Task.Run(() => Student.Find(id));
        if (student == null)
        {
            return NotFound($"Student with ID {id} not found.");
        }
        student.Name = updatedStudent.Name;
        student.Age = updatedStudent.Age;
        student.Grade = updatedStudent.Grade;
        bool isSaved = await Task.Run(() => student.Save());
        if (isSaved)
        {
            return Ok(student.SDTO);
        }
        else
            return StatusCode(500, new { message = "Error Updating the Student" });


    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id}", Name = "DeleteStudent")]
    public async Task<ActionResult> DeleteStudent(int id)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        var student = await Task.Run(() => Student.Find(id));
        if (id < 1)
        {
            _logger.LogWarning(
            "Admin action blocked (invalid id). AdminId={AdminId}, Action=DeleteStudent, TargetId={TargetId}, IP={IP}",
            adminId,
            id,
            ip
        );
            return BadRequest($"Not accepted ID {id}");
        }

        if (student == null)
        {
            _logger.LogWarning(
            "Admin action failed (target not found). AdminId={AdminId}, Action=DeleteStudent, TargetId={TargetId}, IP={IP}",
            adminId,
            id,
            ip
        );
            return NotFound($"Student with ID {id} not found.");
        }
        _logger.LogInformation(
       "Admin action started. AdminId={AdminId}, Action=DeleteStudent, TargetId={TargetId}, TargetEmail={TargetEmail}, IP={IP}",
       adminId,
       student.ID,
       student.Email,
       ip
        );
        bool isDeleted = await Task.Run(() => Student.DeleteStudent(id));
        if (isDeleted)
        {
            _logger.LogInformation(
         "Admin action succeeded. AdminId={AdminId}, Action=DeleteStudent, TargetId={TargetId}, IP={IP}",
         adminId,
         id,
         ip
     );
            return NoContent();
        }
        else
            return NotFound($"Student with ID {id} not found. no rows deleted!");
    }

    [HttpPost("Upload")]
    public async Task<IActionResult> UploadImage(IFormFile imageFile)
    {
        if (imageFile is null || imageFile.Length is 0)
            return BadRequest("No file uploaded");
        var uploadDirectory = @"/home/mamoun/VsProjects";
        var filename = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
        var filePath = Path.Combine(uploadDirectory, filename);

        if (!Directory.Exists(uploadDirectory))
        {
            Directory.CreateDirectory(uploadDirectory);
        }
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(stream);
        }
        return Ok(new { filePath });
    }

    [HttpGet("getImage/{filename}")]
    public IActionResult GetImage(string FileName)
    {
        var uploadDirectory = @"/home/mamoun/VsProjects";
        var filePath = Path.Combine(uploadDirectory, FileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound(new
            {
                Error = "Image not found",
                ReceivedFileName = FileName,
                FullPathChecked = filePath,
                DirectoryExists = Directory.Exists(uploadDirectory),
                ExistingFilesOnDisk = Directory.Exists(uploadDirectory) ? Directory.GetFiles(uploadDirectory) : null
            });
        var image = System.IO.File.OpenRead(filePath);
        var mimeType = GetMimeType(filePath);
        return File(image, mimeType);
    }

    private string GetMimeType(string filePath)
    {
        var extenstion = Path.GetExtension(filePath).ToLowerInvariant();
        return extenstion switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream",
        };
    }
}