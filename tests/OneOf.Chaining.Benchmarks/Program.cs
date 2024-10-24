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

    /*
       | Method                                  | Mean       | Error    | StdDev   | Gen0   | Allocated |
       |---------------------------------------- |-----------:|---------:|---------:|-------:|----------:|
       | GoToEvent                               | 1,115.5 ns | 22.11 ns | 48.98 ns | 0.3948 |    2488 B |
       | GoToEventWithCapture                    | 1,064.5 ns | 21.26 ns | 39.41 ns | 0.4101 |    2576 B |
       | GoToEventWithoutParallel                |   293.5 ns |  8.37 ns | 24.42 ns | 0.1543 |     968 B |
       | GoToEventWithoutChaining                |   381.5 ns |  7.45 ns | 11.61 ns | 0.1874 |    1176 B |
       | GoToEventWithoutChainingWithoutParallel |   123.5 ns |  2.51 ns |  6.01 ns | 0.0892 |     560 B |
       
       // * Legends *
         Mean      : Arithmetic mean of all measurements
         Error     : Half of 99.9% confidence interval
         StdDev    : Standard deviation of all measurements
         Gen0      : GC Generation 0 collects per 1000 operations
         Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
         1 ns      : 1 Nanosecond (0.000000001 sec)     
     */

    [MemoryDiagnoser]
    public class JustForFun
    {
        [Benchmark]
        public async Task<OneOf<MeetupAtendee, Error>> GoToEvent()
        {
            return await MeetupAtendee.Create()
                .Then(Tasks.DriveToEvent)
                .IfThen(x => x.FeelingHungry,
                    x => Tasks.EatPizza(x, "Pepperoni", "Ham&Pineapple", "Margherita"))
                .ThenWaitForFirst(Tasks.FullStomach, Tasks.RunOutOfPizza)
                .ThenWaitForAll(Tasks.LearnStuff, Tasks.AskQuestions)
                .Then(Tasks.ChatWithNewFriends)
                .Then(Tasks.DriveHomeHappyAndHopefullyInspired);
        }

        [Benchmark]
        public async Task<OneOf<MeetupAtendee, Error>> GoToEventWithCapture()
        {
            var favouriteFlavours = new[] {"Pepperoni", "Ham&Pineapple", "Margherita"};
            return await MeetupAtendee.Create()
                .Then(Tasks.DriveToEvent)
                .IfThen(s => s.FeelingHungry,
                    s => Tasks.EatPizza(s,favouriteFlavours))
                .ThenWaitForFirst(Tasks.FullStomach, Tasks.RunOutOfPizza)
                .ThenWaitForAll(Tasks.LearnStuff, Tasks.AskQuestions)
                .Then(Tasks.ChatWithNewFriends)
                .Then(Tasks.DriveHomeHappyAndHopefullyInspired);
        }

        [Benchmark]
        public async Task<OneOf<MeetupAtendee, Error>> GoToEventWithoutParallel()
        {
            var favouriteFlavours = new[] { "Pepperoni", "Ham&Pineapple", "Margherita" };
            return await MeetupAtendee.Create()
                .Then(Tasks.DriveToEvent)
                .IfThen(s => s.FeelingHungry,
                    s => Tasks.EatPizza(s, favouriteFlavours))
                .Then(Tasks.ChatWithNewFriends)
                .Then(Tasks.DriveHomeHappyAndHopefullyInspired);
        }

        [Benchmark]
        public async Task<OneOf<MeetupAtendee, Error>> GoToEventWithoutChaining()
        {
            var s = await MeetupAtendee.Create();
            s = await Tasks.DriveToEvent(s.AsT0);
            if (s.AsT0.FeelingHungry)
            {
                s = await Tasks.EatPizza(s.AsT0, "Pepperoni", "Ham&Pineapple", "Margherita");
            }
            s = await await Task.WhenAny(Tasks.FullStomach(s.AsT0), Tasks.RunOutOfPizza(s.AsT0));
            await Task.WhenAll(Tasks.LearnStuff(s.AsT0), Tasks.AskQuestions(s.AsT0));
            var x4 = await Tasks.ChatWithNewFriends(s.AsT0);
            return await Tasks.DriveHomeHappyAndHopefullyInspired(x4.AsT0);
        }

        [Benchmark]
        public async Task<OneOf<MeetupAtendee, Error>> GoToEventWithoutChainingWithoutParallel()
        {
            var s = await MeetupAtendee.Create();
            s = await Tasks.DriveToEvent(s.AsT0);
            if (s.AsT0.FeelingHungry)
            {
                s = await Tasks.EatPizza(s.AsT0, "Pepperoni", "Ham&Pineapple", "Margherita");
            }
            var x4 = await Tasks.ChatWithNewFriends(s.AsT0);
            return await Tasks.DriveHomeHappyAndHopefullyInspired(x4.AsT0);
        }

        public class MeetupAtendee
        {
            public string Name { get; set; } = "";
            public bool FeelingHungry;

            public static Task<OneOf<MeetupAtendee, Error>> Create() =>
                Task.FromResult(OneOf<MeetupAtendee, Error>.FromT0(new MeetupAtendee { FeelingHungry = true }));
        }

        public class Tasks
        {
            public static async Task<OneOf<MeetupAtendee, Error>> DriveToEvent(MeetupAtendee test)
            {
                return test;
            }
            public static async Task<OneOf<MeetupAtendee, Error>> EatPizza(MeetupAtendee test, params string[] availableFlavours)
            {
                return test;
            }
            public static async Task<OneOf<MeetupAtendee, Error>> FullStomach(MeetupAtendee test)
            {
                return test;
            }
            public static async Task<OneOf<MeetupAtendee, Error>> RunOutOfPizza(MeetupAtendee test)
            {
                return test;
            }
            public static async Task<OneOf<MeetupAtendee, Error>> LearnStuff(MeetupAtendee test)
            {
                return test;
            }
            public static async Task<OneOf<MeetupAtendee, Error>> AskQuestions(MeetupAtendee test)
            {
                return test;
            }
            public static async Task<OneOf<MeetupAtendee, Error>> ChatWithNewFriends(MeetupAtendee test)
            {
                return test;
            }
            public static async Task<OneOf<MeetupAtendee, Error>> DriveHomeHappyAndHopefullyInspired(MeetupAtendee test)
            {
                return test;
            }
        }
    }
}
