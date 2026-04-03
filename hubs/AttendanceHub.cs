using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Api.Hubs
{
    public class AttendanceHub : Hub
    {
        // public async Task SendUpdate(object data)
        // {
        //     await Clients.All.SendAsync("ReceiveAttendanceUpdate", data);
        // }
    }
}
