public interface IRefitClientWrapper<out T>
{
    T CreateClient();
}