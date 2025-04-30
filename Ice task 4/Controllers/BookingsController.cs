using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ice_task_4.Data;
using Ice_task_4.Models;

namespace Ice_task_4.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            // Display success message if available
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
            }

            // If user is admin, show all bookings
            if (User.IsInRole("Admin"))
            {
                var bookings = await _context.Bookings
                    .Include(b => b.Room)
                    .Include(b => b.Guest)
                    .ToListAsync();
                return View(bookings);
            }
            else
            {
                // For regular users, show only their own bookings
                // Find the guest based on the username
                var userEmail = User.Identity.Name;
                var guest = await _context.Guests
                    .FirstOrDefaultAsync(g => g.Email == userEmail);

                if (guest != null)
                {
                    var bookings = await _context.Bookings
                        .Include(b => b.Room)
                        .Include(b => b.Guest)
                        .Where(b => b.GuestId == guest.GuestId)
                        .ToListAsync();

                    return View(bookings);
                }
                else
                {
                    return View(new List<Booking>());
                }
            }
        }

        // GET: Bookings/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Guest)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Receipt/5
        public async Task<IActionResult> Receipt(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Guest)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Check if user is authorized to view this receipt
            if (!User.IsInRole("Admin") && booking.Guest.Email != User.Identity.Name)
            {
                return Forbid();
            }

            return View(booking);
        }

        // GET: Bookings/Cancel/5
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Guest)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Check if user is authorized to cancel this booking
            if (!User.IsInRole("Admin") && booking.Guest.Email != User.Identity.Name)
            {
                return Forbid();
            }

            // Only allow cancellation if not already cancelled and the check-in date is in the future
            if (booking.IsCancelled || booking.CheckInDate <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "This booking cannot be cancelled.";
                return RedirectToAction(nameof(Index));
            }

            return View(booking);
        }

        // POST: Bookings/CancelConfirmed/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Guest)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Check if user is authorized to cancel this booking
            if (!User.IsInRole("Admin") && booking.Guest.Email != User.Identity.Name)
            {
                return Forbid();
            }

            // Only allow cancellation if not already cancelled and the check-in date is in the future
            if (booking.IsCancelled || booking.CheckInDate <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "This booking cannot be cancelled.";
                return RedirectToAction(nameof(Index));
            }

            // Cancel the booking
            booking.IsCancelled = true;

            // Make the room available again
            if (booking.Room != null)
            {
                booking.Room.IsAvailable = true;
                _context.Update(booking.Room);
            }

            _context.Update(booking);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your booking has been successfully cancelled.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Bookings/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["RoomId"] = new SelectList(_context.Rooms.Where(r => r.IsAvailable), "RoomId", "RoomNumber");
            ViewData["GuestId"] = new SelectList(_context.Guests, "GuestId", "FullName", null, "Email");
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("BookingId,RoomId,GuestId,CheckInDate,CheckOutDate,TotalPrice,IsCancelled")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                // Calculate the total price based on the room price and stay duration
                var room = await _context.Rooms.FindAsync(booking.RoomId);
                if (room != null)
                {
                    var days = (booking.CheckOutDate - booking.CheckInDate).Days;
                    booking.TotalPrice = room.PricePerNight * days;

                    // Mark the room as unavailable
                    room.IsAvailable = false;
                    _context.Update(room);
                }

                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["RoomId"] = new SelectList(_context.Rooms.Where(r => r.IsAvailable), "RoomId", "RoomNumber", booking.RoomId);
            ViewData["GuestId"] = new SelectList(_context.Guests, "GuestId", "FullName", booking.GuestId);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomNumber", booking.RoomId);
            ViewData["GuestId"] = new SelectList(_context.Guests, "GuestId", "FullName", booking.GuestId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,RoomId,GuestId,CheckInDate,CheckOutDate,TotalPrice,IsCancelled")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Recalculate the total price based on the room price and stay duration
                    var room = await _context.Rooms.FindAsync(booking.RoomId);
                    if (room != null)
                    {
                        var days = (booking.CheckOutDate - booking.CheckInDate).Days;
                        booking.TotalPrice = room.PricePerNight * days;
                    }

                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["RoomId"] = new SelectList(_context.Rooms, "RoomId", "RoomNumber", booking.RoomId);
            ViewData["GuestId"] = new SelectList(_context.Guests, "GuestId", "FullName", booking.GuestId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.Guest)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking != null)
            {
                // Make the room available again
                if (booking.Room != null)
                {
                    booking.Room.IsAvailable = true;
                    _context.Update(booking.Room);
                }

                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}