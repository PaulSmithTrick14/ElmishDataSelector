using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TemplateSelector
{
  public class PaneTypeDataSelector : DataTemplateSelector
  {
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      var element = container as FrameworkElement;
      if (element == null || item == null)
      {
        return null;
      }

      var model = (element.Parent as ContentControl).Tag as TemplateSelector.Program.Pane.Model;
      if (model.Specific.IsGatherInput)
      {
        return element.FindResource("InputTemplate") as DataTemplate;
      }

      if (model.Specific.IsShowReport)
      {
        return element.FindResource("ReportTemplate") as DataTemplate;
      }

      return null;

    }
  }
}
