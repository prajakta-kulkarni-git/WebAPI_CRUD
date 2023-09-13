using Microsoft.AspNetCore.Mvc;
using UserWebAPI.Model;

namespace UserWebAPI
{
    public interface IUserController
    {
        Task<IActionResult> GetUsers();
        Task<IActionResult> GetUsers(string emailId);
        Task<IActionResult> Create(AddUserRequest obj);
        Task<IActionResult> Update(Guid id, UpdateUserRequest updateUserRequest);
        Task<IActionResult> UpdateByEmail(UpdateUserRequest updateUserRequest);
    }
}
