using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Race_Day.Data;
using Race_Day.Models;
using Race_Day.Models.DTOs;

namespace Race_Day.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        public readonly ApplicationDbcontext dbcontext;
        public UserController(ApplicationDbcontext dbcontext)
        {
            this.dbcontext = dbcontext;
        }
        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var Users = dbcontext.users.ToList();
            return Ok(Users);
        }
        [HttpGet]
        [Route("{id:guid}")]

        public IActionResult GetUserByID(Guid id)
        {
            var User = dbcontext.users.Find(id);
            if (User == null) return NotFound();
            return Ok(User);
        }
        [HttpPost]
        public IActionResult AddNewUser(Add_New_UserDTO addDTO)
        {
            var user = new User
            {
                Username = addDTO.Username,
                FirstName = addDTO.FirstName,
                LastName = addDTO.LastName,
                DateOfBirth = addDTO.DateOfBirth,
                PhoneNumber = addDTO.PhoneNumber,

                // server-controlled:
                CreatedAt = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                IsActive = true,
            };

            dbcontext.users.Add(user);
            dbcontext.SaveChanges();
            return Ok("User was added successfully");
        }
        [HttpPut]
        [Route("{id:guid}")]
        public IActionResult EditUserPorfile(Guid id, Edit_Existing_UserDTO editDTO)
        {
            var user = dbcontext.users.Find(id);
            if (User == null) return NotFound($"No user was found with {id}");
            if (editDTO.Username != null) user.Username = editDTO.Username;
            if (editDTO.FirstName != null) user.FirstName = editDTO.FirstName;
            if (editDTO.LastName != null) user.LastName = editDTO.LastName;
            if (editDTO.DateOfBirth != null) user.DateOfBirth = editDTO.DateOfBirth.Value;
            if (editDTO.PhoneNumber != null) user.PhoneNumber = editDTO.PhoneNumber;


            dbcontext.SaveChanges();
            return Ok("User profile updated.");

        }
        [HttpDelete]
        [Route("{id:guid}")]
        public IActionResult RemoveUser(Guid id)
        {
        var User = dbcontext.users.Find(id);
            if (User == null) return NotFound($"No user was found with {id}");

            dbcontext.users.Remove(User);
            dbcontext.SaveChanges();
            return Ok("User was Deleted successfully");
        }
    }
}
