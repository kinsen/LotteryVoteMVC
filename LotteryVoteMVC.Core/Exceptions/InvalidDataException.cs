using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Core.Exceptions
{
    public class InvalidDataException : Exception
    {
        public string DataName { get; private set; }
        public InvalidDataException(string dataName) : base() { this.DataName = dataName; }
        public InvalidDataException(string dataName, string errorMessage) : this(dataName, errorMessage, null) { }
        public InvalidDataException(string dataName, string errorMessage, Exception innerException) : base(errorMessage, innerException) { this.DataName = dataName; }
    }
}
