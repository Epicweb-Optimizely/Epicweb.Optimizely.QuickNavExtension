using Epicweb.Alloy.QuickNavExtension.Models.ViewModels;

namespace Epicweb.Alloy.QuickNavExtension.Business;

/// <summary>
/// Defines a method which may be invoked by PageContextActionFilter allowing controllers
/// to modify common layout properties of the view model.
/// </summary>
internal interface IModifyLayout
{
    void ModifyLayout(LayoutModel layoutModel);
}
