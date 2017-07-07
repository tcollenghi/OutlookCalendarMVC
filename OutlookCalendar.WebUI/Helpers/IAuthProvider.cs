using System.Threading.Tasks;

namespace OutlookCalendar.WebUI.Helpers
{
    public interface IAuthProvider
    {
        Task<string> GetUserAccessTokenAsync();
    }
}
