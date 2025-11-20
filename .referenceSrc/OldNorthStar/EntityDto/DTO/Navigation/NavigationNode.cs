using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityDto.DTO.Navigation
{
    public class NavigationNode
    {
        public NavigationNode()
        {
            children = new List<NavigationNode>();
        }
        public List<NavigationNode> children { get; set; }
        public string label { get; set; }
        public string url { get; set; }
        public string html { get; set; }
        public string iconClasses { get; set; }

    }
}
