using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradingBot.Api;
using TradingBot.Domain.Entities;

namespace TradingBot.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandlesController : ControllerBase
    {
        private readonly TradingBotDbContext _context;

        public CandlesController(TradingBotDbContext context)
        {
            _context = context;
        }

        // GET: api/Candles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Candle>>> GetCandles()
        {
            return await _context.Candles.ToListAsync();
        }

        // GET: api/Candles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Candle>> GetCandle(int id)
        {
            var candle = await _context.Candles.FindAsync(id);

            if (candle == null)
            {
                return NotFound();
            }

            return candle;
        }

        // PUT: api/Candles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCandle(int id, Candle candle)
        {
            if (id != candle.Id)
            {
                return BadRequest();
            }

            _context.Entry(candle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CandleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Candles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Candle>> PostCandle(Candle candle)
        {
            _context.Candles.Add(candle);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCandle", new { id = candle.Id }, candle);
        }

        // DELETE: api/Candles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCandle(int id)
        {
            var candle = await _context.Candles.FindAsync(id);
            if (candle == null)
            {
                return NotFound();
            }

            _context.Candles.Remove(candle);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CandleExists(int id)
        {
            return _context.Candles.Any(e => e.Id == id);
        }
    }
}
