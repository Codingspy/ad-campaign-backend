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
        [Authorize]
        public async Task<IActionResult> Update(int id, AdCampaign updated)
        {

             var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var existing = await _context.AdCampaigns.FindAsync(id);
            if (existing == null) return NotFound();

            if (!isAdmin && existing.CreatedByUserId != user.Id)
            {
                return Forbid("You do not have permission to update this campaign.");
            }

            existing.Title = updated.Title;
            existing.Description = updated.Description;
            existing.Budget = updated.Budget;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var existing = await _context.AdCampaigns.FindAsync(id);
            if (existing == null) return NotFound();

            if (!isAdmin && existing.CreatedByUserId != user.Id)
            {
                return Forbid("You do not have permission to update this campaign.");
            }

            _context.AdCampaigns.Remove(existing);
            await _context.SaveChangesAsync();
            return Ok("Deleted");
        }

        // Increase Impression
        [HttpPost("{id}/impression")]
        [AllowAnonymous]
        public async Task<IActionResult> AddImpression(int id)
        {
            var campaign = await _context.AdCampaigns.FindAsync(id);
            if (campaign == null) return NotFound();

            campaign.Impressions++;
            await _context.SaveChangesAsync();
            return Ok(campaign);
        }

        // Increase Click
        [HttpPost("{id}/click")]
        [AllowAnonymous]
        public async Task<IActionResult> AddClick(int id)
        {
            var campaign = await _context.AdCampaigns.FindAsync(id);
            if (campaign == null) return NotFound();

            campaign.Clicks++;
            await _context.SaveChangesAsync();
            return Ok(campaign);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var campaign = await _context.AdCampaigns.FindAsync(id);
            if (campaign == null) return NotFound();

            var roi = campaign.Budget > 0 ? ((decimal)campaign.Clicks * 10) / campaign.Budget : 0; // Example: 10 currency units per click
            return Ok(new
            {
                campaign.Id,
                campaign.Title,
                campaign.Impressions,
                campaign.Clicks,
                ROI = roi
            });
        }
        
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMine()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (isAdmin)
            {
                return Ok(await _context.AdCampaigns.ToListAsync());
            }
            else
            {
                return Ok(await _context.AdCampaigns
                    .Where(c => c.CreatedByUserId == user.Id)
                    .ToListAsync());
            }
        }



    }
}
