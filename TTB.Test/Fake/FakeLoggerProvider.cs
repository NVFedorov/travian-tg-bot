using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;

namespace TTB.Test.Fake
{
    public static class FakeLoggerProvider<T>
    {
        public static ILogger<T> GetLogger()
        {
            var logger = new Mock<ILogger<T>>();
            return logger.Object;
        }
    }
}
