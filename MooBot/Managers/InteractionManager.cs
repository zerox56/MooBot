using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Moobot.Modules.Commands;
using MooBot.Modules.Commands;

namespace Moobot.Managers
{
    public class InteractionManager
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;

        public InteractionManager(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
        }

        public async Task InitializeAsync()
        {
            // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            // Process the InteractionCreated payloads to execute Interactions commands
            _client.InteractionCreated += HandleInteraction;
            _client.ModalSubmitted += OnModalSubmitted;
            _client.ButtonExecuted += OnButtonClicked;

            // Process the command execution results 
            _commands.SlashCommandExecuted += SlashCommandExecuted;
            _commands.ContextCommandExecuted += ContextCommandExecuted;
            _commands.ComponentCommandExecuted += ComponentCommandExecuted;
        }

        private Task SlashCommandExecuted(SlashCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
        {
            return Task.CompletedTask;
        }

        private Task ContextCommandExecuted(ContextCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
        {
            return Task.CompletedTask;
        }

        private Task ComponentCommandExecuted(ComponentCommandInfo arg1, Discord.IInteractionContext arg2, IResult arg3)
        {
            return Task.CompletedTask;
        }

        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
            {
                // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
                var ctx = new SocketInteractionContext(_client, arg);
                await _commands.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
                // response, or at least let the user know that something went wrong during the command execution.
                if (arg.Type == InteractionType.ApplicationCommand)
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }

        private Task OnModalSubmitted(SocketModal modal)
        {
            _ = Task.Run(async () =>
            {
                string customId = modal.Data.CustomId;
                ulong customIdNumber = ulong.MinValue;

                string pattern = @"\d+$";
                Match match = Regex.Match(customId, pattern);
                if (match.Success)
                {
                    string numberString = match.Value;
                    customIdNumber = ulong.Parse(numberString);
                    customId = customId.Substring(0, match.Index).Trim();
                }

                switch (customId)
                {
                    case "newReminder":
                        await ReminderCommands.CreateReminderFollowUp(modal);
                        break;
                    case "updateReminder":
                        await ReminderCommands.UpdateReminderFollowUp(modal, customIdNumber);
                        break;
                    case "addUpdateReminderGif":
                        await ReminderCommands.UpdateGifReminderFollowUp(modal, customIdNumber);
                        break;
                    case "addAnimalFact":
                        await AdminCommands.AddAnimalFactFollowUp(modal);
                        break;
                    default:
                        Console.WriteLine($"Uncaught case {modal.Data.CustomId} retrieved");
                        break;
                }
            });
            return Task.CompletedTask;
        }

        private Task OnButtonClicked(SocketMessageComponent component)
        {
            _ = Task.Run(async () =>
            {
                string customId = component.Data.CustomId;
                int customIdNumber = -1;

                string pattern = @"\d+$";
                Match match = Regex.Match(customId, pattern);
                if (match.Success)
                {
                    string numberString = match.Value;
                    customIdNumber = int.Parse(numberString);
                    customId = customId.Substring(0, match.Index).Trim();
                }

                switch (customId)
                {
                    case "setupReminder":
                        await ReminderCommands.CreateReminder(component);
                        break;
                    case "updateReminder":
                        await ReminderCommands.UpdateReminder(component, customIdNumber);
                        break;
                    case "viewReminder":
                        await ReminderCommands.GetReminders(component);
                        break;
                    case "addUpdateReminderGif":
                        await ReminderCommands.UpdateGifReminder(component, customIdNumber);
                        break;
                    case "deleteReminder":
                        await ReminderCommands.DeleteReminder(component, customIdNumber);
                        break;
                    case "changeUserStatusReminder":
                        await ReminderCommands.ChangeUserReminderStatus(component, customIdNumber);
                        break;
                    default:
                        Console.WriteLine($"Uncaught case {component.Data.CustomId} retrieved");
                        break;
                }
            });
            return Task.CompletedTask;
        }
    }
}