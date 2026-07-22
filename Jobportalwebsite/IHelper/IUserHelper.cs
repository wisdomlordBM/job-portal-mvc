using Jobportalwebsite.Models;
using Jobportalwebsite.Viewmodel;

namespace Jobportalwebsite.IHelper
{
    public interface IUserHelper
    {
        Task<ApplicationUser> CreateUserByAsync(RegistrationViewModel model, string role);
        Task<List<ApplicationUser>> GetAllOtherUsersAsync(string currentUser);
    }
}
