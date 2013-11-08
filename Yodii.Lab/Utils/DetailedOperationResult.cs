using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Lab.Utils
{
    public class DetailedOperationResult
    {
        public string Reason { get; private set; }
        public bool IsSuccessful { get; private set; }

        public DetailedOperationResult( bool isSuccessful = true, string reason = "" )
        {
            Reason = reason;
            IsSuccessful = isSuccessful;
        }

        public static implicit operator bool( DetailedOperationResult result )
        {
            return result.IsSuccessful;
        }
    }
}
