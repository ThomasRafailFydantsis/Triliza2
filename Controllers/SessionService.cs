using System.Runtime.ConstrainedExecution;
using Microsoft.EntityFrameworkCore;
using triliza.Data;
using triliza.Models;

namespace triliza.Controllers
{
    public class SessionService
    {
        private readonly ApplicationDbContext _context;

        public SessionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Session> CreateSession(string userId)
        {
            var ses = _context.Sessions.Where(i=>i.IsFull==false).ToList();
            if (ses.Any())
            {
                var ses1 = _context.Sessions.Where(d => d.IsFull == false).ToList().FirstOrDefault();
                
                ses1.IsFull = true;
                await _context.SaveChangesAsync();
                return ses1;
            }
            else
            {
                var sesNew = new Session
                {
                    Score = "",
                    IsFull = false,
                    MatchIds = {},
                    DateOfGame = DateTime.Now
                };
                _context.Sessions.Add(sesNew);
                _context.SaveChanges();
                return sesNew;
                //var sesNew =new Session
                // {
                //     Score = 0,
                //     IsFull = false,
                //     MatchIds = ses.Select(i=> new Match
                //     {

                //         UserId = userId,

                //     }).ToList(),
                //     DateOfGame = DateTime.Now
                // };
                // _context.Sessions.Add(sesNew);
                // _context.SaveChanges();
                // return sesNew;
            }
        }
        //public async Task<Session> AddMatchToSession(int sesId, string userId)
        //{
        //    var ses = _context.Sessions.Where(d => d.Id==sesId).ToList().FirstOrDefault();

        //}
        public async Task<Session> GetSessionByIdAsync(int sessionId)
        {
            var exam = await _context.Sessions
                .Include(c=>c.MatchIds)
                .FirstOrDefaultAsync(e => e.Id == sessionId);

            if (exam == null)
            {
                throw new KeyNotFoundException("Exam not found.");
            }

            return exam;
        }
    }
}
