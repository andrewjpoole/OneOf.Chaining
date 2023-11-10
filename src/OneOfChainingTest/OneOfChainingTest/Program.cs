// See https://aka.ms/new-console-template for more information

using OneOf.Types;
using OneOf.Chaining;
using OneOf;

Console.WriteLine("Hello, World!");
await GoToEvent();

async Task<OneOf<StateStore, Error>> GoToEvent()
{
    return await StateStore.Create()
        .Then(Tasks.DriveToEvent)
        .IfThen(s => s.FeelingHungry, s => Tasks.EatPizza(s, "Pepperoni", "Ham&Pineapple", "Margherita"))
        .ThenWaitForFirst(Tasks.FullStomach, Tasks.RunOutOfPizza)
        .ThenWaitForAll(Tasks.LearnStuff, Tasks.AskQuestions)
        .Then(Tasks.ChatWithNewFriends)
        .Then(Tasks.DriveHomeHappyAndHopefullyInspired);
}


public class StateStore
{
    public string Name { get; set; } = "";
    public bool FeelingHungry;

    public static Task<OneOf<StateStore, Error>> Create() =>
        Task.FromResult(OneOf<StateStore, Error>.FromT0(new StateStore()));
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