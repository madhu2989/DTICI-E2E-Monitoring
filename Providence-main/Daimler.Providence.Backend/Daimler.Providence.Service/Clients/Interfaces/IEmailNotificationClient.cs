using System.Threading.Tasks;
using Daimler.Providence.Service.Models.NotificationRule;
using Daimler.Providence.Service.Models.StateTransition;

namespace Daimler.Providence.Service.Clients.Interfaces
{
    /// <summary>
    /// Client used to send Providence Notifications to users defined in a Notification Rule.
    /// </summary>
    public interface IEmailNotificationClient
    {
        /// <summary>
        /// Method to send an E-Mail via SendGrid to a specified user.
        /// </summary>
        /// <param name="transition">StateTransition which contains information about the state change.</param>
        /// <param name="lastState">The state an element had before the stateChange.</param>
        /// <param name="resolved">The flag which indicates whether an alert was resolved or not.</param>
        /// <param name="rule">Rule which contains information about the user the E-Mail should be sent to.</param>
        /// <param name="highestNotificationLevel">Level for which the notification was triggered.</param>
        Task SendEmail(StateTransition transition, State? lastState, bool resolved, GetNotificationRule rule, string highestNotificationLevel);
    }
}