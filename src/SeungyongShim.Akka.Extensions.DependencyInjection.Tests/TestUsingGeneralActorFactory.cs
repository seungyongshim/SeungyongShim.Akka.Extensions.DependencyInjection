namespace Tests
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Akka.Actor;
    using Akka.DI.Core;
    using Akka.DI.Extensions.DependencyInjection.TestKit;
    using Akka.TestKit.Xunit2;
    using FluentAssertions;
    using FluentAssertions.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Sample;
    using Xunit;

    public class TestUsingGeneralActorFactory : TestKit
    {
        [Fact]
        public async Task Check_Child_Actor_Recieved_Messages()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IActorRef>(sp => TestActor);
            services.AddSingleton<IPropsFactory<ChildActor>, PropsFactory<ChildActor, MockChildActor>>();
            services.AddAkka(Sys, new[]
            {
                "Sample"
            });

            Sys.UseDependencyInjectionServiceProvider(services.BuildServiceProvider());

            var parent = ActorOf(Sys.DI().PropsFactory<ParentActor>().Create(), "Parent");

            ExpectMsg<string>().Should().Be("Hello, Kid");
            ExpectMsg<string>().Should().Be("Hello, Kid");

            var child1 = await Sys.ActorSelection("/user/Parent/Child1").ResolveOne(5.Seconds());
            child1.Path.Name.Should().Be("Child1");

            var child2 = await Sys.ActorSelection("/user/Parent/Child2").ResolveOne(5.Seconds());
            child2.Path.Name.Should().Be("Child2");
        }

        [Fact]
        public async Task AddAkka()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IActorRef>(sp => TestActor);
            services.AddSingleton<IPropsFactory<ChildActor>, PropsFactory<ChildActor, MockChildActor>>();
            services.AddSingleton<ActorSystem>(sp => Sys.UseDependencyInjectionServiceProvider(sp));
            services.AddAkka(new[]
            {
                "Sample"
            });

            services.BuildServiceProvider().GetService<ActorSystem>()
                .ActorOf(Sys.DI().PropsFactory<ParentActor>().Create(), "Parent");

            ExpectMsg<string>().Should().Be("Hello, Kid");
            ExpectMsg<string>().Should().Be("Hello, Kid");

            var child1 = await Sys.ActorSelection("/user/Parent/Child1").ResolveOne(5.Seconds());
            child1.Path.Name.Should().Be("Child1");

            var child2 = await Sys.ActorSelection("/user/Parent/Child2").ResolveOne(5.Seconds());
            child2.Path.Name.Should().Be("Child2");
        }

        [Fact]
        public async Task Production()
        {
            var services = new ServiceCollection();
            services.AddTransient<ParentActor>();
            services.AddTransient<ChildActor>();
            services.AddAkka(Sys);

            Sys.UseDependencyInjectionServiceProvider(services.BuildServiceProvider());

            ActorOf(Sys.DI().PropsFactory<ParentActor>().Create(), "Parent");

            ExpectNoMsg();

            var child1 = await Sys.ActorSelection("/user/Parent/Child1").ResolveOne(5.Seconds());
            child1.Path.Name.Should().Be("Child1");

            var child2 = await Sys.ActorSelection("/user/Parent/Child2").ResolveOne(5.Seconds());
            child2.Path.Name.Should().Be("Child2");
        }

        
    }
}
