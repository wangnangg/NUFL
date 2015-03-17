//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
namespace NUFL.Framework.Model
{
    /// <summary>
    /// defines an InstrumentationModelBuilder
    /// </summary>
    public interface IInstrumentationModelBuilder
    {
        /// <summary>
        /// Build model for a module
        /// </summary>
        /// <param name="full">include class, methods etc</param>
        /// <returns></returns>
        Module BuildModuleModel(bool full);
        
        /// <summary>
        /// check if module can be instrumented
        /// </summary>
        bool CanInstrument { get; }
    }
}