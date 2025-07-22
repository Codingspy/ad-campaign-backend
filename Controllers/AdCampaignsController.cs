using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdCampaignMVP.Data;
using AdCampaignMVP.Models;

namespace AdCampaignMVP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdCampaignsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdCampaignsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.AdCampaigns.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create(AdCampaign model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }
            model.CreatedByUserId = user.Id;

            _context.AdCampaigns.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, AdCampaign updated)
        {
            var existing = await _context.AdCampaigns.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Title = updated.Title;
            existing.Description = updated.Description;
            existing.Budget = updated.Budget;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.AdCampaigns.FindAsync(id);
            if (existing == null) return NotFound();

            _context.AdCampaigns.Remove(existing);
            await _context.SaveChangesAsync();
            return Ok("Deleted");
        }
    }
}
