using Discord.Commands;

namespace MooBot.Preconditions
{
    public class OwnerOnlyAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var application = await context.Client.GetApplicationInfoAsync();
            if (context.User.Id == application.Owner.Id) return PreconditionResult.FromSuccess();

            return PreconditionResult.FromError("Only my owner can see this side of Moo~");
        }
    }
}
