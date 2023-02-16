using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Confluence.Api.Models
{
    public static class ModelExtentions
    {
        public static bool HasContent(this Content content)
        {
            if (content.type != "attachment")
            {
                return content.body.storage.value.Length > 100;
            }

            return false;
        }
    }
}
