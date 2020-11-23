using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using TravianTelegramBot.Commands;
using TTB.Common.Settings;

namespace TravianTelegramBot.Services.Impl
{
    public class CommandFactory : ICommandFactory
    {
        private readonly IServiceProvider _service;
        private readonly ILogger<CommandFactory> _logger;
        private readonly IBotService _bot;

        public CommandFactory(IServiceProvider service, ILogger<CommandFactory> logger, IBotService bot)
        {
            _service = service;
            _logger = logger;
            _bot = bot;
        }

        public AbstractCommand GetCommand(Message message)
        {
            AbstractCommand instance = null;
            var text = message.Text.ToLower().Split(" ");
            if (text.Length > 0)
            {
                instance = GetCommand(text[0].Replace("/", string.Empty).ToLower(), message.Chat.Id);
            }

            return instance ?? new DefaultCommand(_service, message.Chat.Id);
        }

        public IQueueableCommand GetQueueableCommand(string commandName, long chatId)
        {
            IQueueableCommand instance;
            if (commandName.Length > 0)
            {
                var commandTypes = FindAllDerivedTypes<IQueueableCommand>();
                var commandType = commandTypes.FirstOrDefault(x => x.Name.ToLower().Contains(commandName.ToLower()));

                try
                {
                    if (commandType != null)
                    {
                        instance = (IQueueableCommand)Activator.CreateInstance(commandType, _service, chatId);
                        return instance;
                    }
                }
                catch (Exception exc)
                {
                    _logger.LogError(LoggingEvents.CreateCommandException, exc, exc.Message);
                }
            }

            throw new Exception($"Can not find queuable command for type described as [{commandName}]");
        }

        private static List<Type> FindAllDerivedTypes<T>()
        {
            return FindAllDerivedTypes<T>(Assembly.GetAssembly(typeof(T)));
        }

        private static List<Type> FindAllDerivedTypes<T>(Assembly assembly)
        {
            var derivedType = typeof(T);
            return assembly
                .GetTypes()
                .Where(t =>
                    t != derivedType &&
                    derivedType.IsAssignableFrom(t)
                    ).ToList();

        }

        public AbstractCommand GetCommand(string commandName, long chatId)
        {
            AbstractCommand instance = null;
            var commandTypes = FindAllDerivedTypes<AbstractCommand>();
            var commandType = commandTypes.FirstOrDefault(x => x.Name.ToLower().Contains(commandName.ToLower()));
            try
            {
                if (commandType != null)
                {
                    instance = (AbstractCommand)Activator.CreateInstance(commandType, _service, chatId);
                }

                return instance;
            }
            catch (Exception exc)
            {
                _logger.LogError(LoggingEvents.CreateCommandException, exc, exc.Message);
            }

            return instance ?? new DefaultCommand(_service, chatId);
        }
    }
}
