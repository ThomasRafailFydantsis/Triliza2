using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using triliza.Data;
using triliza.Models;

namespace triliza.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserHistoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserHistoryController(ApplicationDbContext context)
        {
         
            this._context = context;
        }

        [HttpGet("get-user-profile/{userId}")]
        public async Task<IActionResult> GetMatchHistoryByUserId(string userId)
        {
            var tem = _context.PreSession
             .Where(p => p.UserId == userId);
            var user = _context.Users
             .Where(u=>u.Id == userId);

            var query = from p in tem
                        join s in _context.Sessions on p.SessionId equals s.Id into sessionGroup
                        from s in sessionGroup.DefaultIfEmpty() // Right join workaround
                        join pr in _context.PreSession on s.Id equals pr.SessionId
                        join u in _context.Users on p.UserId equals u.Id into userGroup
                        from u in userGroup.DefaultIfEmpty()
                        join u2 in _context.Users on pr.UserId equals u2.Id into user2Group
                        from u2 in user2Group.DefaultIfEmpty()
                        where p.UserId != pr.UserId
                        select new
                        {
                            SessionId = s.Id,
                            Score = s.Score,
                            MatchUsers = s.MatchIds.Select(m => new {
                                MatchId = m.Id,
                                User = new
                                {
                                    FirstName = m.User.FirstName,
                                    LastName = m.User.LastName,
                                    UserName = m.User.UserName
                                }
                            })
                        };


            if (!query.Any())
            {
                return NotFound("No certificates found for this user.");
            }

            return Ok(query);
            //List<Session> sessions = new List<Session>();

            //var history = _context.PreSession.Where(p=>p.UserId == userId)
            //    .Select(p=>p.SessionId)
            //    .ToList();
            //if (history == null)
            //{
            //    return NotFound();
            //}

            //var perosnsHistory = _context.Sessions.ToList();


        }
    }
}
