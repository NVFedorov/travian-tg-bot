using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Moq;


namespace TTB.Test.Fake
{
    public class FakeServiceProviderBuilder
    {
        protected Mock<IServiceProvider> _serviceMock;

        protected FakeServiceProviderBuilder()
        {
            _serviceMock = new Mock<IServiceProvider>();
        }

        public static FakeServiceProviderBuilder Builder()
        {
            return new FakeServiceProviderBuilder();
        }

        public FakeServiceProviderBuilder WithService<T>(T service)
        {
            _serviceMock.Setup(x => x.GetService(typeof(T))).Returns(service);
            return this;
        }

        public IServiceProvider Build()
        {
            return _serviceMock.Object;
        }
    }
}
