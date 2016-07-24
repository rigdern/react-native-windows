using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactNative.Modules.Image
{
    public class ReactImageRequest
    {
        public bool IsDirty
        {
            get;
            set;
        }

        private string _source;
        public string Source
        {
            get { return _source;  }
            set { _source = value; IsDirty = true; }
        }

        private IDictionary<string, string> _headers;
        public IDictionary<string, string> Headers
        {
            get { return _headers;  }
            set { _headers = value; IsDirty = true; }
        }
    }
}
