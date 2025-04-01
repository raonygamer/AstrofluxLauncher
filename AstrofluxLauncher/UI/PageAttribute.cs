using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstrofluxLauncher.UI {
    [AttributeUsage(AttributeTargets.Class)]
    public class PageAttribute : Attribute {
        public string ID { get; }
        public string DisplayName { get; }
        public bool SingleInstance { get; }
        public bool ShouldRegisterAsPreviousPage { get; }

        public PageAttribute(string id, string displayName, bool singleInstance = false, bool shouldRegisterAsPreviousPage = true) {
            ID = id;
            DisplayName = displayName;
            SingleInstance = singleInstance;
            ShouldRegisterAsPreviousPage = shouldRegisterAsPreviousPage;
        }
    }
}
