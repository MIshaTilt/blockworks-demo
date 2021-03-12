namespace Blocks.Factories
{
    public interface IFactoryStrategy
    {
        void Build(Block block);
    }
}