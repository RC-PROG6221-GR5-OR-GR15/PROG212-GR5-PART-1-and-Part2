using Microsoft.AspNetCore.Mvc;
using Marathon.Data;
using Marathon.Models;
using Marathon.Models.DTOs;

namespace Marathon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        public readonly ApplicationDbcontext dbcontext;
        public CategoryController(ApplicationDbcontext dbcontext)
        {
            this.dbcontext = dbcontext;
        }
        [HttpGet]
        public IActionResult GetAllCategories()
        {
            var Categories = dbcontext.categories.ToList();
            return Ok(Categories);
        }
        [HttpGet]
        [Route("{id:guid}")]
        public IActionResult GetCategoriesById(Guid id)
        {
            var Category = dbcontext.categories.Find(id);
            if (Category == null) return NotFound($"Category with id {id} was not found ");

            return Ok(Category);
        }
        [HttpPost]
        public IActionResult CreateCategory(NewCategoryDTO categoryDTO)
        {
            if(!dbcontext.events.Any(e => e.EventId == categoryDTO.EventId))
                return BadRequest($"Event {categoryDTO.EventId} does not exist.");



            if (categoryDTO.MinAge > categoryDTO.MaxAge) 
                return BadRequest("MinAge cannot be greater than MaxAge.");
            //Minimizing Duplicate Events
            var duplicate = dbcontext.categories.Any(c =>
            c.EventId == categoryDTO.EventId &&
            c.CategoryName == categoryDTO.CategoryName);
            if(duplicate) return Conflict("A category with that name already exists for this event.");
            var category = new Category
            {
                EventId = categoryDTO.EventId,
                CategoryName = categoryDTO.CategoryName,
                MinAge = categoryDTO.MinAge,
                GenderRestrictions = categoryDTO.GenderRestrictions,
                MaxAge = categoryDTO.MaxAge,
                MaxParticipants = categoryDTO.MaxParticipants,
                CreatedBy = User.Identity?.Name ?? "system",
                CreatedAt = DateTime.UtcNow,
            };
            dbcontext.categories.Add(category);
            dbcontext.SaveChanges();
            return Ok("Role was added successfully");

            return CreatedAtAction(
                nameof(GetCategoriesById),
                new { id = category.CategoryId },
                category
                );
        }
         [HttpPut]
         [Route("{id:guid}")] 
         public IActionResult EditCatery(Edit_CategoryDTO edit_categoryDTO, Guid id) 
        {
            var category = dbcontext.categories.Find(id); 
            
            if(category == null) return NotFound($"Caterogry using id: {id} was not found");
            if (edit_categoryDTO.CategoryName != null) category.CategoryName = edit_categoryDTO.CategoryName;
            if (edit_categoryDTO.MinAge != null) category.MinAge = edit_categoryDTO.MinAge.Value;
            if (edit_categoryDTO.MaxAge != null) category.MaxAge = edit_categoryDTO.MaxAge.Value;
            if (edit_categoryDTO.GenderRestrictions != null) category.GenderRestrictions = edit_categoryDTO.GenderRestrictions;
            if (edit_categoryDTO.MaxParticipants != null) category.MaxParticipants = edit_categoryDTO.MaxParticipants.Value;

            dbcontext.SaveChanges();
            return Ok(new { message = "Category updated" });

        }
        [HttpDelete]
        [Route("{id:guid}")]
        public IActionResult DeleteCategory(Guid id)
        {
            var category = dbcontext.categories.Find(id);
            if (category == null) return NotFound($"Category with id {id} was not found.");
            dbcontext.categories.Remove(category);
            dbcontext.SaveChanges();
            return Ok(new { message = "Category deleted" });
        }


    }

}
