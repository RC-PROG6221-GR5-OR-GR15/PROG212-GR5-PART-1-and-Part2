using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Marathon.Data;

namespace Marathon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    { 
            public readonly ApplicationDbcontext dbcontext;
        public RolesController(ApplicationDbcontext dbcontext)
        {
         this.dbcontext = dbcontext;  
        }
        [HttpGet]
        public IActionResult GetAllRoles() {
            var roles = dbcontext.roles.ToList();
               
           return Ok(roles);
        }
       /*[HttpPost]
        *public IActionResult AddNewRole(NewRoleDTO nrDTO)
        *{
        * var newRole = roles
        * {
        * 
        * }
        *}
        [HttpPut]
        [HttpDelete]
        [HttpPost]
        [HttpDelete]*/

    }
}
