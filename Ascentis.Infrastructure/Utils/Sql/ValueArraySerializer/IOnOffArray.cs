namespace Ascentis.Infrastructure.Utils.Sql.ValueArraySerializer
{
    public interface IOnOffArray
    {
        bool this[int index] { get; set; }
        int Count { get; }
    }
}
