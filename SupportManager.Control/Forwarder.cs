using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using SupportManager.Contracts;
using SupportManager.DAL;

namespace SupportManager.Control
{
    public class Forwarder : IForwarder
    {
        private readonly SupportManagerContext context;
        private readonly TaskFactory taskFactory;

        public Forwarder(SupportManagerContext context, TaskFactory taskFactory)
        {
            this.context = context;
            this.taskFactory = taskFactory;
        }

        public async Task ApplyScheduledForward(int scheduledForwardId)
        {
            var scheduledForward = context.ScheduledForwards.Include(fwd => fwd.Team)
                .Include(fwd => fwd.PhoneNumber)
                .Single(fwd => fwd.Id == scheduledForwardId);

            if (scheduledForward.Deleted) return;

            await taskFactory.StartNew(() =>
            {
                using (var helper = new ATHelper(scheduledForward.Team.ComPort))
                    helper.ForwardTo(scheduledForward.PhoneNumber.Value);
            });
        }

        public async Task ApplyForward(int teamId, int userPhoneNumberId)
        {
            var team = context.Teams.Find(teamId);
            var userPhoneNumber = context.UserPhoneNumbers.Find(userPhoneNumberId);

            if (team == null || userPhoneNumber == null) return;

            await taskFactory.StartNew(() =>
            {
                using (var helper = new ATHelper(team.ComPort))
                    helper.ForwardTo(userPhoneNumber.Value);
            });
        }

        public async Task ReadAllTeamStatus()
        {
            foreach (var team in context.Teams)
            {
                var state = await context.ForwardingStates.OrderByDescending(s => s.When).FirstOrDefaultAsync();
                var number = await taskFactory.StartNew(() =>
                {
                    using (var helper = new ATHelper(team.ComPort)) return helper.GetForwardedPhoneNumber();
                });

                if (state?.RawPhoneNumber == number) continue;

                var newState = new ForwardingState
                {
                    Team = team,
                    When = DateTimeOffset.Now,
                    RawPhoneNumber = number,
                    DetectedPhoneNumber = context.UserPhoneNumbers
                        .FirstOrDefault(p => p.Value == number)
                };

                context.ForwardingStates.Add(newState);
                context.SaveChanges();
            }
        }
    }
}