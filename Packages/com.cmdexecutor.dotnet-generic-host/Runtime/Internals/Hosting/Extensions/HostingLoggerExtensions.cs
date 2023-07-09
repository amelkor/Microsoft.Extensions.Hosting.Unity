using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting.Unity
{
internal static class HostingLoggerExtensions
    {
        public static void ApplicationError(this ILogger logger, EventId eventId, string? message, Exception? exception)
        {
            if (exception is ReflectionTypeLoadException reflectionTypeLoadException)
            {
                foreach (Exception? ex in reflectionTypeLoadException.LoaderExceptions)
                {
                    if (ex is not null)
                    {
                        message = message + Environment.NewLine + ex.Message;
                    }
                }
            }

            logger.LogCritical(
                eventId: eventId,
                message: message,
                exception: exception);
        }

        public static void Starting(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                   eventId: LoggerEventIds.Starting,
                   message: "Hosting starting");
            }
        }

        public static void Started(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    eventId: LoggerEventIds.Started,
                    message: "Hosting started");
            }
        }

        public static void Stopping(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    eventId: LoggerEventIds.Stopping,
                    message: "Hosting stopping");
            }
        }

        public static void Stopped(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    eventId: LoggerEventIds.Stopped,
                    message: "Hosting stopped");
            }
        }

        public static void StoppedWithException(this ILogger logger, Exception? ex)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(
                    eventId: LoggerEventIds.StoppedWithException,
                    exception: ex,
                    message: "Hosting shutdown exception");
            }
        }

        public static void BackgroundServiceFaulted(this ILogger logger, Exception? ex)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(
                    eventId: LoggerEventIds.BackgroundServiceFaulted,
                    exception: ex,
                    message: "BackgroundService failed");
            }
        }

        public static void BackgroundServiceStoppingHost(this ILogger logger, Exception? ex)
        {
            if (logger.IsEnabled(LogLevel.Critical))
            {
                logger.LogCritical(
                    eventId: LoggerEventIds.BackgroundServiceStoppingHost,
                    exception: ex,
                    message: "The HostOptions.BackgroundServiceExceptionBehavior is configured to StopHost. A BackgroundService has thrown an unhandled exception, and the IHost instance is stopping");
            }
        }
    }
}