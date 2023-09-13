using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserWebAPI.Data;
using UserWebAPI.Model;

namespace UserWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class UserController : ControllerBase, IUserController
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<UserController> _logger;
        public UserController(ApplicationDbContext db, ILogger<UserController> logger)
        {
            _db = db;
            _logger = logger;
        }

        //<<summary>>
        //<<Params>>No parameters
        //<<Returns>> list of users
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            _logger.LogInformation("Getting user data...");
            try
            {
                IEnumerable<User> objUserList = await _db.Users.ToListAsync();
                return Ok(objUserList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching user data.");
                throw;
            }
        }

        //<<summary>>
        //<<Params>>string email id
        //<<Returns>> list of users
        [HttpGet("{emailId}")]
        public async Task<IActionResult> GetUsers([FromRoute] string emailId)
        {
            _logger.LogInformation("Getting specific user data by id...");
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == emailId);
                if (user != null)
                {
                    return Ok(user);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching specific user data.");
                throw;
            }
        }

        //<<summary>>Adding new user record 
        //<<Params>>AddUserRequest
        //<<Returns>> list of users
        //POST Method
        [HttpPost]
        public async Task<IActionResult> Create(AddUserRequest obj)
        {
            _logger.LogInformation("Creating user ...");
            try
            {
                if (obj.Email == null) {
                    return Conflict($"Please enter at least email Id.");
                }
                else
                {
                    // Check for an existing user with a non-null email
                    var userWithEmail = await _db.Users.FirstOrDefaultAsync(u => u.Email == obj.Email);

                    if (userWithEmail == null && obj != null)
                    {
                        var user = new User()
                        {
                            Id = Guid.NewGuid(),
                            Address = obj.Address,
                            FirstName = obj.FirstName,
                            LastName = obj.LastName,
                            Email = obj.Email,
                            ContactNumber = obj.ContactNumber
                        };

                        await _db.Users.AddAsync(user);
                        await _db.SaveChangesAsync();
                        return Ok(user);
                    }
                    else
                    {
                        return Conflict($"User with email '{obj.Email}' already exists.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating user.");
                throw;
            }

        }

        //<<summary>>Updating existing user record 
        //<<Params>>Guid id
        //<<Returns>> Updated record.
        //PUT Method
        [HttpPut]
        [Route("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, UpdateUserRequest updateUserRequest)
        {
            _logger.LogInformation("Updating user data...");
            try
            {
                var user = await _db.Users.FindAsync(id);
                if (user != null)
                {
                    if (updateUserRequest.Address != null)
                    {
                        user.Address = updateUserRequest.Address;
                    }
                    if (updateUserRequest.ContactNumber != null)
                    {
                        user.ContactNumber = (long)updateUserRequest.ContactNumber;
                    }
                    if (updateUserRequest.FirstName != null)
                    {
                        user.FirstName = updateUserRequest.FirstName;
                    }
                    if (updateUserRequest.LastName != null)
                    {
                        user.LastName = updateUserRequest.LastName;
                    }
                    if (updateUserRequest.LastName != null)
                    {
                        user.LastName = updateUserRequest.LastName;
                    }
                    if (updateUserRequest.Email != null)
                    {
                        user.Email = updateUserRequest.Email;
                    }
                    await _db.SaveChangesAsync();
                    return Ok(user);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating user.");
                throw;
            }


        }

        //<<summary>>Updating existing user record using email id
        //<<Params>>UpdateUserRequest
        //<<Returns>> Updated record.
        //POST Method
        [HttpPut("updateUserbyemail")]
        public async Task<IActionResult> UpdateByEmail([FromBody] UpdateUserRequest updateUserRequest)
        {
            _logger.LogInformation("Updating user data by email...");
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == updateUserRequest.Email);
                if (user != null && updateUserRequest.Email != null)
                {
                    if (updateUserRequest.Address != null)
                    {
                        user.Address = updateUserRequest.Address;
                    }
                    if (updateUserRequest.ContactNumber != null)
                    {
                        user.ContactNumber = (long)updateUserRequest.ContactNumber;
                    }
                    if (updateUserRequest.FirstName != null)
                    {
                        user.FirstName = updateUserRequest.FirstName;
                    }
                    if (updateUserRequest.LastName != null)
                    {
                        user.LastName = updateUserRequest.LastName;
                    }
                    if (updateUserRequest.LastName != null)
                    {
                        user.LastName = updateUserRequest.LastName;
                    }
                    await _db.SaveChangesAsync();
                    return Ok(user);
                }
                else
                {
                    return NotFound($"User with email '{updateUserRequest.Email}' not found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating user.");
                throw;
            }
        }
    }
}
