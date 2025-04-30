using System.Runtime.Intrinsics.X86;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using triliza.Data;
using triliza.Models;

namespace triliza.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly SessionService _sese;

        public MatchController(ApplicationDbContext context,SessionService sese)
        {
            
            this._context = context;
            this._sese = sese;
        }
        [HttpGet("{SessionId}")]
        public async Task<IActionResult> GetExamById(int SessionId)
        {
            try
            {
                var ses = await _sese.GetSessionByIdAsync(SessionId);

                if (ses == null)
                {
                    return NotFound();
                }

                return Ok(new
                {
                    ses.IsFull,
                    ses.DateOfGame,
                    ses.Score,
                    MatchingIds = ses.MatchIds.Select(a => new
                    {
                        a.Id,
                        a.UserId,
                        a.Shape,
                        a.Points
                       
                    })
                });
            }

            catch (KeyNotFoundException)
            {
                return NotFound("Exam not found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving exam: {ex.Message}");
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpPost("find")]
        public async Task<IActionResult> FindOpps(string userId)
        {
          var ses= _context.Sessions.Where(s => s.IsFull == false).ToList();
            if (ses.Any())
            {
              var ses3 =await _sese.CreateSession(userId);
              var pre2=  new Match
                {
                    UserId = userId,
                    SessionId = ses3.Id,
                    Shape="O",
                    Points=0
                };
                
                _context.PreSession.Add(pre2);
                var latestSs = _context.Sessions.Where(s => s.Id == ses3.Id).FirstOrDefault();
                if (latestSs != null)
                {
                    latestSs.MatchIds.Add(pre2);
                }
                await _context.SaveChangesAsync();
                return Ok(new { ses3.Id });
            }
            var ses2 = await _sese.CreateSession( userId);
            var pre = new Match
            {
                UserId = userId,
                SessionId = ses2.Id,
                Shape = "X",
                Points =0
            };
             
            _context.PreSession.Add(pre);
            var latestS = _context.Sessions.Where(s => s.Id == ses2.Id).FirstOrDefault();
            if (latestS != null)
            {
                latestS.MatchIds.Add(pre);
            }
            await _context.SaveChangesAsync();
               
            return Ok(new { ses2.Id });
        }
    }
}
