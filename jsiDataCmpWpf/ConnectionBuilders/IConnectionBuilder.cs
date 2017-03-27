using jsiDataCmpCore;

namespace jsiDataCmpWpf.ConnectionBuilders
{
    public interface IConnectionBuilder
    {
        IDatabaseManager Manager { get; set; }
        bool? ShowDialog();
    }
}
