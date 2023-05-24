
namespace FPV.Service.Common
{
    /// <summary>
    /// IHandleParameters interface for ViewModels that need to handle navigation parameters
    /// </summary>
    public interface IHandleParameters
    {
        void HandleParameters(object parameters);
    }
}
