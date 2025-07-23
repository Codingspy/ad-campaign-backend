using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdCampaignMVP.Data;
using AdCampaignMVP.Models;
using AdCampaignMVP.Services; // Make sure this is there

namespace AdCampaignMVP.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdCampaignsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public AdCampaignsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.AdCampaigns.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create(AdCampaign campaign)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized("User not found.");

            campaign.CreatedByUserId = user.Id;
            _context.AdCampaigns.Add(campaign);
            await _context.SaveChangesAsync();

            // Send email to user
            await _emailSender.SendEmailAsync(
                user.Email,
                "Campaign Created",
                $"Your campaign \"{campaign.Title}\" has been created with a budget of ₹{campaign.Budget}.");

            // Send copy to Admin
            await _emailSender.SendEmailAsync(
                "admin@example.com",
                "New Campaign Created",
                $"User {user.Email} created campaign \"{campaign.Title}\".");

            return Ok(campaign);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, AdCampaign updated)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized("User not found.");
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
            if (user == null) return Unauthorized("User not found.");
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            var existing = await _context.AdCampaigns.FindAsync(id);
            if (existing == null) return NotFound();

            if (!isAdmin && existing.CreatedByUserId != user.Id)
            {
                return Forbid("You do not have permission to delete this campaign.");
            }

            _context.AdCampaigns.Remove(existing);
            await _context.SaveChangesAsync();
            return Ok("Deleted");
        }

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

            var roi = campaign.Budget > 0 ? ((decimal)campaign.Clicks * 10) / campaign.Budget : 0;
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
            if (user == null) return Unauthorized("User not found.");
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
