using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Marathon.Data;
using Marathon.Models;
using Marathon.Models.DTOs;

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
       [HttpPost]
        public IActionResult AddNewRole(NewRoleDTO nrDTO)
        {
         var newRole = new Role
         {
             RoleName = nrDTO.RoleName,
             Description = nrDTO.Description,
             CreatedAt = nrDTO.CreatedAt
         };
         dbcontext.roles.Add(newRole);
         dbcontext.SaveChanges();
         return CreatedAtAction(nameof(GetAllRoles), new { id = newRole.RoleId }, newRole); 
        }
        
     [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] NewRoleDTO nrDTO)
        {
            var existing = await dbcontext.roles.FindAsync(id);
            if (existing == null)
              return NotFound($"Role with id {id} not found");

           

           

            existing.RoleName = nrDTO.RoleName;
            await dbcontext.SaveChangesAsync();

            return Ok(existing);
        }

        // DELETE: api/roles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await dbcontext.roles.FindAsync(id);
            if (role == null)
                return NotFound($"Role with id {id} not found");

            dbcontext.
               roles.Remove(role);
            await dbcontext.SaveChangesAsync();

            return NoContent();
        }

    }
}
