using Microsoft.AspNetCore.SignalR;

namespace QuizApp.Hubs
{
    public class QuizHub : Hub
    {
        
        public async Task JoinLobby(string pin, string playerName)
        {
           
            await Groups.AddToGroupAsync(Context.ConnectionId, pin);

            
            await Clients.Group(pin).SendAsync("PlayerJoined", playerName);
        }

        
        public async Task StartQuiz(string pin)
        {
            await Clients.Group(pin).SendAsync("QuizStarted");
        }
    }
}