using MazeEscape.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeEscape.Controls
{
    public class WorldsDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UnlockedWorld { get; set; }
        public DataTemplate LockedWorld { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return ((CampaignWorld)item).Locked ? LockedWorld : UnlockedWorld;
        }
    }
}
