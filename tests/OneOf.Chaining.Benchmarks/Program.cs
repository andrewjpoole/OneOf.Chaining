using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using OneOf.Types;

namespace OneOf.Chaining.Benchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<JustForFun>();
        }
    }

    [MemoryDiagnoser]
    public class JustForFun
    {
        [Benchmark]
        public async Task<OneOf<StateStore, Error>> GoToEvent()
        {
            return await StateStore.Create()
                .Then(Tasks.DriveToEvent)
                .IfThen(s => s.FeelingHungry,
                    s => Tasks.EatPizza(s,
                        "Pepperoni", "Ham&Pineapple", "Margherita"))
                .ThenWaitForFirst(Tasks.FullStomach, Tasks.RunOutOfPizza)
                .ThenWaitForAll(Tasks.LearnStuff, Tasks.AskQuestions)
                .Then(Tasks.ChatWithNewFriends)
                .Then(Tasks.DriveHomeHappyAndHopefullyInspired);
        }

        [Benchmark]
        public async Task<OneOf<StateStore, Error>> GoToEventWithoutChaining()
        {
            var s = await StateStore.Create();

            var x1 = await Tasks.DriveToEvent(s.AsT0);

            var x2 = x1;
            if (x1.AsT0.FeelingHungry)
            {
                x2 = await Tasks.EatPizza(x1.AsT0, "Pepperoni", "Ham&Pineapple", "Margherita");
            }

            var x3 = await await Task.WhenAny(Tasks.FullStomach(x2.AsT0), Tasks.RunOutOfPizza(x2.AsT0));

            await Task.WhenAll(Tasks.LearnStuff(x3.AsT0), Tasks.AskQuestions(x3.AsT0));

            var x4 = await Tasks.ChatWithNewFriends(x3.AsT0);

            return await Tasks.DriveHomeHappyAndHopefullyInspired(x4.AsT0);
        }

        public class StateStore
        {
            public string Name { get; set; } = "";
            public bool FeelingHungry;

            public static Task<OneOf<StateStore, Error>> Create() =>
                Task.FromResult(OneOf<StateStore, Error>.FromT0(new StateStore { FeelingHungry = true }));
        }

        public class Tasks
        {
            public static async Task<OneOf<StateStore, Error>> DriveToEvent(StateStore test)
            {
                return test;
            }
            public static async Task<OneOf<StateStore, Error>> EatPizza(StateStore test, params string[] availableFlavours)
            {
                return test;
            }
            public static async Task<OneOf<StateStore, Error>> FullStomach(StateStore test)
            {
                return test;
            }
            public static async Task<OneOf<StateStore, Error>> RunOutOfPizza(StateStore test)
            {
                return test;
            }
            public static async Task<OneOf<StateStore, Error>> LearnStuff(StateStore test)
            {
                return test;
            }
            public static async Task<OneOf<StateStore, Error>> AskQuestions(StateStore test)
            {
                return test;
            }
            public static async Task<OneOf<StateStore, Error>> ChatWithNewFriends(StateStore test)
            {
                return test;
            }
            public static async Task<OneOf<StateStore, Error>> DriveHomeHappyAndHopefullyInspired(StateStore test)
            {
                return test;
            }
        }
    }
}
